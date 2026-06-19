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
/// Unit tests for the grouping half of RDFQueryParser: the GROUP BY clause (partitioning variables absorbing
/// the projection's aggregates) and the HAVING clause (restricted (AGGREGATE OP value) comparisons attached as
/// having-clauses), plus the model-imposed failures (non-variable GroupConditions, disjunctive/non-projected HAVING).
/// </summary>
public partial class RDFQueryParserTest
{
    #region GroupBy

    [TestMethod]
    public void ShouldRoundTripGroupByWithMultiplePartitionVariables()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("a"), new RDFVariable("b"), new RDFVariable("x"))))
            .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("a"), new RDFVariable("b") })
                .AddAggregator(new RDFCountAggregator(new RDFVariable("x"), new RDFVariable("cnt"))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldEvaluateLegacyPerAggregatorHavingClause()
    {
        //The legacy fluent SetHavingClause (per-aggregator '(AGGREGATE OP value)') is preserved for retro-compat:
        //it still builds, prints and FILTERS correctly (only groups whose AVG age is >= 30 survive)
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("e"), new RDFResource("http://example.org/dept"), new RDFVariable("c")))
                .AddPattern(new RDFPattern(new RDFVariable("e"), new RDFResource("http://example.org/age"), new RDFVariable("g"))))
            .AddProjectionVariable(new RDFVariable("c"))
            .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFAvgAggregator(new RDFVariable("g"), new RDFVariable("avg"))
                    .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                                     new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));

        Assert.IsTrue(query.ToString().Contains("HAVING ((AVG(?G) >= "));
        DataTable results = query.ApplyToGraph(BuildAggregationSampleGraph()).SelectResults;
        //Dept A averages (20+30)/2=25 (dropped); dept B averages 40 (kept)
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://example.org/deptB", results.Rows[0]["?C"].ToString());
    }

    [TestMethod]
    public void ShouldRoundTripGroupByWithFreeHavingExpression()
    {
        //The new fluent SetHavingExpression accepts a full boolean expression; an aggregate is referenced simply by
        //its projection variable (here '?avg' for the AVG aggregator) — the easy, public way to reference aggregates
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("e"), new RDFResource("http://example.org/dept"), new RDFVariable("c")))
                .AddPattern(new RDFPattern(new RDFVariable("e"), new RDFResource("http://example.org/age"), new RDFVariable("g"))))
            .AddProjectionVariable(new RDFVariable("c"))
            .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFAvgAggregator(new RDFVariable("g"), new RDFVariable("avg")))
                .SetHavingExpression(new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("avg")),
                    new RDFConstantExpression(new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));

        RDFTestUtilities.AssertIso(query, BuildAggregationSampleGraph());
    }

    [TestMethod]
    public void ShouldParseHavingClauseFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING (AVG(?g) >= 24)");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsNotNull(groupByModifier.HavingExpression);
        //AVG(?g) is projected as ?avg, so HAVING references that (projected) column by its alias
        Assert.IsTrue(query.ToString().Contains("HAVING ((?AVG >= 24))"));
    }

    [TestMethod]
    public void ShouldEvaluateHavingClauseFromHandWrittenQuery()
    {
        //A projected aggregate referenced by HAVING reuses its column: only dept B (avg 40) passes '>= 30'
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (AVG(?g) AS ?avg) WHERE { ?e <http://example.org/dept> ?c . ?e <http://example.org/age> ?g } GROUP BY ?c HAVING (AVG(?g) >= 30)");

        DataTable results = query.ApplyToGraph(BuildAggregationSampleGraph()).SelectResults;
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://example.org/deptB", results.Rows[0]["?C"].ToString());
    }

    [TestMethod]
    public void ShouldParseHavingClauseWithDoubleParentheses()
    {
        //The printer wraps each HAVING condition in its own parentheses: '((AVG(?g) >= 24))' must parse too
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING ((AVG(?g) >= 24))");

        Assert.IsNotNull(query.GetModifiers().OfType<RDFGroupByModifier>().Single().HavingExpression);
    }

    [TestMethod]
    public void ShouldParseHavingClauseWithConjunction()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING ((COUNT(?e) > 1) && (AVG(?g) >= 24))");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsNotNull(groupByModifier.HavingExpression);
        //Both aggregates are projected (?cnt, ?avg), so HAVING references them by their aliases
        Assert.IsTrue(query.ToString().Contains("HAVING (((?CNT > 1) && (?AVG >= 24)))"));
    }

    [TestMethod]
    public void ShouldParseHavingClauseWithMultipleSpaceSeparatedConditions()
    {
        //Two HavingConditions space-separated (W3C 'HavingCondition+') are conjoined (ANDed) into a single expression
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING (COUNT(?e) > 1) (AVG(?g) >= 24)");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsNotNull(groupByModifier.HavingExpression);
        //The two conditions are ANDed: the printed (idempotent) form combines them with '&&'
        Assert.IsTrue(query.ToString().Contains("&&"));
    }

    [TestMethod]
    public void ShouldParseAndEvaluateHavingWithDisjunction()
    {
        //'||' is now representable (IP3.3): keep a group when EITHER condition holds. Dept A avg 25, dept B avg 40:
        //'(AVG >= 40 || AVG <= 25)' keeps both
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (AVG(?g) AS ?avg) WHERE { ?e <http://example.org/dept> ?c . ?e <http://example.org/age> ?g } GROUP BY ?c HAVING ((AVG(?g) >= 40) || (AVG(?g) <= 25))");

        Assert.IsNotNull(query.GetModifiers().OfType<RDFGroupByModifier>().Single().HavingExpression);
        DataTable results = query.ApplyToGraph(BuildAggregationSampleGraph()).SelectResults;
        Assert.AreEqual(2, results.Rows.Count);
    }

    [TestMethod]
    public void ShouldParseAndEvaluateHavingOverNonProjectedAggregate()
    {
        //An aggregate referenced by HAVING but NOT projected by SELECT is served by a HIDDEN aggregator (IP3.3):
        //only its filtering effect shows, the aggregate value never surfaces as a result column
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) WHERE { ?e <http://example.org/dept> ?c . ?e <http://example.org/age> ?g } GROUP BY ?c HAVING (AVG(?g) >= 30)");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsNotNull(groupByModifier.HavingExpression);
        Assert.IsTrue(groupByModifier.Aggregators.Any(ag => ag.IsHidden));

        //PRINTING: the hidden AVG re-prints as its original call (NOT the synthetic '__HAVINGAGG' column), and the
        //printed query round-trips idempotently
        Assert.IsTrue(query.ToString().Contains("HAVING ((AVG(?G) >= 30))"));
        Assert.IsFalse(query.ToString().Contains("__HAVINGAGG"));
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(query.ToString()).ToString()));

        //EVALUATION: only dept B (avg 40 >= 30) survives; the hidden AVG column must NOT appear among result columns
        DataTable results = query.ApplyToGraph(BuildAggregationSampleGraph()).SelectResults;
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://example.org/deptB", results.Rows[0]["?C"].ToString());
        Assert.IsFalse(results.Columns.Contains("?__HAVINGAGG_0"));
        Assert.IsTrue(results.Columns.Contains("?CNT"));
    }

    [TestMethod]
    public void ShouldParseAndEvaluateHavingWithReversedComparison()
    {
        //The aggregate on the RIGHT-hand side ('30 <= AVG(?g)') is now representable (IP3.3): only dept B passes
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (AVG(?g) AS ?avg) WHERE { ?e <http://example.org/dept> ?c . ?e <http://example.org/age> ?g } GROUP BY ?c HAVING (30 <= AVG(?g))");

        Assert.IsNotNull(query.GetModifiers().OfType<RDFGroupByModifier>().Single().HavingExpression);
        DataTable results = query.ApplyToGraph(BuildAggregationSampleGraph()).SelectResults;
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://example.org/deptB", results.Rows[0]["?C"].ToString());
    }

    [TestMethod]
    public void ShouldParseAndEvaluateHavingWithNonComparisonConstraint()
    {
        //A non-comparison constraint ('!(...)') is now representable (IP3.3): keep groups whose COUNT is NOT 1.
        //Dept A has 2 members (kept), dept B has 1 (dropped)
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) WHERE { ?e <http://example.org/dept> ?c . ?e <http://example.org/age> ?g } GROUP BY ?c HAVING (!(COUNT(?e) = 1))");

        Assert.IsNotNull(query.GetModifiers().OfType<RDFGroupByModifier>().Single().HavingExpression);
        DataTable results = query.ApplyToGraph(BuildAggregationSampleGraph()).SelectResults;
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://example.org/deptA", results.Rows[0]["?C"].ToString());
    }

    [TestMethod]
    public void ShouldParseAndEvaluateHavingUnderImplicitGrouping()
    {
        //HAVING with implicit grouping (aggregate projected but NO explicit GROUP BY): a single group over the whole
        //result set. Total members are 3, so '(COUNT(?e) >= 3)' keeps the single (implicit) group
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (COUNT(?e) AS ?cnt) WHERE { ?e <http://example.org/dept> ?c . ?e <http://example.org/age> ?g } HAVING (COUNT(?e) >= 3)");

        Assert.IsNotNull(query.GetModifiers().OfType<RDFGroupByModifier>().Single().HavingExpression);
        DataTable results = query.ApplyToGraph(BuildAggregationSampleGraph()).SelectResults;
        Assert.AreEqual(1, results.Rows.Count);
        Assert.IsTrue(results.Rows[0]["?CNT"].ToString().StartsWith("3^^", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldParseGroupByPartitionVariablesFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?a ?b (COUNT(?x) AS ?cnt) WHERE { ?a ?b ?x } GROUP BY ?a ?b");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.AreEqual(2, groupByModifier.PartitionVariables.Count);
        Assert.AreEqual("?A", groupByModifier.PartitionVariables[0].ToString());
        Assert.AreEqual("?B", groupByModifier.PartitionVariables[1].ToString());
    }

    [TestMethod]
    public void ShouldParseGroupByAnonymousExpressionCondition()
    {
        //GROUP BY (expr) is now representable (IP3.2) as an anonymous grouping column and round-trips identically
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY (?c + 1)");

        RDFGroupByModifier groupBy = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.AreEqual(1, groupBy.PartitionVariables.Count);
        Assert.AreEqual(1, groupBy.SyntheticPartitionVariables.Count);
        Assert.IsTrue(query.ToString().Contains("GROUP BY (?C + 1)"));
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(query.ToString()).ToString()));
    }

    [TestMethod]
    public void ShouldParseGroupByNamedExpressionCondition()
    {
        //GROUP BY (expr AS ?v) is now representable (IP3.2): ?v is a real projectable grouping variable
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?g (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY (?c + 1 AS ?g)");

        RDFGroupByModifier groupBy = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsTrue(groupBy.PartitionVariables.Any(pv => pv.ToString() == "?G"));
        Assert.AreEqual(0, groupBy.SyntheticPartitionVariables.Count);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(query.ToString()).ToString()));
    }

    [TestMethod]
    public void ShouldParseGroupByFunctionCondition()
    {
        //GROUP BY built-in/function (e.g. STR(?c)) is now representable (IP3.2) as an anonymous grouping column
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY STR(?c)");

        RDFGroupByModifier groupBy = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.AreEqual(1, groupBy.PartitionVariables.Count);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(query.ToString()).ToString()));
    }

    [TestMethod]
    public void ShouldThrowOnHavingWithoutGroupBy()
        //No GROUP BY and no projected aggregate: there is no group to filter, so HAVING stays non-representable
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c WHERE { ?e ?p ?c } HAVING (COUNT(?e) > 1)"));

    [TestMethod]
    public void ShouldParseAndEvaluateHavingWithCountAll()
    {
        //COUNT(*) is referenced by HAVING and reuses its projected column: dept A (2 rows) survives '>= 2'
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(*) AS ?n) WHERE { ?e <http://example.org/dept> ?c . ?e <http://example.org/age> ?g } GROUP BY ?c HAVING (COUNT(*) >= 2)");

        Assert.IsNotNull(query.GetModifiers().OfType<RDFGroupByModifier>().Single().HavingExpression);
        DataTable results = query.ApplyToGraph(BuildAggregationSampleGraph()).SelectResults;
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://example.org/deptA", results.Rows[0]["?C"].ToString());
    }

    [TestMethod]
    public void ShouldThrowOnAggregateInsideFilter()
        //The aggregate-aware sink is only active in HAVING / projection: an aggregate inside a FILTER is still an
        //unknown built-in (aggregates are not legal in a FILTER), so it must fail loudly
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c WHERE { ?e ?p ?c FILTER(COUNT(?e) > 1) } GROUP BY ?c"));

    [TestMethod]
    public void ShouldParseAndEvaluateNestedAggregateInProjection()
    {
        //IP3.2 residual: an aggregate nested inside a projection expression ('?cat + COUNT(?w)'). A hidden COUNT
        //aggregator feeds the expression; the synthetic column never surfaces. Both categories sum to 3.
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (?cat + COUNT(?w) AS ?v) WHERE { ?e <http://example.org/cat> ?cat . ?e <http://example.org/w> ?w } GROUP BY ?cat");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsTrue(groupByModifier.Aggregators.Any(ag => ag.IsHidden));
        //PRINTING: the nested aggregate re-prints as 'COUNT(?W)' (NOT the synthetic '__PROJAGG' column), and the
        //printed form round-trips idempotently
        Assert.IsTrue(query.ToString().Contains("((?CAT + COUNT(?W)) AS ?V)"));
        Assert.IsFalse(query.ToString().Contains("__PROJAGG"));
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(query.ToString()).ToString()));

        DataTable results = query.ApplyToGraph(BuildCategorySampleGraph()).SelectResults;
        Assert.IsFalse(results.Columns.Contains("?__PROJAGG_0"));
        Assert.IsTrue(results.Columns.Contains("?V"));
        //cat 1 has 2 items (1+2=3), cat 2 has 1 item (2+1=3)
        List<string> projectedValues = results.Rows.Cast<DataRow>().Select(r => r["?V"].ToString()).ToList();
        Assert.AreEqual(2, projectedValues.Count);
        Assert.IsTrue(projectedValues.All(v => v.StartsWith("3^^", System.StringComparison.Ordinal)));
    }

    /// <summary>
    /// Sample graph for HAVING evaluation: three employees over two departments, with ages chosen so the
    /// departmental AVG age is 25 for dept A (20, 30) and 40 for dept B (40).
    /// </summary>
    private static RDFGraph BuildAggregationSampleGraph()
    {
        RDFResource dept = new RDFResource("http://example.org/dept");
        RDFResource age = new RDFResource("http://example.org/age");
        RDFResource deptA = new RDFResource("http://example.org/deptA");
        RDFResource deptB = new RDFResource("http://example.org/deptB");
        return new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/emp1"), dept, deptA))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/emp1"), age, new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/emp2"), dept, deptA))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/emp2"), age, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/emp3"), dept, deptB))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/emp3"), age, new RDFTypedLiteral("40", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    /// <summary>
    /// Sample graph for the nested-aggregate projection: two integer categories, with two items in category 1 and
    /// one in category 2, so '?cat + COUNT(?w)' equals 3 for both categories.
    /// </summary>
    private static RDFGraph BuildCategorySampleGraph()
    {
        RDFResource cat = new RDFResource("http://example.org/cat");
        RDFResource w = new RDFResource("http://example.org/w");
        return new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/i1"), cat, new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/i1"), w, new RDFPlainLiteral("x")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/i2"), cat, new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/i2"), w, new RDFPlainLiteral("y")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/i3"), cat, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/i3"), w, new RDFPlainLiteral("z")));
    }

    #endregion
}
