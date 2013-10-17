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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Server.Cmdlet
{
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using System.Xml;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Sql;
    using Microsoft.WindowsAzure.Management.Sql.Models;
    using Model;
    using Properties;
    using ServiceManagement;
    using Services;

    /// <summary>
    /// Creates a new Windows Azure SQL Database server in the selected subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSqlDatabaseServer", SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.Low)]
    public class NewAzureSqlDatabaseServer : SqlDatabaseCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true,
            HelpMessage = "Administrator login name for the new SQL Database server.")]
        [ValidateNotNullOrEmpty]
        public string AdministratorLogin
        {
            get;
            set;
        }

        [Parameter(Mandatory = true,
            HelpMessage = "Administrator login password for the new SQL Database server.")]
        [ValidateNotNullOrEmpty]
        public string AdministratorLoginPassword
        {
            get;
            set;
        }

        [Parameter(Mandatory = true,
            HelpMessage = "Location in which to create the new SQL Database server.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Do not confirm on the creation of the server")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new server in the current subscription.
        /// </summary>
        /// <param name="adminLogin">
        /// The administrator login name for the new server.
        /// </param>
        /// <param name="adminLoginPassword">
        /// The administrator login password for the new server.
        /// </param>
        /// <param name="location">
        /// The location in which to create the new server.
        /// </param>
        /// <returns>The context to the newly created server.</returns>
        internal SqlDatabaseServerContext NewAzureSqlDatabaseServerProcess(
            string adminLogin,
            string adminLoginPassword,
            string location)
        {
            // Do nothing if force is not specified and user cancelled the operation
            if (!Force.IsPresent &&
                !ShouldProcess(
                    Resources.NewAzureSqlDatabaseServerDescription,
                    Resources.NewAzureSqlDatabaseServerWarning,
                    Resources.ShouldProcessCaption))
            {
                return null;
            }

            // Get the SQL management client for the current subscription
            SqlManagementClient sqlManagementClient = SqlDatabaseCmdletBase.GetCurrentSqlClient();

            // Issue the create server request
            ServerCreateResponse response = sqlManagementClient.Servers.Create(
                new ServerCreateParameters()
                {
                    Location = location,
                    AdministratorUserName = adminLogin,
                    AdministratorPassword = adminLoginPassword
                });

            SqlDatabaseServerContext operationContext = new SqlDatabaseServerContext()
            {
                OperationStatus = Services.Constants.OperationSuccess,
                OperationDescription = CommandRuntime.ToString(),
                OperationId = response.RequestId,
                ServerName = response.ServerName,
                Location = location,
                AdministratorLogin = adminLogin,
            };

            return operationContext;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                SqlDatabaseServerContext context = this.NewAzureSqlDatabaseServerProcess(
                    this.AdministratorLogin,
                    this.AdministratorLoginPassword,
                    this.Location);

                if (context != null)
                {
                    WriteObject(context, true);
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorDetails(ex);
            }
        }
    }
}
