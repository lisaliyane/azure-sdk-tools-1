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
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using Properties;

    /// <summary>
    /// Implementation of <see cref="IProfileStore"/> that reads
    /// the settings from the old formats used in azure powershell
    /// versions pre 0.7.
    /// </summary>
    public class PowershellOldSettingsProfileStore : IProfileStore
    {
        private readonly string dataDir;

        private string AzureDataDir
        {
            get { return dataDir ?? GlobalPathInfo.AzureAppDir; }
        }

        public PowershellOldSettingsProfileStore()
            : this(null)
        {
        }

        public PowershellOldSettingsProfileStore(string dataDir)
        {
            this.dataDir = dataDir;
        }

        public void Save(ProfileData profile)
        {
            // This will never be used, all writes go to the new format
            throw new InvalidOperationException();
        }

        public ProfileData Load()
        {
            ProfileData profile = new ProfileData();
            LoadEnvironments(profile);
            string defaultSubscriptionId = LoadDefaultSubscription();
            LoadSubscriptions(profile, defaultSubscriptionId);
            return profile;
        }

        public void DestroyData()
        {
            // deliberate noop
        }

        private void LoadEnvironments(ProfileData profile)
        {
            profile.Environments = Transform(PathTo(Resources.EnvironmentsFileName), "WindowsAzureEnvironment",
                el => new AzureEnvironmentData
                {
                    Name = SafeValue(el.Element("Name")),
                    PublishSettingsFileUrl = SafeValue(el.Element("PublishSettingsFileUrl")),
                    ServiceEndpoint = SafeValue(el.Element("ServiceEndpoint")),
                    ManagementPortalUrl = SafeValue(el.Element("ManagementPortalUrl"))
                });
            profile.DefaultEnvironmentName = EnvironmentName.AzureCloud;
        }

        private void LoadSubscriptions(ProfileData profile, string defaultSubscriptionId)
        {
            XNamespace subns = "urn:Microsoft.WindowsAzure.Management:WaPSCmdlets";

            profile.Subscriptions = Transform(PathTo(Resources.SubscriptionDataFileName), subns + "Subscription",
                el => new AzureSubscriptionData
                {
                    Name = el.Attribute("name").Value,
                    SubscriptionId = SafeValue(el.Element(subns + "SubscriptionId")),
                    ManagementCertificate = SafeValue(el.Element(subns + "Thumbprint")),
                    ManagementEndpoint = SafeValue(el.Element(subns + "ServiceEndpoint")),
                    IsDefault = defaultSubscriptionId != null && SafeValue(el.Element(subns + "SubscriptionId")) == defaultSubscriptionId
                });
        }

        private class ConfigObject
        {
            public string subscription { get; set; }
            public string subscriptionName { get; set; }
            public string endpoint { get; set; }
        }

        private string LoadDefaultSubscription()
        {
            string configPath = PathTo(Resources.ConfigurationFileName);
            try
            {
                if (File.Exists(configPath))
                {
                    var config = JsonConvert.DeserializeObject<ConfigObject>(File.ReadAllText(configPath));
                    return config.subscription;
                }
            }
            catch (Exception)
            {
                // As usual, treat any parse or processing failures as if file doesn't exist.
            }
            return null;
        }

        private IEnumerable<TResult> Transform<TResult>(string path, XName elementName,
            Func<XElement, TResult> selector)
        {
            try
            {
                if (File.Exists(path))
                {
                    XDocument doc = XDocument.Load(path);
                    return doc.Descendants(elementName).Select(selector).ToList();
                }
            }
            catch (Exception)
            {
                // Treat any failure as file not existing
            }
            return new TResult[0];
        }

        private string PathTo(string fileName)
        {
            return Path.Combine(AzureDataDir, fileName);
        }

        private string SafeValue(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return element.Value;
        }
    }
}