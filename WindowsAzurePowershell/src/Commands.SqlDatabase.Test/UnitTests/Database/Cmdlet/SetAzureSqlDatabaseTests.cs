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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.Database.Cmdlet
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Commands.Test.Utilities.Common;
    using MockServer;
    using Services;
    using Services.Server;
    using SqlDatabase.Database.Cmdlet;

    [TestClass]
    public class SetAzureSqlDatabaseTests : TestBase
    {
        [TestInitialize]
        public void InitializeTest()
        {
            // Create 2 test databases
            NewAzureSqlDatabaseTests.CreateTestDatabasesWithSqlAuth();
        }

        [TestCleanup]
        public void CleanupTest()
        {
            // Remove the test databases
            NewAzureSqlDatabaseTests.RemoveTestDatabasesWithSqlAuth();

            // Save the mock session results
            MockServerHelper.SaveDefaultSessionCollection();
        }

        [TestMethod]
        public void SetAzureSqlDatabaseSizeWithSqlAuth()
        {
            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                // Create a context
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuth(
                    powershell,
                    "$context");

                HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                    "UnitTests.SetAzureSqlDatabaseSizeWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        if (expected.Index < 5)
                        {
                            // Request 0-2: Set testdb1 with new MaxSize
                            // Request 3-4: Get updated testdb1
                            DatabaseTestHelper.ValidateHeadersForODataRequest(
                                expected.RequestInfo,
                                actual);
                        }
                        else
                        {
                            Assert.Fail("No more requests expected.");
                        }
                    });

                using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
                {
                    // Create context with both ManageUrl and ServerName overriden
                    Collection<PSObject> database;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        database = powershell.InvokeBatchScript(
                            @"Set-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1 " +
                            @"-MaxSizeGB 5 " +
                            @"-Force " +
                            @"-PassThru");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    Assert.IsTrue(
                        database.Single().BaseObject is Services.Server.Database,
                        "Expecting a Database object");
                    Services.Server.Database databaseObj =
                        (Services.Server.Database)database.Single().BaseObject;
                    Assert.AreEqual("testdb1", databaseObj.Name, "Expected db name to be testdb1");
                    Assert.AreEqual("Web", databaseObj.Edition, "Expected edition to be Web");
                    Assert.AreEqual(5, databaseObj.MaxSizeGB, "Expected max size to be 5 GB");
                }
            }
        }

        [TestMethod]
        public void SetAzureSqlDatabaseNameWithSqlAuth()
        {
            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                // Create a context
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuth(
                    powershell,
                    "$context");
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuth(
                    powershell,
                    "$contextCleanup");

                HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                    "UnitTests.SetAzureSqlDatabaseNameWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        if (expected.Index < 10)
                        {
                            // Request 0-4: Set testdb1 with new name of new_testdb1
                            // Request 5-9: Set new_testdb1 with new name of testdb1
                            DatabaseTestHelper.ValidateHeadersForODataRequest(
                                expected.RequestInfo,
                                actual);
                        }
                        else
                        {
                            Assert.Fail("No more requests expected.");
                        }
                    });

                using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
                {
                    // Create context with both ManageUrl and ServerName overriden
                    Collection<PSObject> database;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        database = powershell.InvokeBatchScript(
                           @"Set-AzureSqlDatabase " +
                           @"-Context $context " +
                           @"-DatabaseName testdb1 " +
                           @"-NewName new_testdb1 " +
                           @"-Force " +
                           @"-PassThru");
                        powershell.InvokeBatchScript(
                           @"Set-AzureSqlDatabase " +
                           @"-Context $contextCleanup " +
                           @"-DatabaseName new_testdb1 " +
                           @"-NewName testdb1 " +
                           @"-Force");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    Assert.IsTrue(
                        database.Single().BaseObject is Services.Server.Database,
                        "Expecting a Database object");
                    Services.Server.Database databaseObj =
                        (Services.Server.Database)database.Single().BaseObject;
                    Assert.AreEqual("new_testdb1", databaseObj.Name, "Expected db name to be new_testdb1");
                    Assert.AreEqual("Web", databaseObj.Edition, "Expected edition to be Web");
                    Assert.AreEqual(1, databaseObj.MaxSizeGB, "Expected max size to be 1 GB");
                }
            }
        }

        [TestMethod]
        public void SetAzureSqlDatabaseServiceObjectiveWithSqlAuth()
        {
            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                // Create a context
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuth(
                    powershell,
                    "$context");

                HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                    "UnitTests.SetAzureSqlDatabaseServiceObjectiveWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        switch (expected.Index)
                        {
                            // Request 0-1: Get Service Objective
                            case 0:
                            case 1:
                            // Request 2-7: Get/Update/Re-Get testdb2
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                DatabaseTestHelper.ValidateHeadersForODataRequest(
                                    expected.RequestInfo,
                                    actual);
                                break;
                            default:
                                Assert.Fail("No more requests expected.");
                                break;
                        }
                    });

                using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
                {
                    // Create context with both ManageUrl and ServerName overriden
                    Collection<PSObject> database;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        powershell.InvokeBatchScript(
                            @"$slo = Get-AzureSqlDatabaseServiceObjective " +
                            @"-Context $context " +
                            @"-ServiceObjectiveName ""Reserved P1""");

                        database = powershell.InvokeBatchScript(
                            @"Set-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb2 " +
                            @"-ServiceObjective $slo " +
                            @"-Force " +
                            @"-PassThru");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    Assert.IsTrue(
                        database.Single().BaseObject is Services.Server.Database,
                        "Expecting a Database object");
                    Services.Server.Database databaseObj =
                        (Services.Server.Database)database.Single().BaseObject;
                    databaseObj = (Services.Server.Database)database.Single().BaseObject;
                    Assert.AreEqual("testdb2", databaseObj.Name, "Expected db name to be testdb2");
                    Assert.AreEqual((byte)0, databaseObj.ServiceObjectiveAssignmentState, "Expected assignment state to be pending");
                    //Assert.AreEqual("Reserved P1", databaseObj.ServiceObjective.Name, "Expected Reserved P1");
                    //Assert.AreEqual("Reserved P1", databaseObj.ServiceObjectiveName, "Expected Reserved P1");
                }
            }
        }
    }
}
