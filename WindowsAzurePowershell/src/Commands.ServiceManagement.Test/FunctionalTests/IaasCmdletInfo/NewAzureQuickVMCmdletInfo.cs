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
    using Microsoft.WindowsAzure.ServiceManagement;
    using PowershellCore;

    public class NewAzureQuickVMCmdletInfo : CmdletsInfo
    {
        public NewAzureQuickVMCmdletInfo(OS os, string name, string serviceName, string imageName, string userName, string password)
        {
            cmdletName = Utilities.NewAzureQuickVMCmdletName;

            if (os == OS.Windows)
            {
                cmdletParams.Add(new CmdletParam("Windows", null));
                cmdletParams.Add(new CmdletParam("AdminUsername", userName));
                cmdletParams.Add(new CmdletParam("EnableGuestAgent", false));
            }
            else
            {
                cmdletParams.Add(new CmdletParam("Linux", null));
                cmdletParams.Add(new CmdletParam("LinuxUser", userName));
            }
            cmdletParams.Add(new CmdletParam("ImageName", imageName));
            cmdletParams.Add(new CmdletParam("Name", name));
            cmdletParams.Add(new CmdletParam("ServiceName", serviceName));                
            cmdletParams.Add(new CmdletParam("Password", password));
        }
        
        public NewAzureQuickVMCmdletInfo(OS os, string name, string serviceName, string imageName, string userName, string password, string locationName)
            : this(os, name, serviceName, imageName, userName, password)
        {
            if (!string.IsNullOrEmpty(locationName))
            {
                cmdletParams.Add(new CmdletParam("Location", locationName));
            }
        }

        public NewAzureQuickVMCmdletInfo(OS os, string name, string serviceName, string imageName, string userName, string password, string locationName, InstanceSize? instanceSize)
            : this(os, name, serviceName, imageName, userName, password, locationName)
        {
            if (instanceSize.HasValue)
            {
                cmdletParams.Add(new CmdletParam("InstanceSize", instanceSize.ToString()));
            }               
        }

        public NewAzureQuickVMCmdletInfo(

            OS os,
            string userName,
            string affinityGroup, 
            string availabilitySetName, 
            CertificateSettingList certificates, 
            DnsServer[] dnsSettings, 
            string hostCaching,
            string imageName,
            InstanceSize? instanceSize,            
            string location,
            string mediaLocation,
            string name,
            string password,
            string serviceName,
            LinuxProvisioningConfigurationSet.SSHKeyPairList sshKeyPairs,
            LinuxProvisioningConfigurationSet.SSHPublicKeyList sshPublicKeys,
            string[] subnetNames,
            string vnetName )
            : this(os, name, serviceName, imageName, userName, password, location, instanceSize)
        {
            
            if (!string.IsNullOrEmpty(affinityGroup))
            {
                cmdletParams.Add(new CmdletParam("AffinityGroup", affinityGroup));
            }
            if (!string.IsNullOrEmpty(availabilitySetName))
            {
                cmdletParams.Add(new CmdletParam("AvailabilitySetName", availabilitySetName));
            }
            if (certificates != null)
            {
                cmdletParams.Add(new CmdletParam("Certificates", certificates));
            }
            if (dnsSettings != null)
            {
                cmdletParams.Add(new CmdletParam("DnsSettings", dnsSettings));
            }
            if (!string.IsNullOrEmpty(hostCaching))
            {
                cmdletParams.Add(new CmdletParam("HostCaching", hostCaching));
            }                                                                     
            if (!string.IsNullOrEmpty(mediaLocation))
            {
                cmdletParams.Add(new CmdletParam("MediaLocation", mediaLocation));
            }                                    
            if (sshKeyPairs != null)
            {
                cmdletParams.Add(new CmdletParam("SSHKeyPairs", sshKeyPairs));
            }
            if (sshPublicKeys != null)
            {
                cmdletParams.Add(new CmdletParam("SSHPublicKeys", sshPublicKeys));
            }
            if (subnetNames != null)
            {
                cmdletParams.Add(new CmdletParam("SubnetNames", subnetNames));
            }
            if (!string.IsNullOrEmpty(vnetName))
            {
                cmdletParams.Add(new CmdletParam("VNetName", vnetName));
            }                               
        }
        
    }
}
