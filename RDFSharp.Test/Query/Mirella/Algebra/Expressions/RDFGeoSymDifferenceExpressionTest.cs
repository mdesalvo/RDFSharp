/*
   Copyright 2012-2024 Marco De Salvo

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
using System.Data;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFGeoSymDifferenceExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateGEOSymDifferenceExpressionWithExpressions()
        {
            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariableExpression(new RDFVariable("?V")),
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SYM_DIFFERENCE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:symDifference(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOSymDifferenceExpressionWithExpressionAndVariable()
        {
            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
                new RDFVariable("?V"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SYM_DIFFERENCE}>(\"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, ?V))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:symDifference(\"POINT (1 1)\"^^geosparql:wktLiteral, ?V))"));
        }

        [TestMethod]
        public void ShouldCreateGEOSymDifferenceExpressionWithExpressionAndTypedLiteral()
        {
            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFConstantExpression(new RDFTypedLiteral("POINT (2 2)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
                new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SYM_DIFFERENCE}>(\"POINT (2 2)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:symDifference(\"POINT (2 2)\"^^geosparql:wktLiteral, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOSymDifferenceExpressionWithVariableAndExpression()
        {
            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariable("?V"),
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SYM_DIFFERENCE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:symDifference(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOSymDifferenceExpressionWithVariables()
        {
            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariable("?V1"),
                new RDFVariable("?V2"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SYM_DIFFERENCE}>(?V1, ?V2))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:symDifference(?V1, ?V2))"));
        }

        [TestMethod]
        public void ShouldCreateGEOSymDifferenceExpressionWithVariableAndTypedLiteral()
        {
            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariable("?V"),
                new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SYM_DIFFERENCE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:symDifference(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithEEBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(null as RDFVariableExpression, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithEEBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariableExpression));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithEVBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(null as RDFVariableExpression, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithEVBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithETBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(null as RDFVariableExpression, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithETBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFTypedLiteral));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithETBecauseNotGeographicRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithVEBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithVEBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(new RDFVariable("?V"), null as RDFVariableExpression));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithVVBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(null as RDFVariable, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithVVBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(new RDFVariable("?V"), null as RDFVariable));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithVTBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(null as RDFVariable, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithVTBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(new RDFVariable("?V"), null as RDFTypedLiteral));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOSymDifferenceExpressionWithVTBecauseNotGeographicRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoSymDifferenceExpression(new RDFVariable("?V"), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?MPT", typeof(string));
            DataRow row = table.NewRow();
            row["?MPT"] = new RDFTypedLiteral("<gml:MultiPoint xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pointMember><gml:Point><gml:pos>9.18854 45</gml:pos></gml:Point></gml:pointMember><gml:pointMember><gml:Point><gml:pos>9.18854 48.89</gml:pos></gml:Point></gml:pointMember></gml:MultiPoint>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariableExpression(new RDFVariable("?MPT")),
                new RDFConstantExpression(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 48.89)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResult2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PL1", typeof(string));
            table.Columns.Add("?PL2", typeof(string));
            DataRow row = table.NewRow();
            row["?PL1"] = new RDFTypedLiteral("POLYGON((9.188690185546873 45.46590974421454,7.667083740234373 45.056061242744164,8.916778564453123 44.406316252661384,9.188690185546873 45.46590974421454))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?PL2"] = new RDFTypedLiteral("POLYGON((8.212581852537832 44.9137259976078,9.157406071287832 45.189259240893335,9.701229313475332 45.06135959832709,8.212581852537832 44.9137259976078))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariableExpression(new RDFVariable("?PL1")),
                new RDFVariableExpression(new RDFVariable("?PL2")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("MULTIPOLYGON (((9.068707 44.99966885, 8.91677856 44.40631625, 7.66708374 45.05606124, 9.18869019 45.46590974, 9.11421696 45.17676565, 8.21258185 44.91372599, 9.068707 44.99966885)), ((9.068707 44.99966885, 9.11421696 45.17676565, 9.15740607 45.18925924, 9.70122931 45.06135959, 9.068707 44.99966885)))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEVAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?LS", typeof(string));
            table.Columns.Add("?PT", typeof(string));            
            DataRow row = table.NewRow();
            row["?LS"] = new RDFTypedLiteral("<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:posList>9.18854 45 9.18854 49</gml:posList></gml:LineString>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
            row["?PT"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariableExpression(new RDFVariable("?LS")),
                new RDFVariable("?PT"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("LINESTRING (9.18854 45, 9.18854 49)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithETAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PL", typeof(string));
            DataRow row = table.NewRow();
            row["?PL"] = new RDFTypedLiteral("POLYGON((9.188690185546873 45.46590974421454,7.667083740234373 45.056061242744164,8.916778564453123 44.406316252661384,9.188690185546873 45.46590974421454))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariableExpression(new RDFVariable("?PL")),
                new RDFTypedLiteral("POLYGON((8.488311767578123 45.13361760070825,8.205413818359371 44.8947957646979,8.592681884765623 44.90841397875738,8.488311767578123 45.13361760070825))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);//This is a polygon with hole
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POLYGON ((9.18869019 45.46590974, 8.91677856 44.40631625, 7.66708374 45.05606124, 9.18869019 45.46590974), (8.48831177 45.1336176, 8.20541382 44.89479576, 8.59268188 44.90841397, 8.48831177 45.1336176))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?MILANANDROME", typeof(string));
            table.Columns.Add("?MILAN", typeof(string));
            DataRow row = table.NewRow();
            row["?MILANANDROME"] = new RDFTypedLiteral("<gml:MultiPoint xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pointMember><gml:Point><gml:pos>9.18854 45</gml:pos></gml:Point></gml:pointMember><gml:pointMember><gml:Point><gml:pos>9.18854 54</gml:pos></gml:Point></gml:pointMember></gml:MultiPoint>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
            row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariable("?MILANANDROME"),
                new RDFVariableExpression(new RDFVariable("?MILAN")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 54)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
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

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariable("?MILAN"),
                new RDFVariable("?MILAN"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT EMPTY", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVTAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?LS", typeof(string));
            DataRow row = table.NewRow();
            row["?LS"] = new RDFTypedLiteral("LINESTRING (9.18854 45, 9.18854 54)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFVariable("?LS"),
                new RDFTypedLiteral("LINESTRING (9.18854 45, 9.18854 49)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("MULTILINESTRING ((9.18854 45, 9.18854 54), (9.18854 45, 9.18854 49))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
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

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
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

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
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

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
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

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
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

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
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

            RDFGeoSymDifferenceExpression expression = new RDFGeoSymDifferenceExpression(
                new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
                new RDFVariableExpression(new RDFVariable("?NAPLES")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}