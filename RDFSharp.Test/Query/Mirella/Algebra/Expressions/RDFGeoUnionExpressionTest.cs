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

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFGeoUnionExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateGEOUnionExpressionWithExpressions()
        {
            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariableExpression(new RDFVariable("?V")),
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_UNION}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:union(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOUnionExpressionWithExpressionAndVariable()
        {
            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
                new RDFVariable("?V"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_UNION}>(\"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, ?V))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:union(\"POINT (1 1)\"^^geosparql:wktLiteral, ?V))"));
        }

        [TestMethod]
        public void ShouldCreateGEOUnionExpressionWithExpressionAndTypedLiteral()
        {
            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFConstantExpression(new RDFTypedLiteral("POINT (2 2)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
                new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_UNION}>(\"POINT (2 2)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:union(\"POINT (2 2)\"^^geosparql:wktLiteral, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOUnionExpressionWithVariableAndExpression()
        {
            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariable("?V"),
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_UNION}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:union(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOUnionExpressionWithVariables()
        {
            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariable("?V1"),
                new RDFVariable("?V2"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_UNION}>(?V1, ?V2))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:union(?V1, ?V2))"));
        }

        [TestMethod]
        public void ShouldCreateGEOUnionExpressionWithVariableAndTypedLiteral()
        {
            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariable("?V"),
                new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.SF_UNION}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:union(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithEEBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(null as RDFVariableExpression, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithEEBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariableExpression));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithEVBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(null as RDFVariableExpression, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithEVBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithETBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(null as RDFVariableExpression, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithETBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFTypedLiteral));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithETBecauseNotGeographicRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithVEBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithVEBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(new RDFVariable("?V"), null as RDFVariableExpression));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithVVBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(null as RDFVariable, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithVVBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(new RDFVariable("?V"), null as RDFVariable));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithVTBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(null as RDFVariable, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithVTBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(new RDFVariable("?V"), null as RDFTypedLiteral));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOUnionExpressionWithVTBecauseNotGeographicRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoUnionExpression(new RDFVariable("?V"), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?MILAN", typeof(string));
            table.Columns.Add("?ROME", typeof(string));
            DataRow row = table.NewRow();
            row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.90278199</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariableExpression(new RDFVariable("?MILAN")),
                new RDFVariableExpression(new RDFVariable("?ROME")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("MULTIPOINT ((9.18854 45), (12.496365 41.90278198))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResultUnionSelf()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?MILAN", typeof(string));
            DataRow row = table.NewRow();
            row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariableExpression(new RDFVariable("?MILAN")),
                new RDFVariableExpression(new RDFVariable("?MILAN")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEVAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PT", typeof(string));
            table.Columns.Add("?LS", typeof(string));
            DataRow row = table.NewRow();
            row["?PT"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?LS"] = new RDFTypedLiteral("<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:posList>9.18854 45 9.2225 45.1</gml:posList></gml:LineString>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariableExpression(new RDFVariable("?PT")),
                new RDFVariable("?LS"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("LINESTRING (9.18854 45, 9.2225 45.1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithETAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?MILAN", typeof(string));
            DataRow row = table.NewRow();
            row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariableExpression(new RDFVariable("?MILAN")),
                new RDFTypedLiteral("POLYGON ((9.18854 45, 9.2225 45.1, 9.18854 45.85, 9.18854 45))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POLYGON ((9.18854 45, 9.18854 45.85, 9.2225 45.1, 9.18854 45))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PT", typeof(string));
            table.Columns.Add("?MPT", typeof(string));
            DataRow row = table.NewRow();
            row["?PT"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?MPT"] = new RDFTypedLiteral("<gml:MultiPoint xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pointMember><gml:Point><gml:pos>9.18854 45</gml:pos></gml:Point></gml:pointMember><gml:pointMember><gml:Point><gml:pos>9.18854 48.89</gml:pos></gml:Point></gml:pointMember></gml:MultiPoint>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariable("?PT"),
                new RDFVariableExpression(new RDFVariable("?MPT")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("MULTIPOINT ((9.18854 45), (9.18854 48.89))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
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

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariable("?MILAN"),
                new RDFVariable("?MILAN"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
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

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariable("?MILAN"),
                new RDFTypedLiteral("MULTIPOINT ((9.18854 48.89), (9.18854 45))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("MULTIPOINT ((9.18854 45), (9.18854 48.89))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
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

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
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

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
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

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
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

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
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

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
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

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
                new RDFVariableExpression(new RDFVariable("?NAPLES")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseUnknownLeftVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?MILAN", typeof(string));
            table.Columns.Add("?ROME", typeof(string));
            DataRow row = table.NewRow();
            row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariable("?NAPLES"),
                new RDFVariable("?MILAN"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndNotCalculateResultBecauseUnknownRightVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?MILAN", typeof(string));
            table.Columns.Add("?ROME", typeof(string));
            DataRow row = table.NewRow();
            row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoUnionExpression expression = new RDFGeoUnionExpression(
                new RDFVariable("?MILAN"),
                new RDFVariable("?NAPLES"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}