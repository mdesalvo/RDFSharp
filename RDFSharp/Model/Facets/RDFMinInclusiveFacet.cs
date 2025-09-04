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
    /// RDFMinInclusiveFacet represents a constraint requiring the values of a literal to have a minimum numeric lower bound (included)
    /// </summary>
    public sealed class RDFMinInclusiveFacet : RDFFacet
    {
        #region Properties
        /// <summary>
        /// Minimum numeric lower bound (included) required by the facet
        /// </summary>
        public double InclusiveLowerBound { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a facet requiring the given inclusive lower bound
        /// </summary>
        public RDFMinInclusiveFacet(double inclusiveLowerBound)
          => InclusiveLowerBound = inclusiveLowerBound;
        #endregion

        #region Methods
        /// <summary>
        /// Gives a graph representation of the MinInclusive facet
        /// </summary>
        public override RDFGraph ToRDFGraph()
          => new RDFGraph([
              new RDFTriple(URI, RDFVocabulary.XSD.MIN_INCLUSIVE, new RDFTypedLiteral(Convert.ToString(InclusiveLowerBound, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE)) ]);

        /// <summary>
        /// Validates the given literal value against the MinInclusive facet
        /// </summary>
        public override bool Validate(string literalValue)
        {
            if (double.TryParse(literalValue, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double parseLiteralValue))
                return parseLiteralValue >= InclusiveLowerBound;

            return false;
        }
        #endregion
    }
}