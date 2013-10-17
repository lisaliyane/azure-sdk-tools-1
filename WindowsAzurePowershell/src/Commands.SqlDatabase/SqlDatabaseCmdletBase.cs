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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Commands.Utilities;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Sql;

    /// <summary>
    /// The base class for all Windows Azure Sql Database Management Cmdlets
    /// </summary>
    public abstract class SqlDatabaseCmdletBase : PSCmdlet
    {
        /// <summary>
        /// Stores the session Id for all the request made in this session.
        /// </summary>
        internal static string clientSessionId;

        static SqlDatabaseCmdletBase()
        {
            clientSessionId = SqlDatabaseCmdletBase.GenerateClientTracingId();
        }

        /// <summary>
        /// Generates a client side tracing Id of the format:
        /// [Guid]-[Time in UTC]
        /// </summary>
        /// <returns>A string representation of the client side tracing Id.</returns>
        public static string GenerateClientTracingId()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}-{1}",
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.ToString("u"));
        }

        /// <summary>
        /// Retrieve the SQL Management client for the currently selected subscription.
        /// </summary>
        /// <returns>The SQL Management client for the currently selected subscription.</returns>
        public static SqlManagementClient GetCurrentSqlClient()
        {
            // Get the SQL management client for the current subscription
            WindowsAzureSubscription subscription = WindowsAzureProfile.Instance.CurrentSubscription;
            SqlDatabaseCmdletBase.ValidateSubscription(subscription);
            return subscription.CreateClient<SqlManagementClient>();
        }

        /// <summary>
        /// Validates that the given subscription is valid.
        /// </summary>
        /// <param name="subscription">The <see cref="WindowsAzureSubscription"/> to validate.</param>
        public static void ValidateSubscription(WindowsAzureSubscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentException(
                    Utilities.Properties.Resources.InvalidCurrentSubscription);
            }

            if (string.IsNullOrEmpty(subscription.SubscriptionId))
            {
                throw new ArgumentException(
                    Utilities.Properties.Resources.InvalidCurrentSubscriptionId);
            }
        }

        /// <summary>
        /// Stores the per request session Id for all request made in this cmdlet call.
        /// </summary>
        protected string clientRequestId;

        internal SqlDatabaseCmdletBase()
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();
        }

        protected void WriteErrorDetails(Exception exception)
        {
            // Call the handler to parse and write error details.
            SqlDatabaseExceptionHandler.WriteErrorDetails(this, this.clientRequestId, exception);
        }
    }
}