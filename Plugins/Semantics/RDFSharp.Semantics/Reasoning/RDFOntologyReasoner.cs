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
        /// List of rules applied by the reasoner
        /// </summary>
        internal List<RDFOntologyReasoningRule> Rules { get; set; }
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
                    this.Rules               = new List<RDFOntologyReasoningRule>();
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
        /// Adds the given rule to the reasoner
        /// </summary>
        public RDFOntologyReasoner AddRule(RDFOntologyReasoningRule rule) {
            if(rule   != null) {
                if(this.SelectRule(rule.RuleName) == null) {
                   this.Rules.Add(rule);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given rule from the reasoner
        /// </summary>
        public RDFOntologyReasoner RemoveRule(RDFOntologyReasoningRule rule) {
            if(rule   != null) {
                if(this.SelectRule(rule.RuleName) != null) {
                   this.Rules.RemoveAll(rs => rs.RuleName.ToUpperInvariant().Equals(rule.RuleName.Trim().ToUpperInvariant(), StringComparison.Ordinal));
                }
            }
            return this;
        }

        /// <summary>
        /// Selects the given rule from the resoner
        /// </summary>
        public RDFOntologyReasoningRule SelectRule(String ruleName = "") {
            return this.Rules.Find(r => r.RuleName.ToUpperInvariant().Equals(ruleName.Trim().ToUpperInvariant(), StringComparison.Ordinal));
        }

        /// <summary>
        /// Applies the reasoner on the given ontology, producing a detailed reasoning report.
        /// </summary>
        public RDFOntologyReasoningReport ApplyToOntology(RDFOntology ontology) {
            if(ontology        != null) {
                var report      = new RDFOntologyReasoningReport(ontology.Value.PatternMemberID);

                //Sort the reasoning rules by type, in order to always start with "Standard" ones
                this.Rules.Sort((x, y) => x.RuleType.CompareTo(y.RuleType));

                //Iterate the reasoning rules for sequential execution
                foreach(var r  in this.Rules) {
                    RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Launching execution of reasoning rule '{0}'", r.RuleName));
                    var oldCnt  = report.EvidencesCount;

                    //Execute the reasoning rule
                    r.ExecuteRule(ontology, report);

                    var newCnt  = report.EvidencesCount - oldCnt;
                    RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Completed execution of reasoning rule '{0}': found {1} new evidences", r.RuleName, newCnt));
                }

                return report;
            }
            throw new RDFSemanticsException("Cannot apply RDFOntologyReasoner because given \"ontology\" parameter is null.");
        }
        #endregion

    }

}
