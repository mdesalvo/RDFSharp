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

using System;
using System.Collections.Generic;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFStrBeforeExpression represents a string "before" function to be applied on a query results table.
    /// </summary>
    public sealed class RDFStrBeforeExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a string "before" function with given arguments
        /// </summary>
        public RDFStrBeforeExpression(RDFExpression leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a string "before" function with given arguments
        /// </summary>
        public RDFStrBeforeExpression(RDFExpression leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a string "before" function with given arguments
        /// </summary>
        public RDFStrBeforeExpression(RDFVariable leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a string "before" function with given arguments
        /// </summary>
        public RDFStrBeforeExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the string "before" function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(STRBEFORE(L,R))
            sb.Append("(STRBEFORE(");
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
        /// Applies the string "before" function on the given datarow
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

                #region Calculate Result
                //The "source" (left) is kept as a plain literal preserving its language, since SPARQL STRBEFORE
                //propagates the source's language tag onto the (non-empty) result; the "needle" (right) is reduced
                //to its bare string value
                RDFPlainLiteral sourcePLit = AsWorkingPlainLiteral(leftArgumentPMember, preserveLanguage: true);
                RDFPlainLiteral needlePLit = AsWorkingPlainLiteral(rightArgumentPMember, preserveLanguage: false);
                if (sourcePLit == null || needlePLit == null)
                    return null;

                //Locate the first occurrence of the needle inside the source (ordinal, as in SPARQL string ops)
                int needleIndex = sourcePLit.Value.IndexOf(needlePLit.Value, StringComparison.Ordinal);

                //Not found => empty PLAIN literal (no language), per SPARQL semantics
                if (needleIndex < 0)
                    expressionResult = new RDFPlainLiteral(string.Empty);
                //Found (including the empty-needle case at index 0) => substring before it, carrying the source language
                else
                    expressionResult = new RDFPlainLiteral(sourcePLit.Value.Substring(0, needleIndex), sourcePLit.Language);
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }

        /// <summary>
        /// Reduces an evaluated argument to the plain literal the string function works on (resource/plain/typed-string
        /// only), optionally preserving the language tag; any other binding shape yields null (binding error).
        /// </summary>
        private static RDFPlainLiteral AsWorkingPlainLiteral(RDFPatternMember argumentPMember, bool preserveLanguage)
        {
            switch (argumentPMember)
            {
                case RDFResource _:
                    return new RDFPlainLiteral(argumentPMember.ToString());
                case RDFPlainLiteral plainLiteral:
                    return preserveLanguage ? new RDFPlainLiteral(plainLiteral.Value, plainLiteral.Language) : new RDFPlainLiteral(plainLiteral.Value);
                case RDFTypedLiteral typedLiteral when typedLiteral.HasStringDatatype():
                    return new RDFPlainLiteral(typedLiteral.Value);
                default:
                    return null;
            }
        }
        #endregion
    }
}
