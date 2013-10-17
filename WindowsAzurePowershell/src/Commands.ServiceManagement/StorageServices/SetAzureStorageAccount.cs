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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.StorageServices
{
    using System.Management.Automation;
    using Management.Storage;
    using Management.Storage.Models;
    using Utilities.Common;

    /// <summary>
    /// Updates the label and/or the description for a storage account in Windows Azure.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureStorageAccount"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureStorageAccountCommand : ServiceManagementBaseCmdlet
    {
        /// <summary>
        /// The name for the storage account. (Required)
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Name of the storage account.")]
        [ValidateNotNullOrEmpty]
        [Alias("ServiceName")]
        public string StorageAccountName
        {
            get;
            set;
        }

        /// <summary>
        /// A label for the storage account. The label may be up to 100 characters in length. 
        /// </summary>
        [Parameter(HelpMessage = "Label of the storage account.")]
        [ValidateLength(0, 100)]
        public string Label
        {
            get;
            set;
        }

        /// <summary>
        /// A description for the storage account. The description may be up to 1024 characters in length.
        /// </summary>
        [Parameter(HelpMessage = "Description of the storage account.")]
        [ValidateLength(0, 1024)]
        public string Description
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Enable or Disable Geo Replication")]
        public bool? GeoReplicationEnabled
        {
            get;
            set;
        }

        public void SetStorageAccountProcess()
        {
            var upstorageinput = new StorageAccountUpdateParameters
            {
                GeoReplicationEnabled = GeoReplicationEnabled,
                Description = this.Description,
                Label = this.Label
            };

            ExecuteClientActionNewSM(
                upstorageinput,
                CommandRuntime.ToString(),
                () => this.StorageClient.StorageAccounts.Update(this.StorageAccountName, upstorageinput));
        }

        protected override void OnProcessRecord()
        {
            this.SetStorageAccountProcess();
        }
    }
}
