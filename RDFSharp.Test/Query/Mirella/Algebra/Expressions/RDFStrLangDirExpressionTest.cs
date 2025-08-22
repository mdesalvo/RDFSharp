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
public class RDFStrLangDirExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateEEStrLangDirExpression1()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")),
            RDFQueryEnums.RDFLanguageDirections.LTR);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR((?V1 + ?V2), ?V3, ltr))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR((?V1 + ?V2), ?V3, ltr))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEStrLangDirExpression2()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            RDFQueryEnums.RDFLanguageDirections.RTL);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR((?V1 + ?V2), 25, rtl))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR((?V1 + ?V2), 25, rtl))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(STRLANGDIR((?V1 + ?V2), 25, rtl))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEEStrLangDirExpressionNested()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFStrLangDirExpression(
                new RDFVariableExpression(new RDFVariable("?V3")),
                new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US")),
                RDFQueryEnums.RDFLanguageDirections.LTR),
            RDFQueryEnums.RDFLanguageDirections.RTL);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR((?V1 + ?V2), (STRLANGDIR(?V3, \"hello\"@EN-US, ltr)), rtl))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR((?V1 + ?V2), (STRLANGDIR(?V3, \"hello\"@EN-US, ltr)), rtl))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateEVStrLangDirExpression()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"),
            RDFQueryEnums.RDFLanguageDirections.LTR);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR((?V1 + ?V2), ?V3, ltr))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR((?V1 + ?V2), ?V3, ltr))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVEStrLangDirExpression()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            RDFQueryEnums.RDFLanguageDirections.RTL);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR(?V3, (?V1 + ?V2), rtl))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR(?V3, (?V1 + ?V2), rtl))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateVVStrLangDirExpression()
    {
        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?V3"),
            new RDFVariable("?V1"),
            RDFQueryEnums.RDFLanguageDirections.LTR);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANGDIR(?V3, ?V1, ltr))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANGDIR(?V3, ?V1, ltr))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEEStrLangDirExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangDirExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V")), RDFQueryEnums.RDFLanguageDirections.LTR));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVStrLangDirExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangDirExpression(null as RDFExpression, new RDFVariable("?V"), RDFQueryEnums.RDFLanguageDirections.LTR));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVEStrLangDirExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangDirExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V")), RDFQueryEnums.RDFLanguageDirections.LTR));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVStrLangDirExpressionNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangDirExpression(null as RDFVariable, new RDFVariable("?V"), RDFQueryEnums.RDFLanguageDirections.LTR));

    //EE

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultOnSimplePlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultOnStringBasedTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"hello^^{RDFVocabulary.XSD.STRING}";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.RTL);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--rtl")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotWellFormedLanguageTag()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = "en-";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotSimplePlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello@EN-US";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotStringBasedTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"34^^{RDFVocabulary.XSD.BYTE}";
        row["?B"] = "en-";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //EV

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultOnSimplePlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultOnStringBasedTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"hello^^{RDFVocabulary.XSD.STRING}";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.RTL);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--rtl")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseNotWellFormedLanguageTag()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = "en-";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseNotSimplePlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello@EN-US";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseNotStringBasedTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"34^^{RDFVocabulary.XSD.BYTE}";
        row["?B"] = "en-";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //VE

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultOnSimplePlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultOnStringBasedTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"hello^^{RDFVocabulary.XSD.STRING}";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndNotCalculateResultBecauseNotWellFormedLanguageTag()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = "en-";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndNotCalculateResultBecauseNotSimplePlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello@EN-US";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndNotCalculateResultBecauseNotStringBasedTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"34^^{RDFVocabulary.XSD.BYTE}";
        row["?B"] = "en-";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //VV

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultOnSimplePlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultOnStringBasedTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"hello^^{RDFVocabulary.XSD.STRING}";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US--ltr")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNotWellFormedLanguageTag()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = "en-";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNotSimplePlainLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello@EN-US";
        row["?B"] = "en-US";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNotStringBasedTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"34^^{RDFVocabulary.XSD.BYTE}";
        row["?B"] = "en-";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = RDFVocabulary.XSD.FLOAT;
        row["?B"] = "en-US";
        row["?C"] = new RDFPlainLiteral("C");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?B"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = new RDFPlainLiteral("B");
        row["?C"] = new RDFPlainLiteral("C");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrLangDirExpression expression = new RDFStrLangDirExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"),
            RDFQueryEnums.RDFLanguageDirections.LTR);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}