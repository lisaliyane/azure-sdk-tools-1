﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.CloudService.Services
{
    using System;
    using System.Globalization;
    using System.IO;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Utilities;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    public static class AzureBlob
    {
        private const string ContainerName = "azpsnode122011";

        public static Uri UploadPackageToBlob(IServiceManagement channel, string storageName, string subscriptionId, string packagePath, BlobRequestOptions blobRequestOptions)
        {
            string storageKey;
            string blobEndpointUri;

            StorageService storageService = channel.GetStorageKeys(subscriptionId, storageName);
            storageKey = storageService.StorageServiceKeys.Primary;
            storageService = channel.GetStorageService(subscriptionId, storageName);
            blobEndpointUri = storageService.StorageServiceProperties.Endpoints[0];

            return UploadFile(storageName, blobEndpointUri, storageKey, packagePath, blobRequestOptions);
        }

        /// <summary>
        /// Uploads a file to azure store.
        /// </summary>
        /// <param name="storageName">Store which file will be uploaded to</param>
        /// <param name="storageUri">The storage endpoint Uri</param>
        /// <param name="storageKey">Store access key</param>
        /// <param name="filePath">Path to file which will be uploaded</param>
        /// <param name="blobRequestOptions">The request options for blob uploading.</param>
        /// <returns>Uri which holds locates the uploaded file</returns>
        /// <remarks>The uploaded file name will be guid</remarks>
        public static Uri UploadFile(string storageName, string blobEndpointUri, string storageKey, string filePath, BlobRequestOptions blobRequestOptions)
        {
            StorageCredentials credentials = new StorageCredentials(storageName, storageKey);
            CloudBlobClient client = new CloudBlobClient(new Uri(blobEndpointUri), credentials);
            string blobName = Guid.NewGuid().ToString();

            CloudBlobContainer container = client.GetContainerReference(ContainerName);
            container.CreateIfNotExists();
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            using (FileStream readStream = File.OpenRead(filePath))
            {
                blob.UploadFromStream(readStream, AccessCondition.GenerateEmptyCondition(), blobRequestOptions);
            }

            return new Uri(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}{2}{3}",
                    client.BaseUri,
                    ContainerName,
                    client.DefaultDelimiter,
                    blobName));
        }
    }
}