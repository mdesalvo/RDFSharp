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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFOntologyReasoner represents an inference engine applied on a given ontology
    /// </summary>
    public class RDFOntologyReasoner : IEnumerable<RDFOntologyReasonerRule> {

        #region Properties
        /// <summary>
        /// Count of the rules composing the reasoner
        /// </summary>
        public Int32 RulesCount {
            get { return this.Rules.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the reasoner's rules for iteration
        /// </summary>
        public IEnumerator<RDFOntologyReasonerRule> RulesEnumerator {
            get { return this.Rules.GetEnumerator(); }
        }

        /// <summary>
        /// List of rules applied by the reasoner
        /// </summary>
        internal List<RDFOntologyReasonerRule> Rules { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology reasoner
        /// </summary>
        public RDFOntologyReasoner() {
            this.Rules = new List<RDFOntologyReasonerRule>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the reasoner's rules
        /// </summary>
        IEnumerator<RDFOntologyReasonerRule> IEnumerable<RDFOntologyReasonerRule>.GetEnumerator() {
            return this.RulesEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the reasoner's rules
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.RulesEnumerator;
        }
        #endregion

        #region Methods

        #region Create
        /// <summary>
        /// Adds the given rule to the reasoner
        /// </summary>
        public RDFOntologyReasoner AddRule(RDFOntologyReasonerRule rule) {
            if (rule != null) {
                if (this.SelectRule(rule.RuleName) == null) {
                    this.Rules.Add(rule);
                }
            }
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the given rule from the resoner
        /// </summary>
        public RDFOntologyReasonerRule SelectRule(String ruleName) {
            if (ruleName  != null &&  ruleName.Trim() != String.Empty)
                return this.Rules.FirstOrDefault(r => r.RuleName.Equals(ruleName.Trim(), StringComparison.OrdinalIgnoreCase));
            else
                return null;
        }
        #endregion

        #region Reason
        /// <summary>
        /// Applies the reasoner on the given ontology, producing a reasoning report.
        /// </summary>
        public RDFOntologyReasonerReport ApplyToOntology(ref RDFOntology ontology) {
            var report              = new RDFOntologyReasonerReport();
            if (ontology           != null) {
                RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Reasoner is going to be applied on Ontology '{0}'...", ontology.Value));

                //STEP 1: Expand ontology
                var ontologyValue   = ontology.Value;
                ontology            = ontology.UnionWith(RDFBASEOntology.Instance);

                //STEP 2: Execute BASE rules
                #region BASE rules
                var baseRules       = this.Rules.Where(x   => x.RulePriority <= RDFOntologyReasonerRuleset.RulesCount)
                                                .OrderBy(x => x.RulePriority);
                foreach (var bRule in baseRules) {
                    RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Launching execution of BASE reasoning rule '{0}'...", bRule));
                    var bRuleReport = bRule.ExecuteRule(ontology);
                    report.Merge(bRuleReport);
                    RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Completed execution of BASE reasoning rule '{0}': found {1} evidences.", bRule, bRuleReport.EvidencesCount));
                }
                #endregion

                //STEP 3: Execute custom rules
                #region Custom rules
                var customRules     = this.Rules.Where(x   => x.RulePriority > RDFOntologyReasonerRuleset.RulesCount)
                                                .OrderBy(x => x.RulePriority);
                foreach (var cRule in customRules) {
                    RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Launching execution of reasoning rule '{0}'...", cRule));
                    var cRuleReport = cRule.ExecuteRule(ontology);
                    report.Merge(cRuleReport);
                    RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Completed execution of reasoning rule '{0}': found {1} evidences.", cRule, cRuleReport.EvidencesCount));
                }
                #endregion

                //STEP 4: Unexpand ontology
                ontology            = ontology.DifferenceWith(RDFBASEOntology.Instance);
                ontology.Value      = ontologyValue;

                RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Reasoner has been applied on Ontology '{0}': found " + report.EvidencesCount + " evidences.", ontology.Value));
            }
            return report;
        }
        #endregion

        #endregion

    }

}