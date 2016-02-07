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
    /// RDFSchemaRuleSet represents a subset of standard RDFS ruleset
    /// </summary>
    public class RDFSchemaRuleSet: RDFOntologyReasoningRuleSet {

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the singleton instance of the RDFS ruleset
        /// </summary>
        internal RDFSchemaRuleSet(): base("RDFSchema", "This ruleset implements a subset of RDFS entailment rules") {

            //SubClassTransitivity
            this.AddRule(
                new RDFOntologyReasoningRule("SubClassTransitivity",
                                             "SubClassTransitivity implements possible paths of 'rdfs:subClassOf' subsumption:" +
                                             "((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)"      +
                                             "((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)"      +
                                             "((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)",
                                             SubClassTransitivity));

            //SubPropertyTransitivity
            this.AddRule(
                new RDFOntologyReasoningRule("SubPropertyTransitivity",
                                             "SubPropertyTransitivity implements possible paths of 'rdfs:subPropertyOf' subsumption:" +
                                             "((P1 SUBPROPERTYOF P2)      AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)" +
                                             "((P1 SUBPROPERTYOF P2)      AND (P2 EQUIVALENTPROPERTY P3)) => (P1 SUBPROPERTYOF P3)" +
                                             "((P1 EQUIVALENTPROPERTY P2) AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)",
                                             SubPropertyTransitivity));

        }
        #endregion

        #region Methods

        #region Overrides
        /// <summary>
        /// Adds the given rule to the RDFS ruleset
        /// </summary>
        public override RDFOntologyReasoningRuleSet AddRule(RDFOntologyReasoningRule rule) {
            RDFSemanticsEvents.RaiseSemanticsInfo("Cannot add or remove rules from a standard ruleset (RDFSchema).");
            return this;
        }

        /// <summary>
        /// Removes the given rule from the RDFS ruleset
        /// </summary>
        public override RDFOntologyReasoningRuleSet RemoveRule(RDFOntologyReasoningRule rule) {
            RDFSemanticsEvents.RaiseSemanticsInfo("Cannot add or remove rules from a standard ruleset (RDFSchema).");
            return this;
        }
        #endregion

        #region Rules
        /// <summary>
        /// SubClassTransitivity implements possible paths of 'rdfs:subClassOf' subsumption:
        /// ((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)
        /// ((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)
        /// </summary>
        internal static void SubClassTransitivity(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var c      in ontology.Model.ClassModel) {
                foreach(var sc in RDFOntologyReasoningHelper.EnlistSuperClassesOf(c, ontology.Model.ClassModel)) {

                    //Create the inference as a taxonomy entry
                    var scInfer = new RDFOntologyTaxonomyEntry(c, RDFOntologyVocabulary.ObjectProperties.SUB_CLASS_OF, sc).SetInference(true);

                    //Enrich the class model with the inference
                    ontology.Model.ClassModel.Relations.SubClassOf.AddEntry(scInfer);

                    //Add the inference into the reasoning report
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "SubClassTransitivity", scInfer));

                }
            }
        }

        /// <summary>
        /// SubPropertyTransitivity implements possible paths of 'rdfs:subPropertyOf' subsumption:
        /// ((P1 SUBPROPERTYOF P2)      AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)
        /// ((P1 SUBPROPERTYOF P2)      AND (P2 EQUIVALENTPROPERTY P3)) => (P1 SUBPROPERTYOF P3)
        /// ((P1 EQUIVALENTPROPERTY P2) AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)
        /// </summary>
        internal static void SubPropertyTransitivity(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var p in ontology.Model.PropertyModel) {
                foreach(var sp in RDFOntologyReasoningHelper.EnlistSuperPropertiesOf(p, ontology.Model.PropertyModel)) {

                    //Create the inference as a taxonomy entry
                    var spInfer = new RDFOntologyTaxonomyEntry(p, RDFOntologyVocabulary.ObjectProperties.SUB_PROPERTY_OF, sp).SetInference(true);

                    //Enrich the property model with the inference
                    ontology.Model.PropertyModel.Relations.SubPropertyOf.AddEntry(spInfer);

                    //Add the inference into the reasoning report
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.PropertyModel, "SubPropertyTransitivity", spInfer));

                }
            }
        }
        #endregion

        #endregion

    }

}