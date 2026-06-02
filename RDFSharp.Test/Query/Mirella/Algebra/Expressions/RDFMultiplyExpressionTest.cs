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
public class RDFMultiplyExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateMultiplyExpressionWithExpressions()
    {
        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFMultiplyExpression(new RDFVariable("?V3"), new RDFVariable("?V4")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 * ?V2) * (?V3 * ?V4))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 * ?V2) * (?V3 * ?V4))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateMultiplyExpressionWithExpressionAndVariable()
    {
        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 * ?V2) * ?V3)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 * ?V2) * ?V3)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateMultiplyExpressionWithExpressionAndTypedLiteral()
    {
        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 * ?V2) * 25.1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 * ?V2) * 25.1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateMultiplyExpressionWithVariableAndExpression()
    {
        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFVariable("?V1"),
            new RDFMultiplyExpression(new RDFVariable("?V2"), new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 * (?V2 * ?V3))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 * (?V2 * ?V3))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateMultiplyExpressionWithVariables()
    {
        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 * ?V2)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 * ?V2)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateMultiplyExpressionWithVariableAndTypedLiteral()
    {
        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFVariable("?V"),
            new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V * 25.1)", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V * 25.1)", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithEEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(null as RDFMathExpression, new RDFMultiplyExpression(new RDFVariable("?V1"), new RDFVariable("?V2"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithEVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(null as RDFMathExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithETBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(null as RDFMathExpression, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithVEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(null as RDFVariable, new RDFMultiplyExpression(new RDFVariable("?V1"), new RDFVariable("?V2"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithVVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithVTBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(null as RDFVariable, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithEEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(new RDFMultiplyExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), null as RDFMathExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithEVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(new RDFMultiplyExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithETBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(new RDFMultiplyExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithVEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(new RDFVariable("?V2"), null as RDFMathExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithVVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(new RDFVariable("?V"), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMultiplyExpressionWithVTBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFMultiplyExpression(new RDFVariable("?V"), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("15625", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("625", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithETAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFTypedLiteral("8.2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("1025", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFVariable("?A"),
            new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("625", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("125", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVTAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFVariable("?A"),
            new RDFTypedLiteral("8.24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("42.024", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnknownLeftExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?Z"), new RDFVariable("?B")),
            new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnknownRightExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFMultiplyExpression(new RDFVariable("?Z"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseUnknownLeftExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?Z"), new RDFVariable("?B")),
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseUnknownRightExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariable("?Z"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithETAndNotCalculateResultBecauseUnknownExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(
            new RDFMultiplyExpression(new RDFVariable("?Z"), new RDFVariable("?B")),
            new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseUnknownLeftColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?Z"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseUnknownRightColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?Z"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseResourceLeftColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("ex:subj").ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecausePlainLiteralLeftColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("55").ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseTypedLiteralNotNumericLeftColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("---05", RDFModelEnums.RDFDatatypes.XSD_GDAY).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseResourceRightColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFResource("ex:subj").ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecausePlainLiteralRightColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("55").ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseTypedLiteralNotNumericRightColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
            { "?B", new RDFTypedLiteral("---05", RDFModelEnums.RDFDatatypes.XSD_GDAY).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNullLeftColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", null },
            { "?B", new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNullRightColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString() },
            { "?B", null },
        });

        RDFMultiplyExpression expression = new RDFMultiplyExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}