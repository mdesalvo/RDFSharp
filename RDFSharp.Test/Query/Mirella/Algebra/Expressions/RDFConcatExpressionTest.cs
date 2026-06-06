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
public class RDFConcatExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateEEConcatExpression1()
    {
        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(CONCAT((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(CONCAT((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEConcatExpression2()
    {
        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(CONCAT((?V1 + ?V2), 25))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(CONCAT((?V1 + ?V2), 25))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(CONCAT((?V1 + ?V2), 25))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEConcatExpressionNested()
    {
        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?V3")), new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US"))));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(CONCAT((?V1 + ?V2), (CONCAT(?V3, \"hello\"@EN-US))))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(CONCAT((?V1 + ?V2), (CONCAT(?V3, \"hello\"@EN-US))))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVConcatExpression()
    {
        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(CONCAT((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(CONCAT((?V1 + ?V2), ?V3))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEConcatExpression()
    {
        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(CONCAT(?V3, (?V1 + ?V2)))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(CONCAT(?V3, (?V1 + ?V2)))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVConcatExpression()
    {
        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(CONCAT(?V3, ?V1))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(CONCAT(?V3, ?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEEConcatExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFConcatExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVConcatExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFConcatExpression(null as RDFExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVEConcatExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFConcatExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVConcatExpressionNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFConcatExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("A").ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("AB")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("A").ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("AB")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("A").ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("AB")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("A").ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("AB")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVV2AndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("---13", RDFModelEnums.RDFDatatypes.XSD_GDAY).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("---13ZB")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVV3AndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/B")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVV4AndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("B").ToString() },
            { "?B", new RDFResource("http://example.org/").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("Bhttp://example.org/")));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateComplexResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?V1");
        table.AddColumn("?V2");
        table.AddColumn("?V3");
        table.AddRow(new Dictionary<string, string>
        {
            { "?V1", new RDFTypedLiteral("56", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
            { "?V2", new RDFTypedLiteral("4.2", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?V3", new RDFResource("ex:org").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConcatExpression(
                new RDFVariableExpression(new RDFVariable("?V3")),
                new RDFConstantExpression(new RDFPlainLiteral("hello","en-US"))));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("60.2ex:orghello")));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResultAllEmpties()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?V1");
        table.AddColumn("?V2");
        table.AddRow(new Dictionary<string, string>
        {
            { "?V1", null },
            { "?V2", new RDFTypedLiteral("4.2", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResultOnUnboundLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFConcatExpression(new RDFConcatExpression(new RDFVariable("?C"), new RDFVariable("?C")), new RDFVariable("?C")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("CCC")));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResultOnUnboundRight()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFConcatExpression(new RDFConcatExpression(new RDFVariable("?C"), new RDFVariable("?C")), new RDFVariable("?C")),
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("CCC")));
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
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
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
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() }
        });

        RDFConcatExpression expression = new RDFConcatExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}