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
/// Unit tests for the aggregate-function half of RDFQueryParser: the seven SPARQL aggregates
/// (COUNT/SUM/MIN/MAX/AVG/SAMPLE/GROUP_CONCAT) in the SELECT projection, their DISTINCT and SEPARATOR
/// options, and the model-imposed failures (COUNT(*), expression arguments, aggregates without GROUP BY).
/// </summary>
public partial class RDFQueryParserTest
{
    #region Aggregators

    /// <summary>
    /// Builds the canonical "aggregate over a single pattern, grouped by ?c" select query whose printer output
    /// is the round-trip oracle: <c>SELECT ?C (AGG...) WHERE { ?e ?p ?c } GROUP BY ?C</c>.
    /// </summary>
    private static RDFSelectQuery BuildGroupedQuery(RDFGroupByModifier groupByModifier)
        => new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("e"), new RDFVariable("p"), new RDFVariable("c"))))
            .AddModifier(groupByModifier);

    [TestMethod]
    public void ShouldRoundTripCountAggregate()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFCountAggregator(new RDFVariable("e"), new RDFVariable("cnt")))));

    [TestMethod]
    public void ShouldRoundTripCountDistinctAggregate()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator((RDFCountAggregator)new RDFCountAggregator(new RDFVariable("e"), new RDFVariable("cnt")).Distinct())));

    [TestMethod]
    public void ShouldRoundTripSumAggregate()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFSumAggregator(new RDFVariable("e"), new RDFVariable("sum")))));

    [TestMethod]
    public void ShouldRoundTripAvgAggregate()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFAvgAggregator(new RDFVariable("e"), new RDFVariable("avg")))));

    [TestMethod]
    public void ShouldRoundTripMinAggregate()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFMinAggregator(new RDFVariable("e"), new RDFVariable("min"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric))));

    [TestMethod]
    public void ShouldRoundTripMaxAggregate()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFMaxAggregator(new RDFVariable("e"), new RDFVariable("max"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric))));

    [TestMethod]
    public void ShouldRoundTripSampleAggregate()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFSampleAggregator(new RDFVariable("e"), new RDFVariable("smp")))));

    [TestMethod]
    public void ShouldRoundTripGroupConcatAggregateWithSeparator()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFGroupConcatAggregator(new RDFVariable("e"), new RDFVariable("gc"), "-"))));

    [TestMethod]
    public void ShouldRoundTripGroupConcatDistinctAggregate()
        => AssertSelectQueryRoundTrips(BuildGroupedQuery(
            new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator((RDFGroupConcatAggregator)new RDFGroupConcatAggregator(new RDFVariable("e"), new RDFVariable("gc"), ";").Distinct())));

    [TestMethod]
    public void ShouldParseCountAggregateFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY ?c");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        RDFAggregator aggregator = groupByModifier.Aggregators.OfType<RDFCountAggregator>().Single();
        Assert.AreEqual("?E", aggregator.AggregatorVariable.ToString());
        Assert.AreEqual("?CNT", aggregator.ProjectionVariable.ToString());
        Assert.IsFalse(aggregator.IsDistinct);
    }

    [TestMethod]
    public void ShouldParseCountDistinctAggregateFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(DISTINCT ?e) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY ?c");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsTrue(groupByModifier.Aggregators.OfType<RDFCountAggregator>().Single().IsDistinct);
    }

    [TestMethod]
    public void ShouldParseGroupConcatSeparatorFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (GROUP_CONCAT(?e; SEPARATOR=\"|\") AS ?gc) WHERE { ?e ?p ?c } GROUP BY ?c");

        RDFGroupConcatAggregator aggregator = query.GetModifiers().OfType<RDFGroupByModifier>().Single().Aggregators.OfType<RDFGroupConcatAggregator>().Single();
        Assert.AreEqual("|", aggregator.Separator);
    }

    [TestMethod]
    public void ShouldNotAddAggregateAsProjectionVariable()
    {
        //The aggregate lives on the GroupBy modifier, NOT among the query's projection variables (only ?c is)
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY ?c");

        Assert.IsFalse(query.ProjectionVars.Keys.Any(v => v.ToString() == "?CNT"));
    }

    [TestMethod]
    public void ShouldParseCountStar()
    {
        //COUNT(*) is now representable (IP3.1): it counts the group's solutions and round-trips identically
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(*) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY ?c");

        RDFGroupByModifier groupBy = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsTrue(groupBy.Aggregators.OfType<RDFCountAggregator>().Single().IsCountAll);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(query.ToString()).ToString()));
    }

    [TestMethod]
    public void ShouldParseAggregateWithExpressionArgument()
    {
        //SUM(?x + ?y) is now representable (IP3.2): the expression is materialized into a synthetic column and the
        //aggregator operates on it; the printed form re-emits the original expression and round-trips identically
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (SUM(?x + ?y) AS ?s) WHERE { ?x ?p ?c . ?x ?q ?y } GROUP BY ?c");

        RDFGroupByModifier groupBy = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        RDFSumAggregator sumAggregator = groupBy.Aggregators.OfType<RDFSumAggregator>().Single();
        Assert.IsNotNull(sumAggregator.AggregatorExpression);
        Assert.IsTrue(query.ToString().Contains("SUM((?X + ?Y))"));
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(query.ToString()).ToString()));
    }

    [TestMethod]
    public void ShouldParseAggregateWithoutGroupBy()
    {
        //Aggregates without GROUP BY are now representable (IP3.1) as implicit grouping (single group), and the
        //printed form carries NO 'GROUP BY' clause
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c }");

        RDFGroupByModifier groupBy = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.AreEqual(0, groupBy.PartitionVariables.Count);
        Assert.IsFalse(query.ToString().Contains("GROUP BY"));
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(query.ToString()).ToString()));
    }

    [TestMethod]
    public void ShouldThrowOnGroupConcatWithoutSeparatorKeyword()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (GROUP_CONCAT(?e; \"|\") AS ?gc) WHERE { ?e ?p ?c } GROUP BY ?c"));

    /// <summary>
    /// "SPARQL 100%" iso-functionality gate for IP3.1: implicit grouping, COUNT(*) and ordinary grouped COUNT are
    /// each built VIA API and must be interchangeable with the query obtained by parsing their printed SPARQL —
    /// both on printing round-trip and on evaluation against a sample graph.
    /// </summary>
    [TestMethod]
    public void ShouldRoundTripAndEvaluateAggregationIso()
    {
        RDFResource predicate = new RDFResource("http://example.org/p");
        RDFGraph sampleGraph = new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s1"), predicate, new RDFPlainLiteral("a")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s1"), predicate, new RDFPlainLiteral("b")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s2"), predicate, new RDFPlainLiteral("c")));

        RDFVariable subjectVariable = new RDFVariable("?s");
        RDFVariable objectVariable = new RDFVariable("?o");

        //Implicit grouping: COUNT(*) over the whole result set (single group)
        RDFSelectQuery implicitCountAllQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup().AddPattern(new RDFPattern(subjectVariable, predicate, objectVariable)))
            .AddModifier(new RDFGroupByModifier().AddAggregator(new RDFCountAggregator(new RDFVariable("?cnt"))));
        RDFTestUtilities.AssertIso(implicitCountAllQuery, sampleGraph);

        //Explicit grouping: COUNT(*) per ?s
        RDFSelectQuery groupedCountAllQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup().AddPattern(new RDFPattern(subjectVariable, predicate, objectVariable)))
            .AddModifier(new RDFGroupByModifier([subjectVariable]).AddAggregator(new RDFCountAggregator(new RDFVariable("?cnt"))));
        RDFTestUtilities.AssertIso(groupedCountAllQuery, sampleGraph);

        //Explicit grouping: COUNT(?o) per ?s
        RDFSelectQuery groupedCountVarQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup().AddPattern(new RDFPattern(subjectVariable, predicate, objectVariable)))
            .AddModifier(new RDFGroupByModifier([subjectVariable]).AddAggregator(new RDFCountAggregator(objectVariable, new RDFVariable("?cnt"))));
        RDFTestUtilities.AssertIso(groupedCountVarQuery, sampleGraph);
    }

    /// <summary>
    /// "SPARQL 100%" iso-functionality gate for IP3.2 (aggregation over expressions): SUM over an arithmetic
    /// expression and a named GROUP BY expression are each built VIA API and must be interchangeable with the query
    /// obtained by parsing their printed SPARQL — both on printing round-trip and on evaluation against a sample graph.
    /// </summary>
    [TestMethod]
    public void ShouldRoundTripAndEvaluateAggregationOverExpressionIso()
    {
        RDFResource categoryPredicate = new RDFResource("http://example.org/cat");
        RDFResource xPredicate = new RDFResource("http://example.org/x");
        RDFResource yPredicate = new RDFResource("http://example.org/y");
        RDFGraph sampleGraph = new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e1"), categoryPredicate, new RDFResource("http://example.org/A")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e1"), xPredicate, new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e1"), yPredicate, new RDFTypedLiteral("10", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e2"), categoryPredicate, new RDFResource("http://example.org/A")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e2"), xPredicate, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e2"), yPredicate, new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e3"), categoryPredicate, new RDFResource("http://example.org/B")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e3"), xPredicate, new RDFTypedLiteral("3", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/e3"), yPredicate, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        RDFVariable categoryVariable = new RDFVariable("?cat");
        RDFVariable xVariable = new RDFVariable("?x");
        RDFVariable yVariable = new RDFVariable("?y");

        RDFPatternGroup BuildPatternGroup() => new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?e"), categoryPredicate, categoryVariable))
            .AddPattern(new RDFPattern(new RDFVariable("?e"), xPredicate, xVariable))
            .AddPattern(new RDFPattern(new RDFVariable("?e"), yPredicate, yVariable));

        //SUM over an expression: SUM(?x + ?y) per ?cat
        RDFSelectQuery sumOverExpressionQuery = new RDFSelectQuery()
            .AddPatternGroup(BuildPatternGroup())
            .AddModifier(new RDFGroupByModifier([categoryVariable])
                .AddAggregator(new RDFSumAggregator(new RDFAddExpression(xVariable, yVariable), new RDFVariable("?s"))));
        RDFTestUtilities.AssertIso(sumOverExpressionQuery, sampleGraph);

        //Named GROUP BY expression: GROUP BY (?x + ?y AS ?g) with COUNT(*)
        RDFSelectQuery groupByExpressionQuery = new RDFSelectQuery()
            .AddPatternGroup(BuildPatternGroup())
            .AddModifier(new RDFGroupByModifier()
                .AddPartitionExpression(new RDFVariable("?g"), new RDFAddExpression(xVariable, yVariable), true)
                .AddAggregator(new RDFCountAggregator(new RDFVariable("?cnt"))));
        RDFTestUtilities.AssertIso(groupByExpressionQuery, sampleGraph);
    }

    #endregion
}
