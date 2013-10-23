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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System;
    using System.Management.Automation;
    using System.Xml;
    using Model;

    [Cmdlet(VerbsCommon.Set, "AzureVMDiagnosticsExtension"), OutputType(typeof(IPersistentVM))]
    public class SetAzureVMDiagnosticsExtensionCommand : VirtualMachineConfigurationCmdletBase
    {

        [Parameter(Mandatory = true, HelpMessage = "Diagnostics Configuration")]
        [ValidateNotNullOrEmpty]
        public XmlDocument DiagnosticsConfiguration
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Diagnostics Configuration File")]
        [ValidateNotNullOrEmpty]
        public string DiagnosticConfigurationFile
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Storage Account Name")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Storage Account Key")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountKey
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "To Disable Diagnostics Extension")]
        public SwitchParameter Disabled
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            if (VM.GetInstance().ProvisionGuestAgent == null || !VM.GetInstance().ProvisionGuestAgent.Value)
            {
                throw new ArgumentException("ProvisionGuestAgent must be enabled for setting diagnostics extensions on the VM.");
            }

            VM.GetInstance().ResourceExtensionReferences = VMDiagnosticsExtensionHelper.GetResourceReferenceList(
                !Disabled.IsPresent,
                this.StorageAccountName,
                this.StorageAccountKey,
                null,
                DiagnosticsConfiguration);
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
