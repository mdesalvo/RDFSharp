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
public class RDFAvgAggregatorTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateAvgAggregator()
    {
        RDFAvgAggregator aggregator = new RDFAvgAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.Metadata.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.Metadata.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsFalse(aggregator.Metadata.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(AVG(?AGGVAR) AS ?PROJVAR)", System.StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.Context);
        Assert.IsNotNull(aggregator.Context.ExecutionCache);
        Assert.IsNotNull(aggregator.Context.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAvgAggregatorBecauseNullAggregatorVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFAvgAggregator(null as RDFVariable, new RDFVariable("?PROJVAR")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAvgAggregatorBecauseNullPartitionVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFAvgAggregator(new RDFVariable("?AGGVAR"), null));

    [TestMethod]
    public void ShouldCreateDistinctAvgAggregator()
    {
        RDFAvgAggregator aggregator = new RDFAvgAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"))
            .Distinct() as RDFAvgAggregator;

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.Metadata.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.Metadata.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.Metadata.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(AVG(DISTINCT ?AGGVAR) AS ?PROJVAR)", System.StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.Context);
        Assert.IsNotNull(aggregator.Context.ExecutionCache);
        Assert.IsNotNull(aggregator.Context.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldApplyModifierWithAvgAggregator()
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
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVGPROJ")));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?AVGPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?AVGPROJ"].ToString().Equals($"26.5^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?AVGPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctAvgAggregator()
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
        modifier.AddAggregator(new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVGPROJ")).Distinct());
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?AVGPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?AVGPROJ"].ToString().Equals($"27^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?AVGPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithAvgAggregatorAndHavingClause()
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
        RDFAvgAggregator aggregator = new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVGPROJ"));
        modifier.AddAggregator(aggregator);
        modifier.SetHavingExpression(new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariableExpression(new RDFVariable("?AVGPROJ")),
            new RDFConstantExpression(new RDFTypedLiteral("25.99", RDFModelEnums.RDFDatatypes.XSD_FLOAT))));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?AVGPROJ", result.Columns[1].Name);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?AVGPROJ"].ToString().Equals($"26.5^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        //RDFConstantExpression prints a decimal-category typed literal by its bare value, so the HAVING reads "25.99"
        Assert.IsTrue(modifier.HavingExpression.ToString().Equals("(?AVGPROJ >= 25.99)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithAvgAggregatorOperatingOnNonNumericValues()
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
        modifier.AddAggregator(new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVGPROJ")));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?AVGPROJ", result.Columns[1].Name);
        Assert.AreEqual(3, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?AVGPROJ"].ToString().Equals($"26.85^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?AVGPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[2]["?C"].ToString().Equals("ex:value2", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[2]["?AVGPROJ"].ToString().Equals(string.Empty, System.StringComparison.Ordinal)); //Projection for NaN
    }

    //IP3.2 — aggregate over expression

    [TestMethod]
    public void ShouldCreateAvgAggregatorOverExpression()
    {
        RDFAvgAggregator aggregator = new RDFAvgAggregator(
            new RDFAddExpression(new RDFVariable("?X"), new RDFVariable("?Y")), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsNotNull(aggregator.Metadata.AggregatorExpression);
        Assert.IsTrue(aggregator.ToString().Equals("(AVG((?X + ?Y)) AS ?PROJVAR)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAvgAggregatorOverExpressionBecauseNullExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFAvgAggregator(null as RDFExpression, new RDFVariable("?PROJVAR")));
    #endregion
}
