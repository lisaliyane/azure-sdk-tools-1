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

namespace Microsoft.WindowsAzure.Commands.Service.ResourceModel
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the Gateway Configuration a Virtual Network.
    /// </summary>
    [DataContract(Namespace = ServiceManagement.Constants.ServiceManagementNS)]
    public class VirtualNetworkGatewayConfiguration : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string GatewayIPAddress { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string GatewayMacAddress { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
