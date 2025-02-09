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

using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFBooleanExpression represents a boolean expression to be applied on a query results table.
    /// </summary>
    public abstract class RDFBooleanExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a boolean expression with given arguments
        /// </summary>
        public RDFBooleanExpression(RDFExpression leftArgument, RDFExpression rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the boolean "AND" expression on the given datarow
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
                    if (bool.TryParse(leftArgumentTypedLiteral.Value, out bool leftArgumentBooleanValue)
                         && bool.TryParse(rightArgumentTypedLiteral.Value, out bool rightArgumentBooleanValue))
                    {
                        switch (this)
                        {
                            //Execute the boolean expression's comparison logics
                            case RDFBooleanAndExpression _:
                                expressionResult = leftArgumentBooleanValue && rightArgumentBooleanValue ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                                break;
                            case RDFBooleanOrExpression _:
                                expressionResult = leftArgumentBooleanValue || rightArgumentBooleanValue ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                                break;
                        }
                    }
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}