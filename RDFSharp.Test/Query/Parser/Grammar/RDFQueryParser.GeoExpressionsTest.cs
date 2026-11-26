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

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the GeoSPARQL geof: function-call dispatch of RDFQueryParser.
/// </summary>
public partial class RDFQueryParserTest
{
    #region GeoFunctions

    [TestMethod]
    public void ShouldThrowOnUnknownFunctionIri()
    {
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(<http://ex/fn>(?o)) }"));
    }

    //Helper: the geo expression a bare geo-relation constraint desugars to (geoExpr = true → comparison's left side).
    private static RDFExpression GeoExpressionOf(RDFSelectQuery query)
    {
        RDFFilter expressionFilter = (RDFFilter)SingleFilterOf(query);
        return (RDFExpression)((RDFComparisonExpression)expressionFilter.Expression).LeftArgument;
    }

    [TestMethod]
    public void ShouldParseGeoTopologicalRelationFunction()
    {
        //geof: is a well-known registered prefix, so it resolves without an explicit PREFIX declaration
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:sfWithin(?a, ?b)) }");

        Assert.IsInstanceOfType<RDFGeoWithinExpression>(GeoExpressionOf(query));
    }

    [TestMethod]
    public void ShouldParseGeoEgenhoferRelationFunction()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:ehContains(?a, ?b)) }");

        RDFGeoEgenhoferExpression egenhofer = (RDFGeoEgenhoferExpression)GeoExpressionOf(query);
        Assert.IsNotNull(egenhofer);
    }

    [TestMethod]
    public void ShouldParseGeoRCC8RelationFunction()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:rcc8eq(?a, ?b)) }");

        Assert.IsInstanceOfType<RDFGeoRCC8Expression>(GeoExpressionOf(query));
    }

    [TestMethod]
    public void ShouldParseGeoRelateFunctionWithDe9imMatrix()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:relate(?a, ?b, \"T*T***T**\")) }");

        Assert.IsInstanceOfType<RDFGeoRelateExpression>(GeoExpressionOf(query));
    }

    [TestMethod]
    public void ShouldParseGeoDistanceFunctionInsideComparison()
    {
        //geof:distance(g1, g2, unit) is a scalar function used here as the left side of a numeric comparison
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:distance(?a, ?b, <http://www.opengis.net/def/uom/OGC/1.0/metre>) < 100) }");

        RDFFilter expressionFilter = (RDFFilter)SingleFilterOf(query);
        RDFComparisonExpression comparison = (RDFComparisonExpression)expressionFilter.Expression;
        Assert.IsInstanceOfType<RDFGeoDistanceExpression>(comparison.LeftArgument);
    }

    [TestMethod]
    public void ShouldParseGeoBufferFunctionWithNumericRadius()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?a FILTER(geof:sfWithin(geof:buffer(?a, 10, <http://www.opengis.net/def/uom/OGC/1.0/metre>), ?a)) }");

        RDFGeoWithinExpression within = (RDFGeoWithinExpression)GeoExpressionOf(query);
        Assert.IsInstanceOfType<RDFGeoBufferExpression>(within.LeftArgument);
    }

    [TestMethod]
    public void ShouldParseGeoUnaryFunction()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:sfWithin(geof:envelope(?a), ?b)) }");

        RDFGeoWithinExpression within = (RDFGeoWithinExpression)GeoExpressionOf(query);
        Assert.IsInstanceOfType<RDFGeoEnvelopeExpression>(within.LeftArgument);
    }

    [TestMethod]
    public void ShouldThrowOnGeoFunctionWithWrongArity()
    {
        //geof:sfWithin requires exactly 2 arguments
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a FILTER(geof:sfWithin(?a)) }"));
    }

    [TestMethod]
    public void ShouldParseGeoBufferFunctionWithDynamicRadius()
    {
        //When the radius is not a constant numeric literal (here, a variable), it is kept as a per-row expression
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?a . ?s ?r ?radius FILTER(geof:sfWithin(geof:buffer(?a, ?radius, <http://www.opengis.net/def/uom/OGC/1.0/metre>), ?a)) }");

        RDFGeoWithinExpression within = (RDFGeoWithinExpression)GeoExpressionOf(query);
        RDFGeoBufferExpression buffer = (RDFGeoBufferExpression)within.LeftArgument;
        Assert.IsNotNull(buffer.BufferMetersExpression);
    }

    [TestMethod]
    public void ShouldThrowOnGeoBufferFunctionWithWrongArity()
    {
        //geof:buffer requires 2 or 3 arguments
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a FILTER(geof:sfWithin(geof:buffer(?a), ?a)) }"));
    }

    /// <summary>
    /// Sweeps EVERY GeoSPARQL function in the unary, binary, Egenhofer and RCC8 dispatch tables, invoking each by
    /// its full IRI inside a bare FILTER (where any RDFGeoExpression is wrapped as 'expr = true'), and asserting
    /// the parser builds the expected RDFGeoExpression subtype. This covers every dispatch-table entry — including
    /// the geometry-returning set functions and the eight Egenhofer / eight RCC8 relations — that the focused
    /// tests above did not individually exercise. The function IRIs are taken from RDFVocabulary so the test stays
    /// correct if a vocabulary IRI ever changes.
    /// </summary>
    [TestMethod]
    public void ShouldParseEveryGeoDispatchTableFunction()
    {
        (RDFResource Iri, int Arity, System.Type Expected)[] geoFunctions =
        {
            //Unary (geometry → geometry/scalar)
            (RDFVocabulary.GEOSPARQL.GEOF.BOUNDARY, 1, typeof(RDFGeoBoundaryExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.CONVEX_HULL, 1, typeof(RDFGeoConvexHullExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.ENVELOPE, 1, typeof(RDFGeoEnvelopeExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.GET_SRID, 1, typeof(RDFGeoGetSRIDExpression)),
            (RDFVocabulary.GEOSPARQL.DIMENSION, 1, typeof(RDFGeoDimensionExpression)),
            (RDFVocabulary.GEOSPARQL.IS_EMPTY, 1, typeof(RDFGeoIsEmptyExpression)),
            (RDFVocabulary.GEOSPARQL.IS_SIMPLE, 1, typeof(RDFGeoIsSimpleExpression)),
            //Binary topological (geometry × geometry → boolean)
            (RDFVocabulary.GEOSPARQL.GEOF.SF_CONTAINS, 2, typeof(RDFGeoContainsExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SF_CROSSES, 2, typeof(RDFGeoCrossesExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SF_DISJOINT, 2, typeof(RDFGeoDisjointExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SF_EQUALS, 2, typeof(RDFGeoEqualsExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SF_INTERSECTS, 2, typeof(RDFGeoIntersectsExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SF_OVERLAPS, 2, typeof(RDFGeoOverlapsExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SF_TOUCHES, 2, typeof(RDFGeoTouchesExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SF_WITHIN, 2, typeof(RDFGeoWithinExpression)),
            //Binary set (geometry × geometry → geometry)
            (RDFVocabulary.GEOSPARQL.GEOF.DIFFERENCE, 2, typeof(RDFGeoDifferenceExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.INTERSECTION, 2, typeof(RDFGeoIntersectionExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SYM_DIFFERENCE, 2, typeof(RDFGeoSymDifferenceExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.SF_UNION, 2, typeof(RDFGeoUnionExpression)),
            //Egenhofer relations (all share RDFGeoEgenhoferExpression)
            (RDFVocabulary.GEOSPARQL.GEOF.EH_EQUALS, 2, typeof(RDFGeoEgenhoferExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.EH_DISJOINT, 2, typeof(RDFGeoEgenhoferExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.EH_MEET, 2, typeof(RDFGeoEgenhoferExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.EH_OVERLAP, 2, typeof(RDFGeoEgenhoferExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.EH_COVERS, 2, typeof(RDFGeoEgenhoferExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.EH_COVERED_BY, 2, typeof(RDFGeoEgenhoferExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.EH_INSIDE, 2, typeof(RDFGeoEgenhoferExpression)),
            (RDFVocabulary.GEOSPARQL.GEOF.EH_CONTAINS, 2, typeof(RDFGeoEgenhoferExpression)),
            //RCC8 relations (all share RDFGeoRCC8Expression)
            (RDFVocabulary.GEOSPARQL.GEOF.RCC8DC, 2, typeof(RDFGeoRCC8Expression)),
            (RDFVocabulary.GEOSPARQL.GEOF.RCC8EC, 2, typeof(RDFGeoRCC8Expression)),
            (RDFVocabulary.GEOSPARQL.GEOF.RCC8PO, 2, typeof(RDFGeoRCC8Expression)),
            (RDFVocabulary.GEOSPARQL.GEOF.RCC8TPPI, 2, typeof(RDFGeoRCC8Expression)),
            (RDFVocabulary.GEOSPARQL.GEOF.RCC8TPP, 2, typeof(RDFGeoRCC8Expression)),
            (RDFVocabulary.GEOSPARQL.GEOF.RCC8NTPP, 2, typeof(RDFGeoRCC8Expression)),
            (RDFVocabulary.GEOSPARQL.GEOF.RCC8NTPPI, 2, typeof(RDFGeoRCC8Expression)),
            (RDFVocabulary.GEOSPARQL.GEOF.RCC8EQ, 2, typeof(RDFGeoRCC8Expression))
        };

        foreach ((RDFResource Iri, int Arity, System.Type Expected) geoFunction in geoFunctions)
        {
            string arguments = geoFunction.Arity == 1 ? "?a" : "?a, ?b";
            RDFSelectQuery query = RDFSelectQuery.FromString(
                $"SELECT * WHERE {{ ?s ?p ?a . ?s ?q ?b FILTER(<{geoFunction.Iri}>({arguments})) }}");

            Assert.IsInstanceOfType(GeoExpressionOf(query), geoFunction.Expected, geoFunction.Iri.ToString());
        }
    }
    #endregion

    #region GeoUnits
    private const string MetreUnit = "http://www.opengis.net/def/uom/OGC/1.0/metre";
    private const string QudtUnits = "http://qudt.org/vocab/unit/";

    //Helper: the left side of the comparison that a geo-distance filter desugars to (the converted distance expression).
    private static RDFExpression DistanceExpressionOf(string unit)
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            $"SELECT * WHERE {{ ?s ?p ?a . ?s ?q ?b FILTER(geof:distance(?a, ?b, {unit}) < 100) }}");

        RDFFilter expressionFilter = (RDFFilter)SingleFilterOf(query);
        return (RDFExpression)((RDFComparisonExpression)expressionFilter.Expression).LeftArgument;
    }

    //Helper: the distance in the given unit between two fixed points, computed by the engine on a small graph.
    private static double EvaluateDistanceBetweenCities(string queryText)
    {
        RDFResource position = new RDFResource("http://ex/position");
        RDFGraph cities = new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://ex/milan"), position, new RDFTypedLiteral("POINT (9.19 45.46)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)))
            .AddTriple(new RDFTriple(new RDFResource("http://ex/rome"), position, new RDFTypedLiteral("POINT (12.49 41.89)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)));

        RDFSelectQueryResult result = RDFSelectQuery.FromString(queryText).ApplyToGraph(cities);

        //The cell prints as "value^^datatype": only the value is needed
        return double.Parse(result.SelectResults.Rows[0]["?D"].ToString().Split("^^")[0], System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string DistanceQueryInUnit(string unit)
        => $"SELECT ?d WHERE {{ <http://ex/milan> <http://ex/position> ?milan . <http://ex/rome> <http://ex/position> ?rome BIND(geof:distance(?milan, ?rome, {unit}) AS ?d) }}";

    [TestMethod]
    [DataRow("<" + MetreUnit + ">")]
    [DataRow("<" + QudtUnits + "M>")]
    public void ShouldParseGeoDistanceFunctionInMetresWithoutConversion(string unit)
        => Assert.IsInstanceOfType<RDFGeoDistanceExpression>(DistanceExpressionOf(unit));

    [TestMethod]
    [DataRow("DecaM", 10)]
    [DataRow("HectoM", 100)]
    [DataRow("KiloM", 1000)]
    public void ShouldParseGeoDistanceFunctionInLargerUnitAsDivision(string unitName, int scale)
    {
        RDFDivideExpression division = Assert.IsInstanceOfType<RDFDivideExpression>(DistanceExpressionOf($"<{QudtUnits}{unitName}>"));

        Assert.IsInstanceOfType<RDFGeoDistanceExpression>(division.LeftArgument);
        Assert.AreEqual(scale.ToString(), ((RDFTypedLiteral)division.RightArgument).Value);
        Assert.AreEqual(RDFVocabulary.XSD.INTEGER.ToString(), ((RDFTypedLiteral)division.RightArgument).Datatype.ToString());
    }

    [TestMethod]
    [DataRow("DeciM", 10)]
    [DataRow("CentiM", 100)]
    [DataRow("MilliM", 1000)]
    [DataRow("MicroM", 1000000)]
    public void ShouldParseGeoDistanceFunctionInSmallerUnitAsMultiplication(string unitName, int scale)
    {
        RDFMultiplyExpression multiplication = Assert.IsInstanceOfType<RDFMultiplyExpression>(DistanceExpressionOf($"<{QudtUnits}{unitName}>"));

        Assert.IsInstanceOfType<RDFGeoDistanceExpression>(multiplication.LeftArgument);
        Assert.AreEqual(scale.ToString(), ((RDFTypedLiteral)multiplication.RightArgument).Value);
    }

    [TestMethod]
    public void ShouldEvaluateGeoDistanceFunctionInConvertedUnits()
    {
        double metres = EvaluateDistanceBetweenCities(DistanceQueryInUnit("<" + MetreUnit + ">"));
        double kilometres = EvaluateDistanceBetweenCities(DistanceQueryInUnit("<" + QudtUnits + "KiloM>"));
        double millimetres = EvaluateDistanceBetweenCities(DistanceQueryInUnit("<" + QudtUnits + "MilliM>"));

        Assert.IsGreaterThan(0, metres);
        Assert.AreEqual(metres / 1000, kilometres, 1e-9);
        Assert.AreEqual(metres * 1000, millimetres, 1e-3);
    }

    [TestMethod]
    public void ShouldParseGeoDistanceFunctionInConvertedUnitBackAfterPrinting()
    {
        string kilometresQuery = DistanceQueryInUnit("<" + QudtUnits + "KiloM>");

        RDFSelectQuery parsedQuery = RDFSelectQuery.FromString(kilometresQuery);
        RDFSelectQuery reparsedQuery = RDFSelectQuery.FromString(parsedQuery.ToString());

        Assert.AreEqual(EvaluateDistanceBetweenCities(kilometresQuery), EvaluateDistanceBetweenCities(parsedQuery.ToString()), 1e-9);
        Assert.AreEqual(parsedQuery.ToString(), reparsedQuery.ToString());
    }

    [TestMethod]
    [DataRow(MetreUnit, 10d, 10d)]
    [DataRow(QudtUnits + "M", 10d, 10d)]
    [DataRow(QudtUnits + "KiloM", 10d, 10000d)]
    [DataRow(QudtUnits + "HectoM", 3d, 300d)]
    [DataRow(QudtUnits + "DecaM", 2d, 20d)]
    [DataRow(QudtUnits + "DeciM", 5d, 0.5d)]
    [DataRow(QudtUnits + "CentiM", 25d, 0.25d)]
    [DataRow(QudtUnits + "MilliM", 1500d, 1.5d)]
    [DataRow(QudtUnits + "MicroM", 500000d, 0.5d)]
    public void ShouldParseGeoBufferFunctionWithFixedRadiusConvertedToMetres(string unit, double radius, double expectedMeters)
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            $"SELECT * WHERE {{ ?s ?p ?a FILTER(geof:sfWithin(geof:buffer(?a, {radius.ToString(System.Globalization.CultureInfo.InvariantCulture)}, <{unit}>), ?a)) }}");

        RDFGeoBufferExpression buffer = (RDFGeoBufferExpression)((RDFGeoWithinExpression)GeoExpressionOf(query)).LeftArgument;
        Assert.AreEqual(expectedMeters, buffer.BufferMeters);
        Assert.IsNull(buffer.BufferMetersExpression);
    }

    [TestMethod]
    public void ShouldParseGeoBufferFunctionWithoutUnitInMetres()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a FILTER(geof:sfWithin(geof:buffer(?a, 25), ?a)) }");

        RDFGeoBufferExpression buffer = (RDFGeoBufferExpression)((RDFGeoWithinExpression)GeoExpressionOf(query)).LeftArgument;
        Assert.AreEqual(25d, buffer.BufferMeters);
    }

    [TestMethod]
    public void ShouldParseGeoBufferFunctionWithDynamicRadiusInLargerUnitAsMultiplication()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            $"SELECT * WHERE {{ ?s ?p ?a . ?s ?r ?radius FILTER(geof:sfWithin(geof:buffer(?a, ?radius, <{QudtUnits}KiloM>), ?a)) }}");

        RDFGeoBufferExpression buffer = (RDFGeoBufferExpression)((RDFGeoWithinExpression)GeoExpressionOf(query)).LeftArgument;
        RDFMultiplyExpression multiplication = Assert.IsInstanceOfType<RDFMultiplyExpression>(buffer.BufferMetersExpression);
        Assert.AreEqual("1000", ((RDFTypedLiteral)multiplication.RightArgument).Value);
    }

    [TestMethod]
    public void ShouldParseGeoBufferFunctionWithDynamicRadiusInSmallerUnitAsDivision()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            $"SELECT * WHERE {{ ?s ?p ?a . ?s ?r ?radius FILTER(geof:sfWithin(geof:buffer(?a, ?radius, <{QudtUnits}MilliM>), ?a)) }}");

        RDFGeoBufferExpression buffer = (RDFGeoBufferExpression)((RDFGeoWithinExpression)GeoExpressionOf(query)).LeftArgument;
        RDFDivideExpression division = Assert.IsInstanceOfType<RDFDivideExpression>(buffer.BufferMetersExpression);
        Assert.AreEqual("1000", ((RDFTypedLiteral)division.RightArgument).Value);
    }

    [TestMethod]
    public void ShouldParseGeoBufferFunctionWithDynamicRadiusInMetresWithoutConversion()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            $"SELECT * WHERE {{ ?s ?p ?a . ?s ?r ?radius FILTER(geof:sfWithin(geof:buffer(?a, ?radius, <{MetreUnit}>), ?a)) }}");

        RDFGeoBufferExpression buffer = (RDFGeoBufferExpression)((RDFGeoWithinExpression)GeoExpressionOf(query)).LeftArgument;
        Assert.IsInstanceOfType<RDFVariableExpression>(buffer.BufferMetersExpression);
    }

    [TestMethod]
    [DataRow("<" + QudtUnits + "FT>", QudtUnits + "FT")]
    [DataRow("<" + QudtUnits + "YD>", QudtUnits + "YD")]
    [DataRow("<http://www.opengis.net/def/uom/OGC/1.0/degree>", "http://www.opengis.net/def/uom/OGC/1.0/degree")]
    [DataRow("?u", "?")]
    [DataRow("\"m\"", "m")]
    public void ShouldThrowOnGeoFunctionsWithUnsupportedUnit(string unit, string expectedInMessage)
    {
        RDFQueryException distanceException = Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s ?p ?a . ?s ?q ?b . ?s ?r ?u FILTER(geof:distance(?a, ?b, {unit}) < 100) }}"));
        RDFQueryException bufferException = Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s ?p ?a . ?s ?r ?u FILTER(geof:sfWithin(geof:buffer(?a, 10, {unit}), ?a)) }}"));

        Assert.Contains("is not supported", distanceException.Message);
        Assert.Contains(expectedInMessage, distanceException.Message);
        Assert.Contains("is not supported", bufferException.Message);
        Assert.Contains(expectedInMessage, bufferException.Message);
    }
    #endregion
}
