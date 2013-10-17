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

namespace Microsoft.WindowsAzure.Commands.Utilities.Store
{
    using Commands.Utilities.Common;

    /// <summary>
    /// Search options for getting Windows Azure add on.
    /// </summary>
    public class AddOnSearchOptions
    {
        /// <summary>
        /// Creates new instance from AddOnSearchOptions
        /// </summary>
        /// <param name="name">The add on name</param>
        /// <param name="provider">The add on provider</param>
        /// <param name="geoRegion">The add on region</param>
        public AddOnSearchOptions(string name = null, string provider = null, string geoRegion = null)
        {
            Name = name;

            Provider = provider;

            GeoRegion = geoRegion;
        }

        public string Name { get; set; }

        public string Provider { get; set; }

        public string GeoRegion { get; set; }

        public override bool Equals(object obj)
        {
            AddOnSearchOptions rhs = obj as AddOnSearchOptions;
            if (rhs == null) { return false; }

            return
                General.TryEquals(this.Name, rhs.Name) &&
                General.TryEquals(this.Provider, rhs.Provider) &&
                General.TryEquals(this.GeoRegion, rhs.GeoRegion);
        }

        public override int GetHashCode()
        {
            return
                (Name ?? string.Empty).GetHashCode() ^
                (Provider ?? string.Empty).GetHashCode() ^
                (GeoRegion ?? string.Empty).GetHashCode();
        }
    }
}
