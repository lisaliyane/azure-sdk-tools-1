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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Extensions
{
    using System;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Management.Storage;
    using Properties;

    public abstract class BaseAzureServiceDiagnosticsExtensionCmdlet : BaseAzureServiceExtensionCmdlet
    {
        protected const string ConnectionQualifiersElemStr = "ConnectionQualifiers";
        protected const string DefaultEndpointsProtocolElemStr = "DefaultEndpointsProtocol";
        protected const string StorageAccountElemStr = "StorageAccount";
        protected const string StorageNameElemStr = "Name";
        protected const string StorageKeyElemStr = "StorageKey";
        protected const string WadCfgElemStr = "WadCfg";
        protected const string DiagnosticsExtensionNamespace = "Microsoft.Windows.Azure.Extensions";
        protected const string DiagnosticsExtensionType = "Diagnostics";

        protected string StorageKey { get; set; }
        protected string ConnectionQualifiers { get; set; }
        protected string DefaultEndpointsProtocol { get; set; }

        public virtual string StorageAccountName { get; set; }
        public virtual XmlDocument DiagnosticsConfiguration { get; set; }

        public BaseAzureServiceDiagnosticsExtensionCmdlet()
            : base()
        {
            Initialize();
        }

        protected void Initialize()
        {
            ExtensionNameSpace = DiagnosticsExtensionNamespace;
            ExtensionType = DiagnosticsExtensionType;

            XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";

            PublicConfigurationXmlTemplate = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(configNameSpace + PublicConfigStr,
                    new XElement(configNameSpace + StorageAccountElemStr,
                        new XElement(configNameSpace + ConnectionQualifiersElemStr, string.Empty),
                        new XElement(configNameSpace + DefaultEndpointsProtocolElemStr, string.Empty),
                        new XElement(configNameSpace + StorageNameElemStr, string.Empty)
                    ),
                    new XElement(configNameSpace + WadCfgElemStr, string.Empty)
                )
            );

            PrivateConfigurationXmlTemplate = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XProcessingInstruction("xml-stylesheet", @"type=""text/xsl"" href=""style.xsl"""),
                new XElement(PrivateConfigStr,
                    new XElement(StorageKeyElemStr, "{0}")
                )
            );
        }

        protected void ValidateStorageAccount()
        {
            var storageService = this.StorageClient.StorageAccounts.Get(StorageAccountName);
            if (storageService == null)
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindStorageAccountName, StorageAccountName));
            }

            var storageKeys = this.StorageClient.StorageAccounts.GetKeys(storageService.ServiceName);
            if (storageKeys == null || storageKeys.PrimaryKey == null || storageKeys.SecondaryKey == null)
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindStorageAccountKey, StorageAccountName));
            }
            StorageKey = storageKeys.PrimaryKey != null ? storageKeys.PrimaryKey : storageKeys.SecondaryKey;

            StringBuilder endpointStr = new StringBuilder();
            endpointStr.AppendFormat("BlobEndpoint={0};", storageService.Properties.Endpoints[0]);
            endpointStr.AppendFormat("QueueEndpoint={0};", storageService.Properties.Endpoints[1]);
            endpointStr.AppendFormat("TableEndpoint={0}", storageService.Properties.Endpoints[2]);
            ConnectionQualifiers = endpointStr.ToString();
            DefaultEndpointsProtocol = "https";
        }

        protected override void ValidateConfiguration()
        {
            PublicConfigurationXml = new XDocument(PublicConfigurationXmlTemplate);
            SetPublicConfigValue(ConnectionQualifiersElemStr, ConnectionQualifiers);
            SetPublicConfigValue(DefaultEndpointsProtocolElemStr, DefaultEndpointsProtocol);
            SetPublicConfigValue(StorageNameElemStr, StorageAccountName);
            SetPublicConfigValue(WadCfgElemStr, DiagnosticsConfiguration);
            PublicConfiguration = PublicConfigurationXml.ToString();

            PrivateConfigurationXml = new XDocument(PrivateConfigurationXmlTemplate);
            SetPrivateConfigValue(StorageKeyElemStr, StorageKey);
            PrivateConfiguration = PrivateConfigurationXml.ToString();
        }
    }
}
