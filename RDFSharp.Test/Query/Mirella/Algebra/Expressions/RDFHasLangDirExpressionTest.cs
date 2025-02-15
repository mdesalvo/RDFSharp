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
public class RDFHasLangDirExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateHasLangDirExpressionWithExpression()
    {
        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(HASLANGDIR((?V1 + ?V2)))"));
        Assert.IsTrue(expression.ToString([]).Equals("(HASLANGDIR((?V1 + ?V2)))"));
    }

    [TestMethod]
    public void ShouldCreateHasLangDirExpressionWithVariable()
    {
        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(HASLANGDIR(?V1))"));
        Assert.IsTrue(expression.ToString([]).Equals("(HASLANGDIR(?V1))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingHasLangDirExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFHasLangDirExpression(null as RDFExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingHasLangDirExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFHasLangDirExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNull()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteralWithLanguageLTR()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello", "en-US--ltr").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteralWithLanguageRTL()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello", "en-US--rtl").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotBoundVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?Q")));
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteralWithLanguageLTR()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello", "en-US--ltr").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteralWithLanguageRTL()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello", "en-US--rtl").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
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

        RDFHasLangDirExpression expression = new RDFHasLangDirExpression(
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}