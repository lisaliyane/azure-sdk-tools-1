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

namespace Microsoft.WindowsAzure.Commands.Utilities.ServiceBus
{
    using System.Management.Automation;
    using Commands.Utilities.Common;

    public static class ServiceBusPowerShellUtility
    {
        public static PSObject GetNamespacePSObject(ExtendedAuthorizationRule rule)
        {
            return (null == rule? null : PowerShellUtility.ConstructPSObject(
                typeof(ExtendedAuthorizationRule).FullName,
                "Namespace", rule.Namespace,
                "Name", rule.Name,
                "ConnectionString", rule.ConnectionString,
                "Permission", rule.Permission,
                "Rule", rule.Rule));
        }

        public static PSObject GetEntityPSObject(ExtendedAuthorizationRule rule)
        {
            return (null == rule? null : new PSObject(rule));
        }
    }
}