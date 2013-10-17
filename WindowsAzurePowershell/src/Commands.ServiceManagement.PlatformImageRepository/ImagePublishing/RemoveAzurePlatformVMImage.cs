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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.PlatformImageRepository.ImagePublishing
{
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.Remove, "AzurePlatformVMImage"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzurePlatformVMImage : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the image in the image library.")]
        [ValidateNotNullOrEmpty]
        public string ImageName
        {
            get;
            set;
        }

        protected override void InitChannelCurrentSubscription(bool force)
        {
            DoInitChannelCurrentSubscription(force);
        }

        protected override void OnProcessRecord()
        {
            this.Channel.GetOSImage(CurrentSubscription.SubscriptionId, this.ImageName);
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.UnReplicateOSImage(s, this.ImageName));
        }
    }
}
