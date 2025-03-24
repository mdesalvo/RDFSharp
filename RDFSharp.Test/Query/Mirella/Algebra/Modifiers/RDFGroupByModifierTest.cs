/*
   Copyright 2012-2025 Marco De Salvo

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
using System.Data;
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
        Assert.AreEqual(1, modifier.PartitionVariables.Count);
        Assert.IsTrue(modifier.PartitionVariables[0].Equals(variable));
        Assert.IsNotNull(modifier.Aggregators);
        Assert.AreEqual(1, modifier.Aggregators.Count);
        Assert.IsTrue(modifier.Aggregators[0].ProjectionVariable.Equals(variable));
        Assert.IsTrue(modifier.Aggregators[0].AggregatorVariable.Equals(variable));
        Assert.IsTrue(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("GROUP BY ?VAR"));
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
        Assert.AreEqual(1, modifier.PartitionVariables.Count);
        Assert.IsTrue(modifier.PartitionVariables[0].Equals(variable1));
        Assert.IsNotNull(modifier.Aggregators);
        Assert.AreEqual(2, modifier.Aggregators.Count);
        Assert.IsTrue(modifier.Aggregators[0].AggregatorVariable.Equals(variable1));
        Assert.IsTrue(modifier.Aggregators[0].ProjectionVariable.Equals(variable1));
        Assert.IsTrue(modifier.Aggregators[1].AggregatorVariable.Equals(variable2));
        Assert.IsTrue(modifier.Aggregators[1].ProjectionVariable.Equals(variable3));
        Assert.IsTrue(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("GROUP BY ?VAR1"));
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
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row0 = table.NewRow();
        row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row0["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row0);
        DataRow row1 = table.NewRow();
        row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row1["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row1);
        DataRow row2 = table.NewRow();
        row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
        row2["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row2);
        table.AcceptChanges();

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?D")]);

        Assert.ThrowsExactly<RDFQueryException>(() => modifier.ApplyModifier(table), "Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: ?D");
    }

    [TestMethod]
    public void ShouldThrowExceptionDuringConsistencyChecksBecauseUnavailableAggregatorVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row0 = table.NewRow();
        row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row0["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row0);
        DataRow row1 = table.NewRow();
        row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row1["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row1);
        DataRow row2 = table.NewRow();
        row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
        row2["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row2);
        table.AcceptChanges();

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFAggregator(new RDFVariable("?D"), new RDFVariable("?A")));
        modifier.AddAggregator(new RDFAggregator(new RDFVariable("?D"), new RDFVariable("?B")));

        Assert.ThrowsExactly<RDFQueryException>(() => modifier.ApplyModifier(table), "Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: ?D");
    }

    [TestMethod]
    public void ShouldThrowExceptionDuringConsistencyChecksBecauseCommonPartitionProjectionVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row0 = table.NewRow();
        row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row0["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row0);
        DataRow row1 = table.NewRow();
        row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row1["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row1);
        DataRow row2 = table.NewRow();
        row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
        row2["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row2);
        table.AcceptChanges();

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?B")]);
        modifier.AddAggregator(new RDFAggregator(new RDFVariable("?A"), new RDFVariable("?A")));
        modifier.AddAggregator(new RDFAggregator(new RDFVariable("?A"), new RDFVariable("?B")));

        Assert.ThrowsExactly<RDFQueryException>(() => modifier.ApplyModifier(table), "Cannot apply GroupBy modifier because the following variables have been specified both for partitioning (in GroupBy) and projection (in Aggregator): ?B");
    }

    [TestMethod]
    public void ShouldApplyModifier()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row0 = table.NewRow();
        row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row0["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row0);
        DataRow row1 = table.NewRow();
        row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row1["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row1);
        DataRow row2 = table.NewRow();
        row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
        row2["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row2);
        table.AcceptChanges();

        //This will behave like a partition aggregator on column "?C"
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].ColumnName);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithHavingClause()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row0 = table.NewRow();
        row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row0["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row0);
        DataRow row1 = table.NewRow();
        row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row1["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row1);
        DataRow row2 = table.NewRow();
        row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
        row2["?C"] = new RDFResource("ex:value0").ToString();
        table.Rows.Add(row2);
        DataRow row3 = table.NewRow();
        row3["?A"] = new RDFTypedLiteral("36.0", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row3["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row3["?C"] = DBNull.Value;
        table.Rows.Add(row3);
        table.AcceptChanges();

        //This will behave like a partition aggregator on column "?C" with an having clause "?C = ex:value0"
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.Aggregators[0].SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFResource("ex:value0"));
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].ColumnName);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value0"));
    }
    #endregion
}