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
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;
    using System.Xml.Linq;
    using Helpers;
    using Management.Compute;
    using Management.Compute.Models;
    using Properties;
    using Utilities.CloudService;
    using Utilities.Common;

    public abstract class BaseAzureServiceExtensionCmdlet : ServiceManagementBaseCmdlet
    {
        protected const string PublicConfigStr = "PublicConfig";
        protected const string PrivateConfigStr = "PrivateConfig";
        protected const string XmlNameSpaceAttributeStr = "xmlns";

        protected ExtensionManager ExtensionManager { get; set; }
        protected string ExtensionNameSpace { get; set; }
        protected string ExtensionType { get; set; }
        protected XDocument PublicConfigurationXmlTemplate { get; set; }
        protected XDocument PrivateConfigurationXmlTemplate { get; set; }
        protected XDocument PublicConfigurationXml { get; set; }
        protected XDocument PrivateConfigurationXml { get; set; }
        protected string PublicConfiguration { get; set; }
        protected string PrivateConfiguration { get; set; }
        protected DeploymentGetResponse Deployment { get; set; }
        //protected Deployment Deployment { get; set; }

        public virtual string ServiceName { get; set; }
        public virtual string Slot { get; set; }
        public virtual string[] Role { get; set; }
        public virtual X509Certificate2 X509Certificate { get; set; }
        public virtual string CertificateThumbprint { get; set; }
        public virtual string ThumbprintAlgorithm { get; set; }
        public virtual SwitchParameter UninstallConfiguration { get; set; }

        public BaseAzureServiceExtensionCmdlet()
            : base()
        {
            ServiceManagementProfile.Initialize();
        }

        protected virtual void ValidateParameters()
        {
        }

        protected void ValidateService()
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(General.TryGetServiceRootPath(CurrentPath()),
                ServiceName, null, null, null, null, CurrentSubscription.SubscriptionId, out serviceName);

            if (string.IsNullOrEmpty(serviceName))
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindServiceName, ServiceName));
            }
            else
            {
                ServiceName = serviceName;
                if (ComputeClient.HostedServices.CheckNameAvailability(ServiceName).IsAvailable)
                {
                    throw new Exception(string.Format(Resources.ServiceExtensionCannotFindServiceName, ServiceName));
                }
            }
            ExtensionManager = new ExtensionManager(this, ServiceName);
        }

        protected void ValidateDeployment()
        {
            Slot = string.IsNullOrEmpty(Slot) ? DeploymentSlot.Production.ToString() : Slot;

            Deployment = GetDeployment(Slot);
            if (!UninstallConfiguration)
            {
                if (Deployment == null)
                {
                    throw new Exception(string.Format(Resources.ServiceExtensionCannotFindDeployment, ServiceName, Slot));
                }
                Deployment.ExtensionConfiguration = Deployment.ExtensionConfiguration ?? new Microsoft.WindowsAzure.Management.Compute.Models.ExtensionConfiguration();
            }
        }

        protected void ValidateRoles()
        {
            Role = Role == null ? new string[0] : Role.Select(r => r == null ? string.Empty : r.Trim()).Distinct().ToArray();
            foreach (string roleName in Role)
            {
                if (Deployment.Roles == null || !Deployment.Roles.Any(r => r.RoleName == roleName))
                {
                    throw new Exception(string.Format(Resources.ServiceExtensionCannotFindRole, roleName, Slot, ServiceName));
                }

                if (string.IsNullOrWhiteSpace(roleName))
                {
                    throw new Exception(Resources.ServiceExtensionCannotFindRoleName);
                }
            }
        }

        protected void ValidateThumbprint(bool uploadCert)
        {
            if (X509Certificate != null)
            {
                var operationDescription = string.Format(Resources.ServiceExtensionUploadingCertificate, CommandRuntime, X509Certificate.Thumbprint);
                if (uploadCert)
                {
                    var parameters = new ServiceCertificateCreateParameters
                    {
                        Data = CertUtilsNewSM.GetCertificateData(X509Certificate),
                        Password = null,
                        CertificateFormat = CertificateFormat.Pfx
                    };

                    ExecuteClientActionNewSM(
                        null,
                        CommandRuntime.ToString(),
                        () => this.ComputeClient.ServiceCertificates.Create(this.ServiceName, parameters));
                }

                CertificateThumbprint = X509Certificate.Thumbprint;
            }
            else
            {
                CertificateThumbprint = CertificateThumbprint ?? string.Empty;
            }

            ThumbprintAlgorithm = ThumbprintAlgorithm ?? string.Empty;
        }

        protected virtual void ValidateConfiguration()
        {
        }

        private static string GetConfigValue(string xmlText, string element)
        {
            XDocument config = XDocument.Parse(xmlText);
            var result = from d in config.Descendants()
                         where d.Name.LocalName == element
                         select d.Descendants().Any() ? d.ToString() : d.Value;
            return result.FirstOrDefault();
        }

        protected string GetPublicConfigValue(HostedServiceListExtensionsResponse.Extension extension, string element)
        {
            return extension == null ? string.Empty : GetConfigValue(extension.PublicConfiguration, element);
        }

        private void SetConfigValue(XDocument config, string element, Object value)
        {
            if (config != null && value != null)
            {
                config.Descendants().ForEach(e =>
                {
                    if (e.Name.LocalName == element)
                    {
                        if (value.GetType().Equals(typeof(XmlDocument)))
                        {
                            e.ReplaceAll(XElement.Load(new XmlNodeReader(value as XmlDocument)));
                            e.Descendants().ForEach(d =>
                            {
                                if (string.IsNullOrEmpty(d.Name.NamespaceName))
                                {
                                    d.Name = config.Root.Name.Namespace + d.Name.LocalName;
                                }
                            });
                        }
                        else
                        {
                            e.SetValue(value.ToString());
                        }
                    }
                });
            }
        }

        protected void SetPublicConfigValue(string element, Object value)
        {
            SetConfigValue(PublicConfigurationXml, element, value);
        }

        protected void SetPrivateConfigValue(string element, Object value)
        {
            SetConfigValue(PrivateConfigurationXml, element, value);
        }

        protected void ChangeDeployment(ExtensionConfiguration extConfig)
        {
            DeploymentChangeConfigurationParameters changeConfigInput = new DeploymentChangeConfigurationParameters
            {
                Configuration = Deployment.Configuration,
                ExtensionConfiguration = Deployment.ExtensionConfiguration = extConfig,
                Mode = DeploymentChangeConfigurationMode.Auto,
                TreatWarningsAsError = false
            };

            ExecuteClientActionNewSM(
                null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.Deployments.ChangeConfigurationBySlot(
                    ServiceName,
                    (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), Slot, true),
                    changeConfigInput));
        }

        protected DeploymentGetResponse GetDeployment(string slot)
        {
            var slotType = (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), slot, true);

            DeploymentGetResponse d = null;
            InvokeInOperationContext(() =>
            {
                try
                {
                    d = this.ComputeClient.Deployments.GetBySlot(this.ServiceName, slotType);
                }
                catch (CloudException ex)
                {
                    if (ex.Response.StatusCode != HttpStatusCode.NotFound && IsVerbose() == false)
                    {
                        this.WriteExceptionDetails(ex);
                    }
                }
            });

            return d;
        }
    }
}
