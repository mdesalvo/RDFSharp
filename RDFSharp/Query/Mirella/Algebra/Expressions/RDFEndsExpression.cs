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
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFEndsExpression represents a string ends function to be applied on a query results table.
    /// </summary>
    public class RDFEndsExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a string ends function with given arguments
        /// </summary>
        public RDFEndsExpression(RDFExpression leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build a string ends function with given arguments
        /// </summary>
        public RDFEndsExpression(RDFExpression leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build a string ends function with given arguments
        /// </summary>
        public RDFEndsExpression(RDFVariable leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build a a string ends function with given arguments
        /// </summary>
        public RDFEndsExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the string ends function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(STRENDS(L,R))
            sb.Append("(STRENDS(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(", ");
            if (RightArgument is RDFExpression expRightArgument)
                sb.Append(expRightArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the string ends function on the given datarow
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

                //Evaluate right argument (Expression VS Variable)
                RDFPatternMember rightArgumentPMember = null;
                if (RightArgument is RDFExpression rightArgumentExpression)
                    rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                else
                    rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[RightArgument.ToString()].ToString());
                #endregion

                #region Calculate Result
                //Transform left argument result into a plain literal
                if (leftArgumentPMember is RDFResource)
                    leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMember.ToString());
                else if (leftArgumentPMember is RDFPlainLiteral plitLeftArgumentPMember)
                    leftArgumentPMember = new RDFPlainLiteral(plitLeftArgumentPMember.Value);
                else if (leftArgumentPMember is RDFTypedLiteral tlitLeftArgumentPMember && tlitLeftArgumentPMember.HasStringDatatype())
                    leftArgumentPMember = new RDFPlainLiteral(tlitLeftArgumentPMember.Value);
                else
                    leftArgumentPMember = null; //binding error => cleanup

                //Transform right argument result into a plain literal
                if (rightArgumentPMember is RDFResource)
                    rightArgumentPMember = new RDFPlainLiteral(rightArgumentPMember.ToString());
                else if (rightArgumentPMember is RDFPlainLiteral plitRightArgumentPMember)
                    rightArgumentPMember = new RDFPlainLiteral(plitRightArgumentPMember.Value);
                else if (rightArgumentPMember is RDFTypedLiteral tlitRightArgumentPMember && tlitRightArgumentPMember.HasStringDatatype())
                    rightArgumentPMember = new RDFPlainLiteral(tlitRightArgumentPMember.Value);
                else
                    rightArgumentPMember = null; //binding error => cleanup

                if (leftArgumentPMember != null && rightArgumentPMember != null)
                    expressionResult = leftArgumentPMember.ToString().EndsWith(rightArgumentPMember.ToString(), StringComparison.Ordinal) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}