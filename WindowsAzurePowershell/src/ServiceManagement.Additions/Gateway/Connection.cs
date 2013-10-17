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

namespace Microsoft.WindowsAzure.Commands.Service.Gateway
{
    using System.Runtime.Serialization;

    [DataContract(Name = "Connection", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class Connection : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string ConnectivityState { get; set; }

        [DataMember(Order = 2)]
        public ulong EgressBytesTransferred { get; set; }

        [DataMember(Order = 3)]
        public ulong IngressBytesTransferred { get; set; }

        [DataMember(Order = 4)]
        public string LastConnectionEstablished { get; set; }

        [DataMember(Order = 5)]
        public GatewayEvent LastEvent { get; set; }

        [DataMember(Order = 6)]
        public string LocalNetworkSiteName { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
