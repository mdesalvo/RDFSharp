﻿/*
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

using System;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFOntologyReasonerRule represents an inference rule executed by a reasoner
    /// </summary>
    public class RDFOntologyReasonerRule {

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
        /// Execution priority of the rule (less is more priority)
        /// </summary>
        public UInt32 RulePriority { get; internal set; }

        /// <summary>
        /// Delegate for the function which will be executed as body of the rule
        /// </summary>
        public delegate RDFOntologyReasonerReport ReasonerRuleDelegate(RDFOntology ontology);

        /// <summary>
        /// Function which will be executed as body of the rule
        /// </summary>
        internal ReasonerRuleDelegate ExecuteRule { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a reasoner rule with the given name, description and priority (lower is better)
        /// </summary>
        public RDFOntologyReasonerRule(String ruleName, 
                                       String ruleDescription, 
                                       UInt32 rulePriority, 
                                       ReasonerRuleDelegate ruleDelegate) {
            if (ruleName                     != null && ruleName.Trim()        != String.Empty) {
                if (ruleDescription          != null && ruleDescription.Trim() != String.Empty) {
                    if (ruleDelegate         != null) {
                        this.RuleName         = ruleName.Trim();
                        this.RuleDescription  = ruleDescription.Trim();

                        //Shift-up rule priority to guarantee preliminar execution of BASE rules
                        if (rulePriority     <= RDFOntologyReasonerRuleset.RulesCount)
                            this.RulePriority = rulePriority + (UInt32)RDFOntologyReasonerRuleset.RulesCount + 1;
                        else
                            this.RulePriority = rulePriority;

                        this.ExecuteRule      = ruleDelegate;
                    }
                    else {
                        throw new RDFSemanticsException("Cannot create RDFOntologyReasonerRule because given \"ruleDelegate\" parameter is null.");
                    }
                }
                else {
                    throw new RDFSemanticsException("Cannot create RDFOntologyReasonerRule because given \"ruleDescription\" parameter is null or empty.");
                }
            }
            else {
                throw new RDFSemanticsException("Cannot create RDFOntologyReasonerRule because given \"ruleName\" parameter is null or empty.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the name of the reasoner rule
        /// </summary>
        public override String ToString() {
            return this.RuleName;
        }

        /// <summary>
        /// Gives the full representation of the reasoner rule
        /// </summary>
        public String ToFullString() {
            return this.RuleName + " (PRIORITY " + this.RulePriority + "): " + this.RuleDescription;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Redefines the execution priority of the reasoner rule
        /// </summary>
        internal RDFOntologyReasonerRule SetPriority(UInt32 priority) {
            this.RulePriority = priority;
            return this;
        }
        #endregion

    }

}