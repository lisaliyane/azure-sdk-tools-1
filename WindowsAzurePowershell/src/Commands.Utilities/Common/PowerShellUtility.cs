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
    using System.Diagnostics;
    using System.Management.Automation;

    public static class PowerShellUtility
    {
        public static PSObject ConstructPSObject(string typeName, params object[] args)
        {
            Debug.Assert(args.Length % 2 == 0, "The parameter args length must be even number");

            PSObject outputObject = new PSObject();

            if (!string.IsNullOrEmpty(typeName))
            {
                outputObject.TypeNames.Add(typeName);
            }

            for (int i = 0, j = 0; i < args.Length / 2; i++, j += 2)
            {
                outputObject.Properties.Add(new PSNoteProperty(args[j].ToString(), args[j + 1]));
            }

            return outputObject;
        }
    }
}
