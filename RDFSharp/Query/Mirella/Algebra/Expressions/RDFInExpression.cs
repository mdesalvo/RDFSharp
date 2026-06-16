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
using System.Linq;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFInExpression represents a lookup function to be applied on a query results table.
    /// </summary>
    public sealed class RDFInExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// List of expressions into which the LeftArgument must be searched. Constant terms and variables given to the
        /// pattern-member constructors are wrapped into RDFConstantExpression/RDFVariableExpression, so the membership
        /// test is uniformly an OR-chain of equality comparisons against fully-fledged (possibly dynamic) expressions.
        /// </summary>
        internal List<RDFExpression> InTerms { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a lookup function searching the given (constant or variable) terms
        /// </summary>
        public RDFInExpression(RDFExpression leftArgument, List<RDFPatternMember> inTerms) : base(leftArgument, null as RDFExpression)
            => InTerms = WrapPatternMembers(inTerms);

        /// <summary>
        /// Builds a lookup function searching the given (constant or variable) terms
        /// </summary>
        public RDFInExpression(RDFVariable leftArgument, List<RDFPatternMember> inTerms) : base(leftArgument, null as RDFExpression)
            => InTerms = WrapPatternMembers(inTerms);

        /// <summary>
        /// Builds a lookup function searching the given expressions
        /// </summary>
        public RDFInExpression(RDFExpression leftArgument, List<RDFExpression> inTerms) : base(leftArgument, null as RDFExpression)
            => InTerms = CleanExpressions(inTerms);

        /// <summary>
        /// Builds a lookup function searching the given expressions
        /// </summary>
        public RDFInExpression(RDFVariable leftArgument, List<RDFExpression> inTerms) : base(leftArgument, null as RDFExpression)
            => InTerms = CleanExpressions(inTerms);

        /// <summary>
        /// Wraps the given pattern members into constant/variable expressions (discarding nulls and unsupported terms)
        /// </summary>
        private static List<RDFExpression> WrapPatternMembers(List<RDFPatternMember> inTerms)
        {
            List<RDFExpression> wrappedTerms = new List<RDFExpression>();
            foreach (RDFPatternMember inTerm in inTerms ?? Enumerable.Empty<RDFPatternMember>())
            {
                switch (inTerm)
                {
                    case RDFResource inTermResource: wrappedTerms.Add(new RDFConstantExpression(inTermResource)); break;
                    case RDFLiteral inTermLiteral:   wrappedTerms.Add(new RDFConstantExpression(inTermLiteral));  break;
                    case RDFVariable inTermVariable: wrappedTerms.Add(new RDFVariableExpression(inTermVariable)); break;
                }
            }
            return wrappedTerms;
        }

        /// <summary>
        /// Returns the given expressions without null values
        /// </summary>
        private static List<RDFExpression> CleanExpressions(List<RDFExpression> inTerms)
            => (inTerms ?? Enumerable.Empty<RDFExpression>()).Where(t => t != null).ToList();
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the lookup function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(VAL IN (T1,T2,...,TN))
            sb.Append('(');
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(" IN (");
            sb.Append(string.Join(", ", InTerms.Select(t => PrintInTerm(t, prefixes))));
            sb.Append("))");

            return sb.ToString();
        }

        /// <summary>
        /// Prints a single IN term: a wrapped constant/variable is printed as the bare term it carries (preserving the
        /// round-trip of the legacy constant form), while any other expression is printed in full expression form.
        /// </summary>
        private static string PrintInTerm(RDFExpression inTerm, List<RDFNamespace> prefixes)
        {
            switch (inTerm)
            {
                case RDFConstantExpression constantExpression: return RDFQueryPrinter.PrintPatternMember((RDFPatternMember)constantExpression.LeftArgument, prefixes);
                case RDFVariableExpression variableExpression: return RDFQueryPrinter.PrintPatternMember((RDFPatternMember)variableExpression.LeftArgument, prefixes);
                default:                                       return inTerm.ToString(prefixes);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the lookup function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(RDFTableRow row)
        {
            #region Guards
            if (LeftArgument is RDFVariable && !row.HasColumn(LeftArgument.ToString()))
                return null;
            #endregion

            bool keepRow = false;
            try
            {
                #region Calculate Result
                //This expression is equivalent to an OR-chain of equality comparison expressions
                List<RDFExpression>.Enumerator inTermsEnumerator = InTerms.GetEnumerator();
                while (!keepRow && inTermsEnumerator.MoveNext())
                {
                    RDFComparisonExpression compExpression = LeftArgument is RDFExpression leftArgExpr
                        ? new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, leftArgExpr, inTermsEnumerator.Current)
                        : new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, (RDFVariable)LeftArgument, inTermsEnumerator.Current);

                    RDFPatternMember searchResult = compExpression.ApplyExpression(row);
                    if (searchResult?.Equals(RDFTypedLiteral.True) ?? false)
                        keepRow = true;
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return keepRow ? RDFTypedLiteral.True : RDFTypedLiteral.False;
        }
        #endregion
    }
}