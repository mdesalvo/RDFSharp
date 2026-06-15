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
public class RDFBNodeExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateBNodeExpression()
    {
        RDFBNodeExpression expression = new RDFBNodeExpression();

        Assert.IsNotNull(expression);
        Assert.IsNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(BNODE())", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(BNODE())", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFBNodeExpression expression = new RDFBNodeExpression();

        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);
        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult is RDFResource { IsBlank: true });

        RDFPatternMember expressionResult2 = expression.ApplyExpression(table.Rows[0]);
        Assert.IsTrue(expressionResult2 is RDFResource { IsBlank: true });
        Assert.IsFalse(expressionResult.Equals(expressionResult2));
    }

    [TestMethod]
    public void ShouldCreateBNodeExpressionWithExpressionArgument()
    {
        RDFBNodeExpression expression = new RDFBNodeExpression(
            new RDFVariableExpression(new RDFVariable("?V1")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(BNODE(?V1))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(BNODE(?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateBNodeExpressionWithVariableArgument()
    {
        RDFBNodeExpression expression = new RDFBNodeExpression(
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsTrue(expression.ToString().Equals("(BNODE(?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithArgumentAndCalculateDeterministicResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("seed").ToString() }
        });

        RDFBNodeExpression expression = new RDFBNodeExpression(new RDFVariable("?A"));

        //Same argument value => the very same blank node on every evaluation (deterministic, hash-derived)
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);
        RDFPatternMember expressionResult2 = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult is RDFResource { IsBlank: true });
        Assert.IsTrue(expressionResult.Equals(expressionResult2));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithArgumentAndCalculateNullResultOnUnboundVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("seed").ToString() }
        });

        RDFBNodeExpression expression = new RDFBNodeExpression(new RDFVariable("?NOTBOUND"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}