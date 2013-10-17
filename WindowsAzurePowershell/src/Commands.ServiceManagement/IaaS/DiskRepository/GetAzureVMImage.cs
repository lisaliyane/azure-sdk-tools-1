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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Management.Compute;
    using Management.Compute.Models;
    using Model;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Get, "AzureVMImage"), OutputType(typeof(IEnumerable<OSImageContext>))]
    public class GetAzureVMImage : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, HelpMessage = "Name of the image in the image library.")]
        [ValidateNotNullOrEmpty]
        public string ImageName { get; set; }

        protected void GetAzureVMImageProcess()
        {
            ServiceManagementProfile.Initialize();

            if (!string.IsNullOrEmpty(this.ImageName))
            {
                this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachineImages.Get(this.ImageName),
                    (s, response) => this.ContextFactory<VirtualMachineImageGetResponse, OSImageContext>(response, s));
            }
            else
            {
                this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachineImages.List(),
                    (s, response) => response.Images.Select(image => this.ContextFactory<VirtualMachineImageListResponse.VirtualMachineImage, OSImageContext>(image, s)));
            }
        }

        protected override void OnProcessRecord()
        {
            GetAzureVMImageProcess();
        }
    }
}