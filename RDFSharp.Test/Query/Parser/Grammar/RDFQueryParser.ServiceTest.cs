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
/// Unit tests for the SERVICE half of RDFQueryParser: federated graph patterns mapped onto the first-class
/// RDFService algebra node — concrete or variable endpoint, the SILENT directive, complex inner patterns
/// (UNION/OPTIONAL/sub-select), and nested SERVICE.
/// </summary>
public partial class RDFQueryParserTest
{
    #region Service

    [TestMethod]
    public void ShouldRoundTripServicePatternGroup()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(new RDFSPARQLEndpoint(new Uri("http://example.org/sparql")),
                new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSilentServicePatternGroup()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(new RDFSPARQLEndpoint(new Uri("http://example.org/sparql")),
                new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))),
                new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult }));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripVariableEndpointService()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("ep"), new RDFResource("http://example.org/x"))))
            .AddService(new RDFService(new RDFVariable("ep"),
                new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripServiceWithUnionInner()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(new RDFSPARQLEndpoint(new Uri("http://example.org/sparql")),
                new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
                    .Union(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("a"), new RDFVariable("b"), new RDFVariable("c"))))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripNestedService()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(new RDFSPARQLEndpoint(new Uri("http://example.org/a")),
                new RDFService(new RDFSPARQLEndpoint(new Uri("http://example.org/b")),
                    new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldParseServiceFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/sparql> { ?s ?p ?o } }");

        RDFService service = query.GetServices().Single();
        Assert.IsNull(service.EndpointVariable);
        Assert.AreEqual("http://example.org/sparql", service.Endpoint.BaseAddress.ToString());
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, service.QueryOptions.ErrorBehavior);
        Assert.IsInstanceOfType<RDFPatternGroup>(service.InnerPattern);
    }

    [TestMethod]
    public void ShouldParseSilentServiceFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE SILENT <http://example.org/sparql> { ?s ?p ?o } }");

        RDFService service = query.GetServices().Single();
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult, service.QueryOptions.ErrorBehavior);
    }

    [TestMethod]
    public void ShouldParseServiceWithPrefixedEndpoint()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("PREFIX ex: <http://example.org/> SELECT * WHERE { SERVICE ex:sparql { ?s ?p ?o } }");

        Assert.AreEqual("http://example.org/sparql", query.GetServices().Single().Endpoint.BaseAddress.ToString());
    }

    [TestMethod]
    public void ShouldParseServiceWithMultipleTriplesInner()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/sparql> { ?s ?p ?o . ?o ?q ?z } }");

        RDFService service = query.GetServices().Single();
        Assert.IsInstanceOfType<RDFPatternGroup>(service.InnerPattern);
        Assert.AreEqual(2, ((RDFPatternGroup)service.InnerPattern).GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseServiceWithVariableEndpoint()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE ?ep { ?s ?p ?o } }");

        RDFService service = query.GetServices().Single();
        Assert.IsNotNull(service.EndpointVariable);
        Assert.AreEqual("?EP", service.EndpointVariable.ToString());
    }

    [TestMethod]
    public void ShouldParseServiceWithUnionInner()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/sparql> { { ?s ?p ?o } UNION { ?a ?b ?c } } }");

        RDFService service = query.GetServices().Single();
        Assert.IsInstanceOfType<RDFBinaryQueryMember>(service.InnerPattern);
    }

    [TestMethod]
    public void ShouldParseServiceWithOptionalInner()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/sparql> { ?s ?p ?o OPTIONAL { ?o ?q ?z } } }");

        Assert.IsNotNull(query.GetServices().Single().InnerPattern);
    }

    [TestMethod]
    public void ShouldParseNestedService()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SERVICE <http://example.org/a> { SERVICE <http://example.org/b> { ?s ?p ?o } } }");

        RDFService service = query.GetServices().Single();
        Assert.AreEqual("http://example.org/a", service.Endpoint.BaseAddress.ToString());
        Assert.IsInstanceOfType<RDFService>(service.InnerPattern);
        Assert.AreEqual("http://example.org/b", ((RDFService)service.InnerPattern).Endpoint.BaseAddress.ToString());
    }

    [TestMethod]
    public void ShouldThrowOnServiceWithLiteralEndpoint()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { SERVICE \"endpoint\" { ?s ?p ?o } }"));

    #endregion
}
