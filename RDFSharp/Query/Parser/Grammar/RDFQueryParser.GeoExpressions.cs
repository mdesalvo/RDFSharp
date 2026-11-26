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

using System;
using System.Collections.Generic;
using System.Globalization;
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// GeoSPARQL function-call dispatch for the SPARQL parser (phase F6b). SPARQL has no keyword for the GeoSPARQL
    /// functions: they are invoked through their IRI (e.g. <c>geof:sfWithin(?a, ?b)</c>), so they reach the parser as
    /// an <c>iriOrFunction</c> primary rather than as a <c>BuiltInCall</c>. This partial maps each recognised
    /// <c>geof:</c> / <c>geosparql:</c> function IRI onto its RDFSharp geo-expression class; any other function IRI is
    /// rejected with a precise "not supported" message.
    /// <para>
    /// DISPATCH STRATEGY. The recognised function IRIs are runtime values (<c>RDFVocabulary.GEOSPARQL…ToString()</c>),
    /// not compile-time constants, so they cannot be C# <c>switch</c> case labels without hardcoding (and thereby
    /// duplicating) the IRI strings. The dispatch is therefore driven by static lookup tables built ONCE from the
    /// vocabulary: a unary table, a binary table, and the two relation-family tables (Egenhofer, RCC8). Only the
    /// three argument-shaped functions (distance/buffer/relate), which each have bespoke argument handling, are
    /// matched individually.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region DispatchTables
        /// <summary>
        /// Unary GeoSPARQL functions (geometry → geometry/scalar), keyed by function IRI. Each value builds the
        /// matching geo-expression from the single parsed argument.
        /// </summary>
        private static readonly Dictionary<string, Func<RDFExpression, RDFGeoExpression>> GeoUnaryFunctions =
            new Dictionary<string, Func<RDFExpression, RDFGeoExpression>>
            {
                [RDFVocabulary.GEOSPARQL.GEOF.BOUNDARY.ToString()]    = arg => new RDFGeoBoundaryExpression(arg),
                [RDFVocabulary.GEOSPARQL.GEOF.CONVEX_HULL.ToString()] = arg => new RDFGeoConvexHullExpression(arg),
                [RDFVocabulary.GEOSPARQL.GEOF.ENVELOPE.ToString()]    = arg => new RDFGeoEnvelopeExpression(arg),
                [RDFVocabulary.GEOSPARQL.GEOF.GET_SRID.ToString()]    = arg => new RDFGeoGetSRIDExpression(arg),
                [RDFVocabulary.GEOSPARQL.DIMENSION.ToString()]        = arg => new RDFGeoDimensionExpression(arg),
                [RDFVocabulary.GEOSPARQL.IS_EMPTY.ToString()]         = arg => new RDFGeoIsEmptyExpression(arg),
                [RDFVocabulary.GEOSPARQL.IS_SIMPLE.ToString()]        = arg => new RDFGeoIsSimpleExpression(arg)
            };

        /// <summary>
        /// Binary GeoSPARQL functions (geometry × geometry → boolean/geometry), keyed by function IRI. Each value
        /// builds the matching geo-expression from the two parsed arguments.
        /// </summary>
        private static readonly Dictionary<string, Func<RDFExpression, RDFExpression, RDFGeoExpression>> GeoBinaryFunctions =
            new Dictionary<string, Func<RDFExpression, RDFExpression, RDFGeoExpression>>
            {
                [RDFVocabulary.GEOSPARQL.GEOF.SF_CONTAINS.ToString()]   = (l, r) => new RDFGeoContainsExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SF_CROSSES.ToString()]    = (l, r) => new RDFGeoCrossesExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SF_DISJOINT.ToString()]   = (l, r) => new RDFGeoDisjointExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SF_EQUALS.ToString()]     = (l, r) => new RDFGeoEqualsExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SF_INTERSECTS.ToString()] = (l, r) => new RDFGeoIntersectsExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SF_OVERLAPS.ToString()]   = (l, r) => new RDFGeoOverlapsExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SF_TOUCHES.ToString()]    = (l, r) => new RDFGeoTouchesExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SF_WITHIN.ToString()]     = (l, r) => new RDFGeoWithinExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.DIFFERENCE.ToString()]    = (l, r) => new RDFGeoDifferenceExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.INTERSECTION.ToString()]  = (l, r) => new RDFGeoIntersectionExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SYM_DIFFERENCE.ToString()]= (l, r) => new RDFGeoSymDifferenceExpression(l, r),
                [RDFVocabulary.GEOSPARQL.GEOF.SF_UNION.ToString()]      = (l, r) => new RDFGeoUnionExpression(l, r)
            };

        /// <summary>
        /// Egenhofer-relation functions (<c>geof:eh…</c>), keyed by function IRI, mapping to the relation enum value
        /// that parameterises the shared <see cref="RDFGeoEgenhoferExpression"/>.
        /// </summary>
        private static readonly Dictionary<string, RDFQueryEnums.RDFGeoEgenhoferRelations> GeoEgenhoferFunctions =
            new Dictionary<string, RDFQueryEnums.RDFGeoEgenhoferRelations>
            {
                [RDFVocabulary.GEOSPARQL.GEOF.EH_EQUALS.ToString()]     = RDFQueryEnums.RDFGeoEgenhoferRelations.Equals,
                [RDFVocabulary.GEOSPARQL.GEOF.EH_DISJOINT.ToString()]   = RDFQueryEnums.RDFGeoEgenhoferRelations.Disjoint,
                [RDFVocabulary.GEOSPARQL.GEOF.EH_MEET.ToString()]       = RDFQueryEnums.RDFGeoEgenhoferRelations.Meet,
                [RDFVocabulary.GEOSPARQL.GEOF.EH_OVERLAP.ToString()]    = RDFQueryEnums.RDFGeoEgenhoferRelations.Overlap,
                [RDFVocabulary.GEOSPARQL.GEOF.EH_COVERS.ToString()]     = RDFQueryEnums.RDFGeoEgenhoferRelations.Covers,
                [RDFVocabulary.GEOSPARQL.GEOF.EH_COVERED_BY.ToString()] = RDFQueryEnums.RDFGeoEgenhoferRelations.CoveredBy,
                [RDFVocabulary.GEOSPARQL.GEOF.EH_INSIDE.ToString()]     = RDFQueryEnums.RDFGeoEgenhoferRelations.Inside,
                [RDFVocabulary.GEOSPARQL.GEOF.EH_CONTAINS.ToString()]   = RDFQueryEnums.RDFGeoEgenhoferRelations.Contains
            };

        /// <summary>
        /// RCC8-relation functions (<c>geof:rcc8…</c>), keyed by function IRI, mapping to the relation enum value that
        /// parameterises the shared <see cref="RDFGeoRCC8Expression"/>.
        /// </summary>
        private static readonly Dictionary<string, RDFQueryEnums.RDFGeoRCC8Relations> GeoRCC8Functions =
            new Dictionary<string, RDFQueryEnums.RDFGeoRCC8Relations>
            {
                [RDFVocabulary.GEOSPARQL.GEOF.RCC8DC.ToString()]   = RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC,
                [RDFVocabulary.GEOSPARQL.GEOF.RCC8EC.ToString()]   = RDFQueryEnums.RDFGeoRCC8Relations.RCC8EC,
                [RDFVocabulary.GEOSPARQL.GEOF.RCC8PO.ToString()]   = RDFQueryEnums.RDFGeoRCC8Relations.RCC8PO,
                [RDFVocabulary.GEOSPARQL.GEOF.RCC8TPPI.ToString()] = RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPPI,
                [RDFVocabulary.GEOSPARQL.GEOF.RCC8TPP.ToString()]  = RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPP,
                [RDFVocabulary.GEOSPARQL.GEOF.RCC8NTPP.ToString()] = RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPP,
                [RDFVocabulary.GEOSPARQL.GEOF.RCC8NTPPI.ToString()]= RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPPI,
                [RDFVocabulary.GEOSPARQL.GEOF.RCC8EQ.ToString()]   = RDFQueryEnums.RDFGeoRCC8Relations.RCC8EQ
            };

        /// <summary>
        /// Units of measure accepted by geof:distance and geof:buffer, keyed by unit IRI: each one is the metre or a
        /// decimal multiple/submultiple of it, given as an exact integer scale and the direction of the scale.
        /// </summary>
        private static readonly Dictionary<string, (int Scale, bool IsSubmultiple)> GeoUnits =
            new Dictionary<string, (int Scale, bool IsSubmultiple)>
            {
                [RDFVocabulary.GEOSPARQL.UOM.METRE.ToString()]     = (1, false),
                [RDFVocabulary.GEOSPARQL.UNIT.M.ToString()]        = (1, false),
                [RDFVocabulary.GEOSPARQL.UNIT.DECA_M.ToString()]   = (10, false),
                [RDFVocabulary.GEOSPARQL.UNIT.HECTO_M.ToString()]  = (100, false),
                [RDFVocabulary.GEOSPARQL.UNIT.KILO_M.ToString()]   = (1000, false),
                [RDFVocabulary.GEOSPARQL.UNIT.DECI_M.ToString()]   = (10, true),
                [RDFVocabulary.GEOSPARQL.UNIT.CENTI_M.ToString()]  = (100, true),
                [RDFVocabulary.GEOSPARQL.UNIT.MILLI_M.ToString()]  = (1000, true),
                [RDFVocabulary.GEOSPARQL.UNIT.MICRO_M.ToString()]  = (1000000, true)
            };
        #endregion

        #region GeoFunctionCall
        /// <summary>
        /// Dispatches an IRI-named GeoSPARQL function call (the IRI and its already-parsed <paramref name="arguments"/>)
        /// onto the matching <see cref="RDFGeoExpression"/>, via the static lookup tables for the plain unary/binary and
        /// Egenhofer/RCC8 families, then the three argument-shaped functions (distance/buffer/relate) individually.
        /// </summary>
        /// <exception cref="RDFQueryException">When the function IRI is unknown or its arguments are malformed.</exception>
        private static RDFExpression DispatchGeoFunctionCall(RDFQueryParserContext parserContext, RDFResource functionIri, List<RDFExpression> arguments)
        {
            string functionIriString = functionIri.ToString();

            //Plain unary / binary topological / set functions
            if (GeoUnaryFunctions.TryGetValue(functionIriString, out Func<RDFExpression, RDFGeoExpression> unaryFunction))
            {
                RequireGeoArgumentCount(parserContext, arguments, 1, functionIri);
                return unaryFunction(arguments[0]);
            }
            if (GeoBinaryFunctions.TryGetValue(functionIriString, out Func<RDFExpression, RDFExpression, RDFGeoExpression> binaryFunction))
            {
                RequireGeoArgumentCount(parserContext, arguments, 2, functionIri);
                return binaryFunction(arguments[0], arguments[1]);
            }

            //Egenhofer / RCC8 relation families (binary, parameterised by a relation enum)
            if (GeoEgenhoferFunctions.TryGetValue(functionIriString, out RDFQueryEnums.RDFGeoEgenhoferRelations egenhoferRelation))
            {
                RequireGeoArgumentCount(parserContext, arguments, 2, functionIri);
                return new RDFGeoEgenhoferExpression(arguments[0], arguments[1], egenhoferRelation);
            }
            if (GeoRCC8Functions.TryGetValue(functionIriString, out RDFQueryEnums.RDFGeoRCC8Relations rcc8Relation))
            {
                RequireGeoArgumentCount(parserContext, arguments, 2, functionIri);
                return new RDFGeoRCC8Expression(arguments[0], arguments[1], rcc8Relation);
            }

            //Argument-shaped functions: each has bespoke argument handling (unit IRI, numeric radius, DE-9IM matrix)
            if (functionIriString == RDFVocabulary.GEOSPARQL.GEOF.DISTANCE.ToString()) return BuildGeoDistanceExpression(parserContext, arguments);
            if (functionIriString == RDFVocabulary.GEOSPARQL.GEOF.BUFFER.ToString())   return BuildGeoBufferExpression(parserContext, arguments);
            if (functionIriString == RDFVocabulary.GEOSPARQL.GEOF.RELATE.ToString())   return BuildGeoRelateExpression(parserContext, arguments);

            throw new RDFQueryException("Cannot parse SPARQL function call: the function '" + functionIri + "' is not supported " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Builds a <see cref="RDFGeoDistanceExpression"/> from <c>geof:distance(g1, g2, unit)</c>: the engine computes in metres so a unit other than the metre wraps the distance in a division or a multiplication.
        /// </summary>
        /// <exception cref="RDFQueryException">When the unit is not the metre or one of its decimal multiples/submultiples.</exception>
        private static RDFExpression BuildGeoDistanceExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            RequireGeoArgumentCount(parserContext, arguments, 3, RDFVocabulary.GEOSPARQL.GEOF.DISTANCE);

            //The unit is validated at parse time so that an unsupported one never reaches the evaluation
            (int Scale, bool IsSubmultiple) unit = ReadGeoUnit(parserContext, arguments[2], RDFVocabulary.GEOSPARQL.GEOF.DISTANCE);

            //The engine measures the distance in metres
            RDFGeoDistanceExpression distanceInMetres = new RDFGeoDistanceExpression(arguments[0], arguments[1]);

            //The metre needs no conversion
            if (unit.Scale == 1)
                return distanceInMetres;

            //The query asked for the distance in its own unit: the integer scale keeps the conversion exact
            RDFTypedLiteral unitScale = new RDFTypedLiteral(unit.Scale.ToString(CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_INTEGER);

            //A unit smaller than the metre needs more of itself to cover the same distance while a larger one needs less
            return unit.IsSubmultiple
                ? (RDFExpression)new RDFMultiplyExpression(distanceInMetres, unitScale)
                : new RDFDivideExpression(distanceInMetres, unitScale);
        }

        /// <summary>
        /// Builds a <see cref="RDFGeoBufferExpression"/> from <c>geof:buffer(g, radius [, unit])</c>: the engine buffers in metres so the radius is converted from the (optional) unit, at parse time when it is a constant numeric literal and per row otherwise.
        /// </summary>
        /// <exception cref="RDFQueryException">When the unit is not the metre or one of its decimal multiples/submultiples.</exception>
        private static RDFExpression BuildGeoBufferExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            if (arguments.Count < 2 || arguments.Count > 3)
                throw new RDFQueryException("Cannot parse SPARQL function '" + RDFVocabulary.GEOSPARQL.GEOF.BUFFER + "': expected 2 or 3 argument(s) but found " + arguments.Count + " " + GetCoordinates(parserContext));

            //Without the unit argument the radius is already in metres
            (int Scale, bool IsSubmultiple) unit = arguments.Count == 3
                ? ReadGeoUnit(parserContext, arguments[2], RDFVocabulary.GEOSPARQL.GEOF.BUFFER)
                : (1, false);

            //Fixed radius: convert it to metres now and store it as a double
            if (TryGetDoubleLiteral(arguments[1], out double radius))
                return new RDFGeoBufferExpression(arguments[0], unit.IsSubmultiple ? radius / unit.Scale : radius * unit.Scale);

            //Dynamic radius in metres: keep it as a per-row expression
            if (unit.Scale == 1)
                return new RDFGeoBufferExpression(arguments[0], arguments[1]);

            //Dynamic radius in another unit: the integer scale keeps the per-row conversion to metres exact
            RDFTypedLiteral unitScale = new RDFTypedLiteral(unit.Scale.ToString(CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_INTEGER);

            //A unit smaller than the metre covers less ground per unit while a larger one covers more
            return new RDFGeoBufferExpression(arguments[0], unit.IsSubmultiple
                ? (RDFExpression)new RDFDivideExpression(arguments[1], unitScale)
                : new RDFMultiplyExpression(arguments[1], unitScale));
        }

        /// <summary>
        /// Reads the unit argument of a geo function into the scale that relates it to the metre.
        /// </summary>
        /// <exception cref="RDFQueryException">When the argument is not a constant IRI of the metre or of one of its decimal multiples/submultiples.</exception>
        private static (int Scale, bool IsSubmultiple) ReadGeoUnit(RDFQueryParserContext parserContext, RDFExpression unitArgument, RDFResource functionIri)
        {
            //Only a constant IRI of a known unit is accepted: variables and literals cannot be converted at parse time
            if (unitArgument is RDFConstantExpression constantExpression
                 && constantExpression.LeftArgument is RDFResource unitIri
                 && GeoUnits.TryGetValue(unitIri.ToString(), out (int Scale, bool IsSubmultiple) unit))
                return unit;

            throw new RDFQueryException("Cannot parse SPARQL function '" + functionIri + "': the unit of measure '" + unitArgument + "' is not supported, only the metre and its decimal multiples and submultiples are supported " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Builds a <see cref="RDFGeoRelateExpression"/> from <c>geof:relate(g1, g2, deMatrix)</c>, where the third
        /// argument is the DE-9IM intersection-pattern string literal.
        /// </summary>
        private static RDFExpression BuildGeoRelateExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            RequireGeoArgumentCount(parserContext, arguments, 3, RDFVocabulary.GEOSPARQL.GEOF.RELATE);
            return new RDFGeoRelateExpression(arguments[0], arguments[1], RequireLiteralString(parserContext, arguments[2], "geof:relate DE-9IM matrix"));
        }

        /// <summary>
        /// Validates that a geo function received exactly <paramref name="expectedCount"/> arguments.
        /// </summary>
        private static void RequireGeoArgumentCount(RDFQueryParserContext parserContext, List<RDFExpression> arguments, int expectedCount, RDFResource functionIri)
        {
            if (arguments.Count != expectedCount)
                throw new RDFQueryException("Cannot parse SPARQL function '" + functionIri + "': expected " + expectedCount + " argument(s) but found " + arguments.Count + " " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Reports whether the argument is a constant numeric literal and, if so, yields its value. Used to decide
        /// between the build-time (constant) and per-row (dynamic) shapes of the geo buffer radius.
        /// </summary>
        private static bool TryGetDoubleLiteral(RDFExpression argument, out double doubleValue)
        {
            if (argument is RDFConstantExpression constantExpression
                 && constantExpression.LeftArgument is RDFTypedLiteral typedLiteral
                 && double.TryParse(typedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleValue))
                return true;
            doubleValue = 0;
            return false;
        }
        #endregion
    }
}