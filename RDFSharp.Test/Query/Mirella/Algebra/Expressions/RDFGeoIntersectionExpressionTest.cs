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
using RDFSharp.Model;
using RDFSharp.Query;
using System.Collections.Generic;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFGeoIntersectionExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGEOIntersectionExpressionWithExpressions()
    {
        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?V")),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.INTERSECTION}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:intersection(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEOIntersectionExpressionWithExpressionAndVariable()
    {
        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFVariable("?V"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.INTERSECTION}>(\"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, ?V))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:intersection(\"POINT (1 1)\"^^geosparql:wktLiteral, ?V))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEOIntersectionExpressionWithExpressionAndTypedLiteral()
    {
        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (2 2)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.INTERSECTION}>(\"POINT (2 2)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:intersection(\"POINT (2 2)\"^^geosparql:wktLiteral, \"POINT (1 1)\"^^geosparql:wktLiteral))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEOIntersectionExpressionWithVariableAndExpression()
    {
        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariable("?V"),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.INTERSECTION}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:intersection(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEOIntersectionExpressionWithVariables()
    {
        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.INTERSECTION}>(?V1, ?V2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:intersection(?V1, ?V2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEOIntersectionExpressionWithVariableAndTypedLiteral()
    {
        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariable("?V"),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.INTERSECTION}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:intersection(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithEEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(null as RDFVariableExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithEEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariableExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithEVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(null as RDFVariableExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithEVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithETBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(null as RDFVariableExpression, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithETBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithETBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithVEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithVEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(new RDFVariable("?V"), null as RDFVariableExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithVVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithVVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(new RDFVariable("?V"), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithVTBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(null as RDFVariable, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithVTBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(new RDFVariable("?V"), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOIntersectionExpressionWithVTBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoIntersectionExpression(new RDFVariable("?V"), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariableExpression(new RDFVariable("?MILAN")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResultEmpty()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariableExpression(new RDFVariable("?ROME")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT EMPTY", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?MILANNAPLES");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?MILANNAPLES", new RDFTypedLiteral("<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:posList>9.18854 45 14.2681244 40.8517746</gml:posList></gml:LineString>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariable("?MILANNAPLES"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResultEmpty()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROMENAPLES");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROMENAPLES", new RDFTypedLiteral("<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:posList>12.496365 41.902782 14.2681244 40.8517746</gml:posList></gml:LineString>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariable("?ROMENAPLES"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT EMPTY", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithETAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFTypedLiteral("POLYGON ((9.18854 45, 12.496365 41.902782, 14.268124 40.851774, 9.18854 45))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithETAndCalculateResultEmpty()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFTypedLiteral("POLYGON ((12.496365 41.902782, 14.2681244 40.8517746, 15.2681244 39.8517746, 12.496365 41.902782))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT EMPTY", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?MILANANDROME");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?MILANANDROME", new RDFTypedLiteral("<gml:MultiPoint xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pointMember><gml:Point><gml:pos>9.18854 45</gml:pos></gml:Point></gml:pointMember><gml:pointMember><gml:Point><gml:pos>12.496365 41.902782</gml:pos></gml:Point></gml:pointMember></gml:MultiPoint>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariable("?MILAN"),
            new RDFVariableExpression(new RDFVariable("?MILANANDROME")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariable("?MILAN"),
            new RDFVariable("?MILAN"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVTAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariable("?MILAN"),
            new RDFTypedLiteral("MULTILINESTRING ((9.18854 45, 12.496365 41.902782), (12.496365 41.902782, 14.2681244 40.8517746))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnknownLeftExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnknownRightExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotGeographicLeftExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?ROME")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseNotGeographicRightExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString() },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?ROME")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnboundLeftExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddColumn("?NAPLES");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
            { "?NAPLES", null },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndNotCalculateResultBecauseUnboundRightExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddColumn("?NAPLES");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
            { "?NAPLES", null },
        });

        RDFGeoIntersectionExpression expression = new RDFGeoIntersectionExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}