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
public class RDFDropOperationTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    #region Tests
    [TestMethod]
    public void ShouldCreateDropNamedOperation()
    {
        RDFDropOperation operation = new RDFDropOperation(new Uri("ex:ctx"));

        Assert.IsNotNull(operation);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:ctx")));
        Assert.IsFalse(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "DROP GRAPH <ex:ctx>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenCreatingDropNamedOperationBecauseNullContext()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFDropOperation(null));

    [TestMethod]
    [DataRow(RDFQueryEnums.RDFClearOperationFlavor.ALL)]
    [DataRow(RDFQueryEnums.RDFClearOperationFlavor.DEFAULT)]
    [DataRow(RDFQueryEnums.RDFClearOperationFlavor.NAMED)]
    public void ShouldCreateDropFlavorOperation(RDFQueryEnums.RDFClearOperationFlavor opFlavor)
    {
        RDFDropOperation operation = new RDFDropOperation(opFlavor).Silent();

        Assert.IsNull(operation.FromContext);
        Assert.AreEqual(opFlavor, operation.OperationFlavor);
        Assert.IsTrue(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), $"DROP SILENT {opFlavor}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldParseDropOperationFromString()
    {
        RDFDropOperation operation = RDFDropOperation.FromString("DROP GRAPH <ex:ctx>");

        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:ctx")));
        Assert.IsTrue(string.Equals(RDFDropOperation.FromString(operation.ToString()).ToString(), operation.ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenParsingDifferentOperationAsDrop()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFDropOperation.FromString("CLEAR ALL"));

    [TestMethod]
    public void ShouldApplyToGraphAsClearAll()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")));
        RDFDropOperation operation = new RDFDropOperation(new Uri("ex:ctx"));
        RDFOperationResult result = operation.ApplyToGraph(graph); //DROP on a graph behaves like CLEAR ALL

        Assert.AreEqual(1, result.DeleteResultsCount);
        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToStoreWithContextBehavior()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), RDFVocabulary.RDFS.CLASS, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDropOperation operation = new RDFDropOperation(new Uri("ex:ctx"));
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.AreEqual(1, result.DeleteResultsCount);
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.AreEqual(1, store.QuadruplesCount); //only the default-context quadruple survives
    }

    [TestMethod]
    public void ShouldApplyToStoreWithAllFlavorBehavior()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), RDFVocabulary.RDFS.CLASS, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDropOperation operation = new RDFDropOperation(RDFQueryEnums.RDFClearOperationFlavor.ALL);
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.AreEqual(0, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpoint()
    {
        server
            .Given(Request.Create().WithPath("/RDFDropOperationTest/ShouldApplyToSPARQLUpdateEndpoint"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDropOperationTest/ShouldApplyToSPARQLUpdateEndpoint"));

        RDFDropOperation operation = new RDFDropOperation(new Uri("ex:ctx"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldSuppressFailureWhenApplyingSilentDropToSPARQLUpdateEndpoint()
    {
        server
            .Given(Request.Create().WithPath("/RDFDropOperationTest/ShouldSuppressFailure"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.InternalServerError));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDropOperationTest/ShouldSuppressFailure"));

        RDFDropOperation operation = new RDFDropOperation(RDFQueryEnums.RDFClearOperationFlavor.ALL).Silent();
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsFalse(result);
    }
    #endregion
}
