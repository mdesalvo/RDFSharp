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
using System.Globalization;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFTimezoneExpression represents a datetime timezone function (returning an xsd:dayTimeDuration) to be
    /// applied on a query results table.
    /// <para>
    /// RDFSharp normalizes EVERY temporal typed literal to UTC at storage time, so the timezone offset of any
    /// bound datetime is always zero: this function therefore yields the typed literal "PT0S"^^xsd:dayTimeDuration
    /// for a temporal argument, and an unbound result for anything non-temporal.
    /// </para>
    /// </summary>
    public sealed class RDFTimezoneExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a datetime timezone function with given argument
        /// </summary>
        public RDFTimezoneExpression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Builds a datetime timezone function with given argument
        /// </summary>
        public RDFTimezoneExpression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the datetime timezone function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(TIMEZONE(L))
            sb.Append("(TIMEZONE(");
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
        /// Applies the datetime timezone function on the given datarow
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
                //Only a temporal typed literal carries a timezone: since RDFSharp stores it in UTC, the timezone
                //is the zero day-time duration "PT0S"
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && leftArgumentTypedLiteral.HasDatetimeDatatype()
                     && DateTime.TryParse(leftArgumentTypedLiteral.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out _))
                    expressionResult = new RDFTypedLiteral("PT0S", RDFModelEnums.RDFDatatypes.XSD_DAYTIMEDURATION);
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}
