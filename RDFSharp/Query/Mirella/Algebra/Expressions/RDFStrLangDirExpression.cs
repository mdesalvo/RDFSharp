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
/// RDFStrLangDirExpression represents a language+direction plainliteral creator function to be applied on a query results table.
/// </summary>
public sealed class RDFStrLangDirExpression : RDFExpression
{
    #region Properties
    /// <summary>
    /// Indicates the direction used for the plainlitera's language tags emitted by this expression
    /// </summary>
    public RDFQueryEnums.RDFLanguageDirections Direction { get; }
    #endregion

    #region Ctors
    /// <summary>
    /// Builds a language+direction plainliteral creator function with given arguments
    /// </summary>
    public RDFStrLangDirExpression(RDFExpression leftArgument, RDFExpression rightArgument, RDFQueryEnums.RDFLanguageDirections direction)
        : base(leftArgument, rightArgument) => Direction = direction;

    /// <summary>
    /// Builds a language+direction plainliteral creator function with given arguments
    /// </summary>
    public RDFStrLangDirExpression(RDFExpression leftArgument, RDFVariable rightArgument, RDFQueryEnums.RDFLanguageDirections direction)
        : base(leftArgument, rightArgument) => Direction = direction;

    /// <summary>
    /// Builds a language+direction plainliteral creator function with given arguments
    /// </summary>
    public RDFStrLangDirExpression(RDFVariable leftArgument, RDFExpression rightArgument, RDFQueryEnums.RDFLanguageDirections direction)
        : base(leftArgument, rightArgument) => Direction = direction;

    /// <summary>
    /// Builds a language+direction plainliteral creator function with given arguments
    /// </summary>
    public RDFStrLangDirExpression(RDFVariable leftArgument, RDFVariable rightArgument, RDFQueryEnums.RDFLanguageDirections direction)
        : base(leftArgument, rightArgument) => Direction = direction;
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the language+direction plainliteral creator function
    /// </summary>
    public override string ToString()
        => ToString(RDFModelUtilities.EmptyNamespaceList);
    internal override string ToString(List<RDFNamespace> prefixes)
    {
        StringBuilder sb = new StringBuilder(32);

        //(STRLANGDIR(L,R,D))
        sb.Append("(STRLANGDIR(");
        if (LeftArgument is RDFExpression expLeftArgument)
            sb.Append(expLeftArgument.ToString(prefixes));
        else
            sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
        sb.Append(", ");
        if (RightArgument is RDFExpression expRightArgument)
            sb.Append(expRightArgument.ToString(prefixes));
        else
            sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
        sb.Append(", ");
        sb.Append(Direction == RDFQueryEnums.RDFLanguageDirections.LTR ? "ltr" : "rtl");
        sb.Append("))");

        return sb.ToString();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Applies the language+direction plainliteral creator function on the given datarow
    /// </summary>
    internal override RDFPatternMember ApplyExpression(DataRow row)
    {
        RDFPlainLiteral expressionResult = null;

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
            //We can only proceed if we have been given a well-formed language tag (without direction)
            if (rightArgumentPMember is RDFPlainLiteral rightArgumentPMemberLiteral
                && RDFRegex.LangTagNoDirRegex().IsMatch(rightArgumentPMemberLiteral.Value))
            {
                expressionResult = leftArgumentPMember switch
                {
                    //And a plain literal without language
                    RDFPlainLiteral leftArgumentPMemberPLit when !leftArgumentPMemberPLit.HasLanguage() => new
                        RDFPlainLiteral(leftArgumentPMemberPLit.Value,
                            string.Concat(rightArgumentPMemberLiteral.Value,
                                Direction == RDFQueryEnums.RDFLanguageDirections.LTR ? "--ltr" : "--rtl")),
                    //Or a string-based typed literal
                    RDFTypedLiteral leftArgumentPMemberTLit when leftArgumentPMemberTLit.HasStringDatatype() => new
                        RDFPlainLiteral(leftArgumentPMemberTLit.Value,
                            string.Concat(rightArgumentPMemberLiteral.Value,
                                Direction == RDFQueryEnums.RDFLanguageDirections.LTR ? "--ltr" : "--rtl")),
                    _ => null
                };
            }
            #endregion
        }
        catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

        return expressionResult;
    }
    #endregion
}