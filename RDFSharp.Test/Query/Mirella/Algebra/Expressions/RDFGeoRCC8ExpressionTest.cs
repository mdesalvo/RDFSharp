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
public class RDFGeoRCC8ExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGEORCC8ExpressionWithExpressions()
    {
        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariableExpression(new RDFVariable("?V")),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RCC8DC}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:rcc8dc(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldCreateGEORCC8ExpressionWithExpressionAndVariable()
    {
        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFVariable("?V"),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8EC);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RCC8EC}>(\"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, ?V))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:rcc8ec(\"POINT (1 1)\"^^geosparql:wktLiteral, ?V))"));
    }

    [TestMethod]
    public void ShouldCreateGEORCC8ExpressionWithExpressionAndTypedLiteral()
    {
        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFConstantExpression(new RDFTypedLiteral("POINT (2 2)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8EQ);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RCC8EQ}>(\"POINT (2 2)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:rcc8eq(\"POINT (2 2)\"^^geosparql:wktLiteral, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldCreateGEORCC8ExpressionWithVariableAndExpression()
    {
        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?V"),
            new RDFConstantExpression(new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPP);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RCC8NTPP}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:rcc8ntpp(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldCreateGEORCC8ExpressionWithVariables()
    {
        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPPI);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RCC8NTPPI}>(?V1, ?V2))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:rcc8ntppi(?V1, ?V2))"));
    }

    [TestMethod]
    public void ShouldCreateGEORCC8ExpressionWithVariableAndTypedLiteral1()
    {
        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?V"),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8PO);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RCC8PO}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:rcc8po(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldCreateGEORCC8ExpressionWithVariableAndTypedLiteral2()
    {
        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?V"),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPP);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RCC8TPP}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:rcc8tpp(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldCreateGEORCC8ExpressionWithVariableAndTypedLiteral3()
    {
        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?V"),
            new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPPI);

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.RCC8TPPI}>(?V, \"POINT (1 1)\"^^<{RDFVocabulary.GEOSPARQL.WKT_LITERAL}>))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof"), RDFNamespaceRegister.GetByPrefix("geosparql")]).Equals("(geof:rcc8tppi(?V, \"POINT (1 1)\"^^geosparql:wktLiteral))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithEEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(null as RDFVariableExpression, new RDFVariableExpression(new RDFVariable("?V")), RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithEEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariableExpression, RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithEVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(null as RDFVariableExpression, new RDFVariable("?V"), RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithEVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFVariable, RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithETBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(null as RDFVariableExpression, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithETBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFTypedLiteral, RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithETBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(new RDFVariableExpression(new RDFVariable("?V")), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING), RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithVEBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V")), RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithVEBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(new RDFVariable("?V"), null as RDFVariableExpression, RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithVVBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(null as RDFVariable, new RDFVariable("?V"), RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithVVBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(new RDFVariable("?V"), null as RDFVariable, RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithVTBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(null as RDFVariable, new RDFTypedLiteral("POINT (1 1)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT), RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithVTBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(new RDFVariable("?V"), null as RDFTypedLiteral, RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEORCC8ExpressionWithVTBecauseNotGeographicRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFGeoRCC8Expression(new RDFVariable("?V"), new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING), RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC));

    [TestMethod]
    public void ShouldApplyExpressionWithEEAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?PL1", typeof(string));
        table.Columns.Add("?PL2", typeof(string));
        DataRow row = table.NewRow();
        row["?PL1"] = new RDFTypedLiteral("POLYGON((10.497706267187477 43.83853444111245,11.228297087499977 43.779072001707746,10.898707243749977 43.94146242961902,10.497706267187477 43.83853444111245))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?PL2"] = new RDFTypedLiteral("POLYGON((11.656763884374977 43.80286408108511,11.217310759374977 43.317230644814956,12.299464079687477 43.53266599350556,11.656763884374977 43.80286408108511))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariableExpression(new RDFVariable("?PL1")),
            new RDFVariableExpression(new RDFVariable("?PL2")),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC);
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
        row["?PL1"] = new RDFTypedLiteral("POLYGON((11.656763884374977 43.80286408108511,11.217310759374977 43.317230644814956,12.299464079687477 43.53266599350556,11.656763884374977 43.80286408108511))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?PL2"] = new RDFTypedLiteral("POLYGON((11.656763884374977 43.80286408108511,11.217310759374977 43.317230644814956,11.129420134374977 43.68778129452736,11.656763884374977 43.80286408108511))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariableExpression(new RDFVariable("?PL1")),
            new RDFVariable("?PL2"),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8EC);
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
        row["?PL2"] = new RDFTypedLiteral("POLYGON((11.62520371316032 44.84294151328678,11.33955918191032 44.491335321453306,11.60323105691032 44.64005141513215,11.62520371316032 44.84294151328678))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariableExpression(new RDFVariable("?PL2")),
            new RDFVariable("?PL1"),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8EQ);
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
        row["?PL1"] = new RDFTypedLiteral("POLYGON((11.228297087499977 43.62021611668259,11.244776579687477 43.54063035550621,11.464503142187477 43.6679170744954,11.371119353124977 43.68778129452736,11.228297087499977 43.62021611668259))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?PL2"] = new RDFTypedLiteral("POLYGON((11.656763884374977 43.80286408108511,11.217310759374977 43.317230644814956,11.129420134374977 43.68778129452736,11.656763884374977 43.80286408108511))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?PL1"),
            new RDFVariableExpression(new RDFVariable("?PL2")),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPP);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVVAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?PL1", typeof(string));
        table.Columns.Add("?PL2", typeof(string));
        DataRow row = table.NewRow();
        row["?PL1"] = new RDFTypedLiteral("POLYGON((11.228297087499977 43.62021611668259,11.244776579687477 43.54063035550621,11.464503142187477 43.6679170744954,11.371119353124977 43.68778129452736,11.228297087499977 43.62021611668259))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?PL2"] = new RDFTypedLiteral("POLYGON((11.656763884374977 43.80286408108511,11.217310759374977 43.317230644814956,11.129420134374977 43.68778129452736,11.656763884374977 43.80286408108511))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?PL2"),
            new RDFVariableExpression(new RDFVariable("?PL1")),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPPI);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVTAndCalculateResult1()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?PL", typeof(string));
        DataRow row = table.NewRow();
        row["?PL"] = new RDFTypedLiteral("POLYGON((11.312090143014858 43.600781279343785,11.363588556100796 43.62116478406795,11.346422418405483 43.56994432121584,11.312090143014858 43.600781279343785))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?PL"),
            new RDFTypedLiteral("POLYGON((11.228297087499977 43.62021611668259,11.244776579687477 43.54063035550621,11.464503142187477 43.6679170744954,11.371119353124977 43.68778129452736,11.228297087499977 43.62021611668259))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8PO);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVTAndCalculateResult2()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?PL", typeof(string));
        DataRow row = table.NewRow();
        row["?PL"] = new RDFTypedLiteral("POLYGON((11.312090143014858 43.600781279343785,11.363588556100796 43.62116478406795,11.346422418405483 43.56994432121584,11.312090143014858 43.600781279343785))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?PL"),
            new RDFTypedLiteral("POLYGON((11.312090143014858 43.600781279343785,11.363588556100796 43.62116478406795,11.351915582467983 43.5067289768788,11.312090143014858 43.600781279343785))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPP);
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
        row["?PL"] = new RDFTypedLiteral("POLYGON((11.312090143014858 43.600781279343785,11.363588556100796 43.62116478406795,11.351915582467983 43.5067289768788,11.312090143014858 43.600781279343785))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariable("?PL"),
            new RDFTypedLiteral("POLYGON((11.312090143014858 43.600781279343785,11.363588556100796 43.62116478406795,11.346422418405483 43.56994432121584,11.312090143014858 43.600781279343785))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPPI);
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

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC);
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

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC);
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

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariableExpression(new RDFVariable("?ROME")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC);
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

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?ROME")),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC);
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

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC);
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

        RDFGeoRCC8Expression expression = new RDFGeoRCC8Expression(
            new RDFConstantExpression(new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)),
            new RDFVariableExpression(new RDFVariable("?NAPLES")),
            RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}