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
    /// RDFSubstringExpression represents a substring function to be applied on a query results table.
    /// </summary>
    public sealed class RDFSubstringExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// Start position of the substring search (1 => first char)
        /// </summary>
        public int Start { get; internal set; }

        /// <summary>
        /// Eventual length of the substring search
        /// </summary>
        public int? Length { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a substring function with given arguments
        /// </summary>
        public RDFSubstringExpression(RDFExpression leftArgument, int start, int? length=null)
            : base(leftArgument, null as RDFExpression)
        {
            Start = start;
            Length = length;
        }

        /// <summary>
        /// Builds a substring function with given arguments
        /// </summary>
        public RDFSubstringExpression(RDFVariable leftArgument, int start, int? length=null)
            : base(leftArgument, null as RDFExpression)
        {
            Start = start;
            Length = length;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the substring function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(SUBSTRING(L,START[,LENGTH]))
            sb.Append("(SUBSTRING(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append($", {Start}");
            if (Length.HasValue)
                sb.Append($", {Length}");
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the substring function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFPlainLiteral expressionResult = null;

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

                switch (leftArgumentPMember)
                {
                    //Transform left argument result into a plain literal
                    case RDFResource _:
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMember.ToString());
                        break;
                    case RDFPlainLiteral plitLeftArgumentPMember:
                        leftArgumentPMember = new RDFPlainLiteral(plitLeftArgumentPMember.Value, plitLeftArgumentPMember.Language);
                        break;
                    case RDFTypedLiteral tlitLeftArgumentPMember when tlitLeftArgumentPMember.HasStringDatatype():
                        leftArgumentPMember = new RDFPlainLiteral(tlitLeftArgumentPMember.Value);
                        break;
                    default:
                        leftArgumentPMember = null; //binding error => cleanup
                        break;
                }

                if (leftArgumentPMember == null)
                    return null;

                //Calculate substring on the working plain literal
                RDFPlainLiteral workingPLit = (RDFPlainLiteral)leftArgumentPMember;
                int start = Start <= 1 ? 0 : Start-1;
                if (Length.HasValue)
                {
                    int length = Length.Value < 0 ? 0 : Length.Value;
                    if (length == 0 || start >= workingPLit.Value.Length)
                        expressionResult = new RDFPlainLiteral(string.Empty, workingPLit.Language);
                    else if (length >= workingPLit.Value.Length)
                        expressionResult = workingPLit;
                    else
                        expressionResult = new RDFPlainLiteral(workingPLit.Value.Substring(start, length), workingPLit.Language);
                }
                else
                {
                    expressionResult = start >= workingPLit.Value.Length ? new RDFPlainLiteral(string.Empty, workingPLit.Language)
                                                                         : new RDFPlainLiteral(workingPLit.Value.Substring(start), workingPLit.Language);
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}