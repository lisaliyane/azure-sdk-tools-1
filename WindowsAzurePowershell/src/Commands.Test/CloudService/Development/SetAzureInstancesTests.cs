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
    using Commands.Utilities.Properties;
    using System;
    using System.IO;
    using System.Management.Automation;
    using Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SetAzureInstancesTests : TestBase
    {
        private const string serviceName = "AzureService";

        private MockCommandRuntime mockCommandRuntime;

        private SetAzureServiceProjectRoleCommand cmdlet;

        [TestInitialize]
        public void TestSetup()
        {
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new SetAzureServiceProjectRoleCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.PassThru = true;
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsNode()
        {
            int newRoleInstances = 10;

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                cmdlet.PassThru = false;
                RoleSettings roleSettings = cmdlet.SetAzureInstancesProcess("WebRole1", newRoleInstances, service.Paths.RootPath);
                service = new CloudServiceProject(service.Paths.RootPath, null);

                Assert.AreEqual<int>(newRoleInstances, service.Components.CloudConfig.Role[0].Instances.count);
                Assert.AreEqual<int>(newRoleInstances, service.Components.LocalConfig.Role[0].Instances.count);
                Assert.AreEqual<int>(0, mockCommandRuntime.OutputPipeline.Count);
                Assert.AreEqual<int>(newRoleInstances, roleSettings.Instances.count);
                Assert.AreEqual<string>(roleName, roleSettings.name);

            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsPHP()
        {
            int newRoleInstances = 10;

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Data.PHPWebRoleScaffoldingPath);
                RoleSettings roleSettings = cmdlet.SetAzureInstancesProcess("WebRole1", newRoleInstances, service.Paths.RootPath);
                service = new CloudServiceProject(service.Paths.RootPath, null);

                Assert.AreEqual<int>(newRoleInstances, service.Components.CloudConfig.Role[0].Instances.count);
                Assert.AreEqual<int>(newRoleInstances, service.Components.LocalConfig.Role[0].Instances.count);
                Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[0]).Members[Parameters.RoleName].Value.ToString());
                Assert.IsTrue(((PSObject)mockCommandRuntime.OutputPipeline[0]).TypeNames.Contains(typeof(RoleSettings).FullName));
                Assert.AreEqual<int>(newRoleInstances, roleSettings.Instances.count);
                Assert.AreEqual<string>(roleName, roleSettings.name);
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsRoleNameDoesNotExistFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, roleName, 10), string.Format(Resources.RoleNotFoundMessage, roleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsNodeRoleNameDoesNotExistServiceContainsWebRoleFail()
        {
            string roleName = "WebRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, invalidRoleName, 10), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsPHPRoleNameDoesNotExistServiceContainsWebRoleFail()
        {
            string roleName = "WebRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWebRole(Data.PHPWebRoleScaffoldingPath, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, invalidRoleName, 10), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsNodeRoleNameDoesNotExistServiceContainsWorkerRoleFail()
        {
            string roleName = "WorkerRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWorkerRole(Data.NodeWorkerRoleScaffoldingPath, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, invalidRoleName, 10), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsPHPRoleNameDoesNotExistServiceContainsWorkerRoleFail()
        {
            string roleName = "WorkerRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWorkerRole(Data.PHPWorkerRoleScaffoldingPath, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, invalidRoleName, 10), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsEmptyRoleNameFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, string.Empty, 10), string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.RoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsNullRoleNameFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, null, 10), string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.RoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsLargeRoleInstanceFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, roleName, 2000), string.Format(Resources.InvalidInstancesCount, roleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessNegativeRoleInstanceFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, roleName, -1), string.Format(Resources.InvalidInstancesCount, roleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsCaseInsensitive()
        {
            int newRoleInstances = 10;

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                cmdlet.PassThru = false;
                RoleSettings roleSettings = cmdlet.SetAzureInstancesProcess("WeBrolE1", newRoleInstances, service.Paths.RootPath);
                service = new CloudServiceProject(service.Paths.RootPath, null);

                Assert.AreEqual<int>(newRoleInstances, service.Components.CloudConfig.Role[0].Instances.count);
                Assert.AreEqual<int>(newRoleInstances, service.Components.LocalConfig.Role[0].Instances.count);
                Assert.AreEqual<int>(0, mockCommandRuntime.OutputPipeline.Count);
                Assert.AreEqual<int>(newRoleInstances, roleSettings.Instances.count);
                Assert.AreEqual<string>(roleName, roleSettings.name);

            }
        }

        [TestMethod]
        public void SetAzureServiceProjectRoleWithoutPassingRoleName()
        {
            string originalDirectory = Directory.GetCurrentDirectory();
            string serviceName = "AzureService1";
            CloudServiceProject service = new CloudServiceProject(Directory.GetCurrentDirectory(), serviceName, null);
            service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
            Directory.SetCurrentDirectory(Path.Combine(service.Paths.RootPath, "WebRole1"));
            cmdlet.RoleName = string.Empty;
            cmdlet.ExecuteCmdlet();
            service = new CloudServiceProject(service.Paths.RootPath, null);

            Assert.AreEqual<string>("WebRole1", cmdlet.RoleName);
            Directory.SetCurrentDirectory(originalDirectory);
        }

        [TestMethod]
        public void SetAzureServiceProjectRoleInDeepDirectory()
        {
            string originalDirectory = Directory.GetCurrentDirectory();
            string serviceName = "AzureService2";
            CloudServiceProject service = new CloudServiceProject(Directory.GetCurrentDirectory(), serviceName, null);
            service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
            Directory.SetCurrentDirectory(Path.Combine(service.Paths.RootPath, "WebRole1", "bin"));
            cmdlet.RoleName = string.Empty;
            cmdlet.ExecuteCmdlet();
            service = new CloudServiceProject(service.Paths.RootPath, null);

            Assert.AreEqual<string>("WebRole1", cmdlet.RoleName);
            Directory.SetCurrentDirectory(originalDirectory);
        }

        [TestMethod]
        public void SetAzureServiceProjectRoleInServiecRootDirectoryFail()
        {
            string serviceName = "AzureService3";
            CloudServiceProject service = new CloudServiceProject(Directory.GetCurrentDirectory(), serviceName, null);
            service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
            cmdlet.RoleName = string.Empty;
            Testing.AssertThrows<InvalidOperationException>(() => cmdlet.ExecuteCmdlet(), Resources.CannotFindServiceRoot);
        }
    }
}