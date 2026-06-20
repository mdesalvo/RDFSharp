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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the CONSTRUCT-form half of RDFQueryParser: the graph template (LONG and SHORT forms) plus the
/// WHERE clause mapped onto RDFConstructQuery, and the model-imposed failures (dataset clauses, ORDER BY/GROUP
/// BY/HAVING modifiers, property paths and non-triple elements in the template — all non-representable).
/// </summary>
public partial class RDFQueryParserTest
{
    #region ConstructQuery

    /// <summary>
    /// Asserts that printing the given CONSTRUCT query, parsing the print back, and printing the result yields the
    /// very same text: the round-trip oracle for the CONSTRUCT form (sibling of <see cref="AssertAskQueryRoundTrips"/>).
    /// </summary>
    private static void AssertConstructQueryRoundTrips(RDFConstructQuery originalQuery)
    {
        string printedQuery = originalQuery.ToString();
        RDFConstructQuery reparsedQuery = RDFConstructQuery.FromString(printedQuery);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(printedQuery), RDFTestUtilities.NormalizeEOL(reparsedQuery.ToString()));
    }

    [TestMethod]
    public void ShouldRoundTripConstructWithSinglePattern()
        => AssertConstructQueryRoundTrips(new RDFConstructQuery()
            .AddTemplate(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))));

    [TestMethod]
    public void ShouldRoundTripConstructWithPrefixesAndMultipleTemplates()
        => AssertConstructQueryRoundTrips(new RDFConstructQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
            .AddTemplate(new RDFPattern(new RDFVariable("person"), RDFVocabulary.RDF.TYPE, RDFVocabulary.FOAF.PERSON))
            .AddTemplate(new RDFPattern(new RDFVariable("person"), RDFVocabulary.FOAF.NAME, new RDFVariable("name")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("person"), RDFVocabulary.FOAF.NAME, new RDFVariable("name")))));

    [TestMethod]
    public void ShouldRoundTripConstructWithLimitAndOffset()
        => AssertConstructQueryRoundTrips(new RDFConstructQuery()
            .AddTemplate(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddModifier(new RDFLimitModifier(10))
            .AddModifier(new RDFOffsetModifier(5)));

    [TestMethod]
    public void ShouldRoundTripConstructWithUnionInWhere()
        => AssertConstructQueryRoundTrips(RDFConstructQuery.FromString(
            "CONSTRUCT { ?s ?p ?o } WHERE { { ?s ?p ?o } UNION { ?a ?b ?c } }"));

    [TestMethod]
    public void ShouldRoundTripConstructWithOrderByAndGroupBy()
        => AssertConstructQueryRoundTrips(new RDFConstructQuery()
            .AddTemplate(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddModifier(new RDFGroupByModifier([new RDFVariable("s")]))
            .AddModifier(new RDFOrderByModifier(new RDFStrLenExpression(new RDFVariable("s")), RDFQueryEnums.RDFOrderByFlavors.DESC))
            .AddModifier(new RDFLimitModifier(5)));

    [TestMethod]
    public void ShouldParseConstructWithSinglePattern()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");

        Assert.AreEqual(1, query.Templates.Count);
        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseConstructWithPredicateObjectAndObjectLists()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString(
            "PREFIX foaf: <http://xmlns.com/foaf/0.1/> CONSTRUCT { ?p a foaf:Person ; foaf:name ?n , ?n2 } WHERE { ?p foaf:name ?n }");

        //'a' + two predicate-object groups expanded into three template patterns
        Assert.AreEqual(3, query.Templates.Count);
        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldPreserveGroundTripleInTemplate()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString(
            "CONSTRUCT { <http://example.org/s> <http://example.org/p> <http://example.org/o> } WHERE { ?s ?p ?o }");

        //A fully-ground template triple must survive (it would have been dropped by a pattern group's guard)
        Assert.AreEqual(1, query.Templates.Count);
        Assert.AreEqual(0, query.Templates[0].Variables.Count);
    }

    [TestMethod]
    public void ShouldParseConstructWithCollectionInTemplate()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString(
            "CONSTRUCT { ?s <http://example.org/list> ( ?a ?b ) } WHERE { ?s ?p ?o }");

        //The two-item collection desugars into 1 outer triple + 4 rdf:first/rdf:rest triples = 5 template patterns
        Assert.AreEqual(5, query.Templates.Count);
    }

    [TestMethod]
    public void ShouldParseConstructShortForm()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString("CONSTRUCT WHERE { ?s ?p ?o . ?o ?q ?z }");

        //SHORT form: the WHERE triples are BOTH the template and the WHERE body
        Assert.AreEqual(2, query.Templates.Count);
        Assert.AreEqual(2, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldRoundTripConstructShortForm()
        => AssertConstructQueryRoundTrips(RDFConstructQuery.FromString("CONSTRUCT WHERE { ?s ?p ?o . ?o ?q ?z }"));

    [TestMethod]
    public void ShouldThrowWhenConstructFromStringFedASelectQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFConstructQuery.FromString("SELECT * WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnConstructWithDatasetClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFConstructQuery.FromString("CONSTRUCT { ?s ?p ?o } FROM <http://example.org/g> WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnConstructShortFormWithDatasetClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFConstructQuery.FromString("CONSTRUCT FROM <http://example.org/g> WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnConstructWithPropertyPathInTemplate()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFConstructQuery.FromString(
                "PREFIX ex: <http://example.org/> CONSTRUCT { ?s ex:p/ex:q ?o } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnConstructWithNonTripleInTemplate()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFConstructQuery.FromString("CONSTRUCT { ?s ?p ?o FILTER(BOUND(?s)) } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldParseConstructWithOrderByModifier()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o } ORDER BY ?s");

        Assert.AreEqual(1, query.Templates.Count);
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFOrderByModifier));
    }

    [TestMethod]
    public void ShouldParseConstructWithGroupByModifier()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o } GROUP BY ?s");

        Assert.AreEqual(1, query.Templates.Count);
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFGroupByModifier));
    }

    [TestMethod]
    public void ShouldParseConstructWithLimitOffset()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o } LIMIT 10 OFFSET 5");

        Assert.AreEqual(1, query.Templates.Count);
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFLimitModifier));
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFOffsetModifier));
    }

    #endregion
}
