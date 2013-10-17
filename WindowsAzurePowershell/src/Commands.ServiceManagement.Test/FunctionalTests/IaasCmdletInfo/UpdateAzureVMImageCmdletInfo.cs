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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{
    using System;
    using ConfigDataInfo;
    using PowershellCore;

    public class UpdateAzureVMImageCmdletInfo : CmdletsInfo
    {
        public UpdateAzureVMImageCmdletInfo(string imageName, string label)
        {
            cmdletName = Utilities.UpdateAzureVMImageCmdletName;

            cmdletParams.Add(new CmdletParam("ImageName", imageName));
            cmdletParams.Add(new CmdletParam("Label", label));            
        }

        public UpdateAzureVMImageCmdletInfo(string imageName, string label, InstanceSize? recommendedSize)
            : this(imageName, label)
        {
            if (recommendedSize.HasValue)
            {
                cmdletParams.Add(new CmdletParam("RecommendedVMSize", recommendedSize));
            }
        }

        public UpdateAzureVMImageCmdletInfo(
            string imageName,
            string label,
            InstanceSize? recommendedSize,
            string description,
            string eula,
            string imageFamily,
            Uri privacyUri,
            DateTime publishedDate)
            : this(imageName, label, recommendedSize)
        {
            if (!string.IsNullOrEmpty(description))
            {
                cmdletParams.Add(new CmdletParam("Description", description));
            }
            if (!string.IsNullOrEmpty(eula))
            {
                cmdletParams.Add(new CmdletParam("Eula", eula));
            }
            if (!string.IsNullOrEmpty(imageFamily))
            {
                cmdletParams.Add(new CmdletParam("ImageFamily", imageFamily));
            }
            if (privacyUri != null)
            {
                cmdletParams.Add(new CmdletParam("PrivacyUri", privacyUri.ToString()));
            }
            if (publishedDate != null)
            {
                cmdletParams.Add(new CmdletParam("PublishedDate", publishedDate.ToString()));
            }
        }
    }
}
