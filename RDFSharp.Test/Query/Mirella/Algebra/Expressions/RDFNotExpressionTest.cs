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
using System;
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFNotExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateNotExpressionWithExpression()
    {
        RDFNotExpression expression = new RDFNotExpression(
            new RDFBoundExpression(new RDFVariable("?V")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(!((BOUND(?V))))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(!((BOUND(?V))))", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateNotExpressionWithVariable()
    {
        RDFNotExpression expression = new RDFNotExpression(
            new RDFVariable("?V"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(!(?V))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(!(?V))", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingNotExpressionBecauseNullExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFNotExpression(null as RDFExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingNotExpressionBecauseNullVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFNotExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionAndNegateTrueIntoFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN).ToString() }
        });

        RDFNotExpression expression = new RDFNotExpression(new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNegateFalseIntoTrue()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("false", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN).ToString() }
        });

        RDFNotExpression expression = new RDFNotExpression(new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithNestedExpressionAndNegateBound()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello").ToString() }
        });

        //BOUND(?A) is true => NOT is false
        RDFNotExpression expression = new RDFNotExpression(
            new RDFBoundExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseNotBoolean()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello").ToString() }
        });

        RDFNotExpression expression = new RDFNotExpression(new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN).ToString() }
        });

        RDFNotExpression expression = new RDFNotExpression(new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}
