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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Server;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.MockServer;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.Server.Cmdlet;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.Utilities;
    using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    [TestClass]
    public class AzureSqlDatabaseTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            // Save the mock session results
            MockServerHelper.SaveDefaultSessionCollection();
        }

        /// <summary>
        /// Test Get/Set/Remove a database using certificate authentication.
        /// </summary>
        [TestMethod]
        public void AzureSqlDatabaseCertTests()
        {
            // This test uses the https endpoint, setup the certificates.
            MockHttpServer.SetupCertificates();

            using (PowerShell powershell = PowerShell.Create())
            {
                // Setup the subscription used for the test
                WindowsAzureSubscription subscription =
                    UnitTestHelper.SetupUnitTestSubscription(powershell);

                powershell.Runspace.SessionStateProxy.SetVariable(
                    "serverName",
                    SqlDatabaseTestSettings.Instance.ServerName);

                // Create a new server
                HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                    "UnitTest.AzureSqlDatabaseCertTests");
                ServerTestHelper.SetDefaultTestSessionSettings(testSession);
                //testSession.ServiceBaseUri = new Uri("https://management.core.windows.net");
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.IsTrue(
                            actual.UserAgent.Contains(ApiConstants.UserAgentHeaderValue),
                            "Missing proper UserAgent string.");
                        Assert.IsTrue(
                            UnitTestHelper.GetUnitTestClientCertificate().Equals(actual.Certificate),
                            "Expected correct client certificate");
                    });

                Collection<PSObject> newDatabaseResult1 = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"New-AzureSqlDatabase" +
                            @" -ServerName $serverName" +
                            @" -DatabaseName testdbcert1");
                    });

                Collection<PSObject> newDatabaseResult2 = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"New-AzureSqlDatabase" +
                            @" -ServerName $serverName" +
                            @" -DatabaseName testdbcert2" +
                            @" -Edition Business" +
                            @" -MaxSizeGB 10" +
                            @" -Collation Japanese_CI_AS");
                    });

                Collection<PSObject> getDatabaseResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabase" +
                            @" $serverName");
                    });

                Collection<PSObject> getSingleDatabaseResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabase" +
                            @" $serverName" +
                            @" -DatabaseName testdbcert1");
                    });

                Collection<PSObject> setDatabaseNameResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        powershell.Runspace.SessionStateProxy.SetVariable("db", newDatabaseResult1.FirstOrDefault());
                        return powershell.InvokeBatchScript(
                            @"$db | Set-AzureSqlDatabase" +
                            @" -NewDatabaseName testdbcert3" +
                            @" -PassThru");
                    });

                Collection<PSObject> setDatabaseSizeResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        powershell.Runspace.SessionStateProxy.SetVariable("db", newDatabaseResult1.FirstOrDefault());
                        return powershell.InvokeBatchScript(
                            @"$db | Set-AzureSqlDatabase" +
                            @" -MaxSizeGB 5" +
                            @" -PassThru");
                    });

                Collection<PSObject> removeDatabaseResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        powershell.Runspace.SessionStateProxy.SetVariable("db1", newDatabaseResult1.FirstOrDefault());
                        powershell.Runspace.SessionStateProxy.SetVariable("db2", newDatabaseResult2.FirstOrDefault());
                        powershell.InvokeBatchScript(
                            @"$db1 | Remove-AzureSqlDatabase" +
                            @" -Force");
                        powershell.InvokeBatchScript(
                            @"$db2 | Remove-AzureSqlDatabase" +
                            @" -Force");

                        return powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabase" +
                            @" $serverName");
                    });

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Unexpected Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Unexpected Warnings during run!");

                // Validate New-AzureSqlDatabase
                Database[] databases = new Database[] { newDatabaseResult1.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                // Note: Because the object is piped, this is the final state of the 
                // database object, after all the Set- cmdlet has run.
                Assert.AreEqual("testdbcert3", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(5, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);

                databases = new Database[] { newDatabaseResult2.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                Assert.AreEqual("testdbcert2", databases[0].Name);
                Assert.AreEqual("Business", databases[0].Edition);
                Assert.AreEqual(10, databases[0].MaxSizeGB);
                Assert.AreEqual("Japanese_CI_AS", databases[0].CollationName);

                // Validate Get-AzureSqlDatabase
                databases = getDatabaseResult.Single().BaseObject as Database[];
                Assert.AreEqual(3, databases.Length, "Expecting three databases");
                Assert.IsNotNull(databases[0], "Expecting a Database object.");
                Assert.IsNotNull(databases[1], "Expecting a Database object.");
                Assert.IsNotNull(databases[2], "Expecting a Database object.");
                Assert.AreEqual("master", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(5, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);
                Assert.AreEqual(true, databases[0].IsSystemObject);
                Assert.AreEqual("testdbcert1", databases[1].Name);
                Assert.AreEqual("Web", databases[1].Edition);
                Assert.AreEqual(1, databases[1].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[1].CollationName);
                Assert.AreEqual(false, databases[1].IsSystemObject);
                Assert.AreEqual("testdbcert2", databases[2].Name);
                Assert.AreEqual("Business", databases[2].Edition);
                Assert.AreEqual(10, databases[2].MaxSizeGB);
                Assert.AreEqual("Japanese_CI_AS", databases[2].CollationName);
                Assert.AreEqual(false, databases[2].IsSystemObject);

                databases = new Database[] { getSingleDatabaseResult.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                Assert.AreEqual("testdbcert1", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(1, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);

                // Validate Set-AzureSqlDatabase
                databases = new Database[] { setDatabaseNameResult.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                Assert.AreEqual("testdbcert3", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(1, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);

                databases = new Database[] { setDatabaseSizeResult.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                Assert.AreEqual("testdbcert3", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(5, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);

                // Validate Remove-AzureSqlDatabase
                databases = removeDatabaseResult.Single().BaseObject as Database[];
                Assert.AreEqual(1, databases.Length, "Expecting no databases");
                Assert.IsNotNull(databases[0], "Expecting a Database object.");
                Assert.AreEqual("master", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(5, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);
            }
        }
    }
}
