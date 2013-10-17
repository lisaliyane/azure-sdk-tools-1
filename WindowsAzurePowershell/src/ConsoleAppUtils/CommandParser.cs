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
    using System.Linq;

    public class CommandParser
    {
        public const string SwitchPrefix = "-";
        protected IDictionary<string,CommandDefinition> Commands { get; private set;}
        protected ICommandParserOutputEvents Output { get; private set; }
        public IEnumerable<string> DefinedCommands { get { return Commands.Keys.OrderBy(s => s); } }
        protected IDictionary<string, CommandDefinition> Alias { get; private set; }
        public CommandParser(ICommandParserOutputEvents outEvts)
        {
            Output = outEvts;
            Commands = new Dictionary<string,CommandDefinition>(StringComparer.InvariantCultureIgnoreCase);
            Alias = new Dictionary<string, CommandDefinition>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddLegacyAlias(string alias, string commandName)
        {
            var newCmdDef = Commands[commandName];
            var oldCmdDef = new CommandDefinition(alias, newCmdDef.MinArgs, newCmdDef.MaxArgs, newCmdDef.Switches.Values.ToArray());
            oldCmdDef.Description =
                String.Format("This command is deprecated. Please use the command {0}.", commandName); // TODO move this into a resource.
            oldCmdDef.Category = newCmdDef.Category;
            oldCmdDef.Action = (args, switches) =>
                {
                    Output.CommandDeprecated(oldCmdDef, newCmdDef);
                    return newCmdDef.Action(args, switches);
                };
            Alias.Add(alias, oldCmdDef);
        }

        public void AddAlias(string alias, string commandName)
        {
            var cmdDef = Commands[commandName];
            Alias.Add(alias, cmdDef);
        }
        public void Add(params CommandDefinition[] cmds)
        {
            foreach (var cmd in cmds)
            {
                Commands.Add(cmd.Name, cmd);
            }
        }
        private string Shift(List<string> toks)
        {
            var ret = toks[0];
            toks.RemoveAt(0);
            return ret;
        }
        private bool IsSwitch(string x)
        {
            return x.StartsWith(SwitchPrefix); 
        }
        private SwitchDefinition FindSwitch(CommandDefinition cdef, string tok)
        {
            SwitchDefinition swdef = null;
            var swname = tok.Substring(SwitchPrefix.Length);
            if (cdef.Switches.TryGetValue(swname, out swdef))
            {
                return swdef;
            }
            return null;
        }

        public string SwitchUsage(SwitchDefinition swdef)
        {
            string argSpec;
            if (swdef.IsFlag)
            {
                argSpec = "";
            }
            else
            {
                if(swdef.SwitchFormat != null)
                {
                    argSpec = " " + swdef.SwitchFormat;
                }
                else
                {
                    argSpec = " <string>";
                }
            }
            if (swdef.Required)
            {
                return String.Format("{0}{1}{2}", SwitchPrefix, swdef.Name, argSpec);
            }
            else
            {
                return String.Format("[{0}{1}{2}]", SwitchPrefix, swdef.Name, argSpec);
            }
        }
        
        private bool ParseArguments(
            CommandDefinition cmdDef, List<string> toks,
            List<string> unnamedArgs, Dictionary<string, string> switches)
        {
            while (toks.Count > 0)
            {
                var tok = Shift(toks);
                if (IsSwitch(tok))
                {
                    var swdef = FindSwitch(cmdDef, tok);
                    if (swdef == null)
                    {
                        Output.CommandParamUnknown(cmdDef.Name, tok);
                        return false;
                    }
                    if (switches.ContainsKey(swdef.Name))
                    {
                        var keyValue = switches[swdef.Name];
                        Output.CommandDuplicateParam(SwitchUsage(swdef));
                        return false;
                    }
                    if (swdef.IsFlag)
                    {
                        switches.Add(swdef.Name, true.ToString());
                    }
                    else if (toks.Count > 0)
                    {
                        var switchArg = Shift(toks);
                        switches.Add(swdef.Name, switchArg);
                    }
                    else
                    {
                        Output.CommandParamMissingArgument(cmdDef.Name, SwitchUsage(swdef));
                        return false;
                    }
                }
                else
                {
                    unnamedArgs.Add(tok);
                }
            }
            return true;
        }
        private bool ValidateArguments(
            CommandDefinition cmdDef,
            List<string> unnamedArgs, Dictionary<string, string> switches)
        {
            bool success = true;
            int found = unnamedArgs.Count;
            if (found < cmdDef.MinArgs)
            {
                Output.CommandTooFewArguments(cmdDef.Name, cmdDef.MinArgs, found, unnamedArgs);
                success = false;
            }
            else if (cmdDef.MaxArgs.HasValue)
            {
                int expected = cmdDef.MaxArgs.Value;
                if (found > expected)
                {
                    Output.CommandTooManyArguments(cmdDef.Name, expected, found , unnamedArgs);
                    success = false;
                }
            }

            foreach (var sw in cmdDef.Switches.Values)
            {
                if (!switches.ContainsKey(sw.Name))
                {
                    if (sw.Required)
                    {
                        Output.CommandMissingParam(cmdDef.Name, SwitchUsage(sw));
                        success = false;
                    }
                    else
                    {
                        switches.Add(sw.Name,null);
                    }
                }
            }
            return success;
        }
        public void ShowCommands()
        {
             Output.Format(Commands.Values);
        }

        private bool ParseCommand(CommandDefinition cmdDef, IEnumerable<string> commandArgs)
        {
            List<string> toks = new List<string>(commandArgs);
            List<string> unnamedArgs = new List<string>();
            Dictionary<string, string> switches = new Dictionary<string, string>();
            if (ParseArguments(cmdDef, toks, unnamedArgs, switches) &&
                ValidateArguments(cmdDef, unnamedArgs, switches))
            {
                if (cmdDef.Action != null)
                {
                    return cmdDef.Action(unnamedArgs, switches);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            
        }

        public bool ShowUsage(string cmdName)
        {
            CommandDefinition cmdDef;
            if (TryGetCommand(cmdName, out cmdDef))
            {
                Output.CommandUsage(cmdDef,SwitchUsage);
                return true;
            }
            else
            {
                Output.CommandUnknown(cmdName);
                return false;
            }
        }

        public bool ParseCommand(IEnumerable<string> commandLine)
        {
            var cmd = commandLine.FirstOrDefault();
            if (String.IsNullOrEmpty(cmd))
            {
                Output.CommandMissing();
                Output.Format(Commands.Values);
                return false;
            }
            CommandDefinition cmdDef;
            if (TryGetCommand(cmd, out cmdDef))
            {
                var args = commandLine.Skip(1);
                return ParseCommand(cmdDef, args);
            }
            else
            {
                Output.CommandUnknown(cmd);
                return false;
            }
        }
        private bool TryGetCommand(string cmd, out CommandDefinition cmdDef)
        {
            if(Commands.TryGetValue(cmd, out cmdDef))
            {
                return true;
            }
            return Alias.TryGetValue(cmd,out cmdDef);
        }
    }
}
