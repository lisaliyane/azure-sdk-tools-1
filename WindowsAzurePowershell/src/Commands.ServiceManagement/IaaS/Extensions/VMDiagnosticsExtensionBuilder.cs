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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Model.PersistentVMModel;
    using Utilities.Common;

    public class VMDiagnosticsExtensionBuilder
    {
        public const string ExtensionDefaultReferenceName = "MyDiagnosticsAgent";
        public const string ExtensionDefaultPublisher = "Microsoft.Compute";
        public const string ExtensionDefaultName = "DiagnosticsAgent";
        public const string CurrentExtensionVersion = "0.1";
        public const string ExtensionReferenceKeyStr = "DiagnosticsAgentConfigParameter";

        private const string ConfigurationElem = "Configuration";
        private const string EnabledElem = "Enabled";
        private const string PublicElem = "Public";
        private const string PublicConfigElem = "PublicConfig";
        private const string WadCfgElem = "WadCfg";
        private const string DiagnosticMonitorConfigurationElem = "DiagnosticMonitorConfiguration";
        private const string StorageAccountConnectionStringElem = "StorageAccountConnectionString";

        private const string DefaultEndpointsProtocol = "https";
        private const string StorageConnectionStringFormat = "DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};{3}";

        public VMDiagnosticsExtensionBuilder()
        {
            this.Enabled = false;
        }

        public VMDiagnosticsExtensionBuilder(string storageAccountName, string storageAccountKey, Uri[] endpoints, XmlDocument wadCfg)
        {
            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentNullException("storageAccountName");
            }

            if (string.IsNullOrEmpty(storageAccountKey))
            {
                throw new ArgumentNullException("storageAccountKey");
            }

            if (endpoints != null && endpoints.Length != 3)
            {
                throw new ArgumentOutOfRangeException(
                    "endpoints",
                    "The parameter endpoints must be null or must contain three items: the blob, queue, and table endpoints.");
            }

            if (wadCfg == null)
            {
                throw new ArgumentNullException("wadCfg");
            }

            this.StorageAccountName = storageAccountName;
            this.StorageAccountKey = storageAccountKey;
            this.Endpoints = endpoints;
            this.DiagnosticsConfiguration = wadCfg;
            this.Enabled = true;
        }

        public VMDiagnosticsExtensionBuilder(string extensionCfg)
        {
            if (string.IsNullOrEmpty(extensionCfg))
            {
                throw new ArgumentNullException("extensionCfg");
            }

            LoadFrom(extensionCfg);
        }

        public string StorageAccountName
        {
            get;
            private set;
        }

        public string StorageAccountKey
        {
            get;
            private set;
        }

        public Uri[] Endpoints
        {
            get;
            private set;
        }

        public bool Enabled
        {
            get;
            private set;
        }

        public XmlDocument DiagnosticsConfiguration
        {
            get;
            private set;
        }

        internal static string GetDiagnosticsAgentConfig(bool enabled, string storageAccountName, string storageAccountKey, Uri[] endpoints, XmlDocument diagnosticsCfg)
        {
            XDocument publicCfg = null;
            if (enabled)
            {
                XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";
                publicCfg = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(ConfigurationElem,
                        new XElement(EnabledElem, enabled.ToString().ToLower()),
                        new XElement(PublicElem,
                            new XElement(configNameSpace + PublicConfigElem,
                                new XElement(configNameSpace + WadCfgElem, string.Empty),
                                new XElement(configNameSpace + StorageAccountConnectionStringElem, string.Empty)
                            )
                        )
                    )
                );

                var cloudStorageCredential = new StorageCredentials(storageAccountName, storageAccountKey);
                var cloudStorageAccount = endpoints == null ? new CloudStorageAccount(cloudStorageCredential, true)
                                                            : new CloudStorageAccount(cloudStorageCredential, endpoints[0], endpoints[1], endpoints[2]); // {blob, queue, table}
                var storageConnectionStr = cloudStorageAccount.ToString(true);

                SetConfigValue(publicCfg, WadCfgElem, diagnosticsCfg);
                SetConfigValue(publicCfg, StorageAccountConnectionStringElem, storageConnectionStr);
            }
            else
            {
                publicCfg = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(ConfigurationElem,
                        new XElement(EnabledElem, enabled.ToString().ToLower())
                    )
                );
            }

            return publicCfg.ToString();
        }

        private static void SetConfigValue(XDocument config, string element, Object value)
        {
            if (config != null && value != null)
            {
                var ds = config.Descendants();
                foreach (var e in ds)
                {
                    if (e.Name.LocalName == element)
                    {
                        if (value.GetType().Equals(typeof(XmlDocument)))
                        {
                            e.ReplaceAll(XElement.Load(new XmlNodeReader(value as XmlDocument)));

                            var es = e.Descendants();
                            foreach (var d in es)
                            {
                                if (string.IsNullOrEmpty(d.Name.NamespaceName))
                                {
                                    d.Name = e.Name.Namespace + d.Name.LocalName;
                                }
                            };
                        }
                        else
                        {
                            e.SetValue(value.ToString());
                        }
                        break;
                    }
                };
            }
        }

        private static string GetConfigValue(string xmlText, string element)
        {
            XDocument config = XDocument.Parse(xmlText);
            var result = from d in config.Descendants()
                         where d.Name.LocalName == element
                         select d.Descendants().Any() ? d.ToString() : d.Value;
            return result.FirstOrDefault();
        }

        public Model.PersistentVMModel.ResourceExtensionReference GetResourceReference()
        {
            return new Model.PersistentVMModel.ResourceExtensionReference
            {
                ReferenceName = ExtensionDefaultReferenceName,
                Publisher = ExtensionDefaultPublisher,
                Name = ExtensionDefaultName,
                Version = CurrentExtensionVersion,
                ResourceExtensionParameterValues = new ResourceExtensionParameterValueList(new int[1].Select(i => new Model.PersistentVMModel.ResourceExtensionParameterValue
                {
                    Key = ExtensionReferenceKeyStr,
                    Value = GetDiagnosticsAgentConfig(
                        this.Enabled,
                        this.StorageAccountName,
                        this.StorageAccountKey,
                        this.Endpoints,
                        this.DiagnosticsConfiguration)
                }))
            };
        }

        private void LoadFrom(string extensionCfg)
        {
            this.Enabled = bool.Parse(GetConfigValue(extensionCfg, EnabledElem).ToLower());

            var storageConnectionString = GetConfigValue(extensionCfg, StorageAccountConnectionStringElem);
            var cloudStorageAccount = string.IsNullOrEmpty(storageConnectionString) ? null : CloudStorageAccount.Parse(storageConnectionString);
            if (cloudStorageAccount != null && cloudStorageAccount.Credentials != null)
            {
                this.StorageAccountName = cloudStorageAccount.Credentials.AccountName;
                var exportedKey = cloudStorageAccount.Credentials.ExportKey();
                this.StorageAccountKey = exportedKey == null ? null : Convert.ToBase64String(exportedKey);
                this.Endpoints = new Uri[3] { cloudStorageAccount.BlobEndpoint, cloudStorageAccount.QueueEndpoint, cloudStorageAccount.TableEndpoint };
            }
            else
            {
                this.StorageAccountName = this.StorageAccountKey = null;
                this.Endpoints = null;
            }

            var daCfgContent = GetConfigValue(extensionCfg, DiagnosticMonitorConfigurationElem);
            if (!string.IsNullOrEmpty(daCfgContent))
            {
                XDocument daCfgDoc = XDocument.Parse(daCfgContent);
                daCfgDoc.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();
                daCfgDoc.Descendants().ForEach(e => e.Name = e.Name.LocalName);
                this.DiagnosticsConfiguration = new XmlDocument();
                this.DiagnosticsConfiguration.LoadXml(daCfgDoc.ToString());
            }
            else
            {
                this.DiagnosticsConfiguration = null;
            }
        }
    }
}
