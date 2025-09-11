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
/// RDFVariableExpression represents a single-argument variable expression to be applied on a query results table.
/// </summary>
public sealed class RDFVariableExpression : RDFExpression
{
    #region Ctors
    /// <summary>
    /// Builds a variable expression with given argument
    /// </summary>
    public RDFVariableExpression(RDFExpression leftArgument)
        : base(leftArgument, null as RDFExpression) { }

    /// <summary>
    /// Builds a variable expression with given argument
    /// </summary>
    public RDFVariableExpression(RDFVariable leftArgument)
        : base(leftArgument, null as RDFExpression) { }
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the variable expression
    /// </summary>
    public override string ToString()
        => ToString(RDFModelUtilities.EmptyNamespaceList);
    internal override string ToString(List<RDFNamespace> prefixes)
    {
        StringBuilder sb = new StringBuilder(32);

        //Variable => ?L
        if (LeftArgument is RDFVariable variable)
        {
            sb.Append(RDFQueryPrinter.PrintPatternMember(variable, prefixes));
        }

        //Expression => (?L)
        else
        {
            sb.Append('(');
            sb.Append(((RDFExpression)LeftArgument).ToString(prefixes));
            sb.Append(')');
        }

        return sb.ToString();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Applies the variable expression on the given datarow
    /// </summary>
    internal override RDFPatternMember ApplyExpression(DataRow row)
    {
        #region Guards
        if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
            return null;
        #endregion

        RDFPatternMember expressionResult = null;
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
            expressionResult = leftArgumentPMember;
            #endregion
        }
        catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }
        return expressionResult;
    }
    #endregion
}