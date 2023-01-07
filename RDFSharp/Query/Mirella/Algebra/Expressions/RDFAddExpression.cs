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
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFAddExpression represents an arithmetical addition expression to be applied on a query results table.
    /// </summary>
    public class RDFAddExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an arithmetical addition expression with given arguments
        /// </summary>
        public RDFAddExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        }

        /// <summary>
        /// Default-ctor to build an arithmetical addition expression with given arguments
        /// </summary>
        public RDFAddExpression(RDFVariable leftArgument, RDFTypedLiteral rightArgument) : base(leftArgument, rightArgument)
        {
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.HasDecimalDatatype())
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a numeric typed literal");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the arithmetical addition expression
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => RightArgument is RDFTypedLiteral tlitRightArgument ? $"({LeftArgument} + {tlitRightArgument.Value.ToString(CultureInfo.InvariantCulture)})"
                                                                  : $"({LeftArgument} + {RightArgument})";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFTypedLiteral expressionResult = null;

            #region Guards
            string leftArgumentString = LeftArgument.ToString();
            if (!row.Table.Columns.Contains(leftArgumentString))
                return expressionResult;

            string rightArgumentString = RightArgument.ToString();
            if (RightArgument is RDFVariable && !row.Table.Columns.Contains(rightArgumentString))
                return expressionResult;
            #endregion

            try
            {
                //Fetch data corresponding to the arithmetical addition expression arguments and transform them into pattern members
                RDFPatternMember leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[leftArgumentString].ToString());
                RDFPatternMember rightArgumentPMember = RightArgument is RDFVariable ? RDFQueryUtilities.ParseRDFPatternMember(row[rightArgumentString].ToString()) : RightArgument;

                //Check compatibility of pattern members with the arithmetical addition expression (requires numeric typed literals)
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && leftArgumentTypedLiteral.HasDecimalDatatype()
                      && rightArgumentPMember is RDFTypedLiteral rightArgumentTypedLiteral
                       && rightArgumentTypedLiteral.HasDecimalDatatype())
                {
                    if (double.TryParse(leftArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double leftArgumentNumericValue)
                          && double.TryParse(rightArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double rightArgumentNumericValue))
                        expressionResult = new RDFTypedLiteral(Convert.ToString(leftArgumentNumericValue + rightArgumentNumericValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                }
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}