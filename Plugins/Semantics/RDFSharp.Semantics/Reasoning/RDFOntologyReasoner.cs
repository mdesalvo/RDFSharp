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
        /// Options of the reasoner
        /// </summary>
        public RDFOntologyReasonerOptions ReasonerOptions { get; internal set; }

        /// <summary>
        /// List of rules applied by the reasoner
        /// </summary>
        internal List<RDFOntologyReasoningRule> Rules { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology reasoner with given name, descriptions and options
        /// </summary>
        public RDFOntologyReasoner(String reasonerName, 
                                   String reasonerDescription,
                                   RDFOntologyReasonerOptions reasonerOptions=null) {
            if(reasonerName                 != null && reasonerName.Trim()        != String.Empty) {
                if(reasonerDescription      != null && reasonerDescription.Trim() != String.Empty) {
                    this.ReasonerName        = reasonerName;
                    this.ReasonerDescription = reasonerDescription;
                    this.ReasonerOptions     = (reasonerOptions ?? new RDFOntologyReasonerOptions());
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
            if (ontology         != null) {
                if (vReport      != null && vReport.ValidationReportID == ontology.PatternMemberID) {
                    var rReport   = new RDFOntologyReasoningReport(ontology.Value.PatternMemberID);

                    //Step 0.A: Raise warning reasoning concerns
                    var warnCount = vReport.SelectWarnings().Count;
                    if (warnCount > 0) {
                        RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Inference process is going to start on ontology '{0}', for which the validation report indicates {1} warning evidences: this MAY generate potentially wrong and/or inconsistent inferences!", ontology, warnCount));
                    }

                    //Step 0.B: Raise error reasoning concerns
                    var errCount  = vReport.SelectErrors().Count;
                    if (errCount  > 0) {
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Inference process is going to start on ontology '{0}', for which the validation report indicates {1} error evidences: this WILL generate wrong and/or inconsistent inferences!", ontology, errCount));
                    }

                    //Step 1: Inflate ontology class model
                    RDFSemanticsUtilities.TriggerRule("EquivalentClassTransitivity",    this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("SubClassTransitivity",           this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("DisjointWithEntailment",         this, ontology, rReport);

                    //Step 2: Inflate ontology property model
                    RDFSemanticsUtilities.TriggerRule("EquivalentPropertyTransitivity", this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("SubPropertyTransitivity",        this, ontology, rReport);

                    //Step 3: Inflate ontology data
                    RDFSemanticsUtilities.TriggerRule("SameAsTransitivity",             this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("DifferentFromEntailment",        this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("ClassTypeEntailment",            this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("DomainEntailment",               this, ontology, rReport);
                    RDFSemanticsUtilities.TriggerRule("RangeEntailment",                this, ontology, rReport);

                    //Step 4: Trigger standard rules
                    var stepCnt   = 0;
                    var infCnt    = new List<Boolean>() { false, false, false, false, false };
                    while (true)  {
                        infCnt[0] = RDFSemanticsUtilities.TriggerRule("InverseOfEntailment",          this, ontology, rReport);
                        infCnt[1] = RDFSemanticsUtilities.TriggerRule("SymmetricPropertyEntailment",  this, ontology, rReport);
                        infCnt[2] = RDFSemanticsUtilities.TriggerRule("TransitivePropertyEntailment", this, ontology, rReport);
                        infCnt[3] = RDFSemanticsUtilities.TriggerRule("PropertyEntailment",           this, ontology, rReport);
                        infCnt[4] = RDFSemanticsUtilities.TriggerRule("SameAsEntailment",             this, ontology, rReport);

                        //Exit Condition A: none of the rules have produced new inferences
                        if (infCnt.TrueForAll(inf => inf == false)) {
                            break;
                        }

                        //Exit Condition B: recursion limit option is enabled and have been reached
                        if (this.ReasonerOptions.EnableRecursionLimit) {
                            stepCnt      = stepCnt + 1;
                            if (stepCnt == 3) {
                                break;
                            }
                        }

                    }

                    //Step 5: Trigger user-defined rules
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

    /// <summary>
    /// RDFOntologyReasonerOptions represents a set of options applied to a reasoner
    /// </summary>
    public class RDFOntologyReasonerOptions {

        #region Properties
        /// <summary>
        /// Flag enabling the limit of recursion during application of standard rules
        /// </summary>
        public Boolean EnableRecursionLimit { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a predefined reasoner options object
        /// </summary>
        public RDFOntologyReasonerOptions() {
            this.EnableRecursionLimit = true;
        }
        #endregion

    }

}
