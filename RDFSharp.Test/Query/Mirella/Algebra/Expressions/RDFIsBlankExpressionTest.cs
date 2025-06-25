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
public class RDFIsBlankExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateIsBlankExpressionWithExpression()
    {
        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(ISBLANK((?V1 + ?V2)))"));
        Assert.IsTrue(expression.ToString([]).Equals("(ISBLANK((?V1 + ?V2)))"));
    }

    [TestMethod]
    public void ShouldCreateIsBlankExpressionWithVariable()
    {
        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(ISBLANK(?V1))"));
        Assert.IsTrue(expression.ToString([]).Equals("(ISBLANK(?V1))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingIsBlankExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFIsBlankExpression(null as RDFExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingIsBlankExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFIsBlankExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNull()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteralWithLanguage()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello","en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnResource()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:subj").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }
    
    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnBlankResource()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource().ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotBoundVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:subj");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariableExpression(new RDFVariable("?Q")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnNull()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteralWithLanguage()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnResource()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:subj").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }
    
    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnBlankResource()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("bnode:12345").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotBoundVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFIsBlankExpression expression = new RDFIsBlankExpression(
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}