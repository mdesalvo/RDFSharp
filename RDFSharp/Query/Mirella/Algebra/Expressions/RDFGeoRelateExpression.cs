﻿/*
   Copyright 2012-2023 Marco De Salvo

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
using System.Text.RegularExpressions;

namespace RDFSharp.Query
{
    /// <summary>
    /// GEORelateExpression represents "geof:sfRelate" geographic function to be applied on a query results table.<br/>
    /// The result of this function is a boolean typed literal.
    /// </summary>
    public class RDFGeoRelateExpression : RDFGeoExpression
    {
        #region Properties
        /// <summary>
        /// Indicates the DE-9IM relation to be validated
        /// </summary>
        internal string DE9IMRelation { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a geof:sfRelate function with given arguments
        /// </summary>
        public RDFGeoRelateExpression(RDFExpression leftArgument, RDFExpression rightArgument, string de9imRelation) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (string.IsNullOrWhiteSpace(de9imRelation))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is null or empty");
            if (!Regex.IsMatch(de9imRelation, "^[012TF\\*]{9}$", RegexOptions.IgnoreCase))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is not a valid DE-9IM intersection mask");

            DE9IMRelation = de9imRelation.ToUpper();
        }

        /// <summary>
        /// Default-ctor to build a geof:sfRelate function with given arguments
        /// </summary>
        public RDFGeoRelateExpression(RDFExpression leftArgument, RDFVariable rightArgument, string de9imRelation) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (string.IsNullOrWhiteSpace(de9imRelation))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is null or empty");
            if (!Regex.IsMatch(de9imRelation, "^[012TF\\*]{9}$", RegexOptions.IgnoreCase))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is not a valid DE-9IM intersection mask");

            DE9IMRelation = de9imRelation.ToUpper();
        }

        /// <summary>
        /// Default-ctor to build a geof:sfRelate function with given arguments
        /// </summary>
        public RDFGeoRelateExpression(RDFExpression leftArgument, RDFTypedLiteral rightArgument, string de9imRelation) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)
                 && !rightArgument.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_GML))
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a geographic typed literal");
            if (string.IsNullOrWhiteSpace(de9imRelation))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is null or empty");
            if (!Regex.IsMatch(de9imRelation, "^[012TF\\*]{9}$", RegexOptions.IgnoreCase))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is not a valid DE-9IM intersection mask");

            DE9IMRelation = de9imRelation.ToUpper();
        }

        /// <summary>
        /// Default-ctor to build a geof:sfRelate function with given arguments
        /// </summary>
        public RDFGeoRelateExpression(RDFVariable leftArgument, RDFExpression rightArgument, string de9imRelation) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (string.IsNullOrWhiteSpace(de9imRelation))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is null or empty");
            if (!Regex.IsMatch(de9imRelation, "^[012TF\\*]{9}$", RegexOptions.IgnoreCase))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is not a valid DE-9IM intersection mask");

            DE9IMRelation = de9imRelation.ToUpper();
        }

        /// <summary>
        /// Default-ctor to build a geof:sfRelate function with given arguments
        /// </summary>
        public RDFGeoRelateExpression(RDFVariable leftArgument, RDFVariable rightArgument, string de9imRelation) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (string.IsNullOrWhiteSpace(de9imRelation))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is null or empty");
            if (!Regex.IsMatch(de9imRelation, "^[012TF\\*]{9}$", RegexOptions.IgnoreCase))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is not a valid DE-9IM intersection mask");

            DE9IMRelation = de9imRelation.ToUpper();
        }

        /// <summary>
        /// Default-ctor to build a geof:sfRelate function with given arguments
        /// </summary>
        public RDFGeoRelateExpression(RDFVariable leftArgument, RDFTypedLiteral rightArgument, string de9imRelation) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)
                 && !rightArgument.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_GML))
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a geographic typed literal");
            if (string.IsNullOrWhiteSpace(de9imRelation))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is null or empty");
            if (!Regex.IsMatch(de9imRelation, "^[012TF\\*]{9}$", RegexOptions.IgnoreCase))
                throw new RDFQueryException("Cannot create expression because given \"de9imRelation\" parameter is not a valid DE-9IM intersection mask");

            DE9IMRelation = de9imRelation.ToUpper();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the geof:sfRelate function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(geof:sfRelate(L,R,DE9IM))
            sb.Append($"({RDFQueryPrinter.PrintPatternMember(RDFVocabulary.GEOSPARQL.GEOF.RELATE, prefixes)}(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(", ");
            if (RightArgument is RDFExpression expRightArgument)
                sb.Append(expRightArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
            sb.Append($", \"{DE9IMRelation}\"))");

            return sb.ToString();
        }
        #endregion
    }
}