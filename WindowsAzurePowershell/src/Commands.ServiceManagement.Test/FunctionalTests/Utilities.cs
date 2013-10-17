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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests
{
    using Model;
    using Security.Cryptography;
    using Security.Cryptography.X509Certificates;
    using WindowsAzure.Storage.Auth;
    using WindowsAzure.Storage.Blob;
    using Sync.Download;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;
    using VisualStudio.TestTools.UnitTesting;

    internal class Utilities 
    {
        public static string windowsAzurePowershellPath = Path.Combine(Environment.CurrentDirectory);
        public const string windowsAzurePowershellModuleStorage = "Microsoft.WindowsAzure.Commands.Storage.dll";
        public const string windowsAzurePowershellModuleManagement = "Microsoft.WindowsAzure.Commands.dll";
        public const string windowsAzurePowershellModuleService = "Microsoft.WindowsAzure.Commands.Service.dll";
        public const string windowsAzurePowershellModuleServiceManagement = "Microsoft.WindowsAzure.Commands.ServiceManagement.dll";
        public const string windowsAzurePowershellModuleServiceManagementPlatformImageRepository =
            "Microsoft.WindowsAzure.Commands.ServiceManagement.PlatformImageRepository.dll";

        private const string tclientPath = "tclient.dll";
        private const string clxtsharPath = "clxtshar.dll";
        private const string RDPTestPath = "RDPTest.exe";

        // AzureAffinityGroup
        public const string NewAzureAffinityGroupCmdletName = "New-AzureAffinityGroup";
        public const string GetAzureAffinityGroupCmdletName = "Get-AzureAffinityGroup";
        public const string SetAzureAffinityGroupCmdletName = "Set-AzureAffinityGroup";
        public const string RemoveAzureAffinityGroupCmdletName = "Remove-AzureAffinityGroup";

        // AzureAvailablitySet
        public const string SetAzureAvailabilitySetCmdletName = "Set-AzureAvailabilitySet";

        // AzureCertificate & AzureCertificateSetting
        public const string AddAzureCertificateCmdletName = "Add-AzureCertificate";
        public const string GetAzureCertificateCmdletName = "Get-AzureCertificate";
        public const string RemoveAzureCertificateCmdletName = "Remove-AzureCertificate";
        public const string NewAzureCertificateSettingCmdletName = "New-AzureCertificateSetting";


        // AzureDataDisk
        public const string AddAzureDataDiskCmdletName = "Add-AzureDataDisk";
        public const string GetAzureDataDiskCmdletName = "Get-AzureDataDisk";
        public const string SetAzureDataDiskCmdletName = "Set-AzureDataDisk";        
        public const string RemoveAzureDataDiskCmdletName = "Remove-AzureDataDisk";

        // AzureDeployment
        public const string NewAzureDeploymentCmdletName = "New-AzureDeployment";
        public const string GetAzureDeploymentCmdletName = "Get-AzureDeployment";
        public const string SetAzureDeploymentCmdletName = "Set-AzureDeployment";
        public const string RemoveAzureDeploymentCmdletName = "Remove-AzureDeployment";
        public const string MoveAzureDeploymentCmdletName = "Move-AzureDeployment";

        // AzureDisk        
        public const string AddAzureDiskCmdletName = "Add-AzureDisk";
        public const string GetAzureDiskCmdletName = "Get-AzureDisk";
        public const string UpdateAzureDiskCmdletName = "Update-AzureDisk";
        public const string RemoveAzureDiskCmdletName = "Remove-AzureDisk";


        // AzureDns
        public const string NewAzureDnsCmdletName = "New-AzureDns";
        public const string GetAzureDnsCmdletName = "Get-AzureDns";

        // AzureEndpoint
        public const string AddAzureEndpointCmdletName = "Add-AzureEndpoint";        
        public const string GetAzureEndpointCmdletName = "Get-AzureEndpoint";
        public const string SetAzureEndpointCmdletName = "Set-AzureEndpoint";
        public const string RemoveAzureEndpointCmdletName = "Remove-AzureEndpoint";



        // AzureLocation
        public const string GetAzureLocationCmdletName = "Get-AzureLocation";
        

        // AzureOSDisk & AzureOSVersion
        public const string GetAzureOSDiskCmdletName = "Get-AzureOSDisk";
        public const string SetAzureOSDiskCmdletName = "Set-AzureOSDisk";

        public const string GetAzureOSVersionCmdletName = "Get-AzureOSVersion";

        // AzureProvisioningConfig
        public const string AddAzureProvisioningConfigCmdletName = "Add-AzureProvisioningConfig";

        // AzurePublishSettingsFile
        public const string ImportAzurePublishSettingsFileCmdletName = "Import-AzurePublishSettingsFile";
        public const string GetAzurePublishSettingsFileCmdletName = "Get-AzurePublishSettingsFile";

        // AzureQuickVM
        public const string NewAzureQuickVMCmdletName = "New-AzureQuickVM";

        // AzurePlatformVMImage
        public const string SetAzurePlatformVMImageCmdletName = "Set-AzurePlatformVMImage";
        public const string GetAzurePlatformVMImageCmdletName = "Get-AzurePlatformVMImage";
        public const string RemoveAzurePlatformVMImageCmdletName = "Remove-AzurePlatformVMImage";

        // AzureRemoteDesktopFile
        public const string GetAzureRemoteDesktopFileCmdletName = "Get-AzureRemoteDesktopFile";
        


        // AzureRole & AzureRoleInstnace
        public const string GetAzureRoleCmdletName = "Get-AzureRole";
        public const string SetAzureRoleCmdletName = "Set-AzureRole";

        public const string GetAzureRoleInstanceCmdletName = "Get-AzureRoleInstance";

        // AzureService
        public const string NewAzureServiceCmdletName = "New-AzureService";
        public const string GetAzureServiceCmdletName = "Get-AzureService";
        public const string SetAzureServiceCmdletName = "Set-AzureService";
        public const string RemoveAzureServiceCmdletName = "Remove-AzureService";

        // AzureServiceRemoteDesktopExtension
        public const string NewAzureServiceRemoteDesktopExtensionConfigCmdletName = "New-AzureServiceRemoteDesktopExtensionConfig";
        public const string SetAzureServiceRemoteDesktopExtensionCmdletName = "Set-AzureServiceRemoteDesktopExtension";
        public const string GetAzureServiceRemoteDesktopExtensionCmdletName = "Get-AzureServiceRemoteDesktopExtension";
        public const string RemoveAzureServiceRemoteDesktopExtensionCmdletName = "Remove-AzureServiceRemoteDesktopExtension";

        // AzureServiceDiagnosticExtension
        public const string NewAzureServiceDiagnosticsExtensionConfigCmdletName = "New-AzureServiceDiagnosticsExtensionConfig";
        public const string SetAzureServiceDiagnosticsExtensionCmdletName = "Set-AzureServiceDiagnosticsExtension";
        public const string GetAzureServiceDiagnosticsExtensionCmdletName = "Get-AzureServiceDiagnosticsExtension";
        public const string RemoveAzureServiceDiagnosticsExtensionCmdletName = "Remove-AzureServiceDiagnosticsExtension";
        
        // AzureSSHKey
        public const string NewAzureSSHKeyCmdletName = "New-AzureSSHKey";

        // AzureStorageAccount
        public const string NewAzureStorageAccountCmdletName = "New-AzureStorageAccount";
        public const string GetAzureStorageAccountCmdletName = "Get-AzureStorageAccount";        
        public const string SetAzureStorageAccountCmdletName = "Set-AzureStorageAccount";        
        public static string RemoveAzureStorageAccountCmdletName = "Remove-AzureStorageAccount";


        // AzureStorageKey
        public static string NewAzureStorageKeyCmdletName = "New-AzureStorageKey";
        public static string GetAzureStorageKeyCmdletName = "Get-AzureStorageKey";

        // AzureSubnet
        public static string GetAzureSubnetCmdletName = "Get-AzureSubnet";
        public static string SetAzureSubnetCmdletName = "Set-AzureSubnet";


        // AzureSubscription
        public const string GetAzureSubscriptionCmdletName = "Get-AzureSubscription";
        public const string SetAzureSubscriptionCmdletName = "Set-AzureSubscription";
        public const string SelectAzureSubscriptionCmdletName = "Select-AzureSubscription";
        public const string RemoveAzureSubscriptionCmdletName = "Remove-AzureSubscription";        


        // AzureVhd
        public static string AddAzureVhdCmdletName = "Add-AzureVhd";
        public static string SaveAzureVhdCmdletName = "Save-AzureVhd";

        // AzureVM
        public const string NewAzureVMCmdletName = "New-AzureVM";
        public const string GetAzureVMCmdletName = "Get-AzureVM";
        public const string UpdateAzureVMCmdletName = "Update-AzureVM";                
        public const string RemoveAzureVMCmdletName = "Remove-AzureVM";
        
        public const string ExportAzureVMCmdletName = "Export-AzureVM";
        public const string ImportAzureVMCmdletName = "Import-AzureVM";
        
        public const string StartAzureVMCmdletName = "Start-AzureVM";
        public const string StopAzureVMCmdletName = "Stop-AzureVM";
        public const string RestartAzureVMCmdletName = "Restart-AzureVM";
        
        
        

        // AzureVMConfig
        public const string NewAzureVMConfigCmdletName = "New-AzureVMConfig";

        // AzureVMImage
        
        public const string AddAzureVMImageCmdletName = "Add-AzureVMImage";
        public const string GetAzureVMImageCmdletName = "Get-AzureVMImage";
        public const string RemoveAzureVMImageCmdletName = "Remove-AzureVMImage";
        public const string SaveAzureVMImageCmdletName = "Save-AzureVMImage";
        public const string UpdateAzureVMImageCmdletName = "Update-AzureVMImage";
        
        // AzureVMSize
        public const string SetAzureVMSizeCmdletName = "Set-AzureVMSize";

        // AzureVNetConfig & AzureVNetConnection
        public const string GetAzureVNetConfigCmdletName = "Get-AzureVNetConfig";
        public const string SetAzureVNetConfigCmdletName = "Set-AzureVNetConfig";
        public const string RemoveAzureVNetConfigCmdletName = "Remove-AzureVNetConfig";
        
        public const string GetAzureVNetConnectionCmdletName = "Get-AzureVNetConnection";

        // AzureVnetGateway & AzureVnetGatewayKey
        public const string NewAzureVNetGatewayCmdletName = "New-AzureVNetGateway";
        public const string GetAzureVNetGatewayCmdletName = "Get-AzureVNetGateway";
        public const string SetAzureVNetGatewayCmdletName = "Set-AzureVNetGateway";
        public const string RemoveAzureVNetGatewayCmdletName = "Remove-AzureVNetGateway";

        public const string GetAzureVNetGatewayKeyCmdletName = "Get-AzureVNetGatewayKey";

        // AzureVNetSite
        public const string GetAzureVNetSiteCmdletName = "Get-AzureVNetSite";

        // AzureWalkUpgradeDomain
        public const string SetAzureWalkUpgradeDomainCmdletName = "Set-AzureWalkUpgradeDomain";


        public const string GetModuleCmdletName = "Get-Module";       
        public const string TestAzureNameCmdletName = "Test-AzureName";
        
        public const string CopyAzureStorageBlobCmdletName = "Copy-AzureStorageBlob";


        public static string SetAzureAclConfigCmdletName = "Set-AzureAclConfig";

        public static string NewAzureAclConfigCmdletName = "New-AzureAclConfig";

        public static string GetAzureAclConfigCmdletName = "Get-AzureAclConfig";

        public static string SetAzureLoadBalancedEndpointCmdletName = "Set-AzureLoadBalancedEndpoint";

        public static string GetUniqueShortName(string prefix = "", int length = 6, string suffix = "", bool includeDate = false)
        {
            string dateSuffix = "";
            if (includeDate)
            {
                dateSuffix = string.Format("-{0}{1}", DateTime.Now.Year, DateTime.Now.DayOfYear);
            }
            return string.Format("{0}{1}{2}{3}", prefix, Guid.NewGuid().ToString("N").Substring(0, length), suffix, dateSuffix);
        }

        public static int MatchKeywords(string input, string[] keywords, bool exactMatch = true)
        { //returns -1 for no match, 0 for exact match, and a positive number for how many keywords are matched.
            int result = 0;
            if (string.IsNullOrEmpty(input) || keywords.Length == 0)
                return -1;
            foreach (string keyword in keywords)
            {
                //For whole word match, modify pattern to be "\b{0}\b"
                if (!string.IsNullOrEmpty(keyword) && Regex.IsMatch(input, string.Format(@"{0}", Regex.Escape(keyword)), RegexOptions.IgnoreCase))
                {
                    result++;
                }
            }
            if (result == keywords.Length)
            {
                return 0;
            }
            else if (result == 0)
            {
                return -1;
            }
            else
            {
                if (exactMatch)
                {
                    return -1;
                }
                else
                {
                    return result;
                }
            }
        }

        public static Uri GetDeploymentAndWaitForReady(string serviceName, string slot, int waitTime, int maxWaitTime)
        {

            ServiceManagementCmdletTestHelper vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();            
                       
            DateTime startTime = DateTime.Now;
            while (true)
            {
                bool allReady = true;
                DeploymentInfoContext result = vmPowershellCmdlets.GetAzureDeployment(serviceName, slot);
                int instanceNum = result.RoleInstanceList.Count;
                bool[] isReady = new bool[instanceNum];

                for (int j = 0; j < instanceNum; j++)
                {
                    var instance = result.RoleInstanceList[j];
                    Console.WriteLine("Instance: {0}, Status: {1}", instance.InstanceName, instance.InstanceStatus);
                    isReady[j] = (instance.InstanceStatus == "ReadyRole");
                    allReady &= isReady[j];
                }

                if (!allReady && (DateTime.Now - startTime).TotalSeconds < maxWaitTime)
                {
                    Console.WriteLine("Some roles are not ready, waiting for {0} seconds.", waitTime);
                    Thread.Sleep(waitTime*1000);
                }
                else if (!allReady) // some roles are not ready, and time-out.
                {
                    Assert.Fail("Deployment is not ready within {0} seconds!", maxWaitTime);
                }
                else // all roles are ready
                {
                    Console.WriteLine("Result of the deployment: {0}", result.Status);                    
                    return result.Url;
                }
            }
            
        }

        public static bool PrintAndCompareDeployment
            (DeploymentInfoContext deployment, string serviceName, string deploymentName, string deploymentLabel, string slot, string status, int instanceCount)
        {
            Console.WriteLine("ServiceName:{0}, DeploymentID: {1}, Uri: {2}", deployment.ServiceName, deployment.DeploymentId, deployment.Url.AbsoluteUri);
            Console.WriteLine("Name - {0}, Label - {1}, Slot - {2}, Status - {3}",
                deployment.DeploymentName, deployment.Label, deployment.Slot, deployment.Status);
            Console.WriteLine("RoleInstance: {0}", deployment.RoleInstanceList.Count);
            foreach (var instance in deployment.RoleInstanceList)
            {
                Console.WriteLine("InstanceName - {0}, InstanceStatus - {1}", instance.InstanceName, instance.InstanceStatus);
            }

            Assert.AreEqual(deployment.ServiceName, serviceName);
            Assert.AreEqual(deployment.DeploymentName, deploymentName);
            Assert.AreEqual(deployment.Label, deploymentLabel);
            Assert.AreEqual(deployment.Slot, slot);
            if (status != null)
            {
                Assert.AreEqual(deployment.Status, status);
            }

            Assert.AreEqual(deployment.RoleInstanceList.Count, instanceCount);

            return true;
        }

        // CheckRemove checks if 'fn(name)' exists.    'fn(name)' is usually 'Get-AzureXXXXX name'
        public static bool CheckRemove<Arg, Ret>(Func<Arg, Ret> fn, Arg name)
        {
            try
            {
                fn(name);
                Console.WriteLine("{0} still exists!", name);
                return false;
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("ResourceNotFound"))
                {
                    Console.WriteLine("{0} does not exist.", name);
                    return true;
                }
                else
                {
                    Console.WriteLine("Error: {0}", e.ToString());
                    return false;
                }
            }
        }

        // CheckRemove checks if 'fn(name)' exists.    'fn(name)' is usually 'Get-AzureXXXXX name'
        public static bool CheckRemove<Arg1, Arg2, Ret>(Func<Arg1, Arg2, Ret> fn, Arg1 name1, Arg2 name2)
        {
            try
            {
                fn(name1, name2);
                Console.WriteLine("{0}, {1} still exist!", name1, name2);
                return false;
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("ResourceNotFound"))
                {
                    Console.WriteLine("{0}, {1} is successfully removed", name1, name2);
                    return true;
                }
                else
                {
                    Console.WriteLine("Error: {0}", e.ToString());
                    return false;
                }
            }
        }

        // CheckRemove checks if 'fn(name)' exists.    'fn(name)' is usually 'Get-AzureXXXXX name'
        public static bool CheckRemove<Arg1, Arg2, Arg3, Ret>(Func<Arg1, Arg2, Arg3, Ret> fn, Arg1 name1, Arg2 name2, Arg3 name3)
        {
            try
            {
                fn(name1, name2, name3);
                Console.WriteLine("{0}, {1}, {2} still exist!", name1, name2, name3);
                return false;
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("ResourceNotFound"))
                {
                    Console.WriteLine("{0}, {1}, {2} is successfully removed", name1, name2, name3);
                    return true;
                }
                else
                {
                    Console.WriteLine("Error: {0}", e.ToString());
                    return false;
                }
            }
        }

        public static BlobHandle GetBlobHandle(string blob, string key)
        {
            BlobUri blobPath;
            Assert.IsTrue(BlobUri.TryParseUri(new Uri(blob), out blobPath));
            return new BlobHandle(blobPath, key);
        }

        public static void RetryActionUntilSuccess(Action act, string errorMessage, int maxTry, int intervalSeconds)
        {
            int i = 0;
            while (i < maxTry)
            {
                try
                {

                    act();
                    return;
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains(errorMessage))
                    {
                        Console.WriteLine("{0} error occurs! retrying ...", errorMessage);
                        Console.WriteLine(e.InnerException.ToString());
                        Thread.Sleep(intervalSeconds * 1000);
                        i++;
                        continue;
                    }
                    else
                    {
                        Console.WriteLine(e.ToString());
                        if (e.InnerException != null)
                        {
                            Console.WriteLine(e.InnerException.ToString());
                        }
                        throw;
                    }
                }
            }
        }

        public static X509Certificate2 InstallCert(string certFile, StoreLocation location = StoreLocation.CurrentUser, StoreName name = StoreName.My)
        {
            X509Certificate2 cert = new X509Certificate2(certFile);
            X509Store certStore = new X509Store(name, location);
            certStore.Open(OpenFlags.ReadWrite);
            certStore.Add(cert);
            certStore.Close();
            Console.WriteLine("Cert, {0}, is installed.", cert.Thumbprint);
            return cert;
        }

        public static void UninstallCert(X509Certificate2 cert, StoreLocation location, StoreName name)
        {
            try
            {
                X509Store certStore = new X509Store(name, location);
                certStore.Open(OpenFlags.ReadWrite);
                certStore.Remove(cert);
                certStore.Close();
                Console.WriteLine("Cert, {0}, is uninstalled.", cert.Thumbprint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during uninstalling the cert: {0}", e.ToString());
                throw;
            }
        }
        public static X509Certificate2 CreateCertificate(string password, string issuer = "CN=Windows Azure Powershell Test", string friendlyName = "PSTest")
        {

            var keyCreationParameters = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowExport,
                KeyCreationOptions = CngKeyCreationOptions.None,
                KeyUsage = CngKeyUsages.AllUsages,
                Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider
            };

            keyCreationParameters.Parameters.Add(new CngProperty("Length", BitConverter.GetBytes(2048), CngPropertyOptions.None));

            CngKey key = CngKey.Create(CngAlgorithm2.Rsa, null, keyCreationParameters);

            var creationParams = new X509CertificateCreationParameters(new X500DistinguishedName(issuer))
            {
                TakeOwnershipOfKey = true
            };

            X509Certificate2 cert = key.CreateSelfSignedCertificate(creationParams);
            key = null;
            cert.FriendlyName = friendlyName;

            byte[] bytes = cert.Export(X509ContentType.Pfx, password);
            X509Certificate2 returnCert = new X509Certificate2();
            returnCert.Import(bytes, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            return returnCert;
        }        

        public static SecureString convertToSecureString(string str)
        {
            SecureString secureStr = new SecureString();
            foreach (char c in str)
            {
                secureStr.AppendChar(c);
            }
            return secureStr;
        }

        private static void RegisterDllsForRDP()
        {
            Assert.IsTrue(File.Exists(tclientPath), "{0} does not exist!", tclientPath);
            Assert.IsTrue(File.Exists(clxtsharPath), "{0} does not exist!", clxtsharPath);
            Assert.IsTrue(File.Exists(RDPTestPath), "{0}, does not exist!", RDPTestPath);

            ExecuteSimpleProcess("regsvr32", "/s " + tclientPath);
            ExecuteSimpleProcess("regsvr32", "/s " + clxtsharPath);
        }

        public static bool RDPtestPaaS(string dns, string roleName, int instance, string user, string psswrd, bool shouldSucceed)
        {
            RegisterDllsForRDP();

            int returnCode = ExecuteSimpleProcess(RDPTestPath,
                 String.Format("PaaS {0} {1} {2} {3} {4} {5}", dns, roleName, instance.ToString(), user, psswrd, shouldSucceed.ToString()));
            if (returnCode == 0)
            {
                Console.WriteLine("RDP access succeeded.");
                return true;
            }
            else
            {
                Console.WriteLine("RDP Failed!!");
                return false;
            }
        }

        public static bool RDPtestIaaS(string dns, int? port, string user, string psswrd, bool shouldSucceed)
        {
            RegisterDllsForRDP();

            int returnCode = ExecuteSimpleProcess(RDPTestPath,
                 String.Format("IaaS {0} {1} {2} {3} {4}", dns, port.ToString(), user, psswrd, shouldSucceed.ToString()));
            if (returnCode == 0)
            {
                Console.WriteLine("RDP access succeeded.");
                return true;
            }
            else
            {
                Console.WriteLine("RDP Failed!!");
                return false;
            }
        }

        public static int ExecuteSimpleProcess(string fileName, string arguments)
        {
            Process p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = arguments;
            p.Start();
            p.WaitForExit();
            return p.ExitCode;
        }

        public static string FindSubstring(string givenStr, char givenChar, int i)
        {
            if (i > 0)
            {
                return FindSubstring(givenStr.Substring(givenStr.IndexOf(givenChar) + 1), givenChar, i - 1);
            }
            else
            {
                return givenStr;
            }
        }

        public static bool CompareDateTime(DateTime expectedDate, string givenDate)
        {
            DateTime resultExpDate = DateTime.Parse(givenDate);
            bool result = (resultExpDate.Day == expectedDate.Day);
            result &= (resultExpDate.Month == expectedDate.Month);
            result &= (resultExpDate.Year == expectedDate.Year);
            return result;
        }

        public static string GetInnerXml(string xmlString, string tag)
        {
            string removedHeader = "<" + Utilities.FindSubstring(xmlString, '<', 2);

            byte[] encodedString = Encoding.UTF8.GetBytes(xmlString);
            MemoryStream stream = new MemoryStream(encodedString);
            stream.Flush();
            stream.Position = 0;

            XmlDocument xml = new XmlDocument();
            xml.Load(stream);
            return xml.GetElementsByTagName(tag)[0].InnerXml;
        }

        public static bool CompareWadCfg(string wadcfg, XmlDocument daconfig)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(wadcfg))
                {
                    Assert.IsNull(wadcfg);
                }
                else
                {
                    string innerXml = daconfig.InnerXml;
                    Assert.AreEqual(Utilities.FindSubstring(wadcfg, '<', 2), Utilities.FindSubstring(innerXml, '<', 2));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        static public string GenerateSasUri(string blobStorageEndpointFormat, string storageAccount, string storageAccountKey,
            string blobContainer, string vhdName, int hours = 10, bool read = true, bool write = true, bool delete = true, bool list = true)
        {
            string destinationSasUri = string.Format(@blobStorageEndpointFormat, storageAccount) + string.Format("/{0}/{1}", blobContainer, vhdName);
            var destinationBlob = new CloudPageBlob(new Uri(destinationSasUri), new StorageCredentials(storageAccount, storageAccountKey));
            SharedAccessBlobPermissions permission = 0;
            permission |= (read) ? SharedAccessBlobPermissions.Read : 0;
            permission |= (write) ? SharedAccessBlobPermissions.Write : 0;
            permission |= (delete) ? SharedAccessBlobPermissions.Delete : 0;
            permission |= (list) ? SharedAccessBlobPermissions.List : 0;

            var policy = new SharedAccessBlobPolicy()
            {
                Permissions = permission,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromHours(hours)
            };

            string destinationBlobToken = destinationBlob.GetSharedAccessSignature(policy);
            return (destinationSasUri + destinationBlobToken);
        }

        static public void RecordTimeTaken(ref DateTime prev)
        {
            var period = DateTime.Now - prev;
            Console.WriteLine("{0} minutes {1} seconds and {2} ms passed...", period.Minutes.ToString(), period.Seconds.ToString(), period.Milliseconds.ToString());
            prev = DateTime.Now;
        }
    }
}
