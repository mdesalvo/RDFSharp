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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyValidatorRule represents a rule which analyzes a specific syntactic/semantic aspect of an ontology.
    /// </summary>
    internal class RDFOntologyValidatorRule
    {

        #region Properties
        /// <summary>
        /// Name of the rule
        /// </summary>
        internal string RuleName { get; set; }

        /// <summary>
        /// Description of the rule
        /// </summary>
        internal string RuleDescription { get; set; }

        /// <summary>
        /// Delegate for the function which will be executed as body of the rule
        /// </summary>
        internal delegate RDFOntologyValidatorReport ValidationRuleDelegate(RDFOntology ontology);

        /// <summary>
        /// Function which will be executed as body of the rule
        /// </summary>
        internal ValidationRuleDelegate ExecuteRule { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty validation rule with given name, description and delegate
        /// </summary>
        internal RDFOntologyValidatorRule(string ruleName,
                                          string ruleDescription,
                                          ValidationRuleDelegate ruleDelegate)
        {
            this.RuleName = ruleName;
            this.RuleDescription = ruleDescription;
            this.ExecuteRule = ruleDelegate;
        }
        #endregion

    }

}