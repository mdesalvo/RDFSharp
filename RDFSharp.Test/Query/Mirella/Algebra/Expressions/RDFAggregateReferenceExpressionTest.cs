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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the INTERNAL RDFAggregateReferenceExpression: the parser-only bridge that, bound to an aggregator,
/// reads its already-materialized column at evaluation time and re-prints its original aggregate call at print time.
/// </summary>
[TestClass]
public class RDFAggregateReferenceExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateAggregateReferenceExpressionFromAggregator()
    {
        RDFCountAggregator aggregator = new RDFCountAggregator(new RDFVariable("?E"), new RDFVariable("?CNT"));
        RDFAggregateReferenceExpression expression = new RDFAggregateReferenceExpression(aggregator);

        Assert.IsNotNull(expression);
        //The aggregator is the single source of truth (column to read + call text to re-print, both on demand)
        Assert.AreSame(aggregator, expression.ReferencedAggregator);
        //The referenced column (the aggregator's projection variable) is also exposed as the (left) expression argument
        Assert.AreEqual("?CNT", expression.LeftArgument.ToString());
        Assert.IsNull(expression.RightArgument);
    }

    [TestMethod]
    public void ShouldPrintTheOriginalAggregateCall()
    {
        RDFAvgAggregator aggregator = new RDFAvgAggregator(new RDFVariable("?G"), new RDFVariable("?AVG"));
        RDFAggregateReferenceExpression expression = new RDFAggregateReferenceExpression(aggregator);

        //Printing re-emits the call text (NOT the synthetic column name), with or without prefixes
        Assert.AreEqual("AVG(?G)", expression.ToString());
        Assert.AreEqual("AVG(?G)", expression.ToString([]));
    }

    [TestMethod]
    public void ShouldThrowOnCreatingFromNullAggregator()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            _ = new RDFAggregateReferenceExpression(null));

    [TestMethod]
    public void ShouldApplyExpressionReadingTheAggregateColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?CNT");
        table.AddRow(new Dictionary<string, string>
        {
            { "?CNT", new RDFTypedLiteral("7", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() }
        });

        RDFAggregateReferenceExpression expression =
            new RDFAggregateReferenceExpression(new RDFCountAggregator(new RDFVariable("?E"), new RDFVariable("?CNT")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("7", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    [TestMethod]
    public void ShouldApplyExpressionReturningNullOnMissingColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?OTHER");
        table.AddRow(new Dictionary<string, string> { { "?OTHER", "whatever" } });

        RDFAggregateReferenceExpression expression =
            new RDFAggregateReferenceExpression(new RDFCountAggregator(new RDFVariable("?E"), new RDFVariable("?CNT")));

        //The referenced column is absent from the row: evaluation yields no value (UNBOUND)
        Assert.IsNull(expression.ApplyExpression(table.Rows[0]));
    }
    #endregion
}
