/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasoner represents a SWRL inference engine applied on a given ontology
    /// </summary>
    public class RDFOntologyReasoner
    {
        #region Properties
        /// <summary>
        /// List of standard rules applied by the reasoner
        /// </summary>
        internal List<RDFSemanticsEnums.RDFOntologyStandardReasonerRule> StandardRules { get; set; }

        /// <summary>
        /// List of custom rules applied by the reasoner
        /// </summary>
        internal List<RDFOntologyReasonerRule> CustomRules { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology reasoner
        /// </summary>
        public RDFOntologyReasoner()
        {
            this.StandardRules = new List<RDFSemanticsEnums.RDFOntologyStandardReasonerRule>();
            this.CustomRules = new List<RDFOntologyReasonerRule>();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Adds the given standard rule to the reasoner
        /// </summary>
        public RDFOntologyReasoner AddStandardRule(RDFSemanticsEnums.RDFOntologyStandardReasonerRule standardRule)
        {
            if (!this.StandardRules.Contains(standardRule))
                this.StandardRules.Add(standardRule);
            return this;
        }

        /// <summary>
        /// Adds the given custom rule to the reasoner
        /// </summary>
        public RDFOntologyReasoner AddCustomRule(RDFOntologyReasonerRule customRule)
        {
            if (customRule != null && !this.CustomRules.Any(r => r.RuleUri.ToString().Equals(customRule.RuleUri.ToString(), StringComparison.OrdinalIgnoreCase)))
                this.CustomRules.Add(customRule);
            return this;
        }

        /// <summary>
        /// Applies the reasoner on the given ontology
        /// </summary>
        public RDFOntologyReasonerReport ApplyToOntology(RDFOntology ontology)
            => ApplyToOntology(ontology, new RDFOntologyReasonerOptions());

        /// <summary>
        /// Applies the reasoner on the given ontology with given options
        /// </summary>
        public RDFOntologyReasonerReport ApplyToOntology(RDFOntology ontology, RDFOntologyReasonerOptions options)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            if (ontology != null)
            {
                RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Reasoner is going to be applied on Ontology '{0}': this may require intensive processing, depending on size and complexity of domain knowledge and rules.", ontology.Value));

                RDFOntology tempOntology = ontology.UnionWith(RDFBASEOntology.Instance);

                //Standard Rules
                Parallel.ForEach(this.StandardRules, standardRule =>
                {
                    RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Launching standard rule '{0}'", standardRule));

                    #region Exec
                    RDFOntologyReasonerReport standardRuleReport = new RDFOntologyReasonerReport();
                    switch (standardRule)
                    {                        
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.SubClassTransitivity:
                            standardRuleReport = RDFOntologyReasonerRuleset.SubClassTransitivity(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.EquivalentClassTransitivity:
                            standardRuleReport = RDFOntologyReasonerRuleset.EquivalentClassTransitivity(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.DisjointWithEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.DisjointWithEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.SubPropertyTransitivity:
                            standardRuleReport = RDFOntologyReasonerRuleset.SubPropertyTransitivity(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.EquivalentPropertyTransitivity:
                            standardRuleReport = RDFOntologyReasonerRuleset.EquivalentPropertyTransitivity(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.DomainEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.DomainEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.RangeEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.RangeEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.SameAsTransitivity:
                            standardRuleReport = RDFOntologyReasonerRuleset.SameAsTransitivity(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.DifferentFromEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.DifferentFromEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.ClassTypeEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.ClassTypeEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.NamedIndividualEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.NamedIndividualEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.SymmetricPropertyEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.SymmetricPropertyEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.TransitivePropertyEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.TransitivePropertyEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.ReflexivePropertyEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.ReflexivePropertyEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.InverseOfEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.InverseOfEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.PropertyEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.PropertyEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.SameAsEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.SameAsEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.HasValueEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.HasValueEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.HasSelfEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.HasSelfEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.HasKeyEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.HasKeyEntailment(tempOntology);
                            break;
                        case RDFSemanticsEnums.RDFOntologyStandardReasonerRule.PropertyChainEntailment:
                            standardRuleReport = RDFOntologyReasonerRuleset.PropertyChainEntailment(tempOntology);
                            break;
                    }
                    report.Merge(standardRuleReport);
                    #endregion

                    RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Completed standard rule '{0}': found {1} evidences.", standardRule, standardRuleReport.EvidencesCount));
                });

                //Custom Rules
                Parallel.ForEach(this.CustomRules, rule =>
                {
                    RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Launching custom rule '{0}': {1}", rule.RuleUri, rule));

                    #region Exec
                    RDFOntologyReasonerReport customRuleReport = rule.ApplyToOntology(tempOntology, options);
                    report.Merge(customRuleReport);
                    #endregion

                    RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Completed custom rule '{0}': found {1} evidences.", rule.RuleUri, customRuleReport.EvidencesCount));
                });

                RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Reasoner has been applied on Ontology '{0}': found " + report.EvidencesCount + " unique evidences.", ontology.Value));
            }
            return report;
        }

        /// <summary>
        /// Asynchronously applies the reasoner on the given ontology
        /// </summary>
        public Task<RDFOntologyReasonerReport> ApplyToOntologyAsync(RDFOntology ontology)
            => ApplyToOntologyAsync(ontology, new RDFOntologyReasonerOptions());

        /// <summary>
        /// Asynchronously applies the reasoner on the given ontology with the given options
        /// </summary>
        public Task<RDFOntologyReasonerReport> ApplyToOntologyAsync(RDFOntology ontology, RDFOntologyReasonerOptions options)
            => Task.Run(() => ApplyToOntology(ontology, options));

        #endregion
    }

}