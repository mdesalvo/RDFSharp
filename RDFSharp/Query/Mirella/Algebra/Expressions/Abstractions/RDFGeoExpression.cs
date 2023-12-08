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

using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GML2;
using NetTopologySuite.IO;
using RDFSharp.Model;
using System;
using System.Data;
using System.Globalization;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFGeoExpression represents a geographic expression to be applied on a query results table.
    /// </summary>
    public abstract class RDFGeoExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// Answers if this is a geographic expression using right argument
        /// </summary>
        protected bool HasRightArgument => RightArgument != null;

        /// <summary>
        /// Reader for WKT spatial representation
        /// </summary>
        internal static WKTReader WKTReader = new WKTReader();

        /// <summary>
        /// Writer for WKT spatial representation
        /// </summary>
        internal static WKTWriter WKTWriter = new WKTWriter();

        /// <summary>
        /// Reader for GML spatial representation
        /// </summary>
        internal static GMLReader GMLReader = new GMLReader();

        /// <summary>
        /// Writer for GML spatial representation
        /// </summary>
        internal static GMLWriter GMLWriter = new GMLWriter();
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a geographic expression with given arguments
        /// </summary>
        public RDFGeoExpression(RDFExpressionArgument leftArgument, RDFExpressionArgument rightArgument)
            : base(leftArgument, rightArgument) { }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the geographic expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFTypedLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
                return expressionResult;
            if (RightArgument is RDFVariable && !row.Table.Columns.Contains(RightArgument.ToString()))
                return expressionResult;
            #endregion

            try
            {
                #region Evaluate Arguments
                //Evaluate left argument (Expression VS Variable)
                RDFPatternMember leftArgumentPMember = null;
                if (LeftArgument is RDFExpression leftArgumentExpression)
                    leftArgumentPMember = leftArgumentExpression.ApplyExpression(row);
                else
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[LeftArgument.ToString()].ToString());

                //Evaluate right argument (Expression VS Variable VS TypedLiteral)
                RDFPatternMember rightArgumentPMember = null;
                if (RightArgument is RDFExpression rightArgumentExpression)
                    rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                else if (RightArgument is RDFVariable)
                    rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[RightArgument.ToString()].ToString());
                else
                    rightArgumentPMember = (RDFTypedLiteral)RightArgument;
                #endregion

                #region Calculate Result
                Geometry leftGeometry = null;
                Geometry leftGeometryLAZ = null;
                Geometry rightGeometry = null;
                Geometry rightGeometryLAZ = null;
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && (leftArgumentTypedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT) || 
                          leftArgumentTypedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)))
                {
                    //Parse WGS84 WKT/GML left geometry
                    leftGeometry = leftArgumentTypedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT) ?
                        WKTReader.Read(leftArgumentTypedLiteral.Value) : GMLReader.Read(leftArgumentTypedLiteral.Value);
                    leftGeometry.SRID = 4326;

                    //Short-circuit empty geometry evaluation
                    if (this is RDFGeoIsEmptyExpression)
                        return leftGeometry.IsEmpty ? RDFTypedLiteral.True : RDFTypedLiteral.False;

                    //Project left geometry from WGS84 to Lambert Azimuthal
                    leftGeometryLAZ = RDFGeoConverter.GetLambertAzimuthalGeometryFromWGS84(leftGeometry);

                    //Determine if requested GEO function needs right geometry
                    if (HasRightArgument)
                    {
                        //If so, it must be a well-formed GEO literal (WKT/GML)
                        if (rightArgumentPMember is RDFTypedLiteral rightArgumentTypedLiteral
                             && (rightArgumentTypedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT) 
                                  || rightArgumentTypedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)))
                        {
                            //Parse WGS84 WKT/GML right geometry
                            rightGeometry = rightArgumentTypedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT) ?
                                WKTReader.Read(rightArgumentTypedLiteral.Value) : GMLReader.Read(rightArgumentTypedLiteral.Value);
                            rightGeometry.SRID = 4326;

                            //Project right geometry from WGS84 to Lambert Azimuthal
                            rightGeometryLAZ = RDFGeoConverter.GetLambertAzimuthalGeometryFromWGS84(rightGeometry);
                        }

                        //Otherwise, return null to signal binding error
                        else
                            return expressionResult;
                    }

                    //Execute GEO functions on LAZ geometries
                    if (this is RDFGeoBoundaryExpression)
                    {
                        Geometry boundaryGeometryLAZ = leftGeometryLAZ.Boundary;
                        Geometry boundaryGeometryWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(boundaryGeometryLAZ);
                        string wktBoundaryGeometryWGS84 = WKTWriter.Write(boundaryGeometryWGS84)
                                                                   .Replace("LINEARRING", "LINESTRING");
                        expressionResult = new RDFTypedLiteral(wktBoundaryGeometryWGS84, RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
                    }
                    else if (this is RDFGeoBufferExpression geoBufferExpression)
                    {
                        Geometry bufferGeometryLAZ = leftGeometryLAZ.Buffer(geoBufferExpression.BufferMeters);
                        Geometry bufferGeometryWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(bufferGeometryLAZ);
                        expressionResult = new RDFTypedLiteral(WKTWriter.Write(bufferGeometryWGS84), RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
                    }
                    else if (this is RDFGeoContainsExpression)
                    {
                        expressionResult = leftGeometryLAZ.Contains(rightGeometryLAZ) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoConvexHullExpression)
                    {
                        Geometry convexHullGeometryLAZ = leftGeometryLAZ.ConvexHull();
                        Geometry convexHullGeometryWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(convexHullGeometryLAZ);
                        expressionResult = new RDFTypedLiteral(WKTWriter.Write(convexHullGeometryWGS84), RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
                    }
                    else if (this is RDFGeoCrossesExpression)
                    {
                        expressionResult = leftGeometryLAZ.Crosses(rightGeometryLAZ) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }   
                    else if (this is RDFGeoDifferenceExpression)
                    {
                        Geometry differenceGeometryLAZ = leftGeometryLAZ.Difference(rightGeometryLAZ);
                        Geometry differenceGeometryWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(differenceGeometryLAZ);
                        expressionResult = new RDFTypedLiteral(WKTWriter.Write(differenceGeometryWGS84), RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
                    }
                    else if (this is RDFGeoDimensionExpression)
                    {
                        expressionResult = new RDFTypedLiteral(Convert.ToString((int)leftGeometryLAZ.Dimension, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_INTEGER);
                    }
                    else if (this is RDFGeoDisjointExpression)
                    {
                        expressionResult = leftGeometryLAZ.Disjoint(rightGeometryLAZ) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoDistanceExpression)
                    {
                        expressionResult = new RDFTypedLiteral(Convert.ToString(leftGeometryLAZ.Distance(rightGeometryLAZ), CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                    }
                    else if (this is RDFGeoEgenhoferExpression geoEgenhoferExpression)
                    {
                        bool sfEgenhoferRelate = false;
                        switch (geoEgenhoferExpression.EgenhoferRelation)
                        {
                            case RDFQueryEnums.RDFGeoEgenhoferRelations.Contains:
                                sfEgenhoferRelate = leftGeometryLAZ.Relate(rightGeometryLAZ, "T*TFF*FF*");
                                break;
                            case RDFQueryEnums.RDFGeoEgenhoferRelations.CoveredBy:
                                sfEgenhoferRelate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TFF*TFT**");
                                break;
                            case RDFQueryEnums.RDFGeoEgenhoferRelations.Covers:
                                sfEgenhoferRelate = leftGeometryLAZ.Relate(rightGeometryLAZ, "T*TFT*FF*");
                                break;
                            case RDFQueryEnums.RDFGeoEgenhoferRelations.Disjoint:
                                sfEgenhoferRelate = leftGeometryLAZ.Relate(rightGeometryLAZ, "FF*FF****");
                                break;
                            case RDFQueryEnums.RDFGeoEgenhoferRelations.Equals:
                                sfEgenhoferRelate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TFFFTFFFT");
                                break;
                            case RDFQueryEnums.RDFGeoEgenhoferRelations.Inside:
                                sfEgenhoferRelate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TFF*FFT**");
                                break;
                            case RDFQueryEnums.RDFGeoEgenhoferRelations.Meet:
                                sfEgenhoferRelate = leftGeometryLAZ.Relate(rightGeometryLAZ, "FT*******")
                                                     || leftGeometryLAZ.Relate(rightGeometryLAZ, "F**T*****")
                                                      || leftGeometryLAZ.Relate(rightGeometryLAZ, "F***T****");
                                break;
                            case RDFQueryEnums.RDFGeoEgenhoferRelations.Overlap:
                                sfEgenhoferRelate = leftGeometryLAZ.Relate(rightGeometryLAZ, "T*T***T**");
                                break;
                        }
                        expressionResult = sfEgenhoferRelate ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoEnvelopeExpression)
                    {
                        Geometry envelopeGeometryLAZ = leftGeometryLAZ.Envelope;
                        Geometry envelopeGeometryWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(envelopeGeometryLAZ);
                        expressionResult = new RDFTypedLiteral(WKTWriter.Write(envelopeGeometryWGS84), RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
                    }
                    else if (this is RDFGeoEqualsExpression)
                    {
                        expressionResult = leftGeometryLAZ.EqualsTopologically(rightGeometryLAZ) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoGetSRIDExpression)
                    {
                        expressionResult = new RDFTypedLiteral($"http://www.opengis.net/def/crs/EPSG/0/{leftGeometry.SRID}", RDFModelEnums.RDFDatatypes.XSD_ANYURI);
                    }
                    else if (this is RDFGeoIntersectionExpression)
                    {
                        Geometry intersectionGeometryLAZ = leftGeometryLAZ.Intersection(rightGeometryLAZ);
                        Geometry intersectionGeometryWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(intersectionGeometryLAZ);
                        expressionResult = new RDFTypedLiteral(WKTWriter.Write(intersectionGeometryWGS84), RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
                    }
                    else if (this is RDFGeoIntersectsExpression)
                    {
                        expressionResult = leftGeometryLAZ.Intersects(rightGeometryLAZ) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoIsSimpleExpression)
                    {
                        expressionResult = leftGeometryLAZ.IsSimple ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoOverlapsExpression)
                    {
                        expressionResult = leftGeometryLAZ.Overlaps(rightGeometryLAZ) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoRCC8Expression geoRCC8Expression)
                    {
                        bool sfRCSS8Relate = false;
                        switch (geoRCC8Expression.RCC8Relation)
                        {
                            case RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC:
                                sfRCSS8Relate = leftGeometryLAZ.Relate(rightGeometryLAZ, "FFTFFTTTT");
                                break;
                            case RDFQueryEnums.RDFGeoRCC8Relations.RCC8EC:
                                sfRCSS8Relate = leftGeometryLAZ.Relate(rightGeometryLAZ, "FFTFTTTTT");
                                break;
                            case RDFQueryEnums.RDFGeoRCC8Relations.RCC8EQ:
                                sfRCSS8Relate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TFFFTFFFT");
                                break;
                            case RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPP:
                                sfRCSS8Relate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TFFTFFTTT");
                                break;
                            case RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPPI:
                                sfRCSS8Relate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TTTFFTFFT");
                                break;
                            case RDFQueryEnums.RDFGeoRCC8Relations.RCC8PO:
                                sfRCSS8Relate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TTTTTTTTT");
                                break;
                            case RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPP:
                                sfRCSS8Relate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TFFTTFTTT");
                                break;
                            case RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPPI:
                                sfRCSS8Relate = leftGeometryLAZ.Relate(rightGeometryLAZ, "TTTFTTFFT");
                                break;
                        }
                        expressionResult = sfRCSS8Relate ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoRelateExpression geoRelateExpression)
                    {
                        expressionResult = leftGeometryLAZ.Relate(rightGeometryLAZ, geoRelateExpression.DE9IMRelation) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoSymDifferenceExpression)
                    {
                        Geometry symDifferenceGeometryLAZ = leftGeometryLAZ.SymmetricDifference(rightGeometryLAZ);
                        Geometry symDifferenceGeometryWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(symDifferenceGeometryLAZ);
                        expressionResult = new RDFTypedLiteral(WKTWriter.Write(symDifferenceGeometryWGS84), RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
                    }
                    else if (this is RDFGeoTouchesExpression)
                    {
                        expressionResult = leftGeometryLAZ.Touches(rightGeometryLAZ) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                    else if (this is RDFGeoUnionExpression)
                    {
                        Geometry unionGeometryLAZ = leftGeometryLAZ.Union(rightGeometryLAZ);
                        Geometry unionGeometryWGS84 = RDFGeoConverter.GetWGS84GeometryFromLambertAzimuthal(unionGeometryLAZ);
                        expressionResult = new RDFTypedLiteral(WKTWriter.Write(unionGeometryWGS84), RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);
                    }
                    else if (this is RDFGeoWithinExpression)
                    {
                        expressionResult = leftGeometryLAZ.Within(rightGeometryLAZ) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                    }
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }

    #region GEOConverter
    /// <summary>
    /// RDFGeoConverter is an helper for converting geometries between different coordinate systems
    /// </summary>
    internal static class RDFGeoConverter
    {
        #region Properties
        /// <summary>
        /// Projected system used when coordinates of geometries span across multiple UTM zones
        /// </summary>
        internal static CoordinateSystem LambertAzimutalWGS84 = new CoordinateSystemFactory().CreateFromWkt(
@"PROJCS[""WGS84 / Lambert Azim Mozambique"",
    GEOGCS[""WGS 84"",
        DATUM[""WGS_1984"",
            SPHEROID[""WGS_1984"",6378137.0,298.257223563]],
        PRIMEM[""Greenwich"",0.0],
        UNIT[""degree"",0.017453292519943295],
        AXIS[""Longitude"",EAST],
        AXIS[""Latitude"",NORTH]],
    PROJECTION[""Lambert_Azimuthal_Equal_Area""],
    PARAMETER[""latitude_of_center"",5.0],
    PARAMETER[""longitude_of_center"",20.0],
    PARAMETER[""false_easting"",0.0],
    PARAMETER[""false_northing"",0.0],
    UNIT[""m"",1.0],
    AXIS[""x"",EAST],
    AXIS[""y"",NORTH],
    AUTHORITY[""EPSG"",""42106""]]");
        #endregion

        #region Methods
        /// <summary>
        /// Projects the given WGS84 geometry to an equivalent Lambert Azimuthal geometry
        /// </summary>
        internal static Geometry GetLambertAzimuthalGeometryFromWGS84(Geometry wgs84Geometry)
        {
            ICoordinateTransformation coordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(
                GeographicCoordinateSystem.WGS84, LambertAzimutalWGS84);
            Geometry lazGeometry = TransformGeometry(wgs84Geometry, coordinateTransformation.MathTransform);
            lazGeometry.SRID = Convert.ToInt32(coordinateTransformation.TargetCS.AuthorityCode);
            return lazGeometry;
        }

        /// <summary>
        /// Projects the given Lambert Azimuthal geometry to an equivalent WGS84 geometry
        /// </summary>
        internal static Geometry GetWGS84GeometryFromLambertAzimuthal(Geometry projectedGeometry)
        {
            ICoordinateTransformation coordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(
                LambertAzimutalWGS84, GeographicCoordinateSystem.WGS84);
            Geometry wgs84Geometry = TransformGeometry(projectedGeometry, coordinateTransformation.MathTransform);
            wgs84Geometry.SRID = Convert.ToInt32(coordinateTransformation.TargetCS.AuthorityCode);
            return wgs84Geometry;
        }
        #endregion

        #region Utilities
        private static Geometry TransformGeometry(Geometry geometry, MathTransform mathTransform)
        {
            geometry = geometry.Copy();
            geometry.Apply(new MathTransformFilter(mathTransform));
            return geometry;
        }

        private sealed class MathTransformFilter : ICoordinateSequenceFilter
        {
            private readonly MathTransform _mathTransform;
            public MathTransformFilter(MathTransform mathTransform) => _mathTransform = mathTransform;
            public bool Done => false;
            public bool GeometryChanged => true;

            public void Filter(CoordinateSequence coordinateSequence, int index)
            {
                double x = coordinateSequence.GetX(index);
                double y = coordinateSequence.GetY(index);
                double z = coordinateSequence.GetZ(index);
                _mathTransform.Transform(ref x, ref y, ref z);
                coordinateSequence.SetX(index, Math.Round(x, 8));
                coordinateSequence.SetY(index, Math.Round(y, 8));
                coordinateSequence.SetZ(index, Math.Round(z, 8));
            }
        }
        #endregion
    }
    #endregion
}