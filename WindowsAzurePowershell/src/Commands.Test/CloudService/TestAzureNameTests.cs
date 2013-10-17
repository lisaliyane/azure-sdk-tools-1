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

namespace Microsoft.WindowsAzure.Commands.Test.CloudService
{
    using Commands.Utilities.Common;
    using Commands.CloudService;
    using Test.Utilities.CloudService;
    using Test.Utilities.Common;
    using ServiceManagement;
    using VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
    using Microsoft.WindowsAzure.Commands.Utilities.CloudService;
    using Microsoft.WindowsAzure.Management.Compute.Models;

    [TestClass]
    public class TestAzureNameTests : TestBase
    {
        private Mock<ICloudServiceClient> clientMock;
        MockCommandRuntime mockCommandRuntime;
        TestAzureNameCommand cmdlet;
        Mock<ServiceBusClientExtensions> serviceBusClient;
        string subscriptionId = "my subscription Id";

        [TestInitialize]
        public void SetupTest()
        {
            clientMock = new Mock<ICloudServiceClient>();
            
            mockCommandRuntime = new MockCommandRuntime();
            serviceBusClient = new Mock<ServiceBusClientExtensions>();
            cmdlet = new TestAzureNameCommand()
            {
                CommandRuntime = mockCommandRuntime,
                ServiceBusClient = serviceBusClient.Object,
                CloudServiceClient = clientMock.Object
            };
        }

        [TestMethod]
        public void TestAzureServiceNameUsed()
        {
            string name = "test";
            clientMock.Setup(f => f.CheckHostedServiceNameAvailability(name)).Returns(false);

            cmdlet.IsDNSAvailable(null, name);

            bool actual = (bool)mockCommandRuntime.OutputPipeline[0];
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TestAzureServiceNameIsNotUsed()
        {
            string name = "test";
            clientMock.Setup(f => f.CheckHostedServiceNameAvailability(name)).Returns(true);

            cmdlet.IsDNSAvailable(null, name);

            bool actual = (bool)mockCommandRuntime.OutputPipeline[0];
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void TestAzureStorageNameUsed()
        {
            string name = "test";
            clientMock.Setup(f => f.CheckStorageServiceAvailability(name)).Returns(false);

            cmdlet.IsStorageServiceAvailable(null, name);

            bool actual = (bool)mockCommandRuntime.OutputPipeline[0];
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TestAzureStorageNameIsNotUsed()
        {
            string name = "test";
            clientMock.Setup(f => f.CheckStorageServiceAvailability(name)).Returns(true);

            cmdlet.IsStorageServiceAvailable(null, name);

            bool actual = (bool)mockCommandRuntime.OutputPipeline[0];
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void TestAzureServiceBusNamespaceUsed()
        {
            string name = "test";
            serviceBusClient.Setup(f => f.IsAvailableNamespace(name)).Returns(false);

            cmdlet.IsServiceBusNamespaceAvailable(subscriptionId, name);

            bool actual = (bool)mockCommandRuntime.OutputPipeline[0];

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TestAzureServiceBusNamespaceIsNotUsed()
        {
            string name = "test";
            serviceBusClient.Setup(f => f.IsAvailableNamespace(name)).Returns(true);

            cmdlet.IsServiceBusNamespaceAvailable(subscriptionId, name);

            bool actual = (bool)mockCommandRuntime.OutputPipeline[0];
            
            Assert.IsFalse(actual);
        }
    }
}
