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
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using AutoMapper;
    using Common;
    using Helpers;
    using Management.Compute;
    using Management.Compute.Models;
    using Properties;
    using Storage;
    using Utilities.Common;
    using ConfigurationSet = Model.PersistentVMModel.ConfigurationSet;
    using InputEndpoint = Model.PersistentVMModel.InputEndpoint;
    using OSVirtualHardDisk = Model.PersistentVMModel.OSVirtualHardDisk;

    /// <summary>
    /// Creates a VM without advanced provisioning configuration options
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureQuickVM", DefaultParameterSetName = "Windows"), OutputType(typeof(ManagementOperationContext))]
    public class NewQuickVM : IaaSDeploymentManagementCmdletBase
    {
        private bool _createdDeployment;

        [Parameter(Mandatory = true, ParameterSetName = "Windows", HelpMessage = "Create a Windows VM")]
        public SwitchParameter Windows
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "Linux", HelpMessage = "Create a Linux VM")]
        public SwitchParameter Linux
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Service Name")]
        [ValidateNotNullOrEmpty]
        override public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Virtual Machine Name")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Reference to a platform stock image or a user image from the image repository.")]
        [ValidateNotNullOrEmpty]
        public string ImageName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Administrator password to use for the role.")]
        [ValidateNotNullOrEmpty]
        public string Password
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Use when creating the first virtual machine in a cloud service (or specify affinity group).  The data center region where the cloud service will be created.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Use when creating the first virtual machine in a cloud service (or specify location). The name of an existing affinity group associated with this subscription.")]
        [ValidateNotNullOrEmpty]
        public string AffinityGroup
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "Linux", HelpMessage = "User to Create")]
        [ValidateNotNullOrEmpty]
        public string LinuxUser
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "Windows", HelpMessage = "Specifies the Administrator to create.")]
        [ValidateNotNullOrEmpty]
        public string AdminUsername
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Set of certificates to install in the VM.")]
        [ValidateNotNullOrEmpty]
        public Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.CertificateSettingList Certificates
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Waits for VM to boot")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter WaitForBoot
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Disables WinRM on https")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter DisableWinRMHttps
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Enables WinRM over http")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter EnableWinRMHttp
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Certificate that will be associated with WinRM endpoint")]
        [ValidateNotNullOrEmpty]
        public X509Certificate2 WinRMCertificate
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "X509Certificates that will be deployed")]
        [ValidateNotNullOrEmpty]
        public X509Certificate2[] X509Certificates
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Prevents the private key from being uploaded")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter NoExportPrivateKey
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Prevents the WinRM endpoint from being added")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter NoWinRMEndpoint
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Linux", HelpMessage = "SSH Public Key List")]
        public Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.LinuxProvisioningConfigurationSet.SSHPublicKeyList SSHPublicKeys
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Linux", HelpMessage = "SSH Key Pairs")]
        public Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.LinuxProvisioningConfigurationSet.SSHKeyPairList SSHKeyPairs
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Virtual network name.")]
        public string VNetName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "The list of subnet names.")]
        [AllowEmptyCollection]
        [AllowNull]
        public string[] SubnetNames
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "DNS Settings for Deployment.")]
        [ValidateNotNullOrEmpty]
        public Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsServer[] DnsSettings
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Controls the platform caching behavior of the OS disk.")]
        [ValidateSet("ReadWrite", "ReadOnly", IgnoreCase = true)]
        public String HostCaching
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "The name of the availability set.")]
        [ValidateNotNullOrEmpty]
        public string AvailabilitySetName
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Represents the size of the machine.")]
        [ValidateSet("ExtraSmall", "Small", "Medium", "Large", "ExtraLarge", "A5", "A6", "A7", IgnoreCase = true)]
        public string InstanceSize
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Location of the where the VHD should be created. This link refers to a blob in a storage account. If not specified the VHD will be created in the current storage account in the vhds container.")]
        [ValidateNotNullOrEmpty]
        public string MediaLocation
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

            bool serviceExists = DoesCloudServiceExist(this.ServiceName);

            if(!string.IsNullOrEmpty(this.Location))
            {
                if(serviceExists)
                {
                    throw new ApplicationException(Resources.ServiceExistsLocationCanNotBeSpecified);
                }
            }

            if (!string.IsNullOrEmpty(this.AffinityGroup))
            {
                if (serviceExists)
                {
                    throw new ApplicationException(Resources.ServiceExistsAffinityGroupCanNotBeSpecified);
                }
            }

            if (!serviceExists)
            {
                try
                {
                    //Implicitly created hosted service2012-05-07 23:12 

                    // Create the Cloud Service when
                    // Location or Affinity Group is Specified
                    // or VNET is specified and the service doesn't exist
                    var parameter = new HostedServiceCreateParameters
                    {
                        AffinityGroup = this.AffinityGroup,
                        Location = this.Location,
                        ServiceName = this.ServiceName,
                        Description = String.Format("Implicitly created hosted service{0}", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm")),
                        Label = this.ServiceName
                    };
                    ExecuteClientActionNewSM(
                        parameter,
                        CommandRuntime + Resources.QuickVMCreateCloudService,
                        () => this.ComputeClient.HostedServices.Create(parameter));

                }

                catch (CloudException ex)
                {
                    this.WriteExceptionDetails(ex);
                    return;
                }
            }

            if (ParameterSetName.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                if (WinRMCertificate != null)
                {
                    if (!CertUtilsNewSM.HasExportablePrivateKey(WinRMCertificate))
                    {
                        throw new ArgumentException(Resources.WinRMCertificateDoesNotHaveExportablePrivateKey);
                    }
                    var operationDescription = string.Format(Resources.QuickVMUploadingWinRMCertificate, CommandRuntime, WinRMCertificate.Thumbprint);
                    var parameters = CertUtilsNewSM.Create(WinRMCertificate);
                    ExecuteClientActionNewSM(
                        null,
                        operationDescription,
                        () => this.ComputeClient.ServiceCertificates.Create(this.ServiceName, parameters),
                        (s, r) => ContextFactory<ComputeOperationStatusResponse, ManagementOperationContext>(r, s));
                }

                if (X509Certificates != null)
                {
                    var certificateFilesWithThumbprint = from c in X509Certificates
                                                         select new
                                                         {
                                                             c.Thumbprint,
                                                             CertificateFile = CertUtilsNewSM.Create(c, this.NoExportPrivateKey.IsPresent)
                                                         };
                    foreach (var current in certificateFilesWithThumbprint.ToList())
                    {
                        var operationDescription = string.Format(Resources.QuickVMUploadingCertificate, CommandRuntime, current.Thumbprint);
                        ExecuteClientActionNewSM(
                            null,
                            operationDescription,
                            () => this.ComputeClient.ServiceCertificates.Create(this.ServiceName, current.CertificateFile),
                            (s, r) => ContextFactory<ComputeOperationStatusResponse, ManagementOperationContext>(r, s));
                    }
                }
            }

            var vm = CreatePersistenVMRole(currentStorage);

            // If the current deployment doesn't exist set it create it
            if (CurrentDeploymentNewSM == null)
            {
                try
                {
                    var parameters = new VirtualMachineCreateDeploymentParameters
                    {
                        DeploymentSlot = DeploymentSlot.Production,
                        Name = this.ServiceName,
                        Label = this.ServiceName,
                        VirtualNetworkName = this.VNetName,
                        Roles = {vm}
                    };

                    if (this.DnsSettings != null)
                    {
                        parameters.DnsSettings = new Management.Compute.Models.DnsSettings();

                        foreach (var dns in this.DnsSettings)
                        {
                            parameters.DnsSettings.DnsServers.Add(new Microsoft.WindowsAzure.Management.Compute.Models.DnsServer() { Name = dns.Name, Address = dns.Address });
                        }
                    }

                    var operationDescription = string.Format(Resources.QuickVMCreateDeploymentWithVM, CommandRuntime, vm.RoleName);
                    ExecuteClientActionNewSM(
                        parameters,
                        operationDescription,
                        () => this.ComputeClient.VirtualMachines.CreateDeployment(this.ServiceName, parameters));
                        
                    if (WaitForBoot.IsPresent)
                    {
                        WaitForRoleToBoot(vm.RoleName);
                    }
                }
                catch (CloudException ex)
                {
                    if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception(Resources.ServiceDoesNotExistSpecifyLocationOrAffinityGroup);
                    }
                    this.WriteExceptionDetails(ex);
                    return;
                }

                _createdDeployment = true;
            }
            else
            {
                if (VNetName != null || DnsSettings != null)
                {
                    WriteWarning(Resources.VNetNameOrDnsSettingsCanOnlyBeSpecifiedOnNewDeployments);
                }
            }


            // Only create the VM when a new VM was added and it was not created during the deployment phase.
            if ((_createdDeployment == false))
            {
                try
                {
                    var operationDescription = string.Format(Resources.QuickVMCreateVM, CommandRuntime, vm.RoleName);

                    var parameter = new VirtualMachineCreateParameters
                    {
                        AvailabilitySetName = vm.AvailabilitySetName,
                        OSVirtualHardDisk = Mapper.Map(vm.OSVirtualHardDisk, new Management.Compute.Models.OSVirtualHardDisk()),
                        RoleName = vm.RoleName,
                        RoleSize = vm.RoleSize,
                        ProvisionGuestAgent = true
                    };

                    vm.DataVirtualHardDisks.ForEach(c => parameter.DataVirtualHardDisks.Add(c));
                    vm.ConfigurationSets.ForEach(c => parameter.ConfigurationSets.Add(c));

                    ExecuteClientActionNewSM(
                        vm,
                        operationDescription,
                        () => this.ComputeClient.VirtualMachines.Create(this.ServiceName, this.ServiceName, parameter));

                    if(WaitForBoot.IsPresent)
                    {
                        WaitForRoleToBoot(vm.RoleName);
                    }
                }
                catch (CloudException ex)
                {
                    this.WriteExceptionDetails(ex);
                    return;
                }
            }
        }

        private Management.Compute.Models.Role CreatePersistenVMRole(CloudStorageAccount currentStorage)
        {
            var vm = new Management.Compute.Models.Role
            {
                AvailabilitySetName = AvailabilitySetName,
                RoleName = String.IsNullOrEmpty(Name) ? ServiceName : Name, // default like the portal
                RoleSize = string.IsNullOrEmpty(InstanceSize) ? null :
                           (VirtualMachineRoleSize?)Enum.Parse(typeof(VirtualMachineRoleSize), InstanceSize, true),
                RoleType = "PersistentVMRole",
                Label = ServiceName,
                OSVirtualHardDisk = Mapper.Map(new OSVirtualHardDisk
                {
                    DiskName = null,
                    SourceImageName = ImageName,
                    MediaLink = string.IsNullOrEmpty(MediaLocation) ? null : new Uri(MediaLocation),
                    HostCaching = HostCaching
                }, new Management.Compute.Models.OSVirtualHardDisk()),
                ProvisionGuestAgent = true,
            };

            if (vm.OSVirtualHardDisk.MediaLink == null && String.IsNullOrEmpty(vm.OSVirtualHardDisk.DiskName))
            {
                var mediaLinkFactory = new MediaLinkFactory(currentStorage, this.ServiceName, vm.RoleName);
                vm.OSVirtualHardDisk.MediaLink = mediaLinkFactory.Create();
            }

            var netConfig = CreateNetworkConfigurationSet();

            if (ParameterSetName.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                var windowsConfig = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.WindowsProvisioningConfigurationSet
                {
                    AdminUsername = this.AdminUsername,
                    AdminPassword = Password,
                    ComputerName =
                        string.IsNullOrEmpty(Name) ? ServiceName : Name,
                    EnableAutomaticUpdates = true,
                    ResetPasswordOnFirstLogon = false,
                    StoredCertificateSettings = CertUtilsNewSM.GetCertificateSettings(this.Certificates, this.X509Certificates),
                    WinRM = GetWinRmConfiguration()
                };

                if (windowsConfig.StoredCertificateSettings == null)
                {
                    windowsConfig.StoredCertificateSettings = new Model.PersistentVMModel.CertificateSettingList();
                }

                netConfig.InputEndpoints.Add(new InputEndpoint {LocalPort = 3389, Protocol = "tcp", Name = "RemoteDesktop"});
                if (!this.NoWinRMEndpoint.IsPresent && !this.DisableWinRMHttps.IsPresent)
                {
                    netConfig.InputEndpoints.Add(new InputEndpoint {LocalPort = WinRMConstants.HttpsListenerPort, Protocol = "tcp", Name = WinRMConstants.EndpointName});
                }
                var configurationSets = new Collection<ConfigurationSet>{windowsConfig, netConfig};
                PersistentVMHelper.MapConfigurationSets(configurationSets).ForEach(c => vm.ConfigurationSets.Add(c));
            }
            else
            {
                var linuxConfig = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.LinuxProvisioningConfigurationSet
                {
                    HostName = string.IsNullOrEmpty(this.Name) ? this.ServiceName : this.Name,
                    UserName = this.LinuxUser,
                    UserPassword = this.Password,
                    DisableSshPasswordAuthentication = false
                };

                if (this.SSHKeyPairs != null && this.SSHKeyPairs.Count > 0 ||
                    this.SSHPublicKeys != null && this.SSHPublicKeys.Count > 0)
                {
                    linuxConfig.SSH = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.LinuxProvisioningConfigurationSet.SSHSettings
                    {
                        PublicKeys = this.SSHPublicKeys, 
                        KeyPairs = this.SSHKeyPairs
                    };
                }

                var rdpEndpoint = new InputEndpoint {LocalPort = 22, Protocol = "tcp", Name = "SSH"};
                netConfig.InputEndpoints.Add(rdpEndpoint);

                var configurationSets = new Collection<ConfigurationSet> { linuxConfig, netConfig };
                PersistentVMHelper.MapConfigurationSets(configurationSets).ForEach(c => vm.ConfigurationSets.Add(c));
            }

            return vm;
        }

        private Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.NetworkConfigurationSet CreateNetworkConfigurationSet()
        {
            var netConfig = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.NetworkConfigurationSet {InputEndpoints = new Collection<InputEndpoint>()};
            if (SubnetNames != null)
            {
                netConfig.SubnetNames = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.SubnetNamesCollection();
                foreach (var subnet in SubnetNames)
                {
                    netConfig.SubnetNames.Add(subnet);
                }
            }
            return netConfig;
        }

        private Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.WindowsProvisioningConfigurationSet.WinRmConfiguration GetWinRmConfiguration()
        {
            if(this.DisableWinRMHttps.IsPresent)
            {
                return null;
            }

            var builder = new WinRmConfigurationBuilder();
            if(this.EnableWinRMHttp.IsPresent)
            {
                builder.AddHttpListener();
            }
            builder.AddHttpsListener(WinRMCertificate);
            return builder.Configuration;
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

        protected bool DoesCloudServiceExist(string serviceName)
        {
            try
            {
                WriteVerboseWithTimestamp(string.Format(Resources.QuickVMBeginOperation, CommandRuntime));
                var response = this.ComputeClient.HostedServices.CheckNameAvailability(serviceName);
                WriteVerboseWithTimestamp(string.Format(Resources.QuickVMCompletedOperation, CommandRuntime));
                return !response.IsAvailable;
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

        protected void ValidateParameters()
        {
            if (this.DnsSettings != null && string.IsNullOrEmpty(this.VNetName))
            {
                throw new ArgumentException(Resources.VNetNameRequiredWhenSpecifyingDNSSettings);
            }

            if (this.ParameterSetName.Contains("Linux") && string.IsNullOrEmpty(this.LinuxUser))
            {
                throw new ArgumentException(Resources.SpecifyLinuxUserWhenCreatingLinuxVMs);
            }

            if (this.ParameterSetName.Contains("Linux") && !ValidationHelpers.IsLinuxPasswordValid(this.Password))
            {
                throw new ArgumentException(Resources.PasswordNotComplexEnough);
            }

            if (this.ParameterSetName.Contains("Windows") && !ValidationHelpers.IsWindowsPasswordValid(this.Password))
            {
                throw new ArgumentException(Resources.PasswordNotComplexEnough);
            }

            if (this.ParameterSetName.Contains("Linux"))
            {
                bool valid = false;
                if (string.IsNullOrEmpty(this.Name))
                {
                    valid = ValidationHelpers.IsLinuxHostNameValid(this.ServiceName); // uses servicename if name not specified
                }
                else
                {
                    valid = ValidationHelpers.IsLinuxHostNameValid(this.Name); 
                }
                if (valid == false)
                {
                    throw new ArgumentException(Resources.InvalidHostName);
                }
            }

            if (string.IsNullOrEmpty(this.Name) == false)
            {
                if (this.ParameterSetName.Contains("Windows") && !ValidationHelpers.IsWindowsComputerNameValid(this.Name))
                {
                    throw new ArgumentException(Resources.InvalidComputerName);
                }
            }
            else
            {
                if (this.ParameterSetName.Contains("Windows") && !ValidationHelpers.IsWindowsComputerNameValid(this.ServiceName))
                {
                    throw new ArgumentException(Resources.InvalidComputerName);
                }
            }

            if (!string.IsNullOrEmpty(this.Location) && !string.IsNullOrEmpty(this.AffinityGroup))
            {
                throw new ArgumentException(Resources.EitherLocationOrAffinityGroupBeSpecified);
            }

            if (String.IsNullOrEmpty(this.VNetName) == false && (String.IsNullOrEmpty(this.Location) && String.IsNullOrEmpty(this.AffinityGroup)))
            {
                throw new ArgumentException(Resources.VNetNameCanBeSpecifiedOnlyOnInitialDeployment);
            }
        }
    }
}
