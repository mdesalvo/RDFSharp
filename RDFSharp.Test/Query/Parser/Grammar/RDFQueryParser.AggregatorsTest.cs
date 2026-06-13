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
    public void ShouldThrowOnCountStar()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (COUNT(*) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY ?c"));

    [TestMethod]
    public void ShouldThrowOnAggregateWithExpressionArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (SUM(?x + ?y) AS ?s) WHERE { ?e ?p ?c } GROUP BY ?c"));

    [TestMethod]
    public void ShouldThrowOnAggregateWithoutGroupBy()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c }"));

    [TestMethod]
    public void ShouldThrowOnGroupConcatWithoutSeparatorKeyword()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (GROUP_CONCAT(?e; \"|\") AS ?gc) WHERE { ?e ?p ?c } GROUP BY ?c"));

    #endregion
}
