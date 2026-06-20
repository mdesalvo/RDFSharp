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
public class RDFSampleAggregatorTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateSampleAggregator()
    {
        RDFSampleAggregator aggregator = new RDFSampleAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.Metadata.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.Metadata.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsFalse(aggregator.Metadata.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(SAMPLE(?AGGVAR) AS ?PROJVAR)", System.StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.Context);
        Assert.IsNotNull(aggregator.Context.ExecutionCache);
        Assert.IsNotNull(aggregator.Context.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSampleAggregatorBecauseNullAggregatorVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSampleAggregator(null as RDFVariable, new RDFVariable("?PROJVAR")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSampleAggregatorBecauseNullPartitionVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSampleAggregator(new RDFVariable("?AGGVAR"), null));

    [TestMethod]
    public void ShouldCreateDistinctSampleAggregator()
    {
        RDFSampleAggregator aggregator = new RDFSampleAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"))
            .Distinct() as RDFSampleAggregator;

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.Metadata.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.Metadata.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.Metadata.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals("(SAMPLE(DISTINCT ?AGGVAR) AS ?PROJVAR)", System.StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.Context);
        Assert.IsNotNull(aggregator.Context.ExecutionCache);
        Assert.IsNotNull(aggregator.Context.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldApplyModifierWithSampleAggregator()
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
        modifier.AddAggregator(new RDFSampleAggregator(new RDFVariable("?B"), new RDFVariable("?SAMPLEPROJ")));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?SAMPLEPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithDistinctSampleAggregator()
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
        modifier.AddAggregator(new RDFSampleAggregator(new RDFVariable("?B"), new RDFVariable("?SAMPLEPROJ")).Distinct());
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?SAMPLEPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifierWithSampleAggregatorAndHavingClause()
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
        RDFSampleAggregator aggregator = new RDFSampleAggregator(new RDFVariable("?B"), new RDFVariable("?SAMPLEPROJ"));
        modifier.AddAggregator(aggregator);
        modifier.SetHavingExpression(new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
            new RDFVariableExpression(new RDFVariable("?SAMPLEPROJ")),
            new RDFConstantExpression(new RDFPlainLiteral("hello", "en-UK"))));
        RDFTable result = modifier.ApplyModifier(table);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual("?C", result.Columns[0].Name);
        Assert.AreEqual("?SAMPLEPROJ", result.Columns[1].Name);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[0]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0", System.StringComparison.Ordinal));
        Assert.IsTrue(result.Rows[1]["?SAMPLEPROJ"].ToString().Equals("hello@EN-US", System.StringComparison.Ordinal));
    }

    //IP3.2 — aggregate over expression

    [TestMethod]
    public void ShouldCreateSampleAggregatorOverExpression()
    {
        RDFSampleAggregator aggregator = new RDFSampleAggregator(
            new RDFAddExpression(new RDFVariable("?X"), new RDFVariable("?Y")), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsNotNull(aggregator.Metadata.AggregatorExpression);
        Assert.IsTrue(aggregator.ToString().Equals("(SAMPLE((?X + ?Y)) AS ?PROJVAR)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSampleAggregatorOverExpressionBecauseNullExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSampleAggregator(null as RDFExpression, new RDFVariable("?PROJVAR")));
    #endregion
}
