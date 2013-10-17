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
    using Management.Compute;
    using Management.Compute.Models;
    using Model;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Get, "AzureDisk"), OutputType(typeof(IEnumerable<DiskContext>))]
    public class GetAzureDiskCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, HelpMessage = "Name of the disk in the disk library.")]
        [ValidateNotNullOrEmpty]
        public string DiskName { get; set; }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            if (!string.IsNullOrEmpty(this.DiskName))
            {
                this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachineDisks.GetDisk(this.DiskName),
                    (s, response) => this.ContextFactory<VirtualMachineDiskGetDiskResponse, DiskContext>(response, s));
            }
            else
            {
                this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachineDisks.ListDisks(),
                    (s, response) => response.Disks.Select(disk => this.ContextFactory<VirtualMachineDiskListResponse.VirtualMachineDisk, DiskContext>(disk, s)));
            }
        }
    }
}