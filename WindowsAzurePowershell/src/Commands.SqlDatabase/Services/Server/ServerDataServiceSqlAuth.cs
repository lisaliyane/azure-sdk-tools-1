// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    /// <summary>
    /// Implementation of the <see cref="IServerDataServiceContext"/> with Sql Authentication.
    /// </summary>
    public partial class ServerDataServiceSqlAuth : ServerDataServiceContext, IServerDataServiceContext
    {
        #region Constants

        /// <summary>
        /// Model name used in the connection type string
        /// </summary>
        private const string ServerModelConnectionType = "Server2";

        #endregion

        #region Private data

        /// <summary>
        /// A Guid that identifies this session for end-to-end tracing
        /// </summary>
        private readonly Guid sessionActivityId;

        /// <summary>
        /// The connection type identifying the model and connection parameters to use
        /// </summary>
        private readonly DataServiceConnectionType connectionType;

        /// <summary>
        /// The access token to use in requests
        /// </summary>
        private readonly AccessTokenResult accessToken;

        /// <summary>
        /// The SQL authentication credentials used for this context
        /// </summary>
        private readonly SqlAuthenticationCredentials credentials;

        /// <summary>
        /// A collection of entries to be added to each request's header. HTTP headers are case-insensitive. 
        /// </summary>
        private readonly Dictionary<string, string> supplementalHeaderEntries =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Serializes some instance-level operations
        /// </summary>
        private readonly object instanceSyncObject = new object();

        /// <summary>
        /// The name of the server we are connected to.
        /// </summary>
        private readonly string serverName;

        /// <summary>
        /// The previous request's client request Id.
        /// </summary>
        private string clientRequestId;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataServiceSqlAuth"/> class.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service Uri.</param>
        /// <param name="connectionType">The server connection type with the server name</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="accessToken">The authentication access token to be used for executing web requests.</param>
        /// <param name="credentials">The SQL authentication credentials used for this context</param>
        private ServerDataServiceSqlAuth(
            Uri managementServiceUri,
            DataServiceConnectionType connectionType,
            Guid sessionActivityId,
            AccessTokenResult accessToken,
            SqlAuthenticationCredentials credentials)
            : base(new Uri(managementServiceUri, connectionType.RelativeEntityUri))
        {
            this.sessionActivityId = sessionActivityId;
            this.connectionType = connectionType;
            this.accessToken = accessToken;
            this.credentials = credentials;

            // Generate a requestId and retrieve the server name
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();
            this.serverName = this.Servers.First().Name;
        }

        #region Public Properties

        /// <summary>
        /// Gets the session activity Id associated with this context.
        /// </summary>
        public Guid SessionActivityId
        {
            get
            {
                return this.sessionActivityId;
            }
        }

        /// <summary>
        /// Gets the client per session tracing Id.
        /// </summary>
        public string ClientSessionId
        {
            get
            {
                return SqlDatabaseCmdletBase.clientSessionId;
            }
        }

        /// <summary>
        /// Gets the previous request's client request Id.
        /// </summary>
        public string ClientRequestId
        {
            get
            {
                return this.clientRequestId;
            }
        }

        /// <summary>
        /// Gets the name of the server for this context.
        /// </summary>
        public string ServerName
        {
            get
            {
                return this.serverName;
            }
        }

        #endregion

        /// <summary>
        /// Creates and returns a new instance of the <see cref="ServerDataServiceSqlAuth"/> class which
        /// connects to the specified server using the specified credentials. If the server name
        /// is null, the default server name from the serviceRoot Uri will be used.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service <see cref="Uri"/>.</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="credentials">The credentials to be used to authenticate the user.</param>
        /// <param name="serverName">The name of the server to connect to. (Optional)</param>
        /// <returns>An instance of <see cref="ServerDataServiceSqlAuth"/> class.</returns>
        public static ServerDataServiceSqlAuth Create(
            Uri managementServiceUri,
            Guid sessionActivityId,
            SqlAuthenticationCredentials credentials,
            string serverName)
        {
            if (managementServiceUri == null)
            {
                throw new ArgumentNullException("managementServiceUri");
            }

            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }

            // Retrieve GetAccessToken operation Uri
            Uri accessUri = DataConnectionUtility.GetAccessTokenUri(managementServiceUri);

            // Synchronously call GetAccessToken
            AccessTokenResult result = DataServiceAccess.GetAccessToken(accessUri, credentials);

            // Validate the retrieved access token
            AccessTokenResult.ValidateAccessToken(managementServiceUri, result);

            // Create and return a ServerDataService object
            return Create(managementServiceUri, sessionActivityId, result, serverName, credentials);
        }

        /// <summary>
        /// Creates and returns a new instance of the <see cref="ServerDataServiceSqlAuth"/> class which
        /// connects to the specified server using the specified credentials.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service <see cref="Uri"/>.</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="accessTokenResult">The accessToken to be used to authenticate the user.</param>
        /// <param name="serverName">The name of the server to connect to. (Optional)</param>
        /// <param name="credentials">The SQL authentication credentials used for this context</param>
        /// <returns>An instance of <see cref="ServerDataServiceSqlAuth"/> class.</returns>
        public static ServerDataServiceSqlAuth Create(
            Uri managementServiceUri,
            Guid sessionActivityId,
            AccessTokenResult accessTokenResult,
            string serverName,
            SqlAuthenticationCredentials credentials)
        {
            if (managementServiceUri == null)
            {
                throw new ArgumentNullException("managementServiceUri");
            }

            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            // Create a ServerDataServiceSqlAuth object
            if (serverName == null)
            {
                return new ServerDataServiceSqlAuth(
                    managementServiceUri,
                    new DataServiceConnectionType(ServerModelConnectionType),
                    sessionActivityId,
                    accessTokenResult,
                    credentials);
            }
            else
            {
                return new ServerDataServiceSqlAuth(
                    managementServiceUri,
                    new DataServiceConnectionType(ServerModelConnectionType, serverName),
                    sessionActivityId,
                    accessTokenResult,
                    credentials);
            }
        }

        /// <summary>
        /// Retrieves the metadata for the context as a <see cref="XDocument"/>
        /// </summary>
        /// <returns>The metadata for the context as a <see cref="XDocument"/></returns>
        public XDocument RetrieveMetadata()
        {
            // Create a new request Id for this operation
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            XDocument doc = DataConnectionUtility.GetMetadata(this, EnhanceRequest);
            return doc;
        }

        /// <summary>
        /// Gets the <see cref="SqlAuthenticationCredentials"/> associated with this context.
        /// </summary>
        public SqlAuthenticationCredentials SqlCredentials
        {
            get
            {
                return this.credentials;
            }
        }

        #region IServerDataServiceContext Members


        /// <summary>
        /// Ensures any extra property on the given <paramref name="obj"/> is loaded.
        /// </summary>
        /// <param name="obj">The object that needs the extra properties.</param>
        public void LoadExtraProperties(object obj)
        {
            try
            {
                Database database = obj as Database;
                if (database != null)
                {
                    this.LoadExtraProperties(database);
                    return;
                }

                ServiceObjective serviceObjective = obj as ServiceObjective;
                if (database != null)
                {
                    this.LoadExtraProperties(serviceObjective);
                    return;
                }
            }
            catch
            {
                // Ignore exceptions when loading extra properties, for backward compatibility.
            }
        }

        #region Database Operations

        /// <summary>
        /// Creates a new Sql Database.
        /// </summary>
        /// <param name="databaseName">The name for the new database.</param>
        /// <param name="databaseMaxSize">The max size for the database.</param>
        /// <param name="databaseCollation">The collation for the database.</param>
        /// <param name="databaseEdition">The edition for the database.</param>
        /// <returns>The newly created Sql Database.</returns>
        public Database CreateNewDatabase(
            string databaseName,
            int? databaseMaxSize,
            string databaseCollation,
            DatabaseEdition databaseEdition)
        {
            // Create a new request Id for this operation
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Create the new entity and set its properties
            Database database = new Database();
            database.Name = databaseName;

            if (databaseMaxSize != null)
            {
                database.MaxSizeGB = (int)databaseMaxSize;
            }

            if (!string.IsNullOrEmpty(databaseCollation))
            {
                database.CollationName = databaseCollation;
            }

            if (databaseEdition != DatabaseEdition.None)
            {
                database.Edition = databaseEdition.ToString();
            }

            // Save changes
            this.AddToDatabases(database);
            try
            {
                this.SaveChanges(SaveChangesOptions.None);

                // Re-Query the database for server side updated information
                database = this.RefreshEntity(database);
                if (database == null)
                {
                    throw new ApplicationException(Resources.ErrorRefreshingDatabase);
                }
            }
            catch
            {
                this.ClearTrackedEntity(database);
                throw;
            }

            // Load the extra properties for this object.
            this.LoadExtraProperties(database);

            return database;
        }

        /// <summary>
        /// Retrieves the list of all databases on the server.
        /// </summary>
        /// <returns>An array of all databases on the server.</returns>
        public Database[] GetDatabases()
        {
            Database[] allDatabases = null;

            using (new MergeOptionTemporaryChange(this, MergeOption.OverwriteChanges))
            {
                allDatabases = this.Databases.ToArray();
            }

            // Load the extra properties for all objects.
            foreach (Database database in allDatabases)
            {
                this.LoadExtraProperties(database);
            }

            return allDatabases;
        }

        /// <summary>
        /// Retrieve information on database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to retrieve.</param>
        /// <returns>An object containing the information about the specific database.</returns>
        public Database GetDatabase(string databaseName)
        {
            Database database;

            using (new MergeOptionTemporaryChange(this, MergeOption.OverwriteChanges))
            {
                // Find the database by name
                database = this.Databases.Where(db => db.Name == databaseName).SingleOrDefault();
                if (database == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.DatabaseNotFound,
                            this.ServerName,
                            databaseName));
                }
            }

            // Load the extra properties for this object.
            this.LoadExtraProperties(database);

            return database;
        }

        /// <summary>
        /// Updates the property on the database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to update.</param>
        /// <param name="newDatabaseName">
        /// The new database name, or <c>null</c> to not update.
        /// </param>
        /// <param name="databaseMaxSize">
        /// The new database max size, or <c>null</c> to not update.
        /// </param>
        /// <param name="databaseEdition">
        /// The new database edition, or <c>null</c> to not update.
        /// </param>
        /// <param name="serviceObjective">
        /// The new service objective, or <c>null</c> to not update.
        /// </param>
        /// <returns>The updated database object.</returns>
        public Database UpdateDatabase(
            string databaseName,
            string newDatabaseName,
            int? databaseMaxSize,
            DatabaseEdition? databaseEdition,
            ServiceObjective serviceObjective)
        {
            // Find the database by name
            Database database = GetDatabase(databaseName);

            // Update the name if specified
            if (newDatabaseName != null)
            {
                database.Name = newDatabaseName;
            }

            // Update the max size and edition properties
            database.MaxSizeGB = databaseMaxSize;
            database.Edition = databaseEdition == null ? null : databaseEdition.ToString();

            database.IsRecursiveTriggersOn = null;

            // Update the service objective property if specified
            if (serviceObjective != null)
            {
                database.ServiceObjectiveId = serviceObjective.Id;
            }

            // Mark the database object for update and submit the changes
            this.UpdateObject(database);
            try
            {
                this.SaveChanges();
            }
            catch
            {
                this.RevertChanges(database);
                throw;
            }

            return this.GetDatabase(database.Name);
        }

        /// <summary>
        /// Removes the database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to remove.</param>
        public void RemoveDatabase(string databaseName)
        {
            // Find the database by name
            Database database = GetDatabase(databaseName);

            // Mark the object for delete and submit the changes
            this.DeleteObject(database);
            try
            {
                this.SaveChanges();
            }
            catch
            {
                this.RevertChanges(database);
                throw;
            }
        }

        #endregion

        #region ServiceObjective Operations

        /// <summary>
        /// Retrieves the list of all service objectives on the server.
        /// </summary>
        /// <returns>An array of all service objectives on the server.</returns>
        public ServiceObjective[] GetServiceObjectives()
        {
            ServiceObjective[] allObjectives = null;

            using (new MergeOptionTemporaryChange(this, MergeOption.OverwriteChanges))
            {
                allObjectives = this.ServiceObjectives.ToArray();
            }

            // Load the extra properties for all objects.
            foreach (ServiceObjective objective in allObjectives)
            {
                this.LoadExtraProperties(objective);
            }

            return allObjectives;
        }

        /// <summary>
        /// Retrieve information on service objective with the name
        /// <paramref name="serviceObjectiveName"/>.
        /// </summary>
        /// <param name="serviceObjectiveName">The service objective to retrieve.</param>
        /// <returns>
        /// An object containing the information about the specific service objective.
        /// </returns>
        public ServiceObjective GetServiceObjective(string serviceObjectiveName)
        {
            ServiceObjective objective;

            using (new MergeOptionTemporaryChange(this, MergeOption.OverwriteChanges))
            {
                // Find the service objective by name
                objective = this.ServiceObjectives
                    .Where(db => db.Name == serviceObjectiveName)
                    .SingleOrDefault();
                if (objective == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.ServiceObjectiveNotFound,
                            this.ServerName,
                            serviceObjectiveName));
                }
            }

            // Load the extra properties for this object.
            this.LoadExtraProperties(objective);

            return objective;
        }

        /// <summary>
        /// Gets a quota for a server
        /// </summary>
        /// <param name="quotaName">The name of the quota to retrieve</param>
        /// <returns>A <see cref="ServerQuota"/> object for the quota</returns>
        public ServerQuota GetQuota(string quotaName)
        {
            ServerQuota quota;

            using (new MergeOptionTemporaryChange(this, MergeOption.OverwriteChanges))
            {
                // Find the database by name
                quota = this.ServerQuotas.Where(q => q.Name == quotaName).SingleOrDefault();
                if (quota == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.DatabaseNotFound,
                            this.ServerName,
                            quotaName));
                }
            }

            return quota;
        }

        /// <summary>
        /// Retrieves an array of all the server quotas.
        /// </summary>
        /// <returns>An array of <see cref="ServerQuota"/> objects</returns>
        public ServerQuota[] GetQuotas()
        {
            ServerQuota[] allQuotas = null;

            using (new MergeOptionTemporaryChange(this, MergeOption.OverwriteChanges))
            {
                allQuotas = this.ServerQuotas.ToArray();
            }

            return allQuotas;
        }

        #endregion

        #endregion

        /// <summary>
        /// Sets a supplemental property value that will be send with each request. 
        /// </summary>
        /// <param name="key">A key that uniquely identifies the property</param>
        /// <param name="value">A string representation of the property value</param>
        public void SetSessionHeader(string key, string value)
        {
            lock (this.instanceSyncObject)
            {
                this.supplementalHeaderEntries[key] = value;
            }
        }

        /// <summary>
        /// Handler to add aditional headers and properties to the request.
        /// </summary>
        /// <param name="request">The request to enhance.</param>
        protected override void OnEnhanceRequest(HttpWebRequest request)
        {
            EnhanceRequest(this, request);
        }

        /// <summary>
        /// Enhance a request with auth token.
        /// </summary>
        /// <param name="context">The data service context for the request.</param>
        /// <param name="request">The request.</param>
        private static void EnhanceRequest(ServerDataServiceSqlAuth context, HttpWebRequest request)
        {
            lock (context.instanceSyncObject)
            {
                foreach (KeyValuePair<string, string> entry in context.supplementalHeaderEntries)
                {
                    request.Headers[entry.Key] = entry.Value;
                }
            }

            // Add the UserAgent string
            request.UserAgent = ApiConstants.UserAgentHeaderValue;

            // Add the access token header
            request.Headers[DataServiceConstants.AccessTokenHeader] = context.accessToken.AccessToken;

            // Add the access token cookie
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(context.accessToken.AccessCookie);

            // Add the session activity Id
            request.Headers[DataServiceConstants.SessionTraceActivityHeader] = context.sessionActivityId.ToString();

            // Add the client tracing Ids
            request.Headers[Constants.ClientSessionIdHeaderName] = context.ClientSessionId;
            request.Headers[Constants.ClientRequestIdHeaderName] = context.ClientRequestId;
        }

        #region LoadExtraProperties Implementations

        /// <summary>
        /// Ensures any extra property on the given <paramref name="database"/> is loaded.
        /// </summary>
        /// <param name="database">The database that needs the extra properties.</param>
        private void LoadExtraProperties(Database database)
        {
            // Fill in the context property
            database.Context = this;

            // Fill in the service objective properties
            this.LoadProperty(database, "ServiceObjective");
            database.ServiceObjectiveName =
                database.ServiceObjective == null ? null : database.ServiceObjective.Name;
        }

        /// <summary>
        /// Ensures any extra property on the given <paramref name="serviceObjective"/> is loaded.
        /// </summary>
        /// <param name="serviceObjective">The serviceObjective that needs the extra properties.</param>
        private void LoadExtraProperties(ServiceObjective serviceObjective)
        {
            // Fill in the context property
            serviceObjective.Context = this;

            // Fill in the service objective Dimension Settings
            this.LoadProperty(serviceObjective, "DimensionSettings");
        }

        #endregion
    }
}
