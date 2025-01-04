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

using RDFSharp.Model;
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFExpressionFilter represents a filter executing any kind of supported expressions
    /// </summary>
    public class RDFExpressionFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Expression to be executed by the filtering
        /// </summary>
        public RDFBooleanExpression Expression { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given expression
        /// </summary>
        public RDFExpressionFilter(RDFBooleanExpression expression)
            => Expression = expression ?? throw new RDFQueryException("Cannot create RDFExpressionFilter because given \"expression\" parameter is null.");
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