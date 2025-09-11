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

namespace RDFSharp.Query;

/// <summary>
/// RDFConcatExpression represents a string concat function to be applied on a query results table.
/// </summary>
public sealed class RDFConcatExpression : RDFExpression
{
    #region Ctors
    /// <summary>
    /// Builds a string concat function with given arguments
    /// </summary>
    public RDFConcatExpression(RDFExpression leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

    /// <summary>
    /// Builds a string concat function with given arguments
    /// </summary>
    public RDFConcatExpression(RDFExpression leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }

    /// <summary>
    /// Builds a string concat function with given arguments
    /// </summary>
    public RDFConcatExpression(RDFVariable leftArgument, RDFExpression rightArgument) : base(leftArgument, rightArgument) { }

    /// <summary>
    /// Builds a string concat function with given arguments
    /// </summary>
    public RDFConcatExpression(RDFVariable leftArgument, RDFVariable rightArgument) : base(leftArgument, rightArgument) { }
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the string concat function
    /// </summary>
    public override string ToString()
        => ToString(RDFModelUtilities.EmptyNamespaceList);
    internal override string ToString(List<RDFNamespace> prefixes)
    {
        StringBuilder sb = new StringBuilder(32);

        //(CONCAT(L,R))
        sb.Append("(CONCAT(");
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
    /// Applies the string concat function on the given datarow
    /// </summary>
    internal override RDFPatternMember ApplyExpression(DataRow row)
    {
        #region Guards
        if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
            return null;
        if (RightArgument is RDFVariable && !row.Table.Columns.Contains(RightArgument.ToString()))
            return null;
        #endregion

        RDFPlainLiteral expressionResult = null;
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
            //Transform left argument result into a plain literal
            leftArgumentPMember = leftArgumentPMember switch
            {
                RDFLiteral leftArgumentPMemberLiteral => new RDFPlainLiteral(leftArgumentPMemberLiteral.Value),
                RDFResource leftArgumentPMemberResource => new RDFPlainLiteral(leftArgumentPMemberResource.ToString()),
                _ => leftArgumentPMember
            };
            //Transform right argument result into a plain literal
            rightArgumentPMember = rightArgumentPMember switch
            {
                RDFLiteral rightArgumentPMemberLiteral => new RDFPlainLiteral(rightArgumentPMemberLiteral.Value),
                RDFResource rightArgumentPMemberResource =>
                    new RDFPlainLiteral(rightArgumentPMemberResource.ToString()),
                _ => rightArgumentPMember
            };
            expressionResult = new RDFPlainLiteral(string.Concat(leftArgumentPMember?.ToString(), rightArgumentPMember?.ToString()));
            #endregion
        }
        catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }
        return expressionResult;
    }
    #endregion
}