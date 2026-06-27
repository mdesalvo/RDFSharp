/*
   Copyright 2012-2026 Marco De Salvo

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
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFCreateOperationTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    #region Tests
    [TestMethod]
    public void ShouldCreateCreateOperation()
    {
        RDFCreateOperation operation = new RDFCreateOperation(new Uri("ex:g"));

        Assert.IsNotNull(operation);
        Assert.IsNotNull(operation.FromContext);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:g")));
        Assert.IsFalse(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "CREATE GRAPH <ex:g>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateSilentCreateOperation()
    {
        RDFCreateOperation operation = new RDFCreateOperation(new Uri("ex:g")).Silent();

        Assert.IsTrue(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "CREATE SILENT GRAPH <ex:g>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenCreatingCreateOperationBecauseNullContext()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFCreateOperation(null));

    [TestMethod]
    public void ShouldParseCreateOperationFromString()
    {
        RDFCreateOperation operation = RDFCreateOperation.FromString("CREATE SILENT GRAPH <ex:g>");

        Assert.IsNotNull(operation);
        Assert.IsTrue(operation.IsSilent);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:g")));
        Assert.IsTrue(string.Equals(RDFCreateOperation.FromString(operation.ToString()).ToString(), operation.ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenParsingDifferentOperationAsCreate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFCreateOperation.FromString("CLEAR ALL"));

    [TestMethod]
    public void ShouldApplyToStoreAsNoOp()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")));
        RDFCreateOperation operation = new RDFCreateOperation(new Uri("ex:g"));
        RDFOperationResult result = operation.ApplyToStore(store);

        //RDFSharp does not record empty graphs => CREATE is a no-op locally
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldApplyToGraphAsNoOp()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")));
        RDFCreateOperation operation = new RDFCreateOperation(new Uri("ex:g"));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.AreEqual(1, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpoint()
    {
        server
            .Given(Request.Create().WithPath("/RDFCreateOperationTest/ShouldApplyToSPARQLUpdateEndpoint"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFCreateOperationTest/ShouldApplyToSPARQLUpdateEndpoint"));

        RDFCreateOperation operation = new RDFCreateOperation(new Uri("ex:g"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldSuppressFailureWhenApplyingSilentCreateToSPARQLUpdateEndpoint()
    {
        server
            .Given(Request.Create().WithPath("/RDFCreateOperationTest/ShouldSuppressFailure"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.InternalServerError));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFCreateOperationTest/ShouldSuppressFailure"));

        RDFCreateOperation operation = new RDFCreateOperation(new Uri("ex:g")).Silent();
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        //SILENT swallows the remote failure and reports a benign "false"
        Assert.IsFalse(result);
    }
    #endregion
}
