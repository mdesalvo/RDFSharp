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

using System;
using System.Data;
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasonerRule represents a reasoner rule expressed in SWRL
    /// </summary>
    public class RDFOntologyReasonerRule
    {
        #region Properties
        /// <summary>
        /// Uri of the rule
        /// </summary>
        public Uri RuleUri { get; internal set; }

        /// <summary>
        /// Description of the rule
        /// </summary>
        public string RuleDescription { get; internal set; }

        /// <summary>
        /// Antecedent of the rule
        /// </summary>
        public RDFOntologyReasonerRuleAntecedent Antecedent { get; internal set; }

        /// <summary>
        /// Consequent of the rule
        /// </summary>
        public RDFOntologyReasonerRuleConsequent Consequent { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a rule with given antecedent and consequent
        /// </summary>
        public RDFOntologyReasonerRule(Uri ruleUri, string ruleDescription, RDFOntologyReasonerRuleAntecedent antecedent, RDFOntologyReasonerRuleConsequent consequent)
        {
            if (ruleUri == null)
                throw new RDFSemanticsException("Cannot create rule because given \"ruleUri\" parameter is null");
            if (antecedent == null)
                throw new RDFSemanticsException("Cannot create rule because given \"antecedent\" parameter is null");
            if (consequent == null)
                throw new RDFSemanticsException("Cannot create rule because given \"consequent\" parameter is null");

            this.RuleUri = ruleUri;
            this.RuleDescription = ruleDescription;
            this.Antecedent = antecedent;
            this.Consequent = consequent;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the rule
        /// </summary>
        public override string ToString()
            => string.Concat(this.Antecedent, " -> ", this.Consequent);
        #endregion

        #region Methods
        /// <summary>
        /// Applies the rule to the given ontology
        /// </summary>
        public RDFOntologyReasonerReport ApplyToOntology(RDFOntology ontology)
            => ApplyToOntology(ontology, new RDFOntologyReasonerOptions());

        /// <summary>
        /// Applies the rule to the given ontology with the given options
        /// </summary>
        public RDFOntologyReasonerReport ApplyToOntology(RDFOntology ontology, RDFOntologyReasonerOptions options)
        {
            //Materialize results of the rule's antecedent
            DataTable antecedentResults = this.Antecedent.Evaluate(ontology, options);

            //Materialize results of the rule's consequent
            RDFOntologyReasonerReport consequentResults = this.Consequent.Evaluate(antecedentResults, ontology, options);
            return consequentResults;
        }

        /// <summary>
        /// Asynchronously applies the rule to the given ontology
        /// </summary>
        public Task<RDFOntologyReasonerReport> ApplyToOntologyAsync(RDFOntology ontology)
            => ApplyToOntologyAsync(ontology, new RDFOntologyReasonerOptions());

        /// <summary>
        /// Asynchronously applies the rule to the given ontology with the given options
        /// </summary>
        public Task<RDFOntologyReasonerReport> ApplyToOntologyAsync(RDFOntology ontology, RDFOntologyReasonerOptions options)
            => Task.Run(() => ApplyToOntology(ontology, options));
        #endregion
    }

}