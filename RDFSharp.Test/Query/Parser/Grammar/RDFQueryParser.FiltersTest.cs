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
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the FILTER half of RDFQueryParser: the boolean filter skeleton, the expression-precedence grammar and EXISTS/NOT EXISTS (structure and end-to-end execution).
/// </summary>
public partial class RDFQueryParserTest
{
    #region Filters

    //Helper: the filters of the single pattern group of a single-member WHERE.
    private static List<RDFFilter> FiltersOf(RDFSelectQuery query)
        => ((RDFPatternGroup)query.GetEvaluableQueryMembers().Single()).GetFilters().ToList();

    //Helper: the single filter of the single pattern group of a single-member WHERE.
    private static RDFFilter SingleFilterOf(RDFSelectQuery query)
        => FiltersOf(query).Single();

    [TestMethod]
    public void ShouldAbsorbFilterIntoTheSurroundingPatternGroup()
    {
        //FILTER is a RDFPatternGroupMember: it lands in the SAME pattern group as the triples around it
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://ex/p> ?o FILTER(?o > 2) }");

        RDFPatternGroup group = (RDFPatternGroup)query.GetEvaluableQueryMembers().Single();
        Assert.AreEqual(1, group.GetPatterns().Count());
        Assert.AreEqual(1, group.GetFilters().Count());
    }

    [TestMethod]
    public void ShouldParseComparisonFilterAsExpressionFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o >= 5) }");

        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        RDFComparisonExpression comparison = (RDFComparisonExpression)expressionFilter.Expression;
        Assert.AreEqual(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, comparison.ComparisonFlavor);
    }

    [TestMethod]
    public void ShouldParseConjunctionAsBooleanAndFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o > 2 && ?o < 9) }");

        RDFBooleanAndFilter andFilter = (RDFBooleanAndFilter)SingleFilterOf(query);
        Assert.IsInstanceOfType(andFilter.LeftFilter, typeof(RDFExpressionFilter));
        Assert.IsInstanceOfType(andFilter.RightFilter, typeof(RDFExpressionFilter));
    }

    [TestMethod]
    public void ShouldParseDisjunctionAsBooleanOrFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o = 1 || ?o = 2) }");

        Assert.IsInstanceOfType(SingleFilterOf(query), typeof(RDFBooleanOrFilter));
    }

    [TestMethod]
    public void ShouldGiveDisjunctionLowerPrecedenceThanConjunction()
    {
        //a || b && c parses as a || (b && c): the top node is the OR, whose right side is the AND
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o = 1 || ?o > 2 && ?o < 9) }");

        RDFBooleanOrFilter orFilter = (RDFBooleanOrFilter)SingleFilterOf(query);
        Assert.IsInstanceOfType(orFilter.LeftFilter, typeof(RDFExpressionFilter));
        Assert.IsInstanceOfType(orFilter.RightFilter, typeof(RDFBooleanAndFilter));
    }

    [TestMethod]
    public void ShouldParseNegationAsBooleanNotFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(!BOUND(?o)) }");

        RDFBooleanNotFilter notFilter = (RDFBooleanNotFilter)SingleFilterOf(query);
        RDFExpressionFilter inner = (RDFExpressionFilter)notFilter.Filter;
        Assert.IsInstanceOfType(inner.Expression, typeof(RDFBoundExpression));
    }

    [TestMethod]
    public void ShouldParseInExpressionFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o IN (1, 2, 3)) }");

        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        Assert.IsInstanceOfType(expressionFilter.Expression, typeof(RDFInExpression));
    }

    [TestMethod]
    public void ShouldParseExistsFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER EXISTS { ?s <http://ex/q> ?z } }");

        //EXISTS is now a value-expression wrapped into a plain expression filter
        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        Assert.IsInstanceOfType(expressionFilter.Expression, typeof(RDFExistsExpression));
    }

    [TestMethod]
    public void ShouldParseNotExistsFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER NOT EXISTS { ?s <http://ex/q> ?z } }");

        //NOT EXISTS is modelled as the negation of an EXISTS expression
        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        RDFNotExpression notExpression = (RDFNotExpression)expressionFilter.Expression;
        Assert.IsInstanceOfType(notExpression.LeftArgument, typeof(RDFExistsExpression));
    }

    [TestMethod]
    public void ShouldParseArithmeticInsideComparison()
    {
        //(?x + 1) is a bracketed additive expression used as the left side of a comparison
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?x FILTER((?x + 1) > 10) }");

        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        RDFComparisonExpression comparison = (RDFComparisonExpression)expressionFilter.Expression;
        Assert.IsInstanceOfType(comparison.LeftArgument, typeof(RDFAddExpression));
    }

    [TestMethod]
    public void ShouldParseMultiplicativeWithHigherPrecedenceThanAdditive()
    {
        //?x + ?y * 2 parses as ?x + (?y * 2): the additive right side is the multiplication
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?x FILTER(?x + ?y * 2 = 0) }");

        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        RDFComparisonExpression comparison = (RDFComparisonExpression)expressionFilter.Expression;
        RDFAddExpression addition = (RDFAddExpression)comparison.LeftArgument;
        Assert.IsInstanceOfType(addition.RightArgument, typeof(RDFMultiplyExpression));
    }

    [TestMethod]
    public void ShouldParseTwoSeparateFiltersIntoOneGroup()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o > 1) FILTER(?o < 9) }");

        Assert.AreEqual(2, FiltersOf(query).Count);
    }

    [TestMethod]
    public void ShouldParseNegationInsideValueExpression()
    {
        //'!' now has an expression form (RDFNotExpression): it can be nested inside a value-expression operand
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o = !BOUND(?s)) }");

        List<RDFFilter> filters = FiltersOf(query);
        Assert.AreEqual(1, filters.Count);
        Assert.IsInstanceOfType(filters[0], typeof(RDFExpressionFilter));
        RDFComparisonExpression comparison = (RDFComparisonExpression)((RDFExpressionFilter)filters[0]).Expression;
        Assert.IsInstanceOfType(comparison.RightArgument, typeof(RDFNotExpression));
    }

    [TestMethod]
    public void ShouldParseMultiPatternExistsFilter()
    {
        //EXISTS now carries a full group graph pattern: a multi-triple body is a pattern group
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER EXISTS { ?s <http://ex/q> ?z . ?z <http://ex/r> ?w } }");

        RDFExistsExpression existsExpression = (RDFExistsExpression)((RDFExpressionFilter)SingleFilterOf(query)).Expression;
        Assert.IsInstanceOfType(existsExpression.GroupGraphPattern, typeof(RDFPatternGroup));
        Assert.AreEqual(2, ((RDFPatternGroup)existsExpression.GroupGraphPattern).GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseUnionInsideExistsFilter()
    {
        //A UNION at the head of the EXISTS group parses to a binary tree, represented as a SubSelect
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER EXISTS { { ?s <http://ex/q> ?z } UNION { ?s <http://ex/r> ?z } } }");

        RDFExistsExpression existsExpression = (RDFExistsExpression)((RDFExpressionFilter)SingleFilterOf(query)).Expression;
        Assert.IsInstanceOfType(existsExpression.GroupGraphPattern, typeof(RDFSelectQuery));
    }

    [TestMethod]
    public void ShouldParseOptionalInsideExistsFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER EXISTS { ?s <http://ex/q> ?z . OPTIONAL { ?z <http://ex/r> ?w } } }");

        RDFExistsExpression existsExpression = (RDFExistsExpression)((RDFExpressionFilter)SingleFilterOf(query)).Expression;
        Assert.IsInstanceOfType(existsExpression.GroupGraphPattern, typeof(RDFSelectQuery));
    }

    [TestMethod]
    public void ShouldParseSubSelectInsideExistsFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER EXISTS { SELECT ?s WHERE { ?s <http://ex/q> ?z } } }");

        RDFExistsExpression existsExpression = (RDFExistsExpression)((RDFExpressionFilter)SingleFilterOf(query)).Expression;
        Assert.IsInstanceOfType(existsExpression.GroupGraphPattern, typeof(RDFSelectQuery));
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateMultiPatternExistsIso()
    {
        //Sample graph: alice -knows-> bob -age-> 30 ; carol -knows-> dave (dave has no age)
        RDFGraph graph = new RDFGraph();
        RDFResource alice = new RDFResource("http://ex/alice");
        RDFResource bob = new RDFResource("http://ex/bob");
        RDFResource carol = new RDFResource("http://ex/carol");
        RDFResource dave = new RDFResource("http://ex/dave");
        RDFResource knows = new RDFResource("http://ex/knows");
        RDFResource age = new RDFResource("http://ex/age");
        graph.AddTriple(new RDFTriple(alice, knows, bob));
        graph.AddTriple(new RDFTriple(carol, knows, dave));
        graph.AddTriple(new RDFTriple(bob, age, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        //EXISTS over a two-triple group correlated on ?friend: keep ?person whose friend has an age
        RDFSelectQuery apiQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?friend")))
                .AddFilter(new RDFExpressionFilter(new RDFExistsExpression(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?friend"), age, new RDFVariable("?friendAge"))))))
                .AddPattern(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?friend"))));

        RDFTestUtilities.AssertIso(apiQuery, graph);
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateMultiPatternNotExistsIso()
    {
        RDFGraph graph = new RDFGraph();
        RDFResource alice = new RDFResource("http://ex/alice");
        RDFResource bob = new RDFResource("http://ex/bob");
        RDFResource carol = new RDFResource("http://ex/carol");
        RDFResource dave = new RDFResource("http://ex/dave");
        RDFResource knows = new RDFResource("http://ex/knows");
        RDFResource age = new RDFResource("http://ex/age");
        graph.AddTriple(new RDFTriple(alice, knows, bob));
        graph.AddTriple(new RDFTriple(carol, knows, dave));
        graph.AddTriple(new RDFTriple(bob, age, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        //NOT EXISTS: keep ?person whose friend has NO age (carol -> dave)
        RDFSelectQuery apiQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?friend")))
                .AddFilter(new RDFExpressionFilter(new RDFNotExpression(new RDFExistsExpression(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?friend"), age, new RDFVariable("?friendAge")))))))
                .AddPattern(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?friend"))));

        RDFTestUtilities.AssertIso(apiQuery, graph);
    }

    #region Tests (EXISTS as composable expression — IP12)
    [TestMethod]
    public void ShouldComposeExistsWithBooleanConnectiveInFilter()
    {
        //Previously rejected: EXISTS combined with another condition via '&&' (EXISTS is now an expression)
        RDFGraph graph = BuildPeopleGraph();
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/age> ?a FILTER(EXISTS { ?s <http://ex/name> ?n } && ?a > 18) }");

        DataTable results = query.ApplyToGraph(graph).SelectResults;

        //Only alice has both a name AND age > 18 (carol has no name, bob is under 18)
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://ex/alice", results.Rows[0]["?S"].ToString());
        RDFTestUtilities.AssertIso(query, graph);
    }

    [TestMethod]
    public void ShouldComposeNotExistsWithBooleanConnectiveInFilter()
    {
        RDFGraph graph = BuildPeopleGraph();
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/age> ?a FILTER(NOT EXISTS { ?s <http://ex/name> ?n } && ?a > 18) }");

        DataTable results = query.ApplyToGraph(graph).SelectResults;

        //Only carol has age > 18 AND no name
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://ex/carol", results.Rows[0]["?S"].ToString());
        RDFTestUtilities.AssertIso(query, graph);
    }

    [TestMethod]
    public void ShouldBindExistsExpression()
    {
        //Previously rejected: EXISTS inside a BIND
        RDFGraph graph = BuildPeopleGraph();
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT ?s ?hasName WHERE { ?s <http://ex/age> ?a BIND(EXISTS { ?s <http://ex/name> ?n } AS ?hasName) }");

        DataTable results = query.ApplyToGraph(graph).SelectResults;

        Assert.AreEqual(3, results.Rows.Count);
        //carol is the only subject without a name => bound to 'false'
        DataRow carolRow = results.Select("[?S] = 'http://ex/carol'").Single();
        Assert.IsTrue(carolRow["?hasName"].ToString().Contains("false"));
        RDFTestUtilities.AssertIso(query, graph);
    }

    [TestMethod]
    public void ShouldProjectExistsExpression()
    {
        //Previously rejected: EXISTS inside a projection expression '(expr AS ?v)'
        RDFGraph graph = BuildPeopleGraph();
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT ?s (EXISTS { ?s <http://ex/name> ?n } AS ?hasName) WHERE { ?s <http://ex/age> ?a }");

        DataTable results = query.ApplyToGraph(graph).SelectResults;

        Assert.AreEqual(3, results.Rows.Count);
        DataRow aliceRow = results.Select("[?S] = 'http://ex/alice'").Single();
        Assert.IsTrue(aliceRow["?hasName"].ToString().Contains("true"));
        RDFTestUtilities.AssertIso(query, graph);
    }

    [TestMethod]
    public void ShouldOrderByExistsExpression()
    {
        //Previously rejected: EXISTS inside an ORDER BY key
        RDFGraph graph = BuildPeopleGraph();
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/age> ?a } ORDER BY EXISTS { ?s <http://ex/name> ?n }");

        DataTable results = query.ApplyToGraph(graph).SelectResults;

        //Ascending boolean: the name-less subject (false) sorts first
        Assert.AreEqual(3, results.Rows.Count);
        Assert.AreEqual("http://ex/carol", results.Rows[0]["?S"].ToString());
        RDFTestUtilities.AssertIso(query, graph);
    }

    [TestMethod]
    public void ShouldUseExistsInsideHaving()
    {
        //Previously rejected: EXISTS inside a HAVING condition
        RDFGraph graph = BuildPeopleGraph();
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/age> ?a } GROUP BY ?s HAVING(EXISTS { ?s <http://ex/name> ?n })");

        DataTable results = query.ApplyToGraph(graph).SelectResults;

        //Only the named subjects (alice, bob) survive the HAVING
        Assert.AreEqual(2, results.Rows.Count);
        List<string> keptSubjects = results.Rows.Cast<DataRow>().Select(r => r["?S"].ToString()).OrderBy(s => s).ToList();
        CollectionAssert.AreEqual(new List<string> { "http://ex/alice", "http://ex/bob" }, keptSubjects);
    }
    #endregion

    //Helper: a small graph with integer ages and string names for filter-execution tests.
    private static RDFGraph BuildPeopleGraph()
    {
        RDFGraph graph = new RDFGraph();
        RDFResource alice = new RDFResource("http://ex/alice");
        RDFResource bob = new RDFResource("http://ex/bob");
        RDFResource carol = new RDFResource("http://ex/carol");
        RDFResource age = new RDFResource("http://ex/age");
        RDFResource name = new RDFResource("http://ex/name");
        graph.AddTriple(new RDFTriple(alice, age, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        graph.AddTriple(new RDFTriple(bob, age, new RDFTypedLiteral("17", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        graph.AddTriple(new RDFTriple(carol, age, new RDFTypedLiteral("42", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        graph.AddTriple(new RDFTriple(alice, name, new RDFPlainLiteral("Alice")));
        graph.AddTriple(new RDFTriple(bob, name, new RDFPlainLiteral("Bob")));
        return graph;
    }

    [TestMethod]
    public void ShouldExecuteNumericComparisonFilter()
    {
        RDFGraph graph = BuildPeopleGraph();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/age> ?a FILTER(?a >= 18) }")
            .ApplyToGraph(graph).SelectResults;

        string[] subjects = results.AsEnumerable()
            .Select(row => row.Field<string>("?s"))
            .OrderBy(s => s, System.StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(new[] { "http://ex/alice", "http://ex/carol" }, subjects);
    }

    [TestMethod]
    public void ShouldExecuteConjunctiveRangeFilter()
    {
        RDFGraph graph = BuildPeopleGraph();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/age> ?a FILTER(?a > 18 && ?a < 40) }")
            .ApplyToGraph(graph).SelectResults;

        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://ex/alice", results.Rows[0].Field<string>("?s"));
    }

    [TestMethod]
    public void ShouldExecuteRegexFilter()
    {
        RDFGraph graph = BuildPeopleGraph();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/name> ?n FILTER(REGEX(?n, \"^a\", \"i\")) }")
            .ApplyToGraph(graph).SelectResults;

        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://ex/alice", results.Rows[0].Field<string>("?s"));
    }

    [TestMethod]
    public void ShouldExecuteNotExistsFilter()
    {
        //People who have an age but no name: carol
        RDFGraph graph = BuildPeopleGraph();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/age> ?a FILTER NOT EXISTS { ?s <http://ex/name> ?n } }")
            .ApplyToGraph(graph).SelectResults;

        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://ex/carol", results.Rows[0].Field<string>("?s"));
    }

    [TestMethod]
    public void ShouldExecuteInFilter()
    {
        RDFGraph graph = BuildPeopleGraph();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { ?s <http://ex/age> ?a FILTER(?a IN (17, 42)) }")
            .ApplyToGraph(graph).SelectResults;

        string[] subjects = results.AsEnumerable()
            .Select(row => row.Field<string>("?s"))
            .OrderBy(s => s, System.StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(new[] { "http://ex/bob", "http://ex/carol" }, subjects);
    }

    /// <summary>
    /// Covers EACH relational comparison operator in a FILTER (the two-character operators must win over their
    /// one-character prefixes), checking the parsed flavor. Some operands are prefixed IRIs / typed literals.
    /// </summary>
    [TestMethod]
    [DataRow("?a < 10", RDFQueryEnums.RDFComparisonFlavors.LessThan)]
    [DataRow("?a <= 10", RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan)]
    [DataRow("?a > 10", RDFQueryEnums.RDFComparisonFlavors.GreaterThan)]
    [DataRow("?a >= 10", RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan)]
    [DataRow("?o = foaf:Person", RDFQueryEnums.RDFComparisonFlavors.EqualTo)]
    [DataRow("?o != foaf:Person", RDFQueryEnums.RDFComparisonFlavors.NotEqualTo)]
    [DataRow("?a >= \"5\"^^xsd:integer", RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan)]
    public void ShouldParseRelationalComparisonOperator(string constraint, RDFQueryEnums.RDFComparisonFlavors expectedFlavor)
    {
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s ?p ?a . ?s ?q ?o FILTER({constraint}) }}");

        RDFComparisonExpression comparison = (RDFComparisonExpression)((RDFExpressionFilter)SingleFilterOf(query)).Expression;
        Assert.AreEqual(expectedFlavor, comparison.ComparisonFlavor);
    }

    /// <summary>
    /// Covers EACH arithmetic operator (additive +/-, multiplicative *//, and unary minus) used inside a FILTER
    /// comparison, checking the parsed left-hand arithmetic expression type.
    /// </summary>
    [TestMethod]
    [DataRow("?a + 1 = 2", typeof(RDFAddExpression))]
    [DataRow("?a - 1 = 0", typeof(RDFSubtractExpression))]
    [DataRow("?a * 2 = 4", typeof(RDFMultiplyExpression))]
    [DataRow("?a / 2 = 1", typeof(RDFDivideExpression))]
    [DataRow("- ?a = 0", typeof(RDFSubtractExpression))]
    public void ShouldParseArithmeticOperatorInComparison(string constraint, System.Type expectedLeftType)
    {
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s ?p ?a FILTER({constraint}) }}");

        RDFComparisonExpression comparison = (RDFComparisonExpression)((RDFExpressionFilter)SingleFilterOf(query)).Expression;
        Assert.IsInstanceOfType(comparison.LeftArgument, expectedLeftType);
    }

    [TestMethod]
    public void ShouldParseInExpressionWithPrefixedIris()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?o FILTER(?o IN (foaf:Person, foaf:Agent, foaf:Group)) }");

        Assert.IsInstanceOfType(((RDFExpressionFilter)SingleFilterOf(query)).Expression, typeof(RDFInExpression));
    }

    [TestMethod]
    public void ShouldParseNotInExpression()
    {
        //'NOT IN' is now modelled as '!( … IN … )' via the logical-negation expression
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a FILTER(?a NOT IN (1, 2)) }");

        RDFExpression expression = ((RDFExpressionFilter)SingleFilterOf(query)).Expression;
        Assert.IsInstanceOfType(expression, typeof(RDFNotExpression));
        Assert.IsInstanceOfType(((RDFNotExpression)expression).LeftArgument, typeof(RDFInExpression));
    }

    [TestMethod]
    public void ShouldThrowOnDanglingNotInExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(?a NOT ?b) }"));
    #endregion

    #region IP5.4 — FILTER group-graph-pattern scope

    //Shared dataset for the scope tests. Each subject carries an integer "weight" under one or more of the set
    //membership predicates inA/inB/inC, plus a generic :p marker. The weights make the FILTER outcome observable:
    //  e1  inA=1
    //  e2  inA=2  inC=9
    //  e3  inB=3
    //  e4  inB=4  inC=9
    //  e5  inA=5  inB=5
    //All five carry ":p 0" so "?s :p ?o" binds every subject (used as the mandatory part of OPTIONAL tests).
    private static RDFGraph BuildFilterScopeDataset()
    {
        RDFResource predP = new RDFResource("http://ex/p");
        RDFResource predInA = new RDFResource("http://ex/inA");
        RDFResource predInB = new RDFResource("http://ex/inB");
        RDFResource predInC = new RDFResource("http://ex/inC");

        RDFGraph dataset = new RDFGraph();
        void Weight(string subjectLocalName, RDFResource predicate, int weight)
            => dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/" + subjectLocalName), predicate,
                                               new RDFTypedLiteral(weight.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        foreach (string subjectLocalName in new[] { "e1", "e2", "e3", "e4", "e5" })
            dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/" + subjectLocalName), predP,
                                            new RDFTypedLiteral("0", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Weight("e1", predInA, 1);
        Weight("e2", predInA, 2); Weight("e2", predInC, 9);
        Weight("e3", predInB, 3);
        Weight("e4", predInB, 4); Weight("e4", predInC, 9);
        Weight("e5", predInA, 5); Weight("e5", predInB, 5);
        return dataset;
    }

    //Helper: parse + execute the query on the dataset, returning the bound values of ?s sorted (bag semantics:
    //duplicates are preserved, so multiset cardinality is observable for union/minus tests).
    private static string[] FilterScopeSubjects(string sparqlQuery, RDFGraph dataset)
        => RDFSelectQuery.FromString(sparqlQuery).ApplyToGraph(dataset).SelectResults
                         .AsEnumerable()
                         .Select(row => row.Field<string>("?s"))
                         .OrderBy(subjectValue => subjectValue, System.StringComparer.Ordinal)
                         .ToArray();

    private const string ExPrefix = "PREFIX ex: <http://ex/> ";

    // ---- Semantics: FILTER reaching across OPTIONAL ----

    [TestMethod]
    public void ShouldScopeFilterOverOptionalBoundVariable()
    {
        //THE REGRESSION CASE: FILTER(?v>2) sits AFTER an OPTIONAL that binds ?v. Before IP5.4 the filter landed in
        //its own filter-only pattern group (no ?v there) and was silently dropped. Now it ranges over the whole
        //group: e5(5) passes, e1(1)/e2(2) fail, e3/e4 (no inA → ?v unbound → error) fail. Only e5 survives.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o . OPTIONAL { ?s ex:inA ?v } FILTER(?v > 2) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e5" }, subjects);
    }

    [TestMethod]
    public void ShouldScopeFilterWrittenBeforeOptionalThatBindsItsVariable()
    {
        //NON-POSITIONALITY: the FILTER is written BEFORE the OPTIONAL that binds ?v, yet (unlike BIND) it still sees
        //?v because a FILTER ranges over the whole group regardless of textual position. Same result as above.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o . FILTER(?v > 2) OPTIONAL { ?s ex:inA ?v } }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e5" }, subjects);
    }

    [TestMethod]
    public void ShouldKeepFilterInsideOptionalScopedToTheOptionalGroup()
    {
        //CONTROL: a FILTER written INSIDE the OPTIONAL belongs to the optional's inner group and is applied BEFORE
        //the left-join (it filters the optional matches, it does NOT prune the mandatory rows). With ?w>10 it
        //removes every inB match, so all of e1..e5 survive with ?w unbound. Were it (wrongly) hoisted to the outer
        //group, ?w would be unbound everywhere → error → ZERO rows. Getting 5 rows proves the inner scope.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o OPTIONAL { ?s ex:inB ?w FILTER(?w > 10) } }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e1", "http://ex/e2", "http://ex/e3", "http://ex/e4", "http://ex/e5" }, subjects);
    }

    // ---- Semantics: FILTER over UNION / MINUS ----

    [TestMethod]
    public void ShouldScopeFilterOverUnionBranchesInBagSemantics()
    {
        //FILTER(?v>2) ranges over the whole UNION result (both branches), bag semantics preserved.
        //inA rows: (e1,1)(e2,2)(e5,5); inB rows: (e3,3)(e4,4)(e5,5). Bag union keeps both e5 rows.
        //Keeping ?v>2: e5(5), e3(3), e4(4), e5(5) → subjects {e3,e4,e5,e5}.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { { ?s ex:inA ?v } UNION { ?s ex:inB ?v } FILTER(?v > 2) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e3", "http://ex/e4", "http://ex/e5", "http://ex/e5" }, subjects);
    }

    [TestMethod]
    public void ShouldScopeFilterAfterMinus()
    {
        //Group algebra: Filter(?v>1, Minus({inA}, {inC})). inA = (e1,1)(e2,2)(e5,5); inC subjects = {e2,e4}.
        //Minus removes left rows whose ?s is compatible with the right → removes e2 → {(e1,1),(e5,5)}.
        //Then FILTER(?v>1): e1(1) fails, e5(5) passes → {e5}.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { { ?s ex:inA ?v } MINUS { ?s ex:inC ?c } FILTER(?v > 1) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e5" }, subjects);
    }

    // ---- Semantics: FILTER over sub-select / nested group / BIND / VALUES ----

    [TestMethod]
    public void ShouldScopeFilterOverSubSelectProjectedVariable()
    {
        //FILTER(?v>1) ranges over a group made of a single sub-select projecting ?s,?v from inA = (e1,1)(e2,2)(e5,5).
        //Keeping ?v>1 → {e2,e5}.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { { SELECT ?s ?v WHERE { ?s ex:inA ?v } } FILTER(?v > 1) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e2", "http://ex/e5" }, subjects);
    }

    [TestMethod]
    public void ShouldScopeFilterOverSiblingNestedGroupVariable()
    {
        //FILTER(?w>3) references ?w bound by a sibling nested group { ?s ex:inB ?w }. Only e5 is in both inA and inB
        //(join on ?s), with inB weight 5 → passes ?w>3 → {e5}.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v . { ?s ex:inB ?w } FILTER(?w > 3) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e5" }, subjects);
    }

    [TestMethod]
    public void ShouldScopeFilterOverBindVariableAcrossOptional()
    {
        //BIND(?v*2 AS ?w) computes ?w in the first BGP; the group-level FILTER(?w>6) (placed past an OPTIONAL) sees it.
        //inA: e1(1→2) e2(2→4) e5(5→10). Keeping ?w>6 → only e5.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v . BIND((?v * 2) AS ?w) OPTIONAL { ?s ex:inC ?c } FILTER(?w > 6) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e5" }, subjects);
    }

    [TestMethod]
    public void ShouldScopeFilterOverValuesVariableAcrossOptional()
    {
        //VALUES binds ?lim=3; the group-level FILTER(?v>?lim) (past an OPTIONAL binding ?v) compares the two.
        //inA: e1(1) e2(2) e5(5). Keeping ?v>3 → only e5.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o VALUES ?lim { 3 } OPTIONAL { ?s ex:inA ?v } FILTER(?v > ?lim) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e5" }, subjects);
    }

    // ---- Semantics: multiple filters, EXISTS / NOT EXISTS ----

    [TestMethod]
    public void ShouldConjoinMultipleGroupFilters()
    {
        //Two FILTERs in the same group are ANDed and both range over the whole group.
        //inA: e1(1) e2(2) e5(5). Keeping ?v>=1 AND ?v<5 → {e1,e2}.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v OPTIONAL { ?s ex:inB ?w } FILTER(?v >= 1) FILTER(?v < 5) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e1", "http://ex/e2" }, subjects);
    }

    [TestMethod]
    public void ShouldScopeFilterExistsAtGroupLevel()
    {
        //FILTER EXISTS { ?s ex:inC ?c } past an OPTIONAL: ranges over the whole group. inA = {e1,e2,e5};
        //inC subjects = {e2,e4} → only e2 has an inC triple → {e2}.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v OPTIONAL { ?s ex:inB ?w } FILTER EXISTS { ?s ex:inC ?c } }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e2" }, subjects);
    }

    [TestMethod]
    public void ShouldScopeFilterNotExistsAtGroupLevel()
    {
        //Dual of the previous test: NOT EXISTS keeps the inA subjects WITHOUT an inC triple → {e1,e5}.
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v OPTIONAL { ?s ex:inB ?w } FILTER NOT EXISTS { ?s ex:inC ?c } }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e1", "http://ex/e5" }, subjects);
    }

    [TestMethod]
    public void ShouldScopeInnerAndOuterFiltersToTheirOwnGroups()
    {
        //Nested groups, two filters at two different scopes:
        //  inner { ?s ex:inA ?v FILTER(?v>1) } → lone BGP filter → {e2,e5}
        //  outer FILTER(?v<5) ranges over the outer group → drops e5 → {e2}
        string[] subjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { { ?s ex:inA ?v FILTER(?v > 1) } FILTER(?v < 5) }",
            BuildFilterScopeDataset());

        CollectionAssert.AreEqual(new[] { "http://ex/e2" }, subjects);
    }

    // ---- Model placement ----

    [TestMethod]
    public void ShouldKeepLoneBasicGraphPatternFilterOnThePatternGroup()
    {
        //A lone basic graph pattern: the group coincides with the BGP, so the filter stays ON the pattern group
        //(no WHERE-clause-scoped filter), preserving the historical model and the round-trip.
        RDFSelectQuery query = RDFSelectQuery.FromString(ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v FILTER(?v > 2) }");

        Assert.AreEqual(0, query.QueryFilters.Count);
        RDFPatternGroup patternGroup = (RDFPatternGroup)query.GetEvaluableQueryMembers().Single();
        Assert.AreEqual(1, patternGroup.GetFilters().Count());
    }

    [TestMethod]
    public void ShouldHoistFilterToWhereClauseScopeForCompoundTopLevelGroup()
    {
        //A compound top-level group (BGP + OPTIONAL): the filter is hoisted to WHERE-clause scope (query.QueryFilters)
        //and NOT left on any pattern group.
        RDFSelectQuery query = RDFSelectQuery.FromString(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o OPTIONAL { ?s ex:inA ?v } FILTER(?v > 2) }");

        Assert.AreEqual(1, query.QueryFilters.Count);
        Assert.IsFalse(query.GetPatternGroups().Any(pg => pg.GetFilters().Any()));
    }

    [TestMethod]
    public void ShouldHoistFilterIntoWrappingSubQueryForCompoundNestedGroup()
    {
        //A compound NESTED group { BGP OPTIONAL FILTER } is wrapped into a SELECT * subquery carrying the filter at
        //its own WHERE-clause scope, so the outer query sees a single subquery member with no stray group filter.
        RDFSelectQuery query = RDFSelectQuery.FromString(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o . { ?x ex:p ?y OPTIONAL { ?x ex:inA ?v } FILTER(?v > 2) } }");

        RDFSelectQuery wrappingSubQuery = (RDFSelectQuery)query.GetSubQueries().Single();
        Assert.AreEqual(1, wrappingSubQuery.QueryFilters.Count);
    }

    // ---- Round-trip isofunctionality (Gate A + Gate B) ----

    [TestMethod]
    public void ShouldRoundTripAndEvaluateFilterOverOptionalIso()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o OPTIONAL { ?s ex:inA ?v } FILTER(?v > 2) }");
        RDFTestUtilities.AssertIso(query, BuildFilterScopeDataset());
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateFilterOverUnionIso()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            ExPrefix + "SELECT ?s WHERE { { ?s ex:inA ?v } UNION { ?s ex:inB ?v } FILTER(?v > 2) }");
        RDFTestUtilities.AssertIso(query, BuildFilterScopeDataset());
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateFilterAfterMinusIso()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            ExPrefix + "SELECT ?s WHERE { { ?s ex:inA ?v } MINUS { ?s ex:inC ?c } FILTER(?v > 1) }");
        RDFTestUtilities.AssertIso(query, BuildFilterScopeDataset());
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateFilterExistsAtGroupLevelIso()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v OPTIONAL { ?s ex:inB ?w } FILTER EXISTS { ?s ex:inC ?c } }");
        RDFTestUtilities.AssertIso(query, BuildFilterScopeDataset());
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateLoneBasicGraphPatternFilterIso()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v FILTER(?v > 2) }");
        RDFTestUtilities.AssertIso(query, BuildFilterScopeDataset());
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateMultipleGroupFiltersIso()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            ExPrefix + "SELECT ?s WHERE { ?s ex:inA ?v OPTIONAL { ?s ex:inB ?w } FILTER(?v >= 1) FILTER(?v < 5) }");
        RDFTestUtilities.AssertIso(query, BuildFilterScopeDataset());
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateFilterOverValuesIso()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o VALUES ?lim { 3 } OPTIONAL { ?s ex:inA ?v } FILTER(?v > ?lim) }");
        RDFTestUtilities.AssertIso(query, BuildFilterScopeDataset());
    }

    // ---- Fluent-API ⇄ parser isofunctionality ----

    [TestMethod]
    public void ShouldBuildWhereClauseScopedFilterViaFluentApiEquivalentToParser()
    {
        //Build, via the fluent API, the same compound group as the parser produces: a mandatory BGP, an OPTIONAL,
        //and a WHERE-clause-scoped filter added with RDFSelectQuery.AddFilter. It must round-trip AND evaluate
        //identically to the SPARQL string (isofunctionality: anything doable via SPARQL is doable via the API).
        RDFSelectQuery fluentQuery = new RDFSelectQuery()
            .AddPrefix(new RDFNamespace("ex", "http://ex/"))
            .AddProjectionVariable(new RDFVariable("?s"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?s"), new RDFResource("http://ex/p"), new RDFVariable("?o"))))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?s"), new RDFResource("http://ex/inA"), new RDFVariable("?v")))
                .Optional())
            .AddFilter(new RDFExpressionFilter(new RDFComparisonExpression(
                RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                new RDFVariableExpression(new RDFVariable("?v")),
                new RDFConstantExpression(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));

        //Gate A + Gate B against the equivalent SPARQL string
        RDFTestUtilities.AssertIso(fluentQuery, BuildFilterScopeDataset());

        //Direct evaluation equivalence with the hand-written SPARQL form
        string[] fluentSubjects = fluentQuery.ApplyToGraph(BuildFilterScopeDataset()).SelectResults
                                             .AsEnumerable().Select(row => row.Field<string>("?s"))
                                             .OrderBy(s => s, System.StringComparer.Ordinal).ToArray();
        string[] parsedSubjects = FilterScopeSubjects(
            ExPrefix + "SELECT ?s WHERE { ?s ex:p ?o OPTIONAL { ?s ex:inA ?v } FILTER(?v > 2) }",
            BuildFilterScopeDataset());
        CollectionAssert.AreEqual(parsedSubjects, fluentSubjects);
        CollectionAssert.AreEqual(new[] { "http://ex/e5" }, fluentSubjects);
    }

    #endregion
}
