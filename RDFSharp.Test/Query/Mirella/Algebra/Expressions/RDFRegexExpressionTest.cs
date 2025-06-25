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
using System.Text.RegularExpressions;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFRegexExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateRegexExpressionWithExpression()
    {
        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")), new Regex("^hello$"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(REGEX(STR((?V1 + ?V2)), \"^hello$\"))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REGEX(STR((?V1 + ?V2)), \"^hello$\"))"));
    }

    [TestMethod]
    public void ShouldCreateRegexExpressionWithVariable()
    {
        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?V1"), new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(REGEX(STR(?V1), \"^hello$\", \"ismx\"))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REGEX(STR(?V1), \"^hello$\", \"ismx\"))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFRegexExpression(null as RDFExpression, new Regex("^hello$")));
    
    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithExpressionBecauseNullRegex()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFRegexExpression(new RDFVariableExpression(new RDFVariable("?V1")), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFRegexExpression(null as RDFVariable, new Regex("^hello$")));
    
    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithVariableBecauseNullRegex()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFRegexExpression(new RDFVariable("?V1"), null));

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNull()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("^hello$"));
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
        row["?A"] = new RDFPlainLiteral("hEllo").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("^he", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
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

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("US$", RegexOptions.IgnoreCase));
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

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("ell"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
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

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?A")), new Regex("^ex:"));
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

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariableExpression(new RDFVariable("?Q")), new Regex("^ex:"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
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

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("^hello$"));
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
        row["?A"] = new RDFPlainLiteral("hEllo").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("^he", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteralWithLanguage()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello","en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("US$", RegexOptions.IgnoreCase));
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

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("ell"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
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

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?A"), new Regex("^ex:"));
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
        row["?A"] = new RDFResource("ex:subj");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFRegexExpression expression = new RDFRegexExpression(
            new RDFVariable("?Q"), new Regex("^ex:"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}