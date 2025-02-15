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
        Assert.IsTrue(expression.ToString().Equals($"<{RDFVocabulary.FOAF.AGE}>"));
        Assert.IsTrue(expression.ToString([]).Equals($"<{RDFVocabulary.FOAF.AGE}>"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("foaf")]).Equals("foaf:age"));
    }

    [TestMethod]
    public void ShouldCreateConstantExpressionWithPlainLiteral()
    {
        RDFConstantExpression expression = new RDFConstantExpression(new RDFPlainLiteral("lit","en-US"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("\"lit\"@EN-US"));
        Assert.IsTrue(expression.ToString([]).Equals("\"lit\"@EN-US"));
    }

    [TestMethod]
    public void ShouldCreateConstantExpressionWithNumericTypedLiteral()
    {
        RDFConstantExpression expression = new RDFConstantExpression(new RDFTypedLiteral("25.04", RDFModelEnums.RDFDatatypes.XSD_FLOAT));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("25.04"));
        Assert.IsTrue(expression.ToString([]).Equals("25.04"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("25.04"));
    }

    [TestMethod]
    public void ShouldCreateConstantExpressionWithNotNumericTypedLiteral()
    {
        RDFConstantExpression expression = new RDFConstantExpression(new RDFTypedLiteral("---25", RDFModelEnums.RDFDatatypes.XSD_GDAY));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"\"---25Z\"^^<{RDFVocabulary.XSD.G_DAY}>"));
        Assert.IsTrue(expression.ToString([]).Equals($"\"---25Z\"^^<{RDFVocabulary.XSD.G_DAY}>"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("\"---25Z\"^^xsd:gDay"));
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
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFConstantExpression expression = new RDFConstantExpression(
            RDFVocabulary.FOAF.AGE);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFVocabulary.FOAF.AGE));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithPlainLiteralAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFConstantExpression expression = new RDFConstantExpression(
            new RDFPlainLiteral("hello","en-US"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithTypedLiteralAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFConstantExpression expression = new RDFConstantExpression(
            new RDFTypedLiteral("{\"key\":\"val\"}", RDFModelEnums.RDFDatatypes.RDF_JSON));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("{\"key\":\"val\"}", RDFModelEnums.RDFDatatypes.RDF_JSON)));
    }
    #endregion
}