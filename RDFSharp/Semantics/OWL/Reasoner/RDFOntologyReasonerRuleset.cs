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
        /// SubPropertyTransitivity (RDFS-5) implements structural entailments based on SubPropertyOf model taxonomy<br/>
        /// SUBPROPERTY(P1,P2) ^ SUBPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)<br/>
        /// SUBPROPERTY(P1,P2) ^ EQUIVALENTPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)<br/>
        /// EQUIVALENTPROPERTY(P1,P2) ^ SUBPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)
        /// </summary>
        internal static RDFOntologyReasonerReport SubPropertyTransitivity(RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            RDFOntologyObjectProperty subPropertyOf = RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty();

            //Calculate the set of available properties on which to perform the reasoning (exclude BASE properties and annotation properties)
            IEnumerable<RDFOntologyProperty> availableprops = ontology.Model.PropertyModel.Where(p => !RDFOntologyChecker.CheckReservedProperty(p)
                                                                                                        && !p.IsAnnotationProperty()).ToList();
            foreach (RDFOntologyProperty p in availableprops)
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
        #endregion
    }

}