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

using RDFSharp.Query;
using System.Data;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRuleBuiltIn represents a predefined kind of atom filtering inferences of a rule's antecedent
    /// </summary>
    public abstract class RDFOntologyReasonerRuleBuiltIn : RDFOntologyReasonerRuleAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a built-in with given predicate and arguments
        /// </summary>
        internal RDFOntologyReasonerRuleBuiltIn(RDFOntologyResource predicate, RDFPatternMember leftArgument, RDFPatternMember rightArgument)
            : base(predicate, leftArgument, rightArgument) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the built-in in the context of the given antecedent results
        /// </summary>
        internal abstract DataTable Evaluate(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options);

        internal override DataTable EvaluateOnAntecedent(RDFOntology ontology, RDFOntologyReasonerOptions options) => null;
        internal override RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options) => null;
        #endregion
    }
}