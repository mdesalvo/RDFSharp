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
        Assert.IsNotNull(modifier.PartitionVariables);
        Assert.HasCount(1, modifier.PartitionVariables);
        Assert.IsTrue(modifier.PartitionVariables[0].Equals(variable));
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
        Assert.IsNotNull(modifier.PartitionVariables);
        Assert.HasCount(1, modifier.PartitionVariables);
        Assert.IsTrue(modifier.PartitionVariables[0].Equals(variable1));
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
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.Aggregators[0].SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFResource("ex:value0"));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(result.Rows[0]["?C"].Equals("ex:value0", StringComparison.Ordinal));
    }
    #endregion
}
