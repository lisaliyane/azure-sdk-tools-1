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
    using System.Collections.Generic;
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
        public const string ExtensionPublisher = "Microsoft.Compute";
        public const string ExtensionName = "DiagnosticsAgent";
        public const string CurrentExtensionVersion = "0.1";
        public const string ExtensionReferenceKeyStr = "DiagnosticsAgentConfigParameter";

        private const string ConfigurationElem = "Configuration";
        private const string EnabledElem = "Enabled";
        private const string PublicElem = "Public";
        private const string PublicConfigElem = "PublicConfig";
        private const string WadCfgElem = "WadCfg";
        private const string StorageAccountConnectionStringElem = "StorageAccountConnectionString";

        private const string DefaultEndpointsProtocol = "https";
        private const string StorageConnectionStringFormat = "DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};{3}";

        private string storageConnectionString;
        private string storageAccountName;
        private string storageAccountKey;
        private Uri[] endpoints;
        private XmlDocument wadCfg;
        private bool enabled;

        public VMDiagnosticsExtensionBuilder()
        {
            this.enabled = false;
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

            this.storageAccountName = storageAccountName;
            this.storageAccountKey = storageAccountKey;
            this.endpoints = endpoints;
            this.wadCfg = wadCfg;
            this.enabled = true;
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
            get
            {
                return this.storageAccountName;
            }
        }

        public string StorageAccountKey
        {
            get
            {
                return this.storageAccountKey;
            }
        }

        public Uri[] Endpoints
        {
            get
            {
                return this.endpoints;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
        }

        public XmlDocument WadCfg
        {
            get
            {
                return this.wadCfg;
            }
        }

        public string StorageConnectionString
        {
            get
            {
                return this.storageConnectionString;
            }
        }

        public static ResourceExtensionReferenceList GetResourceExtensionReferenceList(IList<Management.Compute.Models.ResourceExtensionReference> refList)
        {
            ResourceExtensionReferenceList extRefs = new ResourceExtensionReferenceList();
            if (refList != null)
            {
                foreach (var r in refList)
                {
                    extRefs.Add(new Model.PersistentVMModel.ResourceExtensionReference
                    {
                        Name = r.Name,
                        Publisher = r.Publisher,
                        ReferenceName = r.ReferenceName,
                        Version = r.Version,
                        ResourceExtensionParameterValues = r.ResourceExtensionParameterValues.Select(p => new Model.PersistentVMModel.ResourceExtensionParameterValue
                        {
                            Key = p.Key,
                            Value = p.Value
                        }).ToList() as ResourceExtensionParameterValueList
                    });
                }
            }

            return extRefs;
        }

        public static List<Management.Compute.Models.ResourceExtensionReference> GetListOfGetResourceReference(ResourceExtensionReferenceList refList)
        {
            List<Management.Compute.Models.ResourceExtensionReference> extRefs = new List<Management.Compute.Models.ResourceExtensionReference>();
            if (refList != null)
            {
                foreach (var r in refList)
                {
                    extRefs.Add(new Management.Compute.Models.ResourceExtensionReference
                    {
                        Name = r.Name,
                        Publisher = r.Publisher,
                        ReferenceName = r.ReferenceName,
                        Version = r.Version,
                        ResourceExtensionParameterValues = r.ResourceExtensionParameterValues.Select(p => new Management.Compute.Models.ResourceExtensionParameterValue
                        {
                            Key = p.Key,
                            Value = p.Value
                        }).ToList()
                    });
                }
            }

            return extRefs;
        }

        private static string GetDiagnosticsAgentConfig(bool enabled, string storageAccountName, string storageAccountKey, Uri[] endpoints, XmlDocument wadCfg)
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

                SetConfigValue(publicCfg, WadCfgElem, wadCfg);
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
                Publisher = ExtensionPublisher,
                Name = ExtensionName,
                Version = CurrentExtensionVersion,
                ResourceExtensionParameterValues = new ResourceExtensionParameterValueList(new int[1].Select(i => new Model.PersistentVMModel.ResourceExtensionParameterValue
                {
                    Key = ExtensionReferenceKeyStr,
                    Value = GetDiagnosticsAgentConfig(
                        this.enabled,
                        this.storageAccountName,
                        this.storageAccountKey,
                        this.endpoints,
                        this.wadCfg)
                }))
            };
        }

        private void LoadFrom(string extensionCfg)
        {
            this.storageConnectionString = GetConfigValue(extensionCfg, StorageAccountConnectionStringElem);
            var cloudStorageAccount = CloudStorageAccount.Parse(this.storageConnectionString);
            if (cloudStorageAccount != null)
            {
                this.storageAccountName = cloudStorageAccount.Credentials == null ? null : cloudStorageAccount.Credentials.AccountName;
            }

            this.enabled = bool.Parse(GetConfigValue(extensionCfg, EnabledElem));
            this.wadCfg = new XmlDocument();
            wadCfg.LoadXml(GetConfigValue(extensionCfg, WadCfgElem));
        }
    }
}
