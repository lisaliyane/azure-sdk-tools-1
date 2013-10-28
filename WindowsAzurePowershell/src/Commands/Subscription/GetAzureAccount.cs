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

namespace Microsoft.WindowsAzure.Commands.Subscription
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Utilities.Subscription;

    /// <summary>
    /// Cmdlet to list the currently downloaded accounts and their
    /// associated subscriptions.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureAccount")]
    public class GetAzureAccount : SubscriptionCmdletBase
    {
        [Parameter(Position = 0, Mandatory = false, HelpMessage = "Name of account to get information for")]
        public string Name { get; set; }

        public GetAzureAccount() : base(false)
        {
        }

        public override void ExecuteCmdlet()
        {
            IEnumerable<WindowsAzureSubscription> subscriptions = Profile.Subscriptions.Where(s => s.ActiveDirectoryUserId != null);
            if (!string.IsNullOrEmpty(Name))
            {
                subscriptions = subscriptions.Where(s => s.ActiveDirectoryUserId == Name);
            }

            var sortedSubscriptions = from s in subscriptions
                                      orderby s.ActiveDirectoryUserId ascending
                                      group s by s.ActiveDirectoryUserId into g
                                      select new
                                      {
                                          Name = g.Key,
                                          ActiveDirectories = g.Select(s => new { s.ActiveDirectoryTenantId, s.ActiveDirectoryEndpoint }).Distinct()
                                      };
            
            WriteObject(sortedSubscriptions, true);
        }
    }
}