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
    using System.Linq;
    using System.Management.Automation;
    using System.Xml;
    using Model;
    using Model.PersistentVMModel;
    using Properties;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Set, "AzureVMDiagnosticsExtension", DefaultParameterSetName = EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext), OutputType(typeof(IPersistentVM))]
    public class SetAzureVMDiagnosticsExtensionCommand : VirtualMachineConfigurationCmdletBase
    {
        public const string EnableExtensionUsingXmlDocumentParameterSet = "EnableExtensionUsingXmlDocument";
        public const string EnableExtensionUsingXmlFilePathParameterSet = "EnableExtensionUsingXmlFilePath";
        public const string EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext = "EnableExtensionUsingXmlDocumentUsingStorageContext";
        public const string EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext = "EnableExtensionUsingXmlFilePathUsingStorageContext";
        public const string DisableExtensionParameterSet = "DisableExtension";

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Diagnostics Configuration")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext, HelpMessage = "Diagnostics Configuration")]
        [ValidateNotNullOrEmpty]
        public XmlDocument DiagnosticsConfiguration
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Diagnostics Configuration File")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext, HelpMessage = "Diagnostics Configuration File")]
        [ValidateNotNullOrEmpty]
        public string DiagnosticsConfigurationFile
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext, HelpMessage = "Storage Account Context")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext, HelpMessage = "Storage Account Context")]
        [ValidateNotNullOrEmpty]
        public StorageServicePropertiesOperationContext StorageAccountContext
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Storage Account Name")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Storage Account Name")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Storage Account Key")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Storage Account Key")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext, HelpMessage = "Storage Account Key")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext, HelpMessage = "Storage Account Key")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountKey
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Blob/Queue/Table Endpoint Uri for storage services, e.g. {\"http://foo.blob.core.windows.net\", \"http://foo.queue.core.windows.net\", \"http://foo.table.core.windows.net\"}.")]
        [Parameter(Mandatory = false, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Blob/Queue/Table Endpoint Uri for storage services, e.g. {\"http://foo.blob.core.windows.net\", \"http://foo.queue.core.windows.net\", \"http://foo.table.core.windows.net\"}.")]
        [ValidateNotNullOrEmpty]
        public Uri[] Endpoints
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = DisableExtensionParameterSet, HelpMessage = "Disable Diagnostics Extension")]
        public SwitchParameter Disabled
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            VM.GetInstance().ResourceExtensionReferences = new ResourceExtensionReferenceList(
                new int[1].Select(i => (Disabled.IsPresent ? new VMDiagnosticsExtensionBuilder()
                                                           : new VMDiagnosticsExtensionBuilder(
                                                              this.StorageAccountName,
                                                              this.StorageAccountKey,
                                                              this.Endpoints,
                                                              DiagnosticsConfiguration)).GetResourceReference()));
            WriteObject(VM);
        }

        protected override void ProcessRecord()
        {
            ServiceManagementProfile.Initialize();
            try
            {
                ValidateParameters();
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        private void ValidateParameters()
        {
            // GA must be enabled before setting WAD
            if (VM.GetInstance().ProvisionGuestAgent == null || !VM.GetInstance().ProvisionGuestAgent.Value)
            {
                throw new ArgumentException(Resources.ProvisionGuestAgentMustBeEnabledBeforeSettingIaaSDiagnosticsExtension);
            }

            // Validate DA Extension related parameters.
            if (ParameterSetName != DisableExtensionParameterSet)
            {
                if (!string.IsNullOrEmpty(DiagnosticsConfigurationFile))
                {
                    DiagnosticsConfiguration = new XmlDocument();
                    DiagnosticsConfiguration.LoadXml(General.GetConfiguration(DiagnosticsConfigurationFile));
                }
            }

            if (ParameterSetName == EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext ||
                ParameterSetName == EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext)
            {
                this.StorageAccountName = this.StorageAccountContext.StorageAccountName;
                // must be in this order: blob, queue, table endpoints
                this.Endpoints = this.StorageAccountContext.Endpoints == null ? null : this.StorageAccountContext.Endpoints.Select(e => new Uri(e)).ToArray();
            }
        }
    }
}
