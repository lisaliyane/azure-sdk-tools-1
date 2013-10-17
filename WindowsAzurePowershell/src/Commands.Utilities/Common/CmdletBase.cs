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
    using System;
    using System.Diagnostics;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;

    public abstract class CmdletBase : PSCmdlet, IDynamicParameters
    {
        protected string CurrentPath()
        {
            // SessionState is only available within Powershell so default to
            // the CurrentDirectory when being run from tests.
            return (SessionState != null) ?
                SessionState.Path.CurrentLocation.Path :
                Environment.CurrentDirectory;
        }

        protected bool IsVerbose()
        {
            bool verbose = MyInvocation.BoundParameters.ContainsKey("Verbose") && ((SwitchParameter)MyInvocation.BoundParameters["Verbose"]).ToBool();
            return verbose;
        }

        public virtual object GetDynamicParameters()
        {
            return null;
        }

        protected void WriteVerboseWithTimestamp(string message, params object[] args)
        {
            WriteVerbose(string.Format("{0:T} - {1}", DateTime.Now, string.Format(message, args)));
        }

        protected void WriteVerboseWithTimestamp(string message)
        {
            WriteVerbose(string.Format("{0:T} - {1}", DateTime.Now, message));
        }

        protected void WriteDebugWithTimestamp(string message, params object[] args)
        {
            WriteDebug(string.Format("{0:T} - {1}", DateTime.Now, string.Format(message, args)));
        }

        protected void WriteDebugWithTimestamp(string message)
        {
            WriteDebug(string.Format("{0:T} - {1}", DateTime.Now, message));
        }

        /// <summary>
        /// Write an error message for a given exception.
        /// </summary>
        /// <param name="ex">The exception resulting from the error.</param>
        protected virtual void WriteExceptionError(Exception ex)
        {
            Debug.Assert(ex != null, "ex cannot be null or empty.");
            WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
        }

        protected PSObject ConstructPSObject(string typeName, params object[] args)
        {
            return PowerShellUtility.ConstructPSObject(typeName, args);
        }

        protected void SafeWriteOutputPSObject(string typeName, params object[] args)
        {
            PSObject customObject = this.ConstructPSObject(typeName, args);
            WriteObject(customObject);
        }

        public virtual void ExecuteCmdlet()
        {
            // Do nothing.
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ExecuteCmdlet();
            }
            catch (Exception ex)
            {
                WriteExceptionError(ex);
            }
            finally { WriteDebug(HttpRestCallLogger.Flush()); }
        }

        /// <summary>
        /// Cmdlet begin process
        /// </summary>
        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(ParameterSetName))
            {
                WriteDebugWithTimestamp(string.Format(Resources.BeginProcessingWithoutParameterSetLog, this.GetType().Name));
            }
            else
            {
                WriteDebugWithTimestamp(string.Format(Resources.BeginProcessingWithParameterSetLog, this.GetType().Name, ParameterSetName));
            }

            base.BeginProcessing();
        }

        /// <summary>
        /// End processing
        /// </summary>
        protected override void EndProcessing()
        {
            string message = string.Format(Resources.EndProcessingLog, this.GetType().Name);
            WriteDebugWithTimestamp(message);

            base.EndProcessing();
        }

        /// <summary>
        /// Asks for confirmation before executing the action.
        /// </summary>
        /// <param name="force">Do not ask for confirmation</param>
        /// <param name="actionMessage">Message to describe the action</param>
        /// <param name="processMessage">Message to prompt after the active is performed.</param>
        /// <param name="target">The target name.</param>
        /// <param name="action">The action code</param>
        protected void ConfirmAction(bool force, string actionMessage, string processMessage, string target, Action action)
        {
            if (force || ShouldContinue(actionMessage, ""))
            {
                if (ShouldProcess(target, processMessage))
                {
                    action();
                }
            }
        }
    }
}
