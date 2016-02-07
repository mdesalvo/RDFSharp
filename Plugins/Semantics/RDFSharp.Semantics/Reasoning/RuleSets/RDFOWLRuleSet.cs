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

            //DisjointWithEntailment
            this.AddRule(
                new RDFOntologyReasoningRule("DisjointWithEntailment",
                                             "DisjointWithEntailment implements possible paths of 'owl:DisjointWith' entailment:" +
                                             "((C1 EQUIVALENTCLASS C2) AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)"      +
                                             "((C1 SUBCLASSOF C2)      AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)"      +
                                             "((C1 DISJOINTWITH C2)    AND (C2 EQUIVALENTCLASS C3)) => (C1 DISJOINTWITH C3)",
                                             DisjointWithEntailment));

            //EquivalentPropertyTransitivity
            this.AddRule(
                new RDFOntologyReasoningRule("EquivalentPropertyTransitivity",
                                             "EquivalentPropertyTransitivity implements possible paths of 'owl:EquivalentProperty' entailment:" +
                                             "((P1 EQUIVALENTPROPERTY P2) AND (P2 EQUIVALENTPROPERTY P3)) => (P1 EQUIVALENTPROPERTY P3)",
                                             EquivalentPropertyTransitivity));

            //SameAsTransitivity
            this.AddRule(
                new RDFOntologyReasoningRule("SameAsTransitivity",
                                             "SameAsTransitivity implements possible paths of 'owl:sameAs' entailment:" +
                                             "((F1 SAMEAS F2) AND (F2 SAMEAS F3)) => (F1 SAMEAS F3)",
                                             SameAsTransitivity));

            //DifferentFromEntailment
            this.AddRule(
                new RDFOntologyReasoningRule("DifferentFromEntailment",
                                             "DifferentFromEntailment implements possible paths of 'owl:DifferentFrom' entailment:" +
                                             "((F1 SAMEAS F2)        AND (F2 DIFFERENTFROM F3)) => (F1 DIFFERENTFROM F3)" +
                                             "((F1 DIFFERENTFROM F2) AND (F2 SAMEAS F3))        => (F1 DIFFERENTFROM F3)",
                                             DifferentFromEntailment));

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
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "EquivalentClassTransitivity", ecInferA));
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "EquivalentClassTransitivity", ecInferB));

                }
            }
        }

        /// <summary>
        /// DisjointWithEntailment implements possible paths of 'owl:DisjointWith' entailment:
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)
        /// ((C1 SUBCLASSOF C2)      AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)
        /// ((C1 DISJOINTWITH C2)    AND (C2 EQUIVALENTCLASS C3)) => (C1 DISJOINTWITH C3)
        /// </summary>
        internal static void DisjointWithEntailment(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var c       in ontology.Model.ClassModel) {
                foreach(var dwc in RDFOntologyReasoningHelper.EnlistDisjointClassesWith(c, ontology.Model.ClassModel)) {

                    //Create the inference as a taxonomy entry
                    var dcInferA = new RDFOntologyTaxonomyEntry(c,   RDFOntologyVocabulary.ObjectProperties.DISJOINT_WITH, dwc).SetInference(true);
                    var dcInferB = new RDFOntologyTaxonomyEntry(dwc, RDFOntologyVocabulary.ObjectProperties.DISJOINT_WITH, c).SetInference(true);

                    //Enrich the class model with the inference
                    ontology.Model.ClassModel.Relations.DisjointWith.AddEntry(dcInferA);
                    ontology.Model.ClassModel.Relations.DisjointWith.AddEntry(dcInferB);

                    //Add the inference into the reasoning report
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "DisjointWithEntailment", dcInferA));
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "DisjointWithEntailment", dcInferB));

                }
            }
        }

        /// <summary>
        /// EquivalentPropertyTransitivity implements possible paths of 'owl:EquivalentProperty' entailment:
        /// ((P1 EQUIVALENTPROPERTY P2) AND (P2 EQUIVALENTPROPERTY P3)) => (P1 EQUIVALENTPROPERTY P3)
        /// </summary>
        internal static void EquivalentPropertyTransitivity(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var p       in ontology.Model.PropertyModel) {
                foreach(var ep  in RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(p, ontology.Model.PropertyModel)) {

                    //Create the inference as a taxonomy entry
                    var epInferA = new RDFOntologyTaxonomyEntry(p,  RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, ep).SetInference(true);
                    var epInferB = new RDFOntologyTaxonomyEntry(ep, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, p).SetInference(true);

                    //Enrich the property model with the inference
                    ontology.Model.PropertyModel.Relations.EquivalentProperty.AddEntry(epInferA);
                    ontology.Model.PropertyModel.Relations.EquivalentProperty.AddEntry(epInferB);

                    //Add the inference into the reasoning report
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.PropertyModel, "EquivalentPropertyTransitivity", epInferA));
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.PropertyModel, "EquivalentPropertyTransitivity", epInferB));

                }
            }
        }

        /// <summary>
        /// SameAsTransitivity implements possible paths of 'owl:sameAs' entailment:
        /// ((F1 SAMEAS F2) AND (F2 SAMEAS F3)) => (F1 SAMEAS F3)
        /// </summary>
        internal static void SameAsTransitivity(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var f       in ontology.Data) {
                foreach(var sf  in RDFOntologyReasoningHelper.EnlistSameFactsAs(f, ontology.Data)) {

                    //Create the inference as a taxonomy entry
                    var sfInferA = new RDFOntologyTaxonomyEntry(f,  RDFOntologyVocabulary.ObjectProperties.SAME_AS, sf).SetInference(true);
                    var sfInferB = new RDFOntologyTaxonomyEntry(sf, RDFOntologyVocabulary.ObjectProperties.SAME_AS, f).SetInference(true);

                    //Enrich the data with the inference
                    ontology.Data.Relations.SameAs.AddEntry(sfInferA);
                    ontology.Data.Relations.SameAs.AddEntry(sfInferB);

                    //Add the inference into the reasoning report
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "SameAsTransitivity", sfInferA));
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "SameAsTransitivity", sfInferB));

                }
            }
        }

        /// <summary>
        /// DifferentFromEntailment implements possible paths of 'owl:DifferentFrom' entailment:
        /// ((F1 SAMEAS F2)        AND (F2 DIFFERENTFROM F3)) => (F1 DIFFERENTFROM F3)
        /// ((F1 DIFFERENTFROM F2) AND (F2 SAMEAS F3))        => (F1 DIFFERENTFROM F3)
        /// </summary>
        internal static void DifferentFromEntailment(RDFOntology ontology, RDFOntologyReasoningReport report) {
            foreach(var f       in ontology.Data) {
                foreach(var df  in RDFOntologyReasoningHelper.EnlistDifferentFactsFrom(f, ontology.Data)) {

                    //Create the inference as a taxonomy entry
                    var dfInferA = new RDFOntologyTaxonomyEntry(f,  RDFOntologyVocabulary.ObjectProperties.DIFFERENT_FROM, df).SetInference(true);
                    var dfInferB = new RDFOntologyTaxonomyEntry(df, RDFOntologyVocabulary.ObjectProperties.DIFFERENT_FROM, f).SetInference(true);

                    //Enrich the data with the inference
                    ontology.Data.Relations.DifferentFrom.AddEntry(dfInferA);
                    ontology.Data.Relations.DifferentFrom.AddEntry(dfInferB);

                    //Add the inference into the reasoning report
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "DifferentFromEntailment", dfInferA));
                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "DifferentFromEntailment", dfInferB));

                }
            }
        }
        #endregion

        #endregion

    }

}