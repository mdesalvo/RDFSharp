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
    /// GEORCC8Expression represents "geof:rcc8*" geographic function to be applied on a query results table.<br/>
    /// The result of this function is a boolean typed literal.
    /// </summary>
    public sealed class RDFGeoRCC8Expression : RDFGeoExpression
    {
        #region Properties
        /// <summary>
        /// RCC8 relation checked by this expression
        /// </summary>
        internal RDFQueryEnums.RDFGeoRCC8Relations RCC8Relation { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a geof:eh* function with given arguments
        /// </summary>
        public RDFGeoRCC8Expression(RDFExpression leftArgument, RDFExpression rightArgument, RDFQueryEnums.RDFGeoRCC8Relations rcc8Relation)
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");

            RCC8Relation = rcc8Relation;
        }

        /// <summary>
        /// Builds a geof:eh* function with given arguments
        /// </summary>
        public RDFGeoRCC8Expression(RDFExpression leftArgument, RDFVariable rightArgument, RDFQueryEnums.RDFGeoRCC8Relations rcc8Relation)
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");

            RCC8Relation = rcc8Relation;
        }

        /// <summary>
        /// Builds a geof:eh* function with given arguments
        /// </summary>
        public RDFGeoRCC8Expression(RDFExpression leftArgument, RDFTypedLiteral rightArgument, RDFQueryEnums.RDFGeoRCC8Relations rcc8Relation)
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.WKT_LITERAL.ToString())
                 && !rightArgument.Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.GML_LITERAL.ToString()))
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a geographic typed literal");

            RCC8Relation = rcc8Relation;
        }

        /// <summary>
        /// Builds a geof:eh* function with given arguments
        /// </summary>
        public RDFGeoRCC8Expression(RDFVariable leftArgument, RDFExpression rightArgument, RDFQueryEnums.RDFGeoRCC8Relations rcc8Relation)
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");

            RCC8Relation = rcc8Relation;
        }

        /// <summary>
        /// Builds a geof:eh* function with given arguments
        /// </summary>
        public RDFGeoRCC8Expression(RDFVariable leftArgument, RDFVariable rightArgument, RDFQueryEnums.RDFGeoRCC8Relations rcc8Relation)
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");

            RCC8Relation = rcc8Relation;
        }

        /// <summary>
        /// Builds a geof:eh* function with given arguments
        /// </summary>
        public RDFGeoRCC8Expression(RDFVariable leftArgument, RDFTypedLiteral rightArgument, RDFQueryEnums.RDFGeoRCC8Relations rcc8Relation)
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.WKT_LITERAL.ToString())
                 && !rightArgument.Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.GML_LITERAL.ToString()))
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a geographic typed literal");

            RCC8Relation = rcc8Relation;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the geof:eh* function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder(32);

            //(geof:rcc8*(L,R))
            sb.Append($"({RDFQueryPrinter.PrintPatternMember(GetRCC8Function(), prefixes)}(");
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

        #region Utilities
        /// <summary>
        /// Gets the RCC8 function corresponding to this expression
        /// </summary>
        internal RDFResource GetRCC8Function()
        {
            switch (RCC8Relation)
            {
                case RDFQueryEnums.RDFGeoRCC8Relations.RCC8DC:
                    return RDFVocabulary.GEOSPARQL.GEOF.RCC8DC;
                case RDFQueryEnums.RDFGeoRCC8Relations.RCC8EC:
                    return RDFVocabulary.GEOSPARQL.GEOF.RCC8EC;
                case RDFQueryEnums.RDFGeoRCC8Relations.RCC8EQ:
                    return RDFVocabulary.GEOSPARQL.GEOF.RCC8EQ;
                case RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPP:
                    return RDFVocabulary.GEOSPARQL.GEOF.RCC8NTPP;
                case RDFQueryEnums.RDFGeoRCC8Relations.RCC8NTPPI:
                    return RDFVocabulary.GEOSPARQL.GEOF.RCC8NTPPI;
                case RDFQueryEnums.RDFGeoRCC8Relations.RCC8PO:
                    return RDFVocabulary.GEOSPARQL.GEOF.RCC8PO;
                case RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPP:
                    return RDFVocabulary.GEOSPARQL.GEOF.RCC8TPP;
                case RDFQueryEnums.RDFGeoRCC8Relations.RCC8TPPI:
                    return RDFVocabulary.GEOSPARQL.GEOF.RCC8TPPI;
                default: return null;
            }
        }
        #endregion
    }
}