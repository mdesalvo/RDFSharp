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
public class RDFSubstringExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateSubstringExpressionWithExpressionAndStart()
    {
        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?V")), 5);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateSubstringExpressionWithExpressionAndStartAndLength()
    {
        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?V")), 5, 2);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5, 2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5, 2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateSubstringExpressionWithExpressionAndStartAndLengthNull()
    {
        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?V")), 5);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateSubstringExpressionWithVariableAndStart()
    {
        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?V"), 5);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateSubstringExpressionWithVariableAndStartAndLength()
    {
        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?V"), 5, 2);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5, 2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5, 2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateSubstringExpressionWithVariableAndStartAndLengthNull()
    {
        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?V"), 5);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingESubstringExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubstringExpression(null as RDFExpression, 5));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVSubstringExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubstringExpression(null as RDFVariable, 5));

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ttp://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithNestedExpressionAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFSubstringExpression(new RDFVariableExpression(new RDFVariable("?A")), 2), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("tp://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2, 7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ttp://e")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2, 7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndNegativeLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExactEndingStartAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 19);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingStartAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 20);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingStartAndLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 20, 4);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingStartAndExceedingLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 20, 44);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingStartAndNegativeLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 20, -4);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExactEndingLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1, 19);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1, 20);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", null },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseUnboundColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?A")), 1);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseUnknownColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFAddExpression(new RDFVariable("?Q"), new RDFVariable("?A")), 1);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotStringTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndNegativeLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExactEndingStartAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingStartAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExactEndingLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1, 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1, 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndNegativeLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExactEndingStartAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingStartAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExactEndingLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1, 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1, 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNegativeStartAndNegativeLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), -2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExactEndingStartAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingStartAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExactEndingLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1, 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndExceedingLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariableExpression(new RDFVariable("?A")), 1, 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ttp://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2, 7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ttp://e")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2, 7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndNegativeLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExactEndingStartAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 19);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingStartAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 20);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingStartAndLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 20, 4);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingStartAndExceedingLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 20, 44);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingStartAndNegativeLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 20, -4);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExactEndingLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1, 19);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingLengthAndCalculateResultOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1, 20);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", null },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseUnknownColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?Q"), 1);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotStringTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndNegativeLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExactEndingStartAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingStartAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExactEndingLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1, 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingLengthAndCalculateResultOnPlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1, 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndNegativeLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExactEndingStartAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingStartAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExactEndingLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1, 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingLengthAndCalculateResultOnPlainLiteralWithLanguage()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1, 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2, 3);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNegativeStartAndNegativeLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), -2, -7);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExactEndingStartAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingStartAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExactEndingLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1, 5);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndExceedingLengthAndCalculateResultOnTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSubstringExpression expression = new RDFSubstringExpression(
            new RDFVariable("?A"), 1, 6);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
    }
    #endregion
}