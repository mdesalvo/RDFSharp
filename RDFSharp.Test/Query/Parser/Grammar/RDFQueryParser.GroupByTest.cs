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
    public void ShouldRoundTripGroupByWithHavingClause()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("e"), new RDFVariable("p"), new RDFVariable("c")))
                .AddPattern(new RDFPattern(new RDFVariable("e"), new RDFVariable("q"), new RDFVariable("g"))))
            .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { new RDFVariable("c") })
                .AddAggregator(new RDFAvgAggregator(new RDFVariable("g"), new RDFVariable("avg"))
                    .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                                     new RDFTypedLiteral("24", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));

        AssertSelectQueryRoundTrips(query);
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
    public void ShouldParseHavingClauseFromHandWrittenQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING (AVG(?g) >= 24)");

        RDFAggregator aggregator = query.GetModifiers().OfType<RDFGroupByModifier>().Single().Aggregators.OfType<RDFAvgAggregator>().Single();
        Assert.IsTrue(aggregator.HavingClause.Item1);
        Assert.AreEqual(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, aggregator.HavingClause.Item2);
    }

    [TestMethod]
    public void ShouldParseHavingClauseWithDoubleParentheses()
    {
        //The printer wraps each HAVING condition in its own parentheses: '((AVG(?g) >= 24))' must parse too
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING ((AVG(?g) >= 24))");

        Assert.IsTrue(query.GetModifiers().OfType<RDFGroupByModifier>().Single().Aggregators.OfType<RDFAvgAggregator>().Single().HavingClause.Item1);
    }

    [TestMethod]
    public void ShouldParseHavingClauseWithConjunction()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING ((COUNT(?e) > 1) && (AVG(?g) >= 24))");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsTrue(groupByModifier.Aggregators.OfType<RDFCountAggregator>().Single().HavingClause.Item1);
        Assert.IsTrue(groupByModifier.Aggregators.OfType<RDFAvgAggregator>().Single().HavingClause.Item1);
    }

    [TestMethod]
    public void ShouldParseHavingClauseWithMultipleSpaceSeparatedConditions()
    {
        //Two HavingConditions space-separated (W3C 'HavingCondition+'), both conjoined into the model
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING (COUNT(?e) > 1) (AVG(?g) >= 24)");

        RDFGroupByModifier groupByModifier = query.GetModifiers().OfType<RDFGroupByModifier>().Single();
        Assert.IsTrue(groupByModifier.Aggregators.OfType<RDFCountAggregator>().Single().HavingClause.Item1);
        Assert.IsTrue(groupByModifier.Aggregators.OfType<RDFAvgAggregator>().Single().HavingClause.Item1);
    }

    [TestMethod]
    public void ShouldThrowOnGroupByExpressionCondition()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY (?c + 1)"));

    [TestMethod]
    public void ShouldThrowOnGroupByFunctionCondition()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c } GROUP BY STR(?c)"));

    [TestMethod]
    public void ShouldThrowOnHavingWithoutGroupBy()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c WHERE { ?e ?p ?c } HAVING (COUNT(?e) > 1)"));

    [TestMethod]
    public void ShouldThrowOnHavingWithDisjunction()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING ((COUNT(?e) > 1) || (AVG(?g) >= 24))"));

    [TestMethod]
    public void ShouldThrowOnHavingOverNonProjectedAggregate()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (COUNT(?e) AS ?cnt) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING (AVG(?g) >= 24)"));

    [TestMethod]
    public void ShouldThrowOnHavingWithReversedComparison()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT ?c (AVG(?g) AS ?avg) WHERE { ?e ?p ?c . ?e ?q ?g } GROUP BY ?c HAVING (24 < AVG(?g))"));

    #endregion
}
