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
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Properties;
    using Utilities.Subscription;

    /// <summary>
    /// Removes subscriptions associated with an account
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureAccount", SupportsShouldProcess = true), OutputType(typeof(bool))]
    public class RemoveAzureAccountCommand : SubscriptionCmdletBase
    {
        public RemoveAzureAccountCommand() : base(false)
        {
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the account")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 2, HelpMessage = "Do not confirm deletion of account")]
        public SwitchParameter Force { get; set; }

        [Parameter(Position = 3, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        public void RemoveAccountProcess()
        {
            var subscriptions = Profile.Subscriptions.Where(s => s.ActiveDirectoryUserId == Name).ToList();
            foreach (var subscription in subscriptions)
            {
                if (subscription.Certificate != null)
                {
                    subscription.SetAccessToken(null);
                    Profile.UpdateSubscription(subscription);
                }
                else
                {
                    // Warn the user if the removed subscription is the default one.
                    if (subscription.IsDefault)
                    {
                        WriteWarning(Resources.RemoveDefaultSubscription);
                    }

                    // Warn the user if the removed subscription is the current one.
                    if (subscription == Profile.CurrentSubscription)
                    {
                        WriteWarning(Resources.RemoveCurrentSubscription);
                    }

                    Profile.RemoveSubscription(subscription);
                }
            }
            if (PassThru.IsPresent)
            {
                WriteObject(true);
            }
        }

        public override void ExecuteCmdlet()
        {
            ConfirmAction(
                Force.IsPresent,
                string.Format(Resources.RemoveAccountConfirmation, Name),
                Resources.RemoveAccountMessage,
                Name,
                RemoveAccountProcess);
        }
    }
}
