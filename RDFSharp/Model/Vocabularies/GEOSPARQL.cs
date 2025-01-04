/*
   Copyright 2012-2025 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License"));
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies
    /// </summary>
    public static partial class RDFVocabulary
    {
        #region GEOSPARQL
        /// <summary>
        /// GEOSPARQL represents the OGC GeoSPARQL 1.0 vocabulary (with SF and GEOF extensions)
        /// </summary>
        public static class GEOSPARQL
        {
            #region Properties
            /// <summary>
            /// geosparql
            /// </summary>
            public static readonly string PREFIX = "geosparql";

            /// <summary>
            /// http://www.opengis.net/ont/geosparql#
            /// </summary>
            public static readonly string BASE_URI = "http://www.opengis.net/ont/geosparql#";

            /// <summary>
            /// http://www.opengis.net/ont/geosparql#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.opengis.net/ont/geosparql#";

            /// <summary>
            /// geosparql:wktLiteral
            /// </summary>
            public static readonly RDFResource WKT_LITERAL = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "wktLiteral"));

            /// <summary>
            /// geosparql:gmlLiteral
            /// </summary>
            public static readonly RDFResource GML_LITERAL = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "gmlLiteral"));

            /// <summary>
            /// geosparql:SpatialObject
            /// </summary>
            public static readonly RDFResource SPATIAL_OBJECT = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "SpatialObject"));

            /// <summary>
            /// geosparql:Geometry
            /// </summary>
            public static readonly RDFResource GEOMETRY = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "Geometry"));

            /// <summary>
            /// geosparql:Feature
            /// </summary>
            public static readonly RDFResource FEATURE = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "Feature"));

            /// <summary>
            /// geosparql:defaultGeometry
            /// </summary>
            public static readonly RDFResource DEFAULT_GEOMETRY = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "defaultGeometry"));

            /// <summary>
            /// geosparql:ehDisjoint
            /// </summary>
            public static readonly RDFResource EH_DISJOINT = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "ehDisjoint"));

            /// <summary>
            /// geosparql:rcc8ntpp
            /// </summary>
            public static readonly RDFResource RCC8NTPP = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "rcc8ntpp"));

            /// <summary>
            /// geosparql:ehContains
            /// </summary>
            public static readonly RDFResource EH_CONTAINS = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "ehContains"));

            /// <summary>
            /// geosparql:rcc8tppi
            /// </summary>
            public static readonly RDFResource RCC8TPPI = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "rcc8tppi"));

            /// <summary>
            /// geosparql:rcc8ec
            /// </summary>
            public static readonly RDFResource RCC8EC = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "rcc8ec"));

            /// <summary>
            /// geosparql:sfEquals
            /// </summary>
            public static readonly RDFResource SF_EQUALS = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "sfEquals"));

            /// <summary>
            /// geosparql:ehOverlap
            /// </summary>
            public static readonly RDFResource EH_OVERLAP = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "ehOverlap"));

            /// <summary>
            /// geosparql:hasGeometry
            /// </summary>
            public static readonly RDFResource HAS_GEOMETRY = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "hasGeometry"));

            /// <summary>
            /// geosparql:rcc8dc
            /// </summary>
            public static readonly RDFResource RCC8DC = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "rcc8dc"));

            /// <summary>
            /// geosparql:ehCovers
            /// </summary>
            public static readonly RDFResource EH_COVERS = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "ehCovers"));

            /// <summary>
            /// geosparql:ehCoveredBy
            /// </summary>
            public static readonly RDFResource EH_COVERED_BY = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "ehCoveredBy"));

            /// <summary>
            /// geosparql:sfContains
            /// </summary>
            public static readonly RDFResource SF_CONTAINS = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "sfContains"));

            /// <summary>
            /// geosparql:rcc8tpp
            /// </summary>
            public static readonly RDFResource RCC8TPP = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "rcc8tpp"));

            /// <summary>
            /// geosparql:ehEquals
            /// </summary>
            public static readonly RDFResource EH_EQUALS = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "ehEquals"));

            /// <summary>
            /// geosparql:rcc8po
            /// </summary>
            public static readonly RDFResource RCC8PO = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "rcc8po"));

            /// <summary>
            /// geosparql:sfOverlaps
            /// </summary>
            public static readonly RDFResource SF_OVERLAPS = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "sfOverlaps"));

            /// <summary>
            /// geosparql:sfWithin
            /// </summary>
            public static readonly RDFResource SF_WITHIN = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "sfWithin"));

            /// <summary>
            /// geosparql:sfTouches
            /// </summary>
            public static readonly RDFResource SF_TOUCHES = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "sfTouches"));

            /// <summary>
            /// geosparql:sfIntersects
            /// </summary>
            public static readonly RDFResource SF_INTERSECTS = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "sfIntersects"));

            /// <summary>
            /// geosparql:sfCrosses
            /// </summary>
            public static readonly RDFResource SF_CROSSES = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "sfCrosses"));

            /// <summary>
            /// geosparql:rcc8ntppi
            /// </summary>
            public static readonly RDFResource RCC8NTPPI = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "rcc8ntppi"));

            /// <summary>
            /// geosparql:rcc8eq
            /// </summary>
            public static readonly RDFResource RCC8EQ = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "rcc8eq"));

            /// <summary>
            /// geosparql:ehMeet
            /// </summary>
            public static readonly RDFResource EH_MEET = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "ehMeet"));

            /// <summary>
            /// geosparql:sfDisjoint
            /// </summary>
            public static readonly RDFResource SF_DISJOINT = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "sfDisjoint"));

            /// <summary>
            /// geosparql:ehInside
            /// </summary>
            public static readonly RDFResource EH_INSIDE = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "ehInside"));

            /// <summary>
            /// geosparql:spatialDimension
            /// </summary>
            public static readonly RDFResource SPATIAL_DIMENSION = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "spatialDimension"));

            /// <summary>
            /// geosparql:isEmpty
            /// </summary>
            public static readonly RDFResource IS_EMPTY = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "isEmpty"));

            /// <summary>
            /// geosparql:coordinateDimension
            /// </summary>
            public static readonly RDFResource COORDINATE_DIMENSION = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "coordinateDimension"));

            /// <summary>
            /// geosparql:asWKT
            /// </summary>
            public static readonly RDFResource AS_WKT = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "asWKT"));

            /// <summary>
            /// geosparql:asGML
            /// </summary>
            public static readonly RDFResource AS_GML = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "asGML"));

            /// <summary>
            /// geosparql:isSimple
            /// </summary>
            public static readonly RDFResource IS_SIMPLE = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "isSimple"));

            /// <summary>
            /// geosparql:dimension
            /// </summary>
            public static readonly RDFResource DIMENSION = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "dimension"));

            /// <summary>
            /// geosparql:hasSerialization
            /// </summary>
            public static readonly RDFResource HAS_SERIALIZATION = new RDFResource(string.Concat(GEOSPARQL.BASE_URI, "hasSerialization"));
            #endregion

            #region Extended Properties
            /// <summary>
            /// Simple Features extensions
            /// </summary>
            public static class SF
            {
                /// <summary>
                /// sf
                /// </summary>
                public static readonly string PREFIX = "sf";

                /// <summary>
                /// http://www.opengis.net/ont/sf#
                /// </summary>
                public static readonly string BASE_URI = "http://www.opengis.net/ont/sf#";

                /// <summary>
                /// http://www.opengis.net/ont/sf#
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://www.opengis.net/ont/sf#";

                /// <summary>
                /// sf:Point
                /// </summary>
                public static readonly RDFResource POINT = new RDFResource(string.Concat(SF.BASE_URI, "Point"));

                /// <summary>
                /// sf:Curve
                /// </summary>
                public static readonly RDFResource CURVE = new RDFResource(string.Concat(SF.BASE_URI, "Curve"));

                /// <summary>
                /// sf:Surface
                /// </summary>
                public static readonly RDFResource SURFACE = new RDFResource(string.Concat(SF.BASE_URI, "Surface"));

                /// <summary>
                /// sf:Polygon
                /// </summary>
                public static readonly RDFResource POLYGON = new RDFResource(string.Concat(SF.BASE_URI, "Polygon"));

                /// <summary>
                /// sf:Triangle
                /// </summary>
                public static readonly RDFResource TRIANGLE = new RDFResource(string.Concat(SF.BASE_URI, "Triangle"));

                /// <summary>
                /// sf:LineString
                /// </summary>
                public static readonly RDFResource LINESTRING = new RDFResource(string.Concat(SF.BASE_URI, "LineString"));

                /// <summary>
                /// sf:LinearRing
                /// </summary>
                public static readonly RDFResource LINEAR_RING = new RDFResource(string.Concat(SF.BASE_URI, "LinearRing"));

                /// <summary>
                /// sf:Line
                /// </summary>
                public static readonly RDFResource LINE = new RDFResource(string.Concat(SF.BASE_URI, "Line"));

                /// <summary>
                /// sf:GeometryCollection
                /// </summary>
                public static readonly RDFResource GEOMETRY_COLLECTION = new RDFResource(string.Concat(SF.BASE_URI, "GeometryCollection"));

                /// <summary>
                /// sf:MultiPoint
                /// </summary>
                public static readonly RDFResource MULTI_POINT = new RDFResource(string.Concat(SF.BASE_URI, "MultiPoint"));

                /// <summary>
                /// sf:MultiCurve
                /// </summary>
                public static readonly RDFResource MULTI_CURVE = new RDFResource(string.Concat(SF.BASE_URI, "MultiCurve"));

                /// <summary>
                /// sf:MultiSurface
                /// </summary>
                public static readonly RDFResource MULTI_SURFACE = new RDFResource(string.Concat(SF.BASE_URI, "MultiSurface"));

                /// <summary>
                /// sf:MultiPolygon
                /// </summary>
                public static readonly RDFResource MULTI_POLYGON = new RDFResource(string.Concat(SF.BASE_URI, "MultiPolygon"));

                /// <summary>
                /// sf:MultiLineString
                /// </summary>
                public static readonly RDFResource MULTI_LINESTRING = new RDFResource(string.Concat(SF.BASE_URI, "MultiLineString"));

                /// <summary>
                /// sf:PolyhedralSurface
                /// </summary>
                public static readonly RDFResource POLYHEDRAL_SURFACE = new RDFResource(string.Concat(SF.BASE_URI, "PolyhedralSurface"));

                /// <summary>
                /// sf:TIN
                /// </summary>
                public static readonly RDFResource TIN = new RDFResource(string.Concat(SF.BASE_URI, "TIN"));
            }
            
            /// <summary>
            /// Functions extensions
            /// </summary>
            public static class GEOF
            {
                /// <summary>
                /// geof
                /// </summary>
                public static readonly string PREFIX = "geof";

                /// <summary>
                /// http://www.opengis.net/def/function/geosparql/
                /// </summary>
                public static readonly string BASE_URI = "http://www.opengis.net/def/function/geosparql/";

                /// <summary>
                /// http://www.opengis.net/def/function/geosparql/
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://www.opengis.net/def/function/geosparql/";

                /// <summary>
                /// geof:boundary
                /// </summary>
                public static readonly RDFResource BOUNDARY = new RDFResource(string.Concat(GEOF.BASE_URI, "boundary"));

                /// <summary>
                /// geof:buffer
                /// </summary>
                public static readonly RDFResource BUFFER = new RDFResource(string.Concat(GEOF.BASE_URI, "buffer"));

                /// <summary>
                /// geof:ehContains
                /// </summary>
                public static readonly RDFResource EH_CONTAINS = new RDFResource(string.Concat(GEOF.BASE_URI, "ehContains"));

                /// <summary>
                /// geof:sfContains
                /// </summary>
                public static readonly RDFResource SF_CONTAINS = new RDFResource(string.Concat(GEOF.BASE_URI, "sfContains"));

                /// <summary>
                /// geof:convexHull
                /// </summary>
                public static readonly RDFResource CONVEX_HULL = new RDFResource(string.Concat(GEOF.BASE_URI, "convexHull"));

                /// <summary>
                /// geof:ehCoveredBy
                /// </summary>
                public static readonly RDFResource EH_COVERED_BY = new RDFResource(string.Concat(GEOF.BASE_URI, "ehCoveredBy"));

                /// <summary>
                /// geof:ehCovers
                /// </summary>
                public static readonly RDFResource EH_COVERS = new RDFResource(string.Concat(GEOF.BASE_URI, "ehCovers"));

                /// <summary>
                /// geof:sfCrosses
                /// </summary>
                public static readonly RDFResource SF_CROSSES = new RDFResource(string.Concat(GEOF.BASE_URI, "sfCrosses"));

                /// <summary>
                /// geof:difference
                /// </summary>
                public static readonly RDFResource DIFFERENCE = new RDFResource(string.Concat(GEOF.BASE_URI, "difference"));

                /// <summary>
                /// geof:rcc8dc
                /// </summary>
                public static readonly RDFResource RCC8DC = new RDFResource(string.Concat(GEOF.BASE_URI, "rcc8dc"));

                /// <summary>
                /// geof:ehDisjoint
                /// </summary>
                public static readonly RDFResource EH_DISJOINT = new RDFResource(string.Concat(GEOF.BASE_URI, "ehDisjoint"));

                /// <summary>
                /// geof:sfDisjoint
                /// </summary>
                public static readonly RDFResource SF_DISJOINT = new RDFResource(string.Concat(GEOF.BASE_URI, "sfDisjoint"));

                /// <summary>
                /// geof:distance
                /// </summary>
                public static readonly RDFResource DISTANCE = new RDFResource(string.Concat(GEOF.BASE_URI, "distance"));

                /// <summary>
                /// geof:envelope
                /// </summary>
                public static readonly RDFResource ENVELOPE = new RDFResource(string.Concat(GEOF.BASE_URI, "envelope"));

                /// <summary>
                /// geof:ehEquals
                /// </summary>
                public static readonly RDFResource EH_EQUALS = new RDFResource(string.Concat(GEOF.BASE_URI, "ehEquals"));

                /// <summary>
                /// geof:sfEquals
                /// </summary>
                public static readonly RDFResource SF_EQUALS = new RDFResource(string.Concat(GEOF.BASE_URI, "sfEquals"));

                /// <summary>
                /// geof:rcc8eq
                /// </summary>
                public static readonly RDFResource RCC8EQ = new RDFResource(string.Concat(GEOF.BASE_URI, "rcc8eq"));

                /// <summary>
                /// geof:rcc8ec
                /// </summary>
                public static readonly RDFResource RCC8EC = new RDFResource(string.Concat(GEOF.BASE_URI, "rcc8ec"));

                /// <summary>
                /// geof:getSRID
                /// </summary>
                public static readonly RDFResource GET_SRID = new RDFResource(string.Concat(GEOF.BASE_URI, "getSRID"));

                /// <summary>
                /// geof:ehInside
                /// </summary>
                public static readonly RDFResource EH_INSIDE = new RDFResource(string.Concat(GEOF.BASE_URI, "ehInside"));

                /// <summary>
                /// geof:intersection
                /// </summary>
                public static readonly RDFResource INTERSECTION = new RDFResource(string.Concat(GEOF.BASE_URI, "intersection"));

                /// <summary>
                /// geof:sfIntersects
                /// </summary>
                public static readonly RDFResource SF_INTERSECTS = new RDFResource(string.Concat(GEOF.BASE_URI, "sfIntersects"));

                /// <summary>
                /// geof:ehMeet
                /// </summary>
                public static readonly RDFResource EH_MEET = new RDFResource(string.Concat(GEOF.BASE_URI, "ehMeet"));

                /// <summary>
                /// geof:rcc8ntpp
                /// </summary>
                public static readonly RDFResource RCC8NTPP = new RDFResource(string.Concat(GEOF.BASE_URI, "rcc8ntpp"));

                /// <summary>
                /// geof:rcc8ntppi
                /// </summary>
                public static readonly RDFResource RCC8NTPPI = new RDFResource(string.Concat(GEOF.BASE_URI, "rcc8ntppi"));

                /// <summary>
                /// geof:ehOverlap
                /// </summary>
                public static readonly RDFResource EH_OVERLAP = new RDFResource(string.Concat(GEOF.BASE_URI, "ehOverlap"));

                /// <summary>
                /// geof:sfOverlaps
                /// </summary>
                public static readonly RDFResource SF_OVERLAPS = new RDFResource(string.Concat(GEOF.BASE_URI, "sfOverlaps"));

                /// <summary>
                /// geof:rcc8po
                /// </summary>
                public static readonly RDFResource RCC8PO = new RDFResource(string.Concat(GEOF.BASE_URI, "rcc8po"));

                /// <summary>
                /// geof:relate
                /// </summary>
                public static readonly RDFResource RELATE = new RDFResource(string.Concat(GEOF.BASE_URI, "relate"));

                /// <summary>
                /// geof:symDifference
                /// </summary>
                public static readonly RDFResource SYM_DIFFERENCE = new RDFResource(string.Concat(GEOF.BASE_URI, "symDifference"));

                /// <summary>
                /// geof:rcc8tpp
                /// </summary>
                public static readonly RDFResource RCC8TPP = new RDFResource(string.Concat(GEOF.BASE_URI, "rcc8tpp"));

                /// <summary>
                /// geof:rcc8tppi
                /// </summary>
                public static readonly RDFResource RCC8TPPI = new RDFResource(string.Concat(GEOF.BASE_URI, "rcc8tppi"));

                /// <summary>
                /// geof:sfTouches
                /// </summary>
                public static readonly RDFResource SF_TOUCHES = new RDFResource(string.Concat(GEOF.BASE_URI, "sfTouches"));

                /// <summary>
                /// geof:union
                /// </summary>
                public static readonly RDFResource SF_UNION = new RDFResource(string.Concat(GEOF.BASE_URI, "union"));

                /// <summary>
                /// geof:sfWithin
                /// </summary>
                public static readonly RDFResource SF_WITHIN = new RDFResource(string.Concat(GEOF.BASE_URI, "sfWithin"));
            }
            #endregion
        }
        #endregion
    }
}