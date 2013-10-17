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

namespace Microsoft.WindowsAzure.Commands.Test.CloudService.Utilities
{
    using System;
    using System.IO;
    using Commands.Utilities.Common;
    using Test.Utilities.Common;
    using Commands.Utilities.Common.XmlSchema.ServiceDefinitionSchema;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GeneralTests : TestBase
    {
        [TestMethod]
        public void SerializationTestWithGB18030()
        {
            // Setup
            string outputFileName = "outputFile.txt";
            ServiceDefinition serviceDefinition = General.DeserializeXmlFile<ServiceDefinition>(
                Testing.GetTestResourcePath("GB18030ServiceDefinition.csdef"));

            // Test
            File.Create(outputFileName).Close();
            General.SerializeXmlFile<ServiceDefinition>(serviceDefinition, outputFileName);

            // Assert
            // If reached this point means the test passed
        }
    }
}