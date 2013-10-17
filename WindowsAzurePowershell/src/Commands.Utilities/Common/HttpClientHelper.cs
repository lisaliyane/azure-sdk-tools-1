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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using Newtonsoft.Json.Linq;

    public static class HttpClientHelper
    {
        private static readonly char[] uriPathSeparator = new char[] { '/' };

        public static HttpClient CreateClient(string serviceUrl, ICredentials credentials = null, HttpMessageHandler handler = null)
        {
            if (serviceUrl == null)
            {
                throw new ArgumentNullException("serviceUrl");
            }

            HttpMessageHandler effectiveHandler = handler ?? CreateClientHandler(serviceUrl, credentials);
            Uri serviceAddr = new Uri(serviceUrl);
            HttpClient client = new HttpClient(effectiveHandler)
            {
                BaseAddress = serviceAddr,
                MaxResponseContentBufferSize = 30 * 1024 * 1024
            };

            client.DefaultRequestHeaders.Accept.Clear();

            return client;
        }

        public static HttpClientHandler CreateClientHandler(string serviceUrl, ICredentials credentials)
        {
            if (serviceUrl == null)
            {
                throw new ArgumentNullException("serviceUrl");
            }

            // Set up our own HttpClientHandler and configure it
            HttpClientHandler clientHandler = new HttpClientHandler();

            if (credentials != null)
            {
                // Set up credentials cache which will handle basic authentication
                CredentialCache credentialCache = new CredentialCache();

                // Get base address without terminating slash
                string credentialAddress = new Uri(serviceUrl).GetLeftPart(UriPartial.Authority).TrimEnd(uriPathSeparator);

                // Add credentials to cache and associate with handler
                NetworkCredential networkCredentials = credentials.GetCredential(new Uri(credentialAddress), "Basic");
                credentialCache.Add(new Uri(credentialAddress), "Basic", networkCredentials);
                clientHandler.Credentials = credentialCache;
                clientHandler.PreAuthenticate = true;
            }

            // Our handler is ready
            return clientHandler;
        }

        public static HttpContent CreateJsonContent(params KeyValuePair<string, string>[] items)
        {
            var jsonObject = new JObject();
            foreach (KeyValuePair<string, string> kv in items)
            {
                jsonObject.Add(kv.Key, kv.Value);
            }
            return new ObjectContent(typeof(JObject), jsonObject, new JsonMediaTypeFormatter());
        }
    }
}