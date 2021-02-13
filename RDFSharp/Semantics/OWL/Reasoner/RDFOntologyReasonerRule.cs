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

using System;
using System.Collections.Generic;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasonerRule represents an inference rule executed by a reasoner
    /// </summary>
    public class RDFOntologyReasonerRule
    {

        #region Properties
        /// <summary>
        /// Name of the rule
        /// </summary>
        public string RuleName { get; internal set; }

        /// <summary>
        /// Description of the rule
        /// </summary>
        public string RuleDescription { get; internal set; }

        /// <summary>
        /// Delegate for the function which will be executed as body of the rule
        /// </summary>
        public delegate RDFOntologyReasonerReport ReasonerRuleDelegate(RDFOntology ontology, Dictionary<string, List<RDFOntologyResource>> caches = null);

        /// <summary>
        /// Function which will be executed as body of the rule
        /// </summary>
        internal ReasonerRuleDelegate ExecuteRule { get; set; }

        /// <summary>
        /// Flag indicating that the rule is standard
        /// </summary>
        internal bool IsStandardRule { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a reasoner rule with the given name and description
        /// </summary>
        public RDFOntologyReasonerRule(string ruleName, string ruleDescription, ReasonerRuleDelegate ruleDelegate)
        {
            if (!string.IsNullOrWhiteSpace(ruleName))
            {
                if (!string.IsNullOrWhiteSpace(ruleDescription))
                {
                    if (ruleDelegate != null)
                    {
                        this.RuleName = ruleName.Trim();
                        this.RuleDescription = ruleDescription.Trim();
                        this.ExecuteRule = ruleDelegate;
                        this.IsStandardRule = false;
                    }
                    else
                    {
                        throw new RDFSemanticsException("Cannot create RDFOntologyReasonerRule because given \"ruleDelegate\" parameter is null.");
                    }
                }
                else
                {
                    throw new RDFSemanticsException("Cannot create RDFOntologyReasonerRule because given \"ruleDescription\" parameter is null or empty.");
                }
            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyReasonerRule because given \"ruleName\" parameter is null or empty.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the name of the reasoner rule
        /// </summary>
        public override string ToString()
        {
            return this.RuleName;
        }

        /// <summary>
        /// Gives the full representation of the reasoner rule
        /// </summary>
        public string ToFullString()
        {
            return $"{ this.RuleName } : { this.RuleDescription }";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Redefines the reasoner rule as a standard rule
        /// </summary>
        internal RDFOntologyReasonerRule SetStandard()
        {
            this.IsStandardRule = true;
            return this;
        }
        #endregion

    }

}