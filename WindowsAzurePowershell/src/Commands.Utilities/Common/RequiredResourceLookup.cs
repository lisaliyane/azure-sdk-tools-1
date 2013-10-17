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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System.Collections.Generic;
    using Management.WebSites;
    using WindowsAzure.Common;

    /// <summary>
    /// This class handles mapping management client types
    /// to the corresponding required resource provider names.
    /// </summary>
    internal static class RequiredResourceLookup
    {
        // Trivial implementation for now, will replace with lookup
        // based on data in service client types themselves once
        // it gets implemented.
        internal static IList<string> RequiredProvidersFor<T>() where T : ServiceClient<T>
        {
            if (typeof(T) == typeof(WebSiteManagementClient))
            {
                return new[] { "website" };
            }

            return new string[0];
        }
    }
}
