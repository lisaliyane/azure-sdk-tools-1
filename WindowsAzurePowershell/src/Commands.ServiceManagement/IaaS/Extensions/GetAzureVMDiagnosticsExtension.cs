﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Model.PersistentVMModel;

    [Cmdlet(VerbsCommon.Get, "AzureVMDiagnosticsExtension"), OutputType(typeof(IEnumerable<VMDiagnosticExtensionContext>))]
    public class GetAzureVMDiagnosticsExtensionCommand : VirtualMachineConfigurationCmdletBase
    {
        internal void ExecuteCommand()
        {
            List<ResourceExtensionReference> daExtRefList = null;

            if (VM.GetInstance().ResourceExtensionReferences != null)
            {
                daExtRefList = VM.GetInstance().ResourceExtensionReferences.FindAll(
                    r => r.Name == VMDiagnosticsExtensionBuilder.ExtensionName && r.Publisher == VMDiagnosticsExtensionBuilder.ExtensionPublisher);
            }

            IEnumerable<VMDiagnosticExtensionContext> daExtContexts = daExtRefList == null ? null : daExtRefList.Select(
                r =>
                {
                    var extensionKeyValPair = r.ResourceExtensionParameterValues.Find(p => p.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr);
                    var daExtensionBuilder = extensionKeyValPair == null ? null : new VMDiagnosticsExtensionBuilder(extensionKeyValPair.Value);
                    return new VMDiagnosticExtensionContext
                    {
                        Name = r.Name,
                        Publisher = r.Publisher,
                        ReferenceName = r.ReferenceName,
                        Version = r.Version,
                        Enabled = daExtensionBuilder == null ? false : daExtensionBuilder.Enabled,
                        StorageAccountName = daExtensionBuilder == null ? string.Empty : daExtensionBuilder.StorageAccountName,
                        DiagnosticsConfiguration = daExtensionBuilder == null ? null : daExtensionBuilder.WadCfg
                    };
                });

            WriteObject(daExtContexts);
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
