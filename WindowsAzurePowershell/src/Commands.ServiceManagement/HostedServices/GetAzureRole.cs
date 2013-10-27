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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.HostedServices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using AutoMapper;
    using Commands.Utilities.Common;
    using Management.Compute;
    using Management.Compute.Models;
    using Management.Models;
    using Model;
    using Model.PersistentVMModel;
    using Properties;
    using PVM = Model.PersistentVMModel;
    using Role = Management.Compute.Models.Role;
    using RoleInstance = Management.Compute.Models.RoleInstance;

    [Cmdlet(VerbsCommon.Get, "AzureRole"), OutputType(typeof(RoleContext), typeof(RoleInstanceContext))]
    public class GetAzureRoleCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the hosted service.")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot")]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the role.")]
        public string RoleName
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Get Instance Details")]
        public SwitchParameter InstanceDetails
        {
            get;
            set;
        }

        public void GetRoleProcess()
        {
            OperationStatusResponse getDeploymentOperation;
            var currentDeployment = this.GetCurrentDeployment(out getDeploymentOperation);
            if (currentDeployment != null)
            {
                if (this.InstanceDetails.IsPresent)
                {
                    Collection<RoleInstanceContext> instanceContexts = new Collection<RoleInstanceContext>();
                    IList<RoleInstance> roleInstances = null;

                    if (string.IsNullOrEmpty(this.RoleName))
                    {
                        roleInstances = currentDeployment.RoleInstances;
                    }
                    else
                    {
                        roleInstances = new List<RoleInstance>(currentDeployment.RoleInstances.Where(r => r.RoleName.Equals(this.RoleName, StringComparison.OrdinalIgnoreCase)));
                    }

                    foreach (var role in roleInstances)
                    {
                        instanceContexts.Add(new RoleInstanceContext()
                        {
                            ServiceName           = this.ServiceName,
                            OperationId           = getDeploymentOperation.Id,
                            OperationDescription  = this.CommandRuntime.ToString(),
                            OperationStatus       = getDeploymentOperation.Status.ToString(),
                            InstanceErrorCode     = role.InstanceErrorCode,
                            InstanceFaultDomain   = role.InstanceFaultDomain,
                            InstanceName          = role.InstanceName,
                            InstanceSize          = role.InstanceSize.ToString(),
                            InstanceStateDetails  = role.InstanceStateDetails,
                            InstanceStatus        = role.InstanceStatus,
                            InstanceUpgradeDomain = role.InstanceUpgradeDomain,
                            RoleName              = role.RoleName,
                            DeploymentID          = currentDeployment.PrivateId,
                            InstanceEndpoints     = Mapper.Map<PVM.InstanceEndpointList>(role.InstanceEndpoints)
                        });
                    }

                    WriteObject(instanceContexts, true);
                }
                else
                {
                    var roleContexts = new Collection<RoleContext>();
                    IList<Role> roles = null;
                    if (string.IsNullOrEmpty(this.RoleName))
                    {
                        roles = currentDeployment.Roles;
                    }
                    else
                    {
                        roles = new List<Role>(currentDeployment.Roles.Where(r => r.RoleName.Equals(this.RoleName, StringComparison.OrdinalIgnoreCase)));
                    }

                    foreach (var r in roles.Select(role => new RoleContext
                    {
                        InstanceCount        = currentDeployment.RoleInstances.Count(ri => ri.RoleName.Equals(role.RoleName, StringComparison.OrdinalIgnoreCase)),
                        RoleName             = role.RoleName,
                        OperationDescription = this.CommandRuntime.ToString(),
                        OperationStatus      = getDeploymentOperation.Status.ToString(),
                        OperationId          = getDeploymentOperation.Id,
                        ServiceName          = this.ServiceName,
                        DeploymentID         = currentDeployment.PrivateId
                    }))
                    {
                        roleContexts.Add(r);
                    }

                    WriteObject(roleContexts, true);
                }
            }
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();
            this.GetRoleProcess();
        }

        private DeploymentGetResponse GetCurrentDeployment(out OperationStatusResponse operation)
        {
            DeploymentSlot slot = string.IsNullOrEmpty(this.Slot) ? DeploymentSlot.Production :
                                                                   (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), this.Slot, true);

            WriteVerboseWithTimestamp(Resources.GetDeploymentBeginOperation);

            DeploymentGetResponse deploymentGetResponse = null;
            InvokeInOperationContext(() => deploymentGetResponse = this.ComputeClient.Deployments.GetBySlot(this.ServiceName, slot));
            operation = GetOperationNewSM(deploymentGetResponse.RequestId);

            WriteVerboseWithTimestamp(Resources.GetDeploymentCompletedOperation);

            return deploymentGetResponse;
        }
    }
}
