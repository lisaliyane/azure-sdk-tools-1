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
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using ServiceManagement;

    public partial interface IGatewayServiceManagement
    {
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(UriTemplate = "subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway", Method = "POST")]
        IAsyncResult BeginCreateGateway(string subscriptionId, string vnetName, AsyncCallback callback, object state);

        GatewayOperationAsyncResponse EndCreateGateway(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(UriTemplate = "subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway", Method = "DELETE")]
        IAsyncResult BeginDeleteGateway(string subscriptionId, string vnetName, AsyncCallback callback, object state);

        GatewayOperationAsyncResponse EndDeleteGateway(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = "subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway")]
        IAsyncResult BeginGetGateway(string subscriptionId, string vnetName, AsyncCallback callback, object state);

        VnetGateway EndGetGateway(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = "subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway/connection/{localNetworkSiteName}/sharedkey")]
        IAsyncResult BeginGetSharedKey(string subscriptionId, string vnetName, string localNetworkSiteName, AsyncCallback callback, object state);
        
        SharedKey EndGetSharedKey(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = "subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway/connections")]
        IAsyncResult BeginListConnections(string subscriptionId, string vnetName, AsyncCallback callback, object state);

        ConnectionCollection EndListConnections(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = "subscriptions/{subscriptionId}/Services/networking/supporteddevices")]
        IAsyncResult BeginListSupportedDevices(string subscriptionId, AsyncCallback callback, object state);

        Stream EndListSupportedDevices(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(UriTemplate = "subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway/connection/{localNetworkSiteName}", Method = "PUT")]
        IAsyncResult BeginUpdateConnection(string subscriptionId, string vnetName, string localNetworkSiteName, UpdateConnection updateConnection, AsyncCallback callback, object state);
        
        GatewayOperationAsyncResponse EndUpdateConnection(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = "{subscriptionId}/operations/{operationId}")]
        IAsyncResult BeginGetGatewayOperation(string subscriptionId, string operationId, AsyncCallback callback, object state);
        
        Operation EndGetGatewayOperation(IAsyncResult asyncResult);
    }
}