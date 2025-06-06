﻿/*
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
    /// GEODimensionExpression represents "geosparql:dimension" geographic function to be applied on a query results table.<br/>
    /// The result of this function is a numeric typed literal representing the dimension of the working geometry (0=Point,1=LineString,2=Polygon).
    /// </summary>
    public sealed class RDFGeoDimensionExpression : RDFGeoExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a geosparql:dimension function with given arguments
        /// </summary>
        public RDFGeoDimensionExpression(RDFExpression leftArgument) : base(leftArgument, null) { }

        /// <summary>
        /// Default-ctor to build a geosparql:dimension function with given arguments
        /// </summary>
        public RDFGeoDimensionExpression(RDFVariable leftArgument) : base(leftArgument, null) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the geosparql:dimension function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(geosparql:dimension(L))
            sb.Append($"({RDFQueryPrinter.PrintPatternMember(RDFVocabulary.GEOSPARQL.DIMENSION, prefixes)}(");
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