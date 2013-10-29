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
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Net.Http;
    using System.Threading;

    public class HttpRestCallLogger : MessageProcessingHandler
    {
        public static PSCmdlet CurrentCmdlet { get; set; }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            WriteLog(General.GetLog(response));
            return response;
        }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            WriteLog(General.GetLog(request));
            return request;
        }

        private void WriteLog(string log)
        {
            if (CurrentCmdlet.MyInvocation.BoundParameters.ContainsKey("Debug"))
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = RunspaceFactory.CreateRunspace(CurrentCmdlet.Host);
                    ps.Runspace.Open();
                    ps.AddScript("$DebugPreference='Continue'");
                    ps.AddScript(string.Format("Write-Debug @'\n{0}\n'@", log));
                    ps.Invoke();
                }
            }
        }
    }

}
