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
    using Commands.Utilities.Common.XmlSchema.ServiceDefinitionSchema;
    using Commands.Utilities.Properties;
    using System;
    using System.Management.Automation;
    using Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SetAzureVMSizeTests : TestBase
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
        public void SetAzureVMSizeProcessTestsNode()
        {
            string newRoleVMSize = RoleSize.Large.ToString();

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                cmdlet.PassThru = false;
                RoleSettings roleSettings = cmdlet.SetAzureVMSizeProcess("WebRole1", newRoleVMSize, service.Paths.RootPath);
                service = new CloudServiceProject(service.Paths.RootPath, null);

                Assert.AreEqual<string>(newRoleVMSize, service.Components.Definition.WebRole[0].vmsize.ToString());
                Assert.AreEqual<int>(0, mockCommandRuntime.OutputPipeline.Count);
                Assert.AreEqual<string>(roleName, roleSettings.name);
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsPHP()
        {
            string newRoleVMSize = RoleSize.Medium.ToString();

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Data.PHPWebRoleScaffoldingPath);
                RoleSettings roleSettings = cmdlet.SetAzureVMSizeProcess("WebRole1", newRoleVMSize, service.Paths.RootPath);
                service = new CloudServiceProject(service.Paths.RootPath, null);

                
                Assert.AreEqual<string>(newRoleVMSize, service.Components.Definition.WebRole[0].vmsize.ToString());
                Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[0]).Members[Parameters.RoleName].Value.ToString());
                Assert.IsTrue(((PSObject)mockCommandRuntime.OutputPipeline[0]).TypeNames.Contains(typeof(RoleSettings).FullName));
                Assert.AreEqual<string>(roleName, roleSettings.name);
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsRoleNameDoesNotExistFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, roleName, RoleSize.Medium.ToString()), string.Format(Resources.RoleNotFoundMessage, roleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsNodeRoleNameDoesNotExistServiceContainsWebRoleFail()
        {
            string roleName = "WebRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, invalidRoleName, RoleSize.Large.ToString()), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsPHPRoleNameDoesNotExistServiceContainsWebRoleFail()
        {
            string roleName = "WebRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWebRole(Data.PHPWebRoleScaffoldingPath, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, invalidRoleName, RoleSize.Large.ToString()), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsNodeRoleNameDoesNotExistServiceContainsWorkerRoleFail()
        {
            string roleName = "WorkerRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWorkerRole(Data.NodeWorkerRoleScaffoldingPath, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, invalidRoleName, RoleSize.Large.ToString()), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsPHPRoleNameDoesNotExistServiceContainsWorkerRoleFail()
        {
            string roleName = "WorkerRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWorkerRole(Data.PHPWorkerRoleScaffoldingPath, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, invalidRoleName, RoleSize.Large.ToString()), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsEmptyRoleNameFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, string.Empty, RoleSize.Large.ToString()), string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.RoleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsNullRoleNameFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, null, RoleSize.Large.ToString()), string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.RoleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsLargeRoleInstanceFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, roleName, "Gigantic"), string.Format(Resources.InvalidVMSize, roleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessNegativeRoleInstanceFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleVMSize(service.Paths, roleName, string.Empty), string.Format(Resources.InvalidVMSize, roleName));
            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsCaseInsensitive()
        {
            string newRoleVMSize = RoleSize.Large.ToString();

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                cmdlet.PassThru = false;
                RoleSettings roleSettings = cmdlet.SetAzureVMSizeProcess("WeBrolE1", newRoleVMSize, service.Paths.RootPath);
                service = new CloudServiceProject(service.Paths.RootPath, null);

                
                Assert.AreEqual<string>(newRoleVMSize, service.Components.Definition.WebRole[0].vmsize.ToString());
                Assert.AreEqual<int>(0, mockCommandRuntime.OutputPipeline.Count);
                Assert.AreEqual<string>(roleName, roleSettings.name);

            }
        }

        [TestMethod]
        public void SetAzureVMSizeProcessTestsCaseInsensitiveVMSizeSize()
        {
            string newRoleVMSize = "ExTraLaRge";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                cmdlet.PassThru = false;
                RoleSettings roleSettings = cmdlet.SetAzureVMSizeProcess("WebRole1", newRoleVMSize, service.Paths.RootPath);
                service = new CloudServiceProject(service.Paths.RootPath, null);


                Assert.AreEqual<string>(newRoleVMSize.ToLower(), service.Components.Definition.WebRole[0].vmsize.ToString().ToLower());
                Assert.AreEqual<int>(0, mockCommandRuntime.OutputPipeline.Count);
                Assert.AreEqual<string>(roleName, roleSettings.name);

            }
        }
    }
}