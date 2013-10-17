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

namespace Microsoft.WindowsAzure.Commands.Internal.Common
{

    public abstract class StandardOutputEvents
    {
        public abstract void LogError(int ecode, string fmt, params object[] args);
        public abstract void LogWarning(int ecode, string fmt, params object[] args);
        public abstract void LogMessage(int ecode, string fmt, params object[] args);
        public abstract void LogImportantMessage(int ecode, string fmt, params object[] args);
        public abstract void LogDebug(string fmt, params object[] args);
        public abstract void LogProgress(string fmt, params object[] args);

        public void LogError(string msg)
        {
            LogError(0, "{0}", msg);
        }
        public void LogError(string fmt, params object[] args)
        {
            LogError(0, fmt, args);
        }
        public void LogError(int ecode, string msg)
        {
            LogError(ecode, "{0}", msg);
        }
        public void LogWarning(string msg)
        {
            LogWarning(0, "{0}", msg);
        }
        public void LogWarning(string fmt, params object[] args)
        {
            LogWarning(0, fmt, args);
        }
        public void LogWarning(int ecode, string msg)
        {
            LogWarning(ecode, "{0}", msg);
        }
        public void LogMessage(string msg)
        {
            LogMessage(0, "{0}", msg);
        }
        public void LogMessage(string fmt, params object[] args)
        {
            LogMessage(0, fmt, args);
        }
        public void LogMessage(int ecode, string msg)
        {
            LogMessage(ecode, "{0}", msg);
        }
        public void LogDebug(string msg)
        {
            LogDebug("{0}", msg);
        }

        public void LogProgress(string msg)
        {
            LogProgress("{0}", msg);
        }
        public void LogImportantMessage(string fmt, params object[] args)
        {
            LogImportantMessage(0, fmt, args);
        }
        public void LogImportantMessage(int ecode, string msg)
        {
            LogImportantMessage(ecode, "{0}", msg);
        }
    }
}
