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
public class RDFComparisonExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateEEComparisonExpressionLess()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("24.08", RDFModelEnums.RDFDatatypes.XSD_FLOAT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) < 24.08)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) < 24.08)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEComparisonExpressionLessOrEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) <= ?V3)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) <= ?V3)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEComparisonExpressionEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFVariableExpression(new RDFVariable("?V1")),
            new RDFVariableExpression(new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 = ?V2)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 = ?V2)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEComparisonExpressionNotEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
            new RDFVariableExpression(new RDFVariable("?V1")),
            new RDFVariableExpression(new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 != ?V2)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 != ?V2)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEComparisonExpressionGreaterOrEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("24.08", RDFModelEnums.RDFDatatypes.XSD_FLOAT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) >= 24.08)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) >= 24.08)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEComparisonExpressionGreater()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) > ?V3)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) > ?V3)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVComparisonExpressionLess()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) < ?V3)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) < ?V3)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVComparisonExpressionLessOrEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) <= ?V3)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) <= ?V3)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVComparisonExpressionEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFVariableExpression(new RDFVariable("?V1")),
            new RDFVariable("?V2"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 = ?V2)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 = ?V2)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVComparisonExpressionNotEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
            new RDFVariableExpression(new RDFVariable("?V1")),
            new RDFVariable("?V2"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 != ?V2)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 != ?V2)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVComparisonExpressionGreaterOrEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) >= ?V3)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) >= ?V3)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVComparisonExpressionGreater()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) > ?V3)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) > ?V3)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEComparisonExpressionLess()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V3 < (?V1 + ?V2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V3 < (?V1 + ?V2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEComparisonExpressionLessOrEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V3 <= (?V1 + ?V2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V3 <= (?V1 + ?V2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEComparisonExpressionEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFVariable("?V2"),
            new RDFVariableExpression(new RDFVariable("?V1")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V2 = ?V1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V2 = ?V1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEComparisonExpressionNotEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
            new RDFVariable("?V2"),
            new RDFVariableExpression(new RDFVariable("?V1")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V2 != ?V1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V2 != ?V1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEComparisonExpressionGreaterOrEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V3 >= (?V1 + ?V2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V3 >= (?V1 + ?V2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEComparisonExpressionGreater()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V3 > (?V1 + ?V2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V3 > (?V1 + ?V2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVComparisonExpressionLess()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V3 < ?V1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V3 < ?V1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVComparisonExpressionLessOrEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V3 <= ?V1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V3 <= ?V1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVComparisonExpressionEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFVariable("?V2"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V2 = ?V1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V2 = ?V1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVComparisonExpressionNotEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
            new RDFVariable("?V2"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V2 != ?V1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V2 != ?V1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVComparisonExpressionGreaterOrEqual()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V3 >= ?V1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V3 >= ?V1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVComparisonExpressionGreater()
    {
        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V3 > ?V1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V3 > ?V1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEEComparisonExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null as RDFMathExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVComparisonExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null as RDFMathExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVEComparisonExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVComparisonExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEEComparisonExpressionBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariableExpression(new RDFVariable("?V")), null as RDFMathExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVComparisonExpressionBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariable("?V"), null as RDFMathExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVEComparisonExpressionBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVComparisonExpressionBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariable("?V"), null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultTrue()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariableExpression(new RDFVariable("?B")));
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
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("10/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultTrue()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultTrue()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
            new RDFVariable("?B"),
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariable("?B"),
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseTypeError()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
            { "?C", new RDFResource("ex:subj").ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFVariable("?C"),
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseBindingErrorOnExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:subj").ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariable("?B"),
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseBindingErrorOnVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
            { "?C", null }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariable("?C"),
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableInExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariable("?B"),
            new RDFAddExpression(new RDFVariable("?Q"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableInLeftVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            new RDFVariable("?Q"),
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableInRightVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}