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

namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System;
    using System.IO;

    public static class ServiceManagementClientExtensions
    {
        // This file is used for backwards compatibility with tests after the synchronous Service Management operations were moved off of the interface to a separate class.
        // New tests should use the full ServiceManagementClient when possible

        #region Affinity Groups

        public static void CreateAffinityGroup(this IServiceManagement proxy, string subscriptionId, CreateAffinityGroupInput input)
        {
            proxy.EndCreateAffinityGroup(proxy.BeginCreateAffinityGroup(subscriptionId, input, null, null));
        }

        public static void DeleteAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName)
        {
            proxy.EndDeleteAffinityGroup(proxy.BeginDeleteAffinityGroup(subscriptionId, affinityGroupName, null, null));
        }

        public static void UpdateAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName, UpdateAffinityGroupInput input)
        {
            proxy.EndUpdateAffinityGroup(proxy.BeginUpdateAffinityGroup(subscriptionId, affinityGroupName, input, null, null));
        }

        public static AffinityGroupList ListAffinityGroups(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListAffinityGroups(proxy.BeginListAffinityGroups(subscriptionId, null, null));
        }

        public static AffinityGroup GetAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName)
        {
            return proxy.EndGetAffinityGroup(proxy.BeginGetAffinityGroup(subscriptionId, affinityGroupName, null, null));
        }

        #endregion

        #region Certificates

        public static CertificateList ListCertificates(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            return proxy.EndListCertificates(proxy.BeginListCertificates(subscriptionId, serviceName, null, null));
        }

        public static Certificate GetCertificate(this IServiceManagement proxy, string subscriptionId, string serviceName, string algorithm, string thumbprint)
        {
            return proxy.EndGetCertificate(proxy.BeginGetCertificate(subscriptionId, serviceName, algorithm, thumbprint, null, null));
        }

        public static void AddCertificates(this IServiceManagement proxy, string subscriptionId, string serviceName, CertificateFile input)
        {
            proxy.EndAddCertificates(proxy.BeginAddCertificates(subscriptionId, serviceName, input, null, null));
        }

        public static void DeleteCertificate(this IServiceManagement proxy, string subscriptionId, string serviceName, string algorithm, string thumbprint)
        {
            proxy.EndDeleteCertificate(proxy.BeginDeleteCertificate(subscriptionId, serviceName, algorithm, thumbprint, null, null));
        }

        #endregion

        #region Deployment

        public static void SwapDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, SwapDeploymentInput input)
        {
            proxy.EndSwapDeployment(proxy.BeginSwapDeployment(subscriptionId, serviceName, input, null, null));
        }

        public static void CreateDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName,
            Deployment deployment)
        {
            proxy.EndCreateDeployment(proxy.BeginCreateDeployment(subscriptionId, serviceName, deployment, null, null));
        }

        public static void CreateOrUpdateDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, CreateDeploymentInput input)
        {
            proxy.EndCreateOrUpdateDeployment(proxy.BeginCreateOrUpdateDeployment(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void DeleteDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
        {
            proxy.EndDeleteDeployment(proxy.BeginDeleteDeployment(subscriptionId, serviceName, deploymentName, null, null));
        }

        public static void DeleteDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
        {
            proxy.EndDeleteDeploymentBySlot(proxy.BeginDeleteDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
        }

        public static Deployment GetDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
        {
            return proxy.EndGetDeployment(proxy.BeginGetDeployment(subscriptionId, serviceName, deploymentName, null, null));
        }

        public static Deployment GetDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
        {
            return proxy.EndGetDeploymentBySlot(proxy.BeginGetDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
        }

        public static void SuspendDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
        {
            proxy.EndSuspendDeploymentUpdateOrUpgrade(proxy.BeginSuspendDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, null, null));
        }

        public static void SuspendDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
        {
            proxy.EndSuspendDeploymentUpdateOrUpgradeBySlot(proxy.BeginSuspendDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
        }

        public static void ResumeDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
        {
            proxy.EndResumeDeploymentUpdateOrUpgrade(proxy.BeginResumeDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, null, null));
        }

        public static void ResumeDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
        {
            proxy.EndResumeDeploymentUpdateOrUpgradeBySlot(proxy.BeginResumeDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
        }

        public static void UpdateDeploymentStatus(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, UpdateDeploymentStatusInput input)
        {
            proxy.EndUpdateDeploymentStatus(proxy.BeginUpdateDeploymentStatus(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void UpdateDeploymentStatusBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, UpdateDeploymentStatusInput input)
        {
            proxy.EndUpdateDeploymentStatusBySlot(proxy.BeginUpdateDeploymentStatusBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void ChangeConfiguration(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, ChangeConfigurationInput input)
        {
            proxy.EndChangeConfiguration(proxy.BeginChangeConfiguration(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void ChangeConfigurationBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, ChangeConfigurationInput input)
        {
            proxy.EndChangeConfigurationBySlot(proxy.BeginChangeConfigurationBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void UpgradeDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, UpgradeDeploymentInput input)
        {
            proxy.EndUpgradeDeployment(proxy.BeginUpgradeDeployment(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void UpgradeDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, UpgradeDeploymentInput input)
        {
            proxy.EndUpgradeDeploymentBySlot(proxy.BeginUpgradeDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void WalkUpgradeDomain(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, WalkUpgradeDomainInput input)
        {
            proxy.EndWalkUpgradeDomain(proxy.BeginWalkUpgradeDomain(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void WalkUpgradeDomainBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, WalkUpgradeDomainInput input)
        {
            proxy.EndWalkUpgradeDomainBySlot(proxy.BeginWalkUpgradeDomainBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void RollbackDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, RollbackUpdateOrUpgradeInput input)
        {
            proxy.EndRollbackDeploymentUpdateOrUpgrade(proxy.BeginRollbackDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void RollbackDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string slotName, RollbackUpdateOrUpgradeInput input)
        {
            proxy.EndRollbackDeploymentUpdateOrUpgradeBySlot(proxy.BeginRollbackDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, slotName, input, null, null));
        }

        public static void RebootDeploymentRoleInstance(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
        {
            proxy.EndRebootDeploymentRoleInstance(proxy.BeginRebootDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
        }

        public static void ReimageDeploymentRoleInstance(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
        {
            proxy.EndReimageDeploymentRoleInstance(proxy.BeginReimageDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
        }

        public static void RebootDeploymentRoleInstanceBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName)
        {
            proxy.EndRebootDeploymentRoleInstanceBySlot(proxy.BeginRebootDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null));
        }

        public static void ReimageDeploymentRoleInstanceBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName)
        {
            proxy.EndReimageDeploymentRoleInstanceBySlot(proxy.BeginReimageDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null));
        }

        public static void GetPackage(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string containerUri, bool overwriteExisting)
        {
            proxy.EndGetPackage(proxy.BeginGetPackage(subscriptionId, serviceName, deploymentName, containerUri, overwriteExisting, null, null));
        }

        public static void GetPackageBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string containerUri, bool overwriteExisting)
        {
            proxy.EndGetPackageBySlot(proxy.BeginGetPackageBySlot(subscriptionId, serviceName, deploymentSlot, containerUri, overwriteExisting, null, null));
        }


        #endregion

        #region Disk

        public static DiskList ListDisks(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndListDisks(proxy.BeginListDisks(subscriptionID, null, null));
        }

        public static Disk CreateDisk(this IServiceManagement proxy, string subscriptionID, Disk disk)
        {
            return proxy.EndCreateDisk(proxy.BeginCreateDisk(subscriptionID, disk, null, null));
        }

        public static Disk UpdateDisk(this IServiceManagement proxy, string subscriptionID, string diskName, Disk disk)
        {
            return proxy.EndUpdateDisk(proxy.BeginUpdateDisk(subscriptionID, diskName, disk, null, null));
        }

        public static Disk GetDisk(this IServiceManagement proxy, string subscriptionID, string diskName)
        {
            return proxy.EndGetDisk(proxy.BeginGetDisk(subscriptionID, diskName, null, null));
        }

        public static void DeleteDisk(this IServiceManagement proxy, string subscriptionID, string diskName)
        {
            proxy.EndDeleteDisk(proxy.BeginDeleteDisk(subscriptionID, diskName, null, null));
        }

        public static void DeleteDiskEx(this IServiceManagement proxy, string subscriptionID, string diskName, string comp)
        {
            proxy.EndDeleteDiskEx(proxy.BeginDeleteDiskEx(subscriptionID, diskName, comp, null, null));
        }


        #endregion

        #region DurableVMRole
        public static void CaptureRole(this IServiceManagement proxy,
        string subscriptionId,
        string serviceName,
        string deploymentName,
        string roleInstanceName,
        string targetImageName,
        string targetImageLabel,
        PostCaptureAction postCaptureAction,
        ProvisioningConfigurationSet provisioningConfiguration)
        {
            proxy.EndExecuteRoleOperation(
                proxy.BeginExecuteRoleOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                roleInstanceName,
                new CaptureRoleOperation
                {
                    PostCaptureAction = postCaptureAction.ToString(),
                    ProvisioningConfiguration = provisioningConfiguration,
                    TargetImageName = targetImageName,
                    TargetImageLabel = targetImageLabel
                }, null, null));
        }

        public static void ShutdownRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleInstanceName,
            PostShutdownAction? shutdownAction)
        {
            proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                roleInstanceName,
                new ShutdownRoleOperation {PostShutdownAction = shutdownAction},
                null,
                null));
        }

        public static void ShutdownRoles(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            ShutdownRolesOperation shutdownRolesOperation)
        {
            proxy.EndExecuteRoleSetOperation(proxy.BeginExecuteRoleSetOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                shutdownRolesOperation,
                null,
                null));
        }

        public static void StartRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleInstanceName)
        {
            proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                roleInstanceName,
                new StartRoleOperation(),
                null,
                null));
        }

        public static void StartRoles(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            StartRolesOperation startRolesOperation)
        {
            proxy.EndExecuteRoleSetOperation(proxy.BeginExecuteRoleSetOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                startRolesOperation,
                null,
                null));
        }

        public static void RestartRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleInstanceName)
        {
            proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                roleInstanceName,
                new RestartRoleOperation(),
                null,
                null));
        }

        public static void AddDataDisk(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            DataVirtualHardDisk dataDisk)
        {
            proxy.EndAddDataDisk(proxy.BeginAddDataDisk(
                subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                dataDisk,
                null,
                null));
        }

        public static void UpdateDataDisk(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            int lun,
            DataVirtualHardDisk dataDisk)
        {
            proxy.EndUpdateDataDisk(
                proxy.BeginUpdateDataDisk(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                lun.ToString(),
                dataDisk,
                null,
                null));
        }

        public static void DeleteDataDisk(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            int lun)
        {
            proxy.EndDeleteDataDisk(
                proxy.BeginDeleteDataDisk(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                lun.ToString(),
                null,
                null));
        }

        public static DataVirtualHardDisk GetDataDisk(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            int lun)
        {
            return proxy.EndGetDataDisk(
                proxy.BeginGetDataDisk(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                lun.ToString(),
                null,
                null));
        }

        public static void AddRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            PersistentVMRole role)
        {
            proxy.EndAddRole(
                proxy.BeginAddRole(subscriptionId,
                serviceName,
                deploymentName,
                role,
                null,
                null));
        }

        public static void UpdateRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            PersistentVMRole role)
        {
            proxy.EndUpdateRole(
                proxy.BeginUpdateRole(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                role,
                null,
                null));
        }

        public static void UpdateLoadBalancedEndpointSet(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            LoadBalancedEndpointList loadBalancedEndpointList)
        {
            proxy.EndUpdateLoadBalancedEndpointSet(
                proxy.BeginUpdateLoadBalancedEndpointSet(
                    subscriptionId,
                    serviceName,
                    deploymentName,
                    loadBalancedEndpointList,
                    null,
                    null));
        }

        public static PersistentVMRole GetRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName)
        {
            return (PersistentVMRole)proxy.EndGetRole(
                proxy.BeginGetRole(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                null,
                null));
        }

        public static void DeleteRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName)
        {
            proxy.EndDeleteRole(
                proxy.BeginDeleteRole(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                null,
                null));
        }

        public static Stream DownloadRDPFile(this IServiceManagement proxy,
            string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleInstanceName)
        {
            return proxy.EndDownloadRDPFile(
                proxy.BeginDownloadRDPFile(
                subscriptionID,
                serviceName,
                deploymentName,
                roleInstanceName,
                null,
                null));
        }
        #endregion

        #region Hosted Services

        public static void CreateHostedService(this IServiceManagement proxy, string subscriptionId, CreateHostedServiceInput input)
        {
            proxy.EndCreateHostedService(proxy.BeginCreateHostedService(subscriptionId, input, null, null));
        }

        public static void UpdateHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName, UpdateHostedServiceInput input)
        {
            proxy.EndUpdateHostedService(proxy.BeginUpdateHostedService(subscriptionId, serviceName, input, null, null));
        }

        public static void DeleteHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            proxy.EndDeleteHostedService(proxy.BeginDeleteHostedService(subscriptionId, serviceName, null, null));
        }

        public static HostedServiceList ListHostedServices(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListHostedServices(proxy.BeginListHostedServices(subscriptionId, null, null));
        }

        public static HostedService GetHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            return proxy.EndGetHostedService(proxy.BeginGetHostedService(subscriptionId, serviceName, null, null));
        }

        public static HostedService GetHostedServiceWithDetails(this IServiceManagement proxy, string subscriptionId, string serviceName, bool embedDetail)
        {
            return proxy.EndGetHostedServiceWithDetails(proxy.BeginGetHostedServiceWithDetails(subscriptionId, serviceName, embedDetail, null, null));
        }

        public static LocationList ListLocations(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListLocations(proxy.BeginListLocations(subscriptionId, null, null));
        }

        public static AvailabilityResponse IsDNSAvailable(this IServiceManagement proxy, string subscriptionID, string dnsname)
        {
            return proxy.EndIsDNSAvailable(proxy.BeginIsDNSAvailable(subscriptionID, dnsname, null, null));
        }


        #endregion
        
        #region Networking

        public static void SetNetworkConfiguration(this IServiceManagement proxy, string subscriptionID, Stream networkConfiguration)
        {
            proxy.EndSetNetworkConfiguration(proxy.BeginSetNetworkConfiguration(subscriptionID, networkConfiguration, null, null));
        }

        public static Stream GetNetworkConfiguration(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndGetNetworkConfiguration(proxy.BeginGetNetworkConfiguration(subscriptionID, null, null));
        }

        public static VirtualNetworkSiteList ListVirtualNetworkSites(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndListVirtualNetworkSites(proxy.BeginListVirtualNetworkSites(subscriptionID, null, null));
        }

        #endregion

        #region Operating System

        public static OperatingSystemList ListOperatingSystems(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListOperatingSystems(proxy.BeginListOperatingSystems(subscriptionId, null, null));
        }

        public static OperatingSystemFamilyList ListOperatingSystemFamilies(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListOperatingSystemFamilies(proxy.BeginListOperatingSystemFamilies(subscriptionId, null, null));
        }


        #endregion

        #region Operation Tracking

        public static Operation GetOperationStatus(this IServiceManagement proxy, string subscriptionId, string operationId)
        {
            return proxy.EndGetOperationStatus(proxy.BeginGetOperationStatus(subscriptionId, operationId, null, null));
        }

        #endregion

        #region OSImage

        public static OSImageList ListOSImages(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndListOSImages(proxy.BeginListOSImages(subscriptionID, null, null));
        }

        public static OSImage CreateOSImage(this IServiceManagement proxy, string subscriptionID, OSImage image)
        {
            return proxy.EndCreateOSImage(proxy.BeginCreateOSImage(subscriptionID, image, null, null));
        }

        public static OSImage UpdateOSImage(this IServiceManagement proxy, string subscriptionID, string imageName, OSImage image)
        {
            return proxy.EndUpdateOSImage(proxy.BeginUpdateOSImage(subscriptionID, imageName, image, null, null));
        }

        public static OSImage GetOSImage(this IServiceManagement proxy, string subscriptionID, string imageName)
        {
            return proxy.EndGetOSImage(proxy.BeginGetOSImage(subscriptionID, imageName, null, null));
        }

        public static void DeleteOSImage(this IServiceManagement proxy, string subscriptionID, string imageName)
        {
            proxy.EndDeleteOSImage(proxy.BeginDeleteOSImage(subscriptionID, imageName, null, null));
        }

        public static void DeleteOSImageEx(this IServiceManagement proxy, string subscriptionID, string imageName, string comp)
        {
            proxy.EndDeleteOSImageEx(proxy.BeginDeleteOSImageEx(subscriptionID, imageName, comp, null, null));
        }

        public static string ReplicateOSImage(this IServiceManagement proxy, string subscriptionId, string imageName, ReplicationInput replicationInput)
        {
            return proxy.EndReplicateOSImage(proxy.BeginReplicateOSImage(subscriptionId, imageName, replicationInput, null, null));
        }

        public static void UnReplicateOSImage(this IServiceManagement proxy, string subscriptionId, string imageName)
        {
            proxy.EndUnReplicateOSImage(proxy.BeginUnReplicateOSImage(subscriptionId, imageName, null, null));
        }

        public static bool ShareOSImage(this IServiceManagement proxy, string subscriptionId, string imageName, string permission)
        {
            return proxy.EndShareOSImage(proxy.BeginShareOSImage(subscriptionId, imageName, permission, null, null));
        }

        public static OSImageDetails GetOSImageWithDetails(this IServiceManagement proxy, string subscriptionId, string imageName)
        {
            return proxy.EndGetOSImageWithDetails(proxy.BeginGetOSImageWithDetails(subscriptionId, imageName, null, null));
        }

        public static OSImageList QueryOSImages(this IServiceManagement proxy, string subscriptionId, string publisher, string location)
        {
            return proxy.EndQueryOSImages(proxy.BeginQueryOSImages(subscriptionId, publisher, location, null, null));
        }
        #endregion

        #region Storage Service

        public static void CreateStorageService(this IServiceManagement proxy, string subscriptionId, CreateStorageServiceInput input)
        {
            proxy.EndCreateStorageService(proxy.BeginCreateStorageService(subscriptionId, input, null, null));
        }

        public static void UpdateStorageService(this IServiceManagement proxy, string subscriptionId, string serviceName, UpdateStorageServiceInput input)
        {
            proxy.EndUpdateStorageService(proxy.BeginUpdateStorageService(subscriptionId, serviceName, input, null, null));
        }

        public static void DeleteStorageService(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            proxy.EndDeleteStorageService(proxy.BeginDeleteStorageService(subscriptionId, serviceName, null, null));
        }

        public static StorageServiceList ListStorageServices(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListStorageServices(proxy.BeginListStorageServices(subscriptionId, null, null));
        }

        public static StorageService GetStorageService(this IServiceManagement proxy, string subscriptionId, string name)
        {
            return proxy.EndGetStorageService(proxy.BeginGetStorageService(subscriptionId, name, null, null));
        }

        public static StorageService GetStorageKeys(this IServiceManagement proxy, string subscriptionId, string name)
        {
            return proxy.EndGetStorageKeys(proxy.BeginGetStorageKeys(subscriptionId, name, null, null));
        }

        public static StorageService RegenerateStorageServiceKeys(this IServiceManagement proxy, string subscriptionId, string name, RegenerateKeys regenerateKeys)
        {
            return proxy.EndRegenerateStorageServiceKeys(proxy.BeginRegenerateStorageServiceKeys(subscriptionId, name, regenerateKeys, null, null));
        }

        public static AvailabilityResponse IsStorageServiceAvailable(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            return proxy.EndIsStorageServiceAvailable(proxy.BeginIsStorageServiceAvailable(subscriptionId, serviceName, null, null));
        }


        #endregion

        #region Subscription

        public static Subscription GetSubscription(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndGetSubscription(proxy.BeginGetSubscription(subscriptionID, null, null));
        }

        public static SubscriptionOperationCollection ListSubscriptionOperations(this IServiceManagement proxy, string subscriptionID, string startTime, string endTime, string objectIdFilter, string operationResultFilter, string continuationToken)
        {
            return proxy.EndListSubscriptionOperations(proxy.BeginListSubscriptionOperations(subscriptionID, startTime, endTime, objectIdFilter, operationResultFilter, continuationToken, null, null));
        }

        #endregion

        #region Extensions
        public static void AddHostedServiceExtension(this IServiceManagement proxy, string subscriptionId, string serviceName, HostedServiceExtensionInput extension)
        {
            proxy.EndAddHostedServiceExtension(proxy.BeginAddHostedServiceExtension(subscriptionId, serviceName, extension, null, null));
        }

        public static HostedServiceExtensionList ListHostedServiceExtensions(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            return proxy.EndListHostedServiceExtensions(proxy.BeginListHostedServiceExtensions(subscriptionId, serviceName, null, null));
        }

        public static HostedServiceExtension GetHostedServiceExtension(this IServiceManagement proxy, string subscriptionId, string serviceName, string extensionId)
        {
            return proxy.EndGetHostedServiceExtension(proxy.BeginGetHostedServiceExtension(subscriptionId, serviceName, extensionId, null, null));
        }

        public static void DeleteHostedServiceExtension(this IServiceManagement proxy, string subscriptionId, string serviceName, string extensionId)
        {
            proxy.EndDeleteHostedServiceExtension(proxy.BeginDeleteHostedServiceExtension(subscriptionId, serviceName, extensionId, null, null));
        }

        public static ExtensionImageList ListLatestExtensions(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListLatestExtensions(proxy.BeginListLatestExtensions(subscriptionId, null, null));
        }

        #endregion
    }
}