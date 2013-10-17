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
    using System.IO;

    /// <summary>
    /// A profile store that reads and writes to the default
    /// profile storage. If the new profile store file does
    /// not exist, falls back to reading the old files/format.
    /// 
    /// </summary>
    public class PowershellDefaultProfileStore : IProfileStore
    {
        private IProfileStore newStore;

        public void Save(ProfileData profile)
        {
            EnsureNewStore();
            newStore.Save(profile);
        }

        public ProfileData Load()
        {
            IProfileStore store = NewStoreExists() ? EnsureNewStore() : new PowershellOldSettingsProfileStore();
            return store.Load();
        }

        public void DestroyData()
        {
            EnsureNewStore();
            newStore.DestroyData();
        }

        private bool NewStoreExists()
        {
            string profilePath = Path.Combine(GlobalPathInfo.AzureAppDir, PowershellProfileStore.DefaultProfileName);
            return File.Exists(profilePath);
        }

        private IProfileStore EnsureNewStore()
        {
            if (newStore == null)
            {
                newStore = new PowershellProfileStore();
            }
            return newStore;
        }
    }
}