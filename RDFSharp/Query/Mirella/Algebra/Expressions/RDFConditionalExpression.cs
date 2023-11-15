/*
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
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFConditionalExpression represents an IF-THEN-ELSE expression to be applied on a query results table.
    /// </summary>
    public class RDFConditionalExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// Represents the condition argument given to the conditional expression
        /// </summary>
        public RDFExpression ConditionArgument { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a conditional expression with given arguments
        /// </summary>
        public RDFConditionalExpression(RDFExpression conditionArgument, RDFExpression leftArgument, RDFExpression rightArgument) 
            : base(leftArgument, rightArgument) 
        {
            #region Guards
            if (conditionArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"conditionArgument\" parameter is null");
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion

            ConditionArgument = conditionArgument;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the conditional expression
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(IF(C, L, R))
            sb.Append("(IF(");
            sb.Append(ConditionArgument.ToString(prefixes));
            sb.Append(", ");
            sb.Append(((RDFExpression)LeftArgument).ToString(prefixes));
            sb.Append(", ");
            sb.Append(((RDFExpression)RightArgument).ToString(prefixes));
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the conditional expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFPatternMember expressionResult = null;

            try
            {
                #region Evaluate Arguments
                //Evaluate condition argument
                RDFPatternMember conditionArgumentPMember = ConditionArgument.ApplyExpression(row);
                #endregion

                #region Calculate Result
                if (conditionArgumentPMember is RDFTypedLiteral conditionArgumentTypedLiteral
                     && conditionArgumentTypedLiteral.HasBooleanDatatype())
                {
                    if (bool.TryParse(conditionArgumentTypedLiteral.Value, out bool conditionalArgumentBooleanValue))
                        expressionResult = conditionalArgumentBooleanValue ? ((RDFExpression)LeftArgument).ApplyExpression(row) : ((RDFExpression)RightArgument).ApplyExpression(row);
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}