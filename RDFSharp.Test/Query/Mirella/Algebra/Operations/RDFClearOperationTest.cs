/*
   Copyright 2012-2022 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Web;
using System.Threading.Tasks;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFClearOperationTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        [TestCleanup]
        public void Cleanup()  { server.Stop(); server.Dispose(); }

        #region Tests
        [TestMethod]
        public void ShouldCreateClearNamedOperation()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.FromContext);
            Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:ctx")));
            Assert.IsFalse(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), "CLEAR GRAPH <ex:ctx>"));
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenCreatingClearNamedOperationBecauseNullContext()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFClearOperation(null));

        [TestMethod]
        public void ShouldCreateClearSilentNamedOperation()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx")).Silent();

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.FromContext);
            Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:ctx")));
            Assert.IsTrue(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), "CLEAR SILENT GRAPH <ex:ctx>"));
        }

        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.ALL)]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.DEFAULT)]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.NAMED)]
        public void ShouldCreateClearFlavorOperation(RDFQueryEnums.RDFClearOperationFlavor opFlavor)
        {
            RDFClearOperation operation = new RDFClearOperation(opFlavor);

            Assert.IsNotNull(operation);
            Assert.IsNull(operation.FromContext);
            Assert.IsTrue(operation.OperationFlavor == opFlavor);
            Assert.IsFalse(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), $"CLEAR {opFlavor}"));
        }

        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.ALL)]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.DEFAULT)]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.NAMED)]
        public void ShouldCreateClearSilentFlavorOperation(RDFQueryEnums.RDFClearOperationFlavor opFlavor)
        {
            RDFClearOperation operation = new RDFClearOperation(opFlavor).Silent();

            Assert.IsNotNull(operation);
            Assert.IsNull(operation.FromContext);
            Assert.IsTrue(operation.OperationFlavor == opFlavor);
            Assert.IsTrue(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), $"CLEAR SILENT {opFlavor}"));
        }
        #endregion
    }
}