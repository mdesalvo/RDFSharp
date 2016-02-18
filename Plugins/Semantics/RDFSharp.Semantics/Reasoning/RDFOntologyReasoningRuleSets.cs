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

using System;
using System.Collections.Generic;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoningRuleSets represents a collection of predefined rulesets available to reasoners.
    /// </summary>
    public static class RDFOntologyReasoningRuleSets {

        #region RDFS
        /// <summary>
        /// RDFSRuleset implements a subset of RDFS entailment rules
        /// </summary>
        public static class RDFSRuleSet {

            #region Properties
            /// <summary>
            /// SubClassTransitivity (rdfs11) implements structural entailments based on 'rdfs:subClassOf' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule SubClassTransitivity { get; internal set; }

            /// <summary>
            /// SubPropertyTransitivity (rdfs5) implements structural entailments based on 'rdfs:subPropertyOf' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule SubPropertyTransitivity { get; internal set; }

            /// <summary>
            /// ClassTypeEntailment (rdfs9) implements data entailments based on 'rdf:type' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule ClassTypeEntailment { get; internal set; }

            /// <summary>
            /// PropertyEntailment (rdfs7) implements structural entailments based on 'rdfs:subPropertyOf' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule PropertyEntailment { get; internal set; }

            /// <summary>
            /// DomainEntailment (rdfs2) implements structural entailments based on 'rdfs:domain' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule DomainEntailment { get; internal set; }

            /// <summary>
            /// RangeEntailment (rdfs3) implements structural entailments based on 'rdfs:range' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule RangeEntailment { get; internal set; }
            #endregion

            #region Ctors
            /// <summary>
            /// Static-ctor to initialize the RDFS ruleset
            /// </summary>
            static RDFSRuleSet() {

                //SubClassTransitivity (rdfs11)
                SubClassTransitivity    = new RDFOntologyReasoningRule("SubClassTransitivity",
                                                                       "SubClassTransitivity (rdfs11) implements structural entailments based on 'rdfs:subClassOf' taxonomy:" +
                                                                       "((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)" +
                                                                       "((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)" +
                                                                       "((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)",
                                                                       SubClassTransitivityExec);

                //SubPropertyTransitivity (rdfs5)
                SubPropertyTransitivity = new RDFOntologyReasoningRule("SubPropertyTransitivity",
                                                                       "SubPropertyTransitivity (rdfs5) implements structural entailments based on 'rdfs:subPropertyOf' taxonomy:" +
                                                                       "((P1 SUBPROPERTYOF P2)      AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)" +
                                                                       "((P1 SUBPROPERTYOF P2)      AND (P2 EQUIVALENTPROPERTY P3)) => (P1 SUBPROPERTYOF P3)" +
                                                                       "((P1 EQUIVALENTPROPERTY P2) AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)",
                                                                       SubPropertyTransitivityExec);

                //ClassTypeEntailment (rdfs9)
                ClassTypeEntailment     = new RDFOntologyReasoningRule("ClassTypeEntailment",
                                                                       "ClassTypeEntailment (rdfs9) implements structural entailments based on 'rdf:type' taxonomy:" +
                                                                       "((F TYPE C1) AND (C1 SUBCLASSOF C2))      => (F TYPE C2)" +
                                                                       "((F TYPE C1) AND (C1 EQUIVALENTCLASS C2)) => (F TYPE C2)",
                                                                       ClassTypeEntailmentExec);

                //PropertyEntailment (rdfs7)
                PropertyEntailment      = new RDFOntologyReasoningRule("PropertyEntailment",
                                                                       "PropertyEntailment (rdfs7) implements data entailments based on 'rdfs:subPropertyOf' taxonomy:" +
                                                                       "((F1 P1 F2) AND (P1 SUBPROPERTYOF P2))      => (F1 P2 F2)" +
                                                                       "((F1 P1 F2) AND (P1 EQUIVALENTPROPERTY P2)) => (F1 P2 F2)",
                                                                       PropertyEntailmentExec);

                //DomainEntailment (rdfs2)
                DomainEntailment        = new RDFOntologyReasoningRule("DomainEntailment",
                                                                       "DomainEntailment (rdfs2) implements structural entailments based on 'rdfs:domain' taxonomy:" +
                                                                       "((F1 P F2) AND (P RDFS:DOMAIN C)) => (F1 RDF:TYPE C)",
                                                                       DomainEntailmentExec);

                //RangeEntailment (rdfs3)
                RangeEntailment         = new RDFOntologyReasoningRule("RangeEntailment",
                                                                       "RangeEntailment (rdfs2) implements structural entailments based on 'rdfs:range' taxonomy:" +
                                                                       "((F1 P F2) AND (P RDFS:RANGE C)) => (F2 RDF:TYPE C)",
                                                                       RangeEntailmentExec);

            }
            #endregion

            #region Methods
            /// <summary>
            /// SubClassTransitivity (rdfs11) implements structural entailments based on 'rdfs:subClassOf' taxonomy:
            /// ((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)
            /// ((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)
            /// ((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)
            /// </summary>
            internal static void SubClassTransitivityExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var c       in ontology.Model.ClassModel) {
                    foreach(var sc  in RDFOntologyReasoningHelper.EnlistSuperClassesOf(c, ontology.Model.ClassModel)) {

                        //Create the inference as a taxonomy entry
                        var scInfer  = new RDFOntologyTaxonomyEntry(c, RDFOntologyVocabulary.ObjectProperties.SUB_CLASS_OF, sc).SetInference(true);

                        //Enrich the class model with the inference
                        var taxCnt   = ontology.Model.ClassModel.Relations.SubClassOf.EntriesCount;
                        ontology.Model.ClassModel.Relations.SubClassOf.AddEntry(scInfer);

                        //Add the inference into the reasoning report
                        if (ontology.Model.ClassModel.Relations.SubClassOf.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "SubClassTransitivity", scInfer));
                        }

                    }
                }
            }

            /// <summary>
            /// SubPropertyTransitivity (rdfs5) implements structural entailments based on 'rdfs:subPropertyOf' taxonomy:
            /// ((P1 SUBPROPERTYOF P2)      AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)
            /// ((P1 SUBPROPERTYOF P2)      AND (P2 EQUIVALENTPROPERTY P3)) => (P1 SUBPROPERTYOF P3)
            /// ((P1 EQUIVALENTPROPERTY P2) AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)
            /// </summary>
            internal static void SubPropertyTransitivityExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var p       in ontology.Model.PropertyModel) {
                    foreach(var sp  in RDFOntologyReasoningHelper.EnlistSuperPropertiesOf(p, ontology.Model.PropertyModel)) {

                        //Create the inference as a taxonomy entry
                        var spInfer  = new RDFOntologyTaxonomyEntry(p, RDFOntologyVocabulary.ObjectProperties.SUB_PROPERTY_OF, sp).SetInference(true);

                        //Enrich the property model with the inference
                        var taxCnt   = ontology.Model.PropertyModel.Relations.SubPropertyOf.EntriesCount;
                        ontology.Model.PropertyModel.Relations.SubPropertyOf.AddEntry(spInfer);

                        //Add the inference into the reasoning report
                        if (ontology.Model.PropertyModel.Relations.SubPropertyOf.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.PropertyModel, "SubPropertyTransitivity", spInfer));
                        }

                    }
                }
            }

            /// <summary>
            /// ClassTypeEntailment (rdfs9) implements structural entailments based on 'rdf:type' taxonomy:
            /// ((F TYPE C1) AND (C1 SUBCLASSOF C2))      => (F TYPE C2)
            /// ((F TYPE C1) AND (C1 EQUIVALENTCLASS C2)) => (F TYPE C2)
            /// </summary>
            internal static void ClassTypeEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var c          in ontology.Model.ClassModel) {
                    if(!RDFOntologyReasoningHelper.IsLiteralCompatibleClass(c, ontology.Model.ClassModel)) {
                        foreach(var f  in RDFOntologyReasoningHelper.EnlistMembersOf(c, ontology)) {

                            //Create the inference as a taxonomy entry
                            var ctInfer = new RDFOntologyTaxonomyEntry(f,  RDFOntologyVocabulary.ObjectProperties.TYPE, c).SetInference(true);

                            //Enrich the data with the inference
                            var taxCnt  = ontology.Data.Relations.ClassType.EntriesCount;
                            ontology.Data.Relations.ClassType.AddEntry(ctInfer);

                            //Add the inference into the reasoning report
                            if (ontology.Data.Relations.ClassType.EntriesCount > taxCnt) {
                                report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "ClassTypeEntailment", ctInfer));
                            }

                        }
                    }
                }
            }

            /// <summary>
            /// "PropertyEntailment (rdfs7) implements data entailments based on 'rdfs:subPropertyOf' taxonomy:"
            /// "((F1 P1 F2) AND (P1 SUBPROPERTYOF P2))      => (F1 P2 F2)"
            /// "((F1 P1 F2) AND (P1 EQUIVALENTPROPERTY P2)) => (F1 P2 F2)"
            /// </summary>
            internal static void PropertyEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var p1             in ontology.Model.PropertyModel) {

                    //Filter the assertions using the current property (F1 P1 F2)
                    var p1Asns              = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p1);

                    //Enlist the compatible properties of the current property (P1 [SUBPROPERTYOF|EQUIVALENTPROPERTY] P2)
                    foreach(var p2         in RDFOntologyReasoningHelper.EnlistSuperPropertiesOf(p1, ontology.Model.PropertyModel)
                                                    .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(p1, ontology.Model.PropertyModel))) {

                        //Iterate the compatible assertions
                        foreach(var p1Asn  in p1Asns) {

                            //Taxonomy-check for securing inference consistency
                            if((p2.IsObjectProperty()   && p1Asn.TaxonomyObject.IsFact())    ||
                               (p2.IsDatatypeProperty() && p1Asn.TaxonomyObject.IsLiteral())) {

                                //Create the inference as a taxonomy entry
                                var peInfer = new RDFOntologyTaxonomyEntry(p1Asn.TaxonomySubject, p2, p1Asn.TaxonomyObject).SetInference(true);

                                //Enrich the data with the inference
                                var taxCnt  = ontology.Data.Relations.Assertions.EntriesCount;
                                ontology.Data.Relations.Assertions.AddEntry(peInfer);

                                //Add the inference into the reasoning report
                                if (ontology.Data.Relations.Assertions.EntriesCount > taxCnt) {
                                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "PropertyEntailment", peInfer));
                                }

                            }

                        }

                    }

                }
            }

            /// <summary>
            /// "DomainEntailment (rdfs2) implements structural entailments based on 'rdfs:domain' taxonomy:"
            /// "((F1 P F2) AND (P RDFS:DOMAIN C)) => (F1 RDF:TYPE C)"
            /// </summary>
            internal static void DomainEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var p            in ontology.Model.PropertyModel) {
                    if (p.Domain          != null) {

                        //Filter the assertions using the current property (F1 P1 F2)
                        var pAsns         = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                        //Iterate the related assertions
                        foreach(var pAsn in pAsns) {

                            //Create the inference as a taxonomy entry
                            var deInfer   = new RDFOntologyTaxonomyEntry(pAsn.TaxonomySubject, RDFOntologyVocabulary.ObjectProperties.TYPE, p.Domain).SetInference(true);

                            //Enrich the data with the inference
                            var taxCnt    = ontology.Data.Relations.ClassType.EntriesCount;
                            ontology.Data.Relations.ClassType.AddEntry(deInfer);

                            //Add the inference into the reasoning report
                            if (ontology.Data.Relations.ClassType.EntriesCount > taxCnt) {
                                report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "DomainEntailment", deInfer));
                            }

                        }

                    }

                }
            }

            /// <summary>
            /// "RangeEntailment (rdfs3) implements structural entailments based on 'rdfs:range' taxonomy:"
            /// "((F1 P F2) AND (P RDFS:RANGE C)) => (F2 RDF:TYPE C)"
            /// </summary>
            internal static void RangeEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var p              in ontology.Model.PropertyModel) {
                    if (p.Range             != null && p.IsObjectProperty()) {

                        //Filter the assertions using the current property (F1 P1 F2)
                        var pAsns           = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                        //Iterate the related assertions
                        foreach(var pAsn   in pAsns) {

                            //Taxonomy-check for securing inference consistency
                            if (pAsn.TaxonomyObject.IsFact()) {

                                //Create the inference as a taxonomy entry
                                var reInfer = new RDFOntologyTaxonomyEntry(pAsn.TaxonomyObject, RDFOntologyVocabulary.ObjectProperties.TYPE, p.Range).SetInference(true);

                                //Enrich the data with the inference
                                var taxCnt  = ontology.Data.Relations.ClassType.EntriesCount;
                                ontology.Data.Relations.ClassType.AddEntry(reInfer);

                                //Add the inference into the reasoning report
                                if (ontology.Data.Relations.ClassType.EntriesCount > taxCnt) {
                                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "RangeEntailment", reInfer));
                                }

                            }

                        }

                    }

                }
            }
            #endregion

        }
        #endregion

        #region OWL-DL
        /// <summary>
        /// RDFOWLRuleSet implements a subset of OWL-DL entailment rules
        /// </summary>
        public static class RDFOWLRuleSet {

            #region Properties
            /// <summary>
            /// EquivalentClassTransitivity implements structural entailments based on 'owl:EquivalentClass' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule EquivalentClassTransitivity { get; internal set; }

            /// <summary>
            /// DisjointWithEntailment implements structural entailments based on 'owl:DisjointWith' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule DisjointWithEntailment { get; internal set; }

            /// <summary>
            /// EquivalentPropertyTransitivity implements structural entailments based on 'owl:EquivalentProperty' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule EquivalentPropertyTransitivity { get; internal set; }

            /// <summary>
            /// SameAsTransitivity implements structural entailments based on 'owl:SameAs' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule SameAsTransitivity { get; internal set; }

            /// <summary>
            /// DifferentFromEntailment implements structural entailments based on 'owl:DifferentFrom' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule DifferentFromEntailment { get; internal set; }

            /// <summary>
            /// InverseOfEntailment implements data entailments based on 'owl:inverseOf' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule InverseOfEntailment { get; internal set; }

            /// <summary>
            /// SameAsEntailment implements data entailments based on 'owl:SameAs' taxonomy
            /// </summary>
            public static RDFOntologyReasoningRule SameAsEntailment { get; internal set; }

            /// <summary>
            /// SymmetricPropertyEntailment implements data entailments based on 'owl:SymmetricProperty' axiom
            /// </summary>
            public static RDFOntologyReasoningRule SymmetricPropertyEntailment { get; internal set; }

            /// <summary>
            /// TransitivePropertyEntailment implements data entailments based on 'owl:TransitiveProperty' axiom
            /// </summary>
            public static RDFOntologyReasoningRule TransitivePropertyEntailment { get; internal set; }
            #endregion

            #region Ctors
            /// <summary>
            /// Static-ctor to initialize the OWL-DL ruleset
            /// </summary>
            static RDFOWLRuleSet() {

                //EquivalentClassTransitivity
                EquivalentClassTransitivity    = new RDFOntologyReasoningRule("EquivalentClassTransitivity",
                                                                              "EquivalentClassTransitivity implements structural entailments based on 'owl:EquivalentClass' taxonomy:" +
                                                                              "((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)",
                                                                              EquivalentClassTransitivityExec);

                //DisjointWithEntailment
                DisjointWithEntailment         = new RDFOntologyReasoningRule("DisjointWithEntailment",
                                                                              "DisjointWithEntailment implements structural entailments based on 'owl:DisjointWith' taxonomy:" +
                                                                              "((C1 EQUIVALENTCLASS C2) AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)" +
                                                                              "((C1 SUBCLASSOF C2)      AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)" +
                                                                              "((C1 DISJOINTWITH C2)    AND (C2 EQUIVALENTCLASS C3)) => (C1 DISJOINTWITH C3)",
                                                                              DisjointWithEntailmentExec);

                //EquivalentPropertyTransitivity
                EquivalentPropertyTransitivity = new RDFOntologyReasoningRule("EquivalentPropertyTransitivity",
                                                                              "EquivalentPropertyTransitivity implements structural entailments based on 'owl:EquivalentProperty' taxonomy:" +
                                                                              "((P1 EQUIVALENTPROPERTY P2) AND (P2 EQUIVALENTPROPERTY P3)) => (P1 EQUIVALENTPROPERTY P3)",
                                                                              EquivalentPropertyTransitivityExec);

                //SameAsTransitivity
                SameAsTransitivity             = new RDFOntologyReasoningRule("SameAsTransitivity",
                                                                              "SameAsTransitivity implements structural entailments based on 'owl:SameAs' taxonomy:" +
                                                                              "((F1 SAMEAS F2) AND (F2 SAMEAS F3)) => (F1 SAMEAS F3)",
                                                                              SameAsTransitivityExec);

                //DifferentFromEntailment
                DifferentFromEntailment        = new RDFOntologyReasoningRule("DifferentFromEntailment",
                                                                              "DifferentFromEntailment implements structural entailments based on 'owl:DifferentFrom' taxonomy:" +
                                                                              "((F1 SAMEAS F2)        AND (F2 DIFFERENTFROM F3)) => (F1 DIFFERENTFROM F3)" +
                                                                              "((F1 DIFFERENTFROM F2) AND (F2 SAMEAS F3))        => (F1 DIFFERENTFROM F3)",
                                                                              DifferentFromEntailmentExec);

                //InverseOfEntailment
                InverseOfEntailment            = new RDFOntologyReasoningRule("InverseOfEntailment",
                                                                              "InverseOfEntailment implements data entailments based on 'owl:inverseOf' taxonomy:" +
                                                                              "((F1 P1 F2) AND (P1 INVERSEOF P2)) => (F2 P2 F1)",
                                                                              InverseOfEntailmentExec);

                //SameAsEntailment
                SameAsEntailment               = new RDFOntologyReasoningRule("SameAsEntailment",
                                                                              "SameAsEntailment implements data entailments based on 'owl:SameAs' taxonomy:" +
                                                                              "((F1 P F2) AND (F1 SAMEAS F3)) => (F3 P F2)" +
                                                                              "((F1 P F2) AND (F2 SAMEAS F3)) => (F1 P F3)",
                                                                              SameAsEntailmentExec);

                //SymmetricPropertyEntailment
                SymmetricPropertyEntailment    = new RDFOntologyReasoningRule("SymmetricPropertyEntailment",
                                                                              "SymmetricPropertyEntailment implements data entailments based on 'owl:SymmetricProperty' axiom:" +
                                                                              "((F1 P F2) AND (P TYPE SYMMETRICPROPERTY)) => (F2 P F1)",
                                                                              SymmetricPropertyEntailmentExec);

                //TransitivePropertyEntailment
                TransitivePropertyEntailment   = new RDFOntologyReasoningRule("TransitivePropertyEntailment",
                                                                              "TransitivePropertyEntailment implements data entailments based on 'owl:TransitiveProperty' axiom:" +
                                                                              "((F1 P F2) AND (F2 P F3) AND (P TYPE TRANSITIVEPROPERTY)) => (F1 P F3)",
                                                                              TransitivePropertyEntailmentExec);

            }
            #endregion

            #region Methods
            /// <summary>
            /// EquivalentClassTransitivity implements structural entailments based on 'owl:EquivalentClass' taxonomy:
            /// ((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)
            /// </summary>
            internal static void EquivalentClassTransitivityExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var c       in ontology.Model.ClassModel) {
                    foreach(var ec  in RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(c, ontology.Model.ClassModel)) {

                        //Create the inference as a taxonomy entry
                        var ecInferA = new RDFOntologyTaxonomyEntry(c,  RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_CLASS, ec).SetInference(true);
                        var ecInferB = new RDFOntologyTaxonomyEntry(ec, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_CLASS, c).SetInference(true);

                        //Enrich the class model with the inference
                        var taxCnt   = ontology.Model.ClassModel.Relations.EquivalentClass.EntriesCount;
                        ontology.Model.ClassModel.Relations.EquivalentClass.AddEntry(ecInferA);

                        //Add the inference into the reasoning report
                        if (ontology.Model.ClassModel.Relations.EquivalentClass.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "EquivalentClassTransitivity", ecInferA));
                        }

                        //Exploit symmetry of EquivalentClass relation
                        taxCnt       = ontology.Model.ClassModel.Relations.EquivalentClass.EntriesCount;
                        ontology.Model.ClassModel.Relations.EquivalentClass.AddEntry(ecInferB);
                        if (ontology.Model.ClassModel.Relations.EquivalentClass.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "EquivalentClassTransitivity", ecInferB));
                        }

                    }
                }
            }

            /// <summary>
            /// DisjointWithEntailment implements structural entailments based on 'owl:DisjointWith' taxonomy:
            /// ((C1 EQUIVALENTCLASS C2) AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)
            /// ((C1 SUBCLASSOF C2)      AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)
            /// ((C1 DISJOINTWITH C2)    AND (C2 EQUIVALENTCLASS C3)) => (C1 DISJOINTWITH C3)
            /// </summary>
            internal static void DisjointWithEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var c       in ontology.Model.ClassModel) {
                    foreach(var dwc in RDFOntologyReasoningHelper.EnlistDisjointClassesWith(c, ontology.Model.ClassModel)) {

                        //Create the inference as a taxonomy entry
                        var dcInferA = new RDFOntologyTaxonomyEntry(c,   RDFOntologyVocabulary.ObjectProperties.DISJOINT_WITH, dwc).SetInference(true);
                        var dcInferB = new RDFOntologyTaxonomyEntry(dwc, RDFOntologyVocabulary.ObjectProperties.DISJOINT_WITH, c).SetInference(true);

                        //Enrich the class model with the inference
                        var taxCnt   = ontology.Model.ClassModel.Relations.DisjointWith.EntriesCount;
                        ontology.Model.ClassModel.Relations.DisjointWith.AddEntry(dcInferA);

                        //Add the inference into the reasoning report
                        if (ontology.Model.ClassModel.Relations.DisjointWith.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "DisjointWithEntailment", dcInferA));
                        }

                        //Exploit symmetry of DisjointWith relation
                        taxCnt       = ontology.Model.ClassModel.Relations.DisjointWith.EntriesCount;
                        ontology.Model.ClassModel.Relations.DisjointWith.AddEntry(dcInferB);
                        if (ontology.Model.ClassModel.Relations.DisjointWith.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel, "DisjointWithEntailment", dcInferB));
                        }

                    }
                }
            }

            /// <summary>
            /// EquivalentPropertyTransitivity implements structural entailments based on 'owl:EquivalentProperty' taxonomy:
            /// ((P1 EQUIVALENTPROPERTY P2) AND (P2 EQUIVALENTPROPERTY P3)) => (P1 EQUIVALENTPROPERTY P3)
            /// </summary>
            internal static void EquivalentPropertyTransitivityExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var p       in ontology.Model.PropertyModel) {
                    foreach(var ep  in RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(p, ontology.Model.PropertyModel)) {

                        //Create the inference as a taxonomy entry
                        var epInferA = new RDFOntologyTaxonomyEntry(p,  RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, ep).SetInference(true);
                        var epInferB = new RDFOntologyTaxonomyEntry(ep, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, p).SetInference(true);

                        //Enrich the property model with the inference
                        var taxCnt   = ontology.Model.PropertyModel.Relations.EquivalentProperty.EntriesCount;
                        ontology.Model.PropertyModel.Relations.EquivalentProperty.AddEntry(epInferA);

                        //Add the inference into the reasoning report
                        if (ontology.Model.PropertyModel.Relations.EquivalentProperty.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.PropertyModel, "EquivalentPropertyTransitivity", epInferA));
                        }

                        //Exploit symmetry of EquivalentProperty relation
                        taxCnt       = ontology.Model.PropertyModel.Relations.EquivalentProperty.EntriesCount;
                        ontology.Model.PropertyModel.Relations.EquivalentProperty.AddEntry(epInferB);
                        if (ontology.Model.PropertyModel.Relations.EquivalentProperty.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.PropertyModel, "EquivalentPropertyTransitivity", epInferB));
                        }

                    }
                }
            }

            /// <summary>
            /// SameAsTransitivity implements structural entailments based on 'owl:sameAs' taxonomy:
            /// ((F1 SAMEAS F2) AND (F2 SAMEAS F3)) => (F1 SAMEAS F3)
            /// </summary>
            internal static void SameAsTransitivityExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var f       in ontology.Data) {
                    foreach(var sf  in RDFOntologyReasoningHelper.EnlistSameFactsAs(f, ontology.Data)) {

                        //Create the inference as a taxonomy entry
                        var sfInferA = new RDFOntologyTaxonomyEntry(f,  RDFOntologyVocabulary.ObjectProperties.SAME_AS, sf).SetInference(true);
                        var sfInferB = new RDFOntologyTaxonomyEntry(sf, RDFOntologyVocabulary.ObjectProperties.SAME_AS, f).SetInference(true);

                        //Enrich the data with the inference
                        var taxCnt   = ontology.Data.Relations.SameAs.EntriesCount;
                        ontology.Data.Relations.SameAs.AddEntry(sfInferA);

                        //Add the inference into the reasoning report
                        if (ontology.Data.Relations.SameAs.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "SameAsTransitivity", sfInferA));
                        }

                        //Exploit symmetry of SameAs relation
                        taxCnt       = ontology.Data.Relations.SameAs.EntriesCount;
                        ontology.Data.Relations.SameAs.AddEntry(sfInferB);
                        if (ontology.Data.Relations.SameAs.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "SameAsTransitivity", sfInferB));
                        }

                    }
                }
            }

            /// <summary>
            /// DifferentFromEntailment implements structural entailments based on 'owl:DifferentFrom' taxonomy:
            /// ((F1 SAMEAS F2)        AND (F2 DIFFERENTFROM F3)) => (F1 DIFFERENTFROM F3)
            /// ((F1 DIFFERENTFROM F2) AND (F2 SAMEAS F3))        => (F1 DIFFERENTFROM F3)
            /// </summary>
            internal static void DifferentFromEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var f       in ontology.Data) {
                    foreach(var df  in RDFOntologyReasoningHelper.EnlistDifferentFactsFrom(f, ontology.Data)) {

                        //Create the inference as a taxonomy entry
                        var dfInferA = new RDFOntologyTaxonomyEntry(f,  RDFOntologyVocabulary.ObjectProperties.DIFFERENT_FROM, df).SetInference(true);
                        var dfInferB = new RDFOntologyTaxonomyEntry(df, RDFOntologyVocabulary.ObjectProperties.DIFFERENT_FROM, f).SetInference(true);

                        //Enrich the data with the inference
                        var taxCnt   = ontology.Data.Relations.DifferentFrom.EntriesCount;
                        ontology.Data.Relations.DifferentFrom.AddEntry(dfInferA);

                        //Add the inference into the reasoning report
                        if (ontology.Data.Relations.DifferentFrom.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "DifferentFromEntailment", dfInferA));
                        }

                        //Exploit symmetry of DifferentFrom relation
                        taxCnt       = ontology.Data.Relations.DifferentFrom.EntriesCount;
                        ontology.Data.Relations.DifferentFrom.AddEntry(dfInferB);
                        if (ontology.Data.Relations.DifferentFrom.EntriesCount > taxCnt) {
                            report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "DifferentFromEntailment", dfInferB));
                        }

                    }
                }
            }

            /// <summary>
            /// InverseOfEntailment implements data entailments based on 'owl:inverseOf' taxonomy:
            /// ((F1 P1 F2) AND (P1 INVERSEOF P2)) => (F2 P2 F1)
            /// </summary>
            internal static void InverseOfEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                var objPropsEnum            = ontology.Model.PropertyModel.ObjectPropertiesEnumerator;
                while(objPropsEnum.MoveNext()) {
                    var p1                  = objPropsEnum.Current;

                    //Filter the assertions using the current property (F1 P1 F2)
                    var p1Asns              = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p1);

                    //Enlist the inverse properties of the current property
                    foreach(var p2         in RDFOntologyReasoningHelper.EnlistInversePropertiesOf((RDFOntologyObjectProperty)p1, ontology.Model.PropertyModel)) {

                        //Iterate the compatible assertions
                        foreach(var p1Asn  in p1Asns) {

                            //Taxonomy-check for securing inference consistency
                            if (p2.IsObjectProperty() && p1Asn.TaxonomyObject.IsFact()) {

                                //Create the inference as a taxonomy entry
                                var ioInfer = new RDFOntologyTaxonomyEntry(p1Asn.TaxonomyObject, p2, p1Asn.TaxonomySubject).SetInference(true);

                                //Enrich the data with the inference
                                var taxCnt  = ontology.Data.Relations.Assertions.EntriesCount;
                                ontology.Data.Relations.Assertions.AddEntry(ioInfer);

                                //Add the inference into the reasoning report
                                if (ontology.Data.Relations.Assertions.EntriesCount > taxCnt) {
                                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "InverseOfEntailment", ioInfer));
                                }

                            }

                        }

                    }

                }
            }

            /// <summary>
            /// SameAsEntailment implements data entailments based on 'owl:sameAs' taxonomy:
            /// ((F1 P F2) AND (F1 SAMEAS F3)) => (F3 P F2)
            /// ((F1 P F2) AND (F2 SAMEAS F3)) => (F1 P F3)
            /// </summary>
            internal static void SameAsEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                foreach(var f1                 in ontology.Data) {
                    var sameFacts               = RDFOntologyReasoningHelper.EnlistSameFactsAs(f1, ontology.Data);
                    if (sameFacts.FactsCount    > 0) {

                        //Filter the assertions using the current fact
                        var f1AsnsSubj          = ontology.Data.Relations.Assertions.SelectEntriesBySubject(f1);
                        var f1AsnsObj           = ontology.Data.Relations.Assertions.SelectEntriesByObject(f1);

                        //Enlist the same facts of the current fact
                        foreach(var f2         in sameFacts) {

                            #region Subject-Side
                            //Iterate the assertions having the current fact as subject
                            foreach(var f1Asn  in f1AsnsSubj) {

                                //Taxonomy-check for securing inference consistency
                                if (f1Asn.TaxonomyPredicate.IsObjectProperty() && f1Asn.TaxonomyObject.IsFact()) {

                                    //Create the inference as a taxonomy entry
                                    var saInfer = new RDFOntologyTaxonomyEntry(f2, f1Asn.TaxonomyPredicate, f1Asn.TaxonomyObject).SetInference(true);

                                    //Enrich the data with the inference
                                    var taxCnt  = ontology.Data.Relations.Assertions.EntriesCount;
                                    ontology.Data.Relations.Assertions.AddEntry(saInfer);

                                    //Add the inference into the reasoning report
                                    if (ontology.Data.Relations.Assertions.EntriesCount > taxCnt) {
                                        report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "SameAsEntailment", saInfer));
                                    }

                                }

                            }
                            #endregion

                            #region Object-Side
                            //Iterate the assertions having the current fact as object
                            foreach(var f1Asn  in f1AsnsObj) {

                                //Taxonomy-check for securing inference consistency
                                if (f1Asn.TaxonomyPredicate.IsObjectProperty() && f2.IsFact()) {

                                    //Create the inference as a taxonomy entry
                                    var saInfer = new RDFOntologyTaxonomyEntry(f1Asn.TaxonomySubject, f1Asn.TaxonomyPredicate, f2).SetInference(true);

                                    //Enrich the data with the inference
                                    var taxCnt  = ontology.Data.Relations.Assertions.EntriesCount;
                                    ontology.Data.Relations.Assertions.AddEntry(saInfer);

                                    //Add the inference into the reasoning report
                                    if (ontology.Data.Relations.Assertions.EntriesCount > taxCnt) {
                                        report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "SameAsEntailment", saInfer));
                                    }

                                }

                            }
                            #endregion

                        }

                    }
                }
            }

            /// <summary>
            /// SymmetricPropertyEntailment implements data entailments based on 'owl:SymmetricProperty' axiom:
            /// ((F1 P F2) AND (P TYPE SYMMETRICPROPERTY)) => (F2 P F1)
            /// </summary>
            internal static void SymmetricPropertyEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                var symPropsEnum        = ontology.Model.PropertyModel.SymmetricPropertiesEnumerator;
                while(symPropsEnum.MoveNext()) {
                    var p               = symPropsEnum.Current;

                    //Filter the assertions using the current property (F1 P F2)
                    var pAsns           = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                    //Iterate those assertions
                    foreach(var pAsn   in pAsns) {

                        //Taxonomy-check for securing inference consistency
                        if (pAsn.TaxonomyObject.IsFact()) {

                            //Create the inference as a taxonomy entry
                            var spInfer = new RDFOntologyTaxonomyEntry(pAsn.TaxonomyObject, p, pAsn.TaxonomySubject).SetInference(true);

                            //Enrich the data with the inference
                            var taxCnt  = ontology.Data.Relations.Assertions.EntriesCount;
                            ontology.Data.Relations.Assertions.AddEntry(spInfer);

                            //Add the inference into the reasoning report
                            if (ontology.Data.Relations.Assertions.EntriesCount > taxCnt) {
                                report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "SymmetricPropertyEntailment", spInfer));
                            }

                        }

                    }

                }
            }

            /// <summary>
            /// TransitivePropertyEntailment implements data entailments based on 'owl:TransitiveProperty' axiom:
            /// ((F1 P F2) AND (F2 P F3) AND (P TYPE TRANSITIVEPROPERTY)) => (F1 P F3)
            /// </summary>
            internal static void TransitivePropertyEntailmentExec(RDFOntology ontology, RDFOntologyReasoningReport report) {
                var transPropCache      = new Dictionary<Int64, RDFOntologyData>();
                var transPropEnum       = ontology.Model.PropertyModel.TransitivePropertiesEnumerator;
                while(transPropEnum.MoveNext()) {
                    var p               = transPropEnum.Current;

                    //Filter the assertions using the current property (F1 P F2)
                    var pAsns           = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                    //Iterate those assertions
                    foreach(var pAsn   in pAsns) {

                        //Taxonomy-check for securing inference consistency
                        if(pAsn.TaxonomyObject.IsFact()) {

                            if(!transPropCache.ContainsKey(pAsn.TaxonomySubject.PatternMemberID)) {
                                transPropCache.Add(pAsn.TaxonomySubject.PatternMemberID, RDFOntologyReasoningHelper.EnlistTransitiveAssertionsOf((RDFOntologyFact)pAsn.TaxonomySubject, (RDFOntologyObjectProperty)p, ontology.Data));
                            }
                            foreach(var te in transPropCache[pAsn.TaxonomySubject.PatternMemberID]) {

                                //Create the inference as a taxonomy entry
                                var teInfer = new RDFOntologyTaxonomyEntry(pAsn.TaxonomySubject, p, te).SetInference(true);

                                //Enrich the data with the inference
                                var taxCnt  = ontology.Data.Relations.Assertions.EntriesCount;
                                ontology.Data.Relations.Assertions.AddEntry(teInfer);

                                //Add the inference into the reasoning report
                                if (ontology.Data.Relations.Assertions.EntriesCount > taxCnt) {
                                    report.AddEvidence(new RDFOntologyReasoningEvidence(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.Data, "TransitivePropertyEntailment", teInfer));
                                }

                            }

                        }

                    }

                    transPropCache.Clear();
                }

            }
            #endregion

        }        
        #endregion

    }

}