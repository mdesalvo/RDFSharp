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
public class RDFSumAggregatorTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateSumAggregator()
    {
        RDFSumAggregator aggregator = new RDFSumAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsFalse(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(SUM(?AGGVAR) AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSumAggregatorBecauseNullAggregatorVariable()
        =>  Assert.ThrowsException<RDFQueryException>(() => new RDFSumAggregator(null, new RDFVariable("?PROJVAR")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSumAggregatorBecauseNullPartitionVariable()
        =>  Assert.ThrowsException<RDFQueryException>(() => new RDFSumAggregator(new RDFVariable("?AGGVAR"), null));

    [TestMethod]
    public void ShouldCreateDistinctSumAggregator()
    {
        RDFSumAggregator aggregator = new RDFSumAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"))
            .Distinct() as RDFSumAggregator;

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
        Assert.IsTrue(aggregator.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(SUM(DISTINCT ?AGGVAR) AS ?PROJVAR)"));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldApplyModifierWithSumAggregator()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row0 = table.NewRow();
        row0["?A"] = new RDFTypedLiteral("54/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
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
        modifier.AddAggregator(new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?SUMPROJ")));
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Columns.Count == 2);
        Assert.IsTrue(result.Columns[0].ColumnName == "?C");
        Assert.IsTrue(result.Columns[1].ColumnName == "?SUMPROJ");
        Assert.IsTrue(result.Rows.Count == 2);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?SUMPROJ"].ToString().Equals($"53^^{RDFVocabulary.XSD.DOUBLE}"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?SUMPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctSumAggregator()
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
        row2["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row2["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row2["?C"] = new RDFResource("ex:value1").ToString();
        table.Rows.Add(row2);
        table.AcceptChanges();

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?SUMPROJ")).Distinct());
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Columns.Count == 2);
        Assert.IsTrue(result.Columns[0].ColumnName == "?C");
        Assert.IsTrue(result.Columns[1].ColumnName == "?SUMPROJ");
        Assert.IsTrue(result.Rows.Count == 2);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?SUMPROJ"].ToString().Equals($"27^^{RDFVocabulary.XSD.DOUBLE}"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?SUMPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithSumAggregatorAndHavingClause()
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
        RDFSumAggregator aggregator = new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?SUMPROJ"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_BYTE)) as RDFSumAggregator;
        modifier.AddAggregator(aggregator);
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Columns.Count == 2);
        Assert.IsTrue(result.Columns[0].ColumnName == "?C");
        Assert.IsTrue(result.Columns[1].ColumnName == "?SUMPROJ");
        Assert.IsTrue(result.Rows.Count == 1);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[0]["?SUMPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}"));
        Assert.IsTrue(aggregator.PrintHavingClause(null).Equals($"(SUM(?A) <= \"30\"^^<{RDFVocabulary.XSD.BYTE}>)"));
        Assert.IsTrue(aggregator.PrintHavingClause([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(SUM(?A) <= \"30\"^^xsd:byte)"));
    }

    [TestMethod]
    public void ShouldApplyModifierWithSumAggregatorOperatingOnNonNumericValues()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row0 = table.NewRow();
        row0["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
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
        DataRow row3 = table.NewRow();
        row3["?A"] = new RDFTypedLiteral("2022-09-04Z", RDFModelEnums.RDFDatatypes.XSD_DATE).ToString();
        row3["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        row3["?C"] = new RDFResource("ex:value2").ToString();
        table.Rows.Add(row3);
        table.AcceptChanges();

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?SUMPROJ")));
        DataTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Columns.Count == 2);
        Assert.IsTrue(result.Columns[0].ColumnName == "?C");
        Assert.IsTrue(result.Columns[1].ColumnName == "?SUMPROJ");
        Assert.IsTrue(result.Rows.Count == 3);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
        Assert.IsTrue(result.Rows[0]["?SUMPROJ"].ToString().Equals($"53.7^^{RDFVocabulary.XSD.DOUBLE}"));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
        Assert.IsTrue(result.Rows[1]["?SUMPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}"));
        Assert.IsTrue(result.Rows[2]["?C"].ToString().Equals("ex:value2"));
        Assert.IsTrue(result.Rows[2]["?SUMPROJ"].ToString().Equals(string.Empty)); //Projection for NaN
    }
    #endregion
}