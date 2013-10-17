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

namespace Microsoft.WindowsAzure.Commands.Test.ServiceBus
{
    using System.Collections.Generic;
    using Commands.Utilities.Common;
    using Commands.ServiceBus;
    using Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
    using Microsoft.WindowsAzure.Management.ServiceBus.Models;

    [TestClass]
    public class GetAzureSBLocationTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
        }

        [TestMethod]
        public void GetAzureSBLocationSuccessfull()
        {
            // Setup
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            string name = "test";
            GetAzureSBLocationCommand cmdlet = new GetAzureSBLocationCommand()
            {
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
            List<ServiceBusLocation> expected = new List<ServiceBusLocation>();
            expected.Add(new ServiceBusLocation { Code = name, FullName = name });
            client.Setup(f => f.GetServiceBusRegions()).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            List<ServiceBusLocation> actual = mockCommandRuntime.OutputPipeline[0] as List<ServiceBusLocation>;
            Assert.AreEqual<int>(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual<string>(expected[i].Code, actual[i].Code);
                Assert.AreEqual<string>(expected[i].FullName, actual[i].FullName);
            }
        }
    }
}