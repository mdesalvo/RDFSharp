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
    public class RDFLoadOperationTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        [TestCleanup]
        public void Cleanup()  { server.Stop(); server.Dispose(); }

        #region Tests
        [TestMethod]
        public void ShouldCreateLoadOperation()
        {
            RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:fromCtx"));

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.FromContext);
            Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:fromCtx")));
            Assert.IsNull(operation.ToContext);
            Assert.IsFalse(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), "LOAD <ex:fromCtx>"));
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenCreatingLoadOperationBecauseNullContext()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFLoadOperation(null));

        [TestMethod]
        public void ShouldCreateLoadOperationWithToContext()
        {
            RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:fromCtx")).SetContext(new Uri("ex:toCtx"));

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.FromContext);
            Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:fromCtx")));
            Assert.IsNotNull(operation.ToContext);
            Assert.IsTrue(operation.ToContext.Equals(new Uri("ex:toCtx")));
            Assert.IsFalse(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), "LOAD <ex:fromCtx> INTO GRAPH <ex:toCtx>"));
        }

        [TestMethod]
        public void ShouldCreateLoadSilentOperationWithToContext()
        {
            RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:fromCtx")).SetContext(new Uri("ex:toCtx")).Silent();

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.FromContext);
            Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:fromCtx")));
            Assert.IsNotNull(operation.ToContext);
            Assert.IsTrue(operation.ToContext.Equals(new Uri("ex:toCtx")));
            Assert.IsTrue(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), "LOAD SILENT <ex:fromCtx> INTO GRAPH <ex:toCtx>"));
        }

        [TestMethod]
        public void ShouldApplyToNullGraph()
        {
            RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ctx"));
            RDFOperationResult result = operation.ApplyToGraph(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsNotNull(result.DeleteResultsCount == 0);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToGraph()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFLoadOperationTest/ShouldApplyToGraph"))
                .RespondWith(
                    Response.Create()
                            .WithHeader("ContentType", "application/turtle")
                            .WithBody($"@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:subj> <ex:pred> <ex:obj>."));

            RDFGraph graph = new RDFGraph();
            RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToGraph"));
            RDFOperationResult result = operation.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 3);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsNotNull(result.InsertResultsCount == 1);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsTrue(graph.TriplesCount == 1);
        }
        #endregion
    }
}