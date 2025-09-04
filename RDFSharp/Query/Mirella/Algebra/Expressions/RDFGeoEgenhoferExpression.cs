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
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query;

/// <summary>
/// GEOEgenhoferExpression represents "geof:eh*" geographic function to be applied on a query results table.<br/>
/// The result of this function is a boolean typed literal.
/// </summary>
public sealed class RDFGeoEgenhoferExpression : RDFGeoExpression
{
    #region Properties
    /// <summary>
    /// Egenhofer relation checked by this expression
    /// </summary>
    internal RDFQueryEnums.RDFGeoEgenhoferRelations EgenhoferRelation { get; set; }
    #endregion

    #region Ctors
    /// <summary>
    /// Builds a geof:eh* function with given arguments
    /// </summary>
    public RDFGeoEgenhoferExpression(RDFExpression leftArgument, RDFExpression rightArgument, RDFQueryEnums.RDFGeoEgenhoferRelations egenhoferRelation)
        : base(leftArgument, rightArgument)
    {
        if (rightArgument == null)
            throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");

        EgenhoferRelation = egenhoferRelation;
    }

    /// <summary>
    /// Builds a geof:eh* function with given arguments
    /// </summary>
    public RDFGeoEgenhoferExpression(RDFExpression leftArgument, RDFVariable rightArgument, RDFQueryEnums.RDFGeoEgenhoferRelations egenhoferRelation)
        : base(leftArgument, rightArgument)
    {
        if (rightArgument == null)
            throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");

        EgenhoferRelation = egenhoferRelation;
    }

    /// <summary>
    /// Builds a geof:eh* function with given arguments
    /// </summary>
    public RDFGeoEgenhoferExpression(RDFExpression leftArgument, RDFTypedLiteral rightArgument, RDFQueryEnums.RDFGeoEgenhoferRelations egenhoferRelation)
        : base(leftArgument, rightArgument)
    {
        if (rightArgument == null)
            throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        if (!rightArgument.Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.WKT_LITERAL.ToString())
            && !rightArgument.Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.GML_LITERAL.ToString()))
            throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a geographic typed literal");

        EgenhoferRelation = egenhoferRelation;
    }

    /// <summary>
    /// Builds a geof:eh* function with given arguments
    /// </summary>
    public RDFGeoEgenhoferExpression(RDFVariable leftArgument, RDFExpression rightArgument, RDFQueryEnums.RDFGeoEgenhoferRelations egenhoferRelation)
        : base(leftArgument, rightArgument)
    {
        if (rightArgument == null)
            throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");

        EgenhoferRelation = egenhoferRelation;
    }

    /// <summary>
    /// Builds a geof:eh* function with given arguments
    /// </summary>
    public RDFGeoEgenhoferExpression(RDFVariable leftArgument, RDFVariable rightArgument, RDFQueryEnums.RDFGeoEgenhoferRelations egenhoferRelation)
        : base(leftArgument, rightArgument)
    {
        if (rightArgument == null)
            throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");

        EgenhoferRelation = egenhoferRelation;
    }

    /// <summary>
    /// Builds a geof:eh* function with given arguments
    /// </summary>
    public RDFGeoEgenhoferExpression(RDFVariable leftArgument, RDFTypedLiteral rightArgument, RDFQueryEnums.RDFGeoEgenhoferRelations egenhoferRelation)
        : base(leftArgument, rightArgument)
    {
        if (rightArgument == null)
            throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
        if (!rightArgument.Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.WKT_LITERAL.ToString())
            && !rightArgument.Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.GML_LITERAL.ToString()))
            throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a geographic typed literal");

        EgenhoferRelation = egenhoferRelation;
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the geof:eh* function
    /// </summary>
    public override string ToString()
        => ToString(RDFModelUtilities.EmptyNamespaceList);
    internal override string ToString(List<RDFNamespace> prefixes)
    {
        StringBuilder sb = new StringBuilder(32);

        //(geof:eh*(L,R))
        sb.Append($"({RDFQueryPrinter.PrintPatternMember(GetEgenhoferFunction(), prefixes)}(");
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

    #region Utilities
    /// <summary>
    /// Gets the Egenhofer function corresponding to this expression
    /// </summary>
    internal RDFResource GetEgenhoferFunction()
    {
        return EgenhoferRelation switch
        {
            RDFQueryEnums.RDFGeoEgenhoferRelations.Contains => RDFVocabulary.GEOSPARQL.GEOF.EH_CONTAINS,
            RDFQueryEnums.RDFGeoEgenhoferRelations.CoveredBy => RDFVocabulary.GEOSPARQL.GEOF.EH_COVERED_BY,
            RDFQueryEnums.RDFGeoEgenhoferRelations.Covers => RDFVocabulary.GEOSPARQL.GEOF.EH_COVERS,
            RDFQueryEnums.RDFGeoEgenhoferRelations.Disjoint => RDFVocabulary.GEOSPARQL.GEOF.EH_DISJOINT,
            RDFQueryEnums.RDFGeoEgenhoferRelations.Equals => RDFVocabulary.GEOSPARQL.GEOF.EH_EQUALS,
            RDFQueryEnums.RDFGeoEgenhoferRelations.Inside => RDFVocabulary.GEOSPARQL.GEOF.EH_INSIDE,
            RDFQueryEnums.RDFGeoEgenhoferRelations.Meet => RDFVocabulary.GEOSPARQL.GEOF.EH_MEET,
            RDFQueryEnums.RDFGeoEgenhoferRelations.Overlap => RDFVocabulary.GEOSPARQL.GEOF.EH_OVERLAP,
            _ => null,
        };
    }
    #endregion
}