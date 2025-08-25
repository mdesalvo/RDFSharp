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
    /// RDFRegexExpression represents a Regex-checking function to be applied on a query results table.
    /// </summary>
    public sealed class RDFRegexExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// Regular Expression to be filtered
        /// </summary>
        public Regex RegEx { get; }
        #endregion

        #region Ctors

        /// <summary>
        /// Builds a Regex-checking function with given arguments
        /// </summary>
        public RDFRegexExpression(RDFExpression leftArgument, Regex regex) : base(leftArgument, null as RDFExpression)
            => RegEx = regex ?? throw new RDFQueryException("Cannot create RDFRegexExpression because given \"regex\" parameter is null.");

        /// <summary>
        /// Builds a Regex-checking function with given arguments
        /// </summary>
        public RDFRegexExpression(RDFVariable leftArgument, Regex regex) : base(leftArgument, null as RDFExpression)
            => RegEx = regex ?? throw new RDFQueryException("Cannot create RDFRegexExpression because given \"regex\" parameter is null.");
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the Regex-checking function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
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

            //(REGEX(STR(L),regex,flags))
            sb.Append("(REGEX(STR(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append($"), \"{RegEx}\"");
            if (flags.Length > 0)
                sb.Append($", \"{flags}\"");
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the Regex-checking function on the given datarow
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
                if (leftArgumentPMember != null)
                    expressionResult = RegEx.IsMatch(leftArgumentPMember.ToString()) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}