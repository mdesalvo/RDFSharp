/*
   Copyright 2012-2025 Marco De Salvo

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
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFSubtractExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateAddExpressionWithExpressions()
    {
        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), 
            new RDFSubtractExpression(new RDFVariable("?V3"), new RDFVariable("?V4")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 - ?V2) - (?V3 - ?V4))"));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 - ?V2) - (?V3 - ?V4))"));
    }

    [TestMethod]
    public void ShouldCreateAddExpressionWithExpressionAndVariable()
    {
        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 - ?V2) - ?V3)"));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 - ?V2) - ?V3)"));
    }

    [TestMethod]
    public void ShouldCreateAddExpressionWithExpressionAndTypedLiteral()
    {
        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 - ?V2) - 25.1)"));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 - ?V2) - 25.1)"));
    }

    [TestMethod]
    public void ShouldCreateAddExpressionWithVariableAndExpression()
    {
        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFVariable("?V1"),
            new RDFSubtractExpression(new RDFVariable("?V2"), new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 - (?V2 - ?V3))"));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 - (?V2 - ?V3))"));
    }

    [TestMethod]
    public void ShouldCreateAddExpressionWithVariables()
    {
        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFVariable("?V1"), 
            new RDFVariable("?V2"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 - ?V2)"));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 - ?V2)"));
    }

    [TestMethod]
    public void ShouldCreateAddExpressionWithVariableAndTypedLiteral()
    {
        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFVariable("?V"), 
            new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V - 25.1)"));
        Assert.IsTrue(expression.ToString([]).Equals("(?V - 25.1)"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithEEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(null as RDFMathExpression, new RDFSubtractExpression(new RDFVariable("?V1"), new RDFVariable("?V2"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithEVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(null as RDFMathExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithETBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(null as RDFMathExpression, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithVEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(null as RDFVariable, new RDFSubtractExpression(new RDFVariable("?V1"), new RDFVariable("?V2"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithVVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithVTBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(null as RDFVariable, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithEEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(new RDFSubtractExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), null as RDFMathExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithEVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(new RDFSubtractExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithETBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(new RDFSubtractExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithVEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(new RDFVariable("?V2"), null as RDFMathExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithVVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(new RDFVariable("?V"), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAddExpressionWithVTBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFSubtractExpression(new RDFVariable("?V"), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFSubtractExpression(new RDFVariable("?B"), new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("-39.8", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("-25", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithETAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFTypedLiteral("8.24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("-28.14", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFVariable("?A"),
            new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("-19.9", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVTAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFVariable("?A"),
            new RDFTypedLiteral("8.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("-3", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnknownLeftExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?Z"), new RDFVariable("?B")),
            new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnknownRightExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFSubtractExpression(new RDFVariable("?Z"), new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseUnknownLeftExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?Z"), new RDFVariable("?B")),
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseUnknownRightExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B")),
            new RDFVariable("?Z"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithETAndNotCalculateResultBecauseUnknownExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(
            new RDFSubtractExpression(new RDFVariable("?Z"), new RDFVariable("?B")),
            new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseUnknownLeftColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?Z"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseUnknownRightColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?Z"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseResourceLeftColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:subj").ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecausePlainLiteralLeftColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("55").ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseTypedLiteralNotNumericLeftColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("---05", RDFModelEnums.RDFDatatypes.XSD_GDAY).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseResourceRightColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(); 
        row["?B"] = new RDFResource("ex:subj").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecausePlainLiteralRightColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row["?B"] = new RDFPlainLiteral("55").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseTypedLiteralNotNumericRightColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        row["?B"] = new RDFTypedLiteral("---05", RDFModelEnums.RDFDatatypes.XSD_GDAY).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNullLeftColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null;
        row["?B"] = new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNullRightColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString();
        row["?B"] = null;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSubtractExpression expression = new RDFSubtractExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}