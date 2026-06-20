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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFGroupByModifierTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGroupByModifier()
    {
        RDFVariable variable = new RDFVariable("?VAR");
        List<RDFVariable> partitionVariables = [variable];
        RDFGroupByModifier modifier = new RDFGroupByModifier(partitionVariables);

        Assert.IsNotNull(modifier);
        Assert.IsNotNull(modifier.PartitionConditions);
        Assert.HasCount(1, modifier.PartitionConditions);
        Assert.IsTrue(modifier.PartitionConditions[0].Variable.Equals(variable));
        Assert.IsNotNull(modifier.Aggregators);
        Assert.HasCount(1, modifier.Aggregators);
        Assert.IsTrue(modifier.Aggregators[0].ProjectionVariable.Equals(variable));
        Assert.IsTrue(modifier.Aggregators[0].AggregatorVariable.Equals(variable));
        Assert.IsTrue(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("GROUP BY ?VAR", StringComparison.Ordinal));
        Assert.IsNotNull(modifier.QueryMemberStringID);
        Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
        Assert.IsTrue(modifier.Equals(modifier));
        Assert.IsFalse(modifier.Equals(new RDFGroupByModifier(partitionVariables)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGroupByModifierBecauseNullVariables()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGroupByModifier(null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGroupByModifierBecauseEmptyVariables()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGroupByModifier([]));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGroupByModifierBecauseNullItemInVariables()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGroupByModifier([null]));

    [TestMethod]
    public void ShouldAddAggregator()
    {
        RDFVariable variable1 = new RDFVariable("?VAR1");
        RDFVariable variable2 = new RDFVariable("?VAR2");
        RDFVariable variable3 = new RDFVariable("?VAR3");
        List<RDFVariable> partitionVariables = [variable1];
        RDFGroupByModifier modifier = new RDFGroupByModifier(partitionVariables);
        RDFAggregator aggregator = new RDFAggregator(variable2, variable3);
        modifier.AddAggregator(aggregator);
        modifier.AddAggregator(null); //Will be discarded, since null aggregators are not allowed

        Assert.IsNotNull(modifier);
        Assert.IsNotNull(modifier.PartitionConditions);
        Assert.HasCount(1, modifier.PartitionConditions);
        Assert.IsTrue(modifier.PartitionConditions[0].Variable.Equals(variable1));
        Assert.IsNotNull(modifier.Aggregators);
        Assert.HasCount(2, modifier.Aggregators);
        Assert.IsTrue(modifier.Aggregators[0].AggregatorVariable.Equals(variable1));
        Assert.IsTrue(modifier.Aggregators[0].ProjectionVariable.Equals(variable1));
        Assert.IsTrue(modifier.Aggregators[1].AggregatorVariable.Equals(variable2));
        Assert.IsTrue(modifier.Aggregators[1].ProjectionVariable.Equals(variable3));
        Assert.IsTrue(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("GROUP BY ?VAR1", StringComparison.Ordinal));
        Assert.IsNotNull(modifier.QueryMemberStringID);
        Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
        Assert.IsTrue(modifier.Equals(modifier));
        Assert.IsFalse(modifier.Equals(new RDFGroupByModifier(partitionVariables).AddAggregator(aggregator)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingAggregatorBecauseDuplicatePartitionVariable()
    {
        RDFVariable variable1 = new RDFVariable("?VAR1");
        RDFVariable variable2 = new RDFVariable("?VAR2");
        RDFVariable variable3 = new RDFVariable("?VAR3");
        List<RDFVariable> partitionVariables = [variable1];
        RDFGroupByModifier modifier = new RDFGroupByModifier(partitionVariables);
        RDFAggregator aggregator1 = new RDFAggregator(variable1, variable3);
        RDFAggregator aggregator2 = new RDFAggregator(variable2, variable3);
        modifier.AddAggregator(aggregator1);

        Assert.ThrowsExactly<RDFQueryException>(() => modifier.AddAggregator(aggregator2), "Cannot add aggregator to GroupBy modifier because the given projection variable '?VAR3' is already used by another aggregator.");
    }

    [TestMethod]
    public void ShouldThrowExceptionDuringConsistencyChecksBecauseUnavailablePartitionVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?D")]);

        Assert.ThrowsExactly<RDFQueryException>(() => modifier.ApplyModifier(table), "Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: ?D");
    }

    [TestMethod]
    public void ShouldThrowExceptionDuringConsistencyChecksBecauseUnavailableAggregatorVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFAggregator(new RDFVariable("?D"), new RDFVariable("?A")));
        modifier.AddAggregator(new RDFAggregator(new RDFVariable("?D"), new RDFVariable("?B")));

        Assert.ThrowsExactly<RDFQueryException>(() => modifier.ApplyModifier(table), "Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: ?D");
    }

    [TestMethod]
    public void ShouldThrowExceptionDuringConsistencyChecksBecauseCommonPartitionProjectionVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?B")]);
        modifier.AddAggregator(new RDFAggregator(new RDFVariable("?A"), new RDFVariable("?A")));
        modifier.AddAggregator(new RDFAggregator(new RDFVariable("?A"), new RDFVariable("?B")));

        Assert.ThrowsExactly<RDFQueryException>(() => modifier.ApplyModifier(table), "Cannot apply GroupBy modifier because the following variables have been specified both for partitioning (in GroupBy) and projection (in Aggregator): ?B");
    }

    [TestMethod]
    public void ShouldApplyModifier()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });

        //This will behave like a partition aggregator on column "?C"
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual(2, result.RowsCount);
        Assert.IsTrue(result.Rows[0]["?C"].Equals("ex:value1", StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].Equals("ex:value0", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithHavingClause()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("36.0", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString()
        });

        //This will behave like a partition aggregator on column "?C" with an having clause "?C = ex:value0"
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")])
            .SetHavingExpression(new RDFComparisonExpression(
                RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                new RDFVariableExpression(new RDFVariable("?C")),
                new RDFConstantExpression(new RDFResource("ex:value0"))));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(result.Rows[0]["?C"].Equals("ex:value0", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSetHavingExpression()
    {
        RDFComparisonExpression havingExpression = new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariableExpression(new RDFVariable("?CNT")),
            new RDFConstantExpression(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")])
            .SetHavingExpression(havingExpression);

        Assert.AreSame(havingExpression, modifier.HavingExpression);
    }

    [TestMethod]
    public void ShouldApplyModifierWithFreeHavingExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        table.AddColumn("?X");
        table.AddRow(new Dictionary<string, string> { ["?C"] = new RDFResource("ex:c1").ToString(), ["?X"] = new RDFPlainLiteral("x").ToString() });
        table.AddRow(new Dictionary<string, string> { ["?C"] = new RDFResource("ex:c1").ToString(), ["?X"] = new RDFPlainLiteral("y").ToString() });
        table.AddRow(new Dictionary<string, string> { ["?C"] = new RDFResource("ex:c2").ToString(), ["?X"] = new RDFPlainLiteral("z").ToString() });

        //Group by ?C, COUNT(?X) AS ?CNT, keeping only the groups whose count is >= 2 (so ex:c1 survives, ex:c2 is dropped)
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")])
            .AddAggregator(new RDFCountAggregator(new RDFVariable("?X"), new RDFVariable("?CNT")))
            .SetHavingExpression(new RDFComparisonExpression(
                RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariableExpression(new RDFVariable("?CNT")),
                new RDFConstantExpression(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(result.Rows[0]["?C"].Equals("ex:c1", StringComparison.Ordinal));
    }

    //The following two tests guard against a regression where the aggregators' execution
    //context was created once and never reset: re-applying the same modifier (i.e. re-executing
    //the same query object, as happens with cached/reused queries) carried over sums and counters
    //from the previous run, corrupting the computed aggregates. The modifier must be idempotent.

    [TestMethod]
    public void ShouldApplyModifierTwiceWithoutCarryingOverAggregatorState()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });

        //GROUP BY ?C with SUM(?A) AS ?S : ex:value0 -> 51, ex:value1 -> 27
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?S")));

        string expectedSum0 = new RDFTypedLiteral("51", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        string expectedSum1 = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();

        //First application
        RDFTable firstResult = modifier.ApplyModifier(table);
        Assert.IsNotNull(firstResult);
        Assert.AreEqual(2, firstResult.RowsCount);
        Assert.IsTrue(firstResult.Rows[0]["?S"].Equals(expectedSum0, StringComparison.Ordinal));
        Assert.IsTrue(firstResult.Rows[1]["?S"].Equals(expectedSum1, StringComparison.Ordinal));

        //Second application of the very same modifier must produce identical results
        //(before the fix the sums would have doubled to 102 and 54, since state was carried over)
        RDFTable secondResult = modifier.ApplyModifier(table);
        Assert.IsNotNull(secondResult);
        Assert.AreEqual(2, secondResult.RowsCount);
        Assert.IsTrue(secondResult.Rows[0]["?S"].Equals(expectedSum0, StringComparison.Ordinal));
        Assert.IsTrue(secondResult.Rows[1]["?S"].Equals(expectedSum1, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithHavingTwiceWithoutCarryingOverAggregatorState()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });

        //GROUP BY ?C with AVG(?A) AS ?AVG HAVING (?AVG >= 28) : ex:value0 -> 25.5 (dropped), ex:value1 -> 30 (kept)
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVG")))
            .SetHavingExpression(new RDFComparisonExpression(
                RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariableExpression(new RDFVariable("?AVG")),
                new RDFConstantExpression(new RDFTypedLiteral("28", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));

        //First application : only ex:value1 survives the HAVING clause
        RDFTable firstResult = modifier.ApplyModifier(table);
        Assert.IsNotNull(firstResult);
        Assert.AreEqual(1, firstResult.RowsCount);
        Assert.IsTrue(firstResult.Rows[0]["?C"].Equals("ex:value1", StringComparison.Ordinal));

        //Second application must keep the same surviving row: before the fix the averages drifted
        //downward across runs (ex:value1 dropping below 28), so the HAVING clause discarded every
        //group and the second result was empty
        RDFTable secondResult = modifier.ApplyModifier(table);
        Assert.IsNotNull(secondResult);
        Assert.AreEqual(1, secondResult.RowsCount);
        Assert.IsTrue(secondResult.Rows[0]["?C"].Equals("ex:value1", StringComparison.Ordinal));
    }

    //IP3.1 — implicit grouping (no partition variables = single group over the whole result set)

    [TestMethod]
    public void ShouldCreateImplicitGroupByModifier()
    {
        RDFGroupByModifier modifier = new RDFGroupByModifier();

        Assert.IsNotNull(modifier);
        Assert.AreEqual(0, modifier.PartitionConditions.Count);
        Assert.AreEqual(0, modifier.Aggregators.Count);
        Assert.IsTrue(modifier.IsEvaluable);
    }

    [TestMethod]
    public void ShouldApplyImplicitGroupByModifierWithCountAll()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?C"] = new RDFResource("ex:value1").ToString()
        });

        //Implicit grouping: all 3 rows fall into one group => COUNT(*) = 3 in a single result row
        RDFGroupByModifier modifier = new RDFGroupByModifier();
        modifier.AddAggregator(new RDFCountAggregator(new RDFVariable("?CNT")));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual("?CNT", result.Columns[0].Name);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(result.Rows[0]["?CNT"].Equals($"3^^{RDFVocabulary.XSD.DECIMAL}", StringComparison.Ordinal));
    }
    #endregion
}
