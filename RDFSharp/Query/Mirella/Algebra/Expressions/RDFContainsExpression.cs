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

using System.Collections.Generic;
using System.Data;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFContainsExpression represents a string contains function to be applied on a query results table.
    /// </summary>
    public sealed class RDFContainsExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a string contains function with given arguments
        /// </summary>
        public RDFContainsExpression(RDFExpression leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a string contains function with given arguments
        /// </summary>
        public RDFContainsExpression(RDFExpression leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a string contains function with given arguments
        /// </summary>
        public RDFContainsExpression(RDFVariable leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a string contains function with given arguments
        /// </summary>
        public RDFContainsExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the string contains function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder(32);

            //(CONTAINS(L,R))
            sb.Append("(CONTAINS(");
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
        /// Applies the string contains function on the given datarow
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
                RDFPatternMember leftArgumentPMember;
                if (LeftArgument is RDFExpression leftArgumentExpression)
                    leftArgumentPMember = leftArgumentExpression.ApplyExpression(row);
                else
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[LeftArgument.ToString()].ToString());

                //Evaluate right argument (Expression VS Variable)
                RDFPatternMember rightArgumentPMember;
                if (RightArgument is RDFExpression rightArgumentExpression)
                    rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                else
                    rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[RightArgument.ToString()].ToString());
                #endregion

                #region Calculate Result
                //Transform left argument result into a plain literal
                leftArgumentPMember = leftArgumentPMember switch
                {
                    RDFResource _ => new RDFPlainLiteral(leftArgumentPMember.ToString()),
                    RDFPlainLiteral plLeftArgumentPMember => new RDFPlainLiteral(plLeftArgumentPMember.Value),
                    RDFTypedLiteral tlLeftArgumentPMember when tlLeftArgumentPMember.HasStringDatatype() => new RDFPlainLiteral(tlLeftArgumentPMember.Value),
                    _ => null,//binding error => cleanup
                };
                //Transform right argument result into a plain literal
                rightArgumentPMember = rightArgumentPMember switch
                {
                    RDFResource _ => new RDFPlainLiteral(rightArgumentPMember.ToString()),
                    RDFPlainLiteral plRightArgumentPMember => new RDFPlainLiteral(plRightArgumentPMember.Value),
                    RDFTypedLiteral tlRightArgumentPMember when tlRightArgumentPMember.HasStringDatatype() => new RDFPlainLiteral(tlRightArgumentPMember.Value),
                    _ => null,//binding error => cleanup
                };
                if (leftArgumentPMember != null && rightArgumentPMember != null)
                    expressionResult = leftArgumentPMember.ToString().Contains(rightArgumentPMember.ToString()) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}