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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Extensions
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using Model.PersistentVMModel;
    using Utilities.Common;

    /// <summary>
    /// Set Windows Azure Service Remote Desktop Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceRemoteDesktopExtension", DefaultParameterSetName = "SetExtension"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceRemoteDesktopExtensionCommand : BaseAzureServiceRemoteDesktopExtensionCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Cloud Service Name")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Production (default) or Staging.")]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Production (default) or Staging.")]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public override string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [ValidateNotNullOrEmpty]
        public override string[] Role
        {
            get;
            set;
        }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "X509Certificate used to encrypt password.")]
        [ValidateNotNullOrEmpty]
        public override X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Thumbprint of a certificate used for encryption.")]
        [ValidateNotNullOrEmpty]
        public override string CertificateThumbprint
        {
            get;
            set;
        }

        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Algorithm associated with the Thumbprint.")]
        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Algorithm associated with the Thumbprint.")]
        [ValidateNotNullOrEmpty]
        public override string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Position = 5, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "SetExtension", HelpMessage = "Remote Desktop Credential")]
        [Parameter(Position = 5, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Remote Desktop Credential ")]
        [ValidateNotNullOrEmpty]
        public override PSCredential Credential
        {
            get;
            set;
        }

        [Parameter(Position = 6, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Remote Desktop User Expiration Date")]
        [Parameter(Position = 6, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Remote Desktop User Expiration Date")]
        [ValidateNotNullOrEmpty]
        public override DateTime Expiration
        {
            get;
            set;
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ValidateService();
            ValidateDeployment();
            ValidateRoles();
            ValidateThumbprint(true);
            Expiration = Expiration.Equals(default(DateTime)) ? DateTime.Now.AddMonths(12) : Expiration;
            ValidateConfiguration();
        }

        public void ExecuteCommand()
        {
            ValidateParameters();
            ExtensionConfigurationInput context = new ExtensionConfigurationInput
            {
                ProviderNameSpace = ExtensionNameSpace,
                Type = ExtensionType,
                CertificateThumbprint = CertificateThumbprint,
                ThumbprintAlgorithm = ThumbprintAlgorithm,
                X509Certificate = X509Certificate,
                PublicConfiguration = PublicConfiguration,
                PrivateConfiguration = PrivateConfiguration,
                Roles = new ExtensionRoleList(Role != null && Role.Any() ? Role.Select(r => new ExtensionRole(r)) : Enumerable.Repeat(new ExtensionRole(), 1))
            };
            var extConfig = ExtensionManager.InstallExtension(context, Slot, Deployment.ExtensionConfiguration);
            ChangeDeployment(extConfig);
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}
