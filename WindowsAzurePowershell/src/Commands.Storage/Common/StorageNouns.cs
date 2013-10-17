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

namespace Microsoft.WindowsAzure.Commands.Storage.Common
{
    /// <summary>
    /// Storage nouns for cmdlet name
    /// </summary>
    public static class StorageNouns
    {
        /// <summary>
        /// Blob cmdlet name
        /// </summary>
        public const string Blob = "AzureStorageBlob";

        /// <summary>
        /// Blobcontent cmdlet name
        /// </summary>
        public const string BlobContent = "AzureStorageBlobContent";

        /// <summary>
        /// blob snapshot cmdlet name
        /// </summary>
        public const string BlobSnapshot = "AzureStorageBlobSnapshot";

        /// <summary>
        /// Container cmdlet name
        /// </summary>
        public const string Container = "AzureStorageContainer";

        /// <summary>
        /// Container acl cmdlet name
        /// </summary>
        public const string ContainerAcl = "AzureStorageContainerAcl";

        /// <summary>
        /// BlobContainerPublicAccessType is off
        /// </summary>
        public const string ContainerAclOff = "Off";

        /// <summary>
        /// BlobContainerPublicAccessType is blob
        /// </summary>
        public const string ContainerAclBlob = "Blob";

        /// <summary>
        /// BlobContainerPublicAccessType is container
        /// </summary>
        public const string ContainerAclContainer = "Container";

        /// <summary>
        /// Http protocol
        /// </summary>
        public const string HTTP = "Http";

        /// <summary>
        /// Https protocol
        /// </summary>
        public const string HTTPS = "Https";

        /// <summary>
        /// Queue cmdlet name
        /// </summary>
        public const string Queue = "AzureStorageQueue";

        /// <summary>
        /// Storage context cmdlet name
        /// </summary>
        public const string StorageContext = "AzureStorageContext";

        /// <summary>
        /// Storage account name
        /// </summary>
        public const string StorageAccountName = "Storage account name";

        /// <summary>
        /// Table cmdlet name
        /// </summary>
        public const string Table = "AzureStorageTable";

        /// <summary>
        /// Copy azure storage blob
        /// </summary>
        public const string CopyBlob = "AzureStorageBlobCopy";

        /// <summary>
        /// Copy azure storage blob deprecated name
        /// </summary>
        public const string CopyBlobDeprecatedName = "CopyAzureStorageBlob";

        /// <summary>
        /// Copy status for azure storage blob
        /// </summary>
        public const string CopyBlobStatus = "AzureStorageBlobCopyState";
    }
}
