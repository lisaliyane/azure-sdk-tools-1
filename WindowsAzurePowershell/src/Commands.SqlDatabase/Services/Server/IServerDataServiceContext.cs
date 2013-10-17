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
    /// <summary>
    /// Common interface for all server based operations.
    /// </summary>
    public interface IServerDataServiceContext
    {
        /// <summary>
        /// Gets the per session tracing Id.
        /// </summary>
        string ClientSessionId { get; }

        /// <summary>
        /// Gets the previous request's client request Id.
        /// </summary>
        string ClientRequestId { get; }

        /// <summary>
        /// Gets the name of the server for this context.
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Ensures any extra property on the given <paramref name="obj"/> is loaded.
        /// </summary>
        /// <param name="obj">The object that needs the extra properties.</param>
        void LoadExtraProperties(object obj);

        #region Database Operations

        /// <summary>
        /// Retrieves the list of all databases on the server.
        /// </summary>
        /// <returns>An array of all databases on the server.</returns>
        Database[] GetDatabases();

        /// <summary>
        /// Retrieve information on database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to retrieve.</param>
        /// <returns>An object containing the information about the specific database.</returns>
        Database GetDatabase(string databaseName);

        /// <summary>
        /// Creates a new Sql Database.
        /// </summary>
        /// <param name="databaseName">The name for the new database.</param>
        /// <param name="databaseMaxSize">The max size for the database.</param>
        /// <param name="databaseCollation">The collation for the database.</param>
        /// <param name="databaseEdition">The edition for the database.</param>
        /// <returns>The newly created Sql Database.</returns>
        Database CreateNewDatabase(
            string databaseName,
            int? databaseMaxSize,
            string databaseCollation,
            DatabaseEdition databaseEdition);

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
        Database UpdateDatabase(
            string databaseName,
            string newDatabaseName,
            int? databaseMaxSize,
            DatabaseEdition? databaseEdition,
            ServiceObjective serviceObjective);

        /// <summary>
        /// Removes the database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to remove.</param>
        void RemoveDatabase(string databaseName);

        #endregion

        #region Service Objective Operations

        /// <summary>
        /// Retrieves the list of all service objectives on the server.
        /// </summary>
        /// <returns>An array of all service objectives on the server.</returns>
        ServiceObjective[] GetServiceObjectives();

        /// <summary>
        /// Retrieve information on service objective with the name
        /// <paramref name="serviceObjectivesName"/>.
        /// </summary>
        /// <param name="serviceObjectiveName">The service objective to retrieve.</param>
        /// <returns>
        /// An object containing the information about the specific service objective.
        /// </returns>
        ServiceObjective GetServiceObjective(string serviceObjectiveName);

        /// <summary>
        /// Get a specific quota for a server
        /// </summary>
        /// <param name="quotaName">The name of the quota to retrieve</param>
        /// <returns>A quota object.</returns>
        ServerQuota GetQuota(string quotaName);

        /// <summary>
        /// Get a list of all quotas for a server
        /// </summary>
        /// <returns>An array of server quota objects</returns>
        ServerQuota[] GetQuotas();

        #endregion
    }
}
