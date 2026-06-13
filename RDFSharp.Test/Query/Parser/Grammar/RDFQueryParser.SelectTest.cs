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

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the SELECT-shape half of RDFQueryParser (the optional DISTINCT/REDUCED modifier, the
/// projection — <c>*</c> wildcard or variable list — and the round-trip oracle shared by the sibling partials).
/// The prologue, lexer, graph-pattern, triples, modifiers and FILTER/expression clusters live in the sibling
/// partial files mirroring the production split.
/// </summary>
[TestClass]
public partial class RDFQueryParserTest
{
    #region SelectQuery

    /// <summary>
    /// Asserts that printing the given query, parsing the print back, and printing the result yields the very
    /// same text: the round-trip oracle that proves the parser reconstructs the object-model faithfully.
    /// </summary>
    private static void AssertSelectQueryRoundTrips(RDFSelectQuery originalQuery)
    {
        string printedQuery = originalQuery.ToString();
        RDFSelectQuery reparsedQuery = RDFSelectQuery.FromString(printedQuery);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(printedQuery), RDFTestUtilities.NormalizeEOL(reparsedQuery.ToString()));
    }

    [TestMethod]
    public void ShouldRoundTripSelectStarWithSingleAllVariablePattern()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectWithProjectionVariablesAndMultiplePatterns()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("person"), RDFVocabulary.RDF.TYPE, RDFVocabulary.FOAF.PERSON))
                .AddPattern(new RDFPattern(new RDFVariable("person"), RDFVocabulary.FOAF.NAME, new RDFVariable("name"))))
            .AddProjectionVariable(new RDFVariable("person"))
            .AddProjectionVariable(new RDFVariable("name"));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectDistinct()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddProjectionVariable(new RDFVariable("s"))
            .AddModifier(new RDFDistinctModifier());

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectWithLiteralObjects()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("hello")))
                .AddPattern(new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("ciao", "it")))
                .AddPattern(new RDFPattern(new RDFVariable("s"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("42", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectWithMultiplePatternGroups()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("o"), new RDFVariable("q"), new RDFVariable("z"))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldParseSelectStarWithoutWhereKeyword()
    {
        //The WHERE keyword is optional in SPARQL: the group graph pattern braces are enough
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * { ?s ?p ?o }");

        Assert.AreEqual(0, query.ProjectionVars.Count);
        List<RDFPatternGroup> patternGroups = query.GetPatternGroups().ToList();
        Assert.AreEqual(1, patternGroups.Count);
        Assert.AreEqual(1, patternGroups[0].GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseSelectWithProjectionVariables()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?s ?o WHERE { ?s ?p ?o }");

        Assert.AreEqual(2, query.ProjectionVars.Count);
        Assert.IsTrue(query.ProjectionVars.Keys.Any(v => v.VariableName == "?S"));
        Assert.IsTrue(query.ProjectionVars.Keys.Any(v => v.VariableName == "?O"));
    }

    [TestMethod]
    public void ShouldParseVariablesWithDollarSigilInBody()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT $s WHERE { $s $p $o }");

        Assert.AreEqual(1, query.ProjectionVars.Count);
        RDFPattern pattern = query.GetPatternGroups().Single().GetPatterns().Single();
        Assert.AreEqual("?S", ((RDFVariable)pattern.Subject).VariableName);
        Assert.AreEqual("?P", ((RDFVariable)pattern.Predicate).VariableName);
        Assert.AreEqual("?O", ((RDFVariable)pattern.Object).VariableName);
    }

    [TestMethod]
    public void ShouldParseTheVerbAAsRdfType()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s a ?type }");

        RDFPattern pattern = query.GetPatternGroups().Single().GetPatterns().Single();
        Assert.IsInstanceOfType<RDFResource>(pattern.Predicate);
        Assert.AreEqual(RDFVocabulary.RDF.TYPE.ToString(), pattern.Predicate.ToString());
    }

    [TestMethod]
    public void ShouldParsePredicateObjectListWithSemicolon()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p1> ?o1 ; <http://example.org/p2> ?o2 }");

        List<RDFPattern> patterns = query.GetPatternGroups().Single().GetPatterns().ToList();
        Assert.AreEqual(2, patterns.Count);
        Assert.AreEqual("http://example.org/p1", patterns[0].Predicate.ToString());
        Assert.AreEqual("http://example.org/p2", patterns[1].Predicate.ToString());
        //Both predicate-object groups share the same subject
        Assert.AreEqual("?S", ((RDFVariable)patterns[0].Subject).VariableName);
        Assert.AreEqual("?S", ((RDFVariable)patterns[1].Subject).VariableName);
    }

    [TestMethod]
    public void ShouldParseObjectListWithComma()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p> ?o1 , ?o2 }");

        List<RDFPattern> patterns = query.GetPatternGroups().Single().GetPatterns().ToList();
        Assert.AreEqual(2, patterns.Count);
        //Both objects share the same subject and predicate
        Assert.AreEqual("http://example.org/p", patterns[0].Predicate.ToString());
        Assert.AreEqual("http://example.org/p", patterns[1].Predicate.ToString());
        Assert.AreEqual("?O1", ((RDFVariable)patterns[0].Object).VariableName);
        Assert.AreEqual("?O2", ((RDFVariable)patterns[1].Object).VariableName);
    }

    [TestMethod]
    public void ShouldParseKeywordsCaseInsensitively()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("select distinct ?s where { ?s ?p ?o } limit 5");

        Assert.AreEqual(1, query.ProjectionVars.Count);
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFDistinctModifier));
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFLimitModifier));
    }

    [TestMethod]
    public void ShouldTreatReducedAsNoOp()
    {
        //REDUCED is ratified leniency: it is recognized but produces no modifier
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT REDUCED ?s WHERE { ?s ?p ?o }");

        Assert.AreEqual(1, query.ProjectionVars.Count);
        Assert.IsFalse(query.GetModifiers().Any(m => m is RDFDistinctModifier));
    }

    [TestMethod]
    public void ShouldIgnoreCommentsWhileParsing()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } # trailing comment");

        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldReattachDeclaredPrefixesToParsedQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("PREFIX ex: <http://example.org/> SELECT * WHERE { ?s ex:knows ?o }");

        //The PREFIX from the prologue must be carried onto the query so it re-serializes its prologue
        Assert.IsTrue(query.GetPrefixes().Any(p => p.NamespacePrefix == "ex"));
        RDFPattern pattern = query.GetPatternGroups().Single().GetPatterns().Single();
        Assert.AreEqual("http://example.org/knows", pattern.Predicate.ToString());
    }

    [TestMethod]
    public void ShouldThrowOnNullOrEmptyQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("   "));

    [TestMethod]
    public void ShouldThrowOnEmptyProjection()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldRoundTripSelectWithProjectionExpression()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddProjectionVariable(new RDFVariable("s"))
            .AddProjectionVariable(new RDFVariable("sum"),
                new RDFAddExpression(new RDFVariable("s"), new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldParseProjectionExpression()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (STRLEN(?name) AS ?len) WHERE { ?s ?p ?name }");

        //The computed projection contributes one projection variable carrying its value-expression
        Assert.AreEqual(1, query.ProjectionVars.Count);
        KeyValuePair<RDFVariable, (int, RDFExpression)> projection = query.ProjectionVars.Single();
        Assert.AreEqual("?LEN", projection.Key.VariableName);
        Assert.IsNotNull(projection.Value.Item2);
    }

    [TestMethod]
    public void ShouldParseProjectionExpressionInterleavedWithBareVariable()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?s (?s AS ?copy) WHERE { ?s ?p ?o }");

        //A bare variable and a computed projection may be freely interleaved
        Assert.AreEqual(2, query.ProjectionVars.Count);
        Assert.IsTrue(query.ProjectionVars.Any(pv => pv.Key.VariableName == "?S" && pv.Value.Item2 == null));
        Assert.IsTrue(query.ProjectionVars.Any(pv => pv.Key.VariableName == "?COPY" && pv.Value.Item2 != null));
    }

    [TestMethod]
    public void ShouldThrowOnProjectionExpressionWithoutAs()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT (?s ?x) WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnProjectionExpressionWithoutResultVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT (?s AS x) WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnUnclosedWhereClause()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o "));

    [TestMethod]
    public void ShouldThrowOnMissingByAfterOrder()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } ORDER ?s"));

    [TestMethod]
    public void ShouldThrowOnLiteralInSubjectPosition()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { \"lit\" ?p ?o }"));
    #endregion
}
