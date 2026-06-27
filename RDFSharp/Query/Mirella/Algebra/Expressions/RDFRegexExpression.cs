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

using System.Collections.Generic;
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
        /// Regular Expression to be filtered, when given as a fixed (build-time compiled) pattern
        /// </summary>
        public Regex RegEx { get; }

        /// <summary>
        /// Pattern to be filtered, when given as a per-row expression (overrides RegEx)
        /// </summary>
        public RDFExpression PatternExpression { get; }

        /// <summary>
        /// Eventual flags driving the per-row pattern, when given as a per-row expression
        /// </summary>
        public RDFExpression FlagsExpression { get; }
        #endregion

        #region Ctors

        /// <summary>
        /// Builds a Regex-checking function with given (fixed) regular expression
        /// </summary>
        public RDFRegexExpression(RDFExpression leftArgument, Regex regex) : base(leftArgument, null as RDFExpression)
            => RegEx = regex ?? throw new RDFQueryException("Cannot create RDFRegexExpression because given \"regex\" parameter is null.");

        /// <summary>
        /// Builds a Regex-checking function with given (fixed) regular expression
        /// </summary>
        public RDFRegexExpression(RDFVariable leftArgument, Regex regex) : base(leftArgument, null as RDFExpression)
            => RegEx = regex ?? throw new RDFQueryException("Cannot create RDFRegexExpression because given \"regex\" parameter is null.");

        /// <summary>
        /// Builds a Regex-checking function with given (per-row) pattern and flags expressions
        /// </summary>
        public RDFRegexExpression(RDFExpression leftArgument, RDFExpression patternExpression, RDFExpression flagsExpression=null) : base(leftArgument, null as RDFExpression)
        {
            PatternExpression = patternExpression ?? throw new RDFQueryException("Cannot create RDFRegexExpression because given \"patternExpression\" parameter is null.");
            FlagsExpression = flagsExpression;
        }

        /// <summary>
        /// Builds a Regex-checking function with given (per-row) pattern and flags expressions
        /// </summary>
        public RDFRegexExpression(RDFVariable leftArgument, RDFExpression patternExpression, RDFExpression flagsExpression=null) : base(leftArgument, null as RDFExpression)
        {
            PatternExpression = patternExpression ?? throw new RDFQueryException("Cannot create RDFRegexExpression because given \"patternExpression\" parameter is null.");
            FlagsExpression = flagsExpression;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the Regex-checking function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(REGEX(L,regex,flags))
            sb.Append("(REGEX(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));

            if (PatternExpression != null)
            {
                //Per-row pattern/flags: print them in expression form
                sb.Append($", {PatternExpression.ToString(prefixes)}");
                if (FlagsExpression != null)
                    sb.Append($", {FlagsExpression.ToString(prefixes)}");
            }
            else
            {
                //Fixed pattern: serialize the compiled regular expression and its supported flags
                StringBuilder flags = new StringBuilder();
                if (RegEx.Options.HasFlag(RegexOptions.IgnoreCase))
                    flags.Append('i');
                if (RegEx.Options.HasFlag(RegexOptions.Singleline))
                    flags.Append('s');
                if (RegEx.Options.HasFlag(RegexOptions.Multiline))
                    flags.Append('m');
                if (RegEx.Options.HasFlag(RegexOptions.IgnorePatternWhitespace))
                    flags.Append('x');

                sb.Append($", \"{RegEx}\"");
                if (flags.Length > 0)
                    sb.Append($", \"{flags}\"");
            }
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the Regex-checking function on the given datarow
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

                //Resolve the effective regular expression (fixed, or compiled per-row from evaluated pattern/flags)
                Regex effectiveRegex = ResolveRegex(row);
                #endregion

                #region Calculate Result
                if (leftArgumentPMember != null && effectiveRegex != null)
                    expressionResult = effectiveRegex.IsMatch(leftArgumentPMember.ToString()) ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }

        /// <summary>
        /// Resolves the regular expression to apply: the fixed one when given, otherwise compiled per-row from the
        /// evaluated pattern (and flags) expressions. Returns null when the per-row compilation fails (binding error).
        /// </summary>
        private Regex ResolveRegex(RDFTableRow row)
        {
            if (PatternExpression == null)
                return RegEx;

            string pattern = (PatternExpression.ApplyExpression(row) as RDFLiteral)?.Value;
            string flags = (FlagsExpression?.ApplyExpression(row) as RDFLiteral)?.Value ?? string.Empty;
            return RDFQueryUtilities.CompileRegexOrNull(pattern, flags);
        }
        #endregion
    }
}
