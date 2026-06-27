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
using System.Globalization;
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
        /// Start position of the substring search (1 => first char), when given as a fixed integer
        /// </summary>
        public int Start { get; internal set; }

        /// <summary>
        /// Eventual length of the substring search, when given as a fixed integer
        /// </summary>
        public int? Length { get; internal set; }

        /// <summary>
        /// Start position of the substring search, when given as a per-row expression (overrides Start)
        /// </summary>
        public RDFExpression StartExpression { get; internal set; }

        /// <summary>
        /// Eventual length of the substring search, when given as a per-row expression (overrides Length)
        /// </summary>
        public RDFExpression LengthExpression { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a substring function with given (fixed) start/length arguments
        /// </summary>
        public RDFSubstringExpression(RDFExpression leftArgument, int start, int? length=null)
            : base(leftArgument, null as RDFExpression)
        {
            Start = start;
            Length = length;
        }

        /// <summary>
        /// Builds a substring function with given (fixed) start/length arguments
        /// </summary>
        public RDFSubstringExpression(RDFVariable leftArgument, int start, int? length=null)
            : base(leftArgument, null as RDFExpression)
        {
            Start = start;
            Length = length;
        }

        /// <summary>
        /// Builds a substring function with given (per-row) start/length expressions
        /// </summary>
        public RDFSubstringExpression(RDFExpression leftArgument, RDFExpression startExpression, RDFExpression lengthExpression=null)
            : base(leftArgument, null as RDFExpression)
        {
            StartExpression = startExpression ?? throw new RDFQueryException("Cannot create RDFSubstringExpression because given \"startExpression\" parameter is null.");
            LengthExpression = lengthExpression;
        }

        /// <summary>
        /// Builds a substring function with given (per-row) start/length expressions
        /// </summary>
        public RDFSubstringExpression(RDFVariable leftArgument, RDFExpression startExpression, RDFExpression lengthExpression=null)
            : base(leftArgument, null as RDFExpression)
        {
            StartExpression = startExpression ?? throw new RDFQueryException("Cannot create RDFSubstringExpression because given \"startExpression\" parameter is null.");
            LengthExpression = lengthExpression;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the substring function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(SUBSTR(L,START[,LENGTH]))
            sb.Append("(SUBSTR(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(", ");
            sb.Append(StartExpression != null ? StartExpression.ToString(prefixes) : Start.ToString(CultureInfo.InvariantCulture));
            if (LengthExpression != null)
                sb.Append($", {LengthExpression.ToString(prefixes)}");
            else if (Length.HasValue)
                sb.Append($", {Length.Value.ToString(CultureInfo.InvariantCulture)}");
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the substring function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(RDFTableRow row)
        {
            RDFPlainLiteral expressionResult = null;

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

                //Resolve the effective start/length (fixed integers, or evaluated per-row expressions)
                if (!TryResolveInteger(StartExpression, Start, row, out int start))
                    return null;
                bool hasLength = LengthExpression != null || Length.HasValue;
                int length = 0;
                if (hasLength && !TryResolveInteger(LengthExpression, Length ?? 0, row, out length))
                    return null;
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
                int startIndex = start <= 1 ? 0 : start-1;
                if (hasLength)
                {
                    int effectiveLength = length < 0 ? 0 : length;
                    if (effectiveLength == 0 || startIndex >= workingPLit.Value.Length)
                        expressionResult = new RDFPlainLiteral(string.Empty, workingPLit.Language);
                    else if (effectiveLength >= workingPLit.Value.Length)
                        expressionResult = workingPLit;
                    else
                        expressionResult = new RDFPlainLiteral(workingPLit.Value.Substring(startIndex, effectiveLength), workingPLit.Language);
                }
                else
                {
                    expressionResult = startIndex >= workingPLit.Value.Length ? new RDFPlainLiteral(string.Empty, workingPLit.Language)
                                                                              : new RDFPlainLiteral(workingPLit.Value.Substring(startIndex), workingPLit.Language);
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }

        /// <summary>
        /// Resolves an integer argument: when the per-row expression is given it is evaluated and parsed, otherwise the
        /// fixed fallback value is returned. Returns false when a given expression does not evaluate to an integer.
        /// </summary>
        private static bool TryResolveInteger(RDFExpression expression, int fixedValue, RDFTableRow row, out int value)
        {
            if (expression == null)
            {
                value = fixedValue;
                return true;
            }

            value = 0;
            return expression.ApplyExpression(row) is RDFTypedLiteral evaluatedLiteral
                    && evaluatedLiteral.HasDecimalDatatype()
                    && int.TryParse(evaluatedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
        #endregion
    }
}
