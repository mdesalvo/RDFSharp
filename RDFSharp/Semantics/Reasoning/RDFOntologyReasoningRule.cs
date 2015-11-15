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
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoningRule represents a rule which calculates a specific semantic inference on an ontology.
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
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty reasoning rule with given name and description
        /// </summary>
        public RDFOntologyReasoningRule(String ruleName,
                                        String ruleDescription) {
            this.RuleName        = ruleName;
            this.RuleDescription = ruleDescription;
        }
        #endregion

        #region Interfaces

        #endregion

        #region Methods

        #endregion

    }

}