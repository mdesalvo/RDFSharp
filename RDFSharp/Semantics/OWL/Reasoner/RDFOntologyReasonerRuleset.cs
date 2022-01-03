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
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRuleset implements a subset of RDFS/OWL-DL/OWL2 entailment rules
    /// </summary>
    internal static class RDFOntologyReasonerRuleset
    {
        #region Methods
        /// <summary>
        /// SUBCLASS(C1,C2) ^ SUBCLASS(C2,C3) -> SUBCLASS(C1,C3)<br/>
        /// SUBCLASS(C1,C2) ^ EQUIVALENTCLASS(C2,C3) -> SUBCLASS(C1,C3)<br/>
        /// EQUIVALENTCLASS(C1,C2) ^ SUBCLASSOF(C2,C3) -> SUBCLASS(C1,C3)
        /// </summary>
        internal static RDFOntologyReasonerReport SubClassTransitivity(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty subClassOf = RDFVocabulary.RDFS.SUB_CLASS_OF.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyClass c in ontology.Model.ClassModel.Where(c => !RDFOntologyChecker.CheckReservedClass(c)))
            {
                //Enlist the superclasses of the current class
                RDFOntologyClassModel superclasses = ontology.Model.ClassModel.GetSuperClassesOf(c);
                foreach (RDFOntologyClass sc in superclasses)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(c, subClassOf, sc)
                                                            .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Model.ClassModel.Relations.SubClassOf.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.ClassModel, nameof(SubClassTransitivity), nameof(RDFOntologyClassModel.Relations.SubClassOf), sem_inf));
                }
            }
            return report;
        }

        /// <summary>
        /// EQUIVALENTCLASS(C1,C2) ^ EQUIVALENTCLASS(C2,C3) -> EQUIVALENTCLASS(C1,C3)
        /// </summary>
        internal static RDFOntologyReasonerReport EquivalentClassTransitivity(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty equivalentClass = RDFVocabulary.OWL.EQUIVALENT_CLASS.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyClass c in ontology.Model.ClassModel.Where(c => !RDFOntologyChecker.CheckReservedClass(c)))
            {
                //Enlist the equivalent classes of the current class
                RDFOntologyClassModel equivclasses = ontology.Model.ClassModel.GetEquivalentClassesOf(c);
                foreach (RDFOntologyClass ec in equivclasses)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(c, equivalentClass, ec)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(ec, equivalentClass, c)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

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
		/// EQUIVALENTCLASS(C1,C2) ^ DISJOINTWITH(C2,C3) -> DISJOINTWITH(C1,C3)<br/>
		/// SUBCLASS(C1,C2) ^ DISJOINTWITH(C2,C3) -> DISJOINTWITH(C1,C3)<br/>
		/// DISJOINTWITH(C1,C2) ^ EQUIVALENTCLASS(C2,C3) -> DISJOINTWITH(C1,C3)
		/// </summary>
        internal static RDFOntologyReasonerReport DisjointWithEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty disjointWith = RDFVocabulary.OWL.DISJOINT_WITH.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyClass c in ontology.Model.ClassModel.Where(c => !RDFOntologyChecker.CheckReservedClass(c)))
            {
                //Enlist the disjoint classes of the current class
                RDFOntologyClassModel disjclasses = ontology.Model.ClassModel.GetDisjointClassesWith(c);
                foreach (RDFOntologyClass dwc in disjclasses)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(c, disjointWith, dwc)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(dwc, disjointWith, c)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

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
        /// SUBPROPERTY(P1,P2) ^ SUBPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)<br/>
        /// SUBPROPERTY(P1,P2) ^ EQUIVALENTPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)<br/>
        /// EQUIVALENTPROPERTY(P1,P2) ^ SUBPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)
        /// </summary>
        internal static RDFOntologyReasonerReport SubPropertyTransitivity(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty subPropertyOf = RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyProperty p in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                        && !p.IsAnnotationProperty()))
            {
                //Enlist the superproperties of the current property
                RDFOntologyPropertyModel superprops = ontology.Model.PropertyModel.GetSuperPropertiesOf(p);
                foreach (RDFOntologyProperty sp in superprops)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(p, subPropertyOf, sp)
                                                            .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Model.PropertyModel.Relations.SubPropertyOf.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.PropertyModel, nameof(SubPropertyTransitivity), nameof(RDFOntologyPropertyModel.Relations.SubPropertyOf), sem_inf));
                }
            }
            return report;
        }

        /// <summary>
        /// EQUIVALENTPROPERTY(P1,P2) ^ EQUIVALENTPROPERTY(P2,P3) -> EQUIVALENTPROPERTY(P1,P3)
        /// </summary>
        internal static RDFOntologyReasonerReport EquivalentPropertyTransitivity(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty equivProperty = RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyProperty p in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                        && !p.IsAnnotationProperty()))
            {
                //Enlist the equivalent properties of the current property
                RDFOntologyPropertyModel equivprops = ontology.Model.PropertyModel.GetEquivalentPropertiesOf(p);
                foreach (RDFOntologyProperty ep in equivprops)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(p, equivProperty, ep)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(ep, equivProperty, p)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

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
        /// P(F1,F2) ^ DOMAIN(P,C) -> C(F1)
        /// </summary>
        internal static RDFOntologyReasonerReport DomainEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty type = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyProperty p in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                        && !p.IsAnnotationProperty()
                                                                                            && p.Domain != null))
            {
                //Filter the assertions using the current property
                RDFOntologyTaxonomy pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                //Iterate the related assertions
                foreach (RDFOntologyTaxonomyEntry pAsn in pAsns)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomySubject, type, p.Domain)
                                                                .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(DomainEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }
            }
            return report;
        }

        /// <summary>
        /// P(F1,F2) ^ RANGE(P,C) -> C(F2)
        /// </summary>
        internal static RDFOntologyReasonerReport RangeEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty type = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyProperty p in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                        && !p.IsAnnotationProperty()
                                                                                            && p.Range != null))
            {
                //Filter the assertions using the current property
                RDFOntologyTaxonomy pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                //Iterate the related assertions
                foreach (RDFOntologyTaxonomyEntry pAsn in pAsns.Where(x => x.TaxonomyObject.IsFact()))
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomyObject, type, p.Range)
                                                             .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(RangeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }
            }
            return report;
        }

        /// <summary>
        /// SAMEAS(F1,F2) ^ SAMEAS(F2,F3) -> SAMEAS(F1,F3)
        /// </summary>
        internal static RDFOntologyReasonerReport SameAsTransitivity(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty sameAs = RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyFact f in ontology.Data)
            {
                //Enlist the same facts of the current fact
                RDFOntologyData samefacts = ontology.Data.GetSameFactsAs(f);
                foreach (RDFOntologyFact sf in samefacts)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(f, sameAs, sf)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(sf, sameAs, f)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

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
        /// SAMEAS(F1,F2) ^ DIFFERENTFROM(F2,F3) -> DIFFERENTFROM(F1,F3)<br/>
        /// DIFFERENTFROM(F1,F2) ^ SAMEAS(F2,F3) -> DIFFERENTFROM(F1,F3)
        /// </summary>
        internal static RDFOntologyReasonerReport DifferentFromEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty differentFrom = RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty();
            foreach (RDFOntologyFact f in ontology.Data)
            {
                //Enlist the different facts of the current fact
                RDFOntologyData differfacts = ontology.Data.GetDifferentFactsFrom(f);
                foreach (RDFOntologyFact df in differfacts)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(f, differentFrom, df)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                    RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(df, differentFrom, f)
                                                              .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

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
        /// C1(F) ^ SUBCLASSOF(C1,C2) -> C2(F)<br/>
        /// C1(F) ^ EQUIVALENTCLASS(C1,C2) -> C2(F)
        /// </summary>
        internal static RDFOntologyReasonerReport ClassTypeEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty type = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();
            Dictionary<long, RDFOntologyData> membersCache = new Dictionary<long, RDFOntologyData>();

            //Calculate the set of available classes on which to perform the reasoning (exclude BASE classes and literal-compatible classes)
            List<RDFOntologyClass> availableclasses = ontology.Model.ClassModel.Where(c => !RDFOntologyChecker.CheckReservedClass(c)
                                                                                               && !ontology.Model.ClassModel.CheckIsLiteralCompatibleClass(c)).ToList();

            //Evaluate simple classes
            foreach (RDFOntologyClass c in availableclasses.Where(cls => cls.IsSimpleClass()))
            {
                //Enlist the members of the current class
                if (!membersCache.ContainsKey(c.PatternMemberID))
                    membersCache.Add(c.PatternMemberID, ontology.GetMembersOfClass(c));
                foreach (RDFOntologyFact f in membersCache[c.PatternMemberID])
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(f, type, c)
                                                             .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ClassTypeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }
            }

            //Evaluate enumerations
            foreach (RDFOntologyClass c in availableclasses.Where(cls => cls.IsEnumerateClass()))
            {
                //Enlist the members of the current enumeration
                if (!membersCache.ContainsKey(c.PatternMemberID))
                    membersCache.Add(c.PatternMemberID, ontology.GetMembersOfEnumerate((RDFOntologyEnumerateClass)c));
                foreach (RDFOntologyFact f in membersCache[c.PatternMemberID])
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(f, type, c)
                                                             .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ClassTypeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }
            }

            //Evaluate restrictions
            foreach (RDFOntologyClass c in availableclasses.Where(cls => cls.IsRestrictionClass()))
            {
                //Enlist the members of the current restriction
                if (!membersCache.ContainsKey(c.PatternMemberID))
                    membersCache.Add(c.PatternMemberID, ontology.GetMembersOfRestriction((RDFOntologyRestriction)c));
                foreach (RDFOntologyFact f in membersCache[c.PatternMemberID])
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(f, type, c)
                                                             .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ClassTypeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }
            }

            //Evaluate composite classes
            foreach (RDFOntologyClass c in availableclasses.Where(cls => cls.IsCompositeClass()))
            {
                //Enlist the members of the current composite class
                if (!membersCache.ContainsKey(c.PatternMemberID))
                    membersCache.Add(c.PatternMemberID, ontology.GetMembersOfComposite(c, membersCache));
                foreach (RDFOntologyFact f in membersCache[c.PatternMemberID])
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(f, type, c)
                                                             .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ClassTypeEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                }
            }

            return report;
        }

        /// <summary>
        /// C(F) -> NAMEDINDIVIDUAL(F)
        /// </summary>
        internal static RDFOntologyReasonerReport NamedIndividualEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty rdfType = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();
            RDFOntologyClass owlNamedIndividual = RDFVocabulary.OWL.NAMED_INDIVIDUAL.ToRDFOntologyClass();

            foreach (RDFOntologyFact f in ontology.Data.Where(x => !((RDFResource)x.Value).IsBlank))
            {
                //Create the inference as a taxonomy entry
                RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(f, rdfType, owlNamedIndividual)
                                                         .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //Add the inference to the report
                report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(NamedIndividualEntailment), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
            }

            return report;
        }

        /// <summary>
        /// P(F1,F2) ^ SYMMETRICPROPERTY(P) -> P(F2,F1)
        /// </summary>
        internal static RDFOntologyReasonerReport SymmetricPropertyEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            foreach (RDFOntologyProperty p in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                        && p.IsSymmetricProperty()))
            {
                //Filter the assertions using the current property (F1 P F2)
                RDFOntologyTaxonomy pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                //Iterate those assertions
                foreach (RDFOntologyTaxonomyEntry pAsn in pAsns.Where(x => x.TaxonomyObject.IsFact()))
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomyObject, p, pAsn.TaxonomySubject)
                                                             .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(SymmetricPropertyEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                }
            }

            return report;
        }

        /// <summary>
        /// P(F1,F2) ^ P(F2,F3) ^ TRANSITIVEPROPERTY(P) -> P(F1,F3)
        /// </summary>
        internal static RDFOntologyReasonerReport TransitivePropertyEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            Dictionary<long, RDFOntologyData> transPropCache = new Dictionary<long, RDFOntologyData>();

            foreach (RDFOntologyProperty p in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                         && p.IsTransitiveProperty()))
            {
                //Filter the assertions using the current property
                RDFOntologyTaxonomy pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                //Iterate those assertions
                foreach (RDFOntologyTaxonomyEntry pAsn in pAsns.Where(x => x.TaxonomyObject.IsFact()))
                {
                    if (!transPropCache.ContainsKey(pAsn.TaxonomySubject.PatternMemberID))
                        transPropCache.Add(pAsn.TaxonomySubject.PatternMemberID, ontology.Data.GetTransitiveAssertionsOf((RDFOntologyFact)pAsn.TaxonomySubject, (RDFOntologyObjectProperty)p));
                    foreach (RDFOntologyFact te in transPropCache[pAsn.TaxonomySubject.PatternMemberID])
                    {
                        //Create the inference as a taxonomy entry
                        RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomySubject, p, te)
                                                                 .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(TransitivePropertyEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                    }
                }
                transPropCache.Clear();
            }

            return report;
        }

        /// <summary>
        /// P(F1,F2) ^ REFLEXIVEPROPERTY(P) -> P(F1,F1)
        /// </summary>
        internal static RDFOntologyReasonerReport ReflexivePropertyEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            foreach (RDFOntologyProperty p in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                         && p.IsReflexiveProperty()))
            {
                //Filter the assertions using the current property
                RDFOntologyTaxonomy pAsns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p);

                //Iterate those assertions
                foreach (RDFOntologyTaxonomyEntry pAsn in pAsns)
                {
                    //Create the inference as a taxonomy entry
                    RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(pAsn.TaxonomySubject, p, pAsn.TaxonomySubject)
                                                             .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(ReflexivePropertyEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                }
            }

            return report;
        }

        /// <summary>
        /// P1(F1,F2) ^ INVERSE(P1,P2) -> P2(F2,F1)
        /// </summary>
        internal static RDFOntologyReasonerReport InverseOfEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            foreach (RDFOntologyProperty p1 in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                          && p.IsObjectProperty()))
            {
                //Filter the assertions using the current property
                RDFOntologyTaxonomy p1Asns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p1);

                //Enlist the inverse properties of the current property
                RDFOntologyPropertyModel inverseprops = ontology.Model.PropertyModel.GetInversePropertiesOf((RDFOntologyObjectProperty)p1);
                foreach (RDFOntologyProperty p2 in inverseprops.Where(x => x.IsObjectProperty()))
                {
                    //Iterate the compatible assertions
                    foreach (RDFOntologyTaxonomyEntry p1Asn in p1Asns.Where(x => x.TaxonomyObject.IsFact()))
                    {
                        //Create the inference as a taxonomy entry
                        RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(p1Asn.TaxonomyObject, p2, p1Asn.TaxonomySubject)
                                                                 .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(InverseOfEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                    }
                }
            }

            return report;
        }

        /// <summary>
        /// P1(F1,F2) ^ SUBPROPERTY(P1,P2) -> P2(F1,F2)<br/>
        /// P1(F1,F2) ^ EQUIVALENTPROPERTY(P1,P2) -> P2(F1,F2)
        /// </summary>
        internal static RDFOntologyReasonerReport PropertyEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            foreach (RDFOntologyProperty p1 in ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                          && !p.IsAnnotationProperty()))
            {
                //Filter the assertions using the current property (F1 P1 F2)
                RDFOntologyTaxonomy p1Asns = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p1);

                //Enlist the compatible properties of the current property (P1 [SUBPROPERTYOF|EQUIVALENTPROPERTY] P2)
                foreach (RDFOntologyProperty p2 in ontology.Model.PropertyModel.GetSuperPropertiesOf(p1)
                                                                               .UnionWith(ontology.Model.PropertyModel.GetEquivalentPropertiesOf(p1)))
                {
                    //Iterate the compatible assertions
                    foreach (RDFOntologyTaxonomyEntry p1Asn in p1Asns)
                    {
                        //Taxonomy-check for securing inference consistency
                        if ((p2.IsObjectProperty() && p1Asn.TaxonomyObject.IsFact())
                                || (p2.IsDatatypeProperty() && p1Asn.TaxonomyObject.IsLiteral()))
                        {
                            //Create the inference as a taxonomy entry
                            RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(p1Asn.TaxonomySubject, p2, p1Asn.TaxonomyObject)
                                                                     .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

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
        /// P(F1,F2) ^ SAMEAS(F1,F3) -> P(F3,F2)<br/>
        /// P(F1,F2) ^ SAMEAS(F2,F3) -> P(F1,F3)
        /// </summary>
        internal static RDFOntologyReasonerReport SameAsEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            foreach (RDFOntologyFact f1 in ontology.Data)
            {
                //Enlist the same facts of the current fact
                RDFOntologyData sameFacts = ontology.Data.GetSameFactsAs(f1);
                if (sameFacts.FactsCount > 0)
                {
                    //Filter the assertions using the current fact
                    RDFOntologyTaxonomy f1AsnsSubj = ontology.Data.Relations.Assertions.SelectEntriesBySubject(f1);
                    RDFOntologyTaxonomy f1AsnsObj = ontology.Data.Relations.Assertions.SelectEntriesByObject(f1);

                    //Enlist the same facts of the current fact
                    foreach (RDFOntologyFact f2 in sameFacts)
                    {
                        #region Subject-Side
                        //Iterate the assertions having the current fact as subject
                        foreach (RDFOntologyTaxonomyEntry f1Asn in f1AsnsSubj)
                        {
                            //Taxonomy-check for securing inference consistency
                            if (f1Asn.TaxonomyPredicate.IsObjectProperty() && f1Asn.TaxonomyObject.IsFact())
                            {
                                //Create the inference as a taxonomy entry
                                RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(f2, f1Asn.TaxonomyPredicate, f1Asn.TaxonomyObject)
                                                                          .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                                //Add the inference to the report
                                if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_infA))
                                    report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(SameAsEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_infA));
                            }   
                        }
                        #endregion

                        #region Object-Side
                        //Iterate the assertions having the current fact as object
                        foreach (RDFOntologyTaxonomyEntry f1Asn in f1AsnsObj)
                        {
                            //Taxonomy-check for securing inference consistency
                            if (f1Asn.TaxonomyPredicate.IsObjectProperty())
                            {
                                //Create the inference as a taxonomy entry
                                RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(f1Asn.TaxonomySubject, f1Asn.TaxonomyPredicate, f2)
                                                                          .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

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
        /// C(F1) ^ SUBCLASS(C,R) ^ RESTRICTION(R) ^ ONPROPERTY(R,P) ^ HASVALUE(R,F2) -> P(F1,F2)
        /// </summary>
        internal static RDFOntologyReasonerReport HasValueEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            //Fetch owl:hasValue restrictions from the class model (R, P, F2)
            IEnumerable<RDFOntologyHasValueRestriction> hvRestrictions = ontology.Model.ClassModel.Where(c => !RDFOntologyChecker.CheckReservedClass(c))
                                                                                                  .OfType<RDFOntologyHasValueRestriction>();
            foreach (RDFOntologyHasValueRestriction hvRestriction in hvRestrictions)
            {
                //Calculate subclasses of the current owl:hasValue restriction (C)
                RDFOntologyClassModel subClassesOfHVRestriction = ontology.Model.ClassModel.GetSubClassesOf(hvRestriction);
                foreach (RDFOntologyClass subClassOfHVRestriction in subClassesOfHVRestriction)
                {
                    //Calculate members of the current subclass of the current owl:hasValue restriction (F1)
                    RDFOntologyData membersOfSubClassOfHVRestriction = ontology.GetMembersOf(subClassOfHVRestriction);
                    foreach (RDFOntologyFact memberOfSubClassOfHVRestriction in membersOfSubClassOfHVRestriction)
                    {
                        //Create the inference as a taxonomy entry
                        RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(memberOfSubClassOfHVRestriction, hvRestriction.OnProperty, hvRestriction.RequiredValue)
                                                                 .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(HasValueEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                    }
                }
            }

            return report;
        }

        /// <summary>
        /// C(F) ^ SUBCLASS(C,R) ^ RESTRICTION(R) ^ ONPROPERTY(R,P) ^ HASSELF(R,"TRUE") -> P(F,F)
        /// </summary>
        internal static RDFOntologyReasonerReport HasSelfEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            //Fetch owl:hasSelf restrictions from the class model (R, P)
            IEnumerable<RDFOntologyHasSelfRestriction> hsRestrictions = ontology.Model.ClassModel.Where(c => !RDFOntologyChecker.CheckReservedClass(c))
                                                                                                 .OfType<RDFOntologyHasSelfRestriction>();
            foreach (RDFOntologyHasSelfRestriction hsRestriction in hsRestrictions)
            {
                //Calculate subclasses of the current owl:hasSelf restriction (C)
                RDFOntologyClassModel subClassesOfHSRestriction = ontology.Model.ClassModel.GetSubClassesOf(hsRestriction);
                foreach (RDFOntologyClass subClassOfHSRestriction in subClassesOfHSRestriction)
                {
                    //Calculate members of the current subclass of the current owl:hasSelf restriction (F)
                    RDFOntologyData membersOfSubClassOfHSRestriction = ontology.GetMembersOf(subClassOfHSRestriction);
                    foreach (RDFOntologyFact memberOfSubClassOfHSRestriction in membersOfSubClassOfHSRestriction)
                    {
                        //Create the inference as a taxonomy entry
                        RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(memberOfSubClassOfHSRestriction, hsRestriction.OnProperty, memberOfSubClassOfHSRestriction)
                                                                 .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(HasSelfEntailment), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                    }
                }
            }

            return report;
        }

        /// <summary>
        /// HASKEY(C,P) ^ C(F1) ^ C(F2) ^ P(F1,"K") ^ P(F2,"K") -> SAMEAS(F1,F2)
        /// </summary>
        internal static RDFOntologyReasonerReport HasKeyEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty sameAs = RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty();

            foreach (IGrouping<string, RDFOntologyTaxonomyEntry> hasKeyRelation in ontology.Model.ClassModel.Relations.HasKey.GroupBy(te => te.TaxonomySubject.ToString()))
            {
                RDFOntologyClass hasKeyRelationClass = ontology.Model.ClassModel.SelectClass(hasKeyRelation.Key);

                #region Collision Detection
                //Calculate key values for members of the constrained class
                Dictionary<string, List<RDFOntologyResource>> hasKeyRelationMemberValues = ontology.GetKeyValuesOf(hasKeyRelationClass, false);

                //Reverse key values in order to detect eventual collisions between members
                Dictionary<string, List<string>> hasKeyRelationLookup = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, List<RDFOntologyResource>> hasKeyRelationMemberValue in hasKeyRelationMemberValues)
                {
                    string hasKeyRelationMemberValueKey = string.Join("��", hasKeyRelationMemberValue.Value);
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
                foreach (KeyValuePair<string, List<string>> hasKeyRelationLookupEntry in hasKeyRelationLookup)
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
                                RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(outerFact, sameAs, innerFact)
                                                                          .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                                RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(innerFact, sameAs, outerFact)
                                                                          .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

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
        /// PROPERTYCHAINAXIOM(PCA) ^ MEMBER(PCA,P1) ^ MEMBER(PCA,P2) ^ P1(F1,X) ^ P2(X,F2) -> PCA(F1,F2)
        /// </summary>
        internal static RDFOntologyReasonerReport PropertyChainEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            Dictionary<string, RDFOntologyData> propertyChainAxiomsData = ontology.GetPropertyChainAxiomsData();
            foreach (KeyValuePair<string,RDFOntologyData> propertyChainAxiom in propertyChainAxiomsData)
            {
                foreach (RDFOntologyTaxonomyEntry propertyChainAxiomAssertion in propertyChainAxiom.Value.Relations.Assertions)
                {
                    propertyChainAxiomAssertion.SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                    //Add the inference to the report
                    if (!ontology.Data.Relations.Assertions.ContainsEntry(propertyChainAxiomAssertion))
                        report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, nameof(PropertyChainEntailment), nameof(RDFOntologyData.Relations.Assertions), propertyChainAxiomAssertion));
                }
            }

            return report;
        }
        #endregion
    }
}