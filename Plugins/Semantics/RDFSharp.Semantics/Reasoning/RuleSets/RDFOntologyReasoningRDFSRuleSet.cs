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

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoningStandardRuleSets represents an helper exposing standard reasoning rulesets
    /// </summary>
    public static partial class RDFOntologyReasoningStandardRuleSets {

        /// <summary>
        /// RDFSRuleSet represents the standard RDFS ruleset
        /// </summary>
        public class RDFSRuleSet {

            #region Properties
            /// <summary>
            /// Singleton instance of the RDFSRuleSet class
            /// </summary>
            public static RDFSRuleSet Instance { get; internal set; }

            /// <summary>
            /// RuleSet containing the RDFS reasoning rules
            /// </summary>
            internal RDFOntologyReasoningRuleSet RuleSet { get; set; }
            #endregion

            #region Ctors
            /// <summary>
            /// Default-ctor to initialize the singleton instance of the RDFS ruleset
            /// </summary>
            static RDFSRuleSet() {

                //Create the RDFS ruleset
                RDFSRuleSet.Instance         = new RDFSRuleSet();
                RDFSRuleSet.Instance.RuleSet = new RDFOntologyReasoningRuleSet("RDFS", "This ruleset implements the RDFS entailment rules");

                //Add the RDFS reasoning rules
                RDFSRuleSet.Instance.RuleSet.AddRule(
                    new RDFOntologyReasoningRule("SubClassTransitivity",
                                                 "SubClassTransitivity implements possible paths of 'rdfs:subClassOf' subsumption:" +
                                                 "((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)"      +
                                                 "((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)"      +
                                                 "((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)",
                                                 RDFSRuleSet.SubClassTransitivity));

            }
            #endregion

            #region Rules
            /// <summary>
            /// SubClassTransitivity implements possible paths of 'rdfs:subClassOf' subsumption:
            /// ((C1 SUBCLASSOF C2)      AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)
            /// ((C1 SUBCLASSOF C2)      AND (C2 EQUIVALENTCLASS C3)) => (C1 SUBCLASSOF C3)
            /// ((C1 EQUIVALENTCLASS C2) AND (C2 SUBCLASSOF C3))      => (C1 SUBCLASSOF C3)
            /// </summary>
            public static void SubClassTransitivity(RDFOntology ontology, RDFOntologyReasoningReport report) {
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
            #endregion

        }

    }    

}