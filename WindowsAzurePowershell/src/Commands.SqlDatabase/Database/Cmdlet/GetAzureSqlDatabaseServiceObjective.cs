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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Management.Automation;
    using SqlDatabase.Properties;
    using Services.Common;
    using Services.Server;

    /// <summary>
    /// Retrieves a list of Windows Azure SQL Databases in the given server context.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseServiceObjective", ConfirmImpact = ConfirmImpact.None,
        DefaultParameterSetName = "ByName")]
    public class GetAzureSqlDatabaseServiceObjective : PSCmdlet
    {
        #region Parameters

        /// <summary>
        /// Gets or sets the server connection context.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The connection context to the specified server.")]
        [ValidateNotNull]
        public IServerDataServiceContext Context { get; set; }

        /// <summary>
        /// Gets or sets the database object to refresh.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ByInputObject",
            ValueFromPipeline = true, HelpMessage = "The Service Objective object to refresh.")]
        [ValidateNotNull]
        public ServiceObjective ServiceObjective { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to retrieve.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ByName",
            HelpMessage = "The name of the Service Objective to retrieve.")]
        [ValidateNotNullOrEmpty]
        public string ServiceObjectiveName { get; set; }

        #endregion

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            // Obtain the service objective name from the given parameters.
            string serviceObjectiveName = null;
            if (this.MyInvocation.BoundParameters.ContainsKey("ServiceObjective"))
            {
                serviceObjectiveName = this.ServiceObjective.Name;
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("ServiceObjectiveName"))
            {
                serviceObjectiveName = this.ServiceObjectiveName;
            }

            try
            {
                if (serviceObjectiveName != null)
                {
                    // Retrieve the service objective with the specified name
                    this.WriteObject(this.Context.GetServiceObjective(serviceObjectiveName));
                }
                else
                {
                    // No name specified, retrieve all service objectives in the server
                    this.WriteObject(this.Context.GetServiceObjectives(), true);
                }
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    this.Context.ClientRequestId,
                    ex);
            }
        }
    }
}
