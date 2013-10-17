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
    using System.IO;
    using ServiceManagement;

    public static partial class GatewayServiceManagementExtensions
    {
        public static GatewayOperationAsyncResponse NewVirtualNetworkGateway(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName)
        {
            return proxy.EndCreateGateway(proxy.BeginCreateGateway(subscriptionId, virtualNetworkName, null, null));
        }

        public static GatewayOperationAsyncResponse DeleteVirtualNetworkGateway(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName)
        {
            return proxy.EndDeleteGateway(proxy.BeginDeleteGateway(subscriptionId, virtualNetworkName, null, null));
        }

        public static ConnectionCollection ListVirtualNetworkConnections(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName)
        {
            return proxy.EndListConnections(proxy.BeginListConnections(subscriptionId, virtualNetworkName, null, null));
        }

        public static SharedKey GetVirtualNetworkSharedKey(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName, string localNetworkSiteName)
        {
            return proxy.EndGetSharedKey(proxy.BeginGetSharedKey(subscriptionId, virtualNetworkName, localNetworkSiteName, null, null));
        }

        public static VnetGateway GetVirtualNetworkGateway(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName)
        {
            return proxy.EndGetGateway(proxy.BeginGetGateway(subscriptionId, virtualNetworkName, null, null));
        }

        public static Stream GetVirtualNetworkSupportedDevices(this IGatewayServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListSupportedDevices(proxy.BeginListSupportedDevices(subscriptionId, null, null));
        }

        public static GatewayOperationAsyncResponse UpdateVirtualNetworkGatewayConnection(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName, string localNetworkSiteName, UpdateConnection updateConnection)
        {
            return proxy.EndUpdateConnection(proxy.BeginUpdateConnection(subscriptionId, virtualNetworkName, localNetworkSiteName, updateConnection, null, null));
        }

        public static Operation GetGatewayOperation(this IGatewayServiceManagement proxy, string subscriptionId, string operationId)
        {
            return proxy.EndGetGatewayOperation(proxy.BeginGetGatewayOperation(subscriptionId, operationId, null, null));
        }
    }
}
