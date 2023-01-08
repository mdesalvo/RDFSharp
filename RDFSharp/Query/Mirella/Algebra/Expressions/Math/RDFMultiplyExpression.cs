﻿/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFMultiplyExpression represents an arithmetical multiplication expression to be applied on a query results table.
    /// </summary>
    public class RDFMultiplyExpression : RDFMathExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an arithmetical multiplication expression with given arguments
        /// </summary>
        public RDFMultiplyExpression(RDFMathExpression leftArgument, RDFMathExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an arithmetical multiplication expression with given arguments
        /// </summary>
        public RDFMultiplyExpression(RDFMathExpression leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an arithmetical multiplication expression with given arguments
        /// </summary>
        public RDFMultiplyExpression(RDFMathExpression leftArgument, RDFTypedLiteral rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an arithmetical multiplication expression with given arguments
        /// </summary>
        public RDFMultiplyExpression(RDFVariable leftArgument, RDFMathExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an arithmetical multiplication expression with given arguments
        /// </summary>
        public RDFMultiplyExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an arithmetical multiplication expression with given arguments
        /// </summary>
        public RDFMultiplyExpression(RDFVariable leftArgument, RDFTypedLiteral rightArgument) : base(leftArgument, rightArgument) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the arithmetical multiplication expression
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('(');
            sb.Append(LeftArgument.ToString());
            sb.Append(" * ");
            sb.Append(RightArgument is RDFTypedLiteral tlRightArgument ? tlRightArgument.Value.ToString(CultureInfo.InvariantCulture) : RightArgument.ToString());
            sb.Append(')');

            return sb.ToString();
        }
        #endregion
    }
}