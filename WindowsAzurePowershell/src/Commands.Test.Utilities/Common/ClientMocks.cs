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

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.Common
{
    using Management;
    using Management.Compute;
    using Management.Storage;
    using Moq;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// Test helper class for managing mocking the various client objects
    /// and operations in the service management library.
    /// </summary>
    public class ClientMocks
    {
        private readonly MockRepository repository;
        public Mock<ManagementClient> ManagementClientMock { get; private set; }
        public Mock<StorageManagementClient> StorageManagementClientMock { get; private set; }
        public Mock<ComputeManagementClient> ComputeManagementClientMock { get; private set; }

        public ClientMocks(string subscriptionId)
        {
            repository = new MockRepository(MockBehavior.Default) {DefaultValue = DefaultValue.Mock};

            var creds = CreateCredentials(subscriptionId);
            ManagementClientMock = repository.Create<ManagementClient>(creds);
            ComputeManagementClientMock = repository.Create<ComputeManagementClient>(creds);
            StorageManagementClientMock = repository.Create<StorageManagementClient>(creds);
        }

        public void Verify()
        {
            repository.Verify();
        }

        public void VerifyAll()
        {
            repository.VerifyAll();
        }

        private SubscriptionCloudCredentials CreateCredentials(string subscriptionId)
        {
            var mockCreds = repository.Create<SubscriptionCloudCredentials>(MockBehavior.Loose);
            mockCreds.SetupGet(c => c.SubscriptionId).Returns(subscriptionId);
            return mockCreds.Object;
        }

        internal static CloudException Make404Exception()
        {
            return CloudException.CreateFromXml(
                new HttpRequestMessage(),
                null,
                new HttpResponseMessage(HttpStatusCode.NotFound),
                "<Error><Message>Not found</Message></Error>");
        }
    }
}
