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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System.Net.Http;
    using System.Text;
    using System.Threading;

    public class HttpRestCallLogger : MessageProcessingHandler
    {
        private static StringBuilder HttpLog = new StringBuilder();

        public static string Flush()
        {
            string logs = HttpLog.ToString();
            HttpLog.Clear();
            
            return logs;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            HttpLog.Append(General.GetLog(response));
            return response;
        }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpLog.Append(General.GetLog(request));
            return request;
        }
    }

}
