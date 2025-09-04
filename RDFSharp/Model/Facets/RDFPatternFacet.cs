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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RDFSharp.Model;

/// <summary>
/// RDFPatternFacet represents a constraint requiring the values of a literal to match a pattern
/// </summary>
public sealed class RDFPatternFacet : RDFFacet
{
    #region Properties
    /// <summary>
    /// Pattern required by the facet
    /// </summary>
    public string Pattern { get; internal set; }
    private Lazy<Regex> RegEx { get; }
    #endregion

    #region Ctors
    /// <summary>
    /// Builds a facet requiring the given pattern
    /// </summary>
    public RDFPatternFacet(string pattern)
    {
        Pattern = pattern ?? string.Empty;
        RegEx = new Lazy<Regex>(() => new Regex(pattern ?? string.Empty, RegexOptions.Compiled));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Gives a graph representation of the pattern facet
    /// </summary>
    public override RDFGraph ToRDFGraph()
        => new RDFGraph([
            new RDFTriple(URI, RDFVocabulary.XSD.PATTERN, new RDFTypedLiteral(Pattern, RDFModelEnums.RDFDatatypes.XSD_STRING)) ]);

    /// <summary>
    /// Validates the given literal value against the pattern facet
    /// </summary>
    public override bool Validate(string literalValue)
        => RegEx.Value.IsMatch(literalValue ?? string.Empty);
    #endregion
}