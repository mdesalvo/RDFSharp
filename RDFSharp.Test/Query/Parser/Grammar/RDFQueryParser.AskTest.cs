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
/// Unit tests for the ASK-form half of RDFQueryParser: the WHERE-only body mapped onto RDFAskQuery, and the
/// model-imposed failures (dataset clauses and solution modifiers, both non-representable on an ASK query).
/// </summary>
public partial class RDFQueryParserTest
{
    #region AskQuery

    /// <summary>
    /// Asserts that printing the given ASK query, parsing the print back, and printing the result yields the
    /// very same text: the round-trip oracle for the ASK form (sibling of <see cref="AssertSelectQueryRoundTrips"/>).
    /// </summary>
    private static void AssertAskQueryRoundTrips(RDFAskQuery originalQuery)
    {
        string printedQuery = originalQuery.ToString();
        RDFAskQuery reparsedQuery = RDFAskQuery.FromString(printedQuery);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(printedQuery), RDFTestUtilities.NormalizeEOL(reparsedQuery.ToString()));
    }

    [TestMethod]
    public void ShouldRoundTripAskWithSinglePattern()
        => AssertAskQueryRoundTrips(new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))));

    [TestMethod]
    public void ShouldRoundTripAskWithPrefixesAndMultiplePatterns()
        => AssertAskQueryRoundTrips(new RDFAskQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("person"), RDFVocabulary.RDF.TYPE, RDFVocabulary.FOAF.PERSON))
                .AddPattern(new RDFPattern(new RDFVariable("person"), RDFVocabulary.FOAF.NAME, new RDFVariable("name")))));

    [TestMethod]
    public void ShouldRoundTripAskWithUnion()
        => AssertAskQueryRoundTrips(RDFAskQuery.FromString("ASK WHERE { { ?s ?p ?o } UNION { ?a ?b ?c } }"));

    [TestMethod]
    public void ShouldParseAskWithoutWhereKeyword()
    {
        RDFAskQuery query = RDFAskQuery.FromString("ASK { ?s ?p ?o }");

        Assert.AreEqual(1, query.GetPatternGroups().Count());
        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseAskWithWhereKeyword()
    {
        RDFAskQuery query = RDFAskQuery.FromString("ASK WHERE { ?s ?p ?o . ?o ?q ?z }");

        Assert.AreEqual(2, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseAskWithPrefixedTerms()
    {
        RDFAskQuery query = RDFAskQuery.FromString("PREFIX foaf: <http://xmlns.com/foaf/0.1/> ASK { ?p a foaf:Person }");

        Assert.AreEqual(1, query.GetPrefixes().Count);
        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldThrowWhenFromStringFedASelectQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFAskQuery.FromString("SELECT * WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnAskWithDatasetClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFAskQuery.FromString("ASK FROM <http://example.org/g> { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnAskWithNamedDatasetClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFAskQuery.FromString("ASK FROM NAMED <http://example.org/g> { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldParseAskWithSolutionModifiers()
    {
        RDFAskQuery query = RDFAskQuery.FromString("ASK { ?s ?p ?o } ORDER BY ?s LIMIT 5 OFFSET 2");

        //GROUP BY/HAVING/ORDER BY/LIMIT/OFFSET are now representable on an ASK query (they shape the solution
        //sequence before the existence check), so the three modifiers above are parsed onto the query
        Assert.AreEqual(3, query.GetModifiers().Count());
    }

    [TestMethod]
    public void ShouldRoundTripAskWithModifiersAndTrailingValues()
        => AssertAskQueryRoundTrips(RDFAskQuery.FromString("ASK WHERE { ?s ?p ?o } ORDER BY ?s LIMIT 5 OFFSET 2 VALUES ?s { <http://example.org/x> }"));

    [TestMethod]
    public void ShouldEvaluateAskWithLimitZeroAsFalse()
    {
        RDFGraph graph = new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s"), new RDFResource("http://example.org/p"), new RDFResource("http://example.org/o")));

        //Without modifiers the pattern matches → true; LIMIT 0 empties the solution sequence → false; an OFFSET
        //past the last solution likewise → false. The modifiers genuinely drive the boolean answer.
        Assert.IsTrue(RDFAskQuery.FromString("ASK { ?s ?p ?o }").ApplyToGraph(graph).AskResult);
        Assert.IsFalse(RDFAskQuery.FromString("ASK { ?s ?p ?o } LIMIT 0").ApplyToGraph(graph).AskResult);
        Assert.IsFalse(RDFAskQuery.FromString("ASK { ?s ?p ?o } OFFSET 5").ApplyToGraph(graph).AskResult);
    }

    [TestMethod]
    public void ShouldEvaluateAskWithTrailingValues()
    {
        RDFGraph graph = new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s"), new RDFResource("http://example.org/p"), new RDFResource("http://example.org/o")));

        //Trailing VALUES restricts the WHERE solution sequence: a matching binding keeps ASK true, a non-matching one makes it false
        Assert.IsTrue(RDFAskQuery.FromString("ASK { ?s ?p ?o } VALUES ?s { <http://example.org/s> }").ApplyToGraph(graph).AskResult);
        Assert.IsFalse(RDFAskQuery.FromString("ASK { ?s ?p ?o } VALUES ?s { <http://example.org/zzz> }").ApplyToGraph(graph).AskResult);
    }

    [TestMethod]
    public void ShouldThrowOnAskWithUnexpectedTrailingContent()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFAskQuery.FromString("ASK { ?s ?p ?o } SELECT"));

    #endregion
}
