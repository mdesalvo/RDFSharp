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

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the SERVICE half of RDFQueryParser: federated graph patterns mapped onto a single
/// RDFPatternGroup flagged AsService, the SILENT directive, and the model-imposed failures (variable endpoint,
/// complex inner pattern, nested SERVICE).
/// </summary>
public partial class RDFQueryParserTest
{
    #region Service

    [TestMethod]
    public void ShouldRoundTripServicePatternGroup()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
                .AsService(new RDFSPARQLEndpoint(new Uri("http://example.org/sparql"))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSilentServicePatternGroup()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
                .AsService(new RDFSPARQLEndpoint(new Uri("http://example.org/sparql")),
                    new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult }));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldParseServiceFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/sparql> { ?s ?p ?o } }");

        RDFPatternGroup patternGroup = query.GetPatternGroups().Single();
        Assert.IsTrue(patternGroup.EvaluateAsService.HasValue);
        Assert.AreEqual("http://example.org/sparql", patternGroup.EvaluateAsService.Value.Item1.BaseAddress.ToString());
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, patternGroup.EvaluateAsService.Value.Item2.ErrorBehavior);
        Assert.AreEqual(1, patternGroup.GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseSilentServiceFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE SILENT <http://example.org/sparql> { ?s ?p ?o } }");

        RDFPatternGroup patternGroup = query.GetPatternGroups().Single();
        Assert.IsTrue(patternGroup.EvaluateAsService.HasValue);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult, patternGroup.EvaluateAsService.Value.Item2.ErrorBehavior);
    }

    [TestMethod]
    public void ShouldParseServiceWithPrefixedEndpoint()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("PREFIX ex: <http://example.org/> SELECT * WHERE { SERVICE ex:sparql { ?s ?p ?o } }");

        Assert.AreEqual("http://example.org/sparql", query.GetPatternGroups().Single().EvaluateAsService.Value.Item1.BaseAddress.ToString());
    }

    [TestMethod]
    public void ShouldParseServiceWithMultipleTriplesInner()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/sparql> { ?s ?p ?o . ?o ?q ?z } }");

        RDFPatternGroup patternGroup = query.GetPatternGroups().Single();
        Assert.IsTrue(patternGroup.EvaluateAsService.HasValue);
        Assert.AreEqual(2, patternGroup.GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldThrowOnServiceWithVariableEndpoint()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { SERVICE ?ep { ?s ?p ?o } }"));

    [TestMethod]
    public void ShouldThrowOnServiceWithLiteralEndpoint()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { SERVICE \"endpoint\" { ?s ?p ?o } }"));

    [TestMethod]
    public void ShouldThrowOnServiceWithUnionInner()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/sparql> { { ?s ?p ?o } UNION { ?a ?b ?c } } }"));

    [TestMethod]
    public void ShouldThrowOnServiceWithOptionalInner()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/sparql> { ?s ?p ?o OPTIONAL { ?o ?q ?z } } }"));

    [TestMethod]
    public void ShouldThrowOnNestedService()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/a> { SERVICE <http://example.org/b> { ?s ?p ?o } } }"));

    #endregion
}
