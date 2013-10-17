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

namespace Microsoft.WindowsAzure.Commands.Test.CloudService.Development.Scaffolding.Cmdlet
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using Commands.CloudService.Development.Scaffolding;
    using Test.Utilities.Common;
    using Commands.Utilities.Properties;
    using Commands.Utilities.CloudService;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NewAzureServiceTests : TestBase
    {
        NewAzureServiceProjectCommand cmdlet;

        MockCommandRuntime mockCommandRuntime;

        [TestInitialize]
        public void SetupTest()
        {
            cmdlet = new NewAzureServiceProjectCommand();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void NewAzureServiceSuccessfull()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Setup
                string expectedName = "test";
                string expectedRootPath = Path.Combine(files.RootPath, expectedName);
                string expectedServiceCreatedMessage = string.Format(Resources.NewServiceCreatedMessage, expectedRootPath);
                cmdlet.ServiceName = expectedName;

                // Test
                cmdlet.NewAzureServiceProcess(files.RootPath, expectedName);

                // Assert
                PSObject actualPSObject = mockCommandRuntime.OutputPipeline[0] as PSObject;
                string actualServiceCreatedMessage = mockCommandRuntime.VerboseStream[0];
                
                Assert.AreEqual<string>(expectedName, actualPSObject.Members[Parameters.ServiceName].Value.ToString());
                Assert.AreEqual<string>(expectedRootPath, actualPSObject.Members[Parameters.RootPath].Value.ToString());
                Assert.AreEqual<string>(expectedServiceCreatedMessage, actualServiceCreatedMessage);
                AzureAssert.AzureServiceExists(expectedRootPath, Resources.GeneralScaffolding, expectedName);
            }
        }

        [TestMethod]
        public void NewAzureServiceWithInvalidNames()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                foreach (string name in Data.InvalidServiceNames)
                {
                    cmdlet.ServiceName = name;
                    Testing.AssertThrows<ArgumentException>(() => cmdlet.ExecuteCmdlet());
                }
            }
        }
    }
}