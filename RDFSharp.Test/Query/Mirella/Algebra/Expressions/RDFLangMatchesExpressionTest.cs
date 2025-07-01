/*
   Copyright 2012-2025 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific LangMatchesuage governing permissions and
   limitations under the License.
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFLangMatchesExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateLangMatchesExpressionWithExpressionExpression()
    {
        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFPlainLiteral("en-US--ltr")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(LANGMATCHES(LANG((?V1 + ?V2)),\"en-US--ltr\"))"));
        Assert.IsTrue(expression.ToString([]).Equals("(LANGMATCHES(LANG((?V1 + ?V2)),\"en-US--ltr\"))"));
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionWithExpressionVariable()
    {
        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?L"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(LANGMATCHES(LANG((?V1 + ?V2)),?L))"));
        Assert.IsTrue(expression.ToString([]).Equals("(LANGMATCHES(LANG((?V1 + ?V2)),?L))"));
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionWithVariableExpression()
    {
        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(
            new RDFVariable("?L"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(LANGMATCHES(LANG(?L),(?V1 + ?V2)))"));
        Assert.IsTrue(expression.ToString([]).Equals("(LANGMATCHES(LANG(?L),(?V1 + ?V2)))"));
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionWithVariableVariable()
    {
        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(
            new RDFVariable("?L"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(LANGMATCHES(LANG(?L),?V1))"));
        Assert.IsTrue(expression.ToString([]).Equals("(LANGMATCHES(LANG(?L),?V1))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLangMatchesExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLangMatchesExpression(null as RDFExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLangMatchesExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLangMatchesExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLangMatchesExpressionWithExpressionBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLangMatchesExpressionWithVariableBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLangMatchesExpression(new RDFVariable("?V"), null as RDFVariable));

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultTrue()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("en-us")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionWithLeftExpressionAndCalculateResultTrue()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?B")), new RDFConstantExpression(new RDFPlainLiteral("en-us")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultTrueOnSuperLangTag()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US--rtl").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("EN")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionWithLeftExpressionAndCalculateResultTrueOnSuperLangTag()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US--rtl").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?B")), new RDFConstantExpression(new RDFPlainLiteral("EN")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultTrueOnRightVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("en-US").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariable("?A"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionWithLeftExpressionAndCalculateResultTrueOnRightVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("en-US").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?B")), new RDFVariable("?A"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalseOnSuperLangTag()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("EN-US")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalse()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("en-UK")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalseOnNullLeftColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = DBNull.Value;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Empty));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalseOnUnlanguagedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("en-UK")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalseOnResource()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?A"), new RDFConstantExpression(new RDFPlainLiteral("EN-US")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndNotCalculateResultBecauseUnkownLeftColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?Q"), new RDFConstantExpression(new RDFPlainLiteral("en-us")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndNotCalculateResultBecauseUnkownRightColumn()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("en-US").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariable("?Q"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldCreateStarLangMatchesExpressionAndCalculateResultTrue()
    {
       DataTable table = new DataTable();
       table.Columns.Add("?A", typeof(string));
       table.Columns.Add("?B", typeof(string));
       DataRow row = table.NewRow();
       row["?A"] = new RDFResource("http://example.org/").ToString();
       row["?B"] = new RDFPlainLiteral("hello", "en-US--ltr").ToString();
       table.Rows.Add(row);
       table.AcceptChanges();

       RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Star));
       RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

       Assert.IsNotNull(result);
       Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateStarLangMatchesExpressionAndCalculateResultTrueOnRightVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = RDFPlainLiteral.Star.ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US--ltr").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariable("?A"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateStarLangMatchesExpressionAndCalculateResultFalse()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Star));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateNoLangMatchesExpressionAndCalculateResultTrue()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Empty));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateNoLangMatchesExpressionAndCalculateResultTrueOnRightVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = new RDFPlainLiteral("hello").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateNoLangMatchesExpressionAndCalculateResultFalse()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Empty));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionAndNotCalculateResultBecauseLeftVariableResolvingToResource()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = RDFPlainLiteral.Empty.ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionAndNotCalculateResultBecauseRightVariableResolvingToResource()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariable("?A"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }
    #endregion
}