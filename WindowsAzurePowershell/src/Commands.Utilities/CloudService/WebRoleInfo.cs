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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudService
{
    using System.Linq;
    using Common.XmlSchema.ServiceDefinitionSchema;

    /// <summary>
    /// RoleInfo implementation for WebRole
    /// </summary>
    public class WebRoleInfo : RoleInfo
    {
        public WebRoleInfo(string name, int instanceCount) : base(name, instanceCount) { }

        internal override void AddRoleToDefinition(ServiceDefinition def, object template)
        {
            WebRole webRole = template as WebRole;
            var toAdd = new WebRole[] { webRole };

            if (def.WebRole != null)
            {
                def.WebRole = def.WebRole.Concat(toAdd).ToArray();
            }
            else
            {
                def.WebRole = toAdd;
            }
        }
    }
}