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
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Storage.Common
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Model.Contract;

    /// <summary>
    /// base class for azure queue cmdlet
    /// </summary>
    public class StorageQueueBaseCmdlet : StorageCloudCmdletBase<IStorageQueueManagement>
    {
        /// <summary>
        /// get cloud queue client
        /// </summary>
        /// <returns>CloudQueueClient object</returns>
        private CloudQueueClient GetCloudQueueClient()
        {
            CloudStorageAccount account = GetCloudStorageAccount();
            return account.CreateCloudQueueClient();
        }

        /// <summary>
        /// create queue service management channel.
        /// </summary>
        /// <returns>IStorageQueueManagement object</returns>
        protected override IStorageQueueManagement CreateChannel()
        {
            //init storage blob managment channel
            if (Channel == null || !ShareChannel)
            {
                Channel = new StorageQueueManagement(GetCloudQueueClient());
            }

            return Channel;
        }
    }
}
