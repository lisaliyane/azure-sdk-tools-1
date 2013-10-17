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

using Microsoft.WindowsAzure.Management.MediaServices.Models;

namespace Microsoft.WindowsAzure.Commands.Utilities.MediaServices.Services.Entities
{

    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [JsonObject(Title = "AccountDetails")]
    [DataContract(Namespace = MediaServicesUriElements.AccountDetailsNamespace, Name = "AccountDetails")]
    public class MediaServiceAccountDetails
    {
        public MediaServiceAccountDetails(MediaServicesAccountGetResponse response)
        {
            this.AccountKeys = response.StorageAccountKeys;
            this.Location = response.AccountRegion;
            this.StorageAccountName = response.StorageAccountName;
            this.Name = response.AccountName;
        }
        [DataMember]
        internal string AccountKey { get; set; }

        [DataMember]
        internal MediaServicesAccountGetResponse.AccountKeys AccountKeys { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "AccountName")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "AccountRegion")]
        public string Location { get; set; }

        [DataMember]
        public string StorageAccountName { get; set; }

        public string MediaServicesPrimaryAccountKey
        {
            get { return AccountKeys.Primary; }
        }

        public string MediaServicesSecondaryAccountKey
        {
            get { return AccountKeys.Secondary; }
        }
    }
}