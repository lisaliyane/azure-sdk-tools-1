﻿﻿// ----------------------------------------------------------------------------------
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
namespace Microsoft.WindowsAzure.Commands.Storage.File.Cmdlet
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.File;

    [Cmdlet(VerbsCommon.Set, Constants.FileContentCmdletName, DefaultParameterSetName = Constants.ShareNameParameterSetName)]
    public class SetAzureStorageFileContent : AzureStorageFileCmdletBase
    {
        [Parameter(
           Position = 0,
           Mandatory = true,
           ParameterSetName = Constants.ShareNameParameterSetName,
           HelpMessage = "Name of the file share where the file would be uploaded to.")]
        [ValidateNotNullOrEmpty]
        public string ShareName { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ParameterSetName = Constants.ShareParameterSetName,
            HelpMessage = "CloudFileShare object indicated the share where the file would be uploaded to.")]
        [ValidateNotNull]
        public CloudFileShare Share { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ParameterSetName = Constants.DirectoryParameterSetName,
            HelpMessage = "CloudFileDirectory object indicated the cloud directory where the file would be uploaded.")]
        [ValidateNotNull]
        public CloudFileDirectory Directory { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            HelpMessage = "Path to the local file to be uploaded.")]
        [ValidateNotNullOrEmpty]
        public string Source { get; set; }

        [Parameter(
            Position = 2,
            HelpMessage = "Path to the cloud file which would be uploaded to.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        [Parameter(HelpMessage = "Overwrite existing file. By default this cmdlet would throw an error if there's already an existing file with the same name at destination.")]
        public SwitchParameter Force { get; set; }

        [Parameter(HelpMessage = "Returns an object representing the downloaded cloud file. By default, this cmdlet does not generate any output.")]
        public SwitchParameter PassThru { get; set; }

        public override void ExecuteCmdlet()
        {
            // Step 1: Validate source file.
            FileInfo localFile = new FileInfo(this.GetUnresolvedProviderPathFromPSPath(this.Source));
            if (!localFile.Exists)
            {
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Resources.SourceFileNotFound, this.Source));
            }

            // Step 2: Build the CloudFile object which pointed to the
            // destination cloud file.
            this.RunTask(async taskId =>
            {
                bool isDirectory;
                string[] path = NamingUtil.ValidatePath(this.Path, out isDirectory);
                var cloudFileToBeUploaded = await this.BuildCloudFileInstanceFromPathAsync(localFile.Name, path, isDirectory);

                if (!this.Force && await this.Channel.FileExistsAsync(cloudFileToBeUploaded, this.RequestOptions, this.OperationContext, this.CmdletCancellationToken))
                {
                    throw new AzureStorageFileException(
                        ErrorCategory.InvalidArgument,
                        ErrorIdConstants.ResourceAlreadyExists,
                        string.Format(CultureInfo.CurrentCulture, Resources.CloudFileConflict, cloudFileToBeUploaded.Name),
                        this);
                }

                // TODO: Use DMLib to upload file.

                // Step 3: Creates the file before upload it. Notice that create operaiton
                // would replace a file if it already exists.
                await cloudFileToBeUploaded.CreateAsync(localFile.Length, this.AccessCondition, this.RequestOptions, this.OperationContext, this.CmdletCancellationToken);

                // Step 4: Upload the content of the source file.
                await cloudFileToBeUploaded.UploadFromFileAsync(
                    localFile.FullName,
                    FileMode.Open,
                    this.AccessCondition,
                    this.RequestOptions,
                    this.OperationContext,
                    this.CmdletCancellationToken);

                if (this.PassThru)
                {
                    this.OutputStream.WriteObject(taskId, cloudFileToBeUploaded);
                }
            });
        }

        private async Task<CloudFile> BuildCloudFileInstanceFromPathAsync(string defaultFileName, string[] path, bool pathIsDirectory)
        {
            CloudFileDirectory baseDirectory = null;
            bool isPathEmpty = path.Length == 0;
            switch (this.ParameterSetName)
            {
                case Constants.DirectoryParameterSetName:
                    baseDirectory = this.Directory;
                    break;

                case Constants.ShareNameParameterSetName:
                    NamingUtil.ValidateShareName(this.ShareName, false);
                    baseDirectory = this.BuildFileShareObjectFromName(this.ShareName).GetRootDirectoryReference();
                    break;

                case Constants.ShareParameterSetName:
                    baseDirectory = this.Share.GetRootDirectoryReference();
                    break;

                default:
                    throw new PSArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid parameter set name: {0}", this.ParameterSetName));
            }

            if (isPathEmpty)
            {
                return baseDirectory.GetFileReference(defaultFileName);
            }

            var directory = baseDirectory.GetDirectoryReferenceByPath(path);
            if (pathIsDirectory)
            {
                return directory.GetFileReference(defaultFileName);
            }

            bool directoryExists;

            try
            {
                directoryExists = await this.Channel.DirectoryExistsAsync(directory, this.RequestOptions, this.OperationContext, this.CmdletCancellationToken);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation != null &&
                    e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.BadRequest &&
                    e.RequestInformation.ExtendedErrorInformation == null)
                {
                    throw new AzureStorageFileException(ErrorCategory.InvalidArgument, ErrorIdConstants.InvalidResource, Resources.InvalidResource, this);
                }

                throw;
            }

            if (directoryExists)
            {
                // If the directory exist on the cloud, we treat the path as
                // to a directory. So we append the default file name after
                // it and build an instance of CloudFile class.
                return directory.GetFileReference(defaultFileName);
            }
            else
            {
                // If the directory does not exist, we treat the path as to a
                // file. So we use the path of the directory to build out a
                // new instance of CloudFile class.
                return baseDirectory.GetFileReferenceByPath(path);
            }
        }
    }
}
