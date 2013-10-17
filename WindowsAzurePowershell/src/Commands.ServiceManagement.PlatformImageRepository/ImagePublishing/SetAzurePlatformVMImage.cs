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
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Properties;
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.Set, "AzurePlatformVMImage", DefaultParameterSetName = ReplicateParameterSetName), OutputType(typeof(ManagementOperationContext))]
    public class SetAzurePlatformVMImage : ServiceManagementBaseCmdlet
    {
        private const string ReplicateParameterSetName = "Replicate";
        private const string ShareParameterSetName = "Share";

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ReplicateParameterSetName, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the image in the image library.")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ShareParameterSetName, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the image in the image library.")]
        [ValidateNotNullOrEmpty]
        public string ImageName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = ReplicateParameterSetName, ValueFromPipelineByPropertyName = true, HelpMessage = "Specifies the locations that image will be replicated.")]
        [ValidateNotNullOrEmpty]
        public string[] ReplicaLocations
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = ShareParameterSetName, ValueFromPipelineByPropertyName = true, HelpMessage = "Specifies the sharing permission of replicated image.")]
        [ValidateSet("Public", "Private")]
        public string Permission
        {
            get;
            set;
        }

        protected override void InitChannelCurrentSubscription(bool force)
        {
            DoInitChannelCurrentSubscription(force);
        }

        public void SetAzurePlatformVMImageProcess()
        {
            if (this.ParameterSpecified("ReplicaLocations"))
            {
                ProcessReplicateImageParameterSet();
            }
            else if (this.ParameterSpecified("Permission"))
            {
                ProcessShareImageParameterSet();
            }
        }

        private bool ParameterSpecified(string parameterName)
        {
            return this.MyInvocation.BoundParameters.ContainsKey(parameterName);
        }

        private void ProcessShareImageParameterSet()
        {
            this.Channel.GetOSImage(CurrentSubscription.SubscriptionId, this.ImageName);
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.ShareOSImage(s, this.ImageName, this.Permission));
        }

        private void ProcessReplicateImageParameterSet()
        {
            this.Channel.GetOSImage(CurrentSubscription.SubscriptionId, this.ImageName);
            ValidateTargetLocations();
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.ReplicateOSImage(s, this.ImageName, CreateReplicationInput()));
        }

        private ReplicationInput CreateReplicationInput()
        {
            var replicationInput = new ReplicationInput
            {
                TargetLocations = new RegionList()
            };

            foreach (var location in ReplicaLocations)
            {
                replicationInput.TargetLocations.Add(location);
            }
            return replicationInput;
        }

        private void ValidateTargetLocations()
        {
            var locations = this.Channel.ListLocations(CurrentSubscription.SubscriptionId);
            if (this.ReplicaLocations != null)
            {
                var invalidValues = ReplicaLocations.Except(locations.Select(l => l.Name), StringComparer.OrdinalIgnoreCase).ToList();

                if (invalidValues.Any())
                {
                    var validValuesMessage = string.Format(Resources.SetAzurePlatformVMImage_Valid_Values, String.Join(", ", locations.Select(l => "'" + l.Name + "'")));
                    var invalidValuesMessage = string.Format(Resources.SetAzurePlatformVMImage_Invalid_Values, String.Join(", ", invalidValues.Select(l => "'" + l + "'")));

                    throw new ArgumentOutOfRangeException("Location", String.Format(Resources.SetAzurePlatformVMImage_Expected_Found, validValuesMessage, invalidValuesMessage));
                }
            }
        }

        protected override void OnProcessRecord()
        {
            this.SetAzurePlatformVMImageProcess();
        }
    }
}
