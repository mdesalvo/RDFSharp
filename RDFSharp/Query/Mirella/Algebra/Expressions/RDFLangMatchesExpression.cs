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
using System.Text.RegularExpressions;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFLangMatchesExpression represents a Language-checking function to be applied on a query results table.
    /// </summary>
    public sealed class RDFLangMatchesExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a Language-checking function with given arguments
        /// </summary>
        public RDFLangMatchesExpression(RDFExpression leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Builds a Language-checking function with given arguments
        /// </summary>
        public RDFLangMatchesExpression(RDFExpression leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Builds a Language-checking function with given arguments
        /// </summary>
        public RDFLangMatchesExpression(RDFVariable leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Builds a Language-checking function with given arguments
        /// </summary>
        public RDFLangMatchesExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the Language-checking function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder(32);

            //(LANGMATCHES(LANG(L),R))
            sb.Append("(LANGMATCHES(LANG(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append("),");
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
        /// Applies the Language-checking function on the given datarow
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
                if (leftArgumentPMember is RDFPlainLiteral leftArgPLit
                     && rightArgumentPMember is RDFPlainLiteral rightArgPLit)
                {
                    string leftArgPLitString = leftArgPLit.ToString();
                    switch (rightArgPLit.Value)
                    {
                        //NO language is acceptable in the evaluating left argument
                        case "":
                            expressionResult = RDFRegex.EndingLangTagRegex().IsMatch(leftArgPLitString)
                                                 ? RDFTypedLiteral.False : RDFTypedLiteral.True;
                            break;

                        //ANY language is acceptable in the evaluating left argument
                        case "*":
                            expressionResult = RDFRegex.EndingLangTagRegex().IsMatch(leftArgPLitString)
                                                 ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                            break;

                        //GIVEN language is acceptable in the evaluating left argument
                        default:
                            expressionResult = Regex.IsMatch(leftArgPLitString, $"@{rightArgPLit.Value}{RDFRegex.LangTagSubMask}$", RegexOptions.IgnoreCase)
                                                 ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                            break;
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