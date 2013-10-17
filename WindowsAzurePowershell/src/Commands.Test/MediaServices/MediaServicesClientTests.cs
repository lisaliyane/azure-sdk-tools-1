using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.MediaServices;
using Microsoft.WindowsAzure.Management.MediaServices;
using Microsoft.WindowsAzure.Management.MediaServices.Models;
using Microsoft.WindowsAzure.Management.Storage;
using Moq;

namespace Microsoft.WindowsAzure.Commands.Test.MediaServices
{
    [TestClass]
    public class MediaServicesClientTests
    {
        private const string AccountName = "testacc";
        private const string SubscriptionId = "foo";
        private static readonly StorageManagementClient StorageClient = new StorageManagementClient(new CertificateCloudCredentials(SubscriptionId, new X509Certificate2(new byte[] { })), new Uri("http://someValue"));


        [TestMethod]
        public void TestDeleteAzureMediaServiceAccountAsync()
        {
            Mock<MediaServicesManagementClient> clientMock = InitMediaManagementClientMock();
            Mock<IAccountOperations> iAccountOperations = new Mock<IAccountOperations>();
            iAccountOperations.Setup(m => m.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(() => Task.Factory.StartNew(() => new OperationResponse
            {
                RequestId = "request",
                StatusCode = HttpStatusCode.OK
            }));
            clientMock.Setup(m => m.Accounts).Returns(() => iAccountOperations.Object);

            StorageManagementClient storageClient = StorageClient.WithHandler(new FakeHttpMessageHandler());
            MediaServicesClient target = new MediaServicesClient(null,
                clientMock.Object,
                storageClient);

            OperationResponse result = target.DeleteAzureMediaServiceAccountAsync(AccountName).Result;

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [TestMethod]
        public void TestDeleteAzureMediaServiceAccountAsync404()
        {
            FakeHttpMessageHandler fakeHttpHandler;
            MediaServicesManagementClient clientWithHandler = CreateMediaManagementClientWithFakeHttpMessageHandler(out fakeHttpHandler);

            const string responseText = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">{\"Code\":\"NotFound\",\"Message\":\"The specified account was not found.\"}</string>";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new FakeHttpContent(responseText)
            };

            fakeHttpHandler.Send = request => response;

            MediaServicesClient target = new MediaServicesClient(null,
                clientWithHandler,
                StorageClient.WithHandler(new FakeHttpMessageHandler()));

            try
            {
                OperationResponse result = target.DeleteAzureMediaServiceAccountAsync(AccountName).Result;
            }
            catch (AggregateException ax)
            {
                CloudException x = (CloudException)ax.InnerExceptions.Single();
                Assert.AreEqual(HttpStatusCode.NotFound, x.Response.StatusCode);
                return;
            }

            Assert.Fail("ServiceManagementClientException expected");
        }

        [TestMethod]
        public void TestRegenerateMediaServicesAccountAsync()
        {
            Mock<MediaServicesManagementClient> clientMock = InitMediaManagementClientMock();
            Mock<IAccountOperations> iAccountOperations = new Mock<IAccountOperations>();
            iAccountOperations
                .Setup(m => m.RegenerateKeyAsync(It.IsAny<string>(), It.IsAny<MediaServicesKeyType>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.Factory.StartNew(
                    () => new OperationResponse
                    {
                        RequestId = "request",
                        StatusCode = HttpStatusCode.OK
                    }));

            clientMock.Setup(m => m.Accounts).Returns(() => iAccountOperations.Object);

            MediaServicesClient target = new MediaServicesClient(null,
                clientMock.Object,
                StorageClient);

            OperationResponse result = target.RegenerateMediaServicesAccountAsync(AccountName, MediaServicesKeyType.Primary).Result;

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }
        
        [TestMethod]
        public void TestGetMediaServiceAsync()
        {


            FakeHttpMessageHandler fakeHttpHandler;
            MediaServicesManagementClient clientWithHandler = CreateMediaManagementClientWithFakeHttpMessageHandler(out fakeHttpHandler);

            const string responseText = @"<AccountDetails xmlns='http://schemas.datacontract.org/2004/07/Microsoft.Cloud.Media.Management.ResourceProvider.Models' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
                                    <AccountKey>primarykey</AccountKey><AccountKeys>
                                    <Primary>primarykey</Primary>
                                    <Secondary>secondarykey</Secondary>
                                    </AccountKeys><AccountName>testps</AccountName>
                                    <AccountRegion>West US</AccountRegion>
                                    <StorageAccountName>psstorage</StorageAccountName>
                                </AccountDetails>";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new FakeHttpContent(responseText)
            };

            fakeHttpHandler.Send = request => response;

            MediaServicesClient target = new MediaServicesClient(null,
                clientWithHandler,
                StorageClient.WithHandler(new FakeHttpMessageHandler()));

            MediaServicesAccountGetResponse result = target.GetMediaServiceAsync(AccountName).Result;
            Assert.AreEqual("primarykey", result.StorageAccountKeys.Primary);
            Assert.AreEqual("secondarykey", result.StorageAccountKeys.Secondary);
            Assert.AreEqual("testps", result.AccountName);
            Assert.AreEqual("psstorage", result.StorageAccountName);
        }

        [TestMethod]
        public void TestGetMediaServiceAccountsAsync()
        {

            FakeHttpMessageHandler fakeHttpHandler;
            MediaServicesManagementClient clientWithHandler = CreateMediaManagementClientWithFakeHttpMessageHandler(out fakeHttpHandler);

            const string responseText = @"<ServiceResources xmlns='http://schemas.microsoft.com/windowsazure' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
                                    <ServiceResource>
                                        <Name>mymediademo</Name>
                                        <Type>MediaService</Type>
                                        <State>Active</State>
                                        <AccountId>E0658294-5C96-4B0F-AD55-F7446CE4F788</AccountId>
                                    </ServiceResource>
                                    <ServiceResource>
                                        <Name>nimbusorigintrial</Name>
                                        <Type>MediaService</Type>
                                        <State>Active</State>
                                        <AccountId>C92B17C8-5422-4CD1-8D3C-61E576E861DD</AccountId>
                                    </ServiceResource>
                                </ServiceResources>";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new FakeHttpContent(responseText)
            };

            fakeHttpHandler.Send = request => response;

            MediaServicesClient target = new MediaServicesClient(null,
                clientWithHandler,
                StorageClient.WithHandler(new FakeHttpMessageHandler()));

            MediaServicesAccountListResponse.MediaServiceAccount[] result = target.GetMediaServiceAccountsAsync().Result.Accounts.ToArray();
            Assert.AreEqual("E0658294-5C96-4B0F-AD55-F7446CE4F788", result[0].AccountId);
            Assert.AreEqual("C92B17C8-5422-4CD1-8D3C-61E576E861DD", result[1].AccountId);
        }

