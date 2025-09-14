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
using System.Globalization;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFMaxLengthFacet represents a constraint requiring the values of a literal to have a maximum length
    /// </summary>
    public sealed class RDFMaxLengthFacet : RDFFacet
    {
        #region Properties
        /// <summary>
        /// Maximum length required by the facet
        /// </summary>
        public uint Length { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a facet requiring the given maximum length
        /// </summary>
        public RDFMaxLengthFacet(uint length)
          => Length = length;
        #endregion

        #region Methods
        /// <summary>
        /// Gives a graph representation of the maxlength facet
        /// </summary>
        public override RDFGraph ToRDFGraph()
          => new RDFGraph(new List<RDFTriple>(1) {
              new RDFTriple(URI, RDFVocabulary.XSD.MAX_LENGTH, new RDFTypedLiteral(Convert.ToString(Length, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)) });

        /// <summary>
        /// Validates the given literal value against the maxlength facet
        /// </summary>
        public override bool Validate(string literalValue)
          => string.IsNullOrEmpty(literalValue) || literalValue.Length <= Length;
        #endregion
    }
}