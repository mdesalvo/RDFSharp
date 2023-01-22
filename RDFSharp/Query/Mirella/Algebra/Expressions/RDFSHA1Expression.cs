/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Security.Cryptography;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFSHA1Expression represents a SHA1 hash function to be applied on a query results table.
    /// </summary>
    public class RDFSHA1Expression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a SHA1 hash function with given arguments
        /// </summary>
        public RDFSHA1Expression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Default-ctor to build a SHA1 hash function with given arguments
        /// </summary>
        public RDFSHA1Expression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SHA1 hash function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(SHA1(L))
            sb.Append("(SHA1(");
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
        /// Applies the string SHA1 function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFPlainLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
                return expressionResult;
            #endregion

            try
            {
                #region Evaluate Arguments
                //Evaluate left argument (Expression VS Variable)
                RDFPatternMember leftArgumentPMember = null;
                if (LeftArgument is RDFExpression leftArgumentExpression)
                    leftArgumentPMember = leftArgumentExpression.ApplyExpression(row);
                else
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[LeftArgument.ToString()].ToString());
                #endregion

                #region Calculate Result
                if (leftArgumentPMember is RDFLiteral leftArgumentPMemberLiteral)
                    leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberLiteral.Value);
                else if (leftArgumentPMember is RDFResource leftArgumentPMemberResource)
                    leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberResource.ToString());

                if (leftArgumentPMember == null)
                    return expressionResult;
                using (SHA1CryptoServiceProvider SHA1Encryptor = new SHA1CryptoServiceProvider())
                {
                    byte[] hashBytes = SHA1Encryptor.ComputeHash(RDFModelUtilities.UTF8_NoBOM.GetBytes(leftArgumentPMember.ToString()));
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                        sb.Append(hashBytes[i].ToString("x2"));
                    expressionResult = new RDFPlainLiteral(sb.ToString());
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}