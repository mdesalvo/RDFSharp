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

using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFGroupConcatAggregatorTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGroupConcatAggregator()
    {
        RDFGroupConcatAggregator aggregator = new RDFGroupConcatAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"), ";");

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.Separator.Equals(";"));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsFalse(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(GROUP_CONCAT(?AGGVAR; SEPARATOR=\";\") AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldCreateDistinctGroupConcatAggregator()
    {
        RDFGroupConcatAggregator aggregator = new RDFGroupConcatAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"), "sep")
            .Distinct() as RDFGroupConcatAggregator;

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.Separator.Equals("sep"));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsTrue(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(GROUP_CONCAT(DISTINCT ?AGGVAR; SEPARATOR=\"sep\") AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldCreateGroupConcatAggregatorWithDefaultSeparator()
    {
        RDFGroupConcatAggregator aggregator = new RDFGroupConcatAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"), null);

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.Separator.Equals(" "));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsFalse(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(GROUP_CONCAT(?AGGVAR; SEPARATOR=\" \") AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGroupConcatAggregatorBecauseNullAggregatorVariable()
        =>  Assert.ThrowsException<RDFQueryException>(() => new RDFGroupConcatAggregator(null, new RDFVariable("?PROJVAR"), ";"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGroupConcatAggregatorBecauseNullPartitionVariable()
        =>  Assert.ThrowsException<RDFQueryException>(() => new RDFGroupConcatAggregator(new RDFVariable("?AGGVAR"), null, ";"));

    [TestMethod]
    public void ShouldApplyModifierWithGroupConcatAggregator()
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
        row2["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row2);
        table.AcceptChanges();

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFGroupConcatAggregator(new RDFVariable("?A"), new RDFVariable("?CONCATPROJ"), ";"));
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].ColumnName);
        Assert.AreEqual("?CONCATPROJ", result.Columns[1].ColumnName);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?CONCATPROJ"].ToString().Equals($"27^^{RDFVocabulary.XSD.FLOAT};26^^{RDFVocabulary.XSD.FLOAT}"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?CONCATPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.FLOAT}"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctGroupConcatAggregator()
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
        row2["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row2["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row2);
        table.AcceptChanges();

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFGroupConcatAggregator(new RDFVariable("?B"), new RDFVariable("?CONCATPROJ"), ";").Distinct());
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].ColumnName);
        Assert.AreEqual("?CONCATPROJ", result.Columns[1].ColumnName);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?CONCATPROJ"].ToString().Equals("hello@EN-US"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?CONCATPROJ"].ToString().Equals("hello@EN-US"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithSampleAggregatorAndHavingClause()
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
        row2["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row2);
        table.AcceptChanges();

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        RDFGroupConcatAggregator aggregator = new RDFGroupConcatAggregator(new RDFVariable("?B"), new RDFVariable("?CONCATPROJ"), ";")
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo, new RDFPlainLiteral("hello", "en-US")) as RDFGroupConcatAggregator;
        modifier.AddAggregator(aggregator);
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].ColumnName);
        Assert.AreEqual("?CONCATPROJ", result.Columns[1].ColumnName);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?CONCATPROJ"].ToString().Equals("hello@EN-US;hello@EN"));
        Assert.IsTrue(aggregator.PrintHavingClause(null).Equals("(GROUP_CONCAT(?B; SEPARATOR=\";\") != \"hello\"@EN-US)"));
    }
    #endregion
}