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
using System;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFStrDtExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateEEStrDtExpression1()
    {
        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariableExpression(new RDFVariable("?V3")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRDT((?V1 + ?V2), ?V3))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT((?V1 + ?V2), ?V3))"));
    }

    [TestMethod]
    public void ShouldCreateEEStrDtExpression2()
    {
        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRDT((?V1 + ?V2), 25))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT((?V1 + ?V2), 25))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(STRDT((?V1 + ?V2), 25))"));
    }

    [TestMethod]
    public void ShouldCreateEEStrDtExpressionNested()
    {
        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFStrDtExpression(new RDFVariableExpression(new RDFVariable("?V3")), new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US"))));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRDT((?V1 + ?V2), (STRDT(?V3, \"hello\"@EN-US))))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT((?V1 + ?V2), (STRDT(?V3, \"hello\"@EN-US))))"));
    }

    [TestMethod]
    public void ShouldCreateEVStrDtExpression()
    {
        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V3"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRDT((?V1 + ?V2), ?V3))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT((?V1 + ?V2), ?V3))"));
    }

    [TestMethod]
    public void ShouldCreateVEStrDtExpression()
    {
        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?V3"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRDT(?V3, (?V1 + ?V2)))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT(?V3, (?V1 + ?V2)))"));
    }

    [TestMethod]
    public void ShouldCreateVVStrDtExpression()
    {
        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?V3"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(STRDT(?V3, ?V1))"));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT(?V3, ?V1))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEEStrDtExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrDtExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingEVStrDtExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrDtExpression(null as RDFExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVEStrDtExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrDtExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingVVStrDtExpressionNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFStrDtExpression(null as RDFVariable, new RDFVariable("?V")));

    //EE

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultOnKnownDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "44";
        row["?B"] = RDFVocabulary.XSD.INTEGER;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultOnUnknownDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "44";
        row["?B"] = "http://example.org/testdt";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", new RDFDatatype(new Uri("http://example.org/testdt"), RDFModelEnums.RDFDatatypes.RDFS_LITERAL, null))));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotWellFormedDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = RDFVocabulary.XSD.INTEGER;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //EV

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultOnKnownDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "44";
        row["?B"] = RDFVocabulary.XSD.INTEGER;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultOnUnknownDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "44";
        row["?B"] = "http://example.org/testdt";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", new RDFDatatype(new Uri("http://example.org/testdt"), RDFModelEnums.RDFDatatypes.RDFS_LITERAL, null))));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndNotCalculateResultBecauseNotWellFormedDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = RDFVocabulary.XSD.INTEGER;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //VE

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultOnKnownDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "44";
        row["?B"] = RDFVocabulary.XSD.INTEGER;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResultOnUnknownDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "44";
        row["?B"] = "http://example.org/testdt";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", new RDFDatatype(new Uri("http://example.org/testdt"), RDFModelEnums.RDFDatatypes.RDFS_LITERAL, null))));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndNotCalculateResultBecauseNotWellFormedDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = RDFVocabulary.XSD.INTEGER;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    //VV

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultOnKnownDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "44";
        row["?B"] = RDFVocabulary.XSD.INTEGER;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultOnUnknownDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "44";
        row["?B"] = "http://example.org/testdt";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", new RDFDatatype(new Uri("http://example.org/testdt"), RDFModelEnums.RDFDatatypes.RDFS_LITERAL, null))));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseNotWellFormedDatatype()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "hello";
        row["?B"] = RDFVocabulary.XSD.INTEGER;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
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
        row["?B"] = new RDFPlainLiteral("B");
        row["?C"] = new RDFPlainLiteral("C");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?A"));
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
        row["?A"] = new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT);
        row["?B"] = new RDFPlainLiteral("B");
        row["?C"] = new RDFPlainLiteral("C");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseNotDatatypeGiven()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT);
        row["?B"] = "hello";
        row["?C"] = new RDFPlainLiteral("C");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseForbiddenDatatype1Given()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello@EN");
        row["?B"] = RDFVocabulary.RDF.PLAIN_LITERAL;
        row["?C"] = new RDFPlainLiteral("C");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseForbiddenDatatype2Given()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello@EN");
        row["?B"] = RDFVocabulary.RDF.LANG_STRING;
        row["?C"] = new RDFPlainLiteral("C");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseForbiddenDatatype3Given()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        table.Columns.Add("?C", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFPlainLiteral("hello@EN--ltr");
        row["?B"] = RDFVocabulary.RDF.DIR_LANG_STRING;
        row["?C"] = new RDFPlainLiteral("C");
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}