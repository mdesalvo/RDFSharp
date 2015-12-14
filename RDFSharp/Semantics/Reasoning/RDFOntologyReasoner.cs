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
using System.Threading;
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoner analyzes a given ontology through a set of RDFS/OWL-DL rules
    /// in order to materialize semantic inferences discovered within its model and data.
    /// </summary>
    internal class RDFOntologyReasoner {

        #region Properties
        /// <summary>
        /// Set of RDFS/OWL-DL rules characterizing this reasoner
        /// </summary>
        internal RDFOntologyReasoningRuleSet RuleSet { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a reasoner with predefined set of RDFS/OWL-DL rules
        /// </summary>
        internal RDFOntologyReasoner() {
            this.RuleSet = new RDFOntologyReasoningRuleSet();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Runs the ruleset on the given ontology to materialize inferred knowledge
        /// </summary>
        internal void AnalyzeOntology(RDFOntology ontology) {
            
        }
        #endregion

    }

}