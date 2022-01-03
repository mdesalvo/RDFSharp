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
    /// RDFOntologyReasonerRuleAddBuiltIn represents a math built-in of type swrlb:add
    /// </summary>
    public class RDFOntologyReasonerRuleAddBuiltIn : RDFOntologyReasonerRuleMathBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the Uri of the built-in (swrlb:add)
        /// </summary>
        private static RDFResource BuiltInUri = new RDFResource($"swrlb:add");
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a swrlb:add built-in with given arguments
        /// </summary>
        public RDFOntologyReasonerRuleAddBuiltIn(RDFVariable leftArgument, RDFVariable rightArgument, double addValue)
            : base(new RDFOntologyResource() { Value = BuiltInUri }, leftArgument, rightArgument, addValue) { }
        #endregion
    }
}