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

using System.Collections.Generic;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// GEOConvexHullExpression represents "geof:convexHull" geographic function to be applied on a query results table.<br/>
    /// The result of this function is a WKT typed literal representing a sf:Polygon expressed in WGS84 Lon/Lat.
    /// </summary>
    public sealed class RDFGeoConvexHullExpression : RDFGeoExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a geof:convexHull function with given arguments
        /// </summary>
        public RDFGeoConvexHullExpression(RDFExpression leftArgument) : base(leftArgument, null) { }

        /// <summary>
        /// Builds a geof:convexHull function with given arguments
        /// </summary>
        public RDFGeoConvexHullExpression(RDFVariable leftArgument) : base(leftArgument, null) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the geof:distance function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(geof:convexHull(L))
            sb.Append($"({RDFQueryPrinter.PrintPatternMember(RDFVocabulary.GEOSPARQL.GEOF.CONVEX_HULL, prefixes)}(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append("))");

            return sb.ToString();
        }
        #endregion
    }
}