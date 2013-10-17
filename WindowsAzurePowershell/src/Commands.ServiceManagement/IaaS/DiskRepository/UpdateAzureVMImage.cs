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
    using System;
    using System.Management.Automation;
    using Management.Compute;
    using Management.Compute.Models;
    using Model;
    using Utilities.Common;

    [Cmdlet(VerbsData.Update, "AzureVMImage"), OutputType(typeof(OSImageContext))]
    public class UpdateAzureVMImage : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the image in the image library.")]
        [ValidateNotNullOrEmpty]
        public string ImageName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Label of the image.")]
        [ValidateNotNullOrEmpty]
        public string Label { get; set; }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, HelpMessage = "Specifies the End User License Aggreement, recommended value is a URL.")]
        [ValidateNotNullOrEmpty]
        public string Eula { get; set; }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, HelpMessage = "Specifies the description of the OS image.")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, HelpMessage = "Specifies a value that can be used to group OS images.")]
        [ValidateNotNullOrEmpty]
        public string ImageFamily { get; set; }

        [Parameter(Position = 5, ValueFromPipelineByPropertyName = true, HelpMessage = "Specifies the date when the OS image was added to the image repository.")]
        [ValidateNotNullOrEmpty]
        public DateTime? PublishedDate { get; set; }

        [Parameter(Position = 6, ValueFromPipelineByPropertyName = true, HelpMessage = "Specifies the URI that points to a document that contains the privacy policy related to the OS image.")]
        [ValidateNotNullOrEmpty]
        public Uri PrivacyUri { get; set; }

        [Parameter(Position = 7, ValueFromPipelineByPropertyName = true, HelpMessage = " Specifies the size to use for the virtual machine that is created from the OS image.")]
        [ValidateSet("Small", "Medium", "Large", "ExtraLarge", "A5", "A6", "A7", IgnoreCase = true)]
        public string RecommendedVMSize { get; set; }
        
        public void UpdateVMImageProcess()
        {
            var parameters = new VirtualMachineImageUpdateParameters
            {
                Label = this.Label,
                Eula = this.Eula,
                Description = this.Description,
                ImageFamily = this.ImageFamily,
                PublishedDate = this.PublishedDate,
                PrivacyUri = this.PrivacyUri,
                RecommendedVMSize = string.IsNullOrEmpty(this.RecommendedVMSize) ? null :
                                    (VirtualMachineRoleSize?)Enum.Parse(typeof(VirtualMachineRoleSize), this.RecommendedVMSize, true)
            };

            this.ExecuteClientActionNewSM(
                null,
                this.CommandRuntime.ToString(),
                () => this.ComputeClient.VirtualMachineImages.Update(this.ImageName, parameters),
                (s, response) => this.ContextFactory<VirtualMachineImageUpdateResponse, OSImageContext>(response, s));
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();
            this.UpdateVMImageProcess();
        }
    }
}