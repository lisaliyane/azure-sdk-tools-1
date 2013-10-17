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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Management;
    using Utilities.Common;
    using Utilities.Properties;
    using Utilities.Subscription;

    /// <summary>
    /// Implementation of the get-azuresubscription cmdlet that works against
    /// the WindowsAzureProfile layer.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSubscription", DefaultParameterSetName = "ByName")]
    [OutputType(typeof(WindowsAzureSubscription))]
    public class GetAzureSubscriptionCommand : SubscriptionCmdletBase
    {
        public GetAzureSubscriptionCommand() : base(false)
        {
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription", ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieves the default subscription", ParameterSetName = "Default")]
        public SwitchParameter Default { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieves the current subscription", ParameterSetName = "Current")]
        public SwitchParameter Current { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieves extended details about subscription such as quota and usage")]
        public SwitchParameter ExtendedDetails { get; set; }

        public override void ExecuteCmdlet()
        {
            switch (ParameterSetName)
            {
                case "ByName":
                    GetByName();
                    break;
                case "Default":
                    GetDefault();
                    break;
                case "Current":
                    GetCurrent();
                    break;
            }
        }

        public void GetByName()
        {
            IEnumerable<WindowsAzureSubscription> subscriptions = Profile.Subscriptions;
            if (!string.IsNullOrEmpty(SubscriptionName))
            {
                subscriptions = subscriptions.Where(s => s.SubscriptionName == SubscriptionName);
            }
            WriteSubscriptions(subscriptions);
        }

        public void GetDefault()
        {
            if (Profile.DefaultSubscription == null)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException(Resources.InvalidDefaultSubscription), 
                    string.Empty,
                    ErrorCategory.InvalidData, null));
            }
            else
            {
                WriteSubscriptions(Profile.DefaultSubscription);
            }
        }

        public void GetCurrent()
        {
            //
            // Explicitly ignore the SubscriptionDataFile property here,
            // since current is strictly in-memory and we want the real
            // current subscription.
            //
            var currentProfile = BaseProfile;
            if (currentProfile.CurrentSubscription == null)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException(Resources.InvalidSelectedSubscription),
                    string.Empty,
                    ErrorCategory.InvalidData, null));
            }
            else
            {
                WriteSubscriptions(currentProfile.CurrentSubscription);
            }
        }

        private void WriteSubscriptions(params WindowsAzureSubscription[] subscriptions)
        {
            WriteSubscriptions((IEnumerable<WindowsAzureSubscription>) subscriptions);
        }

        private void WriteSubscriptions(IEnumerable<WindowsAzureSubscription> subscriptions)
        {
            IEnumerable<WindowsAzureSubscription> subscriptionOutput;

            if (ExtendedDetails.IsPresent)
            {
                subscriptionOutput = subscriptions.Select(s => s.ToExtendedData());
            }
            else
            {
                subscriptionOutput = subscriptions;
            }

            foreach (WindowsAzureSubscription subscription in subscriptionOutput)
            {
                WriteObject(subscription);
            }
        }
    }

    static class SubscriptionConversions
    {
        internal static SubscriptionDataExtended ToExtendedData(this WindowsAzureSubscription subscription)
        {
            using (var client = subscription.CreateClient<ManagementClient>())
            {
                var response = client.Subscriptions.Get();

                SubscriptionDataExtended result = new SubscriptionDataExtended
                {
                    AccountAdminLiveEmailId = response.AccountAdminLiveEmailId,
                    CurrentCoreCount = response.CurrentCoreCount,
                    CurrentHostedServices = response.CurrentHostedServices,
                    CurrentDnsServers = 0, // TODO: Add to spec
                    CurrentLocalNetworkSites = 0, // TODO: Add to spec
                    MaxCoreCount = response.MaximumCoreCount,
                    MaxDnsServers = response.MaximumDnsServers,
                    MaxHostedServices = response.MaximumHostedServices,
                    MaxVirtualNetworkSites = response.MaximumVirtualNetworkSites,
                    MaxStorageAccounts = response.MaximumStorageAccounts,
                    ServiceAdminLiveEmailId = response.ServiceAdminLiveEmailId,
                    SubscriptionRealName = response.SubscriptionName,
                    SubscriptionStatus = response.SubscriptionStatus.ToString(),
                    SubscriptionName = subscription.SubscriptionName,
                    SubscriptionId = subscription.SubscriptionId,
                    ServiceEndpoint = subscription.ServiceEndpoint,
                    IsDefault = subscription.IsDefault,
                    Certificate = subscription.Certificate,
                    CurrentStorageAccountName = subscription.CurrentStorageAccountName
                };
                
                return result;
            }
        }
    }
}
