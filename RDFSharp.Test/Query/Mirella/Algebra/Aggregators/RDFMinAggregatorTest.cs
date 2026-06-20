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
public class RDFMinAggregatorTest
{
    #region Tests
    [TestMethod]
    [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)]
    [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)]
    public void ShouldCreateMinAggregator(RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor)
    {
        RDFMinAggregator aggregator = new RDFMinAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"), aggregatorFlavor);

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.Metadata.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.Metadata.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsFalse(aggregator.Metadata.IsDistinct);
        Assert.AreEqual(aggregatorFlavor, aggregator.AggregatorFlavor);
        Assert.IsTrue(aggregator.ToString().Equals("(MIN(?AGGVAR) AS ?PROJVAR)", System.StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)]
    [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)]
    public void ShouldThrowExceptionOnCreatingStringMinAggregatorBecauseNullAggregatorVariable(RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor)
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMinAggregator(null as RDFVariable, new RDFVariable("?PROJVAR"), aggregatorFlavor));

    [TestMethod]
    [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)]
    [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)]
    public void ShouldThrowExceptionOnCreatingStringMinAggregatorBecauseNullPartitionVariable(RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor)
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMinAggregator(new RDFVariable("?AGGVAR"), null, aggregatorFlavor));

    [TestMethod]
    [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)]
    [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)]
    public void ShouldCreateDistinctMinAggregator(RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor)
    {
        RDFMinAggregator aggregator = new RDFMinAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"), aggregatorFlavor)
            .Distinct() as RDFMinAggregator;

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.Metadata.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.Metadata.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.Metadata.IsDistinct);
        Assert.AreEqual(aggregatorFlavor, aggregator.AggregatorFlavor);
        Assert.IsTrue(aggregator.ToString().Equals("(MIN(DISTINCT ?AGGVAR) AS ?PROJVAR)", System.StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldApplyModifierWithMinAggregatorString()
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
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-UK").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFMinAggregator(new RDFVariable("?B"), new RDFVariable("?MINPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.String));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?MINPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?MINPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?MINPROJ"].ToString().Equals("hello@EN-UK", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctMinAggregatorString()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFResource("http://example.org/test/test1").ToString() },
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
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("29", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFMinAggregator(new RDFVariable("?B"), new RDFVariable("?MINPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.String).Distinct());
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?MINPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?MINPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?MINPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithMinAggregatorStringAndHavingClause()
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
            { "?B", new RDFPlainLiteral("hello", "en").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFMinAggregator(new RDFVariable("?B"), new RDFVariable("?MINPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.String));
        modifier.SetHavingExpression(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan, new RDFVariableExpression(new RDFVariable("?MINPROJ")), new RDFConstantExpression(new RDFPlainLiteral("hello", "en-US"))));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?MINPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?MINPROJ"].ToString().Equals("hello@EN", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?MINPROJ"].ToString().Equals("hello@EN", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithMinAggregatorNumeric()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("25.114", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
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
            { "?A", new RDFTypedLiteral("22.47", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-UK").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFMinAggregator(new RDFVariable("?A"), new RDFVariable("?MINPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?MINPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?MINPROJ"].ToString().Equals($"26^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?MINPROJ"].ToString().Equals($"22.47^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctMinAggregatorNumeric()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.0", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFResource("http://example.org/test/test1").ToString() },
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
            { "?A", new RDFTypedLiteral("27.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("29", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFMinAggregator(new RDFVariable("?A"), new RDFVariable("?MINPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric).Distinct());
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?MINPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?MINPROJ"].ToString().Equals($"27^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?MINPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithMinAggregatorNumericAndHavingClause()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("28.24", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("28", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("77.77", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFMinAggregator(new RDFVariable("?A"), new RDFVariable("?MINPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric));
        modifier.SetHavingExpression(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFVariableExpression(new RDFVariable("?MINPROJ")), new RDFConstantExpression(new RDFTypedLiteral("28", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER))));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?MINPROJ", result.Columns[1].Name);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?MINPROJ"].ToString().Equals($"28.24^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithMinAggregatorNumericOnNonNumericValues()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:value0").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("26.09", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:value1").ToString() }
        });
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("22.47", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-UK").ToString() },
            { "?C", new RDFResource("ex:value0").ToString() }
        });

        RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
        modifier.AddAggregator(new RDFMinAggregator(new RDFVariable("?A"), new RDFVariable("?MINPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?MINPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?MINPROJ"].ToString().Equals($"26.09^^{RDFVocabulary.XSD.DOUBLE}", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?MINPROJ"].ToString().Equals(string.Empty, System.StringComparison.Ordinal));
    }

    //IP3.2 — aggregate over expression

    [TestMethod]
    public void ShouldCreateMinAggregatorOverExpression()
    {
        RDFMinAggregator aggregator = new RDFMinAggregator(
            new RDFAddExpression(new RDFVariable("?X"), new RDFVariable("?Y")), new RDFVariable("?PROJVAR"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric);

        Assert.IsNotNull(aggregator);
        Assert.IsNotNull(aggregator.Metadata.AggregatorExpression);
        Assert.IsTrue(aggregator.ToString().Equals("(MIN((?X + ?Y)) AS ?PROJVAR)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMinAggregatorOverExpressionBecauseNullExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMinAggregator(null as RDFExpression, new RDFVariable("?PROJVAR"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric));
    #endregion
}
