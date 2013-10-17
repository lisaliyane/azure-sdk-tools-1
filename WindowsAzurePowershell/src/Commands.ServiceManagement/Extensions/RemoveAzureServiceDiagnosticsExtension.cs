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
    using System.Linq;
    using System.Management.Automation;
    using Model.PersistentVMModel;
    using Properties;
    using Utilities.Common;

    /// <summary>
    /// Remove Windows Azure Service Diagnostics Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureServiceDiagnosticsExtension", DefaultParameterSetName = "RemoveByRoles"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureServiceDiagnosticsExtensionCommand : BaseAzureServiceDiagnosticsExtensionCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "RemoveByRoles", HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "RemoveAll", HelpMessage = "Cloud Service Name")]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "RemoveByRoles", HelpMessage = "Deployment Slot: Production (default) or Staging.")]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "RemoveAll", HelpMessage = "Deployment Slot: Production (default) or Staging.")]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public override string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = "RemoveByRoles", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        public override string[] Role
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "RemoveAll", HelpMessage = "If specified uninstall all Diagnostics configurations from the cloud service.")]
        public override SwitchParameter UninstallConfiguration
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
        }

        public void ExecuteCommand()
        {
            ValidateParameters();
            ExtensionConfigurationBuilder configBuilder = ExtensionManager.GetBuilder(Deployment.ExtensionConfiguration);
            if (UninstallConfiguration && configBuilder.ExistAny(ExtensionNameSpace, ExtensionType))
            {
                configBuilder.RemoveAny(ExtensionNameSpace, ExtensionType);
                WriteVerbose(string.Format(Resources.ServiceExtensionRemovingFromAllRoles, ExtensionType, ServiceName));
                ChangeDeployment(configBuilder.ToConfiguration());
            }
            else if (configBuilder.Exist(Role, ExtensionNameSpace, ExtensionType))
            {
                configBuilder.Remove(Role, ExtensionNameSpace, ExtensionType);
                if (Role == null || !Role.Any())
                {
                    WriteVerbose(string.Format(Resources.ServiceExtensionRemovingFromAllRoles, ExtensionType, ServiceName));
                }
                else
                {
                    bool defaultExists = configBuilder.ExistDefault(ExtensionNameSpace, ExtensionType);
                    foreach (var r in Role)
                    {
                        WriteVerbose(string.Format(Resources.ServiceExtensionRemovingFromSpecificRoles, ExtensionType, r, ServiceName));
                        if (defaultExists)
                        {
                            WriteVerbose(string.Format(Resources.ServiceExtensionRemovingSpecificAndApplyingDefault, ExtensionType, r));
                        }
                    }
                }
                ChangeDeployment(configBuilder.ToConfiguration());
            }
            else
            {
                WriteVerbose(string.Format(Resources.ServiceExtensionNoExistingExtensionsEnabledOnRoles, ExtensionNameSpace, ExtensionType));
            }

            if (UninstallConfiguration)
            {
                var allConfig = ExtensionManager.GetBuilder();
                var deploymentList = (from slot in (new string[] { DeploymentSlotType.Production, DeploymentSlotType.Staging })
                                      let d = GetDeployment(slot)
                                      where d != null
                                      select d).ToList();
                deploymentList.ForEach(d => allConfig.Add(d.ExtensionConfiguration));
                ExtensionManager.Uninstall(ExtensionNameSpace, ExtensionType, allConfig.ToConfiguration());
            }
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}
