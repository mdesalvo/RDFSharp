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
    internal static class RDFOntologyReasonerRuleset
    {
        #region Methods
        /// <summary>
        /// SubClassTransitivity (rdfs11) implements structural entailments based on SubClassOf model taxonomy<br/>
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
        /// EquivalentClassTransitivity implements structural entailments based on EquivalentClass model taxonomy<br/>
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
		/// DisjointWithEntailment implements structural entailments based on DisjointWith model taxonomy<br/>
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
        /// SubPropertyTransitivity (RDFS-5) implements structural entailments based on SubPropertyOf model taxonomy<br/>
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
        /// EquivalentPropertyTransitivity implements structural entailments based on EquivalentProperty model taxonomy<br/>
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
        /// DomainEntailment (RDFS-2) implements structural entailments based on Domain model informations<br/>
        /// P(F1,F2) ^ DOMAIN(P,C) -> TYPE(F1,C)
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
        /// RangeEntailment (RDFS-3) implements structural entailments based on Range model informations<br/>
        /// P(F1,F2) ^ RANGE(P,C) -> TYPE(F2,C)"
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
        /// SameAsTransitivity implements structural entailments based on SameAs data taxonomy<br/>
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
        /// DifferentFromEntailment implements structural entailments based on DifferentFrom data taxonomy<br/>
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
        /// ClassTypeEntailment (RDFS-9) implements structural entailments based on Type data taxonomy<br/>
        /// C1(F) ^ SUBCLASSOF(C1,C2) -> C2(F)<br/>
        /// C1(F) ^ EQUIVALENTCLASS(C1,C2) -> C2(F)
        /// </summary>
        internal static RDFOntologyReasonerReport ClassTypeEntailment(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty type = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();
            Dictionary<long, RDFOntologyData> membersCache = new Dictionary<long, RDFOntologyData>();

            //Calculate the set of available classes on which to perform the reasoning (exclude BASE classes and literal-compatible classes)
            IEnumerable<RDFOntologyClass> availableclasses = ontology.Model.ClassModel.Where(c => !RDFOntologyChecker.CheckReservedClass(c)
                                                                                                    && !ontology.Model.ClassModel.CheckIsLiteralCompatibleClass(c));

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
        #endregion
    }

}