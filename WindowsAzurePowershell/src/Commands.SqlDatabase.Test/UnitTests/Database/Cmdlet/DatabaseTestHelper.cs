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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.MockServer;

    public static class DatabaseTestHelper
    {
        /// <summary>
        /// Set the default mock session settings to modify request and responses.
        /// </summary>
        /// <param name="testSession"></param>
        public static void SetDefaultTestSessionSettings(HttpSession testSession)
        {
            testSession.ServiceBaseUri = MockServerHelper.CommonServiceBaseUri;
            testSession.SessionProperties["Servername"] = "testserver";
            testSession.SessionProperties["Username"] = "testuser";
            testSession.SessionProperties["Password"] = "testp@ss1";
            testSession.ResponseModifier =
                new Action<HttpMessage>(
                    (message) =>
                    {
                        DatabaseTestHelper.FixODataResponseUri(
                            message.ResponseInfo,
                            testSession.ServiceBaseUri,
                            MockHttpServer.DefaultServerPrefixUri);
                    });
            testSession.RequestModifier =
                new Action<HttpMessage.Request>(
                    (request) =>
                    {
                        DatabaseTestHelper.FixODataRequestPayload(
                            request,
                            testSession.ServiceBaseUri,
                            MockHttpServer.DefaultServerPrefixUri);
                    });
        }

        /// <summary>
        /// Helper function to validate headers for GetAccessToken request.
        /// </summary>
        public static void ValidateGetAccessTokenRequest(
            HttpMessage.Request expected,
            HttpMessage.Request actual)
        {
            Assert.IsTrue(
                actual.RequestUri.AbsoluteUri.EndsWith("GetAccessToken"),
                "Incorrect Uri specified for GetAccessToken");
            Assert.IsTrue(
                actual.Headers.Contains("sqlauthorization"),
                "sqlauthorization header does not exist in the request");
            Assert.AreEqual(
                expected.Headers["sqlauthorization"],
                actual.Headers["sqlauthorization"],
                "sqlauthorization header does not match");
            Assert.IsNull(
                actual.RequestText,
                "There should be no request text for GetAccessToken");
        }

        /// <summary>
        /// Helper function to validate headers for Service request.
        /// </summary>
        public static void ValidateHeadersForServiceRequest(
            HttpMessage.Request expected,
            HttpMessage.Request actual)
        {
            Assert.IsTrue(
                actual.Headers.Contains(DataServiceConstants.AccessTokenHeader),
                "AccessToken header does not exist in the request");
            Assert.IsTrue(
                actual.Headers.Contains("x-ms-client-session-id"),
                "session-id header does not exist in the request");
            Assert.IsTrue(
                actual.Headers.Contains("x-ms-client-request-id"),
                "request-id header does not exist in the request");
            Assert.IsTrue(
                actual.Cookies.Contains(DataServiceConstants.AccessCookie),
                "AccessCookie does not exist in the request");
        }

        /// <summary>
        /// Helper function to validate headers for OData request.
        /// </summary>
        public static void ValidateHeadersForODataRequest(
            HttpMessage.Request expected,
            HttpMessage.Request actual)
        {
            DatabaseTestHelper.ValidateHeadersForServiceRequest(expected, actual);
            Assert.IsTrue(
                actual.Headers.Contains("DataServiceVersion"),
                "DataServiceVersion header does not exist in the request");
            Assert.AreEqual(
                expected.Headers["DataServiceVersion"],
                actual.Headers["DataServiceVersion"],
                "DataServiceVersion header does not match");
        }

        /// <summary>
        /// Modifies the OData get responses to use the mock server's Uri.
        /// </summary>
        public static void FixODataResponseUri(
            HttpMessage.Response response,
            Uri serviceUri,
            Uri mockServerUri)
        {
            if (serviceUri != null &&
                response.ResponseText.Contains("dataservices") &&
                response.ResponseText.Contains("</entry>"))
            {
                response.ResponseText =
                    response.ResponseText.Replace(serviceUri.ToString(), mockServerUri.ToString());
            }

            if (serviceUri != null &&
                response.Headers.Contains("Location"))
            {
                response.Headers["Location"] = response.Headers["Location"].Replace(
                    serviceUri.ToString(),
                    mockServerUri.ToString());
            }
        }

        /// <summary>
        /// Modifies the OData get request to use the real server's Uri.
        /// </summary>
        public static void FixODataRequestPayload(
            HttpMessage.Request request,
            Uri serviceUri,
            Uri mockServerUri)
        {
            // Fix the $link Uris
            if (serviceUri != null &&
                request.RequestText != null &&
                request.RequestText.Contains("dataservices"))
            {
                request.RequestText =
                    request.RequestText.Replace(mockServerUri.ToString(), serviceUri.ToString());
            }
        }
    }
}
