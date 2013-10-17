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
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.ServiceBus;
    using Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
    using Microsoft.WindowsAzure.Management.ServiceBus.Models;

    [TestClass]
    public class GetAzureSBNamespaceTests : TestBase
    {
        Mock<ServiceBusClientExtensions> client;
        MockCommandRuntime mockCommandRuntime;
        GetAzureSBNamespaceCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
            client = new Mock<ServiceBusClientExtensions>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureSBNamespaceCommand()
            {
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
        }

        [TestMethod]
        public void GetAzureSBNamespaceSuccessfull()
        {
            // Setup
            string name = "test";
            cmdlet.Name = name;
            ExtendedServiceBusNamespace expected = new ExtendedServiceBusNamespace { Name = name };
            client.Setup(f => f.GetNamespace(name)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ExtendedServiceBusNamespace actual = mockCommandRuntime.OutputPipeline[0] as ExtendedServiceBusNamespace;
            Assert.AreEqual<string>(expected.Name, actual.Name);
        }

        [TestMethod]
        public void ListNamespacesSuccessfull()
        {
            // Setup
            string name1 = "test1";
            string name2 = "test2";
            List<ExtendedServiceBusNamespace> expected = new List<ExtendedServiceBusNamespace>();
            expected.Add(new ExtendedServiceBusNamespace { Name = name1 });
            expected.Add(new ExtendedServiceBusNamespace { Name = name2 });
            client.Setup(f => f.GetNamespace()).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            List<ExtendedServiceBusNamespace> actual = mockCommandRuntime.OutputPipeline[0] as List<ExtendedServiceBusNamespace>;
            Assert.AreEqual<int>(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual<string>(expected[i].Name, actual[i].Name);
            }
        }

        [TestMethod]
        public void GetAzureSBNamespaceWithInvalidNamesFail()
        {
            // Setup
            string[] invalidNames = { "1test", "test#", "test invaid", "-test", "_test" };

            foreach (string invalidName in invalidNames)
            {
                MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
                GetAzureSBNamespaceCommand cmdlet = new GetAzureSBNamespaceCommand() { Name = invalidName, CommandRuntime = mockCommandRuntime };
                string expected = string.Format("{0}\r\nParameter name: Name", string.Format(Resources.InvalidNamespaceName, invalidName));

                Testing.AssertThrows<ArgumentException>(() => cmdlet.ExecuteCmdlet(), expected);
            }
        }
    }
}