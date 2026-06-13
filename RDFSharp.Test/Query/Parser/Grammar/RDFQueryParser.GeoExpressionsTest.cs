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

using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        return (RDFExpression)((RDFComparisonExpression)expressionFilter.Expression).LeftArgument;
    }

    [TestMethod]
    public void ShouldParseGeoTopologicalRelationFunction()
    {
        //geof: is a well-known registered prefix, so it resolves without an explicit PREFIX declaration
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:sfWithin(?a, ?b)) }");

        Assert.IsInstanceOfType(GeoExpressionOf(query), typeof(RDFGeoWithinExpression));
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

        Assert.IsInstanceOfType(GeoExpressionOf(query), typeof(RDFGeoRCC8Expression));
    }

    [TestMethod]
    public void ShouldParseGeoRelateFunctionWithDe9imMatrix()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:relate(?a, ?b, \"T*T***T**\")) }");

        Assert.IsInstanceOfType(GeoExpressionOf(query), typeof(RDFGeoRelateExpression));
    }

    [TestMethod]
    public void ShouldParseGeoDistanceFunctionInsideComparison()
    {
        //geof:distance(g1, g2, unit) is a scalar function used here as the left side of a numeric comparison
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:distance(?a, ?b, <http://www.opengis.net/def/uom/OGC/1.0/metre>) < 100) }");

        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        RDFComparisonExpression comparison = (RDFComparisonExpression)expressionFilter.Expression;
        Assert.IsInstanceOfType(comparison.LeftArgument, typeof(RDFGeoDistanceExpression));
    }

    [TestMethod]
    public void ShouldParseGeoBufferFunctionWithNumericRadius()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?a FILTER(geof:sfWithin(geof:buffer(?a, 10, <http://www.opengis.net/def/uom/OGC/1.0/metre>), ?a)) }");

        RDFGeoWithinExpression within = (RDFGeoWithinExpression)GeoExpressionOf(query);
        Assert.IsInstanceOfType(within.LeftArgument, typeof(RDFGeoBufferExpression));
    }

    [TestMethod]
    public void ShouldParseGeoUnaryFunction()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?a . ?s ?q ?b FILTER(geof:sfWithin(geof:envelope(?a), ?b)) }");

        RDFGeoWithinExpression within = (RDFGeoWithinExpression)GeoExpressionOf(query);
        Assert.IsInstanceOfType(within.LeftArgument, typeof(RDFGeoEnvelopeExpression));
    }

    [TestMethod]
    public void ShouldThrowOnGeoFunctionWithWrongArity()
    {
        //geof:sfWithin requires exactly 2 arguments
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?a FILTER(geof:sfWithin(?a)) }"));
    }
    #endregion
}
