// ----------------------------------------------------------------------------------
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
    using Services.Common;
    using Services.Server;

    /// <summary>
    /// Retrieves a list of Windows Azure SQL Database server quotas for the selected server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseServerQuota", ConfirmImpact = ConfirmImpact.None)]
    public class GetAzureSqlDatabaseServerQuota : PSCmdlet
    {
        #region Parameters

        /// <summary>
        /// Gets or sets the server connection context.
        /// </summary>
        [Alias("Context")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The connection context to the specified server.")]
        [ValidateNotNull]
        public IServerDataServiceContext ConnectionContext { get; set; }

        /// <summary>
        /// Gets or sets the name of the server quota to retrieve
        /// </summary>
        [Parameter(Position = 1, Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the quota to retrieve")]
        [ValidateNotNullOrEmpty]
        public string QuotaName { get; set; }

        #endregion

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            // Obtain the quota name from the given parameters.
            string quotaName = null;
            if (this.MyInvocation.BoundParameters.ContainsKey("QuotaName"))
            {
                quotaName = this.QuotaName;
            }

            try
            {
                if (!string.IsNullOrEmpty(quotaName))
                {
                    // Retrieve the quota with the specified name
                    this.WriteObject(this.ConnectionContext.GetQuota(quotaName));
                }
                else
                {
                    // No name specified, retrieve all quotas in the server
                    this.WriteObject(this.ConnectionContext.GetQuotas(), true);
                }
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    this.ConnectionContext.ClientRequestId,
                    ex);
            }
        }
    }
}
