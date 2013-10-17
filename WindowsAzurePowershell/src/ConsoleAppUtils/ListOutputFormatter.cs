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

    public class ListOutputFormatter
    {
        public ListOutputFormatter(StandardOutputEvents output,
            params string[] properties)
        {
            Output = output;
            Properties = new List<string>(properties);
            MaxLength = 0;
            foreach( var p in properties)
            {
                MaxLength = Math.Max(MaxLength, p.Length);
            }
        }
        private IList<string> Properties { get; set; }
        private int MaxLength { get; set; }
        private StandardOutputEvents Output { get; set; }
        public string PadToMaxLength(string s)
        {
            int pad = MaxLength - s.Length;
            var padString = new String(' ', pad);
            return s + padString;
        }
        public IDictionary<string, string> MakeRecord(params object[] args)
        {
            if (args.Length != Properties.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            int i = 0;
            foreach (var p in Properties)
            {
                var obj = args[i++];
                if (obj == null)
                {
                    dict.Add(p, null);
                }
                else
                {
                    dict.Add(p, obj.ToString());
                }
            }
            return dict;
        }
        public bool OutputRecord(params object[] args)
        {
            var record = MakeRecord(args);
            return OutputRecord(record);
        }
        public bool OutputRecord(IDictionary<string, string> dict)
        {
            return OutputRecord(dict, 0);
        }

        public bool OutputRecord(IDictionary<string, string> dict, int indentation)
        {
            bool hasOutput = false;
            var indentString = new string('\t', indentation);
            foreach (var key in Properties)
            {
                string value;
                if (dict.TryGetValue(key, out value))
                {
                    if (value != null)
                    {
                        Output.LogMessage(indentString + "{0} : {1}", PadToMaxLength(key), value);
                        hasOutput = true;
                    }
                }
            }
            return hasOutput;
        }
        public int OutputRecords(IEnumerable<IDictionary<string,string>> records)
        {
            return OutputRecords(records, 0);
        }

        public int OutputRecords(IEnumerable<IDictionary<string, string>> records, int indentation)
        {
            int numRecords = 0;
            foreach (var r in records)
            {
                if (OutputRecord(r, indentation))
                {
                    Output.LogMessage("");
                    numRecords++;
                }
            }
            return numRecords;
        }
    }
}
