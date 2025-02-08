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

using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFMathExpression represents an arithmetical expression to be applied on a query results table.
    /// </summary>
    public abstract class RDFMathExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFExpression leftArgument, RDFExpression rightArgument) 
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFExpression leftArgument, RDFVariable rightArgument) 
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFExpression leftArgument, RDFTypedLiteral rightArgument) 
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.HasDecimalDatatype())
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a numeric typed literal");
            #endregion
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFVariable leftArgument, RDFExpression rightArgument) 
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFVariable leftArgument, RDFVariable rightArgument) 
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFVariable leftArgument, RDFTypedLiteral rightArgument) 
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.HasDecimalDatatype())
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a numeric typed literal");
            #endregion
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the arithmetical addition expression
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(L MATHOP R)
            sb.Append('(');
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            switch (this)
            {
                case RDFAddExpression _:
                    sb.Append(" + ");
                    break;
                case RDFSubtractExpression _:
                    sb.Append(" - ");
                    break;
                case RDFMultiplyExpression _:
                    sb.Append(" * ");
                    break;
                case RDFDivideExpression _:
                    sb.Append(" / ");
                    break;
            }
            switch (RightArgument)
            {
                case RDFExpression expRightArgument:
                    sb.Append(expRightArgument.ToString(prefixes));
                    break;
                case RDFTypedLiteral tlRightArgument:
                    sb.Append(tlRightArgument.Value.ToString(CultureInfo.InvariantCulture));
                    break;
                default:
                    sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
                    break;
            }
            sb.Append(')');

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the arithmetical expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFTypedLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
                return null;
            if (RightArgument is RDFVariable && !row.Table.Columns.Contains(RightArgument.ToString()))
                return null;
            #endregion

            try
            {
                #region Evaluate Arguments
                //Evaluate left argument (Expression VS Variable)
                RDFPatternMember leftArgumentPMember = null;
                if (LeftArgument is RDFExpression leftArgumentExpression)
                    leftArgumentPMember = leftArgumentExpression.ApplyExpression(row);
                else
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[LeftArgument.ToString()].ToString());

                //Evaluate right argument (Expression VS Variable VS TypedLiteral)
                RDFPatternMember rightArgumentPMember = null;
                switch (RightArgument)
                {
                    case RDFExpression rightArgumentExpression:
                        rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                        break;
                    case RDFVariable _:
                        rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[RightArgument.ToString()].ToString());
                        break;
                    default:
                        rightArgumentPMember = (RDFTypedLiteral)RightArgument;
                        break;
                }
                #endregion

                #region Calculate Result
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && leftArgumentTypedLiteral.HasDecimalDatatype()
                     && rightArgumentPMember is RDFTypedLiteral rightArgumentTypedLiteral
                     && rightArgumentTypedLiteral.HasDecimalDatatype())
                {
                    double? leftArgumentNumericValueFinal = null;
                    double? rightArgumentNumericValueFinal = null;

                    //owl:rational needs parsing and evaluation before being compared (LEFT)
                    if (leftArgumentTypedLiteral.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                        leftArgumentNumericValueFinal = Convert.ToDouble(RDFModelUtilities.ComputeOWLRationalValue(leftArgumentTypedLiteral), CultureInfo.InvariantCulture);
                    //owl:rational needs parsing and evaluation before being compared (RIGHT)
                    if (rightArgumentTypedLiteral.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                        rightArgumentNumericValueFinal = Convert.ToDouble(RDFModelUtilities.ComputeOWLRationalValue(rightArgumentTypedLiteral), CultureInfo.InvariantCulture);

                    //Compute the arithmetical expression if we have valid double values from the arguments
                    if (!leftArgumentNumericValueFinal.HasValue && double.TryParse(leftArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double leftArgumentNumericValue))
                        leftArgumentNumericValueFinal = leftArgumentNumericValue;
                    if (!rightArgumentNumericValueFinal.HasValue && double.TryParse(rightArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double rightArgumentNumericValue))
                        rightArgumentNumericValueFinal = rightArgumentNumericValue;
                    if (leftArgumentNumericValueFinal.HasValue && rightArgumentNumericValueFinal.HasValue)
                        expressionResult = EvaluateMathExpression(leftArgumentNumericValueFinal.Value, rightArgumentNumericValueFinal.Value);
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }

        /// <summary>
        /// Compute the arithmetical expression on the given numeric parameters
        /// </summary>
        private RDFTypedLiteral EvaluateMathExpression(double leftArgumentNumericValue, double rightArgumentNumericValue)
        {
            switch (this)
            {
                case RDFAddExpression _:
                    return new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue + rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                case RDFSubtractExpression _:
                    return new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue - rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                case RDFMultiplyExpression _:
                    return new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue * rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                case RDFDivideExpression _ when rightArgumentNumericValue != 0d:
                    return new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue / rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                default:
                    //Just to keep the compiler happy...
                    return null;
            }
        }
        #endregion
    }
}