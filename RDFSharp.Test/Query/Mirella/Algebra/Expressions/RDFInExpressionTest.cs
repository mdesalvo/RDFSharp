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
public class RDFInExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateInExpressionWithExpression()
    {
        RDFInExpression expression = new RDFInExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            [new RDFPlainLiteral("hello","en-US"), null]);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) IN (\"hello\"@EN-US))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("((?V1 + ?V2) IN (\"hello\"@EN-US))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateInExpressionWithVariable()
    {
        RDFInExpression expression = new RDFInExpression(
            new RDFVariable("?V1"),
            [new RDFPlainLiteral("hello","en-US"), null]);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(?V1 IN (\"hello\"@EN-US))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(?V1 IN (\"hello\"@EN-US))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultTrue()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariableExpression(new RDFVariable("?B")),
            [new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL), new RDFPlainLiteral("hello")]);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultTrueWithSearchVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
        row["?C"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariableExpression(new RDFVariable("?B")),
            [new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL), new RDFPlainLiteral("hello"), new RDFVariable("?C")]);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultFalse()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            [new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL), new RDFPlainLiteral("hello")]);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultFalseBecauseNoTerms()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            null);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultFalseBecauseUnknownColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariableExpression(new RDFVariable("?Q")),
            [new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL), new RDFPlainLiteral("hello")]);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultTrue()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:org").ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariable("?A"),
            [new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL), new RDFPlainLiteral("hello"), new RDFResource("ex:org")]);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultTrueWithSearchVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:org").ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariable("?A"),
            [new RDFVariable("?B"), new RDFPlainLiteral("hello"), new RDFResource("ex:org")]);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultFalse()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:org2").ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariable("?A"),
            [new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL), new RDFPlainLiteral("hello"), new RDFResource("ex:org")]);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultFalseBecauseNoTerms()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariable("?A"),
            null);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultFalseBecauseUnknownColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInExpression expression = new RDFInExpression(
            new RDFVariable("?Q"),
            [new RDFTypedLiteral("50/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL), new RDFPlainLiteral("hello")]);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}