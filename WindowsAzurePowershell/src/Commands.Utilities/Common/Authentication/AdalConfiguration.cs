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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication
{
    using System;
    using System.Linq;

    /// <summary>
    /// Class storing the configuration information needed
    /// for ADAL to request token from the right AD tenant
    /// depending on environment.
    /// </summary>
    public class AdalConfiguration
    {
        //
        // These constants define the default values to use for AD authentication
        // against RDFE
        //
        private const string powershellClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        private static readonly Uri powershellRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");
        private const string rdfeResourceUri = "https://management.core.windows.net/";

        // Default endpoint for public azure
        private const string publicAdEndpoint = "https://login.windows.net/";

        // Turn off endpoint validation for known test cluster AD endpoints
        private static readonly string[] knownTestEndpoints = 
        {
            "https://sts.login.windows-int.net/"
        };
        

        // ID for site to pass to enable EBD (email-based differentiation)
        // This gets passed in the call to get the azure branding on the
        // login window. Also adding popup flag to handle overly large login windows.
        internal const string EnableEbdMagicCookie = "site_id=501358&display=popup";

        private string adEndpoint = publicAdEndpoint;

        public string AdEndpoint
        {
            get { return adEndpoint; }
            set { adEndpoint = value; }
        }

        public bool ValidateAuthority
        {
            get { return knownTestEndpoints.All(s => string.Compare(s, adEndpoint, StringComparison.OrdinalIgnoreCase) != 0); }
        }

        public string AdDomain { get; set; }
        public string ClientId { get; set; }
        public Uri ClientRedirectUri { get; set; }
        public string ResourceClientUri { get; set; }


        public AdalConfiguration()
        {
            ClientId = powershellClientId;
            ClientRedirectUri = powershellRedirectUri;
            ResourceClientUri = rdfeResourceUri;
        }

        public AdalConfiguration(WindowsAzureEnvironment environment)
            : this()
        {
            AdEndpoint = environment.ActiveDirectoryEndpoint;
            AdDomain = environment.ActiveDirectoryCommonTenantId;
        }

        public AdalConfiguration(WindowsAzureSubscription subscription)
            : this()
        {
            AdEndpoint = subscription.ActiveDirectoryEndpoint;
            AdDomain = subscription.ActiveDirectoryTenantId;
        }
    }
}