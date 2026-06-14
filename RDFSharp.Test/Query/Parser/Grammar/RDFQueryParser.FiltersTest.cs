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

        Assert.IsInstanceOfType(SingleFilterOf(query), typeof(RDFExistsFilter));
    }

    [TestMethod]
    public void ShouldParseNotExistsFilter()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER NOT EXISTS { ?s <http://ex/q> ?z } }");

        Assert.IsInstanceOfType(SingleFilterOf(query), typeof(RDFNotExistsFilter));
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
    public void ShouldThrowOnNegationInsideValueExpression()
    {
        //'!' has no expression form: it is only valid at FILTER skeleton level, not inside a comparison operand
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o = !BOUND(?s)) }"));
    }

    [TestMethod]
    public void ShouldThrowOnMultiPatternExistsFilter()
    {
        //The engine's EXISTS filter carries a single triple pattern only
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER EXISTS { ?s <http://ex/q> ?z . ?z <http://ex/r> ?w } }"));
    }

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
    public void ShouldThrowOnNotInExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a FILTER(?a NOT IN (1, 2)) }"));

    [TestMethod]
    public void ShouldThrowOnDanglingNotInExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(?a NOT ?b) }"));
    #endregion
}
