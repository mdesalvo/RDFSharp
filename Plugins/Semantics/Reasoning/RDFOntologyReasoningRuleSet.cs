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

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoningRuleSet represents a set of rules available to reasoners.
    /// </summary>
    public class RDFOntologyReasoningRuleSet {

        #region Properties
        /// <summary>
        /// Name of the ruleset
        /// </summary>
        public String RuleSetName { get; internal set; }

        /// <summary>
        /// Description of the ruleset
        /// </summary>
        public String RuleSetDescription { get; internal set; }

        /// <summary>
        /// List of rules representing the ruleset
        /// </summary>
        internal List<RDFOntologyReasoningRule> Rules { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty reasoning ruleset with given name and description
        /// </summary>
        public RDFOntologyReasoningRuleSet(String rulesetName, String rulesetDescription) {
            if(rulesetName                 != null && rulesetName.Trim()        != String.Empty) {
                if(rulesetDescription      != null && rulesetDescription.Trim() != String.Empty) {                    
                    this.RuleSetName        = rulesetName.Trim();
                    this.RuleSetDescription = rulesetDescription.Trim();
                    this.Rules              = new List<RDFOntologyReasoningRule>();
                }
                else {
                    throw new RDFSemanticsException("Cannot create RDFOntologyReasoningRuleSet because given \"rulesetDescription\" parameter is null or empty.");
                }
            }
            else {
                throw new RDFSemanticsException("Cannot create RDFOntologyReasoningRuleSet because given \"rulesetName\" parameter is null or empty.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the reasoning ruleset
        /// </summary>
        public override String ToString() {
            return "RULESET " + this.RuleSetName + ": " + this.RuleSetDescription;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given rule to the ruleset
        /// </summary>
        public RDFOntologyReasoningRuleSet AddRule(RDFOntologyReasoningRule rule) {
            if(rule != null) {
                if(this.SelectRule(rule.RuleName) == null) {
                   this.Rules.Add(rule);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given rule from the ruleset
        /// </summary>
        public RDFOntologyReasoningRuleSet RemoveRule(RDFOntologyReasoningRule rule) {
            if(rule != null) {
                if(this.SelectRule(rule.RuleName) != null) {
                   this.Rules.RemoveAll(rs => rs.RuleName.ToUpperInvariant().Equals(rule.RuleName.Trim().ToUpperInvariant(), StringComparison.Ordinal));
                }
            }
            return this;
        }

        /// <summary>
        /// Selects the given rule from the ruleset
        /// </summary>
        public RDFOntologyReasoningRule SelectRule(String ruleName = "") {
            return this.Rules.Find(rs => rs.RuleName.ToUpperInvariant().Equals(ruleName.Trim().ToUpperInvariant(), StringComparison.Ordinal));
        }
        #endregion

    }

}