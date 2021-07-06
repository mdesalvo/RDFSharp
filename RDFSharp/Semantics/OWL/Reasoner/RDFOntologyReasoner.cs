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
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasoner represents a SWRL inference engine applied on a given ontology
    /// </summary>
    public class RDFOntologyReasoner : IEnumerable<RDFOntologyReasonerRule>
    {
        #region Properties
        /// <summary>
        /// Count of the rules composing the reasoner
        /// </summary>
        public int RulesCount => this.Rules.Count;

        /// <summary>
        /// Gets the enumerator on the reasoner's rules for iteration
        /// </summary>
        public IEnumerator<RDFOntologyReasonerRule> RulesEnumerator => this.Rules.GetEnumerator();

        /// <summary>
        /// List of rules applied by the reasoner
        /// </summary>
        internal List<RDFOntologyReasonerRule> Rules { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology reasoner
        /// </summary>
        public RDFOntologyReasoner()
            => this.Rules = new List<RDFOntologyReasonerRule>();
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the reasoner's rules
        /// </summary>
        IEnumerator<RDFOntologyReasonerRule> IEnumerable<RDFOntologyReasonerRule>.GetEnumerator() => this.RulesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the reasoner's rules
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => this.RulesEnumerator;
        #endregion

        #region Methods

        /// <summary>
        /// Adds the given rule to the reasoner
        /// </summary>
        public RDFOntologyReasoner AddRule(RDFOntologyReasonerRule rule)
        {
            if (rule != null && !this.Rules.Any(r => r.RuleName.Equals(rule.RuleName, StringComparison.OrdinalIgnoreCase)))
                this.Rules.Add(rule);
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
        public RDFOntologyReasonerReport ApplyToOntology(RDFOntology ontology, RDFOntologyReasonerOptions ruleOptions)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            if (ontology != null)
            {
                RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Reasoner is going to be applied on Ontology '{0}': this may require intensive processing, depending on size and complexity of domain knowledge and rules.", ontology.Value));

                RDFOntology expOntology = ontology.UnionWith(RDFBASEOntology.Instance);
                Parallel.ForEach(this.Rules, rule =>
                {
                    RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Launching reasoner rule '{0}'...", rule));

                    RDFOntologyReasonerReport ruleReport = rule.ApplyToOntology(expOntology, ruleOptions);
                    report.Merge(ruleReport);

                    RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Completed reasoner rule '{0}': found {1} evidences.", rule, ruleReport.EvidencesCount));
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
        public Task<RDFOntologyReasonerReport> ApplyToOntologyAsync(RDFOntology ontology, RDFOntologyReasonerOptions ruleOptions)
            => Task.Run(() => ApplyToOntology(ontology, ruleOptions));

        #endregion
    }

}