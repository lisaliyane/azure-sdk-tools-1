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
    using ConfigDataInfo;
    using PowershellCore;

    public class AddAzureProvisioningConfigCmdletInfo : CmdletsInfo
    {
        public AddAzureProvisioningConfigCmdletInfo(AzureProvisioningConfigInfo provConfig)
        {
            this.cmdletName = Utilities.AddAzureProvisioningConfigCmdletName;

            this.cmdletParams.Add(new CmdletParam("VM", provConfig.Vm));

            this.cmdletParams.Add(new CmdletParam(provConfig.OS.ToString()));
            

            this.cmdletParams.Add(new CmdletParam("Password", provConfig.Password));

            if (provConfig.LinuxUser != null)
            {
                this.cmdletParams.Add(new CmdletParam("LinuxUser", provConfig.LinuxUser));
            }

            if (provConfig.AdminUsername != null)
            {
                this.cmdletParams.Add(new CmdletParam("AdminUsername", provConfig.AdminUsername));

            }
            if (provConfig.Option != null && provConfig.Option == "WindowsDomain")
            {
                this.cmdletParams.Add(new CmdletParam("WindowsDomain"));
            }
            if (provConfig.Domain != null)
            {
                this.cmdletParams.Add(new CmdletParam("Domain", provConfig.Domain));
            }
            if (provConfig.JoinDomain != null)
            {
                this.cmdletParams.Add(new CmdletParam("JoinDomain", provConfig.JoinDomain));
            }
            if (provConfig.DomainUserName != null)
            {
                this.cmdletParams.Add(new CmdletParam("DomainUserName", provConfig.DomainUserName));
            }
            if (provConfig.DomainPassword != null)
            {
                this.cmdletParams.Add(new CmdletParam("DomainPassword", provConfig.DomainPassword));
            }

            if (provConfig.Reset)
            {
                this.cmdletParams.Add(new CmdletParam("ResetPasswordInFirstLogon"));
            }


            if (provConfig.Certs != null && provConfig.Certs.Count != 0)
            {
                this.cmdletParams.Add(new CmdletParam("Certificates", provConfig.Certs));
            }

        }
    }
}
