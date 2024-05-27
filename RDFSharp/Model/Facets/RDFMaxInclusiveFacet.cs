/*
   Copyright 2012-2024 Marco De Salvo

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
    /// RDFMaxInclusiveFacet represents a constraint requiring the values of a literal to have a maximum numeric upper bound (included)
    /// </summary>
    public class RDFMaxInclusiveFacet : RDFFacet
    {
        #region Properties
        /// <summary>
        /// Maximum numeric upper bound (included) required by the facet
        /// </summary>
        public double InclusiveUpperBound { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a facet requiring the given exact MaxInclusive
        /// </summary>
        public RDFMaxInclusiveFacet(double inclusiveUpperBound)
          => InclusiveUpperBound = inclusiveUpperBound;
        #endregion

        #region Methods
        /// <summary>
        /// Gives a graph representation of the MaxInclusive facet
        /// </summary>
        public override RDFGraph ToRDFGraph()
          => new RDFGraph(new List<RDFTriple>() {
              new RDFTriple(URI, RDFVocabulary.XSD.MAX_INCLUSIVE, new RDFTypedLiteral(Convert.ToString(InclusiveUpperBound, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE)) });

        /// <summary>
        /// Validates the given literal value against the MaxInclusive facet
        /// </summary>
        public override bool Validate(string literalValue)
        {
            if (double.TryParse(literalValue, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double parseLiteralValue))
                return parseLiteralValue <= InclusiveUpperBound;
            return false;
        }
        #endregion
    }
}
