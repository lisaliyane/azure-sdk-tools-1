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
    using System;
    using System.Collections.Generic;

    public delegate bool CommandAction(IList<string> unnamedArgs, IDictionary<string, string> switches);

    public class CommandDefinition
    {
        public CommandDefinition(
            string name,
            int minArgs, 
            int? maxArgs, 
            params SwitchDefinition[] switches)
        {
            MaxArgs = maxArgs;
            MinArgs = minArgs;
            Name = name;
            Switches = new Dictionary<string, SwitchDefinition>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var sw in switches)
            {
                Switches.Add(sw.Name, sw);
            }
        }
        public string Name { get; private set; }
        public int? MaxArgs { get; private set; }
        public int  MinArgs { get; private set; }
        public string Description { get; set; }
        public IDictionary<string, SwitchDefinition> Switches { get; private set; }
        public CommandAction Action { get; set; }
        public CommandCategory Category { get; set; }
    }

    [Flags]
    public enum CommandCategory
    {
        None = 0,
        IaaS = 2,
        VmRole = 4
    }
}
