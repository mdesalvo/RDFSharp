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
    /// RDFIriExpression represents an IRI-building function to be applied on a query results table
    /// (it also backs the SPARQL <c>URI</c> alias).
    /// <para>
    /// MODEL LIMITATION. RDFSharp has no query-level BASE in its model, so relative IRIs cannot be resolved:
    /// this function returns a resource argument unchanged and turns a string literal into a resource only when
    /// the string is an ABSOLUTE IRI; any relative/invalid input yields an unbound result.
    /// </para>
    /// </summary>
    public sealed class RDFIriExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds an IRI-building function with given argument
        /// </summary>
        public RDFIriExpression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Builds an IRI-building function with given argument
        /// </summary>
        public RDFIriExpression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the IRI-building function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(IRI(L)) - the canonical print also covers the URI alias (idempotent print->parse->print)
            sb.Append("(IRI(");
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
        /// Applies the IRI-building function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(RDFTableRow row)
        {
            RDFPatternMember expressionResult = null;

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
                #endregion

                #region Calculate Result
                switch (leftArgumentPMember)
                {
                    //A resource is already an IRI: IRI(iri) yields it unchanged
                    case RDFResource resourceArgument:
                        expressionResult = resourceArgument;
                        break;
                    //A literal builds a resource only if its value is an ABSOLUTE IRI (no BASE to resolve against)
                    case RDFLiteral literalArgument when Uri.TryCreate(literalArgument.Value, UriKind.Absolute, out _):
                        expressionResult = new RDFResource(literalArgument.Value);
                        break;
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}
