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
using System.Text.RegularExpressions;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFRegexExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateRegexExpressionWithExpression()
    {
        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), new Regex("^hello$"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(REGEX(STR((?V1 + ?V2)), \"^hello$\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(REGEX(STR((?V1 + ?V2)), \"^hello$\"))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateRegexExpressionWithVariable()
    {
        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?V1"), new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(REGEX(STR(?V1), \"^hello$\", \"ismx\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(REGEX(STR(?V1), \"^hello$\", \"ismx\"))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFRegexExpression(null as RDFExpression, new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithExpressionBecauseNullRegex()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFRegexExpression(new RDFVariableExpression(new RDFVariable("?V1")), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFRegexExpression(null as RDFVariable, new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithVariableBecauseNullRegex()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFRegexExpression(new RDFVariable("?V1"), null));

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", null }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hEllo").ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("^he", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello","en-US").ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("US$", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("ell"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:subj").ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("^ex:"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotBoundVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:subj").ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?Q")), new Regex("^ex:"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", null }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hEllo").ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("^he", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello","en-US").ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("US$", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("ell"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:subj").ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("^ex:"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotBoundVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:subj").ToString() }
        });

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?Q"), new Regex("^ex:"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}