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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests
{
    using Commands.Utilities.Common;
    using Model;
    using Properties;
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ServiceManagementTest
    {
        protected const string serviceNamePrefix = "PSTestService";
        protected const string vmNamePrefix = "PSTestVM";
        protected const string password = "p@ssw0rd";
        protected const string username = "pstestuser";
        protected static string localFile = Resource.Vhd;
        protected static string vnetConfigFilePath = Directory.GetCurrentDirectory() + "\\vnetconfig.netcfg";
        protected const string testDataContainer = "testdata";
        protected const string osVhdName = "oneGBFixedWS2008R2.vhd";

        // Test cleanup settings
        protected const bool deleteDefaultStorageAccount = false; // Temporarily set to false
        protected bool cleanupIfPassed = true;
        protected bool cleanupIfFailed = true;
        protected const string vhdContainerName = "vhdstore";

        protected static ServiceManagementCmdletTestHelper vmPowershellCmdlets;
        protected static WindowsAzureSubscription defaultAzureSubscription;
        protected static StorageServiceKeyOperationContext storageAccountKey;
        protected static string blobUrlRoot;

        protected static string locationName;
        protected static string imageName;

        protected bool pass;
        protected string testName;
        protected DateTime testStartTime;

        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            SetTestSettings();
        }

        public static void SetDefaultStorage()
        {
            if (!string.IsNullOrEmpty(GetDefaultStorage(CredentialHelper.DefaultStorageName, CredentialHelper.Location)))
            {
                defaultAzureSubscription = vmPowershellCmdlets.SetAzureSubscription(defaultAzureSubscription.SubscriptionName, CredentialHelper.DefaultStorageName);

                storageAccountKey = vmPowershellCmdlets.GetAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccountName);
                Assert.AreEqual(defaultAzureSubscription.CurrentStorageAccountName, storageAccountKey.StorageAccountName);
                blobUrlRoot = (vmPowershellCmdlets.GetAzureStorageAccount(defaultAzureSubscription.CurrentStorageAccountName)[0].Endpoints.ToArray())[0];
            }
            else
            {
                Console.WriteLine("Unable to get the default storege account");
            }
        }

        private static string GetDefaultStorage(string storageName, string locName)
        {
            Collection<StorageServicePropertiesOperationContext> storageAccounts = vmPowershellCmdlets.GetAzureStorageAccount(null);
            foreach (var storageAccount in storageAccounts)
            {
                if (storageAccount.StorageAccountName == storageName)
                {
                    return storageAccount.StorageAccountName;
                }
            }

            var account = vmPowershellCmdlets.NewAzureStorageAccount(storageName, locName);
            if (account.StorageAccountName == storageName)
            {
                return account.StorageAccountName;
            }

            return null;
        }

        public static void SetTestSettings()
        {
            vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            CredentialHelper.GetTestSettings(Resource.TestSettings);

            vmPowershellCmdlets.RemoveAzureSubscriptions();
            vmPowershellCmdlets.ImportAzurePublishSettingsFile(CredentialHelper.PublishSettingsFile);

            if (string.IsNullOrEmpty(CredentialHelper.DefaultSubscriptionName))
            {
                defaultAzureSubscription = vmPowershellCmdlets.GetCurrentAzureSubscription();
                if (string.IsNullOrEmpty(Resource.DefaultSubscriptionName))
                {
                    CredentialHelper.DefaultSubscriptionName = defaultAzureSubscription.SubscriptionName;
                }
            }
            else
            {
                defaultAzureSubscription = vmPowershellCmdlets.SetDefaultAzureSubscription(CredentialHelper.DefaultSubscriptionName);
            }

            locationName = vmPowershellCmdlets.GetAzureLocationName(new[] { CredentialHelper.Location }); // Get-AzureLocation
            if (String.IsNullOrEmpty(locationName))
            {
                Console.WriteLine("No location is selected!");
            }
            Console.WriteLine("Location Name: {0}", locationName);

            if (defaultAzureSubscription.CurrentStorageAccountName == null && !string.IsNullOrEmpty(CredentialHelper.DefaultStorageName))
            {
                SetDefaultStorage();
            }

            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false); // Get-AzureVMImage
            if (String.IsNullOrEmpty(imageName))
            {
                Console.WriteLine("No image is selected!");
            }
            else
            {
                Console.WriteLine("Image Name: {0}", imageName);
            }
        }

        protected void StartTest(string testname, DateTime testStartTime)
        {
            Console.WriteLine("{0} test starts at {1}", testname, testStartTime);
        }

        [AssemblyCleanup]
        public static void CleanUpAssembly()
        {
            if (defaultAzureSubscription != null)
            {
                Retry(String.Format("Get-AzureDisk | Where {{$_.DiskName.Contains(\"{0}\")}} | Remove-AzureDisk -DeleteVhd", serviceNamePrefix), "in use");
                if (deleteDefaultStorageAccount)
                {
                    //vmPowershellCmdlets.RemoveAzureStorageAccount(defaultAzureSubscription.CurrentStorageAccountName);
                }
            }
        }

        private static void Retry(string cmdlet, string message, int maxTry = 1, int intervalSecond = 10)
        {

            ServiceManagementCmdletTestHelper pscmdlet = new ServiceManagementCmdletTestHelper();

            for (int i = 0; i < maxTry; i++)
            {
                try
                {
                    pscmdlet.RunPSScript(cmdlet);
                    break;
                }
                catch (Exception e)
                {
                    if (i == maxTry)
                    {
                        Console.WriteLine("Max try reached.  Couldn't perform within the given time.");
                    }
                    if (e.ToString().Contains(message))
                    {
                        //Thread.Sleep(intervalSecond * 1000);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        protected static void ReImportSubscription()
        {
            // Re-import the subscription.
            vmPowershellCmdlets.ImportAzurePublishSettingsFile();
            vmPowershellCmdlets.SetDefaultAzureSubscription(CredentialHelper.DefaultSubscriptionName);
            vmPowershellCmdlets.SetAzureSubscription(defaultAzureSubscription.SubscriptionName, defaultAzureSubscription.CurrentStorageAccountName);
        }
    }
}
