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
public class RDFStrLangExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateEEStrLangExpression1()
    {
        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANG((?V1 + ?V2), ?V3))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANG((?V1 + ?V2), ?V3))"));
    }

    [TestMethod]
    public void ShouldCreateEEStrLangExpression2()
    {
        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANG((?V1 + ?V2), 25))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANG((?V1 + ?V2), 25))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(STRLANG((?V1 + ?V2), 25))"));
    }

    [TestMethod]
    public void ShouldCreateEEStrLangExpressionNested()
    {
        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFStrLangExpression(new RDFVariableExpression(new RDFVariable("?V3")), new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US"))));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANG((?V1 + ?V2), (STRLANG(?V3, \"hello\"@EN-US))))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANG((?V1 + ?V2), (STRLANG(?V3, \"hello\"@EN-US))))"));
    }

    [TestMethod]
    public void ShouldCreateEVStrLangExpression()
    {
        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANG((?V1 + ?V2), ?V3))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANG((?V1 + ?V2), ?V3))"));
    }

    [TestMethod]
    public void ShouldCreateVEStrLangExpression()
    {
        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANG(?V3, (?V1 + ?V2)))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANG(?V3, (?V1 + ?V2)))"));
    }

    [TestMethod]
    public void ShouldCreateVVStrLangExpression()
    {
        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRLANG(?V3, ?V1))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRLANG(?V3, ?V1))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEEStrLangExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVStrLangExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangExpression(null as RDFExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVEStrLangExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVStrLangExpressionNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrLangExpression(null as RDFVariable, new RDFVariable("?V")));

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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?B"));
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

        RDFStrLangExpression expression = new RDFStrLangExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}