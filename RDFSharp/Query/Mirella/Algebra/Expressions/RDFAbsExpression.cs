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

using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFAbsExpression represents an arithmetical abs function to be applied on a query results table.
    /// </summary>
    public class RDFAbsExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an arithmetical abs function with given arguments
        /// </summary>
        public RDFAbsExpression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Default-ctor to build an arithmetical abs function with given arguments
        /// </summary>
        public RDFAbsExpression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the arithmetical abs function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(ABS(L))
            sb.Append("(ABS(");
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
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFTypedLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
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
                #endregion

                #region Calculate Result
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && leftArgumentTypedLiteral.HasDecimalDatatype())
                {
                    //owl:rational needs parsing and evaluation before being compared
                    if (leftArgumentTypedLiteral.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                        expressionResult = new RDFTypedLiteral(
                                            Convert.ToString(Math.Abs(RDFModelUtilities.ComputeOWLRationalValue(leftArgumentTypedLiteral)), CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                    else if (double.TryParse(leftArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double leftArgumentNumericValue))
                        expressionResult = new RDFTypedLiteral(
                                            Convert.ToString(Math.Abs(leftArgumentNumericValue), CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}