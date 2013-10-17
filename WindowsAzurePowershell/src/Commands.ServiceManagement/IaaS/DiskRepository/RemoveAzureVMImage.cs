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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.DiskRepository
{
    using System.Management.Automation;
    using Management.Compute;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Remove, "AzureVMImage"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureVMImage : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the image in the image library to remove.")]
        [ValidateNotNullOrEmpty]
        public string ImageName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Specify to remove the underlying VHD from the blob storage.")]
        public SwitchParameter DeleteVHD { get; set; }

        public void RemoveVMImageProcess()
        {
            ServiceManagementProfile.Initialize();

            // Remove the image from the image repository
            this.ExecuteClientActionNewSM(
                null,
                this.CommandRuntime.ToString(),
                () => this.ComputeClient.VirtualMachineImages.Delete(this.ImageName, this.DeleteVHD.IsPresent));
        }

        protected override void OnProcessRecord()
        {
            this.RemoveVMImageProcess();
        }
    }
}
