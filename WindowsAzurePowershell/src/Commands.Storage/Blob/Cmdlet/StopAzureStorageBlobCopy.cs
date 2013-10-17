﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.Storage.Blob.Cmdlet
{
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Model.ResourceModel;

    [Cmdlet(VerbsLifecycle.Stop, StorageNouns.CopyBlob, ConfirmImpact = ConfirmImpact.High, DefaultParameterSetName = NameParameterSet),
       OutputType(typeof(AzureStorageBlob))]
    public class StopAzureStorageBlobCopy : StorageCloudBlobCmdletBase
    {
        /// <summary>
        /// Blob Pipeline parameter set name
        /// </summary>
        private const string BlobPipelineParameterSet = "BlobPipeline";

        /// <summary>
        /// Container pipeline paremeter set name
        /// </summary>
        private const string ContainerPipelineParmeterSet = "ContainerPipeline";

        /// <summary>
        /// Blob name and container name parameter set
        /// </summary>
        private const string NameParameterSet = "NamePipeline";

        [Parameter(HelpMessage = "ICloudBlob Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = BlobPipelineParameterSet)]
        public ICloudBlob ICloudBlob { get; set; }

        [Parameter(HelpMessage = "CloudBlobContainer Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = ContainerPipelineParmeterSet)]
        public CloudBlobContainer CloudBlobContainer { get; set; }

        [Parameter(ParameterSetName = ContainerPipelineParmeterSet, Mandatory = true, Position = 0, HelpMessage = "Blob name")]
        [Parameter(ParameterSetName = NameParameterSet, Mandatory = true, Position = 0, HelpMessage = "Blob name")]
        public string Blob
        {
            get { return BlobName; }
            set { BlobName = value; }
        }
        private string BlobName = String.Empty;

        [Parameter(HelpMessage = "Container name", Mandatory = true, Position = 1,
            ParameterSetName = NameParameterSet)]
        [ValidateNotNullOrEmpty]
        public string Container
        {
            get { return ContainerName; }
            set { ContainerName = value; }
        }
        private string ContainerName = String.Empty;

        [Parameter(HelpMessage = "Force to stop the current copy task on the specified blob")]
        public SwitchParameter Force
        {
            get { return force; }
            set { force = value; }
        }
        private bool force = false;

        [Parameter(HelpMessage = "Copy Id", Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string CopyId
        {
            get { return copyId; }
            set { copyId = value; }
        }
        private string copyId;

        /// <summary>
        /// Execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            string blobName = string.Empty;
            string containerName = string.Empty;
            bool stopped = false;

            switch (ParameterSetName)
            { 
                case NameParameterSet:
                    stopped = StopCopyBlob(ContainerName, BlobName, copyId);
                    blobName = BlobName;
                    containerName = ContainerName;
                    break;
                case ContainerPipelineParmeterSet:
                    stopped = StopCopyBlob(CloudBlobContainer, BlobName, copyId);
                    blobName = BlobName;
                    containerName = CloudBlobContainer.Name;
                    break;
                case BlobPipelineParameterSet:
                    stopped = StopCopyBlob(ICloudBlob, copyId, true);
                    blobName = ICloudBlob.Name;
                    containerName = ICloudBlob.Container.Name;
                    break;
            }

            if (stopped)
            {
                string message = String.Format(Resources.StopCopyBlobSuccessfully, blobName, containerName);
                WriteObject(message);
            }
            else
            {
                string cancelMessage = String.Format(Resources.StopCopyOperationCancelled, blobName, containerName);
                WriteVerboseWithTimestamp(cancelMessage);
            }
        }

        /// <summary>
        /// Stop copy operation by name
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="copyId">copy id</param>
        private bool StopCopyBlob(string containerName, string blobName, string copyId)
        {            
            CloudBlobContainer container = Channel.GetContainerReference(containerName);
            return StopCopyBlob(container, blobName, copyId);
        }

        /// <summary>
        /// Stop copy operation by CloudBlobContainer
        /// </summary>
        /// <param name="container">CloudBlobContainer object</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="copyId">Copy id</param>
        private bool StopCopyBlob(CloudBlobContainer container, string blobName, string copyId)
        {
            ValidateBlobName(blobName);

            ValidateContainerName(container.Name);

            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;
            ICloudBlob blob = Channel.GetBlobReferenceFromServer(container, blobName, accessCondition, options, OperationContext);

            if (blob == null)
            {
                throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, blobName, container.Name));
            }

            return StopCopyBlob(blob, copyId);
        }

        /// <summary>
        /// confirm to abort copy operation
        /// </summary>
        /// <param name="msg">Confirmation message</param>
        /// <returns>True if the opeation is confirmed, otherwise return false</returns>
        internal virtual bool ConfirmAbort(string msg)
        {
            return ShouldProcess(msg);
        }

        /// <summary>
        /// Stop copy operation by ICloudBlob object
        /// </summary>
        /// <param name="blob">ICloudBlob object</param>
        /// <param name="copyId">Copy id</param>
        private bool StopCopyBlob(ICloudBlob blob, string copyId, bool fetchCopyIdFromBlob = false)
        {
            AccessCondition accessCondition = null;
            BlobRequestOptions abortRequestOption = new BlobRequestOptions();

            //Set no retry to resolve the 409 conflict exception
            abortRequestOption.RetryPolicy = new NoRetry();

            if (null == blob)
            {
                throw new ArgumentException(String.Format(Resources.ObjectCannotBeNull, typeof(ICloudBlob).Name));
            }

            string specifiedCopyId = copyId;
            
            if (string.IsNullOrEmpty(specifiedCopyId) && fetchCopyIdFromBlob)
            {
                if (blob.CopyState != null)
                {
                    specifiedCopyId = blob.CopyState.CopyId;
                }
            }

            string abortCopyId = string.Empty;

            if (string.IsNullOrEmpty(specifiedCopyId) || Force)
            {
                //Make sure we use the correct copy id to abort
                //Use default retry policy for FetchBlobAttributes
                BlobRequestOptions options = null;
                Channel.FetchBlobAttributes(blob, accessCondition, options, OperationContext);

                if (blob.CopyState == null || String.IsNullOrEmpty(blob.CopyState.CopyId))
                {
                    throw new ArgumentException(String.Format(Resources.CopyTaskNotFound, blob.Name, blob.Container.Name));
                }
                else
                {
                    abortCopyId = blob.CopyState.CopyId;
                }

                if (!Force)
                {
                    string confirmation = String.Format(Resources.ConfirmAbortCopyOperation, blob.Name, blob.Container.Name, abortCopyId);
                    if (!ConfirmAbort(confirmation))
                    {
                        return false;
                    }
                }
            }
            else
            {
                abortCopyId = specifiedCopyId;
            }

            Channel.AbortCopy(blob, abortCopyId, accessCondition, abortRequestOption, OperationContext);
            return true;
        }
    }
}
