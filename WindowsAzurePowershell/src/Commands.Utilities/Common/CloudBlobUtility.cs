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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Management;
    using Management.Compute;
    using Management.Storage;
    using Management.Storage.Models;
    using ServiceManagement;
    using Storage;
    using Storage.Auth;
    using Storage.Blob;

    public class CloudBlobUtility
    {
        private const string BlobEndpointIdentifier = ".blob.";
        private const string ContainerName = "mydeployments";

        public virtual void DeletePackageFromBlob(
            IServiceManagement channel,
            string storageName,
            string subscriptionId,
            Uri packageUri)
        {
            var storageService = channel.GetStorageKeys(subscriptionId, storageName);
            var storageKey = storageService.StorageServiceKeys.Primary;
            storageService = channel.GetStorageService(subscriptionId, storageName);
            var blobStorageEndpoint = General.CreateHttpsEndpoint(
                storageService.StorageServiceProperties.Endpoints.Find(p => p.Contains(BlobEndpointIdentifier)));
            var credentials = new StorageCredentials(storageName, storageKey);
            var client = new CloudBlobClient(blobStorageEndpoint, credentials);
            ICloudBlob blob = client.GetBlobReferenceFromServer(packageUri);
            blob.DeleteIfExists();
        }

        public virtual void DeletePackageFromBlob(
            StorageManagementClient storageClient,
            string storageName,
            Uri packageUri)
        {
            StorageAccountGetKeysResponse keys = storageClient.StorageAccounts.GetKeys(storageName);
            string storageKey = keys.PrimaryKey;
            var storageService = storageClient.StorageAccounts.Get(storageName);
            var blobStorageEndpoint = storageService.Properties.Endpoints[0];
            var credentials = new StorageCredentials(storageName, storageKey);
            var client = new CloudBlobClient(blobStorageEndpoint, credentials);
            ICloudBlob blob = client.GetBlobReferenceFromServer(packageUri);
            blob.DeleteIfExists();
        }

        public virtual Uri UploadFile(
            string storageName,
            Uri blobEndpointUri,
            string storageKey,
            string filePath,
            BlobRequestOptions blobRequestOptions)
        {
            StorageCredentials credentials = new StorageCredentials(storageName, storageKey);
            CloudBlobClient client = new CloudBlobClient(blobEndpointUri, credentials);
            string blobName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1}",
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture),
                Path.GetFileName(filePath));

            CloudBlobContainer container = client.GetContainerReference(ContainerName);
            container.CreateIfNotExists();
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            using (FileStream readStream = File.OpenRead(filePath))
            {
                blob.UploadFromStream(readStream, AccessCondition.GenerateEmptyCondition(), blobRequestOptions);
            }

            return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", client.BaseUri, ContainerName, client.DefaultDelimiter, blobName));
        }

        public virtual Uri UploadPackageToBlob(
            IServiceManagement channel,
            string storageName,
            string subscriptionId,
            string packagePath,
            BlobRequestOptions blobRequestOptions)
        {
            StorageService storageService = channel.GetStorageKeys(subscriptionId, storageName);
            string storageKey = storageService.StorageServiceKeys.Primary;
            storageService = channel.GetStorageService(subscriptionId, storageName);
            string blobEndpointUri = storageService.StorageServiceProperties.Endpoints[0];

            return UploadFile(
                storageName,
                General.CreateHttpsEndpoint(blobEndpointUri), storageKey, packagePath, blobRequestOptions);
        }

        public virtual Uri UploadPackageToBlob(
            StorageManagementClient storageClient,
            string storageName,
            string packagePath,
            BlobRequestOptions blobRequestOptions)
        {
            StorageAccountGetKeysResponse keys = storageClient.StorageAccounts.GetKeys(storageName);
            string storageKey = keys.PrimaryKey;
            var storageService = storageClient.StorageAccounts.Get(storageName);
            Uri blobEndpointUri = storageService.Properties.Endpoints[0];
            return UploadFile(storageName,
                General.CreateHttpsEndpoint(blobEndpointUri.ToString()),
                storageKey, packagePath, blobRequestOptions);
        }
    }
}