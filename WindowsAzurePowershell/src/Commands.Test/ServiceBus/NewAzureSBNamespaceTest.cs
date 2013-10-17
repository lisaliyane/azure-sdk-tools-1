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
    using System;
    using System.Collections.Generic;
    using Commands.Utilities.Common;
    using Commands.ServiceBus;
    using Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.ServiceBus.Models;
    using Moq;
    using Microsoft.WindowsAzure.Management.ServiceBus;
    using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;

    [TestClass]
    public class NewAzureSBNamespaceTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
        }

        [TestMethod]
        public void NewAzureSBNamespaceSuccessfull()
        {
            // Setup
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();
            string name = "test";
            string location = "West US";
            NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand()
            {
                Name = name,
                Location = location,
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
            ExtendedServiceBusNamespace expected = new ExtendedServiceBusNamespace { Name = name, Region = location };
            client.Setup(f => f.CreateNamespace(name, location)).Returns(expected);
            client.Setup(f => f.GetServiceBusRegions()).Returns(new List<ServiceBusLocation>()
            {
                new ServiceBusLocation () { Code = location }
            });

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ExtendedServiceBusNamespace actual = mockCommandRuntime.OutputPipeline[0] as ExtendedServiceBusNamespace;
            Assert.AreEqual<ExtendedServiceBusNamespace>(expected, actual);
        }

        [TestMethod]
        public void NewAzureSBNamespaceGetsDefaultLocation()
        {
            // Setup
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            string name = "test";
            string location = "West US";
            NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand()
            {
                Name = name,
                CommandRuntime = mockCommandRuntime,
                Client = client.Object,
                Location = location
            };
            ExtendedServiceBusNamespace expected = new ExtendedServiceBusNamespace { Name = name, Region = location };
            client.Setup(f => f.CreateNamespace(name, location)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ExtendedServiceBusNamespace actual = mockCommandRuntime.OutputPipeline[0] as ExtendedServiceBusNamespace;
            Assert.AreEqual<ExtendedServiceBusNamespace>(expected, actual);
        }

        [TestMethod]
        public void NewAzureSBNamespaceWithInvalidNamesFail()
        {
            // Setup
            string[] invalidNames = { "1test", "test#", "test invaid", "-test", "_test" };

            foreach (string invalidName in invalidNames)
            {
                MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
                NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand() { Name = invalidName, Location = "West US", CommandRuntime = mockCommandRuntime };
                string expected = string.Format("{0}\r\nParameter name: Name", string.Format(Resources.InvalidNamespaceName, invalidName));

                Testing.AssertThrows<ArgumentException>(() => cmdlet.ExecuteCmdlet(), expected);
            }
        }

        [TestMethod]
        public void CreatesNewSBCaseInsensitiveRegion()
        {
            // Setup
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();
            string name = "test";
            string location = "West US";
            NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand()
            {
                Name = name,
                Location = location.ToLower(),
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
            ExtendedServiceBusNamespace expected = new ExtendedServiceBusNamespace { Name = name, Region = location };
            client.Setup(f => f.CreateNamespace(name, location.ToLower())).Returns(expected);
            client.Setup(f => f.GetServiceBusRegions()).Returns(new List<ServiceBusLocation>()
            {
                new ServiceBusLocation () { Code = location }
            });

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ExtendedServiceBusNamespace actual = mockCommandRuntime.OutputPipeline[0] as ExtendedServiceBusNamespace;
            Assert.AreEqual<ExtendedServiceBusNamespace>(expected, actual);
        }
    }
}