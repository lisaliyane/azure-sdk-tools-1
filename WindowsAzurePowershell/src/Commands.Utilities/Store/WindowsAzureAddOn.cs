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

namespace Microsoft.WindowsAzure.Commands.Utilities.Store
{
    using ResourceModel;

    public class WindowsAzureAddOn
    {
        public const string DataSetType = "DataMarket";

        public const string DataType = "Data";

        public const string AppServiceType = "App Service";

        public string Type { get; set; }

        public string AddOn { get; set; }

        public string Name { get; set; }

        public string Plan { get; set; }

        public string SchemaVersion { get; set; }

        public string ETag { get; set; }

        public string State { get; set; }

        public UsageMeterList UsageMeters { get; set; }

        public OutputItemList OutputItems { get; set; }

        public OperationStatus LastOperationStatus { get; set; }

        public string Location { get; set; }

        public string CloudService { get; set; }

        /// <summary>
        /// Creates new instance from AddOn
        /// </summary>
        /// <param name="resource">The add on details</param>
        /// <param name="geoRegion">The add on region</param>
        public WindowsAzureAddOn(Resource resource, string geoRegion, string cloudService)
        {
            Type = resource.ResourceProviderNamespace == DataSetType ? DataType : AppServiceType;
            
            AddOn = resource.Type;
            
            Name = resource.Name;
            
            Plan = resource.Plan;
            
            SchemaVersion = resource.SchemaVersion;
            
            ETag = resource.ETag;
            
            State = resource.State;

            UsageMeters = resource.UsageMeters;

            OutputItems = (Type == AppServiceType) ? resource.OutputItems : new OutputItemList();

            LastOperationStatus = resource.OperationStatus;

            Location = geoRegion;

            CloudService = cloudService;
        }
    }
}
