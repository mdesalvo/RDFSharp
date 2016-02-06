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
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoner represents an inference engine applied on a given ontology
    /// </summary>
    public class RDFOntologyReasoner {

        #region Properties
        /// <summary>
        /// Name of the reasoner
        /// </summary>
        public String ReasonerName { get; internal set; }

        /// <summary>
        /// Description of the reasoner
        /// </summary>
        public String ReasonerDescription { get; internal set; }

        /// <summary>
        /// List of rulesets applied by the ontology validator
        /// </summary>
        internal List<RDFOntologyReasoningRuleSet> RuleSets { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology reasoner
        /// </summary>
        public RDFOntologyReasoner(String reasonerName, String reasonerDescription) {
            if(reasonerName                 != null && reasonerName.Trim()        != String.Empty) {
                if(reasonerDescription      != null && reasonerDescription.Trim() != String.Empty) {
                    this.ReasonerName        = reasonerName;
                    this.ReasonerDescription = reasonerDescription;
                    this.RuleSets            = new List<RDFOntologyReasoningRuleSet>();
                }
                else {
                    throw new RDFSemanticsException("Cannot create RDFOntologyReasoner because given \"reasonerDescription\" parameter is null or empty.");
                }
            }
            else {
                throw new RDFSemanticsException("Cannot create RDFOntologyReasoner because given \"reasonerName\" parameter is null or empty.");
            }            
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given ruleset to the reasoner
        /// </summary>
        public RDFOntologyReasoner AddRuleSet(RDFOntologyReasoningRuleSet ruleSet) {
            if(ruleSet != null) {
                if(this.SelectRuleSet(ruleSet.RuleSetName) == null) {
                   this.RuleSets.Add(ruleSet);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given ruleset from the reasoner
        /// </summary>
        public RDFOntologyReasoner RemoveRuleSet(RDFOntologyReasoningRuleSet ruleSet) {
            if(ruleSet != null) {
                if(this.SelectRuleSet(ruleSet.RuleSetName) != null) {
                   this.RuleSets.RemoveAll(rs => rs.RuleSetName.ToUpperInvariant().Equals(ruleSet.RuleSetName.Trim().ToUpperInvariant(), StringComparison.Ordinal));
                }
            }
            return this;
        }

        /// <summary>
        /// Selects the given ruleset from the resoner
        /// </summary>
        public RDFOntologyReasoningRuleSet SelectRuleSet(String ruleSetName = "") {
            return this.RuleSets.Find(rs => rs.RuleSetName.ToUpperInvariant().Equals(ruleSetName.Trim().ToUpperInvariant(), StringComparison.Ordinal));
        }

        /// <summary>
        /// Applies the reasoner on the given ontology, producing a detailed reasoning report.
        /// The ontology is progressively enriched with inferences discovered during the process.
        /// </summary>
        public RDFOntologyReasoningReport ApplyToOntology(RDFOntology ontology) {
            if(ontology   != null) {
                var report = new RDFOntologyReasoningReport(ontology.Value.PatternMemberID);

                //TODO: Rulesets execution


                return report;
            }
            throw new RDFSemanticsException("Cannot apply RDFOntologyReasoner because given \"ontology\" parameter is null.");
        }
        #endregion

    }

}
