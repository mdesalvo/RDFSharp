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
public class RDFGeoBufferExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGEOBufferExpressionWithExpression()
    {
        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariableExpression(new RDFVariable("?V")), 150); //150mt buffering

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.BUFFER}>(?V, 150))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof")]).Equals("(geof:buffer(?V, 150))"));
    }

    [TestMethod]
    public void ShouldCreateGEOBufferExpressionWithVariable()
    {
        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariable("?V"), 150); //150mt buffering

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.GEOSPARQL.GEOF.BUFFER}>(?V, 150))"));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("geof")]).Equals("(geof:buffer(?V, 150))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOBufferExpressionWithEXPBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoBufferExpression(null as RDFVariableExpression, 150));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingGEOBufferExpressionWithVARBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFGeoBufferExpression(null as RDFVariable, 150));

    [TestMethod]
    public void ShouldApplyExpressionWithEXPAndCalculateResultPoint()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        DataRow row = table.NewRow();
        row["?MILAN"] = new RDFTypedLiteral("POINT (9.18854 45.464664)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROME"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariableExpression(new RDFVariable("?MILAN")), 1000); //1000mt buffering
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POLYGON ((9.20054505 45.46515273, 9.20060654 45.46328561, 9.20020433 45.46147148, 9.19935391 45.45978006, 9.19808799 45.45827634, 9.19645522 45.4570181, 9.19451837 45.45605368, 9.19235185 45.45542015, 9.19003889 45.45514182, 9.18766835 45.45522941, 9.18533128 45.45567953, 9.18311747 45.4564749, 9.18111196 45.45758495, 9.1793918 45.45896703, 9.17802311 45.46056803, 9.17705848 45.46232645, 9.17653501 45.46417472, 9.17647286 45.4660418, 9.17687445 45.46785596, 9.17772438 45.46954748, 9.17899002 45.47105134, 9.18062275 45.47230974, 9.18255982 45.47327432, 9.18472678 45.473908, 9.18704033 45.47418641, 9.18941153 45.47409886, 9.19174922 45.47364871, 9.19396352 45.47285325, 9.19596932 45.47174307, 9.19768951 45.47036082, 9.19905799 45.46875965, 9.20002218 45.46700109, 9.20054505 45.46515273))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVARAndCalculateResult()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILAN", typeof(string));
        table.Columns.Add("?ROME", typeof(string));
        DataRow row = table.NewRow();
        row["?ROME"] = new RDFTypedLiteral("POINT (12.496365 41.902782)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?MILAN"] = new RDFTypedLiteral("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>9.18854 45.464664</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariable("?MILAN"), 150); //150mt buffering
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POLYGON ((9.19034075 45.46473734, 9.19035002 45.46445727, 9.19028973 45.46418515, 9.1901622 45.46393143, 9.18997232 45.46370587, 9.18972741 45.46351712, 9.18943687 45.46337245, 9.18911186 45.46327741, 9.18876488 45.46323565, 9.18840926 45.46324879, 9.18805866 45.46331631, 9.18772656 45.46343562, 9.18742571 45.46360214, 9.18716769 45.46380946, 9.1869624 45.46404962, 9.18681773 45.46431339, 9.18673925 45.46459064, 9.18672997 45.4648707, 9.18679024 45.46514283, 9.18691777 45.46539655, 9.18710763 45.46562212, 9.18735254 45.46581087, 9.18764309 45.46595554, 9.1879681 45.46605059, 9.1883151 45.46609234, 9.18867074 45.46607921, 9.18902135 45.46601169, 9.18935346 45.46589237, 9.18965432 45.46572585, 9.18991234 45.46551853, 9.19011763 45.46527836, 9.19026229 45.46501459, 9.19034075 45.46473734))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEXPAndCalculateResultLineString()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILANROME", typeof(string));
        table.Columns.Add("?ROMENAPLES", typeof(string));
        DataRow row = table.NewRow();
        row["?MILANROME"] = new RDFTypedLiteral("LINESTRING (9.18854 45.464664, 12.496365 41.902782)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROMENAPLES"] = new RDFTypedLiteral("<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:posList>12.496365 41.902782 14.2681244 40.8517746</gml:posList></gml:LineString>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariableExpression(new RDFVariable("?MILANROME")), 150); //150mt buffering
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POLYGON ((12.49774323 41.9035796, 12.49791781 41.90333506, 12.49803273 41.90306927, 12.49808355 41.90279244, 12.49806832 41.90251521, 12.49798764 41.90224823, 12.49784461 41.90200176, 12.49764471 41.90178528, 12.49739564 41.9016071, 12.49710696 41.90147407, 12.49678977 41.90139131, 12.49645626 41.90136198, 12.49611924 41.90138723, 12.49579167 41.90146607, 12.49548612 41.90159549, 12.49521435 41.9017705, 12.4949868 41.90198438, 9.18713753 45.46383931, 9.1869398 45.46408313, 9.18680356 45.46434926, 9.18673405 45.46462749, 9.18673394 45.46490712, 9.18680324 45.46517741, 9.18693928 45.46542797, 9.18713684 45.46564918, 9.18738832 45.46583252, 9.18768406 45.46597095, 9.1880127 45.46605916, 9.1883616 45.46609376, 9.18871736 45.46607341, 9.18906631 45.46599889, 9.18939502 45.46587308, 9.18969088 45.4657008, 9.1899425 45.46548867, 12.49774323 41.9035796))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithEXPAndCalculateResultPolygon()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?MILANROMENAPLES", typeof(string));
        table.Columns.Add("?ROMENAPLESMILAN", typeof(string));
        DataRow row = table.NewRow();
        row["?MILANROMENAPLES"] = new RDFTypedLiteral("POLYGON ((9.18854 45.464664, 12.496365 41.902782, 14.2681244 40.8517746, 9.18854 45.464664))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT).ToString();
        row["?ROMENAPLESMILAN"] = new RDFTypedLiteral("<gml:Polygon xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>12.496365 41.902782 14.2681244 40.8517746 9.18854 45.464664 12.496365 41.902782</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariableExpression(new RDFVariable("?ROMENAPLESMILAN")), 150); //150mt buffering
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("POLYGON ((12.4949868 41.90198438, 9.18713753 45.46383931, 9.18693433 45.46409152, 9.18679693 45.46436719, 9.18673095 45.46465502, 9.18673909 45.46494322, 9.18682102 45.46521998, 9.18697338 45.46547396, 9.18718994 45.46569475, 9.18746181 45.4658733, 9.18777787 45.46600231, 9.18812516 45.46607648, 9.18848944 45.46609277, 9.1888558 45.46605052, 9.18920921 45.46595146, 9.1895352 45.46579965, 9.18982042 45.4656013, 14.26939718 40.85267492, 14.26958859 40.85244803, 14.26972557 40.85219612, 14.26980303 40.85192854, 14.26981809 40.85165523, 14.26977019 40.85138636, 14.26966112 40.85113192, 14.26949493 40.85090138, 14.26927779 40.85070329, 14.26901778 40.85054502, 14.26872456 40.85043246, 14.26840904 40.85036979, 14.26808293 40.85035934, 14.26775837 40.8504015, 14.26744741 40.85049469, 14.26716161 40.85063547, 12.49541681 41.9016339, 12.49518427 41.90179447, 12.4949868 41.90198438))", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));
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

        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariable("?NAPLES"), 150);
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

        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariableExpression(new RDFVariable("?ROME")), 150);
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

        RDFGeoBufferExpression expression = new RDFGeoBufferExpression(
            new RDFVariableExpression(new RDFVariable("?NAPLES")), 150);
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}