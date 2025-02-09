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
using System.Security.Cryptography;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFSHA256Expression represents a SHA256 hash function to be applied on a query results table.
    /// </summary>
    public class RDFSHA256Expression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a SHA256 hash function with given arguments
        /// </summary>
        public RDFSHA256Expression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Default-ctor to build a SHA256 hash function with given arguments
        /// </summary>
        public RDFSHA256Expression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SHA256 hash function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(SHA256(L))
            sb.Append("(SHA256(");
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
        /// Applies the string SHA256 function on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFPlainLiteral expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
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
                #endregion

                #region Calculate Result
                switch (leftArgumentPMember)
                {
                    case RDFLiteral leftArgumentPMemberLiteral:
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberLiteral.Value);
                        break;
                    case RDFResource leftArgumentPMemberResource:
                        leftArgumentPMember = new RDFPlainLiteral(leftArgumentPMemberResource.ToString());
                        break;
                }

                if (leftArgumentPMember == null)
                    return null;
                using (SHA256CryptoServiceProvider SHA256Encryptor = new SHA256CryptoServiceProvider())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (byte hashByte in SHA256Encryptor.ComputeHash(RDFModelUtilities.UTF8_NoBOM.GetBytes(leftArgumentPMember.ToString())))
                        sb.Append(hashByte.ToString("x2"));
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