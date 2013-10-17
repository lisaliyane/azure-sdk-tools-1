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

namespace Microsoft.WindowsAzure.Commands.Test.CloudService.Development.Tests.Cmdlet
{
    using Commands.CloudService.Development;
    using Commands.Utilities.CloudService;
    using Commands.Utilities.Common.XmlSchema.ServiceConfigurationSchema;
    using System.IO;
    using System.Management.Automation;
    using Test.Utilities.CloudService;
    using Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SetAzureRuntimeTests : TestBase
    {
        private MockCommandRuntime mockCommandRuntime;

        private SetAzureServiceProjectRoleCommand cmdlet;

        private const string serviceName = "AzureService";

        public static void VerifyPackageJsonVersion(string rootPath, string roleName, string runtime, string version)
        {
            string packagePath = Path.Combine(rootPath, roleName);
            string actualVersion;
            Assert.IsTrue(JavaScriptPackageHelpers.TryGetEngineVersion(packagePath, runtime, out actualVersion));
            Assert.AreEqual(version, actualVersion, true);
        }

        public static void VerifyInvalidPackageJsonVersion(string rootPath, string roleName, string runtime, string version)
        {
            string packagePath = Path.Combine(rootPath, roleName);
            string actualVersion;
            Assert.IsFalse(JavaScriptPackageHelpers.TryGetEngineVersion(packagePath, runtime, out actualVersion));
        }

        [TestInitialize]
        public void TestSetup()
        {
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new SetAzureServiceProjectRoleCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.PassThru = true;
        }

        /// <summary>
        /// Verify that adding valid role runtimes results in valid changes in the commandlet scaffolding 
        /// (in this case, valid package.json changes).  Test for both a valid node runtiem version and 
        /// valid iisnode runtiem version
        /// </summary>
        [TestMethod]
        public void TestSetAzureRuntimeValidRuntimeVersions()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                string roleName = "WebRole1";
                cmdlet.PassThru = false;
                
                RoleSettings roleSettings1 = cmdlet.SetAzureRuntimesProcess(roleName, "node", "0.8.2", service.Paths.RootPath, RuntimePackageHelper.GetTestManifest(files));
                RoleSettings roleSettings2 = cmdlet.SetAzureRuntimesProcess(roleName, "iisnode", "0.1.21", service.Paths.RootPath, RuntimePackageHelper.GetTestManifest(files));
                VerifyPackageJsonVersion(service.Paths.RootPath, roleName, "node", "0.8.2");
                VerifyPackageJsonVersion(service.Paths.RootPath, roleName, "iisnode", "0.1.21");
                Assert.AreEqual<int>(0, mockCommandRuntime.OutputPipeline.Count);
                Assert.AreEqual<string>(roleName, roleSettings1.name);
                Assert.AreEqual<string>(roleName, roleSettings2.name);
            }
        }

        /// <summary>
        /// Test that attempting to set an invlaid runtime version (one that is not listed in the runtime manifest) 
        /// results in no changes to package scaffolding (no changes in package.json)
        /// </summary>
        [TestMethod]
        public void TestSetAzureRuntimeInvalidRuntimeVersion()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                string roleName = "WebRole1";
                RoleSettings roleSettings1 = cmdlet.SetAzureRuntimesProcess(roleName, "node", "0.8.99", service.Paths.RootPath, RuntimePackageHelper.GetTestManifest(files));
                RoleSettings roleSettings2 = cmdlet.SetAzureRuntimesProcess(roleName, "iisnode", "0.9.99", service.Paths.RootPath, RuntimePackageHelper.GetTestManifest(files));
                VerifyInvalidPackageJsonVersion(service.Paths.RootPath, roleName, "node", "*");
                VerifyInvalidPackageJsonVersion(service.Paths.RootPath, roleName, "iisnode", "*");
                Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[0]).Members[Parameters.RoleName].Value.ToString());
                Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[1]).Members[Parameters.RoleName].Value.ToString());
                Assert.IsTrue(((PSObject)mockCommandRuntime.OutputPipeline[0]).TypeNames.Contains(typeof(RoleSettings).FullName));
                Assert.IsTrue(((PSObject)mockCommandRuntime.OutputPipeline[1]).TypeNames.Contains(typeof(RoleSettings).FullName));
                Assert.AreEqual<string>(roleName, roleSettings1.name);
                Assert.AreEqual<string>(roleName, roleSettings2.name);
            }
        }

        /// <summary>
        /// Test that attempting to add a runtime with an invlid runtime type (a runtime type that has no entries in the 
        /// master package.json).  Results in no scaffolding changes - no changes to package.json.
        /// </summary>
        [TestMethod]
        public void TestSetAzureRuntimeInvalidRuntimeType()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                RoleSettings roleSettings1 = cmdlet.SetAzureRuntimesProcess(roleName, "noide", "0.8.99", service.Paths.RootPath, RuntimePackageHelper.GetTestManifest(files));
                RoleSettings roleSettings2 = cmdlet.SetAzureRuntimesProcess(roleName, "iisnoide", "0.9.99", service.Paths.RootPath, RuntimePackageHelper.GetTestManifest(files));
                VerifyInvalidPackageJsonVersion(service.Paths.RootPath, roleName, "node", "*");
                VerifyInvalidPackageJsonVersion(service.Paths.RootPath, roleName, "iisnode", "*");
                Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[0]).Members[Parameters.RoleName].Value.ToString());
                Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[1]).Members[Parameters.RoleName].Value.ToString());
                Assert.IsTrue(((PSObject)mockCommandRuntime.OutputPipeline[0]).TypeNames.Contains(typeof(RoleSettings).FullName));
                Assert.IsTrue(((PSObject)mockCommandRuntime.OutputPipeline[1]).TypeNames.Contains(typeof(RoleSettings).FullName));
                Assert.AreEqual<string>(roleName, roleSettings1.name);
                Assert.AreEqual<string>(roleName, roleSettings2.name);
            }
        }

        /// <summary>
        /// Verify that adding valid role runtimes results in valid changes in the commandlet scaffolding 
        /// (in this case, valid package.json changes).  Test for both a valid node runtiem version and 
        /// valid iisnode runtiem version
        /// </summary>
        [TestMethod]
        public void TestSetAzureRuntimeValidRuntimeVersionsCanInsensitive()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                string roleName = "WebRole1";
                string caseInsensitiveName = "weBrolE1";
                cmdlet.PassThru = false;

                RoleSettings roleSettings1 = cmdlet.SetAzureRuntimesProcess(caseInsensitiveName, "node", "0.8.2", service.Paths.RootPath, RuntimePackageHelper.GetTestManifest(files));
                RoleSettings roleSettings2 = cmdlet.SetAzureRuntimesProcess(caseInsensitiveName, "iisnode", "0.1.21", service.Paths.RootPath, RuntimePackageHelper.GetTestManifest(files));
                VerifyPackageJsonVersion(service.Paths.RootPath, roleName, "node", "0.8.2");
                VerifyPackageJsonVersion(service.Paths.RootPath, roleName, "iisnode", "0.1.21");
                Assert.AreEqual<int>(0, mockCommandRuntime.OutputPipeline.Count);
                Assert.AreEqual<string>(roleName, roleSettings1.name);
                Assert.AreEqual<string>(roleName, roleSettings2.name);
            }
        }
    }
}