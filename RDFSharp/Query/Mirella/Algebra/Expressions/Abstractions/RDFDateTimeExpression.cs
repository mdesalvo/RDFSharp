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

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFDateTimeExpression represents a datetime expression to be applied on a query results table.
    /// </summary>
    public abstract class RDFDateTimeExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a datetime day expression with given arguments
        /// </summary>
        protected RDFDateTimeExpression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Default-ctor to build a datetime day expression with given arguments
        /// </summary>
        protected RDFDateTimeExpression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the datetime day function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            switch (this)
            {
                //(DATETIMEOP(L))
                case RDFYearExpression _:
                    sb.Append("(YEAR(");
                    break;
                case RDFMonthExpression _:
                    sb.Append("(MONTH(");
                    break;
                case RDFDayExpression _:
                    sb.Append("(DAY(");
                    break;
                case RDFHoursExpression _:
                    sb.Append("(HOURS(");
                    break;
                case RDFMinutesExpression _:
                    sb.Append("(MINUTES(");
                    break;
                case RDFSecondsExpression _:
                    sb.Append("(SECONDS(");
                    break;
            }
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
        /// Applies the datetime day function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFTypedLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
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
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[LeftArgument.ToString()].ToString());
                #endregion

                #region Calculate Result
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && leftArgumentTypedLiteral.HasDatetimeDatatype())
                {
                    if (DateTime.TryParse(leftArgumentTypedLiteral.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime leftArgumentDateTimeValue))
                    {
                        switch (this)
                        {
                            //Execute the datetime expression's comparison logics
                            case RDFYearExpression _:
                                expressionResult = new RDFTypedLiteral(leftArgumentDateTimeValue.Year.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER);
                                break;
                            case RDFMonthExpression _:
                                expressionResult = new RDFTypedLiteral(leftArgumentDateTimeValue.Month.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER);
                                break;
                            case RDFDayExpression _:
                                expressionResult = new RDFTypedLiteral(leftArgumentDateTimeValue.Day.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER);
                                break;
                            case RDFHoursExpression _:
                                expressionResult = new RDFTypedLiteral(leftArgumentDateTimeValue.Hour.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER);
                                break;
                            case RDFMinutesExpression _:
                                expressionResult = new RDFTypedLiteral(leftArgumentDateTimeValue.Minute.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER);
                                break;
                            case RDFSecondsExpression _:
                                expressionResult = new RDFTypedLiteral($"{leftArgumentDateTimeValue.Second}.{leftArgumentDateTimeValue.Millisecond}", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
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