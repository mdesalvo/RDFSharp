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
        /// Private-ctor to build a filter on the given expression
        /// </summary>
        private RDFExpressionFilter(RDFExpression expression, bool _)
            => Expression = expression ?? throw new RDFQueryException("Cannot create RDFExpressionFilter because given \"expression\" parameter is null.");

        /// <summary>
        /// Default-ctor to build a filter on the given boolean expression
        /// </summary>
        public RDFExpressionFilter(RDFBooleanExpression expression) : this(expression, true) { }

        /// <summary>
        /// Default-ctor to build a filter on the given IsBlank expression
        /// </summary>
        public RDFExpressionFilter(RDFIsBlankExpression expression) : this(expression, true) { }

        /// <summary>
        /// Default-ctor to build a filter on the given IsLiteral expression
        /// </summary>
        public RDFExpressionFilter(RDFIsLiteralExpression expression) : this(expression, true) { }

        /// <summary>
        /// Default-ctor to build a filter on the given IsUri expression
        /// </summary>
        public RDFExpressionFilter(RDFIsUriExpression expression) : this(expression, true) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => $"FILTER ( {Expression.ToString(prefixes)} )";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter to the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
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