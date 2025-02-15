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
using RDFSharp.Model;
using RDFSharp.Query;
using System.Data;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFGeoEnvelopeExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGEOEnvelopeExpressionWithExpression()
    {
        RDFGeoEnvelopeExpression expression = new RDFGeoEnvelopeExpression(
            new RDFVariableExpression(new RDFVariable("?V")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.ENVELOPE}>(?V))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof")]).Equals("(geof:envelope(?V))"));
    }

    [TestMethod]
    public void ShouldCreateGEOEnvelopeExpressionWithVariable()
    {
        RDFGeoEnvelopeExpression expression = new RDFGeoEnvelopeExpression(
            new RDFVariable("?V"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.ENVELOPE}>(?V))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof")]).Equals("(geof:envelope(?V))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOEnvelopeExpressionWithEXPBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoEnvelopeExpression(null as RDFVariableExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOEnvelopeExpressionWithVARBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoEnvelopeExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionWithEXPAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoEnvelopeExpression expression = new RDFGeoEnvelopeExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVARAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?GC", typeof(string));
        DataRow row = table.NewRow();
        row["?GC"] = new RDFTypedLiteral("GEOMETRYCOLLECTION(LINESTRING(10.399658169597386 45.22267719962008,10.478759866207838 45.29537026093567),POINT(10.661132812499998 45.25052808643704))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoEnvelopeExpression expression = new RDFGeoEnvelopeExpression(
            new RDFVariable("?GC"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POLYGON ((10.39965817 45.2226772, 10.39002572 45.29217802, 10.65424624 45.30159609, 10.66361721 45.23207186, 10.39965817 45.2226772))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownLeftExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45.464664)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoEnvelopeExpression expression = new RDFGeoEnvelopeExpression(
            new RDFVariable("?NAPLES"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseNotGeographicLeftExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45.464664)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoEnvelopeExpression expression = new RDFGeoEnvelopeExpression(
            new RDFVariableExpression(new RDFVariable("?ROME")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundLeftExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        table.Columns.Add("?NAPLES", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45.464664)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        row["?NAPLES"] = null;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoEnvelopeExpression expression = new RDFGeoEnvelopeExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}