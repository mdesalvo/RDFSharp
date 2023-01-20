/*
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
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFUpperCaseExpression represents a string uppercase function to be applied on a query results table.
    /// </summary>
    public class RDFUpperCaseExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a string uppercase function with given arguments
        /// </summary>
        public RDFUpperCaseExpression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Default-ctor to build a a string uppercase function with given arguments
        /// </summary>
        public RDFUpperCaseExpression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the string uppercase function
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(UCASE(L))
            sb.Append("(UCASE(");
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
        /// Applies the string uppercase function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFLiteral expressionResult = null;

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

                //Binding error from arguments
                if (leftArgumentPMember == null)
                    return expressionResult;
                #endregion

                #region Calculate Result
                if (leftArgumentPMember is RDFPlainLiteral leftArgumentPMemberPLiteral)
                    expressionResult = new RDFPlainLiteral(leftArgumentPMemberPLiteral.Value.ToUpperInvariant(), leftArgumentPMemberPLiteral.Language);
                else if (leftArgumentPMember is RDFTypedLiteral leftArgumentPMemberTLiteral && leftArgumentPMemberTLiteral.HasStringDatatype())
                    expressionResult = new RDFTypedLiteral(leftArgumentPMemberTLiteral.Value.ToUpperInvariant(), leftArgumentPMemberTLiteral.Datatype);
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}