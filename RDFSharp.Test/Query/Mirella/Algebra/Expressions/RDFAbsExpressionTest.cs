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
public class RDFAbsExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateAbsExpressionWithExpression()
    {
        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(ABS((?V1 + ?V2)))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(ABS((?V1 + ?V2)))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateAbsExpressionWithVariable()
    {
        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(ABS(?V1))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(ABS(?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAbsExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFAbsExpression(null as RDFMathExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAbsExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFAbsExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("10/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("20.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("-11/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("5.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotNumericLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseUnboundLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFAddExpression(new RDFVariable("?C"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotNumericLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseUnboundLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFVariable("?C"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    [DataRow("-5^^http://www.w3.org/2001/XMLSchema#negativeInteger", "5^^http://www.w3.org/2001/XMLSchema#integer")]
    [DataRow("-2.5^^http://www.w3.org/2001/XMLSchema#decimal", "2.5^^http://www.w3.org/2001/XMLSchema#decimal")]
    [DataRow("-2.5^^http://www.w3.org/2001/XMLSchema#double", "2.5^^http://www.w3.org/2001/XMLSchema#double")]
    [DataRow("-1.5^^http://www.w3.org/2001/XMLSchema#float", "1.5^^http://www.w3.org/2001/XMLSchema#float")]
    [DataRow("-INF^^http://www.w3.org/2001/XMLSchema#double", "INF^^http://www.w3.org/2001/XMLSchema#double")]
    [DataRow("NaN^^http://www.w3.org/2001/XMLSchema#double", "NaN^^http://www.w3.org/2001/XMLSchema#double")]
    [DataRow("-7/2^^http://www.w3.org/2002/07/owl#rational", "3.5^^http://www.w3.org/2001/XMLSchema#decimal")]
    public void ShouldApplyExpressionWithVariableAndGiveBaseNumericTypeOfArgument(string argumentValue, string expectedValue)
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", argumentValue }
        });

        RDFAbsExpression expression = new RDFAbsExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.AreEqual(expectedValue, expressionResult.ToString());
    }
    #endregion
}