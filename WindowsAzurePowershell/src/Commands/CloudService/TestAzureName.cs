﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.CloudService
{
    using System;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
    using ServiceManagement;
    using Microsoft.WindowsAzure.Commands.Utilities.CloudService;

    [Cmdlet(VerbsDiagnostic.Test, "AzureName"), OutputType(typeof(bool))]
    public class TestAzureNameCommand : CmdletWithSubscriptionBase
    {
        internal ServiceBusClientExtensions ServiceBusClient { get; set; }
        internal ICloudServiceClient CloudServiceClient { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Service", HelpMessage = "Test for a cloud service name.")]
        public SwitchParameter Service
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Storage", HelpMessage = "Test for a storage account name.")]
        public SwitchParameter Storage
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "ServiceBusNamespace", HelpMessage = "Test for a service bus namespace name.")]
        public SwitchParameter ServiceBusNamespace
        {
            get;
            set;
        }

        [Parameter(Position = 1, ParameterSetName = "Service", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Cloud service name.")]
        [Parameter(Position = 1, ParameterSetName = "Storage", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Storage account name.")]
        [Parameter(Position = 1, ParameterSetName = "ServiceBusNamespace", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service bus namespace name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        public bool IsDNSAvailable(WindowsAzureSubscription subscription, string name)
        {
            EnsureCloudServiceClientInitialized(subscription);
            bool available = this.CloudServiceClient.CheckHostedServiceNameAvailability(name);
            WriteObject(!available);
            return available;
        }

        public bool IsStorageServiceAvailable(WindowsAzureSubscription subscription, string name)
        {
            EnsureCloudServiceClientInitialized(subscription);
            bool available = this.CloudServiceClient.CheckStorageServiceAvailability(name);
            WriteObject(!available);
            return available;
        }

        public bool IsServiceBusNamespaceAvailable(string subscriptionId, string name)
        {
            bool result = ServiceBusClient.IsAvailableNamespace(name);

            WriteObject(!result);

            return result;
        }

        private void EnsureCloudServiceClientInitialized(WindowsAzureSubscription subscription)
        {
            this.CloudServiceClient = this.CloudServiceClient ?? new CloudServiceClient(
                subscription,
                SessionState.Path.CurrentLocation.Path,
                WriteDebug,
                WriteVerbose,
                WriteWarning);
        }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (Service.IsPresent)
            {
                IsDNSAvailable(CurrentSubscription, Name);
            }
            else if (Storage.IsPresent)
            {
                IsStorageServiceAvailable(CurrentSubscription, Name);
            }
            else
            {
                ServiceBusClient = ServiceBusClient ?? new ServiceBusClientExtensions(CurrentSubscription);
                IsServiceBusNamespaceAvailable(CurrentSubscription.SubscriptionId, Name);
            }
        }
    }
}