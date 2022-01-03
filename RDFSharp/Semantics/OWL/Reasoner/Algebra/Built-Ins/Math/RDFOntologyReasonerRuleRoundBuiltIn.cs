/*
   Copyright 2012-2022 Marco De Salvo
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

using RDFSharp.Model;
using RDFSharp.Query;
using System.Text;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRuleRoundBuiltIn represents a math built-in of type swrlb:round
    /// </summary>
    public class RDFOntologyReasonerRuleRoundBuiltIn : RDFOntologyReasonerRuleMathBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the Uri of the built-in (swrlb:round)
        /// </summary>
        private static RDFResource BuiltInUri = new RDFResource($"swrlb:round");
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a swrlb:ceiling built-in with given arguments
        /// </summary>
        public RDFOntologyReasonerRuleRoundBuiltIn(RDFVariable leftArgument, RDFVariable rightArgument)
            : base(new RDFOntologyResource() { Value = BuiltInUri }, leftArgument, rightArgument, double.NaN) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the built-in
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            //Predicate
            sb.Append(RDFModelUtilities.GetShortUri(((RDFResource)this.Predicate.Value).URI));

            //Arguments
            sb.Append($"({this.LeftArgument},{this.RightArgument})");

            return sb.ToString();
        }
        #endregion
    }
}