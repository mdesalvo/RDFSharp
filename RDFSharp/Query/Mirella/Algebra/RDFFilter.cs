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
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFFilter represents a filter to be applied on a query results table. A filter is exactly a boolean
    /// value-expression (the SPARQL FILTER constraint): it keeps a row when its <see cref="Expression"/> evaluates to
    /// the boolean typed literal <c>true</c>.
    /// </summary>
    public sealed class RDFFilter : RDFPatternGroupMember
    {
        #region Properties
        /// <summary>
        /// Expression executed by the filtering (it is expected to return a boolean typed literal)
        /// </summary>
        public RDFExpression Expression { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a filter on the given expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        private RDFFilter(RDFExpression expression, bool _)
            => Expression = expression ?? throw new RDFQueryException("Cannot create RDFFilter because given \"expression\" parameter is null.");

        // Provide ctors for expressions producing a Boolean response

        /// <summary>
        /// Builds a filter on the given Boolean expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFBooleanExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given Bound expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFBoundExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given Comparison expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFComparisonExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given Contains expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFContainsExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given StrStarts expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFStrStartsExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given StrEnds expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFStrEndsExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given HasLang expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFHasLangExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given HasLangDir expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFHasLangDirExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given In expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFInExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given logical negation (NOT) expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFNotExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given IsBlank expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFIsBlankExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given IsLiteral expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFIsLiteralExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given IsNumeric expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFIsNumericExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given IsUri expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFIsUriExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given LangMatches expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFLangMatchesExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given Regex expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFRegexExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given SameTerm expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFSameTermExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given EXISTS expression (FILTER ( EXISTS { … } ))
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFFilter(RDFExistsExpression expression) : this(expression, true) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal string ToString(List<RDFNamespace> prefixes)
            => $"FILTER ( {Expression.ToString(prefixes)} )";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given table row
        /// </summary>
        internal bool ApplyFilter(RDFTableRow row, bool applyNegation)
        {
            //Execute the expression on the given datarow
            RDFTypedLiteral expressionResult = Expression.ApplyExpression(row) as RDFTypedLiteral;
            bool keepRow = expressionResult?.Equals(RDFTypedLiteral.True) ?? false;

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion
    }
}