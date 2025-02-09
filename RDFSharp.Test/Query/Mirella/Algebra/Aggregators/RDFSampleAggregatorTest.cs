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
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFSampleAggregatorTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateSampleAggregator()
    {
        RDFSampleAggregator aggregator = new RDFSampleAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsFalse(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(SAMPLE(?AGGVAR) AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSampleAggregatorBecauseNullAggregatorVariable()
        =>  Assert.ThrowsException<RDFQueryException>(() => new RDFSampleAggregator(null as RDFVariable, new RDFVariable("?PROJVAR")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSampleAggregatorBecauseNullPartitionVariable()
        =>  Assert.ThrowsException<RDFQueryException>(() => new RDFSampleAggregator(new RDFVariable("?AGGVAR"), null as RDFVariable));

    [TestMethod]
    public void ShouldCreateDistinctSampleAggregator()
    {
        RDFSampleAggregator aggregator = new RDFSampleAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"))
            .Distinct() as RDFSampleAggregator;

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsTrue(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(SAMPLE(DISTINCT ?AGGVAR) AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldApplyModifierWithSampleAggregator()
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
        modifier.AddAggregator(new RDFSampleAggregator(new RDFVariable("?B"), new RDFVariable("?SAMPLEPROJ")));
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Columns.Count == 2);
        Assert.IsTrue(result.Columns[0].ColumnName == "?C");
        Assert.IsTrue(result.Columns[1].ColumnName == "?SAMPLEPROJ");
        Assert.IsTrue(result.Rows.Count == 2);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctSampleAggregator()
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
        modifier.AddAggregator(new RDFSampleAggregator(new RDFVariable("?B"), new RDFVariable("?SAMPLEPROJ")).Distinct());
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Columns.Count == 2);
        Assert.IsTrue(result.Columns[0].ColumnName == "?C");
        Assert.IsTrue(result.Columns[1].ColumnName == "?SAMPLEPROJ");
        Assert.IsTrue(result.Rows.Count == 2);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US"));
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
        RDFSampleAggregator aggregator = new RDFSampleAggregator(new RDFVariable("?B"), new RDFVariable("?SAMPLEPROJ"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFPlainLiteral("hello", "en-UK")) as RDFSampleAggregator;
        modifier.AddAggregator(aggregator);
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Columns.Count == 2);
        Assert.IsTrue(result.Columns[0].ColumnName == "?C");
        Assert.IsTrue(result.Columns[1].ColumnName == "?SAMPLEPROJ");
        Assert.IsTrue(result.Rows.Count == 2);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US"));
        Assert.IsTrue(aggregator.PrintHavingClause(null).Equals("(SAMPLE(?B) > \"hello\"@EN-UK)"));
    }
    #endregion
}