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
    using System.Management.Automation;
    using Management.Compute;
    using Management.Compute.Models;
    using Utilities.Common;

    [Cmdlet(VerbsData.Save, "AzureVMImage"), OutputType(typeof(ManagementOperationContext))]
    public class SaveAzureVMImageCommand : IaaSDeploymentManagementCmdletBase
    {
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the virtual machine to export.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name that will have the new image.")]
        [ValidateNotNullOrEmpty]
        public string NewImageName
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The label that will have the new image.")]
        [ValidateNotNullOrEmpty]
        public string NewImageLabel
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

            var parameter = new VirtualMachineCaptureParameters
            {
                PostCaptureAction = PostCaptureAction.Delete,
                TargetImageLabel = string.IsNullOrEmpty(this.NewImageLabel) ? this.NewImageName : this.NewImageLabel,
                TargetImageName = this.NewImageName
            };

            ExecuteClientActionNewSM(
                null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.VirtualMachines.Capture(this.ServiceName, CurrentDeploymentNewSM.Name, this.Name, parameter));
        }
    }
}
