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
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFEncodeForURIExpression represents a string encoding function to be applied on a query results table.
    /// </summary>
    public class RDFEncodeForURIExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a string encoding function with given arguments
        /// </summary>
        public RDFEncodeForURIExpression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Default-ctor to build a string encoding function with given arguments
        /// </summary>
        public RDFEncodeForURIExpression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the string encoding function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(ENCODE_FOR_URI(L))
            sb.Append("(ENCODE_FOR_URI(");
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
        /// Applies the string encoding function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFPlainLiteral expressionResult = null;

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
                switch (leftArgumentPMember)
                {
                    case RDFResource leftArgumentPMemberResource:
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberResource.ToString());
                        break;
                    case RDFPlainLiteral leftArgumentPMemberPLiteral:
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberPLiteral.Value);
                        break;
                    case RDFTypedLiteral leftArgumentPMemberTLiteral when leftArgumentPMemberTLiteral.HasStringDatatype():
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberTLiteral.Value);
                        break;
                    default:
                        leftArgumentPMember = null; //binding error => cleanup
                        break;
                }

                if (leftArgumentPMember == null)
                    return null;
                expressionResult = new RDFPlainLiteral(Uri.EscapeDataString(leftArgumentPMember.ToString()));
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}