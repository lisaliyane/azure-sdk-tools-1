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

    public class SwitchDefinition
    {
        private SwitchDefinition()
        {
        }
        public string Name { get; private set; }
        public string Description { get; set; }
        public bool IsFlag { get; private set; }
        public bool Required { get; private set; }
        public bool Undocumented { get; set;  }
        public string SwitchFormat { get; set; }
        
        public static SwitchDefinition ParamWithFormat(string name, string description, bool required, string format)
        {
            return new SwitchDefinition()
            {
                Name = name,
                Description = description,
                IsFlag = false,
                Required = required,
                SwitchFormat = format
            };
        }

        public static SwitchDefinition Param(string name, string description, bool required)
        {
            return new SwitchDefinition()
            {
                Name = name,
                Description = description,
                IsFlag = false,
                Required = required
            };
        }
        public static SwitchDefinition Flag(string name, string description)
        {
            return new SwitchDefinition()
            {
                Name = name,
                Description = description,
                IsFlag = true,
                Required = false
            };
        }
    }
}
