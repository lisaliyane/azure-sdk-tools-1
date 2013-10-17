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
    using System;
    using System.Management.Automation;
    using Utilities.Common;
    using Utilities.Subscription;
    using Utilities.Properties;

    /// <summary>
    /// Cmdlet to log into an environment and download the subscriptions
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureAccount")]
    public class AddAzureAccount : SubscriptionCmdletBase
    {
        [Parameter(Mandatory = false, HelpMessage = "Environment containing the account to log into")]
        public string Environment { get; set; }

        public AddAzureAccount() : base(true)
        {
        }

        public override void ExecuteCmdlet()
        {
            WindowsAzureEnvironment env = ChosenEnvironment() ?? Profile.CurrentEnvironment;
            string accountName = Profile.AddAccounts(env);
            WriteVerbose(string.Format(Resources.AddAccountAdded, accountName));
            WriteVerbose(string.Format(Resources.AddAccountShowDefaultSubscription, Profile.DefaultSubscription.SubscriptionName));
            WriteVerbose(Resources.AddAccountViewSubscriptions);
            WriteVerbose(Resources.AddAccountChangeSubscription);
        }

        private WindowsAzureEnvironment ChosenEnvironment()
        {
            if (string.IsNullOrEmpty(Environment))
            {
                return null;
            }

            WindowsAzureEnvironment result;

            if (!Profile.Environments.TryGetValue(Environment, out result))
            {
                throw new Exception(string.Format(Resources.EnvironmentNotFound, Environment));
            }
            return result;
        }
    }
}