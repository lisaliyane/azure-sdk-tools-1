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
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.ServiceBus;
    using Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;

    [TestClass]
    public class RemoveAzureSBNamespaceTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
        }

        [TestMethod]
        public void RemoveAzureSBNamespaceSuccessfull()
        {
            // Setup
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            string name = "test";
            RemoveAzureSBNamespaceCommand cmdlet = new RemoveAzureSBNamespaceCommand()
            {
                Name = name,
                CommandRuntime = mockCommandRuntime,
                PassThru = true,
                Client = client.Object
            };
            bool deleted = false;
            client.Setup(f => f.RemoveNamespace(name)).Callback(() => deleted = true);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            Assert.IsTrue(deleted);
            Assert.IsTrue((bool)mockCommandRuntime.OutputPipeline[0]);
        }
    }
}