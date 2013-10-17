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

namespace Microsoft.WindowsAzure.Commands.CloudService.Development
{
    using System.IO;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Text;
    using Utilities.Common;
    using Utilities.Properties;
    using Utilities.CloudService;
    using Utilities.CloudService.AzureTools;

    /// <summary>
    /// Runs the service in the emulator
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AzureEmulator"), OutputType(typeof(CloudServiceProject))]
    public class StartAzureEmulatorCommand : CmdletBase
    {
        [Parameter(Mandatory = false)]
        [Alias("ln")]
        public SwitchParameter Launch { get; set; }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public CloudServiceProject StartAzureEmulatorProcess(string rootPath)
        {
            string standardOutput;
            string standardError;

            StringBuilder message = new StringBuilder();
            CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath ,null);

            if (Directory.Exists(cloudServiceProject.Paths.LocalPackage))
            {
                WriteVerbose(Resources.StopEmulatorMessage);
                cloudServiceProject.StopEmulator(out standardOutput, out standardError);
                WriteVerbose(Resources.StoppedEmulatorMessage);
                WriteVerbose(string.Format(Resources.RemovePackage, cloudServiceProject.Paths.LocalPackage));
                Directory.Delete(cloudServiceProject.Paths.LocalPackage, true);
            }
            
            WriteVerbose(string.Format(Resources.CreatingPackageMessage, "local"));
            cloudServiceProject.CreatePackage(DevEnv.Local, out standardOutput, out standardError);
            
            WriteVerbose(Resources.StartingEmulator);
            cloudServiceProject.ResolveRuntimePackageUrls();
            cloudServiceProject.StartEmulator(Launch.ToBool(), out standardOutput, out standardError);
            
            WriteVerbose(standardOutput);
            WriteVerbose(Resources.StartedEmulator);
            SafeWriteOutputPSObject(
                cloudServiceProject.GetType().FullName,
                Parameters.ServiceName, cloudServiceProject.ServiceName,
                Parameters.RootPath, cloudServiceProject.Paths.RootPath);

            return cloudServiceProject;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            AzureTool.Validate();
            base.ExecuteCmdlet();
            StartAzureEmulatorProcess(General.GetServiceRootPath(CurrentPath()));
        }
    }
}