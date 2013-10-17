// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services.ImportExport
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the result of querying the status of an import or export database operation
    /// </summary>
    [SerializableAttribute]
    [DataContractAttribute(Name = "StatusInfo",
        Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.SqlServer.Management.Dac.ServiceTypes")]
    public class StatusInfo : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the blob uri
        /// </summary>
        [DataMemberAttribute]
        public string BlobUri { get; set; }

        /// <summary>
        /// Gets or sets the name of the database
        /// </summary>
        [DataMemberAttribute]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the error message if any
        /// </summary>
        [DataMemberAttribute]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets date the database was last modified
        /// </summary>
        [DataMemberAttribute]
        public DateTime LastModifiedTime { get; set; }

        /// <summary>
        /// Gets or sets how long the operation has been queued
        /// </summary>
        [DataMemberAttribute]
        public DateTime QueuedTime { get; set; }

        /// <summary>
        /// Gets or sets the import/export request id
        /// </summary>
        [DataMemberAttribute]
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the type of the request
        /// </summary>
        [DataMemberAttribute]
        public string RequestType { get; set; }

        /// <summary>
        /// Gets or sets the name of the server the database resides in
        /// </summary>
        [DataMemberAttribute]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the status of the import/export operation
        /// </summary>
        [DataMemberAttribute]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the import/export status info extension data
        /// </summary>
        [BrowsableAttribute(false)]
        public ExtensionDataObject ExtensionData { get; set; }
    }
}
