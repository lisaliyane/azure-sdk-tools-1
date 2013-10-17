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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.UnitTests.Cmdlets.StorageServices
{
    using System;
    using System.Reflection;
    using Microsoft.WindowsAzure.Management.ServiceManagement.StorageServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Extensions;
    using Microsoft.WindowsAzure.Management.Test.Stubs;

    using System.IO;

    [TestClass]
    public class SaveAzureVhdCommandTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        public class TestInputParameters
        {
            public string Description { get; set; }
            public Uri Source { get; set; }
            public FileInfo LocalFilePath { get; set; }
            //public Uri BaseImage { get; set; }
            public bool ExpectedResult { get; set; }
        }

        public void ExecuteTestWithInputParameters(TestInputParameters input)
        {
            var command = new SaveAzureVhdCommand
            {
                Source = input.Source,                 
            };
            try
            {
                //command.ValidateParameters();
                if(!input.ExpectedResult)
                {
                    Assert.Fail(input.Description);
                }
            }
            catch (Exception)
            {
                if(input.ExpectedResult)
                {
                    Assert.Fail(input.Description);
                }
            }
        }

        [TestMethod]
        public void InvalidSourceUri()
        {
            ExecuteTestWithInputParameters(new TestInputParameters
            {
                Description = MethodBase.GetCurrentMethod().Name,
                Source = new Uri("http://localhost"),                
                ExpectedResult = false
            });
        }

        [TestMethod]
        public void EmptySourceUri()
        {
            ExecuteTestWithInputParameters(new TestInputParameters
            {
                Description = MethodBase.GetCurrentMethod().Name,
                Source = null,                
                ExpectedResult = false
            });
        }

        [TestMethod]
        public void ValidSourceUri()
        {
            ExecuteTestWithInputParameters(new TestInputParameters
            {
                Description = MethodBase.GetCurrentMethod().Name,
                Source = new Uri("http://myaccount.blob.core.windows.net/mycontainer/myblob"),                
                ExpectedResult = true
            });
        }        
    }
}