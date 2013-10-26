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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.PersistentVMs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using AutoMapper;
    using Helpers;
    using IaaS.Extensions;
    using Management.Compute;
    using Management.Compute.Models;
    using Model;
    using Properties;
    using Storage;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.New, "AzureVM", DefaultParameterSetName = "ExistingService"), OutputType(typeof(ManagementOperationContext))]
    public class NewAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        private bool createdDeployment;

        [Parameter(Mandatory = true, ParameterSetName = "CreateService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service Name")]
        [Parameter(Mandatory = true, ParameterSetName = "ExistingService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service Name")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CreateService", HelpMessage = "Required if AffinityGroup is not specified. The data center region where the cloud service will be created.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "CreateService", HelpMessage = "Required if Location is not specified. The name of an existing affinity group associated with this subscription.")]
        [ValidateNotNullOrEmpty]
        public string AffinityGroup
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "CreateService", HelpMessage = "The label may be up to 100 characters in length. Defaults to Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceLabel
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "CreateService", HelpMessage = "A description for the cloud service. The description may be up to 1024 characters in length.")]
        [ValidateNotNullOrEmpty]
        public string ServiceDescription
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "CreateService", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Label. Will default to service name if not specified.")]
        [Parameter(Mandatory = false, ParameterSetName = "ExistingService", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Label. Will default to service name if not specified.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentLabel
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "CreateService", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Name. Will default to service name if not specified.")]
        [Parameter(Mandatory = false, ParameterSetName = "ExistingService", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Name. Will default to service name if not specified.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "CreateService", HelpMessage = "Virtual network name.")]
        [Parameter(Mandatory = false, ParameterSetName = "ExistingService", HelpMessage = "Virtual network name.")]
        public string VNetName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "CreateService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "DNS Settings for Deployment.")]
        [Parameter(Mandatory = false, ParameterSetName = "ExistingService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "DNS Settings for Deployment.")]
        [ValidateNotNullOrEmpty]
        public Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsServer[] DnsSettings
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "CreateService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List of VMs to Deploy.")]
        [Parameter(Mandatory = true, ParameterSetName = "ExistingService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List of VMs to Deploy.")]
        [ValidateNotNullOrEmpty]
        public PersistentVM[] VMs
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Waits for VM to boot")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter WaitForBoot
        {
            get;
            set;
        }

        public void NewAzureVMProcess()
        {
            WindowsAzureSubscription currentSubscription = CurrentSubscription;
            CloudStorageAccount currentStorage = null;
            try
            {
                currentStorage = currentSubscription.GetCloudStorageAccount();
            }
            catch (Exception ex) // couldn't access
            {
                throw new ArgumentException(Resources.CurrentStorageAccountIsNotAccessible, ex);
            }
            if (currentStorage == null) // not set
            {
                throw new ArgumentException(Resources.CurrentStorageAccountIsNotSet);
            }

            try
            {
                if (this.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase))
                {
                    var parameter = new HostedServiceCreateParameters
                    {
                        AffinityGroup = this.AffinityGroup,
                        Location = this.Location,
                        ServiceName = this.ServiceName,
                        Description = this.ServiceDescription ??
                                        String.Format("Implicitly created hosted service{0}",DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm")),
                        Label = this.ServiceLabel ?? this.ServiceName
                    };
                    ExecuteClientActionNewSM(
                        parameter,
                        CommandRuntime + " - Create Cloud Service",
                        () => this.ComputeClient.HostedServices.Create(parameter));
                }
            }
            catch (CloudException ex)
            {
                this.WriteExceptionDetails(ex);
                return;
            }

            foreach (var vm in from v in VMs let configuration = v.ConfigurationSets.OfType<Model.PersistentVMModel.WindowsProvisioningConfigurationSet>().FirstOrDefault() where configuration != null select v)
            {
                if (vm.WinRMCertificate != null)
                {
                    if(!CertUtilsNewSM.HasExportablePrivateKey(vm.WinRMCertificate))
                    {
                        throw new ArgumentException(Resources.WinRMCertificateDoesNotHaveExportablePrivateKey);
                    }
                    var operationDescription = string.Format(Resources.AzureVMUploadingWinRMCertificate, CommandRuntime, vm.WinRMCertificate.Thumbprint);
                    var parameters = CertUtilsNewSM.Create(vm.WinRMCertificate);
                    ExecuteClientActionNewSM(
                        null,
                        operationDescription,
                        () => this.ComputeClient.ServiceCertificates.Create(this.ServiceName, parameters),
                        (s, r) => ContextFactory<ComputeOperationStatusResponse, ManagementOperationContext>(r, s));

                }
                var certificateFilesWithThumbprint = from c in vm.X509Certificates
                    select new
                           {
                               c.Thumbprint,
                               CertificateFile = CertUtilsNewSM.Create(c, vm.NoExportPrivateKey)
                           };
                foreach (var current in certificateFilesWithThumbprint.ToList())
                {
                    var operationDescription = string.Format(Resources.AzureVMCommandUploadingCertificate, CommandRuntime, current.Thumbprint);
                    ExecuteClientActionNewSM(
                        null,
                        operationDescription,
                        () => this.ComputeClient.ServiceCertificates.Create(this.ServiceName, current.CertificateFile),
                        (s, r) => ContextFactory<ComputeOperationStatusResponse, ManagementOperationContext>(r, s));
                }
            }

            var persistentVMs = this.VMs.Select(vm => CreatePersistentVMRole(vm, currentStorage)).ToList();

            // If the current deployment doesn't exist set it create it
            if (CurrentDeploymentNewSM == null)
            {
                try
                {
                    var parameters = new VirtualMachineCreateDeploymentParameters
                    {
                        DeploymentSlot = DeploymentSlot.Production,
                        Name = this.DeploymentName ?? this.ServiceName,
                        Label = this.DeploymentLabel ?? this.ServiceName,
                        VirtualNetworkName = this.VNetName,
                        Roles = { persistentVMs[0] }
                    };

                    if (this.DnsSettings != null)
                    {
                        parameters.DnsSettings = new Management.Compute.Models.DnsSettings();

                        foreach (var dns in this.DnsSettings)
                        {
                            parameters.DnsSettings.DnsServers.Add(new Microsoft.WindowsAzure.Management.Compute.Models.DnsServer() { Name = dns.Name, Address = dns.Address });
                        }
                    }

                    var operationDescription = string.Format(Resources.AzureVMCommandCreateDeploymentWithVM, CommandRuntime, persistentVMs[0].RoleName);
                    ExecuteClientActionNewSM(
                        parameters,
                        operationDescription,
                        () => this.ComputeClient.VirtualMachines.CreateDeployment(this.ServiceName, parameters));

                    if(this.WaitForBoot.IsPresent)
                    {
                        WaitForRoleToBoot(persistentVMs[0].RoleName);
                    }
                }
                catch (CloudException ex)
                {
                    if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception(Resources.ServiceDoesNotExistSpecifyLocationOrAffinityGroup);
                    }
                    else
                    {
                        this.WriteExceptionDetails(ex);
                    }
                    return;
                }

                this.createdDeployment = true;
            }
            else
            {
                if (this.VNetName != null || this.DnsSettings != null || !string.IsNullOrEmpty(this.DeploymentLabel) || !string.IsNullOrEmpty(this.DeploymentName))
                {
                    WriteWarning(Resources.VNetNameDnsSettingsDeploymentLabelDeploymentNameCanBeSpecifiedOnNewDeployments);
                }
            }

            if (this.createdDeployment == false && CurrentDeploymentNewSM != null)
            {
                this.DeploymentName = CurrentDeploymentNewSM.Name;
            }

            int startingVM = this.createdDeployment ? 1 : 0;

            for (int i = startingVM; i < persistentVMs.Count; i++)
            {
                var operationDescription = string.Format(Resources.AzureVMCommandCreateVM, CommandRuntime, persistentVMs[i].RoleName);
                
                var parameter = new VirtualMachineCreateParameters
                {
                    AvailabilitySetName = persistentVMs[i].AvailabilitySetName,
                    OSVirtualHardDisk = persistentVMs[i].OSVirtualHardDisk,
                    RoleName = persistentVMs[i].RoleName,
                    RoleSize = persistentVMs[i].RoleSize,
                    ProvisionGuestAgent = persistentVMs[i].ProvisionGuestAgent,
                    ResourceExtensionReferences = persistentVMs[i].ResourceExtensionReferences
                };

                persistentVMs[i].DataVirtualHardDisks.ForEach(c => parameter.DataVirtualHardDisks.Add(c));
                persistentVMs[i].ConfigurationSets.ForEach(c => parameter.ConfigurationSets.Add(c));

                ExecuteClientActionNewSM(
                    persistentVMs[i],
                    operationDescription,
                    () => this.ComputeClient.VirtualMachines.Create(this.ServiceName, this.DeploymentName ?? this.ServiceName, parameter));
            }

            if(this.WaitForBoot.IsPresent)
            {
                for (int i = startingVM; i < persistentVMs.Count; i++)
                {
                    WaitForRoleToBoot(persistentVMs[i].RoleName);
                }
            }
        }

        private Management.Compute.Models.Role CreatePersistentVMRole(PersistentVM persistentVM, CloudStorageAccount currentStorage)
        {
            if (!string.IsNullOrEmpty(persistentVM.OSVirtualHardDisk.DiskName) && !NetworkConfigurationSetBuilder.HasNetworkConfigurationSet(persistentVM.ConfigurationSets))
            {
//                var networkConfigurationSetBuilder = new NetworkConfigurationSetBuilder(persistentVM.ConfigurationSets);
//
//                Disk disk = this.Channel.GetDisk(CurrentSubscription.SubscriptionId, persistentVM.OSVirtualHardDisk.DiskName);
//                if (disk.OS == OS.Windows && !persistentVM.NoRDPEndpoint)
//                {
//                    networkConfigurationSetBuilder.AddRdpEndpoint();
//                }
//                else if (disk.OS == OS.Linux && !persistentVM.NoSSHEndpoint)
//                {
//                    networkConfigurationSetBuilder.AddSshEndpoint();
//                }
            }

            var mediaLinkFactory = new MediaLinkFactory(currentStorage, this.ServiceName, persistentVM.RoleName);

            if (persistentVM.OSVirtualHardDisk.MediaLink == null && string.IsNullOrEmpty(persistentVM.OSVirtualHardDisk.DiskName))
            {
                persistentVM.OSVirtualHardDisk.MediaLink = mediaLinkFactory.Create();
            }

            foreach (var datadisk in persistentVM.DataVirtualHardDisks.Where(d => d.MediaLink == null && string.IsNullOrEmpty(d.DiskName)))
            {
                datadisk.MediaLink = mediaLinkFactory.Create();
            }

            var result = new Management.Compute.Models.Role
            {
                AvailabilitySetName = persistentVM.AvailabilitySetName,
                OSVirtualHardDisk = Mapper.Map(persistentVM.OSVirtualHardDisk, new Management.Compute.Models.OSVirtualHardDisk()),
                RoleName = persistentVM.RoleName,
                RoleSize = string.IsNullOrEmpty(persistentVM.RoleSize) ? null :
                           (VirtualMachineRoleSize?)Enum.Parse(typeof(VirtualMachineRoleSize), persistentVM.RoleSize, true),
                RoleType = persistentVM.RoleType,
                Label = persistentVM.Label,
                ProvisionGuestAgent = persistentVM.ProvisionGuestAgent,
                ResourceExtensionReferences = Mapper.Map<List<ResourceExtensionReference>>(persistentVM.ResourceExtensionReferences)
            };

            if (persistentVM.DataVirtualHardDisks != null)
            {
                persistentVM.DataVirtualHardDisks.ForEach(c => 
                {
                    var dataDisk = Mapper.Map(c, new Microsoft.WindowsAzure.Management.Compute.Models.DataVirtualHardDisk());
                    dataDisk.LogicalUnitNumber = dataDisk.LogicalUnitNumber;
                    result.DataVirtualHardDisks.Add(dataDisk);
                });
            }

            if (persistentVM.ConfigurationSets != null)
            {
                PersistentVMHelper.MapConfigurationSets(persistentVM.ConfigurationSets).ForEach(c => result.ConfigurationSets.Add(c));
            }

            return result;
        }

        protected override void ProcessRecord()
        {
            try
            {
                ServiceManagementProfile.Initialize();
                this.ValidateParameters();
                base.ProcessRecord();
                this.NewAzureVMProcess();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        protected void ValidateParameters()
        {
            if (ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(AffinityGroup))
                {
                    throw new ArgumentException(Resources.LocationOrAffinityGroupRequiredWhenCreatingNewCloudService);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(AffinityGroup))
                {
                    throw new ArgumentException(Resources.LocationOrAffinityGroupCanOnlyBeSpecifiedWhenNewCloudService);
                }
            }

            if (this.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (!string.IsNullOrEmpty(this.VNetName) && string.IsNullOrEmpty(this.AffinityGroup))
                {
                    throw new ArgumentException(Resources.MustSpecifySameAffinityGroupAsVirtualNetwork);
                }
            }

            if (this.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase) == true || this.ParameterSetName.Equals("CreateDeployment", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (this.DnsSettings != null && string.IsNullOrEmpty(this.VNetName))
                {
                    throw new ArgumentException(Resources.VNetNameRequiredWhenSpecifyingDNSSettings);
                }
            }

            foreach (var pVM in this.VMs)
            {
                var provisioningConfiguration = pVM.ConfigurationSets
                                    .OfType<Model.PersistentVMModel.ProvisioningConfigurationSet>()
                                    .SingleOrDefault();

                if (provisioningConfiguration == null && pVM.OSVirtualHardDisk.SourceImageName != null)
                {
                    throw new ArgumentException(string.Format(Resources.VMMissingProvisioningConfiguration, pVM.RoleName));
                }
            }
        }

        protected bool DoesCloudServiceExist(string serviceName)
        {
            try
            {
                WriteVerboseWithTimestamp(string.Format(Resources.AzureVMBeginOperation, CommandRuntime));
                var response = this.ComputeClient.HostedServices.CheckNameAvailability(serviceName);
                WriteVerboseWithTimestamp(string.Format(Resources.AzureVMCompletedOperation, CommandRuntime));
                return response.IsAvailable;
            }
            catch (CloudException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                this.WriteExceptionDetails(ex);
            }

            return false;
        }
    }
}