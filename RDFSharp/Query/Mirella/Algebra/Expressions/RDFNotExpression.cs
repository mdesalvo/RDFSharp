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
    /// RDFNotExpression represents a logical negation (boolean "NOT") expression to be applied on a query results table.
    /// It lives inside the value-expression world, so it can be nested anywhere an expression is expected (e.g. to model
    /// "NOT IN" as "!( … IN … )", "NOT EXISTS" as "!EXISTS", or a negated FILTER as "FILTER(!( … ))").
    /// </summary>
    public sealed class RDFNotExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a logical negation function with given argument
        /// </summary>
        public RDFNotExpression(RDFExpression argument) : base(argument, null as RDFExpression) { }

        /// <summary>
        /// Builds a logical negation function with given argument
        /// </summary>
        public RDFNotExpression(RDFVariable argument) : base(argument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the logical negation function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            //NOT EXISTS canonical form: when this negation wraps an EXISTS expression, render the SPARQL-canonical
            //"NOT EXISTS { … }" (rather than the generic "(!(EXISTS { … }))"), preserving round-trip identity while
            //keeping a single object model (NOT EXISTS = !EXISTS).
            if (LeftArgument is RDFExistsExpression existsArgument)
                return string.Concat("NOT ", existsArgument.ToString(prefixes));

            StringBuilder sb = new StringBuilder();

            //(!(L))
            sb.Append("(!(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the logical negation function on the given datarow
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
                #region Evaluate Argument
                //Evaluate argument (Expression VS Variable)
                RDFPatternMember argumentPMember;
                if (LeftArgument is RDFExpression argumentExpression)
                    argumentPMember = argumentExpression.ApplyExpression(row);
                else
                    argumentPMember = RDFQueryUtilities.ParseRDFPatternMember((row[LeftArgument.ToString()] ?? string.Empty));
                #endregion

                #region Calculate Result
                //We can only negate a well-formed boolean typed literal
                if (argumentPMember is RDFTypedLiteral argumentTypedLiteral
                     && argumentTypedLiteral.HasBooleanDatatype()
                     && bool.TryParse(argumentTypedLiteral.Value, out bool argumentBooleanValue))
                {
                    expressionResult = argumentBooleanValue ? RDFTypedLiteral.False : RDFTypedLiteral.True;
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}
