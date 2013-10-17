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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.UnitTests.Cmdlets.IaaS.PersistentVMs
{
    using System;
    using VisualStudio.TestTools.UnitTesting;
    using Commands.Test.Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using System.Collections.Generic;
    using System.Linq;
    using Commands.ServiceManagement.IaaS;

    ////[TestClass]
    public class GetAzureWinRMUriTests : TestBase
    {
        private Uri deploymentUrl;
        private string roleName;
        private int publicPort;
        private int secondRolesPublicPort;
        private MockCommandRuntime mockCommandRuntime;

        //[TestInitialize]
        public void SetupTest()
        {
//            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            deploymentUrl = AnyUrl();
            roleName = AnyString();
            publicPort = AnyIpPort();
            secondRolesPublicPort = AnyIpPort();

            mockCommandRuntime = new MockCommandRuntime();
        }

        //[TestCleanup]
        public void CleanupTest()
        {
        }

        public class GetAzureWinRMUriStub : GetAzureWinRMUri
        {
            public GetAzureWinRMUriStub(Deployment currentDeployment)
            {
                //this.CurrentDeployment = currentDeployment;
            }
        }

        //[TestMethod]
        public void NoCurrentDeployment()
        {
            var winRmUri = new GetAzureWinRMUriStub(null)
            {
                CommandRuntime = mockCommandRuntime
            };

            winRmUri.ExecuteCommandBody();
            Assert.AreEqual(0, mockCommandRuntime.OutputPipeline.Count, "Nothing should be written to output pipeline");
        }

        //[TestMethod]
        public void NoUrlInDeployment()
        {
            var deployment = new Deployment();
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                CommandRuntime = mockCommandRuntime
            };
            try
            {
                winRmUri.ExecuteCommandBody();
                Assert.Fail("Should throw argument out of range exception");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        //[TestMethod]
        public void NoNameSpecifiedAndNoRoleInstanceListInDeployment()
        {
            var deployment = new Deployment
            {
                Url = AnyUrl()
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                CommandRuntime = mockCommandRuntime
            };
            try
            {
                winRmUri.ExecuteCommandBody();
                Assert.Fail("Should throw argument out of range exception");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        //[TestMethod]
        public void NameSpecifiedAndNoRoleInstanceListInDeployment()
        {
            var deployment = new Deployment
            {
                Url = AnyUrl()
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                CommandRuntime = mockCommandRuntime,
                Name = AnyString()
            };
            try
            {
                winRmUri.ExecuteCommandBody();
                Assert.Fail("Should throw argument out of range exception");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        //[TestMethod]
        public void NoNameSpecifiedAndRoleInstanceListWithNullRoleInstanceInDeployment()
        {
            var deployment = new Deployment
            {
                Url = AnyUrl(),
                RoleInstanceList = new RoleInstanceList()
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                CommandRuntime = mockCommandRuntime
            };
            winRmUri.ExecuteCommandBody();
            Assert.AreEqual(0, mockCommandRuntime.OutputPipeline.Count, "Nothing should be written to output pipeline");
        }

        //[TestMethod]
        public void NameSpecifiedAndRoleInstanceListDoesNotHaveMatchingRoleInstanceInDeployment()
        {
            var deployment = new Deployment
            {
                Url = AnyUrl(),
                RoleInstanceList = new RoleInstanceList
                {
                    new RoleInstance()
                }
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                Name = AnyString(),
                CommandRuntime = mockCommandRuntime
            };
            try
            {
                winRmUri.ExecuteCommandBody();
                Assert.Fail("Should never reach here.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            Assert.AreEqual(0, mockCommandRuntime.OutputPipeline.Count, "Nothing should be written to output pipeline");
        }

        //[TestMethod]
        public void NoNameSpecifiedAndRoleInstanceListWithSingleRoleInstanceWithoutInputEndpointsInDeployment()
        {
            var deployment = new Deployment
            {
                Url = AnyUrl(),
                RoleInstanceList = new RoleInstanceList
                {
                    new RoleInstance()
                }
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                CommandRuntime = mockCommandRuntime
            };
            try
            {
                winRmUri.ExecuteCommandBody();
                Assert.Fail("Should never reach here.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            Assert.AreEqual(0, mockCommandRuntime.OutputPipeline.Count, "Nothing should be written to output pipeline");
        }


        //[TestMethod]
        public void NoNameSpecifiedAndRoleInstanceListWithSingleRoleInstanceWithSingleNonWinRMInputEndpointInDeployment()
        {
            var deployment = new Deployment
            {
                Url = AnyUrl(),
                RoleInstanceList = new RoleInstanceList
                {
                    new RoleInstance
                    {
                        InstanceEndpoints = new InstanceEndpointList
                        {
                            new InstanceEndpoint()
                        }
                    }
                }
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                CommandRuntime = mockCommandRuntime
            };
            winRmUri.ExecuteCommandBody();
            Assert.AreEqual(0, mockCommandRuntime.OutputPipeline.Count, "Nothing should be written to output pipeline");
        }

        //[TestMethod]
        public void NoNameSpecifiedAndRoleInstanceListWithSingleRoleInstanceWithSingleWinRMInputEndpointInDeployment()
        {
            var deployment = new Deployment
            {
                Url = deploymentUrl,
                RoleInstanceList = new RoleInstanceList
                {
                    new RoleInstance
                    {
                        InstanceEndpoints = new InstanceEndpointList
                        {
                            new InstanceEndpoint
                            {
                                LocalPort = WinRMConstants.HttpsListenerPort,
                                Name = WinRMConstants.EndpointName,
                                PublicPort = publicPort
                            }
                        }
                    }
                }
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                CommandRuntime = mockCommandRuntime
            };
            winRmUri.ExecuteCommandBody();
            Assert.AreEqual(1, mockCommandRuntime.OutputPipeline.Count, "One item should be in the output pipeline");

            var uris = mockCommandRuntime.OutputPipeline[0] as List<Uri>;
            Assert.IsNotNull(uris, "List<Uri> is expected");

            Uri uri = uris[0];
            var builder = new UriBuilder("https", deploymentUrl.Host, publicPort);
            Assert.AreEqual(builder.Uri, uri);
        }

        //[TestMethod]
        public void NameSpecifiedAndRoleInstanceListWithSingleRoleInstanceWithoutEndpointInDeployment()
        {
            var deployment = new Deployment
            {
                Url = deploymentUrl,
                RoleInstanceList = new RoleInstanceList
                {
                    new RoleInstance
                    {
                        RoleName = roleName,
                    }
                }
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                Name = roleName,
                CommandRuntime = mockCommandRuntime
            };
            try
            {
                winRmUri.ExecuteCommandBody();
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            Assert.AreEqual(0, mockCommandRuntime.OutputPipeline.Count, "There should be no item in the output pipeline");
        }

        //[TestMethod]
        public void NameSpecifiedAndRoleInstanceListWithSingleRoleInstanceWithSingleWinRMInputEndpointInDeployment()
        {
            var deployment = new Deployment
            {
                Url = deploymentUrl,
                RoleInstanceList = new RoleInstanceList
                {
                    new RoleInstance
                    {
                        RoleName = roleName,
                        InstanceEndpoints = new InstanceEndpointList
                        {
                            new InstanceEndpoint
                            {
                                LocalPort = WinRMConstants.HttpsListenerPort,
                                Name = WinRMConstants.EndpointName,
                                PublicPort = publicPort
                            }
                        }
                    }
                }
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                Name = roleName,
                CommandRuntime = mockCommandRuntime
            };
            winRmUri.ExecuteCommandBody();
            Assert.AreEqual(1, mockCommandRuntime.OutputPipeline.Count, "One item should be in the output pipeline");

            var uri = mockCommandRuntime.OutputPipeline[0] as Uri;
            Assert.IsNotNull(uri, "Uri is expected");

            var builder = new UriBuilder("https", deploymentUrl.Host, publicPort);
            Assert.AreEqual(builder.Uri, uri);
        }

        //[TestMethod]
        public void NoNameSpecifiedAndRoleInstanceListWithMultipleRoleInstanceWithSingleWinRMInputEndpointInDeployment()
        {
            var deployment = new Deployment
            {
                Url = deploymentUrl,
                RoleInstanceList = new RoleInstanceList
                {
                    new RoleInstance
                    {
                        InstanceEndpoints = new InstanceEndpointList
                        {
                            new InstanceEndpoint
                            {
                                LocalPort = WinRMConstants.HttpsListenerPort,
                                Name = WinRMConstants.EndpointName,
                                PublicPort = publicPort
                            }
                        }
                    },
                    new RoleInstance
                    {
                        InstanceEndpoints = new InstanceEndpointList
                        {
                            new InstanceEndpoint
                            {
                                LocalPort = WinRMConstants.HttpsListenerPort,
                                Name = WinRMConstants.EndpointName,
                                PublicPort = secondRolesPublicPort
                            }
                        }
                    }
                }
            };
            var winRmUri = new GetAzureWinRMUriStub(deployment)
            {
                CommandRuntime = mockCommandRuntime
            };
            winRmUri.ExecuteCommandBody();

            Assert.AreEqual(1, mockCommandRuntime.OutputPipeline.Count, "One item should be in the output pipeline");

            var uris = mockCommandRuntime.OutputPipeline[0] as List<Uri>;
            Assert.IsNotNull(uris, "List<Uri> is expected");

            var expectedUris = new List<Uri>
            {
                new UriBuilder("https", deploymentUrl.Host, publicPort).Uri,
                new UriBuilder("https", deploymentUrl.Host, secondRolesPublicPort).Uri
            };

            var a = (from e in expectedUris
                    where !uris.Contains(e)
                    select e).Count();

            Assert.IsTrue(a == 0, "Expected count: {0}, Found count:{1}", expectedUris.Count(), uris.Count());
        }
    }
}