// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Storage.Table
{
    using Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Model.Contract;

    /// <summary>
    /// base class for table cmdlet
    /// </summary>
    public class StorageCloudTableCmdletBase : StorageCloudCmdletBase<IStorageTableManagement>
    {
        /// <summary>
        /// get table client
        /// </summary>
        /// <returns>a CloudTableClient object</returns>
        private CloudTableClient GetCloudTableClient()
        {
            CloudStorageAccount account = GetCloudStorageAccount();
            return account.CreateCloudTableClient();
        }

        /// <summary>
        /// create table storage service management channel.
        /// </summary>
        /// <returns>IStorageTableManagement object</returns>
        protected override IStorageTableManagement CreateChannel()
        {
            //Init storage table management channel
            if (Channel == null || !ShareChannel)
            {
                Channel = new StorageTableManagement(GetCloudTableClient());
            }

            return Channel;
        }
    }
}
