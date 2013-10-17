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

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.Common
{
    using System;
    using System.Diagnostics;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Base class for Windows Azure PowerShell unit tests.
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Gets or sets a reference to the TestContext used for interacting
        /// with the test framework.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Log a message with the test framework.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Arguments.</param>
        public void Log(string format, params object[] args)
        {
            Debug.Assert(TestContext != null);
            TestContext.WriteLine(format, args);
        }

        protected static int AnyIpPort()
        {
            return new Random().Next(ushort.MaxValue);
        }

        public static Uri AnyUrl()
        {
            return new Uri("http://www.microsoft.com");
        }

        public static string AnyString()
        {
            return "RandomStringForTestPurposes";
        }
    }
}
