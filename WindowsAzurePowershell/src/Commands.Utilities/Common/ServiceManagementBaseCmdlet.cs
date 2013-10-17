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


namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using AutoMapper;
    using Commands;
    using Management.Compute;
    using Management.Models;
    using Management;
    using Management.Storage;
    using ServiceManagement;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using WindowsAzure.Common;
    using Microsoft.WindowsAzure;
    using Management.VirtualNetworks;
    using Management.VirtualNetworks.Models;
    using System.Threading;

    public abstract class ServiceManagementBaseCmdlet : CloudBaseCmdlet<IServiceManagement>
    {
        private Lazy<Runspace> runspace;

        protected ServiceManagementBaseCmdlet()
        {
            runspace = new Lazy<Runspace>(() => {
                var localRunspace = RunspaceFactory.CreateRunspace(this.Host);
                localRunspace.Open();
                return localRunspace;
            });
            client = new Lazy<ManagementClient>(CreateClient);
            computeClient = new Lazy<ComputeManagementClient>(CreateComputeClient);
            storageClient = new Lazy<StorageManagementClient>(CreateStorageClient);
            networkClient = new Lazy<VirtualNetworkManagementClient>(CreateNetworkClient);
        }

        public ManagementClient CreateClient()
        {
            return this.CurrentSubscription.CreateClient<ManagementClient>();
        }

        public ComputeManagementClient CreateComputeClient()
        {
            return this.CurrentSubscription.CreateClient<ComputeManagementClient>();
        }

        public StorageManagementClient CreateStorageClient()
        {
            return this.CurrentSubscription.CreateClient<StorageManagementClient>();
        }

        public VirtualNetworkManagementClient CreateNetworkClient()
        {
            return this.CurrentSubscription.CreateClient<VirtualNetworkManagementClient>();
        }

        private void LogDebug(string message)
        {
//            lock (runspaceLock)
//            {
//                using (var ps = PowerShell.Create())
//                {
//                    ps.Runspace = runspace.Value;
//                    ps.AddCommand("Write-Debug");
//                    ps.AddParameter("Message", message);
//                    ps.AddParameter("Debug");
//                    ps.Invoke();
//                }
//            }
        }

        private Lazy<ManagementClient> client;
        public ManagementClient ManagementClient 
        { 
            get { return client.Value; }
        }

        private Lazy<ComputeManagementClient> computeClient;
        public ComputeManagementClient ComputeClient 
        {
            get { return computeClient.Value; }
        }

        private Lazy<StorageManagementClient> storageClient;
        public StorageManagementClient StorageClient 
        {
            get { return storageClient.Value; }
        }
        
        private Lazy<VirtualNetworkManagementClient> networkClient;
        public VirtualNetworkManagementClient NetworkClient 
        {
            get { return networkClient.Value; }
        }

        protected override void InitChannelCurrentSubscription(bool force)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposing the client would also dispose the channel we are returning.")]
        protected override IServiceManagement CreateChannel()
        {
            // If ShareChannel is set by a unit test, use the same channel that
            // was passed into out constructor.  This allows the test to submit
            // a mock that we use for all network calls.
            if (ShareChannel)
            {
                return Channel;
            }

            var messageInspectors = new List<IClientMessageInspector>
            {
                new ServiceManagementClientOutputMessageInspector(),
                new HttpRestMessageInspector(this.WriteDebug)
            };

            var clientOptions = new ServiceManagementClientOptions(null, null, null, 0, RetryPolicy.NoRetryPolicy, ServiceManagementClientOptions.DefaultOptions.WaitTimeForOperationToComplete, messageInspectors);
            var smClient = new ServiceManagementClient(new Uri(this.ServiceEndpoint), CurrentSubscription.SubscriptionId, CurrentSubscription.Certificate, clientOptions);

            Type serviceManagementClientType = typeof(ServiceManagementClient);
            PropertyInfo propertyInfo = serviceManagementClientType.GetProperty("SyncService", BindingFlags.Instance | BindingFlags.NonPublic);
            var syncService = (IServiceManagement)propertyInfo.GetValue(smClient, null);

            return syncService;
        }

        /// <summary>
        /// Invoke the given operation within an OperationContextScope if the
        /// channel supports it.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        protected override void InvokeInOperationContext(Action action)
        {
            IContextChannel contextChannel = ToContextChannel();
            if (contextChannel != null)
            {
                using (new OperationContextScope(contextChannel))
                {
                    action();
                }
            }
            else
            {
                action();
            }
        }

        protected virtual IContextChannel ToContextChannel()
        {
            try
            {
                return Channel.ToContextChannel();
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void ExecuteClientAction(object input, string operationDescription, Action<string> action)
        {
            Operation operation = null;

            WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionBeginOperation, operationDescription));

            RetryCall(action);
            operation = GetOperation();

            WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionCompletedOperation, operationDescription));

            if (operation != null)
            {
                var context = new ManagementOperationContext
                {
                    OperationDescription = operationDescription,
                    OperationId = operation.OperationTrackingId,
                    OperationStatus = operation.Status
                };

                WriteObject(context, true);
            }
        }

        protected void ExecuteClientActionInOCS(object input, string operationDescription, Action<string> action)
        {
            IContextChannel contextChannel = null;

            try
            {
                contextChannel = Channel.ToContextChannel();
            }
            catch (Exception)
            {
                // Do nothing, proceed.
            }

            if (contextChannel != null)
            {
                object context = null;

                using (new OperationContextScope(contextChannel))
                {
                    Operation operation = null;

                    WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSBeginOperation, operationDescription));

                    try
                    {
                        RetryCall(action);
                        operation = GetOperation();
                    }
                    catch (ServiceManagementClientException ex)
                    {
                        WriteExceptionDetails(ex);
                    }

                    WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSCompletedOperation, operationDescription));

                    if (operation != null)
                    {
                        context = new ManagementOperationContext
                        {
                            OperationDescription = operationDescription,
                            OperationId = operation.OperationTrackingId,
                            OperationStatus = operation.Status
                        };
                    }
                }

                if (context != null)
                {
                    WriteObject(context, true);
                }
            }
            else
            {
                RetryCall(action);
            }
        }

        protected virtual void WriteExceptionDetails(Exception exception)
        {
            if (CommandRuntime != null)
            {
                WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        protected OperationStatusResponse GetOperationStatusNewSM(string operationId)
        {
            OperationStatusResponse response = this.ManagementClient.GetOperationStatus(operationId);
            return response;
        }

        protected OperationStatusResponse GetOperationNewSM(string operationId)
        {
            OperationStatusResponse operation = null;

            try
            {
                operation = GetOperationStatusNewSM(operationId);

                if (operation.Status == Management.Models.OperationStatus.Failed)
                {
                    var errorMessage = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", operation.Status, operation.Error.Message);
                    var exception = new Exception(errorMessage);
                    WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
                }
            }
            catch (AggregateException ex)
            {
                WriteExceptionDetails(ex);
            }

            return operation;
        }

        //TODO: Input argument is not used and should probably be removed.
        protected void ExecuteClientActionNewSM<TResult>(object input, string operationDescription, Func<TResult> action, Func<OperationStatusResponse, TResult, object> contextFactory) where TResult : OperationResponse
        {
            ExecuteClientActionNewSM(input, operationDescription, action, null, contextFactory);
        }

        protected void ExecuteClientActionNewSM<TResult>(object input, string operationDescription, Func<TResult> action, Func<string, string, OperationStatusResponse> waitOperation, Func<OperationStatusResponse, TResult, object> contextFactory) where TResult : OperationResponse
        {
            TResult result = null;
            OperationStatusResponse operation = null;
            WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSBeginOperation, operationDescription));
            try
            {
                try
                {
                    result = action();
                }
                catch (CloudException ex)
                {
                    WriteExceptionDetails(ex);
                }

                if (waitOperation == null)
                {
                    operation = result == null ? null : GetOperationNewSM(result.RequestId);
                }
                else
                {
                    operation = result == null ? null : waitOperation(result.RequestId, operationDescription);
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is CloudException)
                {
                    WriteExceptionDetails(ex.InnerException);
                }
                else
                {
                    WriteExceptionDetails(ex);
                }
            }

            WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSCompletedOperation, operationDescription));

            if (result != null)
            {
                var context = contextFactory(operation, result);
                if (context != null)
                {
                    WriteObject(context, true);
                }
            }
        }

        protected void ExecuteClientActionNewSM<TResult>(object input, string operationDescription, Func<TResult> action) where TResult : OperationResponse
        {
            this.ExecuteClientActionNewSM(input, operationDescription, action, (s, response) => this.ContextFactory<OperationResponse, ManagementOperationContext>(response, s));
        }

        protected void ExecuteClientActionNewSM<TResult>(object input, string operationDescription, Func<TResult> action, Func<string, string, OperationStatusResponse> waitOperation) where TResult : OperationResponse
        {
            this.ExecuteClientActionNewSM(input, operationDescription, action, waitOperation, (s, response) => this.ContextFactory<OperationResponse, ManagementOperationContext>(response, s));
        }

        protected OperationStatusResponse WaitForNewGatewayOperation(string operationId, string opdesc)
        {
            try
            {
                var opStatus = this.NetworkClient.Gateways.GetOperationStatus(operationId);

                var activityId = new Random().Next(1, 999999);
                var progress = new ProgressRecord(activityId, opdesc, Resources.GatewayOperationStatus + opStatus);
                while (opStatus.Status != GatewayOperationStatus.Successful && opStatus.Status != GatewayOperationStatus.Failed)
                {
                    WriteProgress(progress);
                    Thread.Sleep(1 * 1000);
                    opStatus = this.NetworkClient.Gateways.GetOperationStatus(operationId);
                }

                if (opStatus.Status == GatewayOperationStatus.Failed)
                {
                    var errorMessage = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", opStatus.Status, opStatus.Error.Message);
                    var exception = new Exception(errorMessage);
                    WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
                }
            }
            catch (CommunicationException ex)
            {
                WriteErrorDetails(ex);
            }

            return GetOperationNewSM(operationId);
        }

        protected void ExecuteClientActionInOCS<TResult>(object input, string operationDescription, Func<string, TResult> action, Func<Operation, TResult, object> contextFactory) where TResult : class
        {
            IContextChannel contextChannel = null;

            try
            {
                contextChannel = Channel.ToContextChannel();
            }
            catch (Exception)
            {
                // Do nothing, proceed.
            }

            if (contextChannel != null)
            {
                object context = null;

                using (new OperationContextScope(contextChannel))
                {
                    TResult result = null;
                    Operation operation = null;

                    WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSBeginOperation, operationDescription));

                    try
                    {
                        result = RetryCall(action);
                        operation = GetOperation();
                    }
                    catch (ServiceManagementClientException ex)
                    {
                        WriteExceptionDetails(ex);
                    }

                    WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSCompletedOperation, operationDescription));

                    if (result != null && operation != null)
                    {
                        context = contextFactory(operation, result);
                    }
                }

                if (context != null)
                {
                    WriteObject(context, true);
                }
            }
            else
            {
                TResult result = RetryCall(action);
                if (result != null)
                {
                    WriteObject(result, true);
                }
            }
        }

        protected Operation GetOperation()
        {
            Operation operation = null;

            try
            {
                string operationId = RetrieveOperationId();

                if (!string.IsNullOrEmpty(operationId))
                {
                    operation = RetryCall(s => GetOperationStatus(this.CurrentSubscription.SubscriptionId, operationId));

                    if (string.Compare(operation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var errorMessage = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", operation.Status, operation.Error.Message);
                        var exception = new Exception(errorMessage);
                        WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
                    }
                }
                else
                {
                    operation = new Operation
                    {
                        OperationTrackingId = string.Empty,
                        Status = OperationState.Failed
                    };
                }
            }
            catch (ServiceManagementClientException ex)
            {
                WriteExceptionDetails(ex);
            }

            return operation;
        }

        protected override Operation GetOperationStatus(string subscriptionId, string operationId)
        {
            var channel = (IServiceManagement)Channel;
            return channel.GetOperationStatus(subscriptionId, operationId);
        }

        protected T2 ContextFactory<T1, T2>(T1 source) where T2 : ManagementOperationContext
        {
            var context = Mapper.Map<T1, T2>(source);
            context.OperationDescription = CommandRuntime.ToString();
            return context;
        }

        protected T2 ContextFactory<T1, T2>(T1 source, OperationStatusResponse response) where T2 : ManagementOperationContext
        {
            var context = Mapper.Map<T1, T2>(source);
            context = Mapper.Map(response, context);
            context.OperationDescription = CommandRuntime.ToString();
            return context;
        }
        protected T2 ContextFactory<T1, T2>(T1 source, T2 destination) where T2 : ManagementOperationContext
        {
            var context = Mapper.Map(source, destination);
            return context;
        }
    }
}