﻿/*
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
public class RDFTimeZoneExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateTimeZoneExpressionWithExpression()
    {
        RDFTimeZoneExpression expression = new RDFTimeZoneExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(TIMEZONE((?V1 + ?V2)))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(TIMEZONE((?V1 + ?V2)))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateTimeZoneExpressionWithVariable()
    {
        RDFTimeZoneExpression expression = new RDFTimeZoneExpression(
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(TIMEZONE(?V1))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(TIMEZONE(?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingTimeZoneExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFTimeZoneExpression(null as RDFExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingTimeZoneExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFTimeZoneExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("2022-01-15T10:30:24.612Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFTimeZoneExpression expression = new RDFTimeZoneExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("PT0S", RDFModelEnums.RDFDatatypes.XSD_DURATION)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("2022-01-15T10:30:24.612", RDFModelEnums.RDFDatatypes.XSD_DATETIME).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFTimeZoneExpression expression = new RDFTimeZoneExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("PT0S", RDFModelEnums.RDFDatatypes.XSD_DURATION)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotDateTimeLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFTimeZoneExpression expression = new RDFTimeZoneExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseUnboundLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("2022-01-15T10:00:00.000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFTimeZoneExpression expression = new RDFTimeZoneExpression(
            new RDFVariableExpression(new RDFVariable("?C")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotDateTimeLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFTimeZoneExpression expression = new RDFTimeZoneExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseUnboundLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("2022-01-15T10:00:00.000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFTimeZoneExpression expression = new RDFTimeZoneExpression(
            new RDFVariable("?C"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}