/*
   Copyright 2012-2026 Marco De Salvo

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
using System.Collections.Generic;
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
        Assert.IsTrue(expression.ToString().Equals("(STRDT((?V1 + ?V2), ?V3))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT((?V1 + ?V2), ?V3))", StringComparison.Ordinal));
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
        Assert.IsTrue(expression.ToString().Equals("(STRDT((?V1 + ?V2), 25))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT((?V1 + ?V2), 25))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(STRDT((?V1 + ?V2), 25))", StringComparison.Ordinal));
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
        Assert.IsTrue(expression.ToString().Equals("(STRDT((?V1 + ?V2), (STRDT(?V3, \"hello\"@EN-US))))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT((?V1 + ?V2), (STRDT(?V3, \"hello\"@EN-US))))", StringComparison.Ordinal));
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
        Assert.IsTrue(expression.ToString().Equals("(STRDT((?V1 + ?V2), ?V3))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT((?V1 + ?V2), ?V3))", StringComparison.Ordinal));
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
        Assert.IsTrue(expression.ToString().Equals("(STRDT(?V3, (?V1 + ?V2)))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT(?V3, (?V1 + ?V2)))", StringComparison.Ordinal));
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
        Assert.IsTrue(expression.ToString().Equals("(STRDT(?V3, ?V1))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(STRDT(?V3, ?V1))", StringComparison.Ordinal));
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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "44" },
            { "?B", RDFVocabulary.XSD.INTEGER.ToString() },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "44" },
            { "?B", "http://example.org/testdt" },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "hello" },
            { "?B", RDFVocabulary.XSD.INTEGER.ToString() },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "44" },
            { "?B", RDFVocabulary.XSD.INTEGER.ToString() },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "44" },
            { "?B", "http://example.org/testdt" },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "hello" },
            { "?B", RDFVocabulary.XSD.INTEGER.ToString() },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "44" },
            { "?B", RDFVocabulary.XSD.INTEGER.ToString() },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "44" },
            { "?B", "http://example.org/testdt" },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "hello" },
            { "?B", RDFVocabulary.XSD.INTEGER.ToString() },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "44" },
            { "?B", RDFVocabulary.XSD.INTEGER.ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("44", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultOnLeftResourceAndKnownDatatype()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "http://example.org/" },
            { "?B", RDFVocabulary.XSD.ANY_URI.ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("http://example.org/", RDFModelEnums.RDFDatatypes.XSD_ANYURI)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResultOnUnknownDatatype()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "44" },
            { "?B", "http://example.org/testdt" },
        });

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
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", "hello" },
            { "?B", RDFVocabulary.XSD.INTEGER.ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableLeft()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", RDFVocabulary.XSD.FLOAT.ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableRight()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("B").ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseNotDatatypeGiven()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", "hello" },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseForbiddenDatatype1Given()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello@EN").ToString() },
            { "?B", RDFVocabulary.RDF.PLAIN_LITERAL.ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseForbiddenDatatype2Given()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello@EN").ToString() },
            { "?B", RDFVocabulary.RDF.LANG_STRING.ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseForbiddenDatatype3Given()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello@EN--ltr").ToString() },
            { "?B", RDFVocabulary.RDF.DIR_LANG_STRING.ToString() },
            { "?C", new RDFPlainLiteral("C").ToString() },
        });

        RDFStrDtExpression expression = new RDFStrDtExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}