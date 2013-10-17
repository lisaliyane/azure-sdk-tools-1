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
    using System.Net;
    using System.Runtime.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Xml;
    using Commands.Utilities.Common;
    using ServiceManagement;

    public static class GatewayManagementHelper
    {
        public static IGatewayServiceManagement CreateGatewayManagementChannel(Binding binding, Uri remoteUri, X509Certificate2 cert)
        {
            WebChannelFactory<IGatewayServiceManagement> factory = new WebChannelFactory<IGatewayServiceManagement>(binding, remoteUri);
            factory.Endpoint.Behaviors.Add(new ServiceManagementClientOutputMessageInspector());
            factory.Credentials.ClientCertificate.Certificate = cert;
            return factory.CreateChannel();
        }

        public static bool TryGetExceptionDetails(CommunicationException exception, out ServiceManagementError errorDetails)
        {
            HttpStatusCode httpStatusCode;
            string operationId;
            return TryGetExceptionDetails(exception, out errorDetails, out httpStatusCode, out operationId);
        }

        public static bool TryGetExceptionDetails(CommunicationException exception, out ServiceManagementError errorDetails, out HttpStatusCode httpStatusCode, out string operationId)
        {
            errorDetails = null;
            httpStatusCode = 0;
            operationId = null;

            if (exception == null)
            {
                return false;
            }

            if (exception.Message == "Internal Server Error")
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                return true;
            }

            var wex = exception.InnerException as WebException;

            if (wex == null)
            {
                return false;
            }

            var response = wex.Response as HttpWebResponse;
            if (response == null)
            {
                return false;
            }

            if (response.Headers != null)
            {
                operationId = response.Headers[Constants.OperationTrackingIdHeader];
            }

            using (var s = response.GetResponseStream())
            {
                try
                {
                    if (s == null || s.Length == 0)
                    {
                        return false;
                    }

                    var reader = XmlDictionaryReader.CreateTextReader(s, new XmlDictionaryReaderQuotas());
                    var ser = new DataContractSerializer(typeof(ServiceManagementError));
                    errorDetails = (ServiceManagementError)ser.ReadObject(reader, true);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
