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
    using System.Diagnostics;
    using System.Linq;

    public class ConsoleApplicationStandardOutputEvents : 
        StandardOutputEvents,
        ICommandParserOutputEvents
    {
        static protected ConsoleColor DefaultForegroundColor { get; private set; }
        static protected bool SetCursorPostionSupported { get; private set;}
        static public bool Verbose { get; set; }
        protected TraceSource TraceSource { get; set; }
        public int NumberOfErrors { get; private set; }
        public int NumberOfWarnings { get; private set; }
        public bool HasErrors { get { return NumberOfErrors > 0; } }
        public bool HasWarnings { get { return NumberOfWarnings > 0; } }
       
        protected ConsoleApplicationStandardOutputEvents()
        {
            
        }
        public ConsoleApplicationStandardOutputEvents(TraceSource traceSource)
        {
            TraceSource = traceSource;
        }

        static ConsoleApplicationStandardOutputEvents()
        {
            if (Environment.GetEnvironmentVariable("_CSVERBOSE") != null)
            {
                Verbose = true;
            }
            DefaultForegroundColor = Console.ForegroundColor;
            try
            {
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
                SetCursorPostionSupported = true;
            }
            catch (Exception)
            {
                SetCursorPostionSupported = false;
            }
        }
        
        override public void LogWarning(int ecode, string fmt, params object[] args)
        {
            NumberOfWarnings++;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(fmt,args);
            Console.ForegroundColor = DefaultForegroundColor;
            if (TraceSource != null)
            {
                TraceSource.TraceEvent(TraceEventType.Warning, ecode, fmt, args);
            }
        }
        
        override public void LogError(int ecode, string fmt, params object[] args)
        {
            NumberOfErrors++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(fmt, args);
            Console.ForegroundColor = DefaultForegroundColor;
            if (TraceSource != null)
            {
                TraceSource.TraceEvent(TraceEventType.Error, ecode, fmt, args);
            }
        }

        override public void LogImportantMessage(int ecode, string fmt, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(fmt, args);
            Console.ForegroundColor = DefaultForegroundColor;
            if (TraceSource != null)
            {
                TraceSource.TraceEvent(TraceEventType.Information, ecode, fmt, args);
            }
        }

        override public void LogMessage(int ecode, string fmt, params object[] args)
        {
            Console.WriteLine(fmt, args);
            if (TraceSource != null)
            {
                TraceSource.TraceEvent(TraceEventType.Information, ecode, fmt, args);
            }
        }

        override public void LogDebug(string fmt, params object[] args)
        {
            if (Verbose)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(fmt, args);
                Console.ForegroundColor = DefaultForegroundColor;
            }
            if (TraceSource != null)
            {
                TraceSource.TraceEvent(TraceEventType.Verbose, 0, fmt, args);
            }
        }

        override public void LogProgress(string fmt, params object[] args)
        {
            if (SetCursorPostionSupported)
            {
                int lineWidth = Console.BufferWidth;
                var emptyLine = new String(' ', lineWidth);
                Console.Write(emptyLine);
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine(fmt, args);             
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            else
            {
                Console.WriteLine(fmt, args);
            }
        }

        public void CommandUnknown(string commandName)
        {
            LogError("Command {0} is unknown.", commandName);
        }

        public void CommandTooManyArguments(string commandName, int expected, int found, IEnumerable<string> args)
        {
            var fmtArgs = "'" + string.Join("', '",args.ToArray()) + "'";
            LogError("Too many arguments for command {0} expects {1} argument(s) found {2}.\nFound: {3}", 
                commandName, 
                expected, 
                found,
                fmtArgs);
        }

        public void CommandTooFewArguments(string commandName, int expected, int found, IEnumerable<string> args)
        {
            var fmtArgs = "'" + string.Join("', '", args.ToArray()) + "'";
            LogError("Too few arguments for command {0} expects {1} argument(s) found {2}.\nFound: {3}",
                commandName,
                expected,
                found,
                fmtArgs);
        }

        public void CommandParamUnknown(string commandName, string switchName)
        {
            LogError("Named parameter \"{0}\" for command {1} is unknown.", switchName, commandName);
        }

        public void CommandMissingParam(string commandName, string switchUsage)
        {
            LogError("Named parameter \"{0}\" missing for command {1}.", switchUsage, commandName);
        }

        public void CommandMissing()
        {
            LogError("No command specified.");
        }

        public void CommandParamMissingArgument(string commandName, string switchUsage)
        {
            LogError("Parameter \"{0}\" for command {1} is required.", 
                 switchUsage, commandName);
        }

        public void CommandUsage(CommandDefinition commandDefiniton, Func<SwitchDefinition,string> switchUsage)
        {
            var indent = "    ";
            var switchSyntax = 
                from sw in commandDefiniton.Switches.Values
                orderby sw.Name
                where sw.Undocumented == false
                select switchUsage(sw);

            LogMessage("NAME");
            LogMessage(indent + commandDefiniton.Name);
            LogMessage("");
            if(commandDefiniton.Category != CommandCategory.None)
            {
                LogMessage("CATEGORY");
                LogMessage(indent + commandDefiniton.Category);
                LogMessage("");
            }
            LogMessage("SYNOPSIS");
            LogMessage(indent + commandDefiniton.Description);
            LogMessage("");
            LogMessage("SYNTAX");
            LogMessage(indent + commandDefiniton.Name + " " + String.Join(" ", switchSyntax.ToArray()));
            LogMessage("");
        }

        public void CommandDeprecated(CommandDefinition old, CommandDefinition newCmd)
        {
            LogWarning("The command {0} is deprecated. Please use the command {1} instead.", old.Name, newCmd.Name);
        }

        public void Format(IEnumerable<CommandDefinition> commands)
        {
            if (commands.All(cd => cd.Category == CommandCategory.None))
            {
                var fmt = new ListOutputFormatter(this, "Name", "Synopsis");
                var records = from cmddef in commands
                              orderby cmddef.Name
                              select fmt.MakeRecord(cmddef.Name, cmddef.Description);
                fmt.OutputRecords(records);
            }
            else
            {
                var fmt = new ListOutputFormatter(this, "Name", "Category", "Synopsis");
                var records = from cmddef in commands
                              orderby cmddef.Category, cmddef.Name
                              select fmt.MakeRecord(cmddef.Name, cmddef.Category, cmddef.Description);

                fmt.OutputRecords(records);
            }

        }

        public void CommandDuplicateParam(string switchUsage)
        {
            LogWarning("Duplicate named parameter: {0}", switchUsage);
        }

        public void DebugCommandLine(string[] args)
        {
            int i = 0;
            foreach (var arg in args)
            {
                int argnum = i++;
                LogDebug("arg[{0}]=\"{1}\"", argnum, arg);
                string[] charCodes = arg.ToCharArray().Select(c => String.Format("{0}", (int)c)).ToArray();
                LogDebug("arg[{0}]={{ {1} }}", argnum, String.Join(", ", charCodes));
            }
        }

    }
}
