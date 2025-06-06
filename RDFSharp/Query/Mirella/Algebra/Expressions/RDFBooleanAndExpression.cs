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
    /// RDFBooleanAndExpression represents a boolean "AND" expression to be applied on a query results table.
    /// </summary>
    public sealed class RDFBooleanAndExpression : RDFBooleanExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a boolean "AND" expression with given arguments
        /// </summary>
        public RDFBooleanAndExpression(RDFExpression leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument)  { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the boolean "AND" expression
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(L && R)
            sb.Append('(');
            sb.Append(((RDFExpression)LeftArgument).ToString(prefixes));
            sb.Append(" && ");
            sb.Append(((RDFExpression)RightArgument).ToString(prefixes));
            sb.Append(')');

            return sb.ToString();
        }
        #endregion
    }
}