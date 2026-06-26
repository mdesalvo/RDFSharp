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
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFOperationSetTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    [TestCleanup]
    public void Cleanup() { server?.Stop(); server?.Dispose(); }

    #region Tests (Model)
    [TestMethod]
    public void ShouldCreateEmptyOperationSet()
    {
        RDFOperationSet operationSet = new RDFOperationSet();

        Assert.IsNotNull(operationSet);
        Assert.IsNotNull(operationSet.Operations);
        Assert.IsEmpty(operationSet.Operations);
        Assert.IsEmpty(operationSet.ToString());
    }

    [TestMethod]
    public void ShouldAddOperationsPreservingOrder()
    {
        RDFInsertDataOperation op1 = new RDFInsertDataOperation()
            .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1")));
        RDFDeleteDataOperation op2 = new RDFDeleteDataOperation()
            .AddDeleteTemplate(new RDFPattern(new RDFResource("ex:s2"), new RDFResource("ex:p2"), new RDFResource("ex:o2")));

        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(op1)
            .AddOperation(op2);

        Assert.HasCount(2, operationSet.Operations);
        Assert.AreSame(op1, operationSet.Operations[0]);
        Assert.AreSame(op2, operationSet.Operations[1]);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingNullOperation()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFOperationSet().AddOperation(null));
    #endregion

    #region Tests (Printer / RoundTrip)
    [TestMethod]
    public void ShouldPrintOperationSet()
    {
        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1"))))
            .AddOperation(new RDFDeleteDataOperation()
                .AddDeleteTemplate(new RDFPattern(new RDFResource("ex:s2"), new RDFResource("ex:p2"), new RDFResource("ex:o2"))));

        Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(operationSet.ToString()),
            RDFTestUtilities.NormalizeEOL("""
            INSERT DATA {
              <ex:s1> <ex:p1> <ex:o1> .
            }
            ;
            DELETE DATA {
              <ex:s2> <ex:p2> <ex:o2> .
            }
            """), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldRoundTripOperationSet()
    {
        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1"))))
            .AddOperation(new RDFInsertWhereOperation()
                .AddInsertTemplate(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:knows"), new RDFVariable("?Y")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:friend"), new RDFVariable("?Y")))));

        string printed = operationSet.ToString();
        RDFOperationSet reparsed = RDFOperationSet.FromString(printed);

        Assert.HasCount(2, reparsed.Operations);
        Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(printed),
            RDFTestUtilities.NormalizeEOL(reparsed.ToString()), StringComparison.Ordinal));
    }
    #endregion

    #region Tests (Parser)
    [TestMethod]
    public void ShouldParseOperationSetWithMultipleOperations()
    {
        RDFOperationSet operationSet = RDFOperationSet.FromString("""
            INSERT DATA { <ex:s1> <ex:p1> <ex:o1> } ;
            DELETE DATA { <ex:s2> <ex:p2> <ex:o2> } ;
            INSERT { ?X <ex:knows> ?Y } WHERE { ?X <ex:friend> ?Y }
            """);

        Assert.IsNotNull(operationSet);
        Assert.HasCount(3, operationSet.Operations);
        Assert.IsInstanceOfType<RDFInsertDataOperation>(operationSet.Operations[0]);
        Assert.IsInstanceOfType<RDFDeleteDataOperation>(operationSet.Operations[1]);
        Assert.IsInstanceOfType<RDFInsertWhereOperation>(operationSet.Operations[2]);
    }

    [TestMethod]
    public void ShouldParseOperationSetWithSingleOperation()
    {
        RDFOperationSet operationSet = RDFOperationSet.FromString("INSERT DATA { <ex:s1> <ex:p1> <ex:o1> }");

        Assert.IsNotNull(operationSet);
        Assert.HasCount(1, operationSet.Operations);
        Assert.IsInstanceOfType<RDFInsertDataOperation>(operationSet.Operations[0]);
    }

    [TestMethod]
    public void ShouldParseOperationSetWithTrailingSeparator()
    {
        RDFOperationSet operationSet = RDFOperationSet.FromString("INSERT DATA { <ex:s1> <ex:p1> <ex:o1> } ; DELETE DATA { <ex:s2> <ex:p2> <ex:o2> } ;");

        Assert.IsNotNull(operationSet);
        Assert.HasCount(2, operationSet.Operations);
    }

    [TestMethod]
    public void ShouldParseOperationSetWithCumulativePrologue()
    {
        //The PREFIX declared before the first operation must stay in scope for the second one too
        RDFOperationSet operationSet = RDFOperationSet.FromString("""
            PREFIX ex: <http://example.org/>
            INSERT DATA { ex:s1 ex:p1 ex:o1 } ;
            DELETE DATA { ex:s2 ex:p2 ex:o2 }
            """);

        Assert.IsNotNull(operationSet);
        Assert.HasCount(2, operationSet.Operations);
        Assert.HasCount(1, operationSet.Operations[0].GetPrefixes());
        Assert.HasCount(1, operationSet.Operations[1].GetPrefixes());
    }

    [TestMethod]
    public void ShouldThrowExceptionOnParsingNullOrEmptyOperationSet()
    {
        Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFOperationSet.FromString(null));
        Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFOperationSet.FromString("   "));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnParsingOperationSetWithMissingSeparator()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFOperationSet.FromString("INSERT DATA { <ex:s1> <ex:p1> <ex:o1> } DELETE DATA { <ex:s2> <ex:p2> <ex:o2> }"));

    [TestMethod]
    public void ShouldThrowExceptionOnParsingOperationSetWithEmptyOperationInChain()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFOperationSet.FromString("INSERT DATA { <ex:s1> <ex:p1> <ex:o1> } ; ; DELETE DATA { <ex:s2> <ex:p2> <ex:o2> }"));

    [TestMethod]
    public void ShouldThrowExceptionOnParsingOperationSetWithOnlyPrologue()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFOperationSet.FromString("PREFIX ex: <http://example.org/>"));

    [TestMethod]
    public void ShouldThrowExceptionOnParsingOperationSetWithNonRepresentableOperation()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFOperationSet.FromString("INSERT DATA { <ex:s1> <ex:p1> <ex:o1> } ; CREATE GRAPH <ex:g>"));

    [TestMethod]
    public void ShouldStillRejectChainOnSingleOperationFromString()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = RDFInsertDataOperation.FromString("INSERT DATA { <ex:s1> <ex:p1> <ex:o1> } ; DELETE DATA { <ex:s2> <ex:p2> <ex:o2> }"));
    #endregion

    #region Tests (Apply - Graph)
    [TestMethod]
    public void ShouldApplyToGraphSequentiallyWithVisibleEffects()
    {
        RDFGraph graph = new RDFGraph();

        //op1 inserts a triple; op2 (DELETE WHERE) must SEE and remove the triple inserted by op1
        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))))
            .AddOperation(new RDFDeleteWhereOperation()
                .AddDeleteTemplate(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?WHO")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?WHO")))));

        IReadOnlyList<RDFOperationResult> results = operationSet.ApplyToGraph(graph);

        Assert.IsNotNull(results);
        Assert.HasCount(2, results);
        Assert.AreEqual(1, results[0].InsertResultsCount);
        Assert.AreEqual(1, results[1].DeleteResultsCount);
        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToNullGraph()
    {
        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o"))));

        IReadOnlyList<RDFOperationResult> results = operationSet.ApplyToGraph(null);

        Assert.IsNotNull(results);
        Assert.IsEmpty(results);
    }

    [TestMethod]
    public async Task ShouldApplyToGraphAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o"))));

        IReadOnlyList<RDFOperationResult> results = await operationSet.ApplyToGraphAsync(graph);

        Assert.HasCount(1, results);
        Assert.AreEqual(1, graph.TriplesCount);
    }
    #endregion

    #region Tests (Apply - Store)
    [TestMethod]
    public void ShouldApplyToStoreSequentially()
    {
        RDFMemoryStore store = new RDFMemoryStore();

        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o"))))
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:s2"), new RDFResource("ex:p2"), new RDFResource("ex:o2"))));

        IReadOnlyList<RDFOperationResult> results = operationSet.ApplyToStore(store);

        Assert.HasCount(2, results);
        Assert.AreEqual(2, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldApplyToNullStore()
    {
        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o"))));

        IReadOnlyList<RDFOperationResult> results = operationSet.ApplyToStore(null);

        Assert.IsNotNull(results);
        Assert.IsEmpty(results);
    }

    [TestMethod]
    public async Task ShouldApplyToStoreAsync()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o"))));

        IReadOnlyList<RDFOperationResult> results = await operationSet.ApplyToStoreAsync(store);

        Assert.HasCount(1, results);
        Assert.AreEqual(1, store.QuadruplesCount);
    }
    #endregion

    #region Tests (Apply - Endpoint)
    [TestMethod]
    public void ShouldApplyToNullSPARQLUpdateEndpoint()
    {
        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o"))));

        Assert.IsFalse(operationSet.ApplyToSPARQLUpdateEndpoint(null));
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointInSingleRequest()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFOperationSetTest/ShouldApplyToSPARQLUpdateEndpointInSingleRequest"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFOperationSetTest/ShouldApplyToSPARQLUpdateEndpointInSingleRequest"));

        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1"))))
            .AddOperation(new RDFDeleteDataOperation()
                .AddDeleteTemplate(new RDFPattern(new RDFResource("ex:s2"), new RDFResource("ex:p2"), new RDFResource("ex:o2"))));

        Assert.IsTrue(operationSet.ApplyToSPARQLUpdateEndpoint(endpoint));

        //Exactly ONE request must have hit the endpoint, carrying the whole ';'-joined chain
        Assert.HasCount(1, server.LogEntries);
    }

    [TestMethod]
    public void ShouldFailApplyingToSPARQLUpdateEndpointOnError()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFOperationSetTest/ShouldFailApplyingToSPARQLUpdateEndpointOnError"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.InternalServerError));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFOperationSetTest/ShouldFailApplyingToSPARQLUpdateEndpointOnError"));

        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1"))));

        Assert.ThrowsExactly<RDFQueryException>(() => _ = operationSet.ApplyToSPARQLUpdateEndpoint(endpoint));
    }

    [TestMethod]
    public async Task ShouldApplyToSPARQLUpdateEndpointAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFOperationSetTest/ShouldApplyToSPARQLUpdateEndpointAsync"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFOperationSetTest/ShouldApplyToSPARQLUpdateEndpointAsync"));

        RDFOperationSet operationSet = new RDFOperationSet()
            .AddOperation(new RDFInsertDataOperation()
                .AddInsertTemplate(new RDFPattern(new RDFResource("ex:s1"), new RDFResource("ex:p1"), new RDFResource("ex:o1"))));

        Assert.IsTrue(await operationSet.ApplyToSPARQLUpdateEndpointAsync(endpoint));
    }
    #endregion
}
