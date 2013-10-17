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

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.ServiceManagemenet
{
    using Common;
    using VisualStudio.TestTools.UnitTesting;
    using ServiceManagement.Test.Properties;
    using ServiceManagement.Test.FunctionalTests;
    using ServiceManagement.Model;
    using ServiceManagement.Test.FunctionalTests.ConfigDataInfo;

    [TestClass]
    public class VMProvisionScenarioTests : WindowsAzurePowerShellTest
    {
        public VMProvisionScenarioTests()
            : base("CloudService\\Common.ps1",
                   "ServiceManagement\\Common.ps1",
                   "ServiceManagement\\VMProvisionTests.ps1"
            )
        {

        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceManagement)]
        [TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,Get-AzureVM,Get-AzureLocation,Import-AzurePublishSettingsFile,Get-AzureSubscription,Set-AzureSubscription)")]
        [Ignore] // https://github.com/WindowsAzure/azure-sdk-tools/issues/1402
        public void NewWindowsAzureQuickVM()
        {
            powershell.Invoke();

            ServiceManagementCmdletTestHelper vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            
            string imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
            string locationName = vmPowershellCmdlets.GetAzureLocationName(new[] { Resource.Location });

            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");

            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "pstestuser", "p@ssw0rd", locationName);

            // Verify
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);

            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));
        }
    }
}
