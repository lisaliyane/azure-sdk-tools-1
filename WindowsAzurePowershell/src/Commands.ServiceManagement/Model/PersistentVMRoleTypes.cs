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

//TODO: When transition to SM.NET is completed, rename the namespace to "Microsoft.WindowsAzure.ServiceManagement"

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;

    #region Constants
    public static class Constants
    {
        public const string ContinuationTokenHeaderName = "x-ms-continuation-token";
        public const string SubscriptionIdsHeaderName = "x-ms-subscription-ids";
        public const string ClientRequestIdHeader = "x-ms-client-id";
        public const string OperationTrackingIdHeader = "x-ms-request-id";
        public const string PrincipalHeader = "x-ms-principal-id";
        public const string ServiceManagementNS = "http://schemas.microsoft.com/windowsazure";
        public const string VersionHeaderName = "x-ms-version";
        public readonly static string StandardTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
        // Please put the newest version outside the #endif.MSFTINTERNAL We only want the newest version to show up in what we ship publically.
        // Also, update rdfe\Utilities\Common\VersionHeaders.cs StaticSupportedVersionsList.
        public const string VersionHeaderContent20130801 = "2013-08-01";
        public const string VersionHeaderContentLatest = VersionHeaderContent20130801;
    }


    public static class PrincipalConstants
    {
        public const string AccountAdministrator = "AccountAdministrator";
        public const string ServiceAdministrator = "ServiceAdministrator";
        public const string CoAdministrator = "CoAdministrator";
    }

    public static class DeploymentStatus
    {
        public const string Running = "Running";
        public const string Suspended = "Suspended";
        public const string RunningTransitioning = "RunningTransitioning";
        public const string SuspendedTransitioning = "SuspendedTransitioning";
        public const string Starting = "Starting";
        public const string Suspending = "Suspending";
        public const string Deploying = "Deploying";
        public const string Deleting = "Deleting";
        public const string Unavailable = "Unavailable";
    }

    public static class RoleInstanceStatus
    {
        public const string Initializing = "Initializing";
        public const string Ready = "Ready";
        public const string Busy = "Busy";
        public const string Stopping = "Stopping";
        public const string Stopped = "Stopped";
        public const string Unresponsive = "Unresponsive";

        public const string RoleStateUnknown = "RoleStateUnknown";
        public const string CreatingVM = "CreatingVM";
        public const string StartingVM = "StartingVM";
        public const string CreatingRole = "CreatingRole";
        public const string StartingRole = "StartingRole";
        public const string ReadyRole = "ReadyRole";
        public const string BusyRole = "BusyRole";

        public const string StoppingRole = "StoppingRole";
        public const string StoppingVM = "StoppingVM";
        public const string DeletingVM = "DeletingVM";
        public const string StoppedVM = "StoppedVM";
        public const string RestartingRole = "RestartingRole";
        public const string CyclingRole = "CyclingRole";

        public const string FailedStartingRole = "FailedStartingRole";
        public const string FailedStartingVM = "FailedStartingVM";
        public const string UnresponsiveRole = "UnresponsiveRole";

        public const string Provisioning = "Provisioning";
        public const string ProvisioningFailed = "ProvisioningFailed";
        public const string ProvisioningTimeout = "ProvisioningTimeout";

        public const string StoppingAndDeallocating = "StoppingAndDeallocating";
        public const string StoppedDeallocated = "StoppedDeallocated";
    }

    public static class OperationState
    {
        public const string InProgress = "InProgress";
        public const string Succeeded = "Succeeded";
        public const string Failed = "Failed";
    }

    public static class KeyType
    {
        public const string Primary = "Primary";
        public const string Secondary = "Secondary";
    }

    public static class DeploymentSlotType
    {
        public const string Staging = "Staging";
        public const string Production = "Production";
    }

    public static class UpgradeType
    {
        public const string Auto = "Auto";
        public const string Manual = "Manual";
        public const string Simultaneous = "Simultaneous";
    }

    public static class CurrentUpgradeDomainState
    {
        public const string Before = "Before";
        public const string During = "During";
    }
    public static class GuestAgentType
    {
        public const string ProdGA = "ProdGA";
        public const string TestGA = "TestGA";
        public const string HotfixGA = "HotfixGA";
    }

    #endregion

    #region Mergable
    public interface IResolvable
    {
        object ResolveType();
    }

    public interface IMergable
    {
        void Merge(object other);
    }

    public interface IMergable<T> : IMergable
    {
        void Merge(T other);
    }

    [DataContract]
    public abstract class Mergable<T> : IResolvable, IMergable<T>, IExtensibleDataObject where T : Mergable<T>
    {
        #region Field backing store
        private Dictionary<string, object> propertyStore;
        private Dictionary<string, object> PropertyStore
        {
            get
            {
                if (this.propertyStore == null)
                {
                    this.propertyStore = new Dictionary<string, object>();
                }
                return this.propertyStore;
            }
        }
        #endregion

        protected TValue GetValue<TValue>(string fieldName)
        {
            object value;

            if (this.PropertyStore.TryGetValue(fieldName, out value))
            {
                return (TValue)value;
            }
            return default(TValue);
        }
        protected void SetValue<TValue>(string fieldName, TValue value)
        {
            this.PropertyStore[fieldName] = value;
        }

        protected Nullable<TValue> GetField<TValue>(string fieldName) where TValue : struct
        {
            object value;
            if (this.PropertyStore.TryGetValue(fieldName, out value))
            {
                return new Nullable<TValue>((TValue)value);
            }
            else
            {
                return new Nullable<TValue>();
            }
        }

        protected void SetField<TValue>(string fieldName, Nullable<TValue> value) where TValue : struct
        {
            if (value.HasValue)
            {
                this.PropertyStore[fieldName] = value.Value;
            }
        }

        #region IResolvable Members

        public virtual object ResolveType()
        {
            return this;
        }

        #endregion

        protected TValue Convert<TValue>()
        {
            DataContractSerializer sourceSerializer = new DataContractSerializer(this.GetType());
            DataContractSerializer destinationSerializer = new DataContractSerializer(typeof(TValue));

            using (MemoryStream stream = new MemoryStream())
            {
                sourceSerializer.WriteObject(stream, this);
                stream.Position = 0;
                return (TValue)destinationSerializer.ReadObject(stream);
            }
        }


        #region IMergable Members

        public void Merge(object other)
        {
            ((IMergable<T>)this).Merge((T)other);
        }

        #endregion

        #region IMergable<T> members
        public void Merge(T other)
        {
            Mergable<T> otherObject = (Mergable<T>)other;

            foreach (KeyValuePair<string, object> kvPair in otherObject.PropertyStore)
            {
                object currentValue;

                if (this.PropertyStore.TryGetValue(kvPair.Key, out currentValue))
                {
                    IMergable mergableValue = currentValue as IMergable;

                    if (mergableValue != null)
                    {
                        mergableValue.Merge(kvPair.Value);
                        continue;
                    }
                }
                this.PropertyStore[kvPair.Key] = kvPair.Value;
            }
        }
        #endregion

        #region IExtensibleDataObject Members
        public ExtensionDataObject ExtensionData { get; set; }
        #endregion
    }

    #endregion

    #region VirtualIP
    [CollectionDataContract(Name = "VirtualIPs", ItemName = "VirtualIP", Namespace = Constants.ServiceManagementNS)]
    public class VirtualIPList : List<VirtualIP>
    {
        public VirtualIPList()
        {

        }

        public VirtualIPList(IEnumerable<VirtualIP> ips)
            : base(ips)
        {

        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class VirtualIP : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Address { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public bool? IsDnsProgrammed { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Name { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }

        #region Implements Equals
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            VirtualIP vip = obj as VirtualIP;
            if (vip == null)
            {
                return false;
            }

            return this == vip;
        }

        public static bool operator ==(VirtualIP left, VirtualIP right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if ((object)left == null && (object)right == null)
            {
                return true;
            }

            if ((object)left == null || (object)right == null)
            {
                return false;
            }

            return string.Equals(left.Address, right.Address, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(VirtualIP left, VirtualIP right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return this.Address.GetHashCode();
        }
        #endregion
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class EndpointContract : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Protocol { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public int Port { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Namespace = Constants.ServiceManagementNS, Name = "EndpointContracts", ItemName = "EndpointContract")]
    public class EndpointContractList : List<EndpointContract>
    {
        public EndpointContractList() { }

        public EndpointContractList(IEnumerable<EndpointContract> collection)
            : base(collection)
        {

        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class VirtualIPGroup : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public EndpointContractList EndpointContracts { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public VirtualIPList VirtualIPs { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "VirtualIPGroups", ItemName = "VirtualIPGroup", Namespace = Constants.ServiceManagementNS)]
    public class VirtualIPGroups : List<VirtualIPGroup>
    {
        public VirtualIPGroups()
        {
        }

        public VirtualIPGroups(IEnumerable<VirtualIPGroup> groups)
            : base(groups)
        {
        }
    }
    #endregion

    #region Configuration Set
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    [KnownType(typeof(ProvisioningConfigurationSet))]
    [KnownType(typeof(LinuxProvisioningConfigurationSet))]
    [KnownType(typeof(WindowsProvisioningConfigurationSet))]
    [KnownType(typeof(NetworkConfigurationSet))]
    public class ConfigurationSet : Mergable<ConfigurationSet>
    {
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public virtual string ConfigurationSetType
        {
            get;
            set;
        }

        protected ConfigurationSet()
        {

        }

        public override object ResolveType()
        {
            if (this.GetType() != typeof(ConfigurationSet))
            {
                return this;
            }

            if (!string.IsNullOrEmpty(this.ConfigurationSetType))
            {
                if (string.Equals(this.ConfigurationSetType, "WindowsProvisioningConfiguration"))
                {
                    return base.Convert<WindowsProvisioningConfigurationSet>();
                }

                if (string.Equals(this.ConfigurationSetType, "LinuxProvisioningConfiguration"))
                {
                    return base.Convert<LinuxProvisioningConfigurationSet>();
                }

                if (string.Equals(this.ConfigurationSetType, "NetworkConfiguration"))
                {
                    return base.Convert<NetworkConfigurationSet>();
                }
            }
            return this;
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public abstract class ProvisioningConfigurationSet : ConfigurationSet
    {
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class WindowsProvisioningConfigurationSet : ProvisioningConfigurationSet
    {
        [DataMember(Name = "ComputerName", EmitDefaultValue = false, Order = 1)]
        public string ComputerName
        {
            get
            {
                return this.GetValue<string>("ComputerName");
            }
            set
            {
                this.SetValue("ComputerName", value);
            }
        }

        [DataMember(Name = "AdminPassword", EmitDefaultValue = false, Order = 2)]
        public string AdminPassword
        {
            get
            {
                return this.GetValue<string>("AdminPassword");
            }
            set
            {
                this.SetValue("AdminPassword", value);
            }
        }


        [DataMember(Name = "ResetPasswordOnFirstLogon", EmitDefaultValue = false, Order = 4)]
        private bool? resetPasswordOnFirstLogon
        {
            get
            {
                return this.GetField<bool>("ResetPasswordOnFirstLogon");
            }
            set
            {
                this.SetField("ResetPasswordOnFirstLogon", value);
            }
        }
        public bool ResetPasswordOnFirstLogon
        {
            get
            {
                return base.GetValue<bool>("ResetPasswordOnFirstLogon");
            }
            set
            {
                base.SetValue("ResetPasswordOnFirstLogon", value);
            }
        }

        [DataMember(Name = "EnableAutomaticUpdates", EmitDefaultValue = false, Order = 4)]
        public bool? EnableAutomaticUpdates
        {
            get
            {
                return base.GetValue<bool?>("EnableAutomaticUpdates");
            }
            set
            {
                base.SetValue("EnableAutomaticUpdates", value);
            }
        }

        [DataMember(Name = "TimeZone", EmitDefaultValue = false, Order = 5)]
        public string TimeZone
        {
            get
            {
                return base.GetValue<string>("TimeZone");
            }
            set
            {
                base.SetValue("TimeZone", value);
            }
        }

        [DataMember(Name = "DomainJoin", EmitDefaultValue = false, Order = 6)]
        public DomainJoinSettings DomainJoin
        {
            get
            {
                return base.GetValue<DomainJoinSettings>("DomainJoin");
            }
            set
            {
                base.SetValue("DomainJoin", value);
            }
        }

        [DataMember(Name = "StoredCertificateSettings", EmitDefaultValue = false, Order = 7)]
        public CertificateSettingList StoredCertificateSettings
        {
            get
            {
                return base.GetValue<CertificateSettingList>("StoredCertificateSettings");
            }
            set
            {
                base.SetValue("StoredCertificateSettings", value);
            }
        }

        [DataMember(Name = "WinRM", EmitDefaultValue = false, Order = 8)]
        public WinRmConfiguration WinRM
        {
            get
            {
                return base.GetValue<WinRmConfiguration>("WinRM");
            }
            set
            {
                base.SetValue("WinRM", value);
            }
        }

        [DataMember(Name = "AdminUsername", EmitDefaultValue = false, Order = 9)]
        public string AdminUsername
        {
            get
            {
                return this.GetValue<string>("AdminUsername");
            }
            set
            {
                this.SetValue("AdminUsername", value);
            }
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class WinRmConfiguration
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public WinRmListenerCollection Listeners { get; set; }
        }

        [CollectionDataContract(Namespace = Constants.ServiceManagementNS, ItemName = "Listener")]
        public class WinRmListenerCollection : Collection<WinRmListenerProperties> { }

        public enum WinRmProtocol
        {
            Http,
            Https
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class WinRmListenerProperties
        {

            [DataMember(Order = 0, IsRequired = false, EmitDefaultValue = false)]
            public string CertificateThumbprint { get; set; }

            [DataMember(Order = 1, IsRequired = true)]
            public string Protocol { get; set; }
        }

        public override string ConfigurationSetType
        {
            get
            {
                return "WindowsProvisioningConfiguration";
            }
            set
            {
                base.ConfigurationSetType = value;
            }
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class DomainJoinCredentials
        {
            [DataMember(Name = "Domain", EmitDefaultValue = false, Order = 1)]
            public string Domain { get; set; }

            [DataMember(Name = "Username", EmitDefaultValue = false, Order = 2)]
            public string Username { get; set; }

            [DataMember(Name = "Password", EmitDefaultValue = false, Order = 3)]
            public string Password { get; set; }
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class DomainJoinProvisioning
        {
            [DataMember(Name = "AccountData", EmitDefaultValue = false, Order = 1)]
            public string AccountData { get; set; }
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class DomainJoinSettings
        {
            [DataMember(Name = "Credentials", EmitDefaultValue = false, Order = 1)]
            public DomainJoinCredentials Credentials { get; set; }

            [DataMember(Name = "Provisioning", EmitDefaultValue = false, Order = 2)]
            public DomainJoinProvisioning Provisioning { get; set; }

            [DataMember(Name = "JoinDomain", EmitDefaultValue = false, Order = 3)]
            public string JoinDomain { get; set; }

            [DataMember(Name = "MachineObjectOU", EmitDefaultValue = false, Order = 4)]
            public string MachineObjectOU { get; set; }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class LinuxProvisioningConfigurationSet : ProvisioningConfigurationSet
    {
        [DataMember(Name = "HostName", EmitDefaultValue = false, Order = 1)]
        public string HostName
        {
            get
            {
                return this.GetValue<string>("HostName");
            }
            set
            {
                this.SetValue("HostName", value);
            }
        }

        [DataMember(Name = "UserName", EmitDefaultValue = false, Order = 2)]
        public string UserName
        {
            get
            {
                return this.GetValue<string>("UserName");
            }
            set
            {
                this.SetValue("UserName", value);
            }
        }

        [DataMember(Name = "UserPassword", EmitDefaultValue = false, Order = 3)]
        public string UserPassword
        {
            get
            {
                return this.GetValue<string>("UserPassword");
            }
            set
            {
                this.SetValue("UserPassword", value);
            }
        }

        [DataMember(Name = "DisableSshPasswordAuthentication", EmitDefaultValue = false, Order = 4)]
        public bool? DisableSshPasswordAuthentication
        {
            get
            {
                return base.GetValue<bool?>("DisableSshPasswordAuthentication");
            }
            set
            {
                base.SetValue("DisableSshPasswordAuthentication", value);
            }
        }

        [DataMember(Name = "SSH", EmitDefaultValue = false, Order = 5)]
        public SSHSettings SSH
        {
            get
            {
                return base.GetValue<SSHSettings>("SSH");
            }
            set
            {
                base.SetValue("SSH", value);
            }
        }

        public override string ConfigurationSetType
        {
            get
            {
                return "LinuxProvisioningConfiguration";
            }
            set
            {
                base.ConfigurationSetType = value;
            }
        }

        [DataContract(Name = "SSHSettings", Namespace = Constants.ServiceManagementNS)]
        public class SSHSettings
        {
            [DataMember(Name = "PublicKeys", EmitDefaultValue = false, Order = 1)]
            public SSHPublicKeyList PublicKeys { get; set; }

            [DataMember(Name = "KeyPairs", EmitDefaultValue = false, Order = 2)]
            public SSHKeyPairList KeyPairs { get; set; }
        }

        [CollectionDataContract(Name = "SSHPublicKeyList", ItemName = "PublicKey", Namespace = Constants.ServiceManagementNS)]
        public class SSHPublicKeyList : List<SSHPublicKey>
        {
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class SSHPublicKey
        {
            [DataMember(Name = "Fingerprint", EmitDefaultValue = false, Order = 1)]
            public string Fingerprint { get; set; }

            [DataMember(Name = "Path", EmitDefaultValue = false, Order = 2)]
            public string Path { get; set; }
        }


        [CollectionDataContract(Name = "SSHKeyPairList", ItemName = "KeyPair", Namespace = Constants.ServiceManagementNS)]
        public class SSHKeyPairList : List<SSHKeyPair>
        {
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class SSHKeyPair
        {
            [DataMember(Name = "Fingerprint", EmitDefaultValue = false, Order = 1)]
            public string Fingerprint { get; set; }

            [DataMember(Name = "Path", EmitDefaultValue = false, Order = 2)]
            public string Path { get; set; }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class NetworkConfigurationSet : ConfigurationSet
    {
        public override string ConfigurationSetType
        {
            get
            {
                return "NetworkConfiguration";
            }
            set
            {
                base.ConfigurationSetType = value;
            }
        }

        [DataMember(Name = "InputEndpoints", EmitDefaultValue = false, Order = 0)]
        public Collection<InputEndpoint> InputEndpoints
        {
            get
            {
                return base.GetValue<Collection<InputEndpoint>>("InputEndpoints");
            }
            set
            {
                base.SetValue("InputEndpoints", value);
            }
        }

        [DataMember(Name = "SubnetNames", EmitDefaultValue = false, Order = 1)]
        public SubnetNamesCollection SubnetNames
        {
            get
            {
                return this.GetValue<SubnetNamesCollection>("SubnetNames");
            }
            set
            {
                this.SetValue("SubnetNames", value);
            }
        }

        [DataMember(Name = "VirtualIPGroups", EmitDefaultValue = false, Order = 2)]
        public VirtualIPGroups VirtualIPGroups
        {
            get
            {
                return this.GetValue<VirtualIPGroups>("VirtualIPGroups");
            }
            set
            {
                this.SetValue("VirtualIPGroups", value);
            }
        }
    }

    [CollectionDataContract(Name = "LoadBalancedEndpointList", Namespace = Constants.ServiceManagementNS)]
    public class LoadBalancedEndpointList : List<InputEndpoint>
    {
    }

    [DataContract(Name = "InputEndpoint", Namespace = Constants.ServiceManagementNS)]
    public class InputEndpoint : Mergable<InputEndpoint>
    {
        #region constants
        [IgnoreDataMember]
        private const string EnableDirectServerReturnFieldName = "EnableDirectServerReturn";
        [IgnoreDataMember]
        private const string EndPointAccessControlListMemberName = "EndPointAccessControlList";
        #endregion

        [DataMember(Name = "LoadBalancedEndpointSetName", EmitDefaultValue = false, Order = 1)]
        public string LoadBalancedEndpointSetName
        {
            get
            {
                return base.GetValue<string>("LoadBalancedEndpointSetName");
            }
            set
            {
                base.SetValue("LoadBalancedEndpointSetName", value);
            }
        }

        [DataMember(Name = "LocalPort", EmitDefaultValue = false, Order = 2)]
        private int? localPort
        {
            get
            {
                return base.GetField<int>("LocalPort");
            }
            set
            {
                base.SetField("LocalPort", value);
            }
        }

        public int LocalPort
        {
            get
            {
                return base.GetValue<int>("LocalPort");
            }
            set
            {
                base.SetValue("LocalPort", value);
            }
        }

        [DataMember(Name = "Name", EmitDefaultValue = false, Order = 3)]
        public string Name
        {
            get
            {
                return base.GetValue<string>("Name");
            }
            set
            {
                base.SetValue("Name", value);
            }
        }

        [DataMember(Name = "Port", EmitDefaultValue = false, Order = 4)]
        public int? Port
        {
            get
            {
                return base.GetValue<int?>("Port");
            }
            set
            {
                base.SetValue("Port", value);
            }
        }

        [DataMember(Name = "LoadBalancerProbe", EmitDefaultValue = false, Order = 5)]
        public LoadBalancerProbe LoadBalancerProbe
        {
            get
            {
                return base.GetValue<LoadBalancerProbe>("LoadBalancerProbe");
            }
            set
            {
                base.SetValue("LoadBalancerProbe", value);
            }
        }

        [DataMember(Name = "Protocol", EmitDefaultValue = false, Order = 6)]
        public string Protocol
        {
            get
            {
                return base.GetValue<string>("Protocol");
            }
            set
            {
                base.SetValue("Protocol", value);
            }
        }

        [DataMember(Name = "Vip", EmitDefaultValue = false, Order = 7)]
        public string Vip
        {
            get
            {
                return base.GetValue<string>("Vip");
            }
            set
            {
                base.SetValue("Vip", value);
            }
        }

        [DataMember(Name = "EnableDirectServerReturn", EmitDefaultValue = false, Order = 8)]
        public bool? EnableDirectServerReturn
        {
            get
            {
                return base.GetValue<bool?>(EnableDirectServerReturnFieldName);
            }
            set
            {
                base.SetValue(EnableDirectServerReturnFieldName, value);
            }
        }

        [DataMember(Name = "EndpointAcl", EmitDefaultValue = false, Order = 9)]
        public EndpointAccessControlList EndpointAccessControlList
        {
            get
            {
                return base.GetValue<EndpointAccessControlList>(EndPointAccessControlListMemberName);
            }

            set
            {
                base.SetValue(EndPointAccessControlListMemberName, value);
            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class EndpointAccessControlList : Mergable<EndpointAccessControlList>
    {
        #region private constants
        [IgnoreDataMember]
        private const string AccessControlListRulesMemberName = "AccessControlListRules";
        #endregion

        [DataMember(Name = "Rules", IsRequired = false, Order = 0)]
        public Collection<AccessControlListRule> Rules
        {
            get
            {
                return base.GetValue<Collection<AccessControlListRule>>(AccessControlListRulesMemberName);
            }

            set
            {
                base.SetValue(AccessControlListRulesMemberName, value);
            }
        }
    }

    [DataContract(Name = "Rule", Namespace = Constants.ServiceManagementNS)]
    public class AccessControlListRule : Mergable<AccessControlListRule>
    {
        #region private constants
        private const string OrderMemberName = "Order";
        private const string ActionMemberName = "Action";
        private const string RemoteSubnetMemberName = "RemoteSubnet";
        private const string DescriptionMemberName = "Description";
        #endregion

        [DataMember(Name = OrderMemberName, IsRequired = false, Order = 0)]
        public int? Order
        {
            get
            {
                return base.GetValue<int?>(OrderMemberName);
            }

            set
            {
                base.SetValue(OrderMemberName, value);
            }
        }

        [DataMember(Name = ActionMemberName, IsRequired = false, Order = 1)]
        public string Action
        {
            get
            {
                return base.GetValue<string>(ActionMemberName);
            }

            set
            {
                base.SetValue(ActionMemberName, value);
            }
        }

        [DataMember(Name = RemoteSubnetMemberName, IsRequired = false, Order = 2)]
        public string RemoteSubnet
        {
            get
            {
                return base.GetValue<string>(RemoteSubnetMemberName);
            }

            set
            {
                base.SetValue(RemoteSubnetMemberName, value);
            }
        }

        [DataMember(Name = DescriptionMemberName, IsRequired = false, Order = 3)]
        public string Description
        {
            get
            {
                return base.GetValue<string>(DescriptionMemberName);
            }

            set
            {
                base.SetValue(DescriptionMemberName, value);
            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class LoadBalancerProbe : Mergable<LoadBalancerProbe>
    {
        #region constants
        // NOTE: fields in this region must be marked with IgnoreDataMember
        [IgnoreDataMember]
        private const string IntervalFieldName = "IntervalInSeconds";

        [IgnoreDataMember]
        private const string TimeoutFieldName = "TimeoutInSeconds";
        #endregion

        [DataMember(Name = "Path", EmitDefaultValue = false, Order = 0)]
        public string Path
        {
            get
            {
                return base.GetValue<string>("Path");
            }
            set
            {
                base.SetValue("Path", value);
            }
        }

        [DataMember(Name = "Port", EmitDefaultValue = false, Order = 1)]
        private int? port
        {
            get
            {
                return base.GetField<int>("Port");
            }
            set
            {
                base.SetField("Port", value);
            }
        }
        public int Port
        {
            get
            {
                return base.GetValue<int>("Port");
            }
            set
            {
                base.SetValue("Port", value);
            }
        }

        [DataMember(Name = "Protocol", EmitDefaultValue = false, Order = 2)]
        public string Protocol
        {
            get
            {
                return base.GetValue<string>("Protocol");
            }
            set
            {
                base.SetValue("Protocol", value);
            }
        }

        /// <summary>
        /// This field and its property counterpart represents the Load Balancer Probe Interval.
        /// This allows customers to specify custom load balance probe intervals.
        /// </summary>
        [DataMember(Name = "IntervalInSeconds", EmitDefaultValue = false, Order = 3)]
        public int? IntervalInSeconds
        {
            get
            {
                return base.GetValue<int?>(IntervalFieldName);
            }
            set
            {
                base.SetValue(IntervalFieldName, value);
            }
        }

        /// <summary>
        /// This field and its property counterpart represents the Load Balancer Probe Timeout.
        /// This allows customers to specify custom load balance probe timeouts.
        /// </summary>
        [DataMember(Name = "TimeoutInSeconds", EmitDefaultValue = false, Order = 4)]
        public int? TimeoutInSeconds
        {
            get
            {
                return base.GetValue<int?>(TimeoutFieldName);
            }
            set
            {
                base.SetValue(TimeoutFieldName, value);
            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class CertificateSetting : Mergable<CertificateSetting>
    {
        [DataMember(Name = "StoreLocation", EmitDefaultValue = false, Order = 0)]
        public string StoreLocation
        {
            get
            {
                return base.GetValue<string>("StoreLocation");
            }
            set
            {
                base.SetValue<string>("StoreLocation", value);
            }
        }

        [DataMember(Name = "StoreName", EmitDefaultValue = false, Order = 1)]
        public string StoreName
        {
            get
            {
                return base.GetValue<string>("StoreName");
            }
            set
            {
                base.SetValue<string>("StoreName", value);
            }
        }

        [DataMember(Name = "Thumbprint", EmitDefaultValue = false, Order = 2)]
        public string Thumbprint
        {
            get
            {
                return base.GetValue<string>("Thumbprint");
            }
            set
            {
                base.SetValue<string>("Thumbprint", value);
            }
        }
    }

    [CollectionDataContract(Name = "CertificateSettings", Namespace = Constants.ServiceManagementNS)]
    public class CertificateSettingList : List<CertificateSetting>
    {

    }

    [CollectionDataContract(Name = "SubnetNames", ItemName = "SubnetName", Namespace = Constants.ServiceManagementNS)]
    public class SubnetNamesCollection : Collection<string>
    {

    }

    #endregion

    #region DataDisk
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class DataVirtualHardDisk : Mergable<DataVirtualHardDisk>
    {
        [DataMember(Name = "HostCaching", EmitDefaultValue = false, Order = 0)]
        public string HostCaching
        {
            get
            {
                return this.GetValue<string>("HostCaching");
            }
            set
            {
                this.SetValue("HostCaching", value);
            }
        }

        [DataMember(Name = "DiskLabel", EmitDefaultValue = false, Order = 1)]
        public string DiskLabel
        {
            get
            {
                return this.GetValue<string>("DiskLabel");
            }
            set
            {
                this.SetValue("DiskLabel", value);
            }
        }

        [DataMember(Name = "DiskName", EmitDefaultValue = false, Order = 2)]
        public string DiskName
        {
            get
            {
                return base.GetValue<string>("DiskName");
            }
            set
            {
                base.SetValue("DiskName", value);
            }
        }

        [DataMember(Name = "Lun", EmitDefaultValue = false, Order = 3)]
        public int Lun
        {
            get
            {
                return this.GetValue<int>("Lun");
            }
            set
            {
                this.SetValue("Lun", value);
            }
        }

        [DataMember(Name = "LogicalDiskSizeInGB", EmitDefaultValue = false, Order = 4)]
        private int? logicalDiskSizeInGB
        {
            get
            {
                return this.GetField<int>("LogicalDiskSizeInGB");
            }
            set
            {
                this.SetField("LogicalDiskSizeInGB", value);
            }
        }
        public int LogicalDiskSizeInGB
        {
            get
            {
                return this.GetValue<int>("LogicalDiskSizeInGB");
            }
            set
            {
                this.SetValue("LogicalDiskSizeInGB", value);
            }
        }

        [DataMember(Name = "MediaLink", EmitDefaultValue = false, Order = 5)]
        public Uri MediaLink
        {
            get
            {
                return this.GetValue<Uri>("MediaLink");
            }
            set
            {
                this.SetValue("MediaLink", value);
            }
        }

        [DataMember(Name = "SourceMediaLink", EmitDefaultValue = false, Order = 6)]
        public Uri SourceMediaLink
        {
            get
            {
                return this.GetValue<Uri>("SourceMediaLink");
            }
            set
            {
                this.SetValue("SourceMediaLink", value);
            }
        }
    }
    #endregion

    #region OSDisk
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class OSVirtualHardDisk : Mergable<OSVirtualHardDisk>
    {
        [DataMember(Name = "HostCaching", EmitDefaultValue = false, Order = 0)]
        public string HostCaching
        {
            get
            {
                return this.GetValue<string>("HostCaching");
            }
            set
            {
                this.SetValue("HostCaching", value);
            }
        }

        [DataMember(Name = "DiskLabel", EmitDefaultValue = false, Order = 1)]
        public string DiskLabel
        {
            get
            {
                return this.GetValue<string>("DiskLabel");
            }
            set
            {
                this.SetValue("DiskLabel", value);
            }
        }

        [DataMember(Name = "DiskName", EmitDefaultValue = false, Order = 2)]
        public string DiskName
        {
            get
            {
                return this.GetValue<string>("DiskName");
            }
            set
            {
                this.SetValue("DiskName", value);
            }
        }

        [DataMember(Name = "MediaLink", EmitDefaultValue = false, Order = 3)]
        public Uri MediaLink
        {
            get
            {
                return this.GetValue<Uri>("MediaLink");
            }
            set
            {
                this.SetValue("MediaLink", value);
            }
        }

        [DataMember(Name = "SourceImageName", EmitDefaultValue = false, Order = 4)]
        public string SourceImageName
        {
            get
            {
                return this.GetValue<string>("SourceImageName");
            }
            set
            {
                this.SetValue("SourceImageName", value);
            }
        }

        [DataMember(Name = "OS", EmitDefaultValue = false, Order = 5)]
        public string OS
        {
            get
            {
                return this.GetValue<string>("OS");
            }
            set
            {
                this.SetValue("OS", value);
            }
        }
    }
    #endregion

    #region RoleOperation
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class RoleOperation : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public virtual string OperationType
        {
            get;
            set;
        }

        protected RoleOperation()
        {

        }

        #region IExtensibleDataObject Members

        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }

        #endregion
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class ShutdownRoleOperation : RoleOperation
    {
        public override string OperationType
        {
            get
            {
                return "ShutdownRoleOperation";
            }
            set
            {

            }
        }

        [DataMember(EmitDefaultValue = false, Order = 0)]
        public PostShutdownAction? PostShutdownAction { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum PostShutdownAction
    {
        [EnumMember]
        Stopped,

        [EnumMember]
        StoppedDeallocated,

        [EnumMember]
        Undefined
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class StartRoleOperation : RoleOperation
    {
        public override string OperationType
        {
            get
            {
                return "StartRoleOperation";
            }
            set
            {

            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class RestartRoleOperation : RoleOperation
    {
        public override string OperationType
        {
            get
            {
                return "RestartRoleOperation";
            }
            set
            {

            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class CaptureRoleOperation : RoleOperation
    {
        public override string OperationType
        {
            get
            {
                return "CaptureRoleOperation";
            }
            set
            {

            }
        }

        [DataMember(EmitDefaultValue = false, Order = 0)]
        public string PostCaptureAction { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public ProvisioningConfigurationSet ProvisioningConfiguration
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string TargetImageLabel { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string TargetImageName { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum PostCaptureAction
    {
        [EnumMember]
        Delete,
        [EnumMember]
        Reprovision
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum PowerState
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        Starting,
        [EnumMember]
        Started,
        [EnumMember]
        Stopping,
        [EnumMember]
        Stopped,
    }
    #endregion // RoleOperation

    #region RoleSetOperations

    [CollectionDataContract(Name = "Roles", ItemName = "Name", Namespace = Constants.ServiceManagementNS)]
    public class RoleNamesCollection : Collection<string> { }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class RoleSetOperation : Mergable<RoleSetOperation>
    {
        protected RoleSetOperation() { }

        [DataMember(EmitDefaultValue = false, Order = 0)]
        public virtual string OperationType
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public virtual RoleNamesCollection Roles
        {
            get;
            set;
        }
    }  // RoleSetOperation


    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class ShutdownRolesOperation : RoleSetOperation
    {
        public override string OperationType
        {
            get
            {
                return "ShutdownRolesOperation";
            }
            set
            {
            }
        }

        [DataMember(EmitDefaultValue = false, Order = 0)]
        public PostShutdownAction PostShutdownAction { get; set; }
    } // ShutdownRolesOperation


    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class StartRolesOperation : RoleSetOperation
    {
        public override string OperationType
        {
            get
            {
                return "StartRolesOperation";
            }
            set
            {
            }
        }
    } // StartRolesOperation

    #endregion // RoleSetOperations

    #region ResourceExtension
    [DataContract(Name = "ResourceExtensionParameterValue", Namespace = Constants.ServiceManagementNS)]
    public class ResourceExtensionParameterValue : Mergable<ResourceExtensionParameterValue>
    {
        [DataMember(Name = "Key", EmitDefaultValue = false, Order = 0)]
        public string Key
        {
            get
            {
                return base.GetValue<string>("Key");
            }
            set
            {
                base.SetValue("Key", value);
            }
        }

        [DataMember(Name = "Value", EmitDefaultValue = false, Order = 1)]
        public string Value
        {
            get
            {
                return base.GetValue<string>("Value");
            }
            set
            {
                base.SetValue("Value", value);
            }
        }
    }

    [CollectionDataContract(Name = "ResourceExtensionParameterValues", Namespace = Constants.ServiceManagementNS)]
    public class ResourceExtensionParameterValueList : List<ResourceExtensionParameterValue>
    {
        public ResourceExtensionParameterValueList()
        {
        }

        public ResourceExtensionParameterValueList(IEnumerable<ResourceExtensionParameterValue> values)
            : base(values)
        {
        }
    }


    [DataContract(Name = "ResourceExtensionReference", Namespace = Constants.ServiceManagementNS)]
    public class ResourceExtensionReference : Mergable<ResourceExtensionReference>
    {
        [DataMember(Name = "ReferenceName", EmitDefaultValue = false, Order = 0)]
        public string ReferenceName
        {
            get
            {
                return base.GetValue<string>("ReferenceName");
            }
            set
            {
                base.SetValue("ReferenceName", value);
            }
        }

        [DataMember(Name = "Publisher", EmitDefaultValue = false, Order = 1)]
        public string Publisher
        {
            get
            {
                return base.GetValue<string>("Publisher");
            }
            set
            {
                base.SetValue("Publisher", value);
            }
        }

        [DataMember(Name = "Name", EmitDefaultValue = false, Order = 2)]
        public string Name
        {
            get
            {
                return base.GetValue<string>("Name");
            }
            set
            {
                base.SetValue("Name", value);
            }
        }

        [DataMember(Name = "Version", EmitDefaultValue = false, Order = 3)]
        public string Version
        {
            get
            {
                return base.GetValue<string>("Version");
            }
            set
            {
                base.SetValue("Version", value);
            }
        }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public ResourceExtensionParameterValueList ResourceExtensionParameterValues { get; set; }
    }

    [CollectionDataContract(Name = "ResourceExtensionReferences", Namespace = Constants.ServiceManagementNS)]
    public class ResourceExtensionReferenceList : List<ResourceExtensionReference>
    {
        public ResourceExtensionReferenceList()
        {
        }

        public ResourceExtensionReferenceList(IEnumerable<ResourceExtensionReference> references)
            : base(references)
        {
        }
    }
    #endregion

    #region Certificate
    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class CertificateFile : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Data;

        [DataMember(Order = 2)]
        public string CertificateFormat { get; set; }

        [DataMember(Order = 3)]
        public string Password { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    #endregion

    #region Network
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class VirtualNetworkSite : IExtensibleDataObject
    {
        public VirtualNetworkSite(string name)
        {
            this.Name = name;
        }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; private set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string Label { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public string AffinityGroup { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public string State { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 6)]
        public bool InUse { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 7)]
        public AddressSpace AddressSpace { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 8)]
        public SubnetList Subnets { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 9)]
        public DnsSettings Dns { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public Gateway Gateway { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "VirtualNetworkSites", ItemName = "VirtualNetworkSite", Namespace = Constants.ServiceManagementNS)]
    public class VirtualNetworkSiteList : List<VirtualNetworkSite>
    {
        public VirtualNetworkSiteList()
        {
        }

        public VirtualNetworkSiteList(IEnumerable<VirtualNetworkSite> virtualNetworkSites)
            : base(virtualNetworkSites)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class DnsServer : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string Address { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "DnsServers", ItemName = "DnsServer", Namespace = Constants.ServiceManagementNS)]
    public class DnsServerList : List<DnsServer> { }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class DnsSettings : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public DnsServerList DnsServers { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Gateway : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Profile { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public LocalNetworkSiteList Sites { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public AddressSpace VPNClientAddressPool { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class LocalNetworkSite : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public AddressSpace AddressSpace { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string VpnGatewayAddress { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public ConnectionList Connections { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Connection : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Type { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "LocalNetworkSites", ItemName = "LocalNetworkSite", Namespace = Constants.ServiceManagementNS)]
    public class LocalNetworkSiteList : List<LocalNetworkSite> { }

    [CollectionDataContract(Name = "AddressPrefixes", ItemName = "AddressPrefix", Namespace = Constants.ServiceManagementNS)]
    public class AddressPrefixList : List<string> { }

    [CollectionDataContract(Name = "Connections", ItemName = "Connection", Namespace = Constants.ServiceManagementNS)]
    public class ConnectionList : List<Connection> { }


    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class AddressSpace : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public AddressPrefixList AddressPrefixes { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Subnet : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string AddressPrefix { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "Subnets", ItemName = "Subnet", Namespace = Constants.ServiceManagementNS)]
    public class SubnetList : List<Subnet> { }


    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum GatewaySize
    {
        [EnumMember]
        Small = 0,

        [EnumMember]
        Medium = 1,

        [EnumMember]
        Large = 2,

        [EnumMember]
        ExtraLarge = 3
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum NetworkState
    {
        [EnumMember]
        Created = 0,

        [EnumMember]
        Creating = 1,

        [EnumMember]
        Updating = 2,

        [EnumMember]
        Deleting = 3,

        [EnumMember]
        Unavailable = 4
    }
    #endregion

    #region Role
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    [KnownType(typeof(PersistentVMRole))]
    public class Role : Mergable<PersistentVMRole>
    {
        [DataMember(Order = 1)]
        public virtual string RoleName { get; set; }

        [DataMember(Order = 2)]
        public string OsVersion { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public virtual string RoleType { get; set; }

        [DataMember(Name = "ConfigurationSets", EmitDefaultValue = false, Order = 4)]
        public Collection<ConfigurationSet> ConfigurationSets
        {
            get
            {
                return this.GetValue<Collection<ConfigurationSet>>("ConfigurationSets");
            }
            set
            {
                base.SetValue("ConfigurationSets", value);
            }
        }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public ResourceExtensionReferenceList ResourceExtensionReferences
        {
            get
            {
                return this.GetValue<ResourceExtensionReferenceList>("ResourceExtensionReferences");
            }
            set
            {
                base.SetValue("ResourceExtensionReferences", value);
            }
        }

        public NetworkConfigurationSet NetworkConfigurationSet
        {
            get
            {
                if (ConfigurationSets == null)
                {
                    return null;
                }
                return ConfigurationSets.FirstOrDefault(
                   cset => cset is NetworkConfigurationSet) as NetworkConfigurationSet;
            }
            set
            {
                if (ConfigurationSets == null)
                {
                    ConfigurationSets = new Collection<ConfigurationSet>();
                }
                NetworkConfigurationSet networkConfigurationSet = ConfigurationSets.FirstOrDefault(
                        cset => cset is NetworkConfigurationSet) as NetworkConfigurationSet;

                if (networkConfigurationSet != null)
                {
                    ConfigurationSets.Remove(networkConfigurationSet);
                }

                ConfigurationSets.Add(value);
            }
        }

        public override object ResolveType()
        {
            if (this.GetType() != typeof(Role))
            {
                return this;
            }

            if (this.RoleType == typeof(PersistentVMRole).Name)
            {
                return base.Convert<PersistentVMRole>();
            }
            return this;
        }
    }
    #endregion

    #region PersistentVMRole
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class PersistentVMRole : Role
    {
        private static class PersistentVMRoleConstants
        {
            public const string RoleName = "RoleName";
            public const string AvailabilitySetName = "AvailabilitySetName";
            public const string DataVirtualHardDisks = "DataVirtualHardDisks";
            public const string Label = "Label";
            public const string OSVirtualHardDisk = "OSVirtualHardDisk";
            public const string RoleSize = "RoleSize";
            public const string DefaultWinRmCertificateThumbprint = "DefaultWinRmCertificateThumbprint";
            public const string ProvisionGuestAgent = "ProvisionGuestAgent";
        }

        public override string RoleName
        {
            get
            {
                return this.GetValue<string>(PersistentVMRoleConstants.RoleName);
            }
            set
            {
                this.SetValue(PersistentVMRoleConstants.RoleName, value);
            }
        }

        [DataMember(Name = PersistentVMRoleConstants.AvailabilitySetName, EmitDefaultValue = false, Order = 0)]
        public string AvailabilitySetName
        {
            get
            {
                return this.GetValue<string>(PersistentVMRoleConstants.AvailabilitySetName);
            }
            set
            {
                this.SetValue(PersistentVMRoleConstants.AvailabilitySetName, value);
            }
        }

        [DataMember(Name = PersistentVMRoleConstants.DataVirtualHardDisks, EmitDefaultValue = false, Order = 1)]
        public Collection<DataVirtualHardDisk> DataVirtualHardDisks
        {
            get
            {
                return base.GetValue<Collection<DataVirtualHardDisk>>(PersistentVMRoleConstants.DataVirtualHardDisks);
            }
            set
            {
                base.SetValue(PersistentVMRoleConstants.DataVirtualHardDisks, value);
            }
        }

        [DataMember(Name = PersistentVMRoleConstants.Label, EmitDefaultValue = false, Order = 2)]
        public string Label
        {
            get
            {
                return base.GetValue<string>(PersistentVMRoleConstants.Label);
            }
            set
            {
                base.SetValue(PersistentVMRoleConstants.Label, value);
            }
        }

        [DataMember(Name = PersistentVMRoleConstants.OSVirtualHardDisk, EmitDefaultValue = false, Order = 3)]
        public OSVirtualHardDisk OSVirtualHardDisk
        {
            get
            {
                return base.GetValue<OSVirtualHardDisk>(PersistentVMRoleConstants.OSVirtualHardDisk);
            }
            set
            {
                base.SetValue(PersistentVMRoleConstants.OSVirtualHardDisk, value);
            }
        }

        [DataMember(Name = PersistentVMRoleConstants.RoleSize, EmitDefaultValue = false, Order = 4)]
        public string RoleSize
        {
            get
            {
                return this.GetValue<string>(PersistentVMRoleConstants.RoleSize);
            }
            set
            {
                this.SetValue(PersistentVMRoleConstants.RoleSize, value);
            }
        }

        public override string RoleType
        {
            get
            {
                return typeof(PersistentVMRole).Name;
            }
            set
            {
                base.RoleType = value;
            }
        }

        [DataMember(Name = PersistentVMRoleConstants.DefaultWinRmCertificateThumbprint, EmitDefaultValue = false, Order = 5)]
        public string DefaultWinRmCertificateThumbprint
        {
            get
            {
                return base.GetValue<string>(PersistentVMRoleConstants.DefaultWinRmCertificateThumbprint);
            }
            set
            {
                base.SetValue(PersistentVMRoleConstants.DefaultWinRmCertificateThumbprint, value);
            }
        }

        [DataMember(Name = PersistentVMRoleConstants.ProvisionGuestAgent, EmitDefaultValue = false, Order = 6)]
        public bool? ProvisionGuestAgent
        {
            get
            {
                return base.GetValue<bool?>(PersistentVMRoleConstants.ProvisionGuestAgent);
            }
            set
            {
                base.SetValue(PersistentVMRoleConstants.ProvisionGuestAgent, value);
            }
        }
    }
    #endregion

    [CollectionDataContract(Namespace = Constants.ServiceManagementNS)]
    public class InstanceEndpointList : List<InstanceEndpoint> { }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class InstanceEndpoint : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string Vip { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public int PublicPort { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public int LocalPort { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public string Protocol { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Namespace = Constants.ServiceManagementNS, Name = "AvailableServices", ItemName = "AvailableService")]
    public class AvailableServicesList : List<string>, IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }
    }
}



