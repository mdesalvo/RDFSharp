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
    /// RDFFractionDigitsFacet represents a constraint requiring the values of a literal to have a maximum number of fraction digits
    /// </summary>
    public class RDFFractionDigitsFacet : RDFFacet
    {
        #region Properties
        /// <summary>
        /// Maximum number of fraction digits required by the facet
        /// </summary>
        public uint AllowedDigits { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a facet requiring the given exclusive upper bound
        /// </summary>
        public RDFFractionDigitsFacet(uint allowedDigits)
          => AllowedDigits = allowedDigits;
        #endregion

        #region Methods
        /// <summary>
        /// Gives a graph representation of the FractionDigits facet
        /// </summary>
        public override RDFGraph ToRDFGraph()
          => new RDFGraph(new List<RDFTriple>() {
              new RDFTriple(URI, RDFVocabulary.XSD.FRACTION_DIGITS, new RDFTypedLiteral(Convert.ToString(AllowedDigits, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)) });

        /// <summary>
        /// Validates the given literal value against the FractionDigits facet
        /// </summary>
        public override bool Validate(string literalValue)
        {
            if (double.TryParse(literalValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double _))
            {
                if (literalValue.IndexOf(NumberFormatInfo.InvariantInfo.NumberDecimalSeparator) == -1)
                    return true;

                string fractionDigits = literalValue.Split(NumberFormatInfo.InvariantInfo.NumberDecimalSeparator[0])[1];
                return fractionDigits.Length <= AllowedDigits;
            }
            return false;
        }
        #endregion
    }
}