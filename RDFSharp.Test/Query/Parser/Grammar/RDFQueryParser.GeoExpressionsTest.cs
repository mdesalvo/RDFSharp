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
}
