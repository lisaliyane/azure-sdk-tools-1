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
    using Commands.Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.Get, "AzurePlatformVMImage"), OutputType(typeof(OSImageDetailsContext))]
    public class GetAzurePlatformVMImage : ServiceManagementBaseCmdlet
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
            Func<Operation, OSImageDetails, object> func = (operation, imageDetails) =>
            {
                OSImageDetailsContext osImageDetailsContext = null;

                if (imageDetails != null)
                {
                    osImageDetailsContext = new OSImageDetailsContext
                    {
                        OperationId = operation.OperationTrackingId,
                        OperationDescription = CommandRuntime.ToString(),
                        OperationStatus = operation.Status,
                        AffinityGroup = imageDetails.AffinityGroup,
                        Category = imageDetails.Category,
                        Label = imageDetails.Label,
                        Location = imageDetails.Location,
                        MediaLink = imageDetails.MediaLink,
                        ImageName = imageDetails.Name,
                        OS = imageDetails.OS,
                        LogicalSizeInGB = imageDetails.LogicalSizeInGB,
                        Eula = imageDetails.Eula,
                        Description = imageDetails.Description,
                        IconUri = imageDetails.IconUri,
                        ImageFamily = imageDetails.ImageFamily,
                        IsPremium = imageDetails.IsPremium,
                        PrivacyUri = imageDetails.PrivacyUri,
                        PublishedDate = imageDetails.PublishedDate,
                        RecommendedVMSize = imageDetails.RecommendedVMSize,
                        IsCorrupted = imageDetails.IsCorrupted,
                        ReplicationProgress = imageDetails.ReplicationProgress.Select(
                                                      detail => new ReplicationProgressContext
                                                      {
                                                          Location = detail.Location,
                                                          Progress = detail.Progress
                                                      }).ToList()
                    };
                }

                return osImageDetailsContext;
            };

            ExecuteClientActionInOCS(
                null,
                CommandRuntime.ToString(),
                s => this.Channel.GetOSImageWithDetails(s, this.ImageName),
                func);
        }
    }
}
