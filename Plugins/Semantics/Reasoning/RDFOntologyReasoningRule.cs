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
    /// RDFOntologyReasoningRule represents an inference rule executed by a reasoner
    /// </summary>
    public class RDFOntologyReasoningRule {

        #region Properties
        /// <summary>
        /// Name of the rule
        /// </summary>
        public String RuleName { get; internal set; }

        /// <summary>
        /// Description of the rule
        /// </summary>
        public String RuleDescription { get; internal set; }

        /// <summary>
        /// Delegate for the function which will be executed as body of the rule
        /// </summary>
        public delegate Int64 RuleDelegate(RDFOntology ontology);

        /// <summary>
        /// Function which will be executed as body of the rule
        /// </summary>
        internal RuleDelegate ExecuteRule { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty reasoning rule with given name, description, category and delegate
        /// </summary>
        public RDFOntologyReasoningRule(String ruleName, String ruleDescription, RuleDelegate ruleDelegate) {
            if(ruleName                     != null && ruleName.Trim()        != String.Empty) {
                if(ruleDescription          != null && ruleDescription.Trim() != String.Empty) {
                    if(ruleDelegate         != null) {
                        this.RuleName        = ruleName.Trim();
                        this.RuleDescription = ruleDescription.Trim();
                        this.ExecuteRule     = ruleDelegate;
                    }
                    else {
                        throw new RDFSemanticsException("Cannot create RDFOntologyReasoningRule because given \"ruleDelegate\" parameter is null.");
                    }
                }
                else {
                    throw new RDFSemanticsException("Cannot create RDFOntologyReasoningRule because given \"ruleDescription\" parameter is null or empty.");
                }
            }
            else {
                throw new RDFSemanticsException("Cannot create RDFOntologyReasoningRule because given \"ruleName\" parameter is null or empty.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the reasoning rule
        /// </summary>
        public override String ToString() {
            return "RULE " + this.RuleName + ": " + this.RuleDescription;
        }
        #endregion

        #region Methods
        #endregion

    }

}
