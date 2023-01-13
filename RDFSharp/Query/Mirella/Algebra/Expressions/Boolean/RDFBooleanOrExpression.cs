/*
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
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFBooleanOrExpression represents a boolean "OR" expression to be applied on a query results table.
    /// </summary>
    public class RDFBooleanOrExpression : RDFBooleanExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a boolean "OR" expression with given arguments
        /// </summary>
        public RDFBooleanOrExpression(RDFExpression leftArgument, RDFExpression rightArgument)
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the boolean "OR" expression
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(L || R)
            sb.Append('(');
            sb.Append(((RDFExpression)LeftArgument).ToString(prefixes));
            sb.Append(" || ");
            sb.Append(((RDFExpression)RightArgument).ToString(prefixes));
            sb.Append(')');

            return sb.ToString();
        }
        #endregion
    }
}