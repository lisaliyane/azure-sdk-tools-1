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

namespace Microsoft.WindowsAzure.Commands.Test.Websites
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Utilities.Websites;
    using Commands.Utilities.Websites;
    using Commands.Websites;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetAzureWebsiteLocationTests : WebsitesTestBase
    {
        [TestMethod]
        public void ProcessGetAzureWebsiteLocationTest()
        {
            // Setup
            Mock<IWebsitesClient> clientMock = new Mock<IWebsitesClient>();
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            List<string> regions = new List<string>() {"West US", "North Moon", "Central West Sun"};
            clientMock.Setup(f => f.ListAvailableLocations()).Returns(regions);

            // Test
            GetAzureWebsiteLocationCommand getAzureWebsiteCommand = new GetAzureWebsiteLocationCommand()
            {
                WebsitesClient = clientMock.Object,
                CommandRuntime = commandRuntimeMock.Object
            };

            getAzureWebsiteCommand.ExecuteCmdlet();

            clientMock.Verify(f => f.ListAvailableLocations(), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(regions, true), Times.Once());
        }
    }
}
