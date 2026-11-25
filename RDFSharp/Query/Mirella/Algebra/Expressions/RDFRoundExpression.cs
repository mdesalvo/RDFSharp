/*
   Copyright 2012-2026 Marco De Salvo

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

using System;
using System.Collections.Generic;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFRoundExpression represents an arithmetical round function to be applied on a query results table.
    /// </summary>
    public sealed class RDFRoundExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds an arithmetical round function with given arguments
        /// </summary>
        public RDFRoundExpression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Builds an arithmetical round function with given arguments
        /// </summary>
        public RDFRoundExpression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the arithmetical round function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(ROUND(L))
            sb.Append("(ROUND(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the arithmetical abs function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(RDFTableRow row)
        {
            RDFTypedLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.HasColumn(LeftArgument.ToString()))
                return null;
            #endregion

            try
            {
                #region Evaluate Arguments
                //Evaluate left argument (Expression VS Variable)
                RDFPatternMember leftArgumentPMember;
                if (LeftArgument is RDFExpression leftArgumentExpression)
                    leftArgumentPMember = leftArgumentExpression.ApplyExpression(row);
                else
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember((row[LeftArgument.ToString()] ?? string.Empty));
                #endregion

                #region Calculate Result
                //The result keeps the base numeric type of the argument: exact for integer and decimal values and IEEE 754 for float and double ones
                //fn:round gives the nearest integer and breaks a tie toward positive infinity (-2.5 gives -2 and 2.5 gives 3)
                //Subtracting the floor is exact in both regimes (adding 0.5 would round a long decimal): INF and NaN fall through unchanged because the comparison with NaN is false
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral)
                {
                    expressionResult = RDFArithmeticEngine.ComputeNumericFunction(
                        leftArgumentTypedLiteral,
                        exactValue =>
                        {
                            decimal floorValue = Math.Floor(exactValue);
                            return exactValue - floorValue >= 0.5m ? floorValue + 1 : floorValue;
                        },
                        approximateValue =>
                        {
                            double floorValue = Math.Floor(approximateValue);
                            return approximateValue - floorValue >= 0.5 ? floorValue + 1 : floorValue;
                        });
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}