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
using System.Data;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFBooleanOrExpression represents a boolean "OR" expression to be applied on a query results table.
    /// </summary>
    public class RDFBooleanOrExpression : RDFExpression
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
            => this.ToString(new List<RDFNamespace>());
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

        #region Methods
        /// <summary>
        /// Applies the boolean "OR" expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFTypedLiteral expressionResult = null;

            try
            {
                #region Evaluate Arguments
                //Evaluate left argument
                RDFPatternMember leftArgumentPMember = ((RDFExpression)LeftArgument).ApplyExpression(row);

                //Evaluate right argument
                RDFPatternMember rightArgumentPMember = ((RDFExpression)RightArgument).ApplyExpression(row);
                #endregion

                #region Calculate Result
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && leftArgumentTypedLiteral.HasBooleanDatatype()
                      && rightArgumentPMember is RDFTypedLiteral rightArgumentTypedLiteral
                       && rightArgumentTypedLiteral.HasBooleanDatatype())
                {
                    if (bool.TryParse(leftArgumentTypedLiteral.Value,out bool leftArgumentBooleanValue)
                          && bool.TryParse(rightArgumentTypedLiteral.Value, out bool rightArgumentBooleanValue))
                    expressionResult = (leftArgumentBooleanValue || rightArgumentBooleanValue) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}