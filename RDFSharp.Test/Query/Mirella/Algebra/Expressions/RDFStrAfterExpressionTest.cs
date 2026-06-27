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
public class RDFStrAfterExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateStrAfterExpressionWithExpressions()
    {
        RDFStrAfterExpression expression = new RDFStrAfterExpression(
            new RDFVariableExpression(new RDFVariable("?V1")),
            new RDFVariableExpression(new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRAFTER(?V1, ?V2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRAFTER(?V1, ?V2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateStrAfterExpressionWithVariables()
    {
        RDFStrAfterExpression expression = new RDFStrAfterExpression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRAFTER(?V1, ?V2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingStrAfterExpressionBecauseNullLeftArgumentExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrAfterExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingStrAfterExpressionBecauseNullLeftArgumentVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrAfterExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResultAfterNeedle()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("abcdef").ToString() },
            { "?B", new RDFPlainLiteral("cd").ToString() }
        });

        RDFStrAfterExpression expression = new RDFStrAfterExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ef")));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndPreserveSourceLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?B", new RDFPlainLiteral("he").ToString() }
        });

        RDFStrAfterExpression expression = new RDFStrAfterExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("llo", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateEmptyResultBecauseNeedleNotFound()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("abc").ToString() },
            { "?B", new RDFPlainLiteral("zzz").ToString() }
        });

        RDFStrAfterExpression expression = new RDFStrAfterExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty)));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateNullResultBecauseUnboundVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("abc").ToString() }
        });

        RDFStrAfterExpression expression = new RDFStrAfterExpression(
            new RDFVariable("?A"),
            new RDFVariable("?NOTBOUND"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}
