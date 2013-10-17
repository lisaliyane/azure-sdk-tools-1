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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Management.Compute;
    using Management.Compute.Models;
    using Properties;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Remove, "AzureVM"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the role to remove.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        internal override void ExecuteCommand()
        {
            ServiceManagementProfile.Initialize();

            base.ExecuteCommand();
            if (CurrentDeploymentNewSM == null)
            {
                return;
            }

            DeploymentGetResponse deploymentResponse = this.ComputeClient.Deployments.GetBySlot(this.ServiceName, DeploymentSlot.Production);
            if (deploymentResponse.Roles.FirstOrDefault(r => r.RoleName.Equals(Name, StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                throw new ArgumentOutOfRangeException(String.Format(Resources.RoleInstanceCanNotBeFoundWithName, Name));
            }

            if (deploymentResponse.RoleInstances.Count > 1)
            {
                ExecuteClientActionNewSM(
                    null,
                    CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachines.Delete(this.ServiceName, CurrentDeploymentNewSM.Name, Name));
            }
            else
            {
                ExecuteClientActionNewSM(
                    null,
                    CommandRuntime.ToString(),
                    () => this.ComputeClient.Deployments.DeleteBySlot(this.ServiceName, DeploymentSlot.Production));
            }
        }
    }
}