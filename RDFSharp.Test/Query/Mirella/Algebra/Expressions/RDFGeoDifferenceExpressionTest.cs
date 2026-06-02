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
public class RDFGeoDifferenceExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGEODifferenceExpressionWithExpressions()
    {
        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariableExpression(new RDFVariable("?V")),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.DIFFERENCE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:difference(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEODifferenceExpressionWithExpressionAndVariable()
    {
        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFVariable("?V"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.DIFFERENCE}>(\"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, ?V))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:difference(\"POINT (1 1)\"^^geosparql:wktLiteral, ?V))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEODifferenceExpressionWithExpressionAndTypedLiteral()
    {
        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (2 2)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.DIFFERENCE}>(\"POINT (2 2)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:difference(\"POINT (2 2)\"^^geosparql:wktLiteral, \"POINT (1 1)\"^^geosparql:wktLiteral))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEODifferenceExpressionWithVariableAndExpression()
    {
        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariable("?V"),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.DIFFERENCE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:difference(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEODifferenceExpressionWithVariables()
    {
        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.DIFFERENCE}>(?V1, ?V2))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:difference(?V1, ?V2))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEODifferenceExpressionWithVariableAndTypedLiteral()
    {
        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariable("?V"),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.DIFFERENCE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:difference(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithEEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(null as RDFVariableExpression, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithEEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariableExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithEVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(null as RDFVariableExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithEVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithETBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(null as RDFVariableExpression, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithETBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithETBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithVEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithVEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(new RDFVariable("?V"), null as RDFVariableExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithVVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithVVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(new RDFVariable("?V"), null as RDFVariable));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithVTBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(null as RDFVariable, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithVTBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(new RDFVariable("?V"), null as RDFTypedLiteral));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEODifferenceExpressionWithVTBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoDifferenceExpression(new RDFVariable("?V"), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MPT");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MPT", new RDFTypedLiteral("<gml:MultiPoint xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pointMember><gml:Point><gml:pos>9.18854 45</gml:pos></gml:Point></gml:pointMember><gml:pointMember><gml:Point><gml:pos>9.18854 48.89</gml:pos></gml:Point></gml:pointMember></gml:MultiPoint>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
        });

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariableExpression(new RDFVariable("?MPT")),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 48.89)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult2()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
        });

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariableExpression(new RDFVariable("?ROME")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEVAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?LS");
        table.AddColumn("?PT");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?LS", new RDFTypedLiteral("<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:posList>9.18854 45 9.18854 49</gml:posList></gml:LineString>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
            { "?PT", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariableExpression(new RDFVariable("?LS")),
            new RDFVariable("?PT"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("LINESTRING (9.18854 45, 9.18854 49)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithETAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?PL");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?PL", new RDFTypedLiteral("POLYGON((9.188690185546873 45.46590974421454,7.667083740234373 45.056061242744164,8.916778564453123 44.406316252661384,9.188690185546873 45.46590974421454))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariableExpression(new RDFVariable("?PL")),
            new RDFTypedLiteral("POLYGON((8.488311767578123 45.13361760070825,8.205413818359371 44.8947957646979,8.592681884765623 44.90841397875738,8.488311767578123 45.13361760070825))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);//This is a polygon with hole
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POLYGON ((9.18869019 45.46590974, 8.91677856 44.40631625, 7.66708374 45.05606124, 9.18869019 45.46590974), (8.48831177 45.1336176, 8.20541382 44.89479576, 8.59268188 44.90841397, 8.48831177 45.1336176))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVEAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILANANDROME");
        table.AddColumn("?MILAN");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?MILANANDROME", new RDFTypedLiteral("<gml:MultiPoint xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pointMember><gml:Point><gml:pos>9.18854 45</gml:pos></gml:Point></gml:pointMember><gml:pointMember><gml:Point><gml:pos>9.18854 54</gml:pos></gml:Point></gml:pointMember></gml:MultiPoint>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariable("?MILANANDROME"),
            new RDFVariableExpression(new RDFVariable("?MILAN")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 54)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
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

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariable("?MILAN"),
            new RDFVariable("?MILAN"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT EMPTY", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVTAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?LS");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?LS", new RDFTypedLiteral("LINESTRING (9.18854 45, 9.18854 54)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
        });

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFVariable("?LS"),
            new RDFTypedLiteral("LINESTRING (9.18854 45, 9.18854 49)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("LINESTRING (9.18854 45, 9.18854 54)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
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

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
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

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
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

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
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

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
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

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
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

        RDFGeoDifferenceExpression expression = new RDFGeoDifferenceExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}