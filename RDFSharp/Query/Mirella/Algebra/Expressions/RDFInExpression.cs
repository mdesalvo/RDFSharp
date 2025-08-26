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
        /// List of RDF terms into which the LeftArgument must be searched
        /// </summary>
        internal List<RDFPatternMember> InTerms { get; set; }
        #endregion

        #region Ctors

        /// <summary>
        /// Builds a lookup function with given arguments
        /// </summary>
        public RDFInExpression(RDFExpression leftArgument, List<RDFPatternMember> inTerms) : base(leftArgument, null as RDFExpression)
        {
            InTerms = inTerms ?? Enumerable.Empty<RDFPatternMember>().ToList();
            //Do not accept null values in input list
            InTerms.RemoveAll(t => t == null);
        }

        /// <summary>
        /// Builds a lookup function with given arguments
        /// </summary>
        public RDFInExpression(RDFVariable leftArgument, List<RDFPatternMember> inTerms) : base(leftArgument, null as RDFExpression)
        {
            InTerms = inTerms ?? Enumerable.Empty<RDFPatternMember>().ToList();
            //Do not accept null values in input list
            InTerms.RemoveAll(t => t == null);
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the lookup function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>(0));
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder(64);

            //(VAL IN (T1,T2,...,TN))
            sb.Append('(');
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(" IN (");
            sb.Append(string.Join(", ", InTerms.Select(t => RDFQueryPrinter.PrintPatternMember(t, prefixes))));
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the lookup function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
                return null;
            #endregion

            bool keepRow = false;
            try
            {
                #region Calculate Result
                //This expression is equivalent to an OR-chain of equality comparison expressions
                List<RDFPatternMember>.Enumerator inTermsEnumerator = InTerms.GetEnumerator();
                while (!keepRow && inTermsEnumerator.MoveNext())
                {
                    RDFComparisonExpression compExpression = null;
                    switch (LeftArgument)
                    {
                        case RDFExpression leftArgExpr:
                            compExpression = new RDFComparisonExpression(
                                RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                                leftArgExpr,
                                inTermsEnumerator.Current is RDFResource inTermResE ? new RDFConstantExpression(inTermResE)
                                 : inTermsEnumerator.Current is RDFLiteral inTermLitE ?  new RDFConstantExpression(inTermLitE)
                                 : inTermsEnumerator.Current is RDFVariable inTermVarE ? new RDFVariableExpression(inTermVarE)
                                 : null as RDFExpression);
                            break;

                        case RDFVariable leftArgVar:
                            compExpression = new RDFComparisonExpression(
                                RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                                leftArgVar,
                                inTermsEnumerator.Current is RDFResource inTermResV ? new RDFConstantExpression(inTermResV)
                                 : inTermsEnumerator.Current is RDFLiteral inTermLitV ?  new RDFConstantExpression(inTermLitV)
                                 : inTermsEnumerator.Current is RDFVariable inTermVarV ? new RDFVariableExpression(inTermVarV)
                                 : null as RDFExpression);
                            break;
                    }

                    RDFPatternMember searchResult = compExpression?.ApplyExpression(row);
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