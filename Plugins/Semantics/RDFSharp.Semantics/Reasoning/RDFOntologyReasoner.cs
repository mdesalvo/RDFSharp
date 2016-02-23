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
using System.Linq;
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
        /// Applies the reasoner on the given ontology, producing a reasoning report.
        /// The ontology is progressively enriched with discovered inferences.
        /// </summary>
        public RDFOntologyReasoningReport ApplyToOntology(RDFOntology ontology, RDFOntologyValidationReport vReport) {
            if (ontology       != null) {
                if(vReport     != null && vReport.ValidationReportID == ontology.PatternMemberID) {
                    var rReport = new RDFOntologyReasoningReport(ontology.Value.PatternMemberID);

                    //Step 1: Inflate the ontology class model
                    RDFSemanticsUtilities.TriggerRule("EquivalentClassTransitivity",    this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("SubClassTransitivity",           this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("DisjointWithEntailment",         this, ontology, rReport);

                    //Step 2: Inflate the ontology property model
                    RDFSemanticsUtilities.TriggerRule("EquivalentPropertyTransitivity", this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("SubPropertyTransitivity",        this, ontology, rReport);

                    //Step 3: Inflate the ontology data
                    RDFSemanticsUtilities.TriggerRule("SameAsTransitivity",             this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("DifferentFromEntailment",        this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("ClassTypeEntailment",            this, ontology, rReport);

                    //Step 4: Launch the standard rules (first round)
                    RDFSemanticsUtilities.TriggerRule("DomainEntailment",               this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("RangeEntailment",                this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("InverseOfEntailment",            this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("SymmetricPropertyEntailment",    this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("PropertyEntailment",             this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("TransitivePropertyEntailment",   this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("SameAsEntailment",               this, ontology, rReport);

                    //Step 5: Launch the standard rules (recursive round)
                    //TODO

                    //Step 6: Launch the user-defined rules
                    foreach(var sr in this.Rules.Where(r => r.RuleType == RDFSemanticsEnums.RDFOntologyReasoningRuleType.UserDefined)) {
                        RDFSemanticsUtilities.TriggerRule(sr.RuleName, this, ontology, rReport);
                    }

                    return rReport;
                }
                throw new RDFSemanticsException("Cannot apply RDFOntologyReasoner because given \"vReport\" parameter is null or does not represent a validation report of the given ontology.");
            }
            throw new RDFSemanticsException("Cannot apply RDFOntologyReasoner because given \"ontology\" parameter is null.");
        }
        #endregion

    }

}
