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
using System;
using System.Data;
using System.Globalization;

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
        public RDFMathExpression(RDFMathExpression leftArgument, RDFMathExpression rightArgument) 
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFMathExpression leftArgument, RDFVariable rightArgument) 
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFMathExpression leftArgument, RDFTypedLiteral rightArgument) 
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.HasDecimalDatatype())
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a numeric typed literal");
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFVariable leftArgument, RDFMathExpression rightArgument) 
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFVariable leftArgument, RDFVariable rightArgument) 
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build an arithmetical expression with given arguments
        /// </summary>
        public RDFMathExpression(RDFVariable leftArgument, RDFTypedLiteral rightArgument) 
            : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.HasDecimalDatatype())
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a numeric typed literal");
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
                return expressionResult;
            if (RightArgument is RDFVariable && !row.Table.Columns.Contains(RightArgument.ToString()))
                return expressionResult;
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
                if (RightArgument is RDFExpression rightArgumentExpression)
                    rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                else if (RightArgument is RDFVariable rightArgumentVariable)
                    rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[RightArgument.ToString()].ToString());
                else
                    rightArgumentPMember = (RDFTypedLiteral)RightArgument;
                #endregion

                #region Calculate Result
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && leftArgumentTypedLiteral.HasDecimalDatatype()
                      && rightArgumentPMember is RDFTypedLiteral rightArgumentTypedLiteral
                       && rightArgumentTypedLiteral.HasDecimalDatatype())
                {
                    if (double.TryParse(leftArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double leftArgumentNumericValue)
                          && double.TryParse(rightArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double rightArgumentNumericValue))
                    {
                        //Execute the arithmetical expression's comparison logics
                        if (this is RDFAddExpression)
                            expressionResult = new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue + rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                        else if (this is RDFSubtractExpression)
                            expressionResult = new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue - rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                        else if (this is RDFMultiplyExpression)
                            expressionResult = new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue * rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                        else if (this is RDFDivideExpression && rightArgumentNumericValue != 0d)
                            expressionResult = new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue / rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
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