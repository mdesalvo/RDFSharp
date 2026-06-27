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
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFStrLangDirExpression represents a language+direction plainliteral creator function to be applied on a query results table.
    /// </summary>
    public sealed class RDFStrLangDirExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// Indicates the direction used for the plainlitera's language tags emitted by this expression, when given as a fixed value
        /// </summary>
        public RDFQueryEnums.RDFLanguageDirections Direction { get; }

        /// <summary>
        /// Indicates the direction used for the plainlitera's language tags emitted by this expression, when given as a per-row expression (overrides Direction)
        /// </summary>
        public RDFExpression DirectionExpression { get; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a language+direction plainliteral creator function with given (fixed) direction
        /// </summary>
        public RDFStrLangDirExpression(RDFExpression leftArgument, RDFExpression rightArgument, RDFQueryEnums.RDFLanguageDirections direction)
            : base(leftArgument, rightArgument) => Direction = direction;

        /// <summary>
        /// Builds a language+direction plainliteral creator function with given (fixed) direction
        /// </summary>
        public RDFStrLangDirExpression(RDFExpression leftArgument, RDFVariable rightArgument, RDFQueryEnums.RDFLanguageDirections direction)
            : base(leftArgument, rightArgument) => Direction = direction;

        /// <summary>
        /// Builds a language+direction plainliteral creator function with given (fixed) direction
        /// </summary>
        public RDFStrLangDirExpression(RDFVariable leftArgument, RDFExpression rightArgument, RDFQueryEnums.RDFLanguageDirections direction)
            : base(leftArgument, rightArgument) => Direction = direction;

        /// <summary>
        /// Builds a language+direction plainliteral creator function with given (fixed) direction
        /// </summary>
        public RDFStrLangDirExpression(RDFVariable leftArgument, RDFVariable rightArgument, RDFQueryEnums.RDFLanguageDirections direction)
            : base(leftArgument, rightArgument) => Direction = direction;

        /// <summary>
        /// Builds a language+direction plainliteral creator function with given (per-row) direction expression
        /// </summary>
        public RDFStrLangDirExpression(RDFExpression leftArgument, RDFExpression rightArgument, RDFExpression directionExpression)
            : base(leftArgument, rightArgument) => DirectionExpression = directionExpression ?? throw new RDFQueryException("Cannot create RDFStrLangDirExpression because given \"directionExpression\" parameter is null.");

        /// <summary>
        /// Builds a language+direction plainliteral creator function with given (per-row) direction expression
        /// </summary>
        public RDFStrLangDirExpression(RDFExpression leftArgument, RDFVariable rightArgument, RDFExpression directionExpression)
            : base(leftArgument, rightArgument) => DirectionExpression = directionExpression ?? throw new RDFQueryException("Cannot create RDFStrLangDirExpression because given \"directionExpression\" parameter is null.");

        /// <summary>
        /// Builds a language+direction plainliteral creator function with given (per-row) direction expression
        /// </summary>
        public RDFStrLangDirExpression(RDFVariable leftArgument, RDFExpression rightArgument, RDFExpression directionExpression)
            : base(leftArgument, rightArgument) => DirectionExpression = directionExpression ?? throw new RDFQueryException("Cannot create RDFStrLangDirExpression because given \"directionExpression\" parameter is null.");

        /// <summary>
        /// Builds a language+direction plainliteral creator function with given (per-row) direction expression
        /// </summary>
        public RDFStrLangDirExpression(RDFVariable leftArgument, RDFVariable rightArgument, RDFExpression directionExpression)
            : base(leftArgument, rightArgument) => DirectionExpression = directionExpression ?? throw new RDFQueryException("Cannot create RDFStrLangDirExpression because given \"directionExpression\" parameter is null.");
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the language+direction plainliteral creator function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(STRLANGDIR(L,R,D))
            sb.Append("(STRLANGDIR(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(", ");
            if (RightArgument is RDFExpression expRightArgument)
                sb.Append(expRightArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
            sb.Append(", ");
            sb.Append(DirectionExpression != null
                        ? DirectionExpression.ToString(prefixes)
                        : Direction == RDFQueryEnums.RDFLanguageDirections.LTR ? "\"ltr\"" : "\"rtl\"");
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the language+direction plainliteral creator function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(RDFTableRow row)
        {
            RDFPlainLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.HasColumn(LeftArgument.ToString()))
                return null;
            if (RightArgument is RDFVariable && !row.HasColumn(RightArgument.ToString()))
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

                //Evaluate right argument (Expression VS Variable)
                RDFPatternMember rightArgumentPMember;
                if (RightArgument is RDFExpression rightArgumentExpression)
                    rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                else
                    rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember((row[RightArgument.ToString()] ?? string.Empty));
                #endregion

                //Resolve the effective direction suffix (fixed enum, or evaluated per-row expression)
                string directionSuffix = ResolveDirectionSuffix(row);

                #region Calculate Result
                //We can only proceed if we have been given a well-formed language tag (without direction) and a direction
                if (directionSuffix != null
                     && rightArgumentPMember is RDFPlainLiteral rightArgumentPMemberLiteral
                     && RDFShims.LangTagNoDirRegex.Value.IsMatch(rightArgumentPMemberLiteral.Value))
                {
                    switch (leftArgumentPMember)
                    {
                        //And a plain literal without language
                        case RDFPlainLiteral leftArgumentPMemberPLit when !leftArgumentPMemberPLit.HasLanguage():
                            expressionResult = new RDFPlainLiteral(leftArgumentPMemberPLit.Value, string.Concat(rightArgumentPMemberLiteral.Value, directionSuffix));
                            break;
                        //Or a string-based typed literal
                        case RDFTypedLiteral leftArgumentPMemberTLit when leftArgumentPMemberTLit.HasStringDatatype():
                            expressionResult = new RDFPlainLiteral(leftArgumentPMemberTLit.Value, string.Concat(rightArgumentPMemberLiteral.Value, directionSuffix));
                            break;
                    }
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }

        /// <summary>
        /// Resolves the language-direction suffix ("--ltr"/"--rtl"): from the fixed enum, or from the per-row direction
        /// expression (which must evaluate to a plain literal "ltr"/"rtl"). Returns null on a per-row binding error.
        /// </summary>
        private string ResolveDirectionSuffix(RDFTableRow row)
        {
            if (DirectionExpression == null)
                return Direction == RDFQueryEnums.RDFLanguageDirections.LTR ? "--ltr" : "--rtl";

            switch ((DirectionExpression.ApplyExpression(row) as RDFLiteral)?.Value?.ToLowerInvariant())
            {
                case "ltr": return "--ltr";
                case "rtl": return "--rtl";
                default:    return null;
            }
        }
        #endregion
    }
}