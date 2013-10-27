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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.UnitTests.Cmdlets.IaaS.Extensions
{
    using System;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using ServiceManagement.IaaS.Extensions;

    [TestClass]
    public class IaaSExtensionTests
    {
        const string daCfgContent = @"<DiagnosticMonitorConfiguration configurationChangePollInterval=""PT10M""  overallQuotaInMB=""4096"">" +
                                    @"<DiagnosticInfrastructureLogs  scheduledTransferLogLevelFilter=""Verbose"" bufferQuotaInMB=""100"" scheduledTransferPeriod=""PT1M""/>" +
                                    @"<Directories  bufferQuotaInMB=""1000"" scheduledTransferPeriod=""PT1M"" >" +
                                    @"<CrashDumps container=""crashdumpdir"" directoryQuotaInMB=""500""/>" +
                                    @"<FailedRequestLogs container=""frldir"" directoryQuotaInMB=""100""/>" +
                                    @"<IISLogs container=""iislogdir"" directoryQuotaInMB=""100""/>" +
                                    @"</Directories>" +
                                    @"<PerformanceCounters bufferQuotaInMB=""100""  scheduledTransferPeriod=""PT1M"" >" +
                                    @"<PerformanceCounterConfiguration counterSpecifier=""\Processor(*)\% Processor Time"" sampleRate=""PT10S"" />" +
                                    @"<PerformanceCounterConfiguration counterSpecifier=""\Network Interface(*)\Bytes Received/sec"" sampleRate=""PT10S""/>" +
                                    @"</PerformanceCounters>" +
                                    @"<WindowsEventLog scheduledTransferLogLevelFilter=""Verbose"" bufferQuotaInMB=""100""  scheduledTransferPeriod=""PT1M"" >" +
                                    @"<DataSource name=""Application!*""/>" +
                                    @"<DataSource name=""Setup!*""/>" +
                                    @"<DataSource name=""System!*""/>" +
                                    @"</WindowsEventLog>" +
                                    @"<Logs  scheduledTransferLogLevelFilter=""Verbose"" bufferQuotaInMB=""100"" scheduledTransferPeriod=""PT1M""/>" +
                                    @"</DiagnosticMonitorConfiguration>";

        [TestMethod]
        public void VMDiagnosticsExtensionBuilderDisabledTest()
        {
            var builder = new VMDiagnosticsExtensionBuilder();

            Assert.IsFalse(builder.Enabled);
            Assert.IsTrue(string.IsNullOrEmpty(builder.StorageAccountName));
            Assert.IsTrue(string.IsNullOrEmpty(builder.StorageAccountKey));
            Assert.IsNull(builder.DiagnosticsConfiguration);
            Assert.IsNull(builder.Endpoints);

            var reference = builder.GetResourceReference();
            Assert.IsNotNull(reference != null);
            Assert.IsTrue(reference.Version       == VMDiagnosticsExtensionBuilder.CurrentExtensionVersion);
            Assert.IsTrue(reference.Publisher     == VMDiagnosticsExtensionBuilder.ExtensionDefaultPublisher);
            Assert.IsTrue(reference.Name          == VMDiagnosticsExtensionBuilder.ExtensionDefaultName);
            Assert.IsTrue(reference.ReferenceName == VMDiagnosticsExtensionBuilder.ExtensionDefaultReferenceName);

            Assert.IsNotNull(reference.ResourceExtensionParameterValues);
            Assert.IsTrue(1 == reference.ResourceExtensionParameterValues.FindAll(r => r.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr).Count);

            var item = reference.ResourceExtensionParameterValues.Find(r => r.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr);
            var key = item.Key;
            Assert.IsFalse(string.IsNullOrEmpty(key));
            Assert.IsTrue(key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr);

            var val = item.Value;
            Assert.IsFalse(string.IsNullOrEmpty(val));
            XDocument document = XDocument.Parse(val);

            var publicCfg = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                    new XElement("Configuration",
                        new XElement("Enabled", false.ToString().ToLower())
                )
            );
            Assert.IsTrue(document.ToString() == publicCfg.ToString());

            var builder2 = new VMDiagnosticsExtensionBuilder(document.ToString());
            var reference2 = builder2.GetResourceReference();
            Assert.IsTrue(1 == reference2.ResourceExtensionParameterValues.FindAll(r => r.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr).Count);
            XDocument document2 = XDocument.Parse(reference2.ResourceExtensionParameterValues.Find(r => r.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr).Value);
            Assert.IsTrue(document.ToString() == document2.ToString());
        }

        [TestMethod]
        public void VMDiagnosticsExtensionBuilderEnabledWithNullEndpointsTest()
        {
            string storageAccountName = "testname";
            var bytes = Encoding.UTF8.GetBytes("testkey");
            string storageAccountKey = Convert.ToBase64String(bytes);
            XmlDocument wadCfg = new XmlDocument();
            wadCfg.LoadXml(daCfgContent);

            VMDiagnosticsExtensionBuilder builder = new VMDiagnosticsExtensionBuilder(
                storageAccountName,
                storageAccountKey,
                null,
                wadCfg);

            Assert.IsTrue(builder.Enabled);
            Assert.IsTrue(string.Equals(builder.StorageAccountName, storageAccountName));
            Assert.IsTrue(string.Equals(builder.StorageAccountKey, storageAccountKey));

            CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(new StorageCredentials(builder.StorageAccountName, builder.StorageAccountKey), null, null, null);
            Assert.IsTrue(string.Equals(builder.DiagnosticsConfiguration.OuterXml, wadCfg.OuterXml));
            Assert.IsNull(builder.Endpoints);

            var reference = builder.GetResourceReference();
            Assert.IsNotNull(reference != null);
            Assert.IsTrue(reference.Version == VMDiagnosticsExtensionBuilder.CurrentExtensionVersion);
            Assert.IsTrue(reference.Publisher == VMDiagnosticsExtensionBuilder.ExtensionDefaultPublisher);
            Assert.IsTrue(reference.Name == VMDiagnosticsExtensionBuilder.ExtensionDefaultName);
            Assert.IsTrue(reference.ReferenceName == VMDiagnosticsExtensionBuilder.ExtensionDefaultReferenceName);

            Assert.IsNotNull(reference.ResourceExtensionParameterValues);
            Assert.IsTrue(1 == reference.ResourceExtensionParameterValues.FindAll(r => r.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr).Count);

            var item = reference.ResourceExtensionParameterValues.Find(r => r.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr);
            var key = item.Key;
            Assert.IsFalse(string.IsNullOrEmpty(key));
            Assert.IsTrue(key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr);

            var val = item.Value;
            Assert.IsFalse(string.IsNullOrEmpty(val));
            XDocument document = XDocument.Parse(val);

            var daConfig = VMDiagnosticsExtensionBuilder.GetDiagnosticsAgentConfig(builder.Enabled, builder.StorageAccountName, builder.StorageAccountKey, null, builder.DiagnosticsConfiguration);
            Assert.IsTrue(document.ToString() == daConfig);

            var builder2 = new VMDiagnosticsExtensionBuilder(document.ToString());
            var reference2 = builder2.GetResourceReference();
            Assert.IsTrue(1 == reference2.ResourceExtensionParameterValues.FindAll(r => r.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr).Count);
            XDocument document2 = XDocument.Parse(reference2.ResourceExtensionParameterValues.Find(r => r.Key == VMDiagnosticsExtensionBuilder.ExtensionReferenceKeyStr).Value);
            Assert.IsTrue(document.ToString() == document2.ToString());

            return;
        }
    }
}
