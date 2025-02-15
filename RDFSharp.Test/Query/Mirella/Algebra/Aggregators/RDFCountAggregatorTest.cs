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
public class RDFCountAggregatorTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateCountAggregator()
    {
        RDFCountAggregator aggregator = new RDFCountAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsFalse(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(COUNT(?AGGVAR) AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingCountAggregatorBecauseNullAggregatorVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => new RDFCountAggregator(null, new RDFVariable("?PROJVAR")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingCountAggregatorBecauseNullPartitionVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => new RDFCountAggregator(new RDFVariable("?AGGVAR"), null));

    [TestMethod]
    public void ShouldCreateDistinctCountAggregator()
    {
        RDFCountAggregator aggregator = new RDFCountAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"))
            .Distinct() as RDFCountAggregator;

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsTrue(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(COUNT(DISTINCT ?AGGVAR) AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldApplyModifierWithCountAggregator()
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
        modifier.AddAggregator(new RDFCountAggregator(new RDFVariable("?B"), new RDFVariable("?COUNTPROJ")));
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].ColumnName);
        Assert.AreEqual("?COUNTPROJ", result.Columns[1].ColumnName);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?COUNTPROJ"].ToString().Equals($"2^^{RDFVocabulary.XSD.DECIMAL}"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?COUNTPROJ"].ToString().Equals($"1^^{RDFVocabulary.XSD.DECIMAL}"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctCountAggregator()
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
        modifier.AddAggregator(new RDFCountAggregator(new RDFVariable("?B"), new RDFVariable("?COUNTPROJ")).Distinct());
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].ColumnName);
        Assert.AreEqual("?COUNTPROJ", result.Columns[1].ColumnName);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?COUNTPROJ"].ToString().Equals($"1^^{RDFVocabulary.XSD.DECIMAL}"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?COUNTPROJ"].ToString().Equals($"1^^{RDFVocabulary.XSD.DECIMAL}"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithCountAggregatorAndHavingClause()
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
        RDFCountAggregator aggregator = new RDFCountAggregator(new RDFVariable("?B"), new RDFVariable("?COUNTPROJ"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.LessThan, new RDFTypedLiteral("2.0", RDFModelEnums.RDFDatatypes.XSD_FLOAT)) as RDFCountAggregator;
        modifier.AddAggregator(aggregator);
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].ColumnName);
        Assert.AreEqual("?COUNTPROJ", result.Columns[1].ColumnName);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[0]["?COUNTPROJ"].ToString().Equals($"1^^{RDFVocabulary.XSD.DECIMAL}"));
        Assert.IsTrue(aggregator.PrintHavingClause(null).Equals($"(COUNT(?B) < \"2\"^^<{RDFVocabulary.XSD.FLOAT}>)"));
        Assert.IsTrue(aggregator.PrintHavingClause([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(COUNT(?B) < \"2\"^^xsd:float)"));
    }
    #endregion
}