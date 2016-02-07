/*
   Copyright 2012-2016 Marco De Salvo

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

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOWLRuleSet represents a subset of standard OWL-DL ruleset
    /// </summary>
    public class RDFOWLRuleSet: RDFOntologyReasoningRuleSet {

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the singleton instance of the OWL ruleset
        /// </summary>
        internal RDFOWLRuleSet(): base("RDFOWL", "This ruleset implements a subset of OWL-DL entailment rules") {

            //EquivalentClassTransitivity
            this.AddRule(
                new RDFOntologyReasoningRule("EquivalentClassTransitivity",
                                             "EquivalentClassTransitivity implements possible paths of 'owl:EquivalentClass' entailment:" +
                                             "((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)",
                                             EquivalentClassTransitivity));

        }
        #endregion

        #region Methods

        #region Overrides
        /// <summary>
        /// Adds the given rule to the RDFS ruleset
        /// </summary>
        public override RDFOntologyReasoningRuleSet AddRule(RDFOntologyReasoningRule rule) {
            RDFSemanticsEvents.RaiseSemanticsInfo("Cannot add or remove rules from a standard ruleset (RDFOWL).");
            return this;
        }

        /// <summary>
        /// Removes the given rule from the RDFS ruleset
        /// </summary>
        public override RDFOntologyReasoningRuleSet RemoveRule(RDFOntologyReasoningRule rule) {
            RDFSemanticsEvents.RaiseSemanticsInfo("Cannot add or remove rules from a standard ruleset (RDFOWL).");
            return this;
        }
        #endregion

        #region Rules
        /// <summary>
        /// EquivalentClassTransitivity implements possible paths of 'owl:EquivalentClass' entailment:
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)
        /// </summary>
        internal static void EquivalentClassTransitivity(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var c       in ontology.Model.ClassModel) {
                foreach(var ec  in RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(c, ontology.Model.ClassModel)) {

                    //Create the inference as a taxonomy entry
                    var ecInferA = new RDFOntologyTaxonomyEntry(c,  RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_CLASS, ec).SetInference(true);
                    var ecInferB = new RDFOntologyTaxonomyEntry(ec, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_CLASS, c).SetInference(true);

                    //Enrich the class model with the inference
                    ontology.Model.ClassModel.Relations.EquivalentClass.AddEntry(ecInferA);
                    ontology.Model.ClassModel.Relations.EquivalentClass.AddEntry(ecInferB);

                    //Add the inference into the reasoning report
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "EquvalentClassTransitivity", ecInferA));
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "EquvalentClassTransitivity", ecInferB));

                }
            }
        }
        #endregion

        #endregion

    }

}