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
public class RDFAddOperationTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    #region Tests
    [TestMethod]
    public void ShouldCreateAddOperation()
    {
        RDFAddOperation operation = new RDFAddOperation().SetFromContext(new Uri("ex:g1")).SetToContext(new Uri("ex:g2"));

        Assert.IsNotNull(operation);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:g1")));
        Assert.IsTrue(operation.ToContext.Equals(new Uri("ex:g2")));
        Assert.IsFalse(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "ADD <ex:g1> TO <ex:g2>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateSilentAddOperationWithDefaultEndpoints()
    {
        RDFAddOperation operation = new RDFAddOperation().SetToContext(new Uri("ex:g2")).Silent();

        Assert.IsNull(operation.FromContext); //null denotes the DEFAULT graph
        Assert.IsTrue(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "ADD SILENT DEFAULT TO <ex:g2>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldParseAddOperationFromString()
    {
        RDFAddOperation operation = RDFAddOperation.FromString("ADD SILENT <ex:g1> TO <ex:g2>");

        Assert.IsTrue(operation.IsSilent);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:g1")));
        Assert.IsTrue(operation.ToContext.Equals(new Uri("ex:g2")));
        Assert.IsTrue(string.Equals(RDFAddOperation.FromString(operation.ToString()).ToString(), operation.ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldParseAddOperationFromStringWithDefaultSource()
    {
        RDFAddOperation operation = RDFAddOperation.FromString("ADD DEFAULT TO <ex:g2>");

        Assert.IsNull(operation.FromContext);
        Assert.IsTrue(operation.ToContext.Equals(new Uri("ex:g2")));
        Assert.IsTrue(string.Equals(operation.ToString(), "ADD DEFAULT TO <ex:g2>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenParsingDifferentOperationAsAdd()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFAddOperation.FromString("CLEAR ALL"));

    [TestMethod]
    public void ShouldApplyToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:g1"), new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:g1"), new RDFResource("ex:s1"), new RDFResource("ex:p2"), new RDFResource("ex:o2")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:g2"), new RDFResource("ex:sX"), new RDFResource("ex:pX"), new RDFResource("ex:oX")));
        RDFAddOperation operation = new RDFAddOperation().SetFromContext(new Uri("ex:g1")).SetToContext(new Uri("ex:g2"));
        RDFOperationResult result = operation.ApplyToStore(store);

        //ADD inserts source triples into destination, keeping both the source and the pre-existing destination data
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(2, result.InsertResultsCount);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:g1"), new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:g2"), new RDFResource("ex:sX"), new RDFResource("ex:pX"), new RDFResource("ex:oX"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:g2"), new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:g2"), new RDFResource("ex:s1"), new RDFResource("ex:p2"), new RDFResource("ex:o2"))));
    }

    [TestMethod]
    public void ShouldApplyToStoreAsNoOpWhenSourceEqualsDestination()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:g1"), new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1")));
        RDFAddOperation operation = new RDFAddOperation().SetFromContext(new Uri("ex:g1")).SetToContext(new Uri("ex:g1"));
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingToGraph()
    {
        RDFGraph graph = new RDFGraph();
        RDFAddOperation operation = new RDFAddOperation().SetFromContext(new Uri("ex:g1")).SetToContext(new Uri("ex:g2"));

        //ADD/COPY/MOVE are two-graph operations: a contextless graph cannot represent them
        Assert.ThrowsExactly<RDFQueryException>(() => operation.ApplyToGraph(graph));
    }

    [TestMethod]
    public void ShouldSuppressExceptionWhenApplyingSilentToGraph()
    {
        RDFGraph graph = new RDFGraph();
        RDFAddOperation operation = new RDFAddOperation().SetFromContext(new Uri("ex:g1")).SetToContext(new Uri("ex:g2")).Silent();
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(0, result.InsertResultsCount);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpoint()
    {
        server
            .Given(Request.Create().WithPath("/RDFAddOperationTest/ShouldApplyToSPARQLUpdateEndpoint"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFAddOperationTest/ShouldApplyToSPARQLUpdateEndpoint"));

        RDFAddOperation operation = new RDFAddOperation().SetFromContext(new Uri("ex:g1")).SetToContext(new Uri("ex:g2"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }
    #endregion
}
