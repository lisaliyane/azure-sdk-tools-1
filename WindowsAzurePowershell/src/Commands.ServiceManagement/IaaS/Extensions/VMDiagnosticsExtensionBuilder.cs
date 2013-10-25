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
    using Management.Compute.Models;
    using Management.Storage;
    using Microsoft.WindowsAzure.Storage;
    using Model.PersistentVMModel;

    public class VMDiagnosticsExtensionBuilder
    {
        public const string ExtensionDefaultReferenceName = "MyDiagnosticsAgent";
        public const string ExtensionPublisher = "Microsoft.Compute";
        public const string ExtensionName = "DiagnosticsAgent";
        public const string CurrentExtensionVersion = "0.1";
        public const string ExtensionReferenceKeyStr = "DiagnosticsAgentConfigParameter";

        private string storageAccountName;
        private string storageAccountKey;
        private string endpointStr;
        XmlDocument wadCfg;
        private bool enabled;

        public VMDiagnosticsExtensionBuilder(string storageAccountName, string storageAccountKey, string endpointStr, XmlDocument wadCfg, bool enabled)
        {
            if (enabled && string.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentNullException(storageAccountName);
            }

            if (enabled && string.IsNullOrEmpty(storageAccountKey))
            {
                throw new ArgumentNullException(storageAccountName);
            }

            this.storageAccountName = storageAccountName;
            this.storageAccountKey = storageAccountKey;
            this.endpointStr = endpointStr;
            this.wadCfg = wadCfg;
            this.enabled = enabled;
        }

        public VMDiagnosticsExtensionBuilder(string extensionCfg)
        {
            LoadFrom(extensionCfg);
        }

        public string StorageAccountName
        {
            get
            {
                return this.storageAccountName;
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

        private static string GetDiagnosticsAgentConfig(bool enabled, string storageAccoutName, string storageAccountKey, string endpointStr, XmlDocument wadCfg)
        {
            string storageConnectionStr = "DefaultEndpointsProtocol=https;AccountName=" + storageAccoutName + ";AccountKey=" + storageAccountKey;
            if (!string.IsNullOrEmpty(endpointStr))
            {
                storageConnectionStr += ";" + endpointStr;
            }

            XDocument publicCfg = null;

            if (enabled)
            {
                XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";
                publicCfg = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement("Configuration",
                        new XElement("Enabled", enabled.ToString().ToLower()),
                        new XElement("Public",
                            new XElement(configNameSpace + "PublicConfig",
                                new XElement(configNameSpace + "WadCfg", string.Empty),
                                new XElement(configNameSpace + "StorageAccountConnectionString", string.Empty)
                            )
                        )
                    )
                );

                SetConfigValue(publicCfg, "WadCfg", wadCfg);
                SetConfigValue(publicCfg, "StorageAccountConnectionString", storageConnectionStr);
            }
            else
            {
                XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";
                publicCfg = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement("Configuration",
                        new XElement("Enabled", bool.FalseString.ToLower())
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
                ResourceExtensionParameterValues = new ResourceExtensionParameterValueList(new int[1].Select(j => new Model.PersistentVMModel.ResourceExtensionParameterValue
                {
                    Key = ExtensionReferenceKeyStr,
                    Value = GetDiagnosticsAgentConfig(enabled, storageAccountName, storageAccountKey, endpointStr, wadCfg)
                }))
            };
        }

        private void LoadFrom(string extensionCfg)
        {
            string connStr = GetConfigValue(extensionCfg, "StorageAccountConnectionString");
            var cloudStorageAccount = CloudStorageAccount.Parse(connStr);

            if (cloudStorageAccount != null)
            {
                this.storageAccountName = cloudStorageAccount.Credentials.AccountName;
            }

            this.enabled = bool.Parse(GetConfigValue(extensionCfg, "Enabled"));
            this.wadCfg = new XmlDocument();
            wadCfg.LoadXml(GetConfigValue(extensionCfg, "WadCfg"));
        }
    }
}
