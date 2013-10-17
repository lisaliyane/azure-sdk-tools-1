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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.StorageServices
{
    using System;
    using Management.Storage;
    using Properties;
    using Storage.Auth;
    using Sync.Download;
    using Utilities.Common;

    public class StorageCredentialsFactory
    {
        private StorageManagementClient client;
        private WindowsAzureSubscription currentSubscription;

        public static bool IsChannelRequired(Uri destination)
        {
            return String.IsNullOrEmpty(destination.Query);
        }

        public StorageCredentialsFactory()
        {
        }

        public StorageCredentialsFactory(StorageManagementClient client, WindowsAzureSubscription currentSubscription)
        {
            this.client = client;
            this.currentSubscription = currentSubscription;
        }

        public StorageCredentials Create(BlobUri destination)
        {
            if (IsChannelRequired(destination.Uri))
            {
                if(currentSubscription == null)
                {
                    throw new ArgumentException(Resources.StorageCredentialsFactoryCurrentSubscriptionNotSet, "SubscriptionId");
                }

                var storageKeys = this.client.StorageAccounts.GetKeys(destination.StorageAccountName);
                return new StorageCredentials(destination.StorageAccountName, storageKeys.PrimaryKey);
            }

            return new StorageCredentials(destination.Uri.Query);
        }
    }
}