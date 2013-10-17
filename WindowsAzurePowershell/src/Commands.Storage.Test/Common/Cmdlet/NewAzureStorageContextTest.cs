﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.Storage.Test.Common.Cmdlet
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Storage.Common;
    using Storage.Common.Cmdlet;

    [TestClass]
    public class NewAzureStorageContextTest : StorageTestBase
    {
        /// <summary>
        /// StorageCmdletBase command
        /// </summary>
        public NewAzureStorageContext command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new NewAzureStorageContext
            {
                CommandRuntime = new MockCommandRuntime()
            };
        }

        [TestCleanup]
        public void CleanCommand()
        {
            command = null;
        }

        [TestMethod]
        public void GetStorageAccountByNameAndKeyTest()
        {
            AssertThrows<FormatException>(()=>command.GetStorageAccountByNameAndKey("a", "key", false));
            command.GetStorageAccountByNameAndKey("a", "Xg+4nFQ832QfisuH/CkQwdQUmlqrZebQTJWpAQZ6klWjTVsIBVZy5xNdCDje4EWP0gdWK8vIFAX8LOmz85Wmcg==", false);
        }

        [TestMethod]
        public void GetStorageAccountBySasTokenTest()
        {
            command.GetStorageAccountBySasToken("a", "?st=d", true);
            AssertThrows<Exception>(()=>command.GetStorageAccountBySasToken("a", string.Empty, false));
            AssertThrows<Exception>(() => command.GetStorageAccountBySasToken("a", "token", false));
        }

        [TestMethod]
        public void GetStorageAccountByConnectionStringTest()
        {
            AssertThrows<Exception>(() => command.GetStorageAccountByConnectionString(String.Empty));
            AssertThrows<Exception>(() => command.GetStorageAccountByConnectionString("connection string"));

            Assert.IsNotNull(command.GetStorageAccountByConnectionString("UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://myProxyUri"));
        }

        [TestMethod]
        public void GetLocalDevelopmentStorageAccountTest()
        {
            Assert.IsNotNull(command.GetLocalDevelopmentStorageAccount());
        }

        [TestMethod]
        public void GetAnonymousStorageAccountTest()
        {
            Assert.IsNotNull(command.GetAnonymousStorageAccount("a", false));
        }

        [TestMethod]
        public void GetStorageAccountWithEndPointTest()
        {
            string name = string.Empty;
            StorageCredentials credential = new StorageCredentials();
            AssertThrows<ArgumentException>(() => command.GetStorageAccountWithEndPoint(credential, name, false), String.Format(Resources.ObjectCannotBeNull, StorageNouns.StorageAccountName));

            name = "test";
            Assert.IsNotNull(command.GetStorageAccountWithEndPoint(credential, name, false));
        }

        [TestMethod]
        public void ExecuteNewAzureStorageContextCmdlet()
        {
            AssertThrows<ArgumentException>(() => command.ExecuteCmdlet(), Resources.DefaultStorageCredentialsNotFound);
        }

        [TestMethod]
        public void GetDefaultEndPointDomainTest()
        {
            Assert.AreEqual(command.GetDefaultEndPointDomain(), Resources.DefaultStorageEndPointDomain);
        }
    }
}
