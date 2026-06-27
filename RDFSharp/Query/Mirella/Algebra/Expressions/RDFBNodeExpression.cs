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
    /// RDFBNodeExpression represents a blank node generator function to be applied on a query results table.
    /// </summary>
    public sealed class RDFBNodeExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a blank node generator function (fresh blank node on each evaluation)
        /// </summary>
        public RDFBNodeExpression() { }

        /// <summary>
        /// Builds a blank node generator function deterministically derived from the given argument
        /// </summary>
        public RDFBNodeExpression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Builds a blank node generator function deterministically derived from the given argument
        /// </summary>
        public RDFBNodeExpression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the blank node generator function
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            //(BNODE()) when no argument was given
            if (LeftArgument == null)
                return "(BNODE())";

            StringBuilder sb = new StringBuilder();

            //(BNODE(L))
            sb.Append("(BNODE(");
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
        /// Applies the blank node generator expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(RDFTableRow row)
        {
            //No argument => a fresh blank node on each evaluation
            if (LeftArgument == null)
                return new RDFResource();

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

                if (leftArgumentPMember == null)
                    return null;
                #endregion

                #region Calculate Result
                return new RDFResource($"bnode:{RDFModelUtilities.CreateHash(leftArgumentPMember.ToString())}");
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return null;
        }
        #endregion
    }
}
