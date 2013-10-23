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
    using Model.PersistentVMModel;

    public static class VMDiagnosticsExtensionHelper
    {
        public const string ExtensionDefaultReferenceName = "MyDiagnosticsAgent";
        public const string ExtensionPublisher = "Microsoft.Compute";
        public const string ExtensionName = "DiagnosticsAgent";
        public const string CurrentExtensionVersion = "0.1";
        public const string ExtensionReferenceKeyStr = "DiagnosticsAgentConfigParameter";

        public static ResourceExtensionReferenceList GetResourceReferenceList(bool enabled, string storageAccountName, string storageAccountKey, string endpointStr, XmlDocument diagnosticsCfg)
        {
            ResourceExtensionReferenceList refList = new ResourceExtensionReferenceList();
            refList.Add(new Model.PersistentVMModel.ResourceExtensionReference
            {
                ReferenceName = ExtensionDefaultReferenceName,
                Publisher = ExtensionPublisher,
                Name = ExtensionName,
                Version = CurrentExtensionVersion,
                ResourceExtensionParameterValues = new ResourceExtensionParameterValueList(new int[1].Select(j => new Model.PersistentVMModel.ResourceExtensionParameterValue
                {
                    Key = ExtensionReferenceKeyStr,
                    Value = GetDiagnosticsAgentConfig(enabled, storageAccountName, storageAccountKey, endpointStr, diagnosticsCfg)
                }))
            });

            return refList;
        }

        public static List<Management.Compute.Models.ResourceExtensionReference> GetResourceReferences(ResourceExtensionReferenceList refList)
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

        public static List<Management.Compute.Models.ResourceExtensionReference> GetResourceReferences(bool enabled, string storageAccountName, string storageAccountKey, string endpointStr, XmlDocument diagnosticsCfg)
        {
            List<Management.Compute.Models.ResourceExtensionReference> extRefs = new int[1].Select(i => new Management.Compute.Models.ResourceExtensionReference
            {
                ReferenceName = ExtensionDefaultReferenceName,
                Publisher = ExtensionPublisher,
                Name = ExtensionName,
                Version = CurrentExtensionVersion,
                ResourceExtensionParameterValues = new int[1].Select(j => new Management.Compute.Models.ResourceExtensionParameterValue
                {
                    Key = ExtensionReferenceKeyStr,
                    Value = GetDiagnosticsAgentConfig(enabled, storageAccountName, storageAccountKey, endpointStr, diagnosticsCfg)
                }).ToList()
            }).ToList();

            return extRefs;
        }

        private static string GetDiagnosticsAgentConfig(bool enabled, string storageAccoutName, string storageAccountKey, string endpointStr, XmlDocument diagnosticsCfg)
        {
            string storageConnectionStr = "DefaultEndpointsProtocol=https;AccountName=" + storageAccoutName + ";AccountKey=" + storageAccountKey;
            if (!string.IsNullOrEmpty(endpointStr))
            {
                storageConnectionStr += ";" + endpointStr;
            }

            XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";
            var publicCfg = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("Configuration",
                    new XElement("Enabled", bool.TrueString.ToLower()),
                    new XElement("Public",
                        new XElement(configNameSpace + "PublicConfig",
                            new XElement(configNameSpace + "WadCfg", string.Empty),
                            new XElement(configNameSpace + "StorageAccountConnectionString", string.Empty)
                        )
                    )
                )
            );

            SetConfigValue(publicCfg, "WadCfg", diagnosticsCfg);
            SetConfigValue(publicCfg, "StorageAccountConnectionString", storageConnectionStr);

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
    }
}
