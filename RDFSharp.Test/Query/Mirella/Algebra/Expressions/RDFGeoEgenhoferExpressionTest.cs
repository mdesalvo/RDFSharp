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
    public class RDFGeoEgenhoferExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateGEOEgenhoferExpressionWithExpressions()
        {
            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariableExpression(new RDFVariable("?V")),
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Contains);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.EH_CONTAINS}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:ehContains(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOEgenhoferExpressionWithExpressionAndVariable()
        {
            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
                new RDFVariable("?V"),
                RDFQueryEnums.RDFGeoEgenhoferRelations.CoveredBy);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.EH_COVERED_BY}>(\"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, ?V))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:ehCoveredBy(\"POINT (1 1)\"^^geosparql:wktLiteral, ?V))"));
        }

        [TestMethod]
        public void ShouldCreateGEOEgenhoferExpressionWithExpressionAndTypedLiteral()
        {
            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFConstantExpression(new RDFTypedLiteral("POINT (2 2)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
                new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Covers);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.EH_COVERS}>(\"POINT (2 2)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:ehCovers(\"POINT (2 2)\"^^geosparql:wktLiteral, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOEgenhoferExpressionWithVariableAndExpression()
        {
            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?V"),
                new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Disjoint);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.EH_DISJOINT}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:ehDisjoint(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOEgenhoferExpressionWithVariables()
        {
            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?V1"),
                new RDFVariable("?V2"),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Equals);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.EH_EQUALS}>(?V1, ?V2))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:ehEquals(?V1, ?V2))"));
        }

        [TestMethod]
        public void ShouldCreateGEOEgenhoferExpressionWithVariableAndTypedLiteral1()
        {
            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?V"),
                new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Inside);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.EH_INSIDE}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:ehInside(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOEgenhoferExpressionWithVariableAndTypedLiteral2()
        {
            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?V"),
                new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Meet);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.EH_MEET}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:ehMeet(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldCreateGEOEgenhoferExpressionWithVariableAndTypedLiteral3()
        {
            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?V"),
                new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Overlap);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.EH_OVERLAP}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:ehOverlap(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithEEBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(null as RDFVariableExpression, new RDFVariableExpression(new RDFVariable("?V")), RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithEEBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariableExpression, RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithEVBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(null as RDFVariableExpression, new RDFVariable("?V"), RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithEVBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable, RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithETBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(null as RDFVariableExpression, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithETBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFTypedLiteral, RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithETBecauseNotGeographicRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING), RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithVEBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V")), RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithVEBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(new RDFVariable("?V"), null as RDFVariableExpression, RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithVVBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(null as RDFVariable, new RDFVariable("?V"), RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithVVBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(new RDFVariable("?V"), null as RDFVariable, RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithVTBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(null as RDFVariable, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithVTBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(new RDFVariable("?V"), null as RDFTypedLiteral, RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGEOEgenhoferExpressionWithVTBecauseNotGeographicRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGeoEgenhoferExpression(new RDFVariable("?V"), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING), RDFQueryEnums.RDFGeoEgenhoferRelations.Equals));

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PL", typeof(string));
            table.Columns.Add("?LS", typeof(string));
            DataRow row = table.NewRow();
            row["?PL"] = new RDFTypedLiteral("POLYGON ((9.18854 45, 12.496365 41.902782, 14.268124 40.851774, 9.18854 45))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?LS"] = new RDFTypedLiteral("LINESTRING(10.705256576624173 43.75731680339918,10.62217247017886 43.72631206695182)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariableExpression(new RDFVariable("?PL")),
                new RDFVariableExpression(new RDFVariable("?LS")),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Contains);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEVAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PL1", typeof(string));
            table.Columns.Add("?PL2", typeof(string));
            DataRow row = table.NewRow();
            row["?PL1"] = new RDFTypedLiteral("POLYGON((11.62520371316032 44.84294151328678,11.33955918191032 44.491335321453306,11.60323105691032 44.64005141513215,11.62520371316032 44.84294151328678))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?PL2"] = new RDFTypedLiteral("POLYGON((11.62520371316032 44.84294151328678,11.33955918191032 44.491335321453306,12.19649277566032 44.42075815909547,11.62520371316032 44.84294151328678))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariableExpression(new RDFVariable("?PL1")),
                new RDFVariable("?PL2"),
                RDFQueryEnums.RDFGeoEgenhoferRelations.CoveredBy);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithETAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PL1", typeof(string));
            table.Columns.Add("?PL2", typeof(string));
            DataRow row = table.NewRow();
            row["?PL1"] = new RDFTypedLiteral("POLYGON((11.62520371316032 44.84294151328678,11.33955918191032 44.491335321453306,11.60323105691032 44.64005141513215,11.62520371316032 44.84294151328678))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?PL2"] = new RDFTypedLiteral("POLYGON((11.62520371316032 44.84294151328678,11.33955918191032 44.491335321453306,12.19649277566032 44.42075815909547,11.62520371316032 44.84294151328678))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariableExpression(new RDFVariable("?PL2")),
                new RDFVariable("?PL1"),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Covers);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PL1", typeof(string));
            table.Columns.Add("?PL2", typeof(string));
            DataRow row = table.NewRow();
            row["?PL1"] = new RDFTypedLiteral("POLYGON((9.93880234597282 45.14206714385015,9.85640488503532 44.84294151328678,10.12007676003532 44.90133518584387,9.93880234597282 45.14206714385015))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            row["?PL2"] = new RDFTypedLiteral("POLYGON((11.62520371316032 44.84294151328678,11.33955918191032 44.491335321453306,12.19649277566032 44.42075815909547,11.62520371316032 44.84294151328678))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?PL1"),
                new RDFVariableExpression(new RDFVariable("?PL2")),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Disjoint);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PL", typeof(string));
            DataRow row = table.NewRow();
            row["?PL"] = new RDFTypedLiteral("POLYGON((12.396842691027471 42.77775532402146,12.619315835558721 42.95691699452285,12.745658608996221 42.737422725317835,12.396842691027471 42.77775532402146))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?PL"),
                new RDFVariable("?PL"),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Equals);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVTAndCalculateResult1()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?LS", typeof(string));
            DataRow row = table.NewRow();
            row["?LS"] = new RDFTypedLiteral("LINESTRING(11.05940781472282 44.874092178351034,11.27364121316032 44.7727908998851)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?LS"),
                new RDFTypedLiteral("POLYGON((10.80672226784782 45.16143690467078,10.87264023659782 44.7727908998851,11.33955918191032 44.491335321453306,11.63069687722282 44.850730759484335,10.80672226784782 45.16143690467078))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Inside);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVTAndCalculateResult2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?LS", typeof(string));
            DataRow row = table.NewRow();
            row["?LS"] = new RDFTypedLiteral("LINESTRING(11.05940781472282 44.874092178351034,11.27364121316032 44.7727908998851)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?LS"),
                new RDFTypedLiteral("LINESTRING(11.27364121316032 44.7727908998851,10.86165390847282 44.47565887940483)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Meet);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVTAndCalculateResult3()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?PL", typeof(string));
            DataRow row = table.NewRow();
            row["?PL"] = new RDFTypedLiteral("POLYGON((10.79024277566032 45.16143690467078,10.03218613503532 45.130442127495094,10.31783066628532 44.79618391717168,10.87264023659782 44.78448859345848,10.79024277566032 45.16143690467078))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariable("?PL"),
                new RDFTypedLiteral("POLYGON((10.40572129128532 44.9946418263578,10.44417343972282 44.4874166058652,11.27913437722282 44.530507996624955,11.03194199441032 45.052881454101424,10.40572129128532 44.9946418263578))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Overlap);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
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

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariableExpression(new RDFVariable("?NAPLES")),
                new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Equals);
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

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
                new RDFVariableExpression(new RDFVariable("?NAPLES")),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Equals);
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

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariableExpression(new RDFVariable("?ROME")),
                new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Equals);
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

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
                new RDFVariableExpression(new RDFVariable("?ROME")),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Equals);
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

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFVariableExpression(new RDFVariable("?NAPLES")),
                new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Equals);
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

            RDFGeoEgenhoferExpression expression = new RDFGeoEgenhoferExpression(
                new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
                new RDFVariableExpression(new RDFVariable("?NAPLES")),
                RDFQueryEnums.RDFGeoEgenhoferRelations.Equals);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}