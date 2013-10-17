﻿// ----------------------------------------------------------------------------------
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
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Sql;
    using Microsoft.WindowsAzure.Management.Sql.Models;

    /// <summary>
    /// Implementation of the <see cref="IServerDataServiceContext"/> with Certificate authentication.
    /// </summary>
    public partial class ServerDataServiceCertAuth : IServerDataServiceContext
    {
        #region Private Fields

        /// <summary>
        /// The number of bytes in 1 gigabyte.
        /// </summary>
        private const long BytesIn1Gb = 1 * 1024L * 1024L * 1024L;

        /// <summary>
        /// The previous request's client request ID
        /// </summary>
        private string clientRequestId;

        /// <summary>
        /// The name of the server we are connected to.
        /// </summary>
        private readonly string serverName;

        /// <summary>
        /// The subscription used to connect and authenticate.
        /// </summary>
        private readonly WindowsAzureSubscription subscription;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataServicesCertAuth"/> class
        /// </summary>
        /// <param name="subscription">The subscription used to connect and authenticate.</param>
        /// <param name="serverName">The name of the server to connect to.</param>
        private ServerDataServiceCertAuth(
            WindowsAzureSubscription subscription,
            string serverName)
        {
            this.serverName = serverName;
            this.subscription = subscription;
        }

        #region Public Properties

        /// <summary>
        /// Gets the client per-session tracing ID.
        /// </summary>
        public string ClientSessionId
        {
            get
            {
                return SqlDatabaseCmdletBase.clientSessionId;
            }
        }

        /// <summary>
        /// Gets the previous request's client request ID.
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
        /// Creates and returns a new instance of the <see cref="ServerDataServiceCertAuth"/> class
        /// which connects to the specified server using the specified subscription credentials.
        /// </summary>
        /// <param name="subscription">The subscription used to connect and authenticate.</param>
        /// <param name="serverName">The name of the server to connect to.</param>
        /// <returns>An instance of <see cref="ServerDataServiceCertAuth"/> class.</returns>
        public static ServerDataServiceCertAuth Create(
            string serverName,
            WindowsAzureSubscription subscription)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ArgumentException("serverName");
            }

            SqlDatabaseCmdletBase.ValidateSubscription(subscription);

            // Create a new ServerDataServiceCertAuth object to be used
            return new ServerDataServiceCertAuth(
                subscription,
                serverName);
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
            }
            catch
            {
                // Ignore exceptions when loading extra properties, for backward compatibility.
            }
        }

        #region Database Operations

        /// <summary>
        /// Gets a list of all the databases in the current context.
        /// </summary>
        /// <returns>An array of databases in the current context</returns>
        public Database[] GetDatabases()
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the list of databases
            DatabaseListResponse response = sqlManagementClient.Databases.List(this.serverName);

            // Construct the resulting Database objects
            Database[] databases = response.Databases.Select((db) => CreateDatabaseFromResponse(db)).ToArray();
            return databases;
        }

        /// <summary>
        /// Retrieve a specific database from the current context
        /// </summary>
        /// <param name="databaseName">The name of the database to retrieve</param>
        /// <returns>A database object</returns>
        public Database GetDatabase(string databaseName)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the specified database
            DatabaseGetResponse response = sqlManagementClient.Databases.Get(
                this.serverName,
                databaseName);

            // Construct the resulting Database object
            Database database = CreateDatabaseFromResponse(response);
            return database;
        }

        /// <summary>
        /// Creates a new sql database.
        /// </summary>
        /// <param name="databaseName">The name for the new database</param>
        /// <param name="databaseMaxSizeInGB">The maximum size of the new database</param>
        /// <param name="databaseCollation">The collation for the new database</param>
        /// <param name="databaseEdition">The edition for the new database</param>
        /// <returns>The newly created Sql Database</returns>
        public Database CreateNewDatabase(
            string databaseName,
            int? databaseMaxSizeInGB,
            string databaseCollation,
            DatabaseEdition databaseEdition)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Create the database
            DatabaseCreateResponse response = sqlManagementClient.Databases.Create(
                this.serverName,
                new DatabaseCreateParameters()
                {
                    Name = databaseName,
                    Edition = databaseEdition != DatabaseEdition.None ?
                        databaseEdition.ToString() : DatabaseEdition.Web.ToString(),
                    CollationName = databaseCollation ?? string.Empty,
                    MaximumDatabaseSizeInGB = databaseMaxSizeInGB ??
                        (databaseEdition == DatabaseEdition.Business ? 10 : 1),
                });

            // Construct the resulting Database object
            Database database = CreateDatabaseFromResponse(response);
            return database;
        }

        /// <summary>
        /// Update a database on the server.
        /// </summary>
        /// <param name="databaseName">The name of the database to modify.</param>
        /// <param name="newDatabaseName">The new name of the database.</param>
        /// <param name="databaseMaxSizeInGB">The new maximum size of the database.</param>
        /// <param name="databaseEdition">The new edition of the database.</param>
        /// <param name="serviceObjective">The new service objective of the database.</param>
        /// <returns>The updated database.</returns>
        public Database UpdateDatabase(
            string databaseName,
            string newDatabaseName,
            int? databaseMaxSizeInGB,
            DatabaseEdition? databaseEdition,
            ServiceObjective serviceObjective)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the specified database
            DatabaseGetResponse database = sqlManagementClient.Databases.Get(
                this.serverName,
                databaseName);

            // Update the database with the new properties
            DatabaseUpdateResponse response = sqlManagementClient.Databases.Update(
                this.serverName,
                databaseName,
                new DatabaseUpdateParameters()
                {
                    Id = database.Id,
                    Name = !string.IsNullOrEmpty(newDatabaseName) ?
                        newDatabaseName : database.Name,
                    Edition = databaseEdition.HasValue && (databaseEdition != DatabaseEdition.None) ?
                        databaseEdition.ToString() : (database.Edition ?? string.Empty),
                    CollationName = database.CollationName ?? string.Empty,
                    MaximumDatabaseSizeInGB = databaseMaxSizeInGB.HasValue ?
                        databaseMaxSizeInGB.Value : database.MaximumDatabaseSizeInGB,
                    ServiceObjectiveId = serviceObjective != null ?
                        serviceObjective.Id.ToString() : null,
                });

            // Construct the resulting Database object
            Database updatedDatabase = CreateDatabaseFromResponse(response);
            return updatedDatabase;
        }

        /// <summary>
        /// Remove a database from a server
        /// </summary>
        /// <param name="databaseName">The name of the database to delete</param>
        public void RemoveDatabase(string databaseName)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the list of databases
            OperationResponse response = sqlManagementClient.Databases.Delete(
                this.serverName,
                databaseName);
        }

        #endregion

        #region Service Objective Operations

        public ServiceObjective[] GetServiceObjectives()
        {
            throw new NotSupportedException();
        }

        public ServiceObjective GetServiceObjective(string serviceObjectiveName)
        {
            throw new NotSupportedException();
        }

        public ServerQuota GetQuota(string quotaName)
        {
            throw new NotSupportedException();
        }

        public ServerQuota[] GetQuotas()
        {
            throw new NotSupportedException();
        }

        #endregion

        #endregion

        #region Helper functions

        /// <summary>
        /// Given a <see cref="DatabaseGetResponse"/> this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="response">The response to turn into a <see cref="Database"/></param>
        /// <returns>a <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(DatabaseGetResponse response)
        {
            return this.CreateDatabaseFromResponse(
                response.Id,
                response.Name,
                response.CreationDate,
                response.Edition,
                response.CollationName,
                response.MaximumDatabaseSizeInGB,
                response.IsFederationRoot,
                response.IsSystemObject,
                response.SizeMB,
                response.ServiceObjectiveAssignmentErrorCode,
                response.ServiceObjectiveAssignmentErrorDescription,
                response.ServiceObjectiveAssignmentState,
                response.ServiceObjectiveAssignmentStateDescription,
                response.ServiceObjectiveAssignmentSuccessDate,
                response.ServiceObjectiveId);
        }

        /// <summary>
        /// Given a <see cref="DatabaseListResponse.Database"/> this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="response">The response to turn into a <see cref="Database"/></param>
        /// <returns>a <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(DatabaseListResponse.Database response)
        {
            return this.CreateDatabaseFromResponse(
                response.Id,
                response.Name,
                response.CreationDate,
                response.Edition,
                response.CollationName,
                response.MaximumDatabaseSizeInGB,
                response.IsFederationRoot,
                response.IsSystemObject,
                response.SizeMB,
                response.ServiceObjectiveAssignmentErrorCode,
                response.ServiceObjectiveAssignmentErrorDescription,
                response.ServiceObjectiveAssignmentState,
                response.ServiceObjectiveAssignmentStateDescription,
                response.ServiceObjectiveAssignmentSuccessDate,
                response.ServiceObjectiveId);
        }

        /// <summary>
        /// Given a <see cref="DatabaseCreateResponse"/> this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="response">The response to turn into a <see cref="Database"/></param>
        /// <returns>a <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(DatabaseCreateResponse response)
        {
            return this.CreateDatabaseFromResponse(
               response.Id,
               response.Name,
               response.CreationDate,
               response.Edition,
               response.CollationName,
               response.MaximumDatabaseSizeInGB,
               response.IsFederationRoot,
               response.IsSystemObject,
               response.SizeMB,
               response.ServiceObjectiveAssignmentErrorCode,
               response.ServiceObjectiveAssignmentErrorDescription,
               response.ServiceObjectiveAssignmentState,
               response.ServiceObjectiveAssignmentStateDescription,
               response.ServiceObjectiveAssignmentSuccessDate,
               response.ServiceObjectiveId);
        }

        /// <summary>
        /// Given a <see cref="DatabaseUpdateResponse"/> this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="response">The response to turn into a <see cref="Database"/></param>
        /// <returns>a <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(DatabaseUpdateResponse response)
        {
            return this.CreateDatabaseFromResponse(
                response.Id,
                response.Name,
                response.CreationDate,
                response.Edition,
                response.CollationName,
                response.MaximumDatabaseSizeInGB,
                response.IsFederationRoot,
                response.IsSystemObject,
                response.SizeMB,
                response.ServiceObjectiveAssignmentErrorCode,
                response.ServiceObjectiveAssignmentErrorDescription,
                response.ServiceObjectiveAssignmentState,
                response.ServiceObjectiveAssignmentStateDescription,
                response.ServiceObjectiveAssignmentSuccessDate,
                response.ServiceObjectiveId);
        }

        /// <summary>
        /// Given a set of database properties this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="id">The database Id.</param>
        /// <param name="name">The database name.</param>
        /// <param name="creationDate">The database creation date.</param>
        /// <param name="edition">The database edition.</param>
        /// <param name="collationName">The database collation name.</param>
        /// <param name="maximumDatabaseSizeInGB">The database maximum size.</param>
        /// <param name="isFederationRoot">Whether or not the database is a federation root.</param>
        /// <param name="isSystemObject">Whether or not the database is a system object.</param>
        /// <param name="sizeMB">The current database size.</param>
        /// <param name="serviceObjectiveAssignmentErrorCode">
        /// The last error code received for service objective assignment change.
        /// </param>
        /// <param name="serviceObjectiveAssignmentErrorDescription">
        /// The last error received for service objective assignment change.
        /// </param>
        /// <param name="serviceObjectiveAssignmentState">
        /// The state of the current service objective assignment.
        /// </param>
        /// <param name="serviceObjectiveAssignmentStateDescription">
        /// The state description for the current service objective assignment.
        /// </param>
        /// <param name="serviceObjectiveAssignmentSuccessDate">
        /// The last success date for a service objective assignment on this database.
        /// </param>
        /// <param name="serviceObjectiveId">The service objective Id for this database.</param>
        /// <returns>A <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(
            int id,
            string name,
            DateTime creationDate,
            string edition,
            string collationName,
            long maximumDatabaseSizeInGB,
            bool isFederationRoot,
            bool isSystemObject,
            string sizeMB,
            string serviceObjectiveAssignmentErrorCode,
            string serviceObjectiveAssignmentErrorDescription,
            string serviceObjectiveAssignmentState,
            string serviceObjectiveAssignmentStateDescription,
            string serviceObjectiveAssignmentSuccessDate,
            string serviceObjectiveId)
        {
            Database result = new Database()
            {
                Id = id,
                Name = name,
                CollationName = collationName,
                CreationDate = creationDate,
                Edition = edition,
                MaxSizeGB = (int)maximumDatabaseSizeInGB,
                MaxSizeBytes = maximumDatabaseSizeInGB * BytesIn1Gb,
                IsFederationRoot = isFederationRoot,
                IsSystemObject = isSystemObject,
            };

            // Parse any additional database information
            if (!string.IsNullOrEmpty(sizeMB))
            {
                result.SizeMB = decimal.Parse(sizeMB, CultureInfo.InvariantCulture);
            }

            // Parse the service objective information
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentErrorCode))
            {
                result.ServiceObjectiveAssignmentErrorCode = int.Parse(serviceObjectiveAssignmentErrorCode);
            }
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentErrorDescription))
            {
                result.ServiceObjectiveAssignmentErrorDescription = serviceObjectiveAssignmentErrorDescription;
            }
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentState))
            {
                result.ServiceObjectiveAssignmentState = byte.Parse(serviceObjectiveAssignmentState);
            }
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentStateDescription))
            {
                result.ServiceObjectiveAssignmentStateDescription = serviceObjectiveAssignmentStateDescription;
            }
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentSuccessDate))
            {
                result.ServiceObjectiveAssignmentSuccessDate = DateTime.Parse(serviceObjectiveAssignmentSuccessDate, CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(serviceObjectiveId))
            {
                result.ServiceObjectiveId = Guid.Parse(serviceObjectiveId);
            }

            this.LoadExtraProperties(result);

            return result;
        }

        #endregion

        /// <summary>
        /// Add the tracing session and request headers to the client.
        /// </summary>
        /// <param name="sqlManagementClient">The client to add the headers on.</param>
        private void AddTracingHeaders(SqlManagementClient sqlManagementClient)
        {
            sqlManagementClient.HttpClient.DefaultRequestHeaders.Add(
                Constants.ClientSessionIdHeaderName,
                this.ClientSessionId);
            sqlManagementClient.HttpClient.DefaultRequestHeaders.Add(
                Constants.ClientRequestIdHeaderName,
                this.ClientRequestId);
        }

        /// <summary>
        /// Ensures any extra property on the given <paramref name="database"/> is loaded.
        /// </summary>
        /// <param name="database">The database that needs the extra properties.</param>
        private void LoadExtraProperties(Database database)
        {
            // Fill in the context property
            database.Context = this;
        }
    }
}
