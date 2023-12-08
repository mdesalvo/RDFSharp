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

using RDFSharp.Model;
using RDFSharp.Query;
using System.Collections.Generic;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// GEOOverlapsExpression represents "geof:sfOverlaps" geographic function to be applied on a query results table.<br/>
    /// The result of this function is a boolean typed literal.
    /// </summary>
    public class RDFGeoOverlapsExpression : RDFGeoExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a geof:sfOverlaps function with given arguments
        /// </summary>
        public RDFGeoOverlapsExpression(RDFExpression leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build a geof:sfOverlaps function with given arguments
        /// </summary>
        public RDFGeoOverlapsExpression(RDFExpression leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build a geof:sfOverlaps function with given arguments
        /// </summary>
        public RDFGeoOverlapsExpression(RDFExpression leftArgument, RDFTypedLiteral rightArgument) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)
                 && !rightArgument.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_GML))
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a geographic typed literal");
        }

        /// <summary>
        /// Default-ctor to build a geof:sfOverlaps function with given arguments
        /// </summary>
        public RDFGeoOverlapsExpression(RDFVariable leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build a geof:sfOverlaps function with given arguments
        /// </summary>
        public RDFGeoOverlapsExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build a geof:sfOverlaps function with given arguments
        /// </summary>
        public RDFGeoOverlapsExpression(RDFVariable leftArgument, RDFTypedLiteral rightArgument) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)
                 && !rightArgument.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_GML))
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a geographic typed literal");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the geof:sfOverlaps function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(geof:sfOverlaps(L,R))
            sb.Append($"({RDFQueryPrinter.PrintPatternMember(RDFVocabulary.GEOSPARQL.GEOF.SF_OVERLAPS, prefixes)}(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(", ");
            if (RightArgument is RDFExpression expRightArgument)
                sb.Append(expRightArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
            sb.Append("))");

            return sb.ToString();
        }
        #endregion
    }
}