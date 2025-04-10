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
    /// RDFMinExclusiveFacet represents a constraint requiring the values of a literal to have a minimum numeric lower bound (excluded)
    /// </summary>
    public sealed class RDFMinExclusiveFacet : RDFFacet
    {
        #region Properties
        /// <summary>
        /// Minimum numeric lower bound (excluded) required by the facet
        /// </summary>
        public double ExclusiveLowerBound { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a facet requiring the given exclusive lower bound
        /// </summary>
        public RDFMinExclusiveFacet(double exclusiveLowerBound)
          => ExclusiveLowerBound = exclusiveLowerBound;
        #endregion

        #region Methods
        /// <summary>
        /// Gives a graph representation of the MinExclusive facet
        /// </summary>
        public override RDFGraph ToRDFGraph()
          => new RDFGraph(new List<RDFTriple> {
              new RDFTriple(URI, RDFVocabulary.XSD.MIN_EXCLUSIVE, new RDFTypedLiteral(Convert.ToString(ExclusiveLowerBound, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE)) });

        /// <summary>
        /// Validates the given literal value against the MinExclusive facet
        /// </summary>
        public override bool Validate(string literalValue)
        {
            if (double.TryParse(literalValue, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double parseLiteralValue))
                return parseLiteralValue > ExclusiveLowerBound;

            return false;
        }
        #endregion
    }
}