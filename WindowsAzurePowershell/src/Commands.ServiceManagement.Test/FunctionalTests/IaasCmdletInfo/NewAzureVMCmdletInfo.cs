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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{
    using Model;
    using Model.PersistentVMModel;
    using PowershellCore;

    public class NewAzureVMCmdletInfo : CmdletsInfo
    {

        public NewAzureVMCmdletInfo(string serviceName, PersistentVM[] vMs, string vnetName, DnsServer[] dnsSettings,
            string serviceLabel, string serviceDescription, string deploymentLabel, string deploymentDescription, string location, string affinityGroup)
        {
            this.cmdletName = Utilities.NewAzureVMCmdletName;

            this.cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
            this.cmdletParams.Add(new CmdletParam("VMs", vMs));
            if (vnetName != null)
            {
                this.cmdletParams.Add(new CmdletParam("VNetName", vnetName));
            }
            if (dnsSettings != null)
            {
                this.cmdletParams.Add(new CmdletParam("DnsSettings", dnsSettings));
            }
            if (affinityGroup != null)
            {
                this.cmdletParams.Add(new CmdletParam("AffinityGroup", affinityGroup));
            }
            if (serviceLabel != null)
            {
                this.cmdletParams.Add(new CmdletParam("ServiceLabel", serviceLabel));
            }
            if (serviceDescription != null)
            {
                this.cmdletParams.Add(new CmdletParam("ServiceDescription", serviceDescription));
            }
            if (deploymentLabel != null)
            {
                this.cmdletParams.Add(new CmdletParam("DeploymentLabel", deploymentLabel));
            }
            if (deploymentDescription != null)
            {
                this.cmdletParams.Add(new CmdletParam("DeploymentDescription", deploymentDescription));
            }
            if (location != null)
            {
                this.cmdletParams.Add(new CmdletParam("Location", location));
            }
        }
    }
}
