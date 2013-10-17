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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudService.AzureTools
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Win32;

    public class AzureTool
    {
        public string AzureSdkDirectory { get; private set; }
        public string AzureSdkBinDirectory { get; private set; }
        public string AzureEmulatorDirectory { get; private set; }
        public string AzureSdkVersion { get; private set; }

        public static void Validate()
        {
            // This instantiation will throw if user is running with incompatible Windows Azure SDK version.
            new AzureTool();
        }

        public AzureTool()
        {
            string min = Resources.MinSupportAzureSdkVersion;
            string max = Resources.MaxSupportAzureSdkVersion;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Resources.AzureSdkRegistryKeyName);
            if (key == null)
            {
                throw new Exception(Resources.AzureToolsNotInstalledMessage);
            }

            AzureSdkVersion = key.GetSubKeyNames().Where(n => (n.CompareTo(min) == 1 && n.CompareTo(max) == -1) || n.CompareTo(min) == 0 || n.CompareTo(max) == 0).Max<string>();
            
            if (string.IsNullOrEmpty(AzureSdkVersion) && key.GetSubKeyNames().Length > 0)
            {
                throw new Exception(string.Format(Resources.AzureSdkVersionNotSupported, min, max));
            }
            else if (string.IsNullOrEmpty(AzureSdkVersion) && key.GetSubKeyNames().Length == 0)
            {
                throw new Exception(Resources.AzureToolsNotInstalledMessage);
            }
            else
            {
                string keyName = Path.Combine(Resources.AzureSdkRegistryKeyName, AzureSdkVersion);
                AzureSdkDirectory = (string)Registry.GetValue(Path.Combine(Registry.LocalMachine.Name, keyName), Resources.AzureSdkInstallPathRegistryKeyValue, null);
                AzureSdkBinDirectory = Path.Combine(AzureSdkDirectory, Resources.RoleBinFolderName);

                var emulatorPath = Registry.GetValue(Path.Combine(Registry.LocalMachine.Name, Resources.AzureEmulatorRegistryKey), Resources.AzureSdkInstallPathRegistryKeyValue, null);
                if (emulatorPath == null)
                {
                    throw new Exception(Resources.AzureEmulatorNotInstalledMessage);
                }

                AzureEmulatorDirectory = Path.Combine((string)emulatorPath, 
                    Resources.AzureEmulatorDirectory);
            }

            // Removes the leading 'v'
            AzureSdkVersion = AzureSdkVersion.Remove(0, 1);

            // Add build version if it does not exist. For example, if the version is 1.7
            // this code changes it to be 1.7.0
            if (AzureSdkVersion.Split('.').Length == 2)
            {
                AzureSdkVersion = AzureSdkVersion + ".0";
            }
        }
    }
}