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
public class RDFGeoGetSRIDExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGEOGetSRIDExpressionWithExpression()
    {
        RDFGeoGetSRIDExpression expression = new RDFGeoGetSRIDExpression(
            new RDFVariableExpression(new RDFVariable("?V")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.GET_SRID}>(?V))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof")]).Equals("(geof:getSRID(?V))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateGEOGetSRIDExpressionWithVariable()
    {
        RDFGeoGetSRIDExpression expression = new RDFGeoGetSRIDExpression(
            new RDFVariable("?V"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.GET_SRID}>(?V))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof")]).Equals("(geof:getSRID(?V))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOGetSRIDExpressionWithEXPBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoGetSRIDExpression(null as RDFVariableExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOGetSRIDExpressionWithVARBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoGetSRIDExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionWithEXPAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddRow(new Dictionary<string, string>
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() }
        });

        RDFGeoGetSRIDExpression expression = new RDFGeoGetSRIDExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("http://www.opengis.net/def/crs/EPSG/0/4326", RDFModelEnums.RDFDatatypes.XSD_ANYURI)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVARAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?LS");
        table.AddRow(new Dictionary<string, string>
        {
            { "?LS", new RDFTypedLiteral("LINESTRING (9.18854 45, 10.399658169597386 45.22267719962008, 10.478759866207838 45.29537026093567)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() }
        });

        RDFGeoGetSRIDExpression expression = new RDFGeoGetSRIDExpression(
            new RDFVariable("?LS"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("http://www.opengis.net/def/crs/EPSG/0/4326", RDFModelEnums.RDFDatatypes.XSD_ANYURI)));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownLeftExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddRow(new Dictionary<string, string>
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45.464664)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() }
        });

        RDFGeoGetSRIDExpression expression = new RDFGeoGetSRIDExpression(
            new RDFVariable("?NAPLES"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseNotGeographicLeftExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddRow(new Dictionary<string, string>
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45.464664)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString() }
        });

        RDFGeoGetSRIDExpression expression = new RDFGeoGetSRIDExpression(
            new RDFVariableExpression(new RDFVariable("?ROME")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundLeftExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?MILAN");
        table.AddColumn("?ROME");
        table.AddColumn("?NAPLES");
        table.AddRow(new Dictionary<string, string>
        {
            { "?MILAN", new RDFTypedLiteral("POINT (9.18854 45.464664)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString() },
            { "?ROME", new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString() },
            { "?NAPLES", null }
        });

        RDFGeoGetSRIDExpression expression = new RDFGeoGetSRIDExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}