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

            //SubClassTransitivity (rdfs11)
            this.AddRule(
                new RDFOntologyReasoningRule("SubClassTransitivity",
                                             "SubClassTransitivity (rdfs11) implements possible paths of 'rdfs:subClassOf' subsumption:" +
                                             "((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)"               +
                                             "((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)"               +
                                             "((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)",
                                             SubClassTransitivity));

            //SubPropertyTransitivity (rdfs5)
            this.AddRule(
                new RDFOntologyReasoningRule("SubPropertyTransitivity",
                                             "SubPropertyTransitivity (rdfs5) implements possible paths of 'rdfs:subPropertyOf' subsumption:" +
                                             "((P1 SUBPROPERTYOF P2)      AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)"           +
                                             "((P1 SUBPROPERTYOF P2)      AND (P2 EQUIVALENTPROPERTY P3)) => (P1 SUBPROPERTYOF P3)"           +
                                             "((P1 EQUIVALENTPROPERTY P2) AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)",
                                             SubPropertyTransitivity));

            //ClassTypeEntailment (rdfs9)
            this.AddRule(
                new RDFOntologyReasoningRule("ClassTypeEntailment",
                                             "ClassTypeEntailment (rdfs9) implements possible paths of 'rdf:type' entailment:" +
                                             "((F TYPE C1) AND (C1 SUBCLASSOF C2))      => (F TYPE C2)"                        +
                                             "((F TYPE C1) AND (C1 EQUIVALENTCLASS C2)) => (F TYPE C2)",
                                             ClassTypeEntailment));

            //PropertyEntailment (rdfs7)
            this.AddRule(
                new RDFOntologyReasoningRule("PropertyEntailment",
                                             "PropertyEntailment (rdfs7) expands data assertions through 'rdfs:subPropertyOf' entailment:" +
                                             "((F1 P1 F2) AND (P1 SUBPROPERTYOF P2))      => (F1 P2 F2)"                                   +
                                             "((F1 P1 F2) AND (P1 EQUIVALENTPROPERTY P2)) => (F1 P2 F2)",
                                             PropertyEntailment));

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
        /// SubClassTransitivity (rdfs11) implements possible paths of 'rdfs:subClassOf' subsumption:
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
        /// SubPropertyTransitivity (rdfs5) implements possible paths of 'rdfs:subPropertyOf' subsumption:
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

        /// <summary>
        /// ClassTypeEntailment (rdfs9) implements possible paths of 'rdf:type' entailment:
        /// ((F TYPE C1) AND (C1 SUBCLASSOF C2))      => (F TYPE C2)
        /// ((F TYPE C1) AND (C1 EQUIVALENTCLASS C2)) => (F TYPE C2)
        /// </summary>
        internal static void ClassTypeEntailment(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var c      in ontology.Model.ClassModel) {
                foreach(var f  in RDFOntologyReasoningHelper.EnlistMembersOf(c, ontology)) {

                    //Create the inference as a taxonomy entry
                    var ctInfer = new RDFOntologyTaxonomyEntry(f,  RDFOntologyVocabulary.ObjectProperties.TYPE, c).SetInference(true);

                    //Enrich the data with the inference
                    ontology.Data.Relations.ClassType.AddEntry(ctInfer);

                    //Add the inference into the reasoning report
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "ClassTypeEntailment", ctInfer));

                }
            }
        }

        /// <summary>
        /// "PropertyEntailment (rdfs7) expands data assertions through 'rdfs:subPropertyOf' entailment:"
        /// "((F1 P1 F2) AND (P1 SUBPROPERTYOF P2))      => (F1 P2 F2)"
        /// "((F1 P1 F2) AND (P1 EQUIVALENTPROPERTY P2)) => (F1 P2 F2)"
        /// </summary>
        internal static void PropertyEntailment(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var p1     in ontology.Model.PropertyModel) {

                //Filter the assertions using the current property (F1 P1 F2)
                var p1Asns      = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p1);

                //Enlist the compatible properties of the current property (P1 [SUBPROPERTYOF|EQUIVALENTPROPERTY] P2)
                foreach(var p2 in RDFOntologyReasoningHelper.EnlistSuperPropertiesOf(p1, ontology.Model.PropertyModel)
                                       .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(p1, ontology.Model.PropertyModel))) {

                    //Iterate the compatible assertions
                    foreach(var p1Asn in p1Asns) {

                        //Taxonomy-check for securing inference consistency
                        if((p2.IsObjectProperty()   && p1Asn.TaxonomyObject.IsFact())     ||
                           (p2.IsDatatypeProperty() && p1Asn.TaxonomyObject.IsLiteral()))  {

                            //Create the inference as a taxonomy entry
                            var peInfer = new RDFOntologyTaxonomyEntry(p1Asn.TaxonomySubject, p2, p1Asn.TaxonomyObject).SetInference(true);

                            //Enrich the data with the inference
                            ontology.Data.Relations.Assertions.AddEntry(peInfer);

                            //Add the inference into the reasoning report
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "PropertyEntailment", peInfer));

                        }

                    }

                }

            }
        }
        #endregion

        #endregion

    }

}