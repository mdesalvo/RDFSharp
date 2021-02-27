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

using RDFSharp.Model;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasonerRuleset implements a subset of RDFS/OWL-DL/OWL2 entailment rules
    /// </summary>
    public static class RDFOntologyReasonerRuleset
    {

        #region Properties

        /// <summary>
        /// Counter of rules available in the standard RDFS/OWL-DL/OWL2 ruleset
        /// </summary>
        public static readonly int RulesCount = 22;

        #region RDFS
        /// <summary>
        /// SubClassTransitivity (rdfs11) implements structural entailments based on 'rdfs:subClassOf' taxonomy<br/>
        /// ((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)"<br/>
        /// ((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)"<br/>
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)"
        /// </summary>
        public static RDFOntologyReasonerRule SubClassTransitivity { get; internal set; }

        /// <summary>
        /// SubPropertyTransitivity (rdfs5) implements structural entailments based on 'rdfs:subPropertyOf' taxonomy<br/>
        /// ((P1 SUBPROPERTYOF P2)      AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)<br/>
        /// ((P1 SUBPROPERTYOF P2)      AND (P2 EQUIVALENTPROPERTY P3)) => (P1 SUBPROPERTYOF P3)<br/>
        /// ((P1 EQUIVALENTPROPERTY P2) AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)
        /// </summary>
        public static RDFOntologyReasonerRule SubPropertyTransitivity { get; internal set; }

        /// <summary>
        /// ClassTypeEntailment (rdfs9) implements data entailments based on 'rdf:type' taxonomy<br/>
        /// ((F TYPE C1) AND (C1 SUBCLASSOF C2))      => (F TYPE C2)<br/>
        /// ((F TYPE C1) AND (C1 EQUIVALENTCLASS C2)) => (F TYPE C2)
        /// </summary>
        public static RDFOntologyReasonerRule ClassTypeEntailment { get; internal set; }

        /// <summary>
        /// PropertyEntailment (rdfs7) implements data entailments based on 'rdfs:subPropertyOf' taxonomy<br/>
        /// ((F1 P1 F2) AND (P1 SUBPROPERTYOF P2))      => (F1 P2 F2)<br/>
        /// ((F1 P1 F2) AND (P1 EQUIVALENTPROPERTY P2)) => (F1 P2 F2)
        /// </summary>
        public static RDFOntologyReasonerRule PropertyEntailment { get; internal set; }

        /// <summary>
        /// DomainEntailment (rdfs2) implements structural entailments based on 'rdfs:domain' taxonomy<br/>
        /// ((F1 P F2) AND (P DOMAIN C)) => (F1 TYPE C)
        /// </summary>
        public static RDFOntologyReasonerRule DomainEntailment { get; internal set; }

        /// <summary>
        /// RangeEntailment (rdfs3) implements structural entailments based on 'rdfs:range' taxonomy<br/>
        /// ((F1 P F2) AND (P RANGE C)) => (F2 TYPE C)
        /// </summary>
        public static RDFOntologyReasonerRule RangeEntailment { get; internal set; }
        #endregion

        #region OWL-DL
        /// <summary>
        /// EquivalentClassTransitivity implements structural entailments based on 'owl:EquivalentClass' taxonomy<br/>
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)
        /// </summary>
        public static RDFOntologyReasonerRule EquivalentClassTransitivity { get; internal set; }

        /// <summary>
        /// DisjointWithEntailment implements structural entailments based on 'owl:DisjointWith' taxonomy<br/>
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)<br/>
        /// ((C1 SUBCLASSOF C2)      AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)<br/>
        /// ((C1 DISJOINTWITH C2)    AND (C2 EQUIVALENTCLASS C3)) => (C1 DISJOINTWITH C3)
        /// </summary>
        public static RDFOntologyReasonerRule DisjointWithEntailment { get; internal set; }

        /// <summary>
        /// EquivalentPropertyTransitivity implements structural entailments based on 'owl:EquivalentProperty' taxonomy<br/>
        /// ((P1 EQUIVALENTPROPERTY P2) AND (P2 EQUIVALENTPROPERTY P3)) => (P1 EQUIVALENTPROPERTY P3)
        /// </summary>
        public static RDFOntologyReasonerRule EquivalentPropertyTransitivity { get; internal set; }

        /// <summary>
        /// SameAsTransitivity implements structural entailments based on 'owl:SameAs' taxonomy<br/>
        /// ((F1 SAMEAS F2) AND (F2 SAMEAS F3)) => (F1 SAMEAS F3)
        /// </summary>
        public static RDFOntologyReasonerRule SameAsTransitivity { get; internal set; }

        /// <summary>
        /// DifferentFromEntailment implements structural entailments based on 'owl:DifferentFrom' taxonomy<br/>
        /// ((F1 SAMEAS F2)        AND (F2 DIFFERENTFROM F3)) => (F1 DIFFERENTFROM F3)<br/>
        /// ((F1 DIFFERENTFROM F2) AND (F2 SAMEAS F3))        => (F1 DIFFERENTFROM F3)
        /// </summary>
        public static RDFOntologyReasonerRule DifferentFromEntailment { get; internal set; }

        /// <summary>
        /// InverseOfEntailment implements data entailments based on 'owl:inverseOf' taxonomy<br/>
        /// ((F1 P1 F2) AND (P1 INVERSEOF P2)) => (F2 P2 F1)
        /// </summary>
        public static RDFOntologyReasonerRule InverseOfEntailment { get; internal set; }

        /// <summary>
        /// SameAsEntailment implements data entailments based on 'owl:SameAs' taxonomy<br/>
        /// ((F1 P F2) AND (F1 SAMEAS F3)) => (F3 P F2)<br/>
        /// ((F1 P F2) AND (F2 SAMEAS F3)) => (F1 P F3)
        /// </summary>
        public static RDFOntologyReasonerRule SameAsEntailment { get; internal set; }

        /// <summary>
        /// SymmetricPropertyEntailment implements data entailments based on 'owl:SymmetricProperty' axiom<br/>
        /// ((F1 P F2) AND (P TYPE SYMMETRICPROPERTY)) => (F2 P F1)
        /// </summary>
        public static RDFOntologyReasonerRule SymmetricPropertyEntailment { get; internal set; }

        /// <summary>
        /// TransitivePropertyEntailment implements data entailments based on 'owl:TransitiveProperty' axiom<br/>
        /// ((F1 P F2) AND (F2 P F3) AND (P TYPE TRANSITIVEPROPERTY)) => (F1 P F3)
        /// </summary>
        public static RDFOntologyReasonerRule TransitivePropertyEntailment { get; internal set; }

        /// <summary>
        /// HasValueEntailment implements data entailments based on 'owl:hasValue' restrictions<br/>
        /// ((F1 TYPE C) AND (C SUBCLASSOF R) AND (R TYPE RESTRICTION) AND (R ONPROPERTY P) AND (R HASVALUE F2)) => (F1 P F2)
        /// </summary>
        public static RDFOntologyReasonerRule HasValueEntailment { get; internal set; }
        #endregion

        #region OWL2
        /// <summary>
        /// ReflexivePropertyEntailment implements data entailments based on 'owl:ReflexiveProperty' axiom [OWL2]<br/>
        /// ((F1 P F2) AND (P TYPE REFLEXIVEPROPERTY)) => (F1 P F1)
        /// </summary>
        public static RDFOntologyReasonerRule ReflexivePropertyEntailment { get; internal set; }

        /// <summary>
        /// HasKeyEntailment implements data entailments based on 'owl:hasKey' axiom [OWL2]<br/>
        /// ((C HASKEY P) AND (F1 TYPE C) AND (F2 TYPE C) AND (F1 P K) AND (F2 P K)) => (F1 SAMEAS F2)
        /// </summary>
        public static RDFOntologyReasonerRule HasKeyEntailment { get; internal set; }

        /// <summary>
        /// PropertyChainEntailment implements data entailments based on 'owl:propertyChainAxiom' axiom [OWL2]<br/>
        /// ((PCA PROPERTYCHAINAXIOM P1) AND (PCA PROPERTYCHAINAXIOM P2) AND (F1 P1 X) AND (X P2 F2)) => (F1 PCA F2)
        /// </summary>
        public static RDFOntologyReasonerRule PropertyChainEntailment { get; internal set; }

        /// <summary>
        /// NamedIndividualEntailment implements data entailments based on 'owl:NamedIndividual' declaration [OWL2]<br/>
        /// ((F TYPE C) AND (F ISNOT BLANK) => (F TYPE NAMEDINDIVIDUAL)
        /// </summary>
        public static RDFOntologyReasonerRule NamedIndividualEntailment { get; internal set; }

        /// <summary>
        /// HasSelfEntailment implements data entailments based on 'owl:hasSelf' restrictions [OWL2]<br/>
        /// ((F TYPE C) AND (C SUBCLASSOF R) AND (R TYPE RESTRICTION) AND (R ONPROPERTY P) AND (R HASSELF TRUE)) => (F P F)
        /// </summary>
        public static RDFOntologyReasonerRule HasSelfEntailment { get; internal set; }

        /// <summary>
        /// TopPropertyEntailment implements structural entailments based on 'owl:topObjectProperty' and 'owl:topDataProperty' subsumption [OWL2]<br/>
        /// ((P TYPE OBJECTPROPERTY)   => (P SUBPROPERTYOF TOPOBJECTPROPERTY)<br/>
        /// ((P TYPE DATATYPEPROPERTY) => (P SUBPROPERTYOF TOPDATAPROPERTY)
        /// </summary>
        public static RDFOntologyReasonerRule TopPropertyEntailment { get; internal set; }
        #endregion

        #endregion

        #region Ctors
        /// <summary>
        /// Static-ctor to initialize the BASE ruleset
        /// </summary>
        static RDFOntologyReasonerRuleset()
        {

            #region RDFS
            //SubClassTransitivity (rdfs11)
            SubClassTransitivity = new RDFOntologyReasonerRule("SubClassTransitivity",
                                                               "SubClassTransitivity (rdfs11) implements structural entailments based on 'rdfs:subClassOf' taxonomy:" +
                                                               "((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3);" +
                                                               "((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3);" +
                                                               "((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)",
                                                               SubClassTransitivityExec).SetStandard();

            //SubPropertyTransitivity (rdfs5)
            SubPropertyTransitivity = new RDFOntologyReasonerRule("SubPropertyTransitivity",
                                                                  "SubPropertyTransitivity (rdfs5) implements structural entailments based on 'rdfs:subPropertyOf' taxonomy:" +
                                                                  "((P1 SUBPROPERTYOF P2)      AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3);" +
                                                                  "((P1 SUBPROPERTYOF P2)      AND (P2 EQUIVALENTPROPERTY P3)) => (P1 SUBPROPERTYOF P3);" +
                                                                  "((P1 EQUIVALENTPROPERTY P2) AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)",
                                                                  SubPropertyTransitivityExec).SetStandard();

            //ClassTypeEntailment (rdfs9)
            ClassTypeEntailment = new RDFOntologyReasonerRule("ClassTypeEntailment",
                                                              "ClassTypeEntailment (rdfs9) implements structural entailments based on 'rdf:type' taxonomy:" +
                                                              "((F TYPE C1) AND (C1 SUBCLASSOF C2))      => (F TYPE C2);" +
                                                              "((F TYPE C1) AND (C1 EQUIVALENTCLASS C2)) => (F TYPE C2)",
                                                              ClassTypeEntailmentExec).SetStandard();

            //PropertyEntailment (rdfs7)
            PropertyEntailment = new RDFOntologyReasonerRule("PropertyEntailment",
                                                             "PropertyEntailment (rdfs7) implements data entailments based on 'rdfs:subPropertyOf' taxonomy:" +
                                                             "((F1 P1 F2) AND (P1 SUBPROPERTYOF P2))      => (F1 P2 F2);" +
                                                             "((F1 P1 F2) AND (P1 EQUIVALENTPROPERTY P2)) => (F1 P2 F2)",
                                                             PropertyEntailmentExec).SetStandard();

            //DomainEntailment (rdfs2)
            DomainEntailment = new RDFOntologyReasonerRule("DomainEntailment",
                                                           "DomainEntailment (rdfs2) implements structural entailments based on 'rdfs:domain' taxonomy:" +
                                                           "((F1 P F2) AND (P DOMAIN C)) => (F1 TYPE C)",
                                                           DomainEntailmentExec).SetStandard();

            //RangeEntailment (rdfs3)
            RangeEntailment = new RDFOntologyReasonerRule("RangeEntailment",
                                                          "RangeEntailment (rdfs3) implements structural entailments based on 'rdfs:range' taxonomy:" +
                                                          "((F1 P F2) AND (P RANGE C)) => (F2 TYPE C)",
                                                          RangeEntailmentExec).SetStandard();
            #endregion

            #region OWL-DL
            //EquivalentClassTransitivity
            EquivalentClassTransitivity = new RDFOntologyReasonerRule("EquivalentClassTransitivity",
                                                                      "EquivalentClassTransitivity implements structural entailments based on 'owl:EquivalentClass' taxonomy:" +
                                                                      "((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)",
                                                                      EquivalentClassTransitivityExec).SetStandard();

            //DisjointWithEntailment
            DisjointWithEntailment = new RDFOntologyReasonerRule("DisjointWithEntailment",
                                                                 "DisjointWithEntailment implements structural entailments based on 'owl:DisjointWith' taxonomy:" +
                                                                 "((C1 EQUIVALENTCLASS C2) AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3);" +
                                                                 "((C1 SUBCLASSOF C2)      AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3);" +
                                                                 "((C1 DISJOINTWITH C2)    AND (C2 EQUIVALENTCLASS C3)) => (C1 DISJOINTWITH C3)",
                                                                 DisjointWithEntailmentExec).SetStandard();

            //EquivalentPropertyTransitivity
            EquivalentPropertyTransitivity = new RDFOntologyReasonerRule("EquivalentPropertyTransitivity",
                                                                         "EquivalentPropertyTransitivity implements structural entailments based on 'owl:EquivalentProperty' taxonomy:" +
                                                                         "((P1 EQUIVALENTPROPERTY P2) AND (P2 EQUIVALENTPROPERTY P3)) => (P1 EQUIVALENTPROPERTY P3)",
                                                                         EquivalentPropertyTransitivityExec).SetStandard();

            //SameAsTransitivity
            SameAsTransitivity = new RDFOntologyReasonerRule("SameAsTransitivity",
                                                             "SameAsTransitivity implements structural entailments based on 'owl:SameAs' taxonomy:" +
                                                             "((F1 SAMEAS F2) AND (F2 SAMEAS F3)) => (F1 SAMEAS F3)",
                                                             SameAsTransitivityExec).SetStandard();

            //DifferentFromEntailment
            DifferentFromEntailment = new RDFOntologyReasonerRule("DifferentFromEntailment",
                                                                  "DifferentFromEntailment implements structural entailments based on 'owl:DifferentFrom' taxonomy:" +
                                                                  "((F1 SAMEAS F2)        AND (F2 DIFFERENTFROM F3)) => (F1 DIFFERENTFROM F3);" +
                                                                  "((F1 DIFFERENTFROM F2) AND (F2 SAMEAS F3))        => (F1 DIFFERENTFROM F3)",
                                                                  DifferentFromEntailmentExec).SetStandard();

            //InverseOfEntailment
            InverseOfEntailment = new RDFOntologyReasonerRule("InverseOfEntailment",
                                                              "InverseOfEntailment implements data entailments based on 'owl:inverseOf' taxonomy:" +
                                                              "((F1 P1 F2) AND (P1 INVERSEOF P2)) => (F2 P2 F1)",
                                                              InverseOfEntailmentExec).SetStandard();

            //SameAsEntailment
            SameAsEntailment = new RDFOntologyReasonerRule("SameAsEntailment",
                                                           "SameAsEntailment implements data entailments based on 'owl:SameAs' taxonomy:" +
                                                           "((F1 P F2) AND (F1 SAMEAS F3)) => (F3 P F2);" +
                                                           "((F1 P F2) AND (F2 SAMEAS F3)) => (F1 P F3)",
                                                           SameAsEntailmentExec).SetStandard();

            //SymmetricPropertyEntailment
            SymmetricPropertyEntailment = new RDFOntologyReasonerRule("SymmetricPropertyEntailment",
                                                                      "SymmetricPropertyEntailment implements data entailments based on 'owl:SymmetricProperty' axiom:" +
                                                                      "((F1 P F2) AND (P TYPE SYMMETRICPROPERTY)) => (F2 P F1)",
                                                                      SymmetricPropertyEntailmentExec).SetStandard();

            //TransitivePropertyEntailment
            TransitivePropertyEntailment = new RDFOntologyReasonerRule("TransitivePropertyEntailment",
                                                                       "TransitivePropertyEntailment implements data entailments based on 'owl:TransitiveProperty' axiom:" +
                                                                       "((F1 P F2) AND (F2 P F3) AND (P TYPE TRANSITIVEPROPERTY)) => (F1 P F3)",
                                                                       TransitivePropertyEntailmentExec).SetStandard();

            //HasValueEntailment
            HasValueEntailment = new RDFOntologyReasonerRule("HasValueEntailment",
                                                             "HasValueEntailment implements data entailments based on 'owl:hasValue' restrictions:" +
                                                             "((F1 TYPE C) AND (C SUBCLASSOF R) AND (R TYPE RESTRICTION) AND (R ONPROPERTY P) AND (R HASVALUE F2)) => (F1 P F2)",
                                                             HasValueEntailmentExec).SetStandard();
            #endregion

            #region OWL2
            //ReflexivePropertyEntailment
            ReflexivePropertyEntailment = new RDFOntologyReasonerRule("ReflexivePropertyEntailment",
                                                                      "(OWL2) ReflexivePropertyEntailment implements data entailments based on 'owl:ReflexiveProperty' axiom:" +
                                                                      "((F1 P F2) AND (P TYPE REFLEXIVEPROPERTY)) => (F1 P F1)",
                                                                      ReflexivePropertyEntailmentExec).SetStandard();

            //HasKeyEntailment
            HasKeyEntailment = new RDFOntologyReasonerRule("HasKeyEntailment",
                                                           "(OWL2) HasKeyEntailment implements data entailments based on 'owl:hasKey' taxonomy:" +
                                                           "((C HASKEY P) AND (F1 TYPE C) AND (F2 TYPE C) AND (F1 P K) AND (F2 P K)) => (F1 SAMEAS F2)",
                                                           HasKeyEntailmentExec).SetStandard();

            //PropertyChainEntailment
            PropertyChainEntailment = new RDFOntologyReasonerRule("PropertyChainEntailment",
                                                                  "(OWL2) PropertyChainEntailment implements data entailments based on 'owl:propertyChainAxiom' taxonomy:" +
                                                                  "((PCA PROPERTYCHAINAXIOM P1) AND (PCA PROPERTYCHAINAXIOM P2) AND (F1 P1 X) AND (X P2 F2)) => (F1 PCA F2)",
                                                                  PropertyChainEntailmentExec).SetStandard();

            //NamedIndividualEntailment
            NamedIndividualEntailment = new RDFOntologyReasonerRule("NamedIndividualEntailment",
                                                                    "(OWL2) NamedIndividualEntailment implements data entailments based on 'owl:NamedIndividual' declaration:" +
                                                                    "((F TYPE C) AND (F ISNOT BLANK)) => (F TYPE NAMEDINDIVIDUAL)",
                                                                    NamedIndividualEntailmentExec).SetStandard();

            //HasSelfEntailment
            HasSelfEntailment = new RDFOntologyReasonerRule("HasSelfEntailment",
                                                            "(OWL2) HasSelfEntailment implements data entailments based on 'owl:hasSelf' restrictions:" +
                                                            "((F TYPE C) AND (C SUBCLASSOF R) AND (R TYPE RESTRICTION) AND (R ONPROPERTY P) AND (R HASSELF TRUE)) => (F P F)",
                                                            HasSelfEntailmentExec).SetStandard();

            //TopPropertyEntailment
            TopPropertyEntailment = new RDFOntologyReasonerRule("TopPropertyEntailment",
                                                                "(OWL2) TopPropertyEntailment implements structural entailments based on 'owl:topObjectProperty' and 'owl:topDataProperty' subsumption:" +
                                                                "(P TYPE OBJECTPROPERTY) => (P SUBPROPERTYOF TOPOBJECTPROPERTY);" +
                                                                "(P TYPE DATATYPEPROPERTY) => (P SUBPROPERTYOF TOPDATAPROPERTY);",
                                                                TopPropertyEntailmentExec).SetStandard();
            #endregion

        }
        #endregion

        #region Methods

        #region RDFS
        /// <summary>
        /// SubClassTransitivity (rdfs11) implements structural entailments based on 'rdfs:subClassOf' taxonomy:<br/>
        /// ((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)<br/>
        /// ((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)<br/>
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)
        /// </summary>
        internal static RDFOntologyReasonerReport SubClassTransitivityExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var subClassOf = RDFVocabulary.RDFS.SUB_CLASS_OF.ToRDFOntologyObjectProperty();
            foreach (var c in caches["TB_FreeClasses"].OfType<RDFOntologyClass>())
            {

                //Enlist the superclasses of the current class
                var superclasses = ontology.Model.ClassModel.GetSuperClassesOf(c);
                foreach (var sc in superclasses)
                {
                    //Create the inference as a taxonomy entry
                    var sem_inf = new RDFOntologyTaxonomyEntry(c, subClassOf, sc).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Model.ClassModel.Relations.SubClassOf.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.ClassModel, nameof(SubClassTransitivity), nameof(RDFOntologyClassModel.Relations.SubClassOf), sem_inf));
                }

            }
            return report;
        }

        /// <summary>
        /// SubPropertyTransitivity (rdfs5) implements structural entailments based on 'rdfs:subPropertyOf' taxonomy:<br/>
        /// ((P1 SUBPROPERTYOF P2)      AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)<br/>
        /// ((P1 SUBPROPERTYOF P2)      AND (P2 EQUIVALENTPROPERTY P3)) => (P1 SUBPROPERTYOF P3)<br/>
        /// ((P1 EQUIVALENTPROPERTY P2) AND (P2 SUBPROPERTYOF P3))      => (P1 SUBPROPERTYOF P3)
        /// </summary>
        internal static RDFOntologyReasonerReport SubPropertyTransitivityExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var subPropertyOf = RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and annotation properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => !prop.IsAnnotationProperty()).OfType<RDFOntologyProperty>().ToList();
            foreach (var p in availableprops)
            {
                //Enlist the superproperties of the current property
                var superprops = ontology.Model.PropertyModel.GetSuperPropertiesOf(p);
                foreach (var sp in superprops)
                {
                    //Create the inference as a taxonomy entry
                    var sem_inf = new RDFOntologyTaxonomyEntry(p, subPropertyOf, sp).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Model.PropertyModel.Relations.SubPropertyOf.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.PropertyModel, nameof(SubPropertyTransitivity), nameof(RDFOntologyPropertyModel.Relations.SubPropertyOf), sem_inf));
                }
            }
            return report;
        }

        /// <summary>
        /// ClassTypeEntailment (rdfs9) implements structural entailments based on 'rdf:type' taxonomy:<br/>
        /// ((F TYPE C1) AND (C1 SUBCLASSOF C2))      => (F TYPE C2)<br/>
        /// ((F TYPE C1) AND (C1 EQUIVALENTCLASS C2)) => (F TYPE C2)
        /// </summary>
        internal static RDFOntologyReasonerReport ClassTypeEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var type = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();

            //Calculate the set of available classes on which to perform the reasoning (exclude BASE classes and literal-compatible classes)
            var availableclasses = caches["TB_FreeClasses"].OfType<RDFOntologyClass>().Where(cls => !ontology.Model.ClassModel.CheckIsLiteralCompatibleClass(cls));
            var membersCache = new Dictionary<long, RDFOntologyData>();

            //Evaluate enumerations
            foreach (var c in availableclasses.Where(cls => cls.IsEnumerateClass()))
            {

                //Enlist the members of the current enumeration
                if (!membersCache.ContainsKey(c.PatternMemberID))
                    membersCache.Add(c.PatternMemberID, ontology.GetMembersOfEnumerate((RDFOntologyEnumerateClass)c));
                foreach (var f in membersCache[c.PatternMemberID])
                {
                    //Create the inference as a taxonomy entry
                    var sem_inf = new RDFOntologyTaxonomyEntry(f, type, c).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ClassTypeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }

            }

            //Evaluate restrictions
            foreach (var c in availableclasses.Where(cls => cls.IsRestrictionClass()))
            {

                //Enlist the members of the current restriction
                if (!membersCache.ContainsKey(c.PatternMemberID))
                    membersCache.Add(c.PatternMemberID, ontology.GetMembersOfRestriction((RDFOntologyRestriction)c));
                foreach (var f in membersCache[c.PatternMemberID])
                {
                    //Create the inference as a taxonomy entry
                    var sem_inf = new RDFOntologyTaxonomyEntry(f, type, c).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ClassTypeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }

            }

            //Evaluate simple classes
            foreach (var c in availableclasses.Where(cls => cls.IsSimpleClass()))
            {

                //Enlist the members of the current class
                if (!membersCache.ContainsKey(c.PatternMemberID))
                    membersCache.Add(c.PatternMemberID, ontology.GetMembersOfClass(c));
                foreach (var f in membersCache[c.PatternMemberID])
                {
                    //Create the inference as a taxonomy entry
                    var sem_inf = new RDFOntologyTaxonomyEntry(f, type, c).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ClassTypeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }

            }

            //Evaluate composite classes
            foreach (var c in availableclasses.Where(cls => cls.IsCompositeClass()))
            {

                //Enlist the members of the current composite class
                if (!membersCache.ContainsKey(c.PatternMemberID))
                    membersCache.Add(c.PatternMemberID, ontology.GetMembersOfComposite(c, membersCache));
                foreach (var f in membersCache[c.PatternMemberID])
                {
                    //Create the inference as a taxonomy entry
                    var sem_inf = new RDFOntologyTaxonomyEntry(f, type, c).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ClassTypeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }

            }

            return report;
        }

        /// <summary>
        /// PropertyEntailment (rdfs7) implements data entailments based on 'rdfs:subPropertyOf' taxonomy:<br/>
        /// ((F1 P1 F2) AND (P1 SUBPROPERTYOF P2))      => (F1 P2 F2)<br/>
        /// ((F1 P1 F2) AND (P1 EQUIVALENTPROPERTY P2)) => (F1 P2 F2)
        /// </summary>
        internal static RDFOntologyReasonerReport PropertyEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and annotation properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => !prop.IsAnnotationProperty()).OfType<RDFOntologyProperty>().ToList();
            foreach (var p1 in availableprops)
            {

                //Filter the assertions using the current property (F1 P1 F2)
                var p1Asns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p1);

                //Enlist the compatible properties of the current property (P1 [SUBPROPERTYOF|EQUIVALENTPROPERTY] P2)
                foreach (var p2 in ontology.Model.PropertyModel.GetSuperPropertiesOf(p1)
                                                               .UnionWith(ontology.Model.PropertyModel.GetEquivalentPropertiesOf(p1)))
                {

                    //Iterate the compatible assertions
                    foreach (var p1Asn in p1Asns)
                    {

                        //Taxonomy-check for securing inference consistency
                        if ((p2.IsObjectProperty() && p1Asn.TaxonomyObject.IsFact())
                                || (p2.IsDatatypeProperty() && p1Asn.TaxonomyObject.IsLiteral()))
                        {
                            //Create the inference as a taxonomy entry
                            var sem_inf = new RDFOntologyTaxonomyEntry(p1Asn.TaxonomySubject, p2, p1Asn.TaxonomyObject).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                            //Add the inference to the report
                            if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                                report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(PropertyEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                        }

                    }

                }

            }
            return report;
        }

        /// <summary>
        /// DomainEntailment (rdfs2) implements structural entailments based on 'rdfs:domain' taxonomy:<br/>
        /// ((F1 P F2) AND (P DOMAIN C)) => (F1 TYPE C)"
        /// </summary>
        internal static RDFOntologyReasonerReport DomainEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var type = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and annotation properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => !prop.IsAnnotationProperty()).OfType<RDFOntologyProperty>().ToList();
            foreach (var p in availableprops)
            {
                if (p.Domain != null)
                {

                    //Filter the assertions using the current property (F1 P1 F2)
                    var pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                    //Iterate the related assertions
                    foreach (var pAsn in pAsns)
                    {
                        //Create the inference as a taxonomy entry
                        var sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomySubject, type, p.Domain).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(DomainEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                    }

                }
            }
            return report;
        }

        /// <summary>
        /// RangeEntailment (rdfs3) implements structural entailments based on 'rdfs:range' taxonomy:<br/>
        /// ((F1 P F2) AND (P RANGE C)) => (F2 TYPE C)"
        /// </summary>
        internal static RDFOntologyReasonerReport RangeEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var type = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and annotation properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => !prop.IsAnnotationProperty()).OfType<RDFOntologyProperty>().ToList();
            foreach (var p in availableprops)
            {
                if (p.Range != null)
                {

                    //Filter the assertions using the current property (F1 P1 F2)
                    var pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                    //Iterate the related assertions
                    foreach (var pAsn in pAsns)
                    {

                        //Taxonomy-check for securing inference consistency
                        if (pAsn.TaxonomyObject.IsFact())
                        {
                            //Create the inference as a taxonomy entry
                            var sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomyObject, type, p.Range).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                            //Add the inference to the report
                            if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                                report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(RangeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                        }

                    }

                }
            }
            return report;
        }
        #endregion

        #region OWL-DL
        /// <summary>
        /// EquivalentClassTransitivity implements structural entailments based on 'owl:EquivalentClass' taxonomy:<br/>
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)
        /// </summary>
        internal static RDFOntologyReasonerReport EquivalentClassTransitivityExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var equivalentClass = RDFVocabulary.OWL.EQUIVALENT_CLASS.ToRDFOntologyObjectProperty();
            foreach (var c in caches["TB_FreeClasses"].OfType<RDFOntologyClass>())
            {

                //Enlist the equivalent classes of the current class
                var equivclasses = ontology.Model.ClassModel.GetEquivalentClassesOf(c);
                foreach (var ec in equivclasses)
                {
                    //Create the inference as a taxonomy entry
                    var sem_infA = new RDFOntologyTaxonomyEntry(c, equivalentClass, ec).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    var sem_infB = new RDFOntologyTaxonomyEntry(ec, equivalentClass, c).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Model.ClassModel.Relations.EquivalentClass.ContainsEntry(sem_infA))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.ClassModel, nameof(EquivalentClassTransitivity), nameof(RDFOntologyClassModel.Relations.EquivalentClass), sem_infA));
                    if (!ontology.Model.ClassModel.Relations.EquivalentClass.ContainsEntry(sem_infB))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.ClassModel, nameof(EquivalentClassTransitivity), nameof(RDFOntologyClassModel.Relations.EquivalentClass), sem_infB));
                }

            }
            return report;
        }

        /// <summary>
        /// DisjointWithEntailment implements structural entailments based on 'owl:DisjointWith' taxonomy:<br/>
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)<br/>
        /// ((C1 SUBCLASSOF C2)      AND (C2 DISJOINTWITH C3))    => (C1 DISJOINTWITH C3)<br/>
        /// ((C1 DISJOINTWITH C2)    AND (C2 EQUIVALENTCLASS C3)) => (C1 DISJOINTWITH C3)
        /// </summary>
        internal static RDFOntologyReasonerReport DisjointWithEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var disjointWith = RDFVocabulary.OWL.DISJOINT_WITH.ToRDFOntologyObjectProperty();
            foreach (var c in caches["TB_FreeClasses"].OfType<RDFOntologyClass>())
            {

                //Enlist the disjoint classes of the current class
                var disjclasses = ontology.Model.ClassModel.GetDisjointClassesWith(c);
                foreach (var dwc in disjclasses)
                {
                    //Create the inference as a taxonomy entry
                    var sem_infA = new RDFOntologyTaxonomyEntry(c, disjointWith, dwc).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    var sem_infB = new RDFOntologyTaxonomyEntry(dwc, disjointWith, c).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Model.ClassModel.Relations.DisjointWith.ContainsEntry(sem_infA))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.ClassModel, nameof(DisjointWithEntailment), nameof(RDFOntologyClassModel.Relations.DisjointWith), sem_infA));
                    if (!ontology.Model.ClassModel.Relations.DisjointWith.ContainsEntry(sem_infB))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.ClassModel, nameof(DisjointWithEntailment), nameof(RDFOntologyClassModel.Relations.DisjointWith), sem_infB));
                }

            }
            return report;
        }

        /// <summary>
        /// EquivalentPropertyTransitivity implements structural entailments based on 'owl:EquivalentProperty' taxonomy:<br/>
        /// ((P1 EQUIVALENTPROPERTY P2) AND (P2 EQUIVALENTPROPERTY P3)) => (P1 EQUIVALENTPROPERTY P3)
        /// </summary>
        internal static RDFOntologyReasonerReport EquivalentPropertyTransitivityExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var equivProperty = RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and annotation properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => !prop.IsAnnotationProperty()).OfType<RDFOntologyProperty>().ToList();
            foreach (var p in availableprops)
            {

                //Enlist the equivalent properties of the current property
                var equivprops = ontology.Model.PropertyModel.GetEquivalentPropertiesOf(p);
                foreach (var ep in equivprops)
                {
                    //Create the inference as a taxonomy entry
                    var sem_infA = new RDFOntologyTaxonomyEntry(p, equivProperty, ep).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    var sem_infB = new RDFOntologyTaxonomyEntry(ep, equivProperty, p).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Model.PropertyModel.Relations.EquivalentProperty.ContainsEntry(sem_infA))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.PropertyModel, nameof(EquivalentPropertyTransitivity), nameof(RDFOntologyPropertyModel.Relations.EquivalentProperty), sem_infA));
                    if (!ontology.Model.PropertyModel.Relations.EquivalentProperty.ContainsEntry(sem_infA))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.PropertyModel, nameof(EquivalentPropertyTransitivity), nameof(RDFOntologyPropertyModel.Relations.EquivalentProperty), sem_infB));
                }

            }
            return report;
        }

        /// <summary>
        /// SameAsTransitivity implements structural entailments based on 'owl:sameAs' taxonomy:<br/>
        /// ((F1 SAMEAS F2) AND (F2 SAMEAS F3)) => (F1 SAMEAS F3)
        /// </summary>
        internal static RDFOntologyReasonerReport SameAsTransitivityExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var sameAs = RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty();
            foreach (var f in ontology.Data)
            {

                //Enlist the same facts of the current fact
                var samefacts = ontology.Data.GetSameFactsAs(f);
                foreach (var sf in samefacts)
                {
                    //Create the inference as a taxonomy entry
                    var sem_infA = new RDFOntologyTaxonomyEntry(f, sameAs, sf).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    var sem_infB = new RDFOntologyTaxonomyEntry(sf, sameAs, f).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.SameAs.ContainsEntry(sem_infA))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(SameAsTransitivity), nameof(RDFOntologyData.Relations.SameAs), sem_infA));
                    if (!ontology.Data.Relations.SameAs.ContainsEntry(sem_infB))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(SameAsTransitivity), nameof(RDFOntologyData.Relations.SameAs), sem_infB));
                }

            }
            return report;
        }

        /// <summary>
        /// DifferentFromEntailment implements structural entailments based on 'owl:DifferentFrom' taxonomy:<br/>
        /// ((F1 SAMEAS F2)        AND (F2 DIFFERENTFROM F3)) => (F1 DIFFERENTFROM F3)<br/>
        /// ((F1 DIFFERENTFROM F2) AND (F2 SAMEAS F3))        => (F1 DIFFERENTFROM F3)
        /// </summary>
        internal static RDFOntologyReasonerReport DifferentFromEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var differentFrom = RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty();
            foreach (var f in ontology.Data)
            {

                //Enlist the different facts of the current fact
                var differfacts = ontology.Data.GetDifferentFactsFrom(f);
                foreach (var df in differfacts)
                {
                    //Create the inference as a taxonomy entry
                    var sem_infA = new RDFOntologyTaxonomyEntry(f, differentFrom, df).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    var sem_infB = new RDFOntologyTaxonomyEntry(df, differentFrom, f).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.DifferentFrom.ContainsEntry(sem_infA))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(DifferentFromEntailment), nameof(RDFOntologyData.Relations.DifferentFrom), sem_infA));
                    if (!ontology.Data.Relations.DifferentFrom.ContainsEntry(sem_infA))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(DifferentFromEntailment), nameof(RDFOntologyData.Relations.DifferentFrom), sem_infB));
                }

            }
            return report;
        }

        /// <summary>
        /// InverseOfEntailment implements data entailments based on 'owl:inverseOf' taxonomy:<br/>
        /// ((F1 P1 F2) AND (P1 INVERSEOF P2)) => (F2 P2 F1)
        /// </summary>
        internal static RDFOntologyReasonerReport InverseOfEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and annotation/datatype properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => prop.IsObjectProperty()).ToList();
            foreach (var p1 in availableprops)
            {

                //Filter the assertions using the current property (F1 P1 F2)
                var p1Asns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p1);

                //Enlist the inverse properties of the current property
                var inverseprops = ontology.Model.PropertyModel.GetInversePropertiesOf((RDFOntologyObjectProperty)p1);
                foreach (var p2 in inverseprops)
                {

                    //Iterate the compatible assertions
                    foreach (var p1Asn in p1Asns)
                    {

                        //Taxonomy-check for securing inference consistency
                        if (p2.IsObjectProperty() && p1Asn.TaxonomyObject.IsFact())
                        {
                            //Create the inference as a taxonomy entry
                            var sem_inf = new RDFOntologyTaxonomyEntry(p1Asn.TaxonomyObject, p2, p1Asn.TaxonomySubject).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                            //Add the inference to the report
                            if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                                report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(InverseOfEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                        }

                    }

                }
            }

            return report;
        }

        /// <summary>
        /// SameAsEntailment implements data entailments based on 'owl:sameAs' taxonomy:<br/>
        /// ((F1 P F2) AND (F1 SAMEAS F3)) => (F3 P F2)<br/>
        /// ((F1 P F2) AND (F2 SAMEAS F3)) => (F1 P F3)
        /// </summary>
        internal static RDFOntologyReasonerReport SameAsEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            foreach (var f1 in ontology.Data)
            {

                //Enlist the same facts of the current fact
                var sameFacts = ontology.Data.GetSameFactsAs(f1);
                if (sameFacts.FactsCount > 0)
                {

                    //Filter the assertions using the current fact
                    var f1AsnsSubj = ontology.Data.Relations.Assertions.SelectEntriesBySubject(f1);
                    var f1AsnsObj = ontology.Data.Relations.Assertions.SelectEntriesByObject(f1);

                    //Enlist the same facts of the current fact
                    foreach (var f2 in sameFacts)
                    {

                        #region Subject-Side
                        //Iterate the assertions having the current fact as subject
                        foreach (var f1Asn in f1AsnsSubj)
                        {

                            //Taxonomy-check for securing inference consistency
                            if (f1Asn.TaxonomyPredicate.IsObjectProperty() && f1Asn.TaxonomyObject.IsFact())
                            {
                                //Create the inference as a taxonomy entry
                                var sem_infA = new RDFOntologyTaxonomyEntry(f2, f1Asn.TaxonomyPredicate, f1Asn.TaxonomyObject).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                                //Add the inference to the report
                                if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_infA))
                                    report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(SameAsEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_infA));
                            }

                        }
                        #endregion

                        #region Object-Side
                        //Iterate the assertions having the current fact as object
                        foreach (var f1Asn in f1AsnsObj)
                        {

                            //Taxonomy-check for securing inference consistency
                            if (f1Asn.TaxonomyPredicate.IsObjectProperty() && f2.IsFact())
                            {
                                //Create the inference as a taxonomy entry
                                var sem_infB = new RDFOntologyTaxonomyEntry(f1Asn.TaxonomySubject, f1Asn.TaxonomyPredicate, f2).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                                //Add the inference to the report
                                if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_infB))
                                    report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(SameAsEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_infB));
                            }

                        }
                        #endregion

                    }

                }

            }
            return report;
        }

        /// <summary>
        /// SymmetricPropertyEntailment implements data entailments based on 'owl:SymmetricProperty' axiom:<br/>
        /// ((F1 P F2) AND (P TYPE SYMMETRICPROPERTY)) => (F2 P F1)
        /// </summary>
        internal static RDFOntologyReasonerReport SymmetricPropertyEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and not-symmetric properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => prop.IsSymmetricProperty()).ToList();
            foreach (var p in availableprops)
            {

                //Filter the assertions using the current property (F1 P F2)
                var pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                //Iterate those assertions
                foreach (var pAsn in pAsns)
                {

                    //Taxonomy-check for securing inference consistency
                    if (pAsn.TaxonomyObject.IsFact())
                    {
                        //Create the inference as a taxonomy entry
                        var sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomyObject, p, pAsn.TaxonomySubject).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(SymmetricPropertyEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                    }

                }

            }

            return report;
        }

        /// <summary>
        /// TransitivePropertyEntailment implements data entailments based on 'owl:TransitiveProperty' axiom:<br/>
        /// ((F1 P F2) AND (F2 P F3) AND (P TYPE TRANSITIVEPROPERTY)) => (F1 P F3)
        /// </summary>
        internal static RDFOntologyReasonerReport TransitivePropertyEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var transPropCache = new Dictionary<long, RDFOntologyData>();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and not-transitive properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => prop.IsTransitiveProperty()).ToList();
            foreach (var p in availableprops)
            {

                //Filter the assertions using the current property (F1 P F2)
                var pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                //Iterate those assertions
                foreach (var pAsn in pAsns)
                {

                    //Taxonomy-check for securing inference consistency
                    if (pAsn.TaxonomyObject.IsFact())
                    {

                        if (!transPropCache.ContainsKey(pAsn.TaxonomySubject.PatternMemberID))
                            transPropCache.Add(pAsn.TaxonomySubject.PatternMemberID, ontology.Data.GetTransitiveAssertionsOf((RDFOntologyFact)pAsn.TaxonomySubject, (RDFOntologyObjectProperty)p));
                        foreach (var te in transPropCache[pAsn.TaxonomySubject.PatternMemberID])
                        {
                            //Create the inference as a taxonomy entry
                            var sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomySubject, p, te).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                            //Add the inference to the report
                            if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                                report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(TransitivePropertyEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                        }

                    }

                }
                transPropCache.Clear();

            }
            return report;
        }

        /// <summary>
        /// HasValueEntailment implements data entailments based on 'owl:hasValue' restrictions:<br/>
        /// ((F1 TYPE C) AND (C SUBCLASSOF R) AND (R TYPE RESTRICTION) AND (R ONPROPERTY P) AND (R HASVALUE F2)) => (F1 P F2)
        /// </summary>
        internal static RDFOntologyReasonerReport HasValueEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();

            //Fetch owl:hasValue restrictions from the class model (R, P, F2)
            var hvRestrictions = caches["TB_FreeClasses"].OfType<RDFOntologyHasValueRestriction>();
            foreach (var hvRestriction in hvRestrictions)
            {
                //Calculate subclasses of the current owl:hasValue restriction (C)
                var subClassesOfHVRestriction = ontology.Model.ClassModel.GetSubClassesOf(hvRestriction);
                foreach (var subClassOfHVRestriction in subClassesOfHVRestriction)
                {
                    //Calculate members of the current subclass of the current owl:hasValue restriction (F1)
                    var membersOfSubClassOfHVRestriction = ontology.GetMembersOf(subClassOfHVRestriction);
                    foreach (var memberOfSubClassOfHVRestriction in membersOfSubClassOfHVRestriction)
                    {
                        //Create the inference as a taxonomy entry
                        var sem_inf = new RDFOntologyTaxonomyEntry(memberOfSubClassOfHVRestriction, hvRestriction.OnProperty, hvRestriction.RequiredValue).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(HasValueEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                    }
                }
            }

            return report;
        }
        #endregion

        #region OWL2
        /// <summary>
        /// (OWL2) ReflexivePropertyEntailment implements data entailments based on 'owl:ReflexiveProperty' axiom:<br/>
        /// ((F1 P F2) AND (P TYPE REFLEXIVEPROPERTY)) => (F1 P F1)
        /// </summary>
        internal static RDFOntologyReasonerReport ReflexivePropertyEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and not-reflexive properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => prop.IsReflexiveProperty()).ToList();
            foreach (var p in availableprops)
            {

                //Filter the assertions using the current property (F1 P F2)
                var pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                //Iterate those assertions
                foreach (var pAsn in pAsns)
                {
                    //Create the inference as a taxonomy entry
                    var sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomySubject, p, pAsn.TaxonomySubject).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ReflexivePropertyEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                }

            }

            return report;
        }

        /// <summary>
        /// (OWL2) HasKeyEntailment implements data entailments based on 'owl:hasKey' axiom:<br/>
        /// ((C HASKEY P) AND (F1 TYPE C) AND (F2 TYPE C) AND (F1 P K) AND (F2 P K)) => (F1 SAMEAS F2)
        /// </summary>
        internal static RDFOntologyReasonerReport HasKeyEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var sameAs = RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty();

            foreach (var hasKeyRelation in ontology.Model.ClassModel.Relations.HasKey.GroupBy(te => te.TaxonomySubject.ToString()))
            {
                RDFOntologyClass hasKeyRelationClass = ontology.Model.ClassModel.SelectClass(hasKeyRelation.Key);

                #region Collision Detection
                //Calculate key values for members of the constrained class
                Dictionary<string, List<RDFOntologyResource>> hasKeyRelationMemberValues = ontology.GetKeyValuesOf(hasKeyRelationClass, false);

                //Reverse key values in order to detect eventual collisions between members
                Dictionary<string, List<string>> hasKeyRelationLookup = new Dictionary<string, List<string>>();
                foreach (var hasKeyRelationMemberValue in hasKeyRelationMemberValues)
                {
                    string hasKeyRelationMemberValueKey = string.Join("§§", hasKeyRelationMemberValue.Value);
                    if (!hasKeyRelationLookup.ContainsKey(hasKeyRelationMemberValueKey))
                        hasKeyRelationLookup.Add(hasKeyRelationMemberValueKey, new List<string>() { hasKeyRelationMemberValue.Key });
                    else
                        hasKeyRelationLookup[hasKeyRelationMemberValueKey].Add(hasKeyRelationMemberValue.Key);
                }
                hasKeyRelationLookup = hasKeyRelationLookup.Where(hkrl => hkrl.Value.Count > 1)
                                                           .ToDictionary(kv => kv.Key, kv => kv.Value);
                #endregion

                //Analyze detected collisions in order to decide if they can be tolerate or not,
                //depending on semantic compatibility between facts (they must not be different)
                foreach (var hasKeyRelationLookupEntry in hasKeyRelationLookup)
                {
                    #region Collision Analysis
                    for (int i = 0; i < hasKeyRelationLookupEntry.Value.Count; i++)
                    {
                        RDFOntologyFact outerFact = ontology.Data.SelectFact(hasKeyRelationLookupEntry.Value[i]);
                        for (int j = i + 1; j < hasKeyRelationLookupEntry.Value.Count; j++)
                        {
                            RDFOntologyFact innerFact = ontology.Data.SelectFact(hasKeyRelationLookupEntry.Value[j]);
                            if (RDFOntologyChecker.CheckSameAsCompatibility(ontology.Data, outerFact, innerFact))
                            {
                                //Create the inference as a taxonomy entry
                                var sem_infA = new RDFOntologyTaxonomyEntry(outerFact, sameAs, innerFact).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                                var sem_infB = new RDFOntologyTaxonomyEntry(innerFact, sameAs, outerFact).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                                //Add the inference to the report
                                if (!ontology.Data.Relations.SameAs.ContainsEntry(sem_infA))
                                    report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(HasKeyEntailment), nameof(RDFOntologyData.Relations.SameAs), sem_infA));
                                if (!ontology.Data.Relations.SameAs.ContainsEntry(sem_infB))
                                    report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(HasKeyEntailment), nameof(RDFOntologyData.Relations.SameAs), sem_infB));
                            }
                        }
                    }
                    #endregion
                }
            }

            return report;
        }

        /// <summary>
        /// (OWL2) PropertyChainEntailment implements data entailments based on 'owl:propertyChainAxiom' axiom:<br/>
        /// ((PCA PROPERTYCHAINAXIOM P1) AND (PCA PROPERTYCHAINAXIOM P2) AND (F1 P1 X) AND (X P2 F2)) => (F1 PCA F2)
        /// </summary>
        internal static RDFOntologyReasonerReport PropertyChainEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();

            Dictionary<string, RDFOntologyData> propertyChainAxiomsData = ontology.GetPropertyChainAxiomsData();
            foreach (var propertyChainAxiom in propertyChainAxiomsData)
            {
                foreach (var propertyChainAxiomAssertion in propertyChainAxiom.Value.Relations.Assertions)
                {
                    propertyChainAxiomAssertion.SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.Assertions.ContainsEntry(propertyChainAxiomAssertion))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(PropertyChainEntailment), nameof(RDFOntologyData.Relations.Assertions), propertyChainAxiomAssertion));
                }
            }

            return report;
        }

        /// <summary>
        /// (OWL2) NamedIndividualEntailment implements data entailments based on 'owl:NamedIndividual' declaration:<br/>
        /// ((F TYPE C) AND (F ISNOT BLANK) => (F TYPE NAMEDINDIVIDUAL)
        /// </summary>
        internal static RDFOntologyReasonerReport NamedIndividualEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var rdfType = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();
            var owlNamedIndividual = RDFVocabulary.OWL.NAMED_INDIVIDUAL.ToRDFOntologyClass();

            foreach (var f in ontology.Data)
            {
                if (!((RDFResource)f.Value).IsBlank)
                {
                    //Create the inference as a taxonomy entry
                    var sem_inf = new RDFOntologyTaxonomyEntry(f, rdfType, owlNamedIndividual).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(NamedIndividualEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }
            }

            return report;
        }

        /// <summary>
        /// (OWL2) HasSelfEntailment implements data entailments based on 'owl:hasSelf' restrictions:<br/>
        /// ((F TYPE C) AND (C SUBCLASSOF R) AND (R TYPE RESTRICTION) AND (R ONPROPERTY P) AND (R HASSELF TRUE)) => (F P F)
        /// </summary>
        internal static RDFOntologyReasonerReport HasSelfEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();

            //Fetch owl:hasSelf restrictions from the class model (R, P)
            var hsRestrictions = caches["TB_FreeClasses"].OfType<RDFOntologyHasSelfRestriction>();
            foreach (var hsRestriction in hsRestrictions)
            {
                //Calculate subclasses of the current owl:hasSelf restriction (C)
                var subClassesOfHSRestriction = ontology.Model.ClassModel.GetSubClassesOf(hsRestriction);
                foreach (var subClassOfHSRestriction in subClassesOfHSRestriction)
                {
                    //Calculate members of the current subclass of the current owl:hasSelf restriction (F)
                    var membersOfSubClassOfHSRestriction = ontology.GetMembersOf(subClassOfHSRestriction);
                    foreach (var memberOfSubClassOfHSRestriction in membersOfSubClassOfHSRestriction)
                    {
                        //Create the inference as a taxonomy entry
                        var sem_inf = new RDFOntologyTaxonomyEntry(memberOfSubClassOfHSRestriction, hsRestriction.OnProperty, memberOfSubClassOfHSRestriction).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(HasSelfEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                    }
                }
            }

            return report;
        }

        /// <summary>
        /// (OWL2) TopPropertyEntailment implements structural entailments based on 'owl:topObjectProperty' and 'owl:topDataProperty' subsumption:<br/>
        /// ((P TYPE OBJECTPROPERTY)   => (P SUBPROPERTYOF TOPOBJECTPROPERTY)<br/>
        /// ((P TYPE DATATYPEPROPERTY) => (P SUBPROPERTYOF TOPDATAPROPERTY)
        /// </summary>
        internal static RDFOntologyReasonerReport TopPropertyEntailmentExec(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null)
        {
            var report = new RDFOntologyReasonerReport();
            var subPropertyOf = RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty();
            var topObjectProperty = RDFVocabulary.OWL.TOP_OBJECT_PROPERTY.ToRDFOntologyObjectProperty();
            var topDataProperty = RDFVocabulary.OWL.TOP_DATA_PROPERTY.ToRDFOntologyDatatypeProperty();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and annotation properties)
            var availableprops = caches["TB_FreeProperties"].Where(prop => !prop.IsAnnotationProperty()).OfType<RDFOntologyProperty>().ToList();
            foreach (var op in availableprops.OfType<RDFOntologyObjectProperty>())
            {
                //Create the inference as a taxonomy entry
                var sem_inf = new RDFOntologyTaxonomyEntry(op, subPropertyOf, topObjectProperty).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //Add the inference to the report
                if (!ontology.Model.PropertyModel.Relations.SubPropertyOf.ContainsEntry(sem_inf))
                    report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.PropertyModel, nameof(TopPropertyEntailment), nameof(RDFOntologyPropertyModel.Relations.SubPropertyOf), sem_inf));
            }
            foreach (var dp in availableprops.OfType<RDFOntologyDatatypeProperty>())
            {
                //Create the inference as a taxonomy entry
                var sem_inf = new RDFOntologyTaxonomyEntry(dp, subPropertyOf, topDataProperty).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //Add the inference to the report
                if (!ontology.Model.PropertyModel.Relations.SubPropertyOf.ContainsEntry(sem_inf))
                    report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.PropertyModel, nameof(TopPropertyEntailment), nameof(RDFOntologyPropertyModel.Relations.SubPropertyOf), sem_inf));
            }
            return report;
        }
        #endregion

        #endregion

    }

}