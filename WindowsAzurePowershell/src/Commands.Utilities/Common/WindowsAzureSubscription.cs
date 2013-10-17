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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using Authentication;
    using Management;
    using Management.Storage;
    using Storage;
    using Storage.Auth;
    using Subscriptions;
    using WindowsAzure.Common;
    using Properties;

    /// <summary>
    /// Representation of a subscription in memory
    /// </summary>
    public class WindowsAzureSubscription
    {
        public string SubscriptionName { get; set; }
        public string SubscriptionId { get; set; }
        public Uri ServiceEndpoint { get; set; }

        public string ActiveDirectoryEndpoint { get; set; }
        public string ActiveDirectoryTenantId { get; set; }

        public bool IsDefault { get; set; }
        public X509Certificate2 Certificate { get; set; }

        private string currentStorageAccountName;
        private CloudStorageAccount cloudStorageAccount;

        private readonly List<string> registeredResourceProviders = new List<string>();

        internal List<string> RegisteredResourceProviders
        {
            get { return registeredResourceProviders; }
        }

        /// <summary>
        /// Delegate used to trigger profile to save itself, used
        /// when cached list of resource providers is updated.
        /// </summary>
        internal Action Save { get; set; }

        public string CurrentStorageAccountName
        {
            get { return currentStorageAccountName; }
            set
            {
                if (currentStorageAccountName != value)
                {
                    currentStorageAccountName = value;
                    cloudStorageAccount = null;
                }
            }
        }

        public CloudStorageAccount CurrentCloudStorageAccount
        {
            get { return cloudStorageAccount; }
        }

        // Access token / account name for Active Directory
        public string ActiveDirectoryUserId { get; set; }
        internal ITokenProvider TokenProvider { get; set; }

        private IAccessToken accessToken;
        
        /// <summary>
        /// Set the access token to use for authentication
        /// when creating azure management clients from this
        /// subscription. This also updates the <see cref="ActiveDirectoryUserId"/> and
        /// <see cref="ActiveDirectoryLoginType"/> fields.
        /// </summary>
        /// <param name="token">The access token to use. If null,
        /// clears out the token and the active directory login information.</param>
        public void SetAccessToken(IAccessToken token)
        {
            if (token != null)
            {
                ActiveDirectoryUserId = token.UserId;
            }
            else
            {
                ActiveDirectoryUserId = null;
            }
            accessToken = token;
        }

        private SubscriptionCloudCredentials CreateCredentials()
        {
            if (accessToken == null && ActiveDirectoryUserId == null)
            {
                return new CertificateCloudCredentials(SubscriptionId, Certificate);
            }
            if (accessToken == null)
            {
                accessToken = TokenProvider.GetCachedToken(this, ActiveDirectoryUserId);
            }
            return new AccessTokenCredential(SubscriptionId, accessToken);
        }

        /// <summary>
        /// Update the contents of this subscription with the data from the
        /// given new subscription. Does a merge of the data, leaving for example
        /// existing certificate if subscription is also download from azure AD.
        /// </summary>
        /// <param name="newSubscription">Subscription data to update from</param>
        public void Update(WindowsAzureSubscription newSubscription)
        {
            // AD Data - if present in new subscription, take it else preserve existing
            ActiveDirectoryEndpoint = newSubscription.ActiveDirectoryEndpoint ??
                ActiveDirectoryEndpoint;
            ActiveDirectoryTenantId = newSubscription.ActiveDirectoryTenantId ??
                ActiveDirectoryTenantId;
            ActiveDirectoryUserId = newSubscription.ActiveDirectoryUserId ??
                ActiveDirectoryUserId;

            // Certificate - if present in new take it, else preserve
            Certificate = newSubscription.Certificate ??
                Certificate;

            // One of them is the default
            IsDefault = newSubscription.IsDefault || IsDefault;

            // And overwrite the rest
            SubscriptionId = newSubscription.SubscriptionId;
            ServiceEndpoint = newSubscription.ServiceEndpoint;
            SubscriptionName = newSubscription.SubscriptionName;
        }

        /// <summary>
        /// Create a service management client for this subscription,
        /// with appropriate credentials supplied.
        /// </summary>
        /// <typeparam name="TClient">Type of client to create, must be derived from <see cref="ServiceClient{T}"/></typeparam>
        /// <returns>The service client instance</returns>
        public TClient CreateClient<TClient>() where TClient : ServiceClient<TClient>
        {
            var credential = CreateCredentials();
            RegisterRequiredResourceProviders<TClient>(credential);
            var constructor = typeof(TClient).GetConstructor(new[] { typeof(SubscriptionCloudCredentials), typeof(Uri) });
            if (constructor == null)
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidManagementClientType, typeof(TClient).Name));
            }

            // Dispose the client because the WithHandler call will create a
            // new instance that we'll be using with our commands
            using (var client = (TClient)constructor.Invoke(new object[] { credential, ServiceEndpoint }))
            {
                // Set the UserAgent
                client.UserAgent.Add(ApiConstants.UserAgentValue);

                // Add the logging handler
                var withHandlerMethod = typeof(TClient).GetMethod("WithHandler", new[] { typeof(DelegatingHandler) });
                return (TClient)withHandlerMethod.Invoke(client, new object[] { new HttpRestCallLogger() });
            }
        }

        public CloudStorageAccount GetCloudStorageAccount()
        {
            if (cloudStorageAccount == null)
            {
                using (var storageClient = CreateClient<StorageManagementClient>())
                {
                    var storageServiceResponse = storageClient.StorageAccounts.Get(CurrentStorageAccountName);
                    var storageKeysResponse = storageClient.StorageAccounts.GetKeys(CurrentStorageAccountName);

                    cloudStorageAccount = new CloudStorageAccount(
                        new StorageCredentials(storageServiceResponse.ServiceName, storageKeysResponse.PrimaryKey),
                        General.CreateHttpsEndpoint(storageServiceResponse.Properties.Endpoints[0].ToString()),
                        General.CreateHttpsEndpoint(storageServiceResponse.Properties.Endpoints[1].ToString()),
                        General.CreateHttpsEndpoint(storageServiceResponse.Properties.Endpoints[2].ToString()));
                }
            }
            return cloudStorageAccount;
        }

        private void RegisterRequiredResourceProviders<T>(SubscriptionCloudCredentials credentials) where T : ServiceClient<T>
        {
            var requiredProviders = RequiredResourceLookup.RequiredProvidersFor<T>();
            var unregisteredProviders = requiredProviders.Where(p => !RegisteredResourceProviders.Contains(p)).ToList();

            if (unregisteredProviders.Count > 0)
            {
                using(var client = new ManagementClient(credentials, ServiceEndpoint))
                {
                    foreach (var provider in unregisteredProviders)
                    {
                        try
                        {
                            client.Subscriptions.RegisterResource(provider);
                        }
                        catch (CloudException ex)
                        {
                            if (ex.Response.StatusCode != HttpStatusCode.Conflict && ex.Response.StatusCode != HttpStatusCode.NotFound)
                            {
                                // Conflict means already registered, that's OK.
                                // NotFound means there is no registration support, like Windows Azure Pack.
                                // Otherwise it's a failure.
                                throw;
                            }
                        }
                        RegisteredResourceProviders.Add(provider);
                    }
                    Save();
                }
            }
        }
    }
}