        [TestMethod]
        public void TestCreateNewAzureMediaServiceAsync()
        {
            FakeHttpMessageHandler fakeHttpHandler;
            MediaServicesManagementClient clientWithHandler = CreateMediaManagementClientWithFakeHttpMessageHandler(out fakeHttpHandler);

            const string responseText = @"<AccountCreationResult xmlns='http://schemas.datacontract.org/2004/07/Microsoft.Cloud.Media.Management.ResourceProvider.Models' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
                                    <AccountId>e26ca098-e363-450d-877c-384ce5a97c72</AccountId>
                                    <AccountName>tmp</AccountName>
                                    <StatusCode>Created</StatusCode>
                                    <Subscription>d4e66bc8-6ccb-4e49-9ee6-dc6925d5bbdb</Subscription>
                                </AccountCreationResult>";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new FakeHttpContent(responseText)
            };

            fakeHttpHandler.Send = request => response;

            MediaServicesClient target = new MediaServicesClient(null,
                clientWithHandler,
                StorageClient);

            MediaServicesAccountCreateParameters creationRequest = new MediaServicesAccountCreateParameters
            {
                AccountName = AccountName,
                BlobStorageEndpointUri = new Uri("http://tmp"),
                Region = "West US",
                StorageAccountKey = Guid.NewGuid().ToString(),
                StorageAccountName = "test"
            };

            MediaServicesAccountCreateResponse result = target.CreateNewAzureMediaServiceAsync(creationRequest).Result;
            Assert.AreEqual("tmp", result.AccountName);
        }

        [TestMethod]
        public void TestCreateNewAzureMediaServiceAsyncInvalidAccount()
        {
            FakeHttpMessageHandler fakeHttpHandler;
            MediaServicesManagementClient clientWithHandler = CreateMediaManagementClientWithFakeHttpMessageHandler(out fakeHttpHandler);

            const string responseText = @"<Error xmlns='http://schemas.microsoft.com/windowsazure' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
                                        <Code>BadRequest</Code>
                                        <Message>Account Creation Request contains an invalid account name.</Message>
                                    </Error>";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new FakeHttpContent(responseText)
            };

            fakeHttpHandler.Send = request => response;

            MediaServicesClient target = new MediaServicesClient(null,
                clientWithHandler,
                StorageClient);

            MediaServicesAccountCreateParameters creationRequest = new MediaServicesAccountCreateParameters
            {
                AccountName = AccountName,
                BlobStorageEndpointUri = new Uri("http://tmp"),
                Region = "West US",
                StorageAccountKey = Guid.NewGuid().ToString(),
                StorageAccountName = "test"
            };

            try
            {
                MediaServicesAccountCreateResponse result = target.CreateNewAzureMediaServiceAsync(creationRequest).Result;
            }
            catch (AggregateException ex)
            {
                CloudException cloudException = ex.Flatten().InnerException as CloudException;
                Assert.IsNotNull(cloudException);
                Assert.AreEqual(HttpStatusCode.BadRequest, cloudException.Response.StatusCode);
            }

        }

        #region Helper  Methods

        private static WindowsAzureSubscription GetWindowsAzureSubscription()
        {
            WindowsAzureSubscription windowsAzureSubscription = new WindowsAzureSubscription
            {
                SubscriptionId = SubscriptionId,
                Certificate = new X509Certificate2(new byte[] {}),
                ServiceEndpoint = new Uri("http://someValue")
            };
            return windowsAzureSubscription;
        }
        private static Mock<MediaServicesManagementClient> InitMediaManagementClientMock()
        {
            return new Mock<MediaServicesManagementClient>(new CertificateCloudCredentials(SubscriptionId, new X509Certificate2(new byte[] { })), new Uri("http://someValue"));
        }
        private static MediaServicesManagementClient CreateMediaManagementClientWithFakeHttpMessageHandler(out FakeHttpMessageHandler fakeHttpHandler)
        {
            fakeHttpHandler = new FakeHttpMessageHandler();
            MediaServicesManagementClient managementClient = InitManagementClient();
            MediaServicesManagementClient clientWithHandler = managementClient.WithHandler(fakeHttpHandler);
            return clientWithHandler;
        }
        private static MediaServicesManagementClient InitManagementClient()
        {
            return new MediaServicesManagementClient(new CertificateCloudCredentials(SubscriptionId, new X509Certificate2(new byte[] { })), new Uri("http://someValue"));
        }

        #endregion

    }
}
