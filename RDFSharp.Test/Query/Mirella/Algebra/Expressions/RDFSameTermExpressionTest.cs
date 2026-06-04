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
public class RDFSameTermExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateEESameTermExpression1()
    {
        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SAMETERM((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SAMETERM((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEESameTermExpression2()
    {
        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(SAMETERM((?V1 + ?V2), \"hello\"^^<{RDFVocabulary.XSD.STRING}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals($"(SAMETERM((?V1 + ?V2), \"hello\"^^<{RDFVocabulary.XSD.STRING}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(SAMETERM((?V1 + ?V2), \"hello\"^^xsd:string))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEESameTermExpressionNested()
    {
        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFSameTermExpression(new RDFVariableExpression(new RDFVariable("?V3")), new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US"))));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SAMETERM((?V1 + ?V2), (SAMETERM(?V3, \"hello\"@EN-US))))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SAMETERM((?V1 + ?V2), (SAMETERM(?V3, \"hello\"@EN-US))))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVSameTermExpression()
    {
        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SAMETERM((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SAMETERM((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVESameTermExpression()
    {
        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SAMETERM(?V3, (?V1 + ?V2)))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SAMETERM(?V3, (?V1 + ?V2)))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVSameTermExpression()
    {
        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(SAMETERM(?V3, ?V1))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(SAMETERM(?V3, ?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEESameTermExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSameTermExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVSameTermExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSameTermExpression(null as RDFExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVESameTermExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSameTermExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVSameTermExpressionNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSameTermExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("HELLO","EN-US").ToString() },
            { "?B", new RDFPlainLiteral("HELLO","EN-US").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/test1").ToString() },
            { "?B", new RDFResource("http://example.org/test1").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFPlainLiteral("ex:org").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
            { "?B", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("HELO", "EN-US").ToString() },
            { "?B", new RDFPlainLiteral("HELO").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://e.org/test1").ToString() },
            { "?B", new RDFResource("http://example.org/").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("I am ex:Org literal").ToString() },
            { "?B", new RDFResource("ex:org").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("helo", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString() },
            { "?B", new RDFTypedLiteral("helo", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResultEvenOnNullLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", null },
            { "?B", new RDFPlainLiteral("hello").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResultEvenOnNullRight()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
            { "?B", null },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResultEvenOnNullLeftAndRight1()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", null },
            { "?B", null },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResultEvenOnNullLeftAndRight2()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", null },
            { "?B", RDFPlainLiteral.Empty.ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariable("?C"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundRight()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?C"),
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
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
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?A"));
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
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFSameTermExpression expression = new RDFSameTermExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}