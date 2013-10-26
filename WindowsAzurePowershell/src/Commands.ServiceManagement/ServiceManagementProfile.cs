namespace Microsoft.WindowsAzure.Commands.ServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using AutoMapper;
    using Management.Compute.Models;
    using Management.Models;
    using Management.Storage.Models;
    using Model;
    using Utilities.Common;
    using NSM = Microsoft.WindowsAzure.Management.Compute.Models;
    using PVM = Model.PersistentVMModel;

    public class ServiceManagementProfile : Profile
    {
        private static readonly Lazy<bool> initialize;

        static ServiceManagementProfile()
        {
            initialize = new Lazy<bool>(() =>
            {
                Mapper.Initialize(m => m.AddProfile<ServiceManagementProfile>());
                return true;
            });
        }

        public static bool Initialize()
        {
            return initialize.Value;
        }

        public override string ProfileName
        {
            get { return "ServiceManagementProfile"; }
        }

        protected override void Configure()
        {
            //SM to NewSM mapping
            Mapper.CreateMap<PVM.LoadBalancerProbe, NSM.LoadBalancerProbe>()
                  .ForMember(c => c.Protocol, o => o.MapFrom(r => r.Protocol));
            Mapper.CreateMap<PVM.InputEndpoint, NSM.InputEndpoint>()
                  .ForMember(c => c.VirtualIPAddress, o => o.MapFrom(r => r.Vip != null ? IPAddress.Parse(r.Vip) : null));
            Mapper.CreateMap<PVM.DataVirtualHardDisk, NSM.DataVirtualHardDisk>()
                  .ForMember(c => c.LogicalUnitNumber, o => o.MapFrom(r => r.Lun));
            Mapper.CreateMap<PVM.OSVirtualHardDisk, NSM.OSVirtualHardDisk>()
                  .ForMember(c => c.OperatingSystem, o => o.MapFrom(r => r.OS));
            Mapper.CreateMap<PVM.NetworkConfigurationSet, NSM.ConfigurationSet>()
                  .ForMember(c => c.InputEndpoints, o => o.MapFrom(r => r.InputEndpoints != null ? r.InputEndpoints.ToList() : null))
                  .ForMember(c => c.SubnetNames, o => o.MapFrom(r => r.SubnetNames != null ? r.SubnetNames.ToList() : null));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.ProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.ConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.InstanceEndpoint, NSM.InstanceEndpoint>()
                  .ForMember(c => c.VirtualIPAddress, o => o.MapFrom(r => r.Vip != null ? IPAddress.Parse(r.Vip) : null));
            Mapper.CreateMap<PVM.InstanceEndpointList, IList<NSM.InstanceEndpoint>>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration, NSM.WindowsRemoteManagementSettings>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties, NSM.WindowsRemoteManagementListener>()
                  .ForMember(c => c.ListenerType, o => o.MapFrom(r => r.Protocol));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection, IList<NSM.WindowsRemoteManagementListener>>();

            Mapper.CreateMap<Microsoft.WindowsAzure.ServiceManagement.RoleInstanceList, IList<NSM.RoleInstance>>();

            //NewSM to SM mapping
            Mapper.CreateMap<NSM.LoadBalancerProbe, PVM.LoadBalancerProbe>()
                  .ForMember(c => c.Protocol, o => o.MapFrom(r => r.Protocol.ToString().ToLower()));
            Mapper.CreateMap<NSM.InputEndpoint, PVM.InputEndpoint>()
                  .ForMember(c => c.Vip, o => o.MapFrom(r => r.VirtualIPAddress != null ? r.VirtualIPAddress.ToString() : null));
            Mapper.CreateMap<NSM.DataVirtualHardDisk, PVM.DataVirtualHardDisk>()
                  .ForMember(c => c.Lun, o => o.MapFrom(r => r.LogicalUnitNumber));
            Mapper.CreateMap<NSM.OSVirtualHardDisk, PVM.OSVirtualHardDisk>()
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystem));
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.ConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.NetworkConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.WindowsProvisioningConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.LinuxProvisioningConfigurationSet>();
            Mapper.CreateMap<NSM.InstanceEndpoint, PVM.InstanceEndpoint>()
                  .ForMember(c => c.Vip, o => o.MapFrom(r => r.VirtualIPAddress != null ? r.VirtualIPAddress.ToString() : null));
            Mapper.CreateMap<IList<NSM.InstanceEndpoint>, PVM.InstanceEndpointList>();
            Mapper.CreateMap<NSM.WindowsRemoteManagementSettings, PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration>();
            Mapper.CreateMap<NSM.WindowsRemoteManagementListener, PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties>()
                  .ForMember(c => c.Protocol, o => o.MapFrom(r => r.ListenerType.ToString()));
            Mapper.CreateMap<IList<NSM.WindowsRemoteManagementListener>, PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection>();

            Mapper.CreateMap<IList<NSM.RoleInstance>, Microsoft.WindowsAzure.ServiceManagement.RoleInstanceList>();

            // LoadBalancedEndpointList mapping
            Mapper.CreateMap<PVM.AccessControlListRule, NSM.AccessControlListRule>();
            Mapper.CreateMap<PVM.EndpointAccessControlList, NSM.AccessControlList>();
            Mapper.CreateMap<PVM.InputEndpoint, VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>()
                  .ForMember(c => c.Rules, o => o.MapFrom(r => r.EndpointAccessControlList == null ? null : r.EndpointAccessControlList.Rules))
                  .ForMember(c => c.VirtualIPAddress, o => o.MapFrom(r => r.Vip));
            Mapper.CreateMap<PVM.LoadBalancedEndpointList, IList<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>>();
            Mapper.CreateMap<PVM.LoadBalancedEndpointList, List<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>>();

            Mapper.CreateMap<NSM.AccessControlListRule, PVM.AccessControlListRule>();
            Mapper.CreateMap<NSM.AccessControlList, PVM.EndpointAccessControlList>();
            Mapper.CreateMap<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint, PVM.InputEndpoint>()
                  .ForMember(c => c.EndpointAccessControlList, o => o.MapFrom(r => r.Rules == null ? null : r.Rules))
                  .ForMember(c => c.Vip, o => o.MapFrom(r => r.VirtualIPAddress));
            Mapper.CreateMap<IList<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>, PVM.LoadBalancedEndpointList>();
            Mapper.CreateMap<List<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>, PVM.LoadBalancedEndpointList>();

            //Common mapping
            Mapper.CreateMap<OperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<OperationResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.RequestId))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.StatusCode.ToString()));

            //AffinityGroup mapping
            Mapper.CreateMap<AffinityGroupGetResponse, AffinityGroupContext>();
            Mapper.CreateMap<AffinityGroupListResponse.AffinityGroup, AffinityGroupContext>();
            Mapper.CreateMap<AffinityGroupGetResponse.HostedServiceReference, AffinityGroupContext.Service>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<AffinityGroupGetResponse.StorageServiceReference, AffinityGroupContext.Service>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<OperationStatusResponse, AffinityGroupContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Location mapping
            Mapper.CreateMap<LocationsListResponse.Location, LocationsContext>();
            Mapper.CreateMap<OperationStatusResponse, LocationsContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));
            
            //ServiceCertificate mapping
            Mapper.CreateMap<ServiceCertificateGetResponse, CertificateContext>()
                  .ForMember(c => c.Data, o => o.MapFrom(r => r.Data != null ? Convert.ToBase64String(r.Data) : null));
            Mapper.CreateMap<ServiceCertificateListResponse.Certificate, CertificateContext>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.CertificateUri))
                  .ForMember(c => c.Data, o => o.MapFrom(r => r.Data != null ? Convert.ToBase64String(r.Data) : null));
            Mapper.CreateMap<OperationStatusResponse, CertificateContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));
            Mapper.CreateMap<ComputeOperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //OperatingSystems mapping
            Mapper.CreateMap<OperatingSystemListResponse.OperatingSystem, OSVersionsContext>();
            Mapper.CreateMap<OperationStatusResponse, OSVersionsContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Service mapping
            Mapper.CreateMap<HostedServiceGetResponse, HostedServiceDetailedContext>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<HostedServiceProperties, HostedServiceDetailedContext>()
                  .ForMember(c => c.Description, o => o.MapFrom(r => string.IsNullOrEmpty(r.Description) ? null : r.Description));
            Mapper.CreateMap<HostedServiceListResponse.HostedService, HostedServiceDetailedContext>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<OperationStatusResponse, HostedServiceDetailedContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Disk mapping
            Mapper.CreateMap<VirtualMachineDiskListResponse.VirtualMachineDisk, DiskContext>()
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.AttachedTo, o => o.MapFrom(r => r.UsageDetails));
            Mapper.CreateMap<VirtualMachineDiskListResponse.VirtualMachineDiskUsageDetails, DiskContext.RoleReference>();

            Mapper.CreateMap<VirtualMachineDiskGetDiskResponse, DiskContext>()
                  .ForMember(c => c.AttachedTo, o => o.MapFrom(r => r.UsageDetails))
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB))
                  .ForMember(c => c.IsCorrupted, o => o.MapFrom(r => r.IsCorrupted))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType));
            Mapper.CreateMap<VirtualMachineDiskGetDiskResponse.VirtualMachineDiskUsageDetails, DiskContext.RoleReference>();

            Mapper.CreateMap<VirtualMachineDiskCreateDiskResponse, DiskContext>()
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystem))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB))
                  .ForMember(c => c.AttachedTo, o => o.MapFrom(r => r.UsageDetails));
            Mapper.CreateMap<VirtualMachineDiskCreateDiskResponse.VirtualMachineDiskUsageDetails, DiskContext.RoleReference>();

            Mapper.CreateMap<VirtualMachineDiskUpdateDiskResponse, DiskContext>()
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB));

            Mapper.CreateMap<OperationStatusResponse, DiskContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Image mapping
            Mapper.CreateMap<VirtualMachineImageListResponse.VirtualMachineImage, OSImageContext>()
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => new DateTime?(r.PublishedDate)))
                  .ForMember(c => c.IconUri, o => o.MapFrom(r => r.SmallIconUri))
                  .ForMember(c => c.LogicalSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB));

            Mapper.CreateMap<VirtualMachineImageGetResponse, OSImageContext>()
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => new DateTime?(r.PublishedDate)))
                  .ForMember(c => c.LogicalSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB));

            Mapper.CreateMap<VirtualMachineImageCreateResponse, OSImageContext>()
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.IconUri, o => o.MapFrom(r => r.SmallIconUri))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => r.PublishedDate))
                  .ForMember(c => c.LogicalSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB));
            
            Mapper.CreateMap<VirtualMachineImageUpdateResponse, OSImageContext>()
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.IconUri, o => o.MapFrom(r => r.SmallIconUri))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => r.PublishedDate))
                  .ForMember(c => c.LogicalSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB));

            Mapper.CreateMap<OperationStatusResponse, OSImageContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<VirtualMachineDiskCreateDiskResponse, OSImageContext>()
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystem));

            //Storage mapping
            Mapper.CreateMap<StorageServiceGetResponse, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountDescription, o => o.MapFrom(r => r.Properties == null ? null : r.Properties.Description))
                  .ForMember(c => c.StorageAccountName, o => o.MapFrom(r => r.ServiceName));
            Mapper.CreateMap<StorageServiceProperties, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountDescription, o => o.MapFrom(r => r.Description))
                  .ForMember(c => c.GeoPrimaryLocation, o => o.MapFrom(r => r.GeoPrimaryRegion))
                  .ForMember(c => c.GeoSecondaryLocation, o => o.MapFrom(r => r.GeoSecondaryRegion))
                  .ForMember(c => c.StorageAccountStatus, o => o.MapFrom(r => r.Status))
                  .ForMember(c => c.StatusOfPrimary, o => o.MapFrom(r => r.StatusOfGeoPrimaryRegion))
                  .ForMember(c => c.StatusOfSecondary, o => o.MapFrom(r => r.StatusOfGeoSecondaryRegion));
            Mapper.CreateMap<StorageServiceListResponse.StorageService, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountDescription, o => o.MapFrom(r => r.Properties == null ? null : r.Properties.Description))
                  .ForMember(c => c.StorageAccountName, o => o.MapFrom(r => r.ServiceName));
            Mapper.CreateMap<OperationStatusResponse, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<StorageAccountGetKeysResponse, StorageServiceKeyOperationContext>()
                  .ForMember(c => c.Primary, o => o.MapFrom(r => r.PrimaryKey))
                  .ForMember(c => c.Secondary, o => o.MapFrom(r => r.SecondaryKey));
            Mapper.CreateMap<OperationStatusResponse, StorageServiceKeyOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<StorageOperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            // DomainJoinSettings mapping for IaaS
            Mapper.CreateMap<NSM.DomainJoinCredentials, PVM.WindowsProvisioningConfigurationSet.DomainJoinCredentials>()
                  .ForMember(c => c.Domain, o => o.MapFrom(r => r.Domain))
                  .ForMember(c => c.Username, o => o.MapFrom(r => r.UserName))
                  .ForMember(c => c.Password, o => o.MapFrom(r => r.Password));
            Mapper.CreateMap<NSM.DomainJoinProvisioning, PVM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning>()
                  .ForMember(c => c.AccountData, o => o.MapFrom(r => r.AccountData));
            Mapper.CreateMap<NSM.DomainJoinSettings, PVM.WindowsProvisioningConfigurationSet.DomainJoinSettings>()
                  .ForMember(c => c.Credentials, o => o.MapFrom(r => r.Credentials))
                  .ForMember(c => c.JoinDomain, o => o.MapFrom(r => r.DomainToJoin))
                  .ForMember(c => c.MachineObjectOU, o => o.MapFrom(r => r.LdapMachineObjectOU))
                  .ForMember(c => c.Provisioning, o => o.MapFrom(r => r.Provisioning));

            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinCredentials, NSM.DomainJoinCredentials>()
                  .ForMember(c => c.Domain, o => o.MapFrom(r => r.Domain))
                  .ForMember(c => c.UserName, o => o.MapFrom(r => r.Username))
                  .ForMember(c => c.Password, o => o.MapFrom(r => r.Password));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning, NSM.DomainJoinProvisioning>()
                  .ForMember(c => c.AccountData, o => o.MapFrom(r => r.AccountData));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinSettings, NSM.DomainJoinSettings>()
                  .ForMember(c => c.Credentials, o => o.MapFrom(r => r.Credentials))
                  .ForMember(c => c.DomainToJoin, o => o.MapFrom(r => r.JoinDomain))
                  .ForMember(c => c.LdapMachineObjectOU, o => o.MapFrom(r => r.MachineObjectOU))
                  .ForMember(c => c.Provisioning, o => o.MapFrom(r => r.Provisioning));

            // New SM to Model
            Mapper.CreateMap<IList<NSM.StoredCertificateSettings>, PVM.CertificateSettingList>();
            Mapper.CreateMap<NSM.StoredCertificateSettings, PVM.CertificateSetting>();

            // Model to New SM
            Mapper.CreateMap<PVM.CertificateSettingList, IList<NSM.StoredCertificateSettings>>();
            Mapper.CreateMap<PVM.CertificateSetting, NSM.StoredCertificateSettings>();

            // Resource Reference Mapping
            Mapper.CreateMap<NSM.ResourceExtensionParameterValue, PVM.ResourceExtensionParameterValue>();
            Mapper.CreateMap<IList<NSM.ResourceExtensionParameterValue>, PVM.ResourceExtensionParameterValueList>();
            Mapper.CreateMap<IEnumerable<NSM.ResourceExtensionParameterValue>, PVM.ResourceExtensionParameterValueList>();
            Mapper.CreateMap<List<NSM.ResourceExtensionParameterValue>, PVM.ResourceExtensionParameterValueList>();
            Mapper.CreateMap<NSM.ResourceExtensionReference, PVM.ResourceExtensionReference>()
                  .ForMember(c => c.ResourceExtensionParameterValues, o => o.MapFrom(
                      r => new PVM.ResourceExtensionParameterValueList(r.ResourceExtensionParameterValues.Select(
                          p => Mapper.Map<PVM.ResourceExtensionParameterValue>(p)))));
            Mapper.CreateMap<IList<NSM.ResourceExtensionReference>, List<PVM.ResourceExtensionReference>>()
                  .Include<IList<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();
            Mapper.CreateMap<IEnumerable<NSM.ResourceExtensionReference>, List<PVM.ResourceExtensionReference>>()
                  .Include<IEnumerable<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();
            Mapper.CreateMap<List<NSM.ResourceExtensionReference>, List<PVM.ResourceExtensionReference>>()
                  .Include<List<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();
            Mapper.CreateMap<IList<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();
            Mapper.CreateMap<IEnumerable<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();
            Mapper.CreateMap<List<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();

            Mapper.CreateMap<PVM.ResourceExtensionParameterValue, NSM.ResourceExtensionParameterValue>();
            Mapper.CreateMap<PVM.ResourceExtensionParameterValueList, IList<NSM.ResourceExtensionParameterValue>>();
            Mapper.CreateMap<PVM.ResourceExtensionParameterValueList, IEnumerable<NSM.ResourceExtensionParameterValue>>();
            Mapper.CreateMap<PVM.ResourceExtensionParameterValueList, List<NSM.ResourceExtensionParameterValue>>();
            Mapper.CreateMap<PVM.ResourceExtensionReference, NSM.ResourceExtensionReference>()
                  .ForMember(c => c.ResourceExtensionParameterValues, o => o.MapFrom(
                      r => new List<NSM.ResourceExtensionParameterValue>(r.ResourceExtensionParameterValues.Select(
                          p => Mapper.Map<NSM.ResourceExtensionParameterValue>(p)))));
            Mapper.CreateMap<PVM.ResourceExtensionReference, NSM.ResourceExtensionReference>();
            Mapper.CreateMap<List<PVM.ResourceExtensionReference>, IList<NSM.ResourceExtensionReference>>()
                  .Include<PVM.ResourceExtensionReferenceList, IList<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<List<PVM.ResourceExtensionReference>, IEnumerable<NSM.ResourceExtensionReference>>()
                  .Include<PVM.ResourceExtensionReferenceList, IEnumerable<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<List<PVM.ResourceExtensionReference>, List<NSM.ResourceExtensionReference>>()
                  .Include<PVM.ResourceExtensionReferenceList, List<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<PVM.ResourceExtensionReferenceList, IList<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<PVM.ResourceExtensionReferenceList, IEnumerable<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<PVM.ResourceExtensionReferenceList, List<NSM.ResourceExtensionReference>>();

            // SM to Model
            Mapper.CreateMap<WindowsAzure.ServiceManagement.LoadBalancerProbe,                                           PVM.LoadBalancerProbe>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.LoadBalancedEndpointList,                                    PVM.EndpointAccessControlList>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.InputEndpoint,                                               PVM.InputEndpoint>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.InstanceEndpoint,                                            PVM.InstanceEndpoint>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.InstanceEndpointList,                                        PVM.InstanceEndpointList>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.DataVirtualHardDisk,                                         PVM.DataVirtualHardDisk>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.OSVirtualHardDisk,                                           PVM.OSVirtualHardDisk>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.CertificateFile,                                             PVM.CertificateFile>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.CertificateSetting,                                          PVM.CertificateSetting>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.CertificateSettingList,                                      PVM.CertificateSettingList>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.DomainJoinCredentials,   PVM.WindowsProvisioningConfigurationSet.DomainJoinCredentials>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.DomainJoinProvisioning,  PVM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.DomainJoinSettings,      PVM.WindowsProvisioningConfigurationSet.DomainJoinSettings>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.WinRmProtocol,           PVM.WindowsProvisioningConfigurationSet.WinRmProtocol>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.WinRmListenerProperties, PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.WinRmListenerCollection, PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.WinRmConfiguration,      PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHKeyPair,                PVM.LinuxProvisioningConfigurationSet.SSHKeyPair>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHKeyPairList,            PVM.LinuxProvisioningConfigurationSet.SSHKeyPairList>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHPublicKey,              PVM.LinuxProvisioningConfigurationSet.SSHPublicKey>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHPublicKeyList,          PVM.LinuxProvisioningConfigurationSet.SSHPublicKeyList>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHSettings,               PVM.LinuxProvisioningConfigurationSet.SSHSettings>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.NetworkConfigurationSet,                                     PVM.NetworkConfigurationSet>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet,                         PVM.WindowsProvisioningConfigurationSet>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet,                           PVM.LinuxProvisioningConfigurationSet>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.ProvisioningConfigurationSet,                                PVM.ProvisioningConfigurationSet>();
            Mapper.CreateMap<WindowsAzure.ServiceManagement.ConfigurationSet,                                            PVM.ConfigurationSet>();

            // Model to SM
            Mapper.CreateMap<PVM.LoadBalancerProbe,                                           WindowsAzure.ServiceManagement.LoadBalancerProbe>();
            Mapper.CreateMap<PVM.LoadBalancedEndpointList,                                    WindowsAzure.ServiceManagement.EndpointAccessControlList>();
            Mapper.CreateMap<PVM.InputEndpoint,                                               WindowsAzure.ServiceManagement.InputEndpoint>();
            Mapper.CreateMap<PVM.InstanceEndpoint,                                            WindowsAzure.ServiceManagement.InstanceEndpoint>();
            Mapper.CreateMap<PVM.InstanceEndpointList,                                        WindowsAzure.ServiceManagement.InstanceEndpointList>();
            Mapper.CreateMap<PVM.DataVirtualHardDisk,                                         WindowsAzure.ServiceManagement.DataVirtualHardDisk>();
            Mapper.CreateMap<PVM.OSVirtualHardDisk,                                           WindowsAzure.ServiceManagement.OSVirtualHardDisk>();
            Mapper.CreateMap<PVM.CertificateFile,                                             WindowsAzure.ServiceManagement.CertificateFile>();
            Mapper.CreateMap<PVM.CertificateSetting,                                          WindowsAzure.ServiceManagement.CertificateSetting>();
            Mapper.CreateMap<PVM.CertificateSettingList,                                      WindowsAzure.ServiceManagement.CertificateSettingList>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinCredentials,   WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.DomainJoinCredentials>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning,  WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.DomainJoinProvisioning>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinSettings,      WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.DomainJoinSettings>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmProtocol,           WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.WinRmProtocol>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties, WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.WinRmListenerProperties>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection, WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.WinRmListenerCollection>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration,      WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet.WinRmConfiguration>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHKeyPair,                WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHKeyPair>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHKeyPairList,            WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHKeyPairList>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHPublicKey,              WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHPublicKey>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHPublicKeyList,          WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHPublicKeyList>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHSettings,               WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet.SSHSettings>();
            Mapper.CreateMap<PVM.NetworkConfigurationSet,                                     WindowsAzure.ServiceManagement.NetworkConfigurationSet>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet,                         WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet,                           WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet>();
            Mapper.CreateMap<PVM.ProvisioningConfigurationSet,                                WindowsAzure.ServiceManagement.ProvisioningConfigurationSet>();
            Mapper.CreateMap<PVM.ConfigurationSet,                                            WindowsAzure.ServiceManagement.ConfigurationSet>();
        }
    }
}