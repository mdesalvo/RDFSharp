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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// GEOBufferExpression represents "geof:buffer" geographic function to be applied on a query results table.<br/>
    /// The result of this function is a WKT typed literal representing a sf:Polygon expressed in WGS84 Lon/Lat.
    /// </summary>
    public sealed class RDFGeoBufferExpression : RDFGeoExpression
    {
        #region Properties
        /// <summary>
        /// Indicates the meters of the buffer polygon to be computed
        /// </summary>
        public double BufferMeters { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a geof:buffer function with given arguments
        /// </summary>
        public RDFGeoBufferExpression(RDFExpression leftArgument, double bufferMeters) : base(leftArgument, null)
            => BufferMeters = bufferMeters;

        /// <summary>
        /// Builds a geof:buffer function with given arguments
        /// </summary>
        public RDFGeoBufferExpression(RDFVariable leftArgument, double bufferMeters) : base(leftArgument, null)
            => BufferMeters = bufferMeters;
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the geof:distance function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder(32);

            //(geof:buffer(L,N))
            sb.Append($"({RDFQueryPrinter.PrintPatternMember(RDFVocabulary.GEOSPARQL.GEOF.BUFFER, prefixes)}(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append($", {Convert.ToString(BufferMeters, CultureInfo.InvariantCulture)}");
            sb.Append("))");

            return sb.ToString();
        }
        #endregion
    }
}