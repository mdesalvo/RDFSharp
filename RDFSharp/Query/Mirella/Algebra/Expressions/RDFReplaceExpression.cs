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
    /// RDFReplaceExpression represents a Regex-replacing function to be applied on a query results table.
    /// </summary>
    public sealed class RDFReplaceExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// Regular Expression to be applied
        /// </summary>
        public Regex RegEx { get; }
        #endregion

        #region Ctors

        /// <summary>
        /// Builds a Regex-replacing function with given arguments
        /// </summary>
        public RDFReplaceExpression(RDFExpression leftArgument, RDFExpression rightArgument, Regex regex) : base(
            leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create RDFReplaceExpression because given \"rightArgument\" parameter is null");
            #endregion

            RegEx = regex ?? throw new RDFQueryException("Cannot create RDFReplaceExpression because given \"regex\" parameter is null.");
        }

        /// <summary>
        /// Builds a Regex-replacing function with given arguments
        /// </summary>
        public RDFReplaceExpression(RDFExpression leftArgument, RDFVariable rightArgument, Regex regex) : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create RDFReplaceExpression because given \"rightArgument\" parameter is null");
            #endregion

            RegEx = regex ?? throw new RDFQueryException("Cannot create RDFReplaceExpression because given \"regex\" parameter is null.");
        }

        /// <summary>
        /// Builds a Regex-replacing function with given arguments
        /// </summary>
        public RDFReplaceExpression(RDFVariable leftArgument, RDFExpression rightArgument, Regex regex) : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create RDFReplaceExpression because given \"rightArgument\" parameter is null");
            #endregion

            RegEx = regex ?? throw new RDFQueryException("Cannot create RDFReplaceExpression because given \"regex\" parameter is null.");
        }

        /// <summary>
        /// Builds a Regex-replacing function with given arguments
        /// </summary>
        public RDFReplaceExpression(RDFVariable leftArgument, RDFVariable rightArgument, Regex regex) : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create RDFReplaceExpression because given \"rightArgument\" parameter is null");
            #endregion

            RegEx = regex ?? throw new RDFQueryException("Cannot create RDFReplaceExpression because given \"regex\" parameter is null.");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the Regex-replacing function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder(64);

            //Serialize supported flags
            StringBuilder flags = new StringBuilder(4);
            if (RegEx.Options.HasFlag(RegexOptions.IgnoreCase))
                flags.Append('i');
            if (RegEx.Options.HasFlag(RegexOptions.Singleline))
                flags.Append('s');
            if (RegEx.Options.HasFlag(RegexOptions.Multiline))
                flags.Append('m');
            if (RegEx.Options.HasFlag(RegexOptions.IgnorePatternWhitespace))
                flags.Append('x');

            //(REPLACE(STR(L),regex,STR(R),flags))
            sb.Append("(REPLACE(STR(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append($"), \"{RegEx}\", STR(");
            if (RightArgument is RDFExpression expRightArgument)
                sb.Append(expRightArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
            sb.Append(')');
            if (flags.Length > 0)
                sb.Append($", \"{flags}\"");
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the Regex-replacing function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFPatternMember expressionResult = null;

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
                switch (leftArgumentPMember)
                {
                    //Transform left argument result into a plain literal
                    case RDFResource _:
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMember.ToString());
                        break;
                    case RDFPlainLiteral plitLeftArgumentPMember:
                        leftArgumentPMember = new RDFPlainLiteral(plitLeftArgumentPMember.Value);
                        break;
                    case RDFTypedLiteral tlitLeftArgumentPMember when tlitLeftArgumentPMember.HasStringDatatype():
                        leftArgumentPMember = new RDFPlainLiteral(tlitLeftArgumentPMember.Value);
                        break;
                    default:
                        leftArgumentPMember = null; //binding error => cleanup
                        break;
                }

                switch (rightArgumentPMember)
                {
                    //Transform right argument result into a plain literal
                    case RDFResource _:
                        rightArgumentPMember = new RDFPlainLiteral(rightArgumentPMember.ToString());
                        break;
                    case RDFPlainLiteral plitRightArgumentPMember:
                        rightArgumentPMember = new RDFPlainLiteral(plitRightArgumentPMember.Value);
                        break;
                    case RDFTypedLiteral tlitRightArgumentPMember when tlitRightArgumentPMember.HasStringDatatype():
                        rightArgumentPMember = new RDFPlainLiteral(tlitRightArgumentPMember.Value);
                        break;
                    default:
                        rightArgumentPMember = null; //binding error => cleanup
                        break;
                }

                if (leftArgumentPMember != null && rightArgumentPMember != null)
                {
                    string result = Regex.Replace(leftArgumentPMember.ToString(), RegEx.ToString(), rightArgumentPMember.ToString());

                    //We want to expose a strongly typed result, depending on its nature
                    if (RDFModelUtilities.GetUriFromString(result) != null)
                        expressionResult = new RDFResource(result);
                    else
                        expressionResult = new RDFPlainLiteral(result);
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}