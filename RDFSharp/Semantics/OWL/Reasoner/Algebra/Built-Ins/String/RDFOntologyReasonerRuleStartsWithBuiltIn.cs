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
using System.Text.RegularExpressions;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRuleStartsWithBuiltIn represents a built-in of type swrlb:startsWith
    /// </summary>
    public class RDFOntologyReasonerRuleStartsWithBuiltIn : RDFOntologyReasonerRuleFilterBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the Uri of the built-in (swrlb:startsWith)
        /// </summary>
        private static RDFResource BuiltInUri = new RDFResource($"swrlb:startsWith");
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a swrlb:startsWith built-in with given arguments
        /// </summary>
        public RDFOntologyReasonerRuleStartsWithBuiltIn(RDFVariable leftArgument, string startString)
            : base(new RDFOntologyResource() { Value = BuiltInUri }, leftArgument, null)
        {
            if (startString == null)
                throw new RDFSemanticsException("Cannot create built-in because given \"startString\" parameter is null.");

            //For printing, this built-in requires simulation of the right argument as plain literal
            this.RightArgument = new RDFOntologyLiteral(new RDFPlainLiteral(startString));

            this.BuiltInFilter = new RDFRegexFilter(leftArgument, new Regex($"^{startString}"));
        }
        #endregion
    }
}