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

namespace Microsoft.WindowsAzure.Commands.Service.Gateway
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "GatewayEvent", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class GatewayEvent
    {
        // Properties
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Data { get;  set; }

        [DataMember]
        public int Id { get;  set; }

        [DataMember]
        public string Message { get;  set; }

        [DataMember]
        public DateTime Timestamp { get;  set; }
    }
}