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
using NetTopologySuite.Geometries;
using RDFSharp.Model;
using RDFSharp.Query;
using System.Data;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFGeoWithinExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGEOWithinExpressionWithExpressions()
    {
        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariableExpression(new RDFVariable("?V")),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_WITHIN}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:sfWithin(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldCreateGEOWithinExpressionWithExpressionAndVariable()
    {
        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFVariable("?V"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_WITHIN}>(\"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, ?V))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:sfWithin(\"POINT (1 1)\"^^geosparql:wktLiteral, ?V))"));
    }

    [TestMethod]
    public void ShouldCreateGEOWithinExpressionWithExpressionAndTypedLiteral()
    {
        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (2 2)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_WITHIN}>(\"POINT (2 2)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:sfWithin(\"POINT (2 2)\"^^geosparql:wktLiteral, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldCreateGEOWithinExpressionWithVariableAndExpression()
    {
        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariable("?V"),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_WITHIN}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:sfWithin(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldCreateGEOWithinExpressionWithVariables()
    {
        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_WITHIN}>(?V1, ?V2))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:sfWithin(?V1, ?V2))"));
    }

    [TestMethod]
    public void ShouldCreateGEOWithinExpressionWithVariableAndTypedLiteral()
    {
        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariable("?V"),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_WITHIN}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:sfWithin(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithEEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(null as RDFVariableExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithEEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariableExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithEVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(null as RDFVariableExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithEVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithETBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(null as RDFVariableExpression, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithETBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithETBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithVEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithVEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(new RDFVariable("?V"), null as RDFVariableExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithVVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithVVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(new RDFVariable("?V"), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithVTBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(null as RDFVariable, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithVTBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(new RDFVariable("?V"), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOWithinExpressionWithVTBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoWithinExpression(new RDFVariable("?V"), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?NAPLES", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?NAPLES"] = new RDFTypedLiteral("POINT (14.2681244 40.8517746)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariableExpression(new RDFVariable("?NAPLES")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?MILANNAPLES", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?MILANNAPLES"] = new RDFTypedLiteral("<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:posList>9.18854 45 14.2681244 40.8517746</gml:posList></gml:LineString>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariable("?MILANNAPLES"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithETAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?LS", typeof(string));
        DataRow row = table.NewRow();
        row["?LS"] = new RDFTypedLiteral("LINESTRING(10.705256576624173 43.75731680339918,10.62217247017886 43.72631206695182)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariableExpression(new RDFVariable("?LS")),
            new RDFTypedLiteral("POLYGON ((9.18854 45, 12.496365 41.902782, 14.268124 40.851774, 9.18854 45))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?PL", typeof(string));
        table.Columns.Add("?MPT", typeof(string));
        DataRow row = table.NewRow();
        row["?PL"] = new RDFTypedLiteral("POLYGON ((9.55854 46, 12.496365 41.902782, 14.268124 40.851774, 9.55854 46))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
        row["?MPT"] = new RDFTypedLiteral("<gml:MultiPoint xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pointMember><gml:Point><gml:pos>9.18854 45</gml:pos></gml:Point></gml:pointMember><gml:pointMember><gml:Point><gml:pos>9.18854 39</gml:pos></gml:Point></gml:pointMember></gml:MultiPoint>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariable("?MPT"),
            new RDFVariableExpression(new RDFVariable("?PL")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariable("?MILAN"),
            new RDFVariable("?MILAN"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVTAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariable("?MILAN"),
            new RDFTypedLiteral("MULTILINESTRING ((9.18854 45, 12.496365 41.902782), (12.496365 41.902782, 14.2681244 40.8517746))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnknownLeftExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnknownRightExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotGeographicLeftExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariableExpression(new RDFVariable("?ROME")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotGeographicRightExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?ROME")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnboundLeftExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        table.Columns.Add("?NAPLES", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        row["?NAPLES"] = null;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnboundRightExpression()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        table.Columns.Add("?NAPLES", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        row["?NAPLES"] = null;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoWithinExpression expression = new RDFGeoWithinExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}

[TestClass]
public class RDFGeoConverterTest
{
    #region Tests
    [TestMethod]
    public void ShouldGetLAZGeometryFromWGS84()
    {
        Geometry milanWGS84 = new Point(9.188540, 45.464664);
        Geometry milanLAZ = RDFGeoConverter.GetLambertAzimuthalGeometryFromWGS84(milanWGS84);

        Assert.IsNotNull(milanLAZ);
        Assert.IsTrue(string.Equals(milanLAZ.ToText(), "POINT (-899167.1609069 4413535.43188373)"));
        Assert.AreEqual(42106, milanLAZ.SRID);
    }

    [TestMethod]
    public void ShouldGetWGS84GeometryFromLAZ()
    {
        Geometry milanLAZ = RDFGeoConverter.GetLambertAzimuthalGeometryFromWGS84(new Point(9.188540, 45.464664));
        Geometry milanWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(milanLAZ);

        Assert.IsNotNull(milanWGS84);
        Assert.IsTrue(string.Equals(milanWGS84.ToText(), "POINT (9.18854 45.464664)"));
        Assert.AreEqual(4326, milanWGS84.SRID);
    }
    #endregion
}