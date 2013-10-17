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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ConfigDataInfo
{
    using Model;
    using Model.PersistentVMModel;

    public class AzureProvisioningConfigInfo
    {
        public OS OS;
        public readonly string Password;
        public CertificateSettingList Certs =  new CertificateSettingList();
        public string LinuxUser = (string) null;
        public string AdminUsername = (string)null;
        public string Option = (string) null;
        public string JoinDomain = (string)null;
        public string Domain = (string)null;
        public string DomainUserName = (string)null;
        public string DomainPassword = (string)null;
        public bool Reset = false;
        public bool DisableAutomaticUpdate = false;
        public bool DisableSSH = false;
        public bool NoRDPEndpoint = false;
        public bool NoSSHEndpoint = false;

        public AzureProvisioningConfigInfo(string option, string joinDomain, string domain, string domainUserName, string domainPassword,  string password, bool resetPasswordFirstLogon)
        {
            this.Option = option;
            this.Password = password;
            this.Domain = domain;
            this.JoinDomain = joinDomain;
            this.DomainUserName = domainUserName;
            this.DomainPassword = domainPassword;
            this.Reset = resetPasswordFirstLogon;
        }

        public AzureProvisioningConfigInfo(OS os, string user, string password)
        {
            this.OS = os;
            this.Password = password;
            if (os == OS.Windows)
            {
                this.AdminUsername = user;
            }
            else
            {
                this.LinuxUser = user;
            }
        }

        public AzureProvisioningConfigInfo(OS os, CertificateSettingList certs, string user, string password)
        {
            this.OS = os;            
            this.Password = password;
            foreach (CertificateSetting cert in certs)
            {
                Certs.Add(cert);
            }
            if (os == OS.Windows)
            {
                this.AdminUsername = user;
            }
            else
            {
                this.LinuxUser = user;
            }

        }


        public PersistentVM  Vm { get; set; }
    }
}
