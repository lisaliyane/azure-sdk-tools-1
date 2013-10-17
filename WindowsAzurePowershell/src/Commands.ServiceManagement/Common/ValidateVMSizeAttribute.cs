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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Properties;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ValidateVMSizeAttribute : ValidateEnumeratedArgumentsAttribute
    {
        private IList<string> validValues = new List<string>();
        private string commaSeparatedVMSizes;

        public ValidateVMSizeAttribute() : this(true)
        {
        }

        public ValidateVMSizeAttribute(bool includeExtraSmall)
        {
            this.validValues = new List<string>
            {
                "Small",
                "Medium",
                "Large",
                "ExtraLarge",
                "A6",
                "A7"
            };
            if (includeExtraSmall)
            {
                this.validValues.Insert(0, "ExtraSmall");
            }
            commaSeparatedVMSizes = validValues.Aggregate((c, n) => c + ", " + n);
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ValidationMetadataException("ArgumentIsEmpty", null);
            }
            var stringElement = element.ToString();
            if(validValues.FirstOrDefault(s => string.Compare(s, stringElement, true, CultureInfo.InvariantCulture) == 0)  == null)
            {
                var message = string.Format(Resources.InvalidVMSize, stringElement, commaSeparatedVMSizes);
                throw new ValidationMetadataException(message);
            }
        }
    }
}