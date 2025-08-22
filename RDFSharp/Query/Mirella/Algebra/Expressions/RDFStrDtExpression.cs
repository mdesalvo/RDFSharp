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
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFStrDtExpression represents a datatype creator function to be applied on a query results table.
    /// </summary>
    public sealed class RDFStrDtExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a datatype creator function with given arguments
        /// </summary>
        public RDFStrDtExpression(RDFExpression leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a datatype creator function with given arguments
        /// </summary>
        public RDFStrDtExpression(RDFExpression leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a datatype creator function with given arguments
        /// </summary>
        public RDFStrDtExpression(RDFVariable leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

        /// <summary>
        /// Builds a datatype creator function with given arguments
        /// </summary>
        public RDFStrDtExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the datatype creator function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder(32); //Initial capacity=32 seems a good tradeoff for medium length of this expression

            //(STRDT(L,R))
            sb.Append("(STRDT(");
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(", ");
            if (RightArgument is RDFExpression expRightArgument)
                sb.Append(expRightArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
            sb.Append("))");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the datatype creator function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFTypedLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
                return null;
            if (RightArgument is RDFVariable && !row.Table.Columns.Contains(RightArgument.ToString()))
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
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[LeftArgument.ToString()].ToString());

                //Evaluate right argument (Expression VS Variable)
                RDFPatternMember rightArgumentPMember;
                if (RightArgument is RDFExpression rightArgumentExpression)
                    rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                else
                    rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[RightArgument.ToString()].ToString());
                #endregion

                #region Calculate Result
                switch (leftArgumentPMember)
                {
                    //Transform left argument result into a plain literal
                    case RDFLiteral leftArgumentPMemberLiteral:
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberLiteral.Value);
                        break;
                    case RDFResource leftArgumentPMemberResource:
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberResource.ToString());
                        break;
                }

                //We can proceed only if the given datatype is an IRI and doesn't belong
                //to the ones explicitly involved in the creation of plain literals
                if (rightArgumentPMember is RDFResource rightArgumentPMemberResource
                     && !rightArgumentPMemberResource.Equals(RDFVocabulary.RDF.PLAIN_LITERAL)
                     && !rightArgumentPMemberResource.Equals(RDFVocabulary.RDF.LANG_STRING)
                     && !rightArgumentPMemberResource.Equals(RDFVocabulary.RDF.DIR_LANG_STRING))
                {
                    RDFDatatype rightArgumentPMemberDatatype = RDFDatatypeRegister.GetDatatype(rightArgumentPMemberResource.ToString())
                                                                ?? new RDFDatatype(rightArgumentPMemberResource.URI, RDFModelEnums.RDFDatatypes.RDFS_LITERAL, null);
                    expressionResult = new RDFTypedLiteral(leftArgumentPMember.ToString(), rightArgumentPMemberDatatype);
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}