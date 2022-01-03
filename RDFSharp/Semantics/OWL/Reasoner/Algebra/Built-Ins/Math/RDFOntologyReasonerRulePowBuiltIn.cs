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

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRulePowBuiltIn represents a math built-in of type swrlb:pow
    /// </summary>
    public class RDFOntologyReasonerRulePowBuiltIn : RDFOntologyReasonerRuleMathBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the Uri of the built-in (swrlb:pow)
        /// </summary>
        private static RDFResource BuiltInUri = new RDFResource($"swrlb:pow");
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a swrlb:pow built-in with given arguments
        /// </summary>
        public RDFOntologyReasonerRulePowBuiltIn(RDFVariable leftArgument, RDFVariable rightArgument, double expValue)
            : base(new RDFOntologyResource() { Value = BuiltInUri }, leftArgument, rightArgument, expValue) { }
        #endregion
    }
}