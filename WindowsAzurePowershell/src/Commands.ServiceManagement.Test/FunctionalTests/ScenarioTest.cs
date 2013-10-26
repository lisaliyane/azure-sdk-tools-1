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
    using Commands.Utilities.Common;
    using ConfigDataInfo;
    using Extensions;
    using Model;
    using Properties;
    using Service.Gateway;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Net.Cache;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using VisualStudio.TestTools.UnitTesting;
    using WindowsAzure.ServiceManagement;
    using System.Security.Cryptography.X509Certificates;

    [TestClass]
    public class ScenarioTest : ServiceManagementTest
    {
        private string serviceName;
        string perfFile;

        [TestInitialize]
        public void Initialize()
        {
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
            pass = false;
            testStartTime = DateTime.Now;
        }

        /// <summary>
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestCategory("BVT"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,Get-AzureVM,Get-AzureLocation,Import-AzurePublishSettingsFile,Get-AzureSubscription,Set-AzureSubscription)")]
        public void NewWindowsAzureQuickVM()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string newAzureQuickVMName = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {

                if (string.IsNullOrEmpty(imageName))
                    imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);

                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName);

                // Verify
                PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName);
                Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

                try
                {
                    vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName + "wrongVMName", serviceName);
                    Assert.Fail("Should Fail!!");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Fail as expected: {0}", e.ToString());
                }

                // Cleanup
                vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
                Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Basic Provisioning a Virtual Machine
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestCategory("BVT"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (Get-AzureLocation,Test-AzureName ,Get-AzureVMImage,New-AzureQuickVM,Get-AzureVM ,Restart-AzureVM,Stop-AzureVM , Start-AzureVM)")]
        public void ProvisionLinuxVM()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");
            string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);

            try
            {
                vmPowershellCmdlets.NewAzureQuickVM(OS.Linux, newAzureLinuxVMName, serviceName, linuxImageName, "user",
                                                    password, locationName);

                // Verify
                PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, serviceName);
                Assert.AreEqual(newAzureLinuxVMName, vmRoleCtxt.Name, true);
                try
                {
                    vmPowershellCmdlets.RemoveAzureVM(newAzureLinuxVMName + "wrongVMName", serviceName);
                    Assert.Fail("Should Fail!!");
                }
                catch (Exception e)
                {
                    if (e is AssertFailedException)
                    {
                        throw;
                    }
                    Console.WriteLine("Fail as expected: {0}", e);
                }

                // Cleanup
                vmPowershellCmdlets.RemoveAzureVM(newAzureLinuxVMName, serviceName);
                Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, serviceName));
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Verify Advanced Provisioning
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureService,New-AzureVMConfig,Add-AzureProvisioningConfig ,Add-AzureDataDisk ,Add-AzureEndpoint,New-AzureVM)")]
        public void AdvancedProvisioning()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            string newAzureVM1Name = Utilities.GetUniqueShortName(vmNamePrefix);
            string newAzureVM2Name = Utilities.GetUniqueShortName(vmNamePrefix);
            if (string.IsNullOrEmpty(imageName))
            {
                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
            }

            vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);

            AzureVMConfigInfo azureVMConfigInfo1 = new AzureVMConfigInfo(newAzureVM1Name, InstanceSize.ExtraSmall, imageName);
            AzureVMConfigInfo azureVMConfigInfo2 = new AzureVMConfigInfo(newAzureVM2Name, InstanceSize.ExtraSmall, imageName);
            AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password);
            AddAzureDataDiskConfig azureDataDiskConfigInfo = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AzureEndPointConfigInfo azureEndPointConfigInfo = new AzureEndPointConfigInfo(AzureEndPointConfigInfo.ParameterSet.CustomProbe, ProtocolInfo.tcp, 80, 80, "web", "lbweb", 80, ProtocolInfo.http, @"/", null, null);

            PersistentVMConfigInfo persistentVMConfigInfo1 = new PersistentVMConfigInfo(azureVMConfigInfo1, azureProvisioningConfig, azureDataDiskConfigInfo, azureEndPointConfigInfo);
            PersistentVMConfigInfo persistentVMConfigInfo2 = new PersistentVMConfigInfo(azureVMConfigInfo2, azureProvisioningConfig, azureDataDiskConfigInfo, azureEndPointConfigInfo);

            PersistentVM persistentVM1 = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo1);
            PersistentVM persistentVM2 = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo2);

            PersistentVM[] VMs = { persistentVM1, persistentVM2 };
            vmPowershellCmdlets.NewAzureVM(serviceName, VMs);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureVM1Name, serviceName);
            vmPowershellCmdlets.RemoveAzureVM(newAzureVM2Name, serviceName);

            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureVM1Name, serviceName));
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureVM2Name, serviceName));
            pass = true;
        }

        /// <summary>
        /// Modifying Existing Virtual Machines
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig ,Add-AzureDataDisk ,Add-AzureEndpoint,New-AzureVM)")]
        public void ModifyingVM()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            string newAzureQuickVMName = Utilities.GetUniqueShortName(vmNamePrefix);
            if (string.IsNullOrEmpty(imageName))
                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);

            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName);

            AddAzureDataDiskConfig azureDataDiskConfigInfo1 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AddAzureDataDiskConfig azureDataDiskConfigInfo2 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk2", 1);
            AzureEndPointConfigInfo azureEndPointConfigInfo = new AzureEndPointConfigInfo(AzureEndPointConfigInfo.ParameterSet.NoLB, ProtocolInfo.tcp, 1433, 2000, "sql");
            AddAzureDataDiskConfig[] dataDiskConfig = { azureDataDiskConfigInfo1, azureDataDiskConfigInfo2 };
            vmPowershellCmdlets.AddVMDataDisksAndEndPoint(newAzureQuickVMName, serviceName, dataDiskConfig, azureEndPointConfigInfo);

            SetAzureDataDiskConfig setAzureDataDiskConfig1 = new SetAzureDataDiskConfig(HostCaching.ReadWrite, 0);
            SetAzureDataDiskConfig setAzureDataDiskConfig2 = new SetAzureDataDiskConfig(HostCaching.ReadWrite, 0);
            SetAzureDataDiskConfig[] diskConfig = { setAzureDataDiskConfig1, setAzureDataDiskConfig2 };
            vmPowershellCmdlets.SetVMDataDisks(newAzureQuickVMName, serviceName, diskConfig);

            vmPowershellCmdlets.GetAzureDataDisk(newAzureQuickVMName, serviceName);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);

            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
            pass = true;
        }

        /// <summary>
        /// Changes that Require a Reboot
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (Get-AzureVM,Set-AzureDataDisk ,Update-AzureVM,Set-AzureVMSize)")]
        public void UpdateAndReboot()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            if (string.IsNullOrEmpty(imageName))
                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);

            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName);

            AddAzureDataDiskConfig azureDataDiskConfigInfo1 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AddAzureDataDiskConfig azureDataDiskConfigInfo2 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk2", 1);
            AddAzureDataDiskConfig[] dataDiskConfig = { azureDataDiskConfigInfo1, azureDataDiskConfigInfo2 };
            vmPowershellCmdlets.AddVMDataDisks(newAzureQuickVMName, serviceName, dataDiskConfig);

            SetAzureDataDiskConfig setAzureDataDiskConfig1 = new SetAzureDataDiskConfig(HostCaching.ReadOnly, 0);
            SetAzureDataDiskConfig setAzureDataDiskConfig2 = new SetAzureDataDiskConfig(HostCaching.ReadOnly, 0);
            SetAzureDataDiskConfig[] diskConfig = { setAzureDataDiskConfig1, setAzureDataDiskConfig2 };
            vmPowershellCmdlets.SetVMDataDisks(newAzureQuickVMName, serviceName, diskConfig);

            SetAzureVMSizeConfig vmSizeConfig = new SetAzureVMSizeConfig(InstanceSize.Medium);
            vmPowershellCmdlets.SetVMSize(newAzureQuickVMName, serviceName, vmSizeConfig);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
            pass = true;
        }

        /// <summary>
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Get-AzureDisk,Remove-AzureVM,Remove-AzureDisk,Get-AzureVMImage)")]
        public void ManagingDiskImages()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Create a unique VM name and Service Name
            string newAzureQuickVMName = Utilities.GetUniqueShortName(vmNamePrefix);
            if (string.IsNullOrEmpty(imageName))
            {
                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
            }

            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, serviceName);

            // starting the test.
            Collection<DiskContext> vmDisks = vmPowershellCmdlets.GetAzureDiskAttachedToRoleName(new[] { newAzureQuickVMName });  // Get-AzureDisk | Where {$_.AttachedTo.RoleName -eq $vmname }

            foreach (var disk in vmDisks)
                Console.WriteLine("The disk, {0}, is created", disk.DiskName);

            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);  // Remove-AzureVM
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
            Console.WriteLine("The VM, {0}, is successfully removed.", newAzureQuickVMName);

            foreach (var disk in vmDisks)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        vmPowershellCmdlets.RemoveAzureDisk(disk.DiskName, false); // Remove-AzureDisk
                        break;
                    }
                    catch (Exception e)
                    {
                        if (e.ToString().ToLowerInvariant().Contains("currently in use") && i != 2)
                        {
                            Console.WriteLine("The vhd, {0}, is still in the state of being used by the deleted VM", disk.DiskName);
                            Thread.Sleep(120000);
                            continue;
                        }
                        else
                        {
                            Assert.Fail("error during Remove-AzureDisk: {0}", e.ToString());
                        }
                    }
                }

                try
                {
                    vmPowershellCmdlets.GetAzureDisk(disk.DiskName); // Get-AzureDisk -DiskName (try to get the removed disk.)
                    Console.WriteLine("Disk is not removed: {0}", disk.DiskName);
                    pass = false;
                }
                catch (Exception e)
                {
                    if (e.ToString().ToLowerInvariant().Contains("does not exist"))
                    {
                        Console.WriteLine("The disk, {0}, is successfully removed.", disk.DiskName);
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Exception: {0}", e.ToString());
                    }
                }
            }
            pass = true;
        }

        /// <summary>
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM,Save-AzureVMImage)")]
        public void CaptureImagingExportingImportingVMConfig()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Create a unique VM name
            string newAzureVMName = Utilities.GetUniqueShortName("PSTestVM");
            Console.WriteLine("VM Name: {0}", newAzureVMName);

            // Create a unique Service Name
            vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
            Console.WriteLine("Service Name: {0}", serviceName);
            if (string.IsNullOrEmpty(imageName))
                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);

            // starting the test.
            AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(newAzureVMName, InstanceSize.Small, imageName); // parameters for New-AzureVMConfig (-Name -InstanceSize -ImageName)
            AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password); // parameters for Add-AzureProvisioningConfig (-Windows -Password)
            PersistentVMConfigInfo persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
            PersistentVM persistentVM = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo); // New-AzureVMConfig & Add-AzureProvisioningConfig

            PersistentVM[] VMs = { persistentVM };
            vmPowershellCmdlets.NewAzureVM(serviceName, VMs); // New-AzureVM
            Console.WriteLine("The VM is successfully created: {0}", persistentVM.RoleName);
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, serviceName);
            Assert.AreEqual(vmRoleCtxt.Name, persistentVM.RoleName, true);


            vmPowershellCmdlets.StopAzureVM(newAzureVMName, serviceName, true); // Stop-AzureVM
            for (int i = 0; i < 3; i++)
            {
                vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, serviceName);
                if (vmRoleCtxt.InstanceStatus == "StoppedVM")
                    break;
                else
                {
                    Console.WriteLine("The status of the VM {0} : {1}", persistentVM.RoleName, vmRoleCtxt.InstanceStatus);
                    Thread.Sleep(120000);
                }
            }
            Assert.AreEqual(vmRoleCtxt.InstanceStatus, "StoppedVM", true);

            //TODO
            // RDP

            //TODO:
            // Run sysprep and shutdown

            // Check the status of VM
            //PersistentVMRoleContext vmRoleCtxt2 = vmPowershellCmdlets.GetAzureVM(newAzureVMName, newAzureSvcName); // Get-AzureVM -Name
            //Assert.AreEqual(newAzureVMName, vmRoleCtxt2.Name, true);  //

            // Save-AzureVMImage
            //string newImageName = "newImage";
            //string newImageLabel = "newImageLabel";
            //string postAction = "Delete";

            // Save-AzureVMImage -ServiceName -Name -NewImageName -NewImageLabel -PostCaptureAction
            //vmPowershellCmdlets.SaveAzureVMImage(newAzureSvcName, newAzureVMName, newImageName, newImageLabel, postAction);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(persistentVM.RoleName, serviceName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, serviceName));
        }

        /// <summary>
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Export-AzureVM,Remove-AzureVM,Import-AzureVM,New-AzureVM)")]
        public void ExportingImportingVMConfigAsTemplateforRepeatableUsage()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Create a new Azure quick VM
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            if (string.IsNullOrEmpty(imageName))
                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, serviceName);

            // starting the test.
            string path = ".\\mytestvmconfig1.xml";
            PersistentVMRoleContext vmRole = vmPowershellCmdlets.ExportAzureVM(newAzureQuickVMName, serviceName, path); // Export-AzureVM
            Console.WriteLine("Exporting VM is successfully done: path - {0}  Name - {1}", path, vmRole.Name);

            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName); // Remove-AzureVM
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
            Console.WriteLine("The VM is successfully removed: {0}", newAzureQuickVMName);

            List<PersistentVM> VMs = new List<PersistentVM>();
            foreach (var pervm in vmPowershellCmdlets.ImportAzureVM(path)) // Import-AzureVM
            {
                VMs.Add(pervm);
                Console.WriteLine("The VM, {0}, is imported.", pervm.RoleName);
            }


            for (int i = 0; i < 3; i++)
            {
                try
                {
                    vmPowershellCmdlets.NewAzureVM(serviceName, VMs.ToArray()); // New-AzureVM
                    Console.WriteLine("All VMs are successfully created.");
                    foreach (var vm in VMs)
                    {
                        Console.WriteLine("created VM: {0}", vm.RoleName);
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (e.ToString().ToLowerInvariant().Contains("currently in use") && i != 2)
                    {
                        Console.WriteLine("The removed VM is still using the vhd");
                        Thread.Sleep(120000);
                        continue;
                    }
                    else
                    {
                        Assert.Fail("error during New-AzureVM: {0}", e.ToString());
                    }
                }
            }

            // Verify
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName);
            Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));

            pass = true;
        }

        /// <summary>
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Get-AzureVM,Get-AzureEndpoint,Get-AzureRemoteDesktopFile)")]
        public void ManagingRDPSSHConnectivity()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Create a new Azure quick VM
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            if (string.IsNullOrEmpty(imageName))
                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, serviceName);

            // starting the test.
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName); // Get-AzureVM
            InputEndpointContext inputEndpointCtxt = vmPowershellCmdlets.GetAzureEndPoint(vmRoleCtxt)[0]; // Get-AzureEndpoint
            Console.WriteLine("InputEndpointContext Name: {0}", inputEndpointCtxt.Name);
            Console.WriteLine("InputEndpointContext port: {0}", inputEndpointCtxt.Port);
            Console.WriteLine("InputEndpointContext protocol: {0}", inputEndpointCtxt.Protocol);
            Assert.AreEqual(inputEndpointCtxt.Name, "RemoteDesktop", true);

            string path = ".\\myvmconnection.rdp";
            vmPowershellCmdlets.GetAzureRemoteDesktopFile(newAzureQuickVMName, serviceName, path, false); // Get-AzureRemoteDesktopFile
            Console.WriteLine("RDP file is successfully created at: {0}", path);

            // ToDo: Automate RDP.
            //vmPowershellCmdlets.GetAzureRemoteDesktopFile(newAzureQuickVMName, newAzureQuickVMSvcName, path, true); // Get-AzureRemoteDesktopFile -Launch

            Console.WriteLine("Test passed");

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));

            pass = true;
        }

        /// <summary>
        /// Basic Provisioning a Virtual Machine
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New,Get,Set,Remove,Move)-AzureDeployment)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageScenario.csv", "packageScenario#csv", DataAccessMethod.Sequential)]
        public void DeploymentUpgrade()
        {

            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            perfFile = @"..\deploymentUpgradeResult.csv";

            // Choose the package and config files from local machine
            string path = Convert.ToString(TestContext.DataRow["path"]);
            string packageName = Convert.ToString(TestContext.DataRow["packageName"]);
            string configName = Convert.ToString(TestContext.DataRow["configName"]);
            string upgradePackageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            string upgradeConfigName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);
            string upgradeConfigName2 = Convert.ToString(TestContext.DataRow["upgradeConfig2"]);

            var packagePath1 = new FileInfo(@path + packageName); // package with two roles
            var packagePath2 = new FileInfo(@path + upgradePackageName); // package with one role
            var configPath1 = new FileInfo(@path + configName); // config with 2 roles, 4 instances each
            var configPath2 = new FileInfo(@path + upgradeConfigName); // config with 1 role, 2 instances
            var configPath3 = new FileInfo(@path + upgradeConfigName2); // config with 1 role, 4 instances

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            DeploymentInfoContext result;

            try
            {
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                Console.WriteLine("service, {0}, is created.", serviceName);

                // New deployment to Production
                DateTime start = DateTime.Now;
                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false);

                TimeSpan duration = DateTime.Now - start;

                Uri site = Utilities.GetDeploymentAndWaitForReady(serviceName, DeploymentSlotType.Production, 10, 1000);

                System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("Deployment, {0}, {1}", duration, DateTime.Now - start) });

                Console.WriteLine("site: {0}", site.ToString());
                Console.WriteLine("Time for all instances to become in ready state: {0}", DateTime.Now - start);

                // Auto-Upgrade the deployment
                start = DateTime.Now;
                vmPowershellCmdlets.SetAzureDeploymentUpgrade(serviceName, DeploymentSlotType.Production, UpgradeType.Auto, packagePath1.FullName, configPath1.FullName);
                duration = DateTime.Now - start;
                Console.WriteLine("Auto upgrade took {0}.", duration);

                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, serviceName, DeploymentSlotType.Production, null, 8);
                Console.WriteLine("successfully updated the deployment");

                site = Utilities.GetDeploymentAndWaitForReady(serviceName, DeploymentSlotType.Production, 10, 600);
                System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("Auto Upgrade, {0}, {1}", duration, DateTime.Now - start) });

                // Manual-Upgrade the deployment
                start = DateTime.Now;
                vmPowershellCmdlets.SetAzureDeploymentUpgrade(serviceName, DeploymentSlotType.Production, UpgradeType.Manual, packagePath1.FullName, configPath1.FullName);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 0);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 1);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 2);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 3);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 4);
                duration = DateTime.Now - start;
                Console.WriteLine("Manual upgrade took {0}.", duration);

                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, serviceName, DeploymentSlotType.Production, null, 8);
                Console.WriteLine("successfully updated the deployment");

                site = Utilities.GetDeploymentAndWaitForReady(serviceName, DeploymentSlotType.Production, 10, 600);
                System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("Manual Upgrade, {0}, {1}", duration, DateTime.Now - start) });

                // Simulatenous-Upgrade the deployment
                start = DateTime.Now;
                vmPowershellCmdlets.SetAzureDeploymentUpgrade(serviceName, DeploymentSlotType.Production, UpgradeType.Simultaneous, packagePath1.FullName, configPath1.FullName);
                duration = DateTime.Now - start;
                Console.WriteLine("Simulatenous upgrade took {0}.", duration);

                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, serviceName, DeploymentSlotType.Production, null, 8);
                Console.WriteLine("successfully updated the deployment");

                site = Utilities.GetDeploymentAndWaitForReady(serviceName, DeploymentSlotType.Production, 10, 600);
                System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("Simulatenous Upgrade, {0}, {1}", duration, DateTime.Now - start) });

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                pass = Utilities.CheckRemove(vmPowershellCmdlets.GetAzureDeployment, serviceName);
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }


        /// <summary>
        /// AzureVNetGatewayTest()
        /// </summary>
        /// Note: Create a VNet, a LocalNet from the portal without creating a gateway.
        [TestMethod(), TestCategory("LongRunningTest"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"),
        Description("Test the cmdlet ((Set,Remove)-AzureVNetConfig, Get-AzureVNetSite, (New,Get,Set,Remove)-AzureVNetGateway, Get-AzureVNetConnection)")]
        public void VNetTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            string newAzureQuickVMName = Utilities.GetUniqueShortName(vmNamePrefix);
            if (string.IsNullOrEmpty(imageName))
                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);

            // Read the vnetconfig file and get the names of local networks, virtual networks and affinity groups.
            XDocument vnetconfigxml = XDocument.Load(vnetConfigFilePath);
            List<string> localNets = new List<string>();
            List<string> virtualNets = new List<string>();
            HashSet<string> affinityGroups = new HashSet<string>();

            foreach (XElement el in vnetconfigxml.Descendants())
            {
                switch (el.Name.LocalName)
                {
                    case "LocalNetworkSite":
                        localNets.Add(el.FirstAttribute.Value);
                        break;
                    case "VirtualNetworkSite":
                        virtualNets.Add(el.Attribute("name").Value);
                        affinityGroups.Add(el.Attribute("AffinityGroup").Value);
                        break;
                    default:
                        break;
                }
            }

            foreach (string aff in affinityGroups)
            {
                if (Utilities.CheckRemove(vmPowershellCmdlets.GetAzureAffinityGroup, aff))
                {

                    vmPowershellCmdlets.NewAzureAffinityGroup(aff, Resource.Location, null, null);
                }
            }

            string vnet1 = virtualNets[0];
            string lnet1 = localNets[0];

            try
            {
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName); // New-AzureQuickVM
                Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, serviceName);

                vmPowershellCmdlets.SetAzureVNetConfig(vnetConfigFilePath);

                foreach (VirtualNetworkSiteContext site in vmPowershellCmdlets.GetAzureVNetSite(null))
                {
                    Console.WriteLine("Name: {0}, AffinityGroup: {1}", site.Name, site.AffinityGroup);
                }

                foreach (string vnet in virtualNets)
                {
                    Assert.AreEqual(vnet, vmPowershellCmdlets.GetAzureVNetSite(vnet)[0].Name);
                    Assert.AreEqual(ProvisioningState.NotProvisioned, vmPowershellCmdlets.GetAzureVNetGateway(vnet)[0].State);
                }

                vmPowershellCmdlets.NewAzureVNetGateway(vnet1);

                Assert.IsTrue(GetVNetState(vnet1, ProvisioningState.Provisioned, 12, 60));

                // Set-AzureVNetGateway -Connect Test
                vmPowershellCmdlets.SetAzureVNetGateway("connect", vnet1, lnet1);

                foreach (GatewayConnectionContext connection in vmPowershellCmdlets.GetAzureVNetConnection(vnet1))
                {
                    Console.WriteLine("Connectivity: {0}, LocalNetwork: {1}", connection.ConnectivityState, connection.LocalNetworkSiteName);
                    Assert.IsFalse(connection.ConnectivityState.ToLowerInvariant().Contains("notconnected"));
                }

                // Get-AzureVNetGatewayKey
                SharedKeyContext result = vmPowershellCmdlets.GetAzureVNetGatewayKey(vnet1,
                    vmPowershellCmdlets.GetAzureVNetConnection(vnet1)[0].LocalNetworkSiteName);
                Console.WriteLine("Gateway Key: {0}", result.Value);


                // Set-AzureVNetGateway -Disconnect
                vmPowershellCmdlets.SetAzureVNetGateway("disconnect", vnet1, lnet1);

                foreach (GatewayConnectionContext connection in vmPowershellCmdlets.GetAzureVNetConnection(vnet1))
                {
                    Console.WriteLine("Connectivity: {0}, LocalNetwork: {1}", connection.ConnectivityState, connection.LocalNetworkSiteName);
                }

                // Remove-AzureVnetGateway
                vmPowershellCmdlets.RemoveAzureVNetGateway(vnet1);

                foreach (string vnet in virtualNets)
                {
                    VirtualNetworkGatewayContext gateway = vmPowershellCmdlets.GetAzureVNetGateway(vnet)[0];

                    Console.WriteLine("State: {0}, VIP: {1}", gateway.State.ToString(), gateway.VIPAddress);
                    if (vnet.Equals(vnet1))
                    {
                        Assert.AreEqual(ProvisioningState.Deprovisioning, gateway.State);
                    }
                    else
                    {
                        Assert.AreEqual(ProvisioningState.NotProvisioned, gateway.State);
                    }

                }

                //Utilities.RetryFunctionUntilSuccess<ManagementOperationContext>(vmPowershellCmdlets.RemoveAzureVNetConfig, "in use", 10, 30);
                Utilities.RetryActionUntilSuccess(() => vmPowershellCmdlets.RemoveAzureVNetConfig(), "in use", 10, 30);

                pass = true;

            }
            catch (Exception e)
            {
                pass = false;
                if (cleanupIfFailed)
                {
                    try
                    {
                        vmPowershellCmdlets.RemoveAzureVNetGateway(vnet1);
                    }
                    catch { }
                    Utilities.RetryActionUntilSuccess(() => vmPowershellCmdlets.RemoveAzureVNetConfig(), "in use", 10, 30);
                    //Utilities.RetryFunctionUntilSuccess<ManagementOperationContext>(vmPowershellCmdlets.RemoveAzureVNetConfig, "in use", 10, 30);
                }
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
            finally
            {
                foreach (string aff in affinityGroups)
                {
                    try
                    {
                        vmPowershellCmdlets.RemoveAzureAffinityGroup(aff);
                    }
                    catch
                    {
                        // Some service uses the affinity group, so it cannot be deleted.  Just leave it.
                    }
                }
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (New-AzureServiceRemoteDesktopConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\nodiagpackage.csv", "nodiagpackage#csv", DataAccessMethod.Sequential)]
        public void AzureServiceDiagnosticsExtensionConfigScenarioTest()
        {

            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Choose the package and config files from local machine
            string packageName = Convert.ToString(TestContext.DataRow["packageName"]);
            string configName = Convert.ToString(TestContext.DataRow["configName"]);
            var packagePath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + packageName);
            var configPath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + configName);

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            DeploymentInfoContext result;
            string storage = defaultAzureSubscription.CurrentStorageAccountName;
            XmlDocument daConfig = new XmlDocument();
            daConfig.Load(@".\da.xml");

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                Console.WriteLine("service, {0}, is created.", serviceName);

                ExtensionConfigurationInput config = vmPowershellCmdlets.NewAzureServiceDiagnosticsExtensionConfig(storage, daConfig);

                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false, config);

                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                pass = Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Production, null, 2);
                Console.WriteLine("successfully deployed the package");

                DiagnosticExtensionContext resultContext = vmPowershellCmdlets.GetAzureServiceDiagnosticsExtension(serviceName)[0];

                VerifyDiagExtContext(resultContext, "AllRoles", "Default-Diagnostics-Production-Ext-0", storage, daConfig);

                vmPowershellCmdlets.RemoveAzureServiceDiagnosticsExtension(serviceName);

                Assert.AreEqual(vmPowershellCmdlets.GetAzureServiceDiagnosticsExtension(serviceName).Count, 0);

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                pass &= Utilities.CheckRemove(vmPowershellCmdlets.GetAzureDeployment, serviceName, DeploymentSlotType.Production);
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }


        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceRemoteDesktopExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\nodiagpackage.csv", "nodiagpackage#csv", DataAccessMethod.Sequential)]
        public void AzureServiceDiagnosticsExtensionTest()
        {

            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Choose the package and config files from local machine
            string packageName = Convert.ToString(TestContext.DataRow["packageName"]);
            string configName = Convert.ToString(TestContext.DataRow["configName"]);
            var packagePath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + packageName);
            var configPath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + configName);

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            DeploymentInfoContext result;

            string storage = defaultAzureSubscription.CurrentStorageAccountName;
            XmlDocument daConfig = new XmlDocument();
            daConfig.Load(@".\da.xml");

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                Console.WriteLine("service, {0}, is created.", serviceName);

                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false);

                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                pass = Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Production, null, 2);
                Console.WriteLine("successfully deployed the package");

                vmPowershellCmdlets.SetAzureServiceDiagnosticsExtension(serviceName, storage, daConfig);

                DiagnosticExtensionContext resultContext = vmPowershellCmdlets.GetAzureServiceDiagnosticsExtension(serviceName)[0];

                Assert.IsTrue(VerifyDiagExtContext(resultContext, "AllRoles", "Default-Diagnostics-Production-Ext-0", storage, daConfig));

                vmPowershellCmdlets.RemoveAzureServiceDiagnosticsExtension(serviceName, true);

                Assert.AreEqual(vmPowershellCmdlets.GetAzureServiceDiagnosticsExtension(serviceName).Count, 0);

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                pass &= Utilities.CheckRemove(vmPowershellCmdlets.GetAzureDeployment, serviceName, DeploymentSlotType.Production);
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }


        // Disabled. Tracking Issue # 1479
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (New-AzureServiceRemoteDesktopConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void AzureServiceRemoteDesktopExtensionConfigScenarioTest()
        {

            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Choose the package and config files from local machine
            string packageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            string configName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);
            var packagePath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + packageName);
            var configPath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + configName);

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            DeploymentInfoContext result;

            PSCredential cred = new PSCredential(username, Utilities.convertToSecureString(password));
            string rdpPath = @".\WebRole1.rdp";
            string dns;

            try
            {

                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                Console.WriteLine("service, {0}, is created.", serviceName);

                ExtensionConfigurationInput config = vmPowershellCmdlets.NewAzureServiceRemoteDesktopExtensionConfig(cred);

                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false, config);

                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                pass = Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Production, null, 2);
                Console.WriteLine("successfully deployed the package");

                RemoteDesktopExtensionContext resultContext = vmPowershellCmdlets.GetAzureServiceRemoteDesktopExtension(serviceName)[0];

                VerifyRDPExtContext(resultContext, "AllRoles", "Default-RDP-Production-Ext-0", username, DateTime.Now.AddMonths(6));

                Utilities.GetDeploymentAndWaitForReady(serviceName, DeploymentSlotType.Production, 10, 600);

                vmPowershellCmdlets.GetAzureRemoteDesktopFile("WebRole1_IN_0", serviceName, rdpPath, false);

                using (StreamReader stream = new StreamReader(rdpPath))
                {
                    string firstLine = stream.ReadLine();
                    dns = Utilities.FindSubstring(firstLine, ':', 2);
                }

                Assert.IsTrue((Utilities.RDPtestPaaS(dns, "WebRole1", 0, username, password, true)), "Cannot RDP to the instance!!");

                vmPowershellCmdlets.RemoveAzureServiceRemoteDesktopExtension(serviceName);

                try
                {
                    vmPowershellCmdlets.GetAzureRemoteDesktopFile("WebRole1_IN_0", serviceName, rdpPath, false);
                    Assert.Fail("Succeeded, but extected to fail!");
                }
                catch (Exception e)
                {
                    if (e is AssertFailedException)
                    {
                        throw;
                    }
                    else
                    {
                        Console.WriteLine("Failed to get RDP file as expected");
                    }
                }

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                pass &= Utilities.CheckRemove(vmPowershellCmdlets.GetAzureDeployment, serviceName, DeploymentSlotType.Production);
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        // Disabled. Tracking Issue # 1479
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceRemoteDesktopExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void AzureServiceRemoteDesktopExtensionTest()
        {

            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Choose the package and config files from local machine
            string packageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            string configName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);
            var packagePath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + packageName);
            var configPath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + configName);

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            DeploymentInfoContext result;

            PSCredential cred = new PSCredential(username, Utilities.convertToSecureString(password));
            string rdpPath = @".\WebRole1.rdp";
            string dns;

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                Console.WriteLine("service, {0}, is created.", serviceName);

                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false);

                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                pass = Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Production, null, 2);
                Console.WriteLine("successfully deployed the package");

                vmPowershellCmdlets.SetAzureServiceRemoteDesktopExtension(serviceName, cred);

                RemoteDesktopExtensionContext resultContext = vmPowershellCmdlets.GetAzureServiceRemoteDesktopExtension(serviceName)[0];

                VerifyRDPExtContext(resultContext, "AllRoles", "Default-RDP-Production-Ext-0", username, DateTime.Now.AddMonths(6));

                vmPowershellCmdlets.GetAzureRemoteDesktopFile("WebRole1_IN_0", serviceName, rdpPath, false);

                using (StreamReader stream = new StreamReader(rdpPath))
                {
                    string firstLine = stream.ReadLine();
                    dns = Utilities.FindSubstring(firstLine, ':', 2);
                }

                Assert.IsTrue((Utilities.RDPtestPaaS(dns, "WebRole1", 0, username, password, true)), "Cannot RDP to the instance!!");

                vmPowershellCmdlets.RemoveAzureServiceRemoteDesktopExtension(serviceName, true);

                try
                {
                    vmPowershellCmdlets.GetAzureRemoteDesktopFile("WebRole1_IN_0", serviceName, rdpPath, false);
                    Assert.Fail("Succeeded, but extected to fail!");
                }
                catch (Exception e)
                {
                    if (e is AssertFailedException)
                    {
                        throw;
                    }
                    else
                    {
                        Console.WriteLine("Failed to get RDP file as expected");
                    }
                }

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                pass &= Utilities.CheckRemove(vmPowershellCmdlets.GetAzureDeployment, serviceName, DeploymentSlotType.Production);
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }


        /// <summary>
        /// Deploy an IaaS VM with Domain Join
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMDomainJoinTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            var newAzureVMName = Utilities.GetUniqueShortName(vmNamePrefix);

            const string joinDomainStr = "www.microsoft.com";
            const string domainStr = "microsoft.com";
            const string domainUser = "pstestdomainuser";
            const string domainPassword = "p@ssw0rd";

            try
            {
                if (string.IsNullOrEmpty(imageName))
                {
                    imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] {"Windows"}, false);
                }

                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);

                var azureVMConfigInfo = new AzureVMConfigInfo(newAzureVMName, InstanceSize.Small, imageName);
                var azureProvisioningConfig = new AzureProvisioningConfigInfo("WindowsDomain", username, password,
                                                                              joinDomainStr, domainStr, domainUser,
                                                                              domainPassword);
                var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null,
                                                                        null);
                PersistentVM persistentVM = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);

                PersistentVM[] VMs = {persistentVM};
                vmPowershellCmdlets.NewAzureVM(serviceName, VMs);

                // Todo: Check the domain of the VM
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        [TestCleanup]
        public virtual void CleanUp()
        {
            Console.WriteLine("Test {0}", pass ? "passed" : "failed");

            // Remove the service
            if ((cleanupIfPassed && pass) || (cleanupIfFailed && !pass))
            {
                vmPowershellCmdlets.RemoveAzureService(serviceName);
                try
                {
                    vmPowershellCmdlets.GetAzureService(serviceName);
                    Console.WriteLine("The service, {0}, is not removed", serviceName);
                }
                catch (Exception e)
                {
                    if (e.ToString().ToLowerInvariant().Contains("does not exist"))
                    {
                        Console.WriteLine("The service, {0}, is successfully removed", serviceName);
                    }
                    else
                    {
                        Console.WriteLine("Error occurred: {0}", e.ToString());
                    }
                }
            }
        }

        private string GetSiteContent(Uri uri, int maxRetryTimes, bool holdConnection)
        {
            Console.WriteLine("GetSiteContent. uri={0} maxRetryTimes={1}", uri.AbsoluteUri, maxRetryTimes);

            HttpWebRequest request;
            HttpWebResponse response = null;

            var noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            HttpWebRequest.DefaultCachePolicy = noCachePolicy;

            int i;
            for (i = 1; i <= maxRetryTimes; i++)
            {
                try
                {
                    request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Timeout = 10 * 60 * 1000; //set to 10 minutes, default 100 sec. default IE7/8 is 60 minutes
                    response = (HttpWebResponse)request.GetResponse();
                    break;
                }
                catch (WebException e)
                {
                    Console.WriteLine("Exception Message: " + e.Message);
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        Console.WriteLine("Status Code: {0}", ((HttpWebResponse)e.Response).StatusCode);
                        Console.WriteLine("Status Description: {0}", ((HttpWebResponse)e.Response).StatusDescription);
                    }
                }

                Thread.Sleep(30 * 1000);
            }

            if (i > maxRetryTimes)
            {
                throw new Exception("Web Site has error and reached maxRetryTimes");
            }

            Stream responseStream = response.GetResponseStream();
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[100];
            int length;
            while ((length = responseStream.Read(buf, 0, 100)) != 0)
            {
                if (holdConnection)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
                sb.Append(Encoding.UTF8.GetString(buf, 0, length));
            }

            string responseString = sb.ToString();
            Console.WriteLine("Site content: (IsFromCache={0})", response.IsFromCache);
            Console.WriteLine(responseString);

            return responseString;
        }

        private bool GetVNetState(string vnet, ProvisioningState expectedState, int maxTime, int intervalTime)
        {
            ProvisioningState vnetState;
            int i = 0;
            do
            {
                vnetState = vmPowershellCmdlets.GetAzureVNetGateway(vnet)[0].State;
                Thread.Sleep(intervalTime * 1000);
                i++;
            }
            while (!vnetState.Equals(expectedState) || i < maxTime);

            return vnetState.Equals(expectedState);
        }

        private bool VerifyDiagExtContext(DiagnosticExtensionContext resultContext, string role, string extID, string storage, XmlDocument config)
        {
            try
            {
                Assert.AreEqual(role, resultContext.Role.RoleType.ToString());
                Assert.AreEqual("Diagnostics", resultContext.Extension);
                Assert.AreEqual(extID, resultContext.Id);
                Assert.AreEqual(storage, resultContext.StorageAccountName);

                string inner = Utilities.GetInnerXml(resultContext.WadCfg, "WadCfg");
                Assert.IsTrue(Utilities.CompareWadCfg(inner, config));

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool VerifyRDPExtContext(RemoteDesktopExtensionContext resultContext, string role, string extID, string userName, DateTime exp)
        {
            try
            {
                Assert.AreEqual(role, resultContext.Role.RoleType.ToString());
                Assert.AreEqual("RDP", resultContext.Extension);
                Assert.AreEqual(extID, resultContext.Id);
                Assert.AreEqual(userName, resultContext.UserName);
                Assert.IsTrue(Utilities.CompareDateTime(exp, resultContext.Expiration));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
