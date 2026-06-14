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
public class RDFStrLangDirExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateEEStrLangDirExpression1()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")),
            RDFQueryEnums.RDFLanguageDirections.LTR);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR((?V1 + ?V2), ?V3, \"ltr\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR((?V1 + ?V2), ?V3, \"ltr\"))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEStrLangDirExpression2()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            RDFQueryEnums.RDFLanguageDirections.RTL);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR((?V1 + ?V2), 25, \"rtl\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR((?V1 + ?V2), 25, \"rtl\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(STRLANGDIR((?V1 + ?V2), 25, \"rtl\"))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEStrLangDirExpressionNested()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFStrLangDirExpression(
                new RDFVariableExpression(new RDFVariable("?V3")),
                new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US")),
                RDFQueryEnums.RDFLanguageDirections.LTR),
            RDFQueryEnums.RDFLanguageDirections.RTL);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR((?V1 + ?V2), (STRLANGDIR(?V3, \"hello\"@EN-US, \"ltr\")), \"rtl\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR((?V1 + ?V2), (STRLANGDIR(?V3, \"hello\"@EN-US, \"ltr\")), \"rtl\"))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVStrLangDirExpression()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"),
            RDFQueryEnums.RDFLanguageDirections.LTR);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR((?V1 + ?V2), ?V3, \"ltr\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR((?V1 + ?V2), ?V3, \"ltr\"))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEStrLangDirExpression()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            RDFQueryEnums.RDFLanguageDirections.RTL);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR(?V3, (?V1 + ?V2), \"rtl\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR(?V3, (?V1 + ?V2), \"rtl\"))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVStrLangDirExpression()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?V3"),
            new RDFVariable("?V1"),
            RDFQueryEnums.RDFLanguageDirections.LTR);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR(?V3, ?V1, \"ltr\"))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR(?V3, ?V1, \"ltr\"))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEEStrLangDirExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangDirExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V")), RDFQueryEnums.RDFLanguageDirections.LTR));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVStrLangDirExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangDirExpression(null as RDFExpression, new RDFVariable("?V"), RDFQueryEnums.RDFLanguageDirections.LTR));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVEStrLangDirExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangDirExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V")), RDFQueryEnums.RDFLanguageDirections.LTR));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVStrLangDirExpressionNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangDirExpression(null as RDFVariable, new RDFVariable("?V"), RDFQueryEnums.RDFLanguageDirections.LTR));

    //EE

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultOnSimplePlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultOnStringBasedTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", $"hello^^{RDFVocabulary.XSD.STRING}" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.RTL);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--rtl")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotWellFormedLanguageTag()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", "en-" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotSimplePlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello@EN-US" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotStringBasedTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", $"34^^{RDFVocabulary.XSD.BYTE}" },
            { "?B", "en-" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //EV

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultOnSimplePlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultOnStringBasedTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", $"hello^^{RDFVocabulary.XSD.STRING}" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.RTL);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--rtl")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseNotWellFormedLanguageTag()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", "en-" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseNotSimplePlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello@EN-US" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseNotStringBasedTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", $"34^^{RDFVocabulary.XSD.BYTE}" },
            { "?B", "en-" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //VE

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultOnSimplePlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultOnStringBasedTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", $"hello^^{RDFVocabulary.XSD.STRING}" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndNotCalculateResultBecauseNotWellFormedLanguageTag()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", "en-" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndNotCalculateResultBecauseNotSimplePlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello@EN-US" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndNotCalculateResultBecauseNotStringBasedTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", $"34^^{RDFVocabulary.XSD.BYTE}" },
            { "?B", "en-" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //VV

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultOnSimplePlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultOnStringBasedTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", $"hello^^{RDFVocabulary.XSD.STRING}" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNotWellFormedLanguageTag()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", "en-" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNotSimplePlainLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello@EN-US" },
            { "?B", "en-US" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNotStringBasedTypedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", $"34^^{RDFVocabulary.XSD.BYTE}" },
            { "?B", "en-" }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", RDFVocabulary.XSD.FLOAT.ToString() },
            { "?B", "en-US" },
            { "?C", new RDFPlainLiteral("C").ToString() }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableRight()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", "hello" },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() }
        });

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}