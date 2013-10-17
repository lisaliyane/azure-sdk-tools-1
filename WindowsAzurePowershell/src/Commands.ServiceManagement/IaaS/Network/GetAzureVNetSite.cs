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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using System.ServiceModel;
    using Management.VirtualNetworks;
    using Model;
    using Properties;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Get, "AzureVNetSite"), OutputType(typeof(IEnumerable<VirtualNetworkSiteContext>))]
    public class GetAzureVNetSiteCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = false, HelpMessage = "The virtual network name.")]
        [ValidateNotNullOrEmpty]
        public string VNetName
        {
            get;
            set;
        }

        public IEnumerable<VirtualNetworkSiteContext> GetVirtualNetworkSiteProcess()
        {
            IEnumerable<VirtualNetworkSiteContext> result = null;

            InvokeInOperationContext(() =>
            {
                try
                {
                    WriteVerboseWithTimestamp(string.Format(Resources.AzureVNetSiteBeginOperation, CommandRuntime.ToString()));

                    var response = this.NetworkClient.Networks.List();
                    var sites = response.VirtualNetworkSites;

                    if (!string.IsNullOrEmpty(this.VNetName))
                    {
                        sites = sites.Where(s => string.Equals(s.Name, this.VNetName, StringComparison.InvariantCultureIgnoreCase)).ToList();

                        if (sites.Count() == 0)
                        {
                            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.VirtualNetworkNameNotFound, this.VNetName), "VirtualNetworkName");
                        }
                    }

                    var operation = GetOperationNewSM(response.RequestId);

                    WriteVerboseWithTimestamp(string.Format(Resources.AzureVNetSiteCompletedOperation, CommandRuntime.ToString()));

                    result = sites.Select(s => new VirtualNetworkSiteContext
                    {
                        OperationId          = operation.Id,
                        OperationDescription = CommandRuntime.ToString(),
                        OperationStatus      = operation.Status.ToString(),
                        AddressSpacePrefixes = s.AddressSpace != null ? s.AddressSpace.AddressPrefixes : null,
                        AffinityGroup        = s.AffinityGroup,
                        DnsServers           = s.DnsServers == null ? null : from ds in s.DnsServers
                                                                   select new Model.PersistentVMModel.DnsServer
                                                                   {
                                                                       Address = ds.Address.ToString(),
                                                                       Name = ds.Name
                                                                   },
                        GatewayProfile       = s.Gateway != null ? s.Gateway.Profile.ToString() : null,
                        GatewaySites         = s.Gateway == null ? null : s.Gateway.Sites == null ? null :
                                               s.Gateway.Sites.Select(gs => new Model.PersistentVMModel.LocalNetworkSite
                                               {
                                                   AddressSpace = new Model.PersistentVMModel.AddressSpace
                                                   {
                                                       AddressPrefixes = gs.AddressSpace == null ? null : gs.AddressSpace.AddressPrefixes == null ? null :
                                                       gs.AddressSpace.AddressPrefixes.ToList() as Model.PersistentVMModel.AddressPrefixList
                                                   },
                                                   Connections = gs.Connections == null ? null : gs.Connections.Select(gc => new Model.PersistentVMModel.Connection
                                                   {
                                                       Type = gc.Type.ToString()
                                                   }) as Model.PersistentVMModel.ConnectionList,
                                                   Name = gs.Name,
                                                   VpnGatewayAddress = gs.VpnGatewayAddress.ToString()
                                               }) as Model.PersistentVMModel.LocalNetworkSiteList,
                        Id                   = s.Id,
                        Label                = s.Label,
                        Name                 = s.Name,
                        State                = s.State,
                        Subnets              = s.Subnets.Select(sn => new Model.PersistentVMModel.Subnet {AddressPrefix = sn.AddressPrefix, Name = sn.Name})
                    }).ToList();
                }
                catch (CloudException ex)
                {
                    if (ex.Response.StatusCode == HttpStatusCode.NotFound && !IsVerbose())
                    {
                        result = null;
                    }
                    else
                    {
                        this.WriteExceptionDetails(ex);
                    }
                }
            });

            return result;
        }

        protected override void OnProcessRecord()
        {
            var virtualNetworkSites = this.GetVirtualNetworkSiteProcess();

            if (virtualNetworkSites != null)
            {
                WriteObject(virtualNetworkSites, true);
            }
        }
    }
}
