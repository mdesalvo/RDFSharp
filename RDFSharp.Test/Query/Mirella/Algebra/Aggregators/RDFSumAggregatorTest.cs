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
using System.Collections.Generic;
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
        Assert.IsTrue(aggregator.ToString().Equals("(SUM(?AGGVAR) AS ?PROJVAR)", System.StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSumAggregatorBecauseNullAggregatorVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSumAggregator(null as RDFVariable, new RDFVariable("?PROJVAR")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSumAggregatorBecauseNullPartitionVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSumAggregator(new RDFVariable("?AGGVAR"), null));

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
        Assert.IsTrue(aggregator.ToString().Equals("(SUM(DISTINCT ?AGGVAR) AS ?PROJVAR)", System.StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldApplyModifierWithSumAggregator()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("54/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?SUMPROJ")));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?SUMPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?SUMPROJ"].ToString().Equals($"53^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?SUMPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctSumAggregator()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?SUMPROJ")).Distinct());
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?SUMPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?SUMPROJ"].ToString().Equals($"27^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?SUMPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithSumAggregatorAndHavingClause()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        RDFSumAggregator aggregator = new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?SUMPROJ"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_BYTE)) as RDFSumAggregator;
        modifier.AddAggregator(aggregator);
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?SUMPROJ", result.Columns[1].Name);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?SUMPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(aggregator.PrintHavingClause(null).Equals($"(SUM(?A) <= \"30\"^^<{RDFVocabulary.XSD.BYTE}>)", System.StringComparison.Ordinal));
        Assert.IsTrue(aggregator.PrintHavingClause([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(SUM(?A) <= \"30\"^^xsd:byte)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithSumAggregatorOperatingOnNonNumericValues()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("2022-09-04Z", RDFModelEnums.RDFDatatypes.XSD_DATE).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value2").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFSumAggregator(new RDFVariable("?A"), new RDFVariable("?SUMPROJ")));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?SUMPROJ", result.Columns[1].Name);
        Assert.AreEqual(3, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?SUMPROJ"].ToString().Equals($"53.7^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?SUMPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[2]["?C"].ToString().Equals("ex:value2", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[2]["?SUMPROJ"].ToString().Equals(string.Empty, System.StringComparison.Ordinal)); //Projection for NaN
    }

    //IP3.2 — aggregate over expression

    [TestMethod]
    public void ShouldCreateSumAggregatorOverExpression()
    {
        RDFSumAggregator aggregator = new RDFSumAggregator(
            new RDFAddExpression(new RDFVariable("?X"), new RDFVariable("?Y")), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsNotNull(aggregator.AggregatorExpression);
        Assert.IsTrue(aggregator.ToString().Equals("(SUM((?X + ?Y)) AS ?PROJVAR)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSumAggregatorOverExpressionBecauseNullExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSumAggregator(null as RDFExpression, new RDFVariable("?PROJVAR")));

    [TestMethod]
    public void ShouldApplySumAggregatorOverExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?X");
        table.AddColumn("?Y");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?X", new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
            { "?Y", new RDFTypedLiteral("10", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?X", new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
            { "?Y", new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });

        //SUM(?X + ?Y) over the single group ex:value0 => (1+10)+(2+20) = 33
        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFSumAggregator(new RDFAddExpression(new RDFVariable("?X"), new RDFVariable("?Y")), new RDFVariable("?S")));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount); //?C + ?S (the synthetic aggregator column does not surface)
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(result.Rows[0]["?S"].ToString().Equals($"33^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
    }
    #endregion
}
