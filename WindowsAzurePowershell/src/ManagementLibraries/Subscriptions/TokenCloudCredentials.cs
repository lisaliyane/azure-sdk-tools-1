//
// Copyright (c) Microsoft.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Common;

namespace Microsoft.WindowsAzure
{
    /// <summary>
    /// Base class for credentials using a token for authorization.
    /// </summary>
    /// <remarks>
    /// This is not permanent and should move into Common once it's been 
    /// vetted a bit
    /// </remarks>
    public partial class TokenCloudCredentials : CloudCredentials
    {
        /// <summary>
        /// The default authorization scheme.
        /// </summary>
        private const string DefaultAuthorizationScheme = "Bearer";

        /// <summary>
        /// Gets or sets the authorization scheme to use with the token.
        /// </summary>
        public virtual string AuthorizationScheme { get; set; }

        /// <summary>
        /// Gets or sets the token to use for authentication.
        /// </summary>
        public virtual string Token { get; set; }

        /// <summary>
        /// Initializes a new instance of the TokenCloudCredentials class.
        /// </summary>
        public TokenCloudCredentials()
        {
            AuthorizationScheme = DefaultAuthorizationScheme;
        }

        /// <summary>
        /// Initializes a new instance of the TokenCloudCredentials class.
        /// </summary>
        /// <param name="token">Token to use for authorization.</param>
        public TokenCloudCredentials(string token)
            : this()
        {
            Token = token;
        }

        /// <summary>
        /// Initialize a ServiceClient instance to process credentials.
        /// </summary>
        /// <typeparam name="T">Type of ServiceClient.</typeparam>
        /// <param name="client">The ServiceClient.</param>
        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            base.InitializeServiceClient(client);
        }

        /// <summary>
        /// Apply the credentials to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// Task that will complete when processing has completed.
        /// </returns>
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationScheme, Token);
            }
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
