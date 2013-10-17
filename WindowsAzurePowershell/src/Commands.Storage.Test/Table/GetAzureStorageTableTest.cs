// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Commands.Storage.Test.Table
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Storage.Common;
    using Storage.Table.Cmdlet;

    [TestClass]
    public class GetAzureStorageTableTest : StorageTableStorageTestBase
    {
        public GetAzureStorageTableCommand command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new GetAzureStorageTableCommand(tableMock)
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
        public void ListTablesByNameWithEmptyNameTest()
        {
            List<CloudTable> tableList = command.ListTablesByName("").ToList();
            Assert.AreEqual(0, tableList.Count);

            AddTestTables();
            tableList = command.ListTablesByName("").ToList();
            Assert.AreEqual(2, tableList.Count);
        }

        [TestMethod]
        public void ListTablesByNameWithWildCardTest()
        {
            AddTestTables();

            List<CloudTable> tableList = command.ListTablesByName("te*t").ToList();
            Assert.AreEqual(2, tableList.Count);

            tableList = command.ListTablesByName("tx*t").ToList();
            Assert.AreEqual(0, tableList.Count);

            tableList = command.ListTablesByName("t?st").ToList();
            Assert.AreEqual(1, tableList.Count);
            Assert.AreEqual("test", tableList[0].Name);
        }

        [TestMethod]
        public void ListTablesByNameWithInvalidNameTest()
        {
            string invalidName = "a";
            AssertThrows<ArgumentException>(() => command.ListTablesByName(invalidName).ToList(),
                String.Format(Resources.InvalidTableName, invalidName));

            invalidName = "xx%%d";
            AssertThrows<ArgumentException>(() => command.ListTablesByName(invalidName).ToList(),
                String.Format(Resources.InvalidTableName, invalidName));
        }

        [TestMethod]
        public void ListTablesByNameWithNoExistsTableTest()
        {
            string notExistingName = "abcdefg";
            AssertThrows<ResourceNotFoundException>(() => command.ListTablesByName(notExistingName).ToList(),
                String.Format(Resources.TableNotFound, notExistingName));
        }

        [TestMethod]
        public void ListTablesByNameSuccessfullyTest()
        {
            AddTestTables();
            List<CloudTable> tableList = command.ListTablesByName("text").ToList();
            Assert.AreEqual(1, tableList.Count);
            Assert.AreEqual("text", tableList[0].Name);
        }

        [TestMethod]
        public void ListTablesByPrefixWithInvalidPrefixTest()
        {
            AssertThrows<ArgumentException>(() => command.ListTablesByPrefix(String.Empty),
                String.Format(Resources.InvalidTableName, String.Empty));

            string prefix = "?";
            AssertThrows<ArgumentException>(() => command.ListTablesByPrefix(prefix),
                String.Format(Resources.InvalidTableName, prefix));
        }

        [TestMethod]
        public void ListTablesByPrefixSuccessfullyTest()
        {
            AddTestTables();
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            List<CloudTable> tableList = command.ListTablesByPrefix("te").ToList();
            Assert.AreEqual(2, tableList.Count);

            tableList = command.ListTablesByPrefix("tes").ToList();
            Assert.AreEqual(1, tableList.Count);
            Assert.AreEqual("test", tableList[0].Name);

            tableList = command.ListTablesByPrefix("testx").ToList();
            Assert.AreEqual(0, tableList.Count);
        }

        [TestMethod]
        public void WriteTablesWithStorageContextTest()
        {
            command.WriteTablesWithStorageContext(null);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.WriteTablesWithStorageContext(tableMock.tableList);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            AddTestTables();
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.WriteTablesWithStorageContext(tableMock.tableList);
            Assert.AreEqual(2, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
        }

        [TestMethod]
        public void ExecuteCommandGetContainerTest()
        {
            AddTestTables();
            command.Name = "test";
            command.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
        }
    }
}
