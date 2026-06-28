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
    /// RDFExpressionFilter represents a filter executing any kind of supported expressions
    /// </summary>
    public sealed class RDFExpressionFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Expression to be executed by the filtering (should return a boolean typed literal)
        /// </summary>
        public RDFExpression Expression { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a filter on the given expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        private RDFExpressionFilter(RDFExpression expression, bool _)
            => Expression = expression ?? throw new RDFQueryException("Cannot create RDFExpressionFilter because given \"expression\" parameter is null.");

        /// <summary>
        /// Builds a filter on the given Boolean expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFBooleanExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given Bound expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFBoundExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given Comparison expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFComparisonExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given Contains expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFContainsExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given StrStarts expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFStrStartsExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given StrEnds expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFStrEndsExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given HasLang expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFHasLangExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given HasLangDir expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFHasLangDirExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given In expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFInExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given logical negation (NOT) expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFNotExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given IsBlank expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFIsBlankExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given IsLiteral expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFIsLiteralExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given IsNumeric expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFIsNumericExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given IsUri expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFIsUriExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given LangMatches expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFLangMatchesExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given Regex expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFRegexExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given SameTerm expression
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFSameTermExpression expression) : this(expression, true) { }

        /// <summary>
        /// Builds a filter on the given EXISTS expression (FILTER ( EXISTS { … } ))
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExpressionFilter(RDFExistsExpression expression) : this(expression, true) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
            => $"FILTER ( {Expression.ToString(prefixes)} )";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter to the given datarow
        /// </summary>
        internal override bool ApplyFilter(RDFTableRow row, bool applyNegation)
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