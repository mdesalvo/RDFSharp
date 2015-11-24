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
                    "((C1 SUBCLASSOF C2) AND (C2 SUBCLASSOF C3)) => (C1 SUBCLASSOF C3)",
                    RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory.ClassModel,
                    RDFOntologyReasoningRuleSet.ClassModel_SubClassTransitivity)
            
            };
        }
        #endregion

        #region Methods

        #region ClassModel_SubClassTransitivity
        /// <summary>
        /// ((C1 SUBCLASSOF C2) AND (C2 SUBCLASSOF C3)) => (C1 SUBCLASSOF C3)
        /// </summary>
        internal static List<RDFOntologyReasoningEvidence> ClassModel_SubClassTransitivity(RDFOntology ontology) {
            var evidences = new List<RDFOntologyReasoningEvidence>();

            return evidences;
        }
        #endregion

        #endregion

    }

}