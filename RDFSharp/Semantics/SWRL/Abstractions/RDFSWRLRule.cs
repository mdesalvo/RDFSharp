/*
   Copyright 2015-2020 Marco De Salvo

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

using RDFSharp.Semantics.OWL;
using System.Data;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLRule represents a rule expressed in SWRL
    /// </summary>
    public class RDFSWRLRule
    {
        #region Properties
        /// <summary>
        /// Antecedent of the SRWL rule
        /// </summary>
        public RDFSWRLRuleAntecedent Antecedent { get; internal set; }

        /// <summary>
        /// Consequent of the SRWL rule
        /// </summary>
        public RDFSWRLRuleConsequent Consequent { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a SWRL rule with given antecedent and consequent
        /// </summary>
        public RDFSWRLRule(RDFSWRLRuleAntecedent antecedent, RDFSWRLRuleConsequent consequent)
        {
            if (antecedent == null)
                throw new RDFSemanticsException("Cannot create SWRL rule because given \"antecedent\" parameter is null");

            if (consequent == null)
                throw new RDFSemanticsException("Cannot create SWRL rule because given \"consequent\" parameter is null");

            this.Antecedent = antecedent;
            this.Consequent = consequent;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the rule
        /// </summary>
        public override string ToString()
            => string.Concat(this.Antecedent, " ⇒ ", this.Consequent);
        #endregion

        #region Methods
        /// <summary>
        /// Applies the rule to the given ontology
        /// </summary>
        public RDFOntologyReasonerReport ApplyToOntology(RDFOntology ontology, RDFSWRLRuleOptions ruleOptions)
        {
            //Materialize results of the rule's antecedent
            DataTable antecedentResults = this.Antecedent.Evaluate(ontology, ruleOptions);

            //Materialize results of the rule's consequent
            RDFOntologyReasonerReport consequentResults = this.Consequent.Evaluate(antecedentResults, ontology, ruleOptions);
            return consequentResults;
        }
        #endregion
    }
}