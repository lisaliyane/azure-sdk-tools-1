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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Endpoints
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using IaaS;
    using Model;
    using Model.PersistentVMModel;

    [Cmdlet(VerbsCommon.Get, "AzureEndpoint"), OutputType(typeof(InputEndpointContext), typeof(Collection<InputEndpointContext>))]
    public class GetAzureEndpoint : VirtualMachineConfigurationCmdletBase
    {
        [Parameter(Position = 0, Mandatory = false, HelpMessage = "Endpoint name")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            var endpoints = GetInputEndpoints();

            if (string.IsNullOrEmpty(Name))
            {
                WriteObject(endpoints, true);
            }
            else
            {
                var endpoint = endpoints.SingleOrDefault(ep => System.String.Compare(ep.Name, Name, System.StringComparison.OrdinalIgnoreCase) == 0);
                WriteObject(endpoint, true);
            }
        }

        protected override void ProcessRecord()
        {
            ExecuteCommand();
        }

        protected Collection<InputEndpointContext> GetInputEndpoints()
        {
            var role = VM.GetInstance();

            var networkConfiguration = role.ConfigurationSets
                                        .OfType<NetworkConfigurationSet>()
                                        .SingleOrDefault();

            if (networkConfiguration == null)
            {
                networkConfiguration = new NetworkConfigurationSet();
                role.ConfigurationSets.Add(networkConfiguration);
            }

            if (networkConfiguration.InputEndpoints == null)
            {
                networkConfiguration.InputEndpoints = new Collection<InputEndpoint>();
            }

            var inputEndpoints = networkConfiguration.InputEndpoints;

            Collection<InputEndpointContext> endpoints = new Collection<InputEndpointContext>();
            foreach (InputEndpoint ep in inputEndpoints)
            {
                InputEndpointContext endpointCtx = new InputEndpointContext
                {
                    LBSetName = ep.LoadBalancedEndpointSetName,
                    LocalPort = ep.LocalPort,
                    Name = ep.Name,
                    Port = ep.Port,
                    Protocol = ep.Protocol,
                    Vip = ep.Vip,
                    Acl = ep.EndpointAccessControlList,
                    EnableDirectServerReturn = ep.EnableDirectServerReturn
                };

                if (ep.LoadBalancerProbe != null && string.IsNullOrEmpty(endpointCtx.LBSetName) == false)
                {
                    endpointCtx.ProbePath = ep.LoadBalancerProbe.Path;
                    endpointCtx.ProbePort = ep.LoadBalancerProbe.Port;
                    endpointCtx.ProbeProtocol = ep.LoadBalancerProbe.Protocol;
                    endpointCtx.ProbeIntervalInSeconds = ep.LoadBalancerProbe.IntervalInSeconds;
                    endpointCtx.ProbeTimeoutInSeconds = ep.LoadBalancerProbe.TimeoutInSeconds;
                }

                endpoints.Add(endpointCtx);
            }

            return endpoints;
        }
    }
}