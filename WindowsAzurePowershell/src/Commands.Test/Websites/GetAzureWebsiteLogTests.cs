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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Websites;
    using Commands.Utilities.Websites;
    using Commands.Utilities.Websites.Services;
    using Commands.Utilities.Websites.Services.DeploymentEntities;
    using Commands.Utilities.Websites.Services.WebEntities;
    using Commands.Websites;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetAzureWebsiteLogTests : WebsitesTestBase
    {

        private Mock<ICommandRuntime> commandRuntimeMock;

        private Mock<IWebsitesClient> websitesClientMock;

        private GetAzureWebsiteLogCommand getAzureWebsiteLogCmdlet;

        private string websiteName = "TestWebsiteName";

        private string repoUrl = "TheRepoUrl";

        private List<string> logs;

        private Site website;

        Predicate<string> stopCondition;

        [TestInitialize]
        public override void SetupTest()
        {
            base.SetupTest();
            websitesClientMock = new Mock<IWebsitesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            stopCondition = (string line) => line != null;
            websitesClientMock.Setup(f => f.StartLogStreaming(
                websiteName,
                string.Empty,
                string.Empty,
                stopCondition,
                It.IsAny<int>()))
                .Returns(logs);
            logs = new List<string>() { "Log1", "Error: Log2", "Log3", "Error: Log4", null };
            getAzureWebsiteLogCmdlet = new GetAzureWebsiteLogCommand(null)
            {
                CommandRuntime = commandRuntimeMock.Object,
                WebsitesClient = websitesClientMock.Object,
                StopCondition = stopCondition,
                Name = websiteName,
                ShareChannel = true
            };
            website = new Site()
            {
                Name = websiteName,
                WebSpace = "webspaceName",
                SiteProperties = new SiteProperties()
                {
                    Properties = new List<NameValuePair>()
                    {
                        new NameValuePair() { Name = UriElements.RepositoryUriProperty, Value = repoUrl }
                    }
                }
            };
            Cache.AddSite(getAzureWebsiteLogCmdlet.CurrentSubscription.SubscriptionId, website);
            websitesClientMock.Setup(c => c.GetWebsite(websiteName))
                .Returns(website);
        }

        [TestMethod]
        public void GetAzureWebsiteLogTest()
        {
            getAzureWebsiteLogCmdlet.Tail = true;

            getAzureWebsiteLogCmdlet.ExecuteCmdlet();

            websitesClientMock.Verify(f => f.StartLogStreaming(
                websiteName,
                null,
                null,
                stopCondition,
                It.IsAny<int>()),
                Times.Once());
        }

        [TestMethod]
        public void CanGetAzureWebsiteLogWithPath()
        {
            getAzureWebsiteLogCmdlet.Tail = true;
            getAzureWebsiteLogCmdlet.Path = "http";

            getAzureWebsiteLogCmdlet.ExecuteCmdlet();

            websitesClientMock.Verify(f => f.StartLogStreaming(
                websiteName,
                "http",
                null,
                stopCondition,
                It.IsAny<int>()),
                Times.Once());
        }

        [TestMethod]
        public void TestGetAzureWebsiteLogListPath()
        {
            List<LogPath> paths = new List<LogPath>() { 
                new LogPath() { Name = "http" }, new LogPath() { Name = "Git" }
            };
            List<string> expected = new List<string>() { "http", "Git" };
            List<string> actual = new List<string>();
            websitesClientMock.Setup(f => f.ListLogPaths(websiteName)).Returns(paths);
            commandRuntimeMock.Setup(f => f.WriteObject(It.IsAny<IEnumerable<string>>(), true))
                .Callback<object, bool>((o, b) => actual = actual = ((IEnumerable<string>)o).ToList<string>());
            getAzureWebsiteLogCmdlet.ListPath = true;

            getAzureWebsiteLogCmdlet.ExecuteCmdlet();

            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
