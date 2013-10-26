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
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Model;
    using Model.PersistentVMModel;

    public class AzureProvisioningConfigInfo
    {
        public string WindowsDomain = "WindowsDomain";

        public OS OS;
        public string Option = null;

        public readonly string Password;
        public CertificateSettingList Certs =  new CertificateSettingList();
        public string LinuxUser = null;
        public string AdminUsername = null;

        public string JoinDomain = null;
        public string Domain = null;
        public string DomainUserName = null;
        public string DomainPassword = null;
        public string MachineObjectOU = null;

        public bool Reset = false;
        public bool DisableAutomaticUpdate = false;
        public bool DisableSSH = false;

        public bool DisableWinRMHttps = false;
        public bool EnableWinRMHttp = false;
        public bool NoWinRMEndpoint = false;
        public X509Certificate2 WinRMCertificate = null;
        public X509Certificate2[] X509Certificates = null;

        public bool NoExportPrivateKey = false;
        public bool NoRDPEndpoint = false;
        public bool NoSSHEndpoint = false;

        public LinuxProvisioningConfigurationSet.SSHKeyPairList SSHKeyPairs = null;
        public LinuxProvisioningConfigurationSet.SSHPublicKeyList SshPublicKeys = null;
        public string TimeZone = null;

        // WindowsDomain paramenter set
        public AzureProvisioningConfigInfo(string option, string user, string password, string joinDomain, string domain, string domainUserName, string domainPassword, string objectOU = null)
        {
            if (string.Compare(option, WindowsDomain, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                this.Option = WindowsDomain;
                this.AdminUsername = user;
                this.Password = password;
                this.Domain = domain;
                this.JoinDomain = joinDomain;
                this.DomainUserName = domainUserName;
                this.DomainPassword = domainPassword;
                this.MachineObjectOU = objectOU;
            }
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
