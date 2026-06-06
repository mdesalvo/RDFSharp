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

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFConstantExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateConstantExpressionWithResource()
    {
        RDFConstantExpression expression = new RDFConstantExpression(RDFVocabulary.FOAF.AGE);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"<{RDFVocabulary.FOAF.AGE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals($"<{RDFVocabulary.FOAF.AGE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("foaf")]).Equals("foaf:age", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateConstantExpressionWithPlainLiteral()
    {
        RDFConstantExpression expression = new RDFConstantExpression(new RDFPlainLiteral("lit","en-US"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("\"lit\"@EN-US", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("\"lit\"@EN-US", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateConstantExpressionWithNumericTypedLiteral()
    {
        RDFConstantExpression expression = new RDFConstantExpression(new RDFTypedLiteral("25.04", RDFModelEnums.RDFDatatypes.XSD_FLOAT));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("25.04", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("25.04", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("25.04", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateConstantExpressionWithNotNumericTypedLiteral()
    {
        RDFConstantExpression expression = new RDFConstantExpression(new RDFTypedLiteral("---25", RDFModelEnums.RDFDatatypes.XSD_GDAY));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"\"---25Z\"^^<{RDFVocabulary.XSD.G_DAY}>", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals($"\"---25Z\"^^<{RDFVocabulary.XSD.G_DAY}>", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("\"---25Z\"^^xsd:gDay", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingConstantExpressionWithResourceBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFConstantExpression(null as RDFResource));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingConstantExpressionWithLiteralBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFConstantExpression(null as RDFLiteral));

    [TestMethod]
    public void ShouldApplyExpressionWithResourceAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFConstantExpression expression = new RDFConstantExpression(
            RDFVocabulary.FOAF.AGE);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFVocabulary.FOAF.AGE));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithPlainLiteralAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFConstantExpression expression = new RDFConstantExpression(
            new RDFPlainLiteral("hello","en-US"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithTypedLiteralAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFConstantExpression expression = new RDFConstantExpression(
            new RDFTypedLiteral("{\"key\":\"val\"}", RDFModelEnums.RDFDatatypes.RDF_JSON));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("{\"key\":\"val\"}", RDFModelEnums.RDFDatatypes.RDF_JSON)));
    }
    #endregion
}