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
public class RDFGeoRelateExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGEORelateExpressionWithExpressions()
    {
        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariableExpression(new RDFVariable("?V")),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            "012TF**FT");

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RELATE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"012TF**FT\"))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:relate(?V, \"POINT (1 1)\"^^geosparql:wktLiteral, \"012TF**FT\"))"));
    }

    [TestMethod]
    public void ShouldCreateGEORelateExpressionWithExpressionAndVariable()
    {
        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFVariable("?V"),
            "012TF**FT");

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RELATE}>(\"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, ?V, \"012TF**FT\"))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:relate(\"POINT (1 1)\"^^geosparql:wktLiteral, ?V, \"012TF**FT\"))"));
    }

    [TestMethod]
    public void ShouldCreateGEORelateExpressionWithExpressionAndTypedLiteral()
    {
        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (2 2)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            "012TF**FT");

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RELATE}>(\"POINT (2 2)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"012TF**FT\"))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:relate(\"POINT (2 2)\"^^geosparql:wktLiteral, \"POINT (1 1)\"^^geosparql:wktLiteral, \"012TF**FT\"))"));
    }

    [TestMethod]
    public void ShouldCreateGEORelateExpressionWithVariableAndExpression()
    {
        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariable("?V"),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            "012TF**FT");

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RELATE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"012TF**FT\"))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:relate(?V, \"POINT (1 1)\"^^geosparql:wktLiteral, \"012TF**FT\"))"));
    }

    [TestMethod]
    public void ShouldCreateGEORelateExpressionWithVariables()
    {
        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"),
            "012TF**FT");

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RELATE}>(?V1, ?V2, \"012TF**FT\"))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:relate(?V1, ?V2, \"012TF**FT\"))"));
    }

    [TestMethod]
    public void ShouldCreateGEORelateExpressionWithVariableAndTypedLiteral()
    {
        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariable("?V"),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            "012TF**FT");

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RELATE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"012TF**FT\"))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:relate(?V, \"POINT (1 1)\"^^geosparql:wktLiteral, \"012TF**FT\"))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(null as RDFVariableExpression, new RDFVariableExpression(new RDFVariable("?V")), "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariableExpression, "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEEBecauseNullDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFVariableExpression(new RDFVariable("?V")), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEEBecauseTooLongDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFVariableExpression(new RDFVariable("?V")), "012TF**FT2")); //10 chars

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEEBecauseInvalidDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFVariableExpression(new RDFVariable("?V")), "019TF**FT")); //9 is not allowed char

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(null as RDFVariableExpression, new RDFVariable("?V"), "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable, "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEVBecauseNullDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFVariable("?V"), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEVBecauseTooLongDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFVariable("?V"), "012TF**FT2")); //10 chars

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithEVBecauseInvalidDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFVariable("?V"), "019TF**FT")); //9 is not allowed char

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithETBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(null as RDFVariableExpression, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithETBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFTypedLiteral, "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithETBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING), "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithETBecauseNullDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithETBecauseTooLongDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), "012TF**FT2")); //10 chars

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithETBecauseInvalidDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), "019TF**FT")); //9 is not allowed char

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V")), "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), null as RDFVariableExpression, "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVEBecauseNullDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFVariableExpression(new RDFVariable("?V")), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVEBecauseTooLongDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFVariableExpression(new RDFVariable("?V")), "012TF**FT2")); //10 chars

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVEBecauseInvalidDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFVariableExpression(new RDFVariable("?V")), "019TF**FT")); //9 is not allowed char

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(null as RDFVariable, new RDFVariable("?V"), "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), null as RDFVariable, "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVVBecauseNullDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFVariable("?V"), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVVBecauseTooLongDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFVariable("?V"), "012TF**FT2")); //10 chars

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVVBecauseInvalidDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFVariable("?V"), "019TF**FT")); //9 is not allowed char

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVTBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(null as RDFVariable, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVTBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), null as RDFTypedLiteral, "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVTBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING), "012TF**FT"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVTBecauseNullDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVTBecauseTooLongDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), "012TF**FT2")); //10 chars

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORelateExpressionWithVTBecauseInvalidDE9IMRelation()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRelateExpression(new RDFVariable("?V"), new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), "019TF**FT")); //9 is not allowed char

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            "T*F**FFF*"); //equals
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")),
            new RDFVariable("?MILANNAPLES"),
            "*T*******"); //one of intersects patterns
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariableExpression(new RDFVariable("?LS")),
            new RDFTypedLiteral("POLYGON ((9.18854 45, 12.496365 41.902782, 14.268124 40.851774, 9.18854 45))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            "T*F**F***"); //within
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariable("?MPT"),
            new RDFVariableExpression(new RDFVariable("?PL")),
            "T*F**FFF*"); //equals
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariable("?MILAN"),
            new RDFVariable("?MILAN"),
            "T*F**FFF*"); //equals
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariable("?MILAN"),
            new RDFTypedLiteral("MULTILINESTRING ((9.18854 45, 12.496365 41.902782), (12.496365 41.902782, 14.2681244 40.8517746))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            "T*F**FFF*"); //equals
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            "T*F**FFF*"); //equals
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            "T*F**FFF*"); //equals
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariableExpression(new RDFVariable("?ROME")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            "T*F**FFF*"); //equals
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?ROME")),
            "T*F**FFF*"); //equals
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            "T*F**FFF*"); //equals
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

        RDFGeoRelateExpression expression = new RDFGeoRelateExpression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            "T*F**FFF*"); //equals
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}