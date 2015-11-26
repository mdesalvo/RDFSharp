/*
   Copyright 2012-2015 Marco De Salvo

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoningRuleSet represents a set of RDFS/OWL-DL rules which are applied by a reasoner 
    /// on a given ontology in order to materialize semantic inferences discovered within its model and data.
    /// </summary>
    internal class RDFOntologyReasoningRuleSet {

        #region Properties
        /// <summary>
        /// List of rules contained in the reasoning ruleset
        /// </summary>
        internal List<RDFOntologyReasoningRule> Rules { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a predefined reasoning ruleset
        /// </summary>
        internal RDFOntologyReasoningRuleSet() {
            this.Rules = new List<RDFOntologyReasoningRule>() { 
            
                //ClassModel_SubClassTransitivity
                new RDFOntologyReasoningRule(
                    "ClassModel_SubClassTransitivity", 
                    "((C1 [EQUIVALENTCLASS|SUBCLASSOF] C2) AND (C2 SUBCLASSOF C3)) => (C1 SUBCLASSOF C3)",
                    RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel,
                    RDFOntologyReasoningRuleSet.ClassModel_SubClassTransitivity),

                //ClassModel_EquivalentClassTransitivity
                new RDFOntologyReasoningRule(
                    "ClassModel_EquivalentClassTransitivity", 
                    "((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)",
                    RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel,
                    RDFOntologyReasoningRuleSet.ClassModel_EquivalentClassTransitivity),

                //ClassModel_DisjointWithInference
                new RDFOntologyReasoningRule(
                    "ClassModel_DisjointWithInference", 
                    "((C1 [EQUIVALENTCLASS|SUBCLASSOF] C2) AND (C2 DISJOINTWITH C3)) => (C1 DISJOINTWITH C3)",
                    RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel,
                    RDFOntologyReasoningRuleSet.ClassModel_DisjointWithInference)
            
            };
        }
        #endregion

        #region Methods

        #region Rule:ClassModel_SubClassTransitivity
        /// <summary>
        /// ((C1 [EQUIVALENTCLASS|SUBCLASSOF] C2) AND (C2 SUBCLASSOF C3)) => (C1 SUBCLASSOF C3)
        /// </summary>
        internal static List<RDFOntologyReasoningEvidence> ClassModel_SubClassTransitivity(RDFOntology ontology) {
            var evidences        = new List<RDFOntologyReasoningEvidence>();
            foreach (var c      in ontology.Model.ClassModel) {
                var supcls       = RDFOntologyHelper.EnlistSuperClassesOf(c, ontology.Model.ClassModel);
                foreach (var sc in supcls) {
                    var scTax    = new RDFOntologyTaxonomyEntry(c, RDFOntologyVocabulary.ObjectProperties.SUB_CLASS_OF, sc);
                    ontology.Model.ClassModel.Relations.SubClassOf.AddEntry(scTax.SetInference(true));
                }
            }
            return evidences;
        }
        #endregion

        #region Rule:ClassModel_EquivalentClassTransitivity
        /// <summary>
        /// ((C1 EQUIVALENTCLASS C2) AND (C2 EQUIVALENTCLASS C3)) => (C1 EQUIVALENTCLASS C3)
        /// </summary>
        internal static List<RDFOntologyReasoningEvidence> ClassModel_EquivalentClassTransitivity(RDFOntology ontology) {
            var evidences        = new List<RDFOntologyReasoningEvidence>();
            foreach (var c in ontology.Model.ClassModel) {
                var eqcls        = RDFOntologyHelper.EnlistEquivalentClassesOf(c, ontology.Model.ClassModel);
                foreach (var ec in eqcls) {
                    var ecTax    = new RDFOntologyTaxonomyEntry(c, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_CLASS, ec);
                    ontology.Model.ClassModel.Relations.EquivalentClass.AddEntry(ecTax.SetInference(true));
                }
            }
            return evidences;
        }
        #endregion

        #region Rule:ClassModel_DisjointWithInference
        /// <summary>
        /// ((C1 [EQUIVALENTCLASS|SUBCLASSOF] C2) AND (C2 DISJOINTWITH C3)) => (C1 DISJOINTWITH C3)
        /// </summary>
        internal static List<RDFOntologyReasoningEvidence> ClassModel_DisjointWithInference(RDFOntology ontology) {
            var evidences        = new List<RDFOntologyReasoningEvidence>();
            foreach (var c in ontology.Model.ClassModel) {
                var dwcls        = RDFOntologyHelper.EnlistDisjointClassesWith(c, ontology.Model.ClassModel);
                foreach (var dc in dwcls) {
                    var dcTax    = new RDFOntologyTaxonomyEntry(c, RDFOntologyVocabulary.ObjectProperties.DISJOINT_WITH, dc);
                    ontology.Model.ClassModel.Relations.DisjointWith.AddEntry(dcTax.SetInference(true));
                }
            }
            return evidences;
        }
        #endregion

        #endregion

    }

}