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

namespace Microsoft.WindowsAzure.Commands.Test.CloudService.Utilities
{
    using Commands.Utilities.CloudService;
    using Commands.Utilities.Common;
    using Commands.Utilities.Properties;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CsPackTests : TestBase
    {
        private const string serviceName = "AzureService";

        [TestMethod]
        public void CreateLocalPackageWithOneNodeWebRoleTest()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                RoleInfo webRoleInfo = service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                string logsDir = Path.Combine(service.Paths.RootPath, webRoleInfo.Name, "server.js.logs");
                string logFile = Path.Combine(logsDir, "0.txt");
                string targetLogsFile = Path.Combine(service.Paths.LocalPackage, "roles", webRoleInfo.Name, @"approot\server.js.logs\0.txt");
                files.CreateDirectory(logsDir);
                files.CreateEmptyFile(logFile);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WebRole1\approot"), Path.Combine(Resources.NodeScaffolding, Resources.WebRole));
                Assert.IsTrue(File.Exists(targetLogsFile));
            }
        }

        [TestMethod]
        public void CreateLocalPackageWithOnePHPWebRoleTest()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                RoleInfo webRoleInfo = service.AddWebRole(Data.PHPWebRoleScaffoldingPath);
                string logsDir = Path.Combine(service.Paths.RootPath, webRoleInfo.Name, "server.js.logs");
                string logFile = Path.Combine(logsDir, "0.txt");
                string targetLogsFile = Path.Combine(service.Paths.LocalPackage, "roles", webRoleInfo.Name, @"approot\server.js.logs\0.txt");
                files.CreateDirectory(logsDir);
                files.CreateEmptyFile(logFile);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WebRole1\approot"), Path.Combine(Resources.PHPScaffolding, Resources.WebRole));
                Assert.IsTrue(File.Exists(targetLogsFile));
            }
        }

        [TestMethod]
        public void CreateLocalPackageWithNodeWorkerRoleTest()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWorkerRole(Data.NodeWorkerRoleScaffoldingPath);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WorkerRole1\approot"), Path.Combine(Resources.NodeScaffolding, Resources.WorkerRole));
            }
        }

        [TestMethod]
        public void CreateLocalPackageWithPHPWorkerRoleTest()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWorkerRole(Data.PHPWorkerRoleScaffoldingPath);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WorkerRole1\approot"), Path.Combine(Resources.PHPScaffolding, Resources.WorkerRole));
            }
        }

        [TestMethod]
        public void CreateLocalPackageWithMultipleRoles()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWorkerRole(Data.NodeWorkerRoleScaffoldingPath);
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                service.AddWorkerRole(Data.PHPWorkerRoleScaffoldingPath);
                service.AddWebRole(Data.PHPWebRoleScaffoldingPath);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WorkerRole1\approot"), Path.Combine(Resources.NodeScaffolding, Resources.WorkerRole));
                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WebRole1\approot"), Path.Combine(Resources.NodeScaffolding, Resources.WebRole));
                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WorkerRole2\approot"), Path.Combine(Resources.PHPScaffolding, Resources.WorkerRole));
                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WebRole2\approot"), Path.Combine(Resources.PHPScaffolding, Resources.WebRole));
            }
        }

        [TestMethod]
        public void CreateCloudPackageWithMultipleRoles()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                CloudServiceProject service = new CloudServiceProject(files.RootPath, serviceName, null);
                service.AddWorkerRole(Data.NodeWorkerRoleScaffoldingPath);
                service.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                service.AddWorkerRole(Data.PHPWorkerRoleScaffoldingPath);
                service.AddWebRole(Data.PHPWebRoleScaffoldingPath);
                service.CreatePackage(DevEnv.Cloud, out standardOutput, out standardError);

                using (Package package = Package.Open(service.Paths.CloudPackage))
                {
                    Assert.AreEqual(9, package.GetParts().Count());
                }
            }
        }
    }
}