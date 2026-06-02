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
public class RDFCoalesceExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateEECoalesceExpression1()
    {
        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(COALESCE((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(COALESCE((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEECoalesceExpression2()
    {
        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(COALESCE((?V1 + ?V2), \"hello\"^^<{RDFVocabulary.XSD.STRING}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals($"(COALESCE((?V1 + ?V2), \"hello\"^^<{RDFVocabulary.XSD.STRING}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(COALESCE((?V1 + ?V2), \"hello\"^^xsd:string))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEECoalesceExpressionNested()
    {
        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFCoalesceExpression(new RDFVariableExpression(new RDFVariable("?V3")), new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US"))));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(COALESCE((?V1 + ?V2), (COALESCE(?V3, \"hello\"@EN-US))))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(COALESCE((?V1 + ?V2), (COALESCE(?V3, \"hello\"@EN-US))))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVCoalesceExpression()
    {
        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(COALESCE((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(COALESCE((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVECoalesceExpression()
    {
        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(COALESCE(?V3, (?V1 + ?V2)))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(COALESCE(?V3, (?V1 + ?V2)))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVCoalesceExpression()
    {
        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(COALESCE(?V3, ?V1))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(COALESCE(?V3, ?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEECoalesceExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFCoalesceExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVCoalesceExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFCoalesceExpression(null as RDFExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVECoalesceExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFCoalesceExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVCoalesceExpressionNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFCoalesceExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultCoalesceLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFAddExpression(new RDFConstantExpression(RDFTypedLiteral.One), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultCoalesceRight()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFAddExpression(new RDFConstantExpression(RDFTypedLiteral.One), new RDFVariable("?B")),
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultCoalesceNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?A")),
            new RDFAddExpression(new RDFConstantExpression(RDFTypedLiteral.One), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultCoalesceLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultCoalesceRight()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultCoalesceNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?W"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithNestedCoalesce()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() },
            { "?C", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFCoalesceExpression(
                new RDFSubstringExpression(new RDFVariable("?C"), 1),
                new RDFAddExpression(new RDFConstantExpression(RDFTypedLiteral.One), new RDFVariable("?B"))),
            new RDFCoalesceExpression(
                new RDFVariableExpression(new RDFVariable("?A")),
                new RDFAddExpression(new RDFConstantExpression(RDFTypedLiteral.One), new RDFVariable("?B"))));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithNestedCoalesceNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() },
            { "?C", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFCoalesceExpression(
                new RDFSubstringExpression(new RDFVariable("?C"), 1),
                new RDFAddExpression(new RDFConstantExpression(RDFTypedLiteral.One), new RDFVariable("?B"))),
            new RDFCoalesceExpression(
                new RDFVariableExpression(new RDFVariable("?Q")),
                new RDFVariable("?Y")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithDeepNestedCoalesce()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() },
            { "?C", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFCoalesceExpression(
                new RDFCoalesceExpression(
                    new RDFSubstringExpression(new RDFVariable("?C"), 1),
                    new RDFAddExpression(new RDFConstantExpression(RDFTypedLiteral.One), new RDFVariable("?B"))),
                new RDFCoalesceExpression(
                    new RDFVariableExpression(new RDFVariable("?T")),
                    new RDFAddExpression(new RDFVariable("?C"), new RDFVariable("?B")))),
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithDeepNestedCoalesceNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() },
            { "?C", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
        });

        RDFCoalesceExpression expression = new RDFCoalesceExpression(
            new RDFCoalesceExpression(
                new RDFCoalesceExpression(
                    new RDFSubstringExpression(new RDFVariable("?C"), 1),
                    new RDFAddExpression(new RDFConstantExpression(RDFTypedLiteral.One), new RDFVariable("?B"))),
                new RDFCoalesceExpression(
                    new RDFVariableExpression(new RDFVariable("?T")),
                    new RDFAddExpression(new RDFVariable("?C"), new RDFVariable("?B")))),
            new RDFVariable("?Z"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}