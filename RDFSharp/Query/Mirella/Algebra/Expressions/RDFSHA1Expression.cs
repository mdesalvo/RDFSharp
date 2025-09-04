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

namespace RDFSharp.Query;

/// <summary>
/// RDFSHA1Expression represents a SHA1 hash function to be applied on a query results table.
/// </summary>
public sealed class RDFSHA1Expression : RDFExpression
{
    #region Ctors
    /// <summary>
    /// Builds a SHA1 hash function with given arguments
    /// </summary>
    public RDFSHA1Expression(RDFExpression leftArgument) : base(leftArgument, null as RDFExpression) { }

    /// <summary>
    /// Builds a SHA1 hash function with given arguments
    /// </summary>
    public RDFSHA1Expression(RDFVariable leftArgument) : base(leftArgument, null as RDFExpression) { }
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the SHA1 hash function
    /// </summary>
    public override string ToString()
        => ToString(RDFModelUtilities.EmptyNamespaceList);
    internal override string ToString(List<RDFNamespace> prefixes)
    {
        StringBuilder sb = new StringBuilder(32);

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

            string leftArgumentPMemberString = leftArgumentPMember.ToString();
            StringBuilder sb = new StringBuilder(leftArgumentPMemberString.Length);
            foreach (byte hashByte in SHA1.HashData(RDFModelUtilities.UTF8_NoBOM.GetBytes(leftArgumentPMemberString)))
                sb.Append(hashByte.ToString("x2"));
            expressionResult = new RDFPlainLiteral(sb.ToString());
            #endregion
        }
        catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

        return expressionResult;
    }
    #endregion
}