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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyValidator analyzes a given ontology through a predefined set of RDFS/OWL-DL rules
    /// in order to find error and inconsistency evidences affecting its model and data.
    /// </summary>
    internal class RDFOntologyValidator {

        #region Properties
        /// <summary>
        /// List of rules applied by the ontology validator
        /// </summary>
        internal List<RDFOntologyValidationRule> Rules { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an ontology validator
        /// </summary>
        internal RDFOntologyValidator() {
            this.Rules = new List<RDFOntologyValidationRule>() {
            
                //Vocabulary_Reservation
                new RDFOntologyValidationRule(
                    "Vocabulary_Reservation", 
                    "This rule checks for usage of reserved RDF/RDFS/OWL terms in vocabulary of classes, properties and facts",
                    RDFOntologyValidationRuleSet.Vocabulary_Reservation),

                //Vocabulary_Disjointness
                new RDFOntologyValidationRule(
                    "Vocabulary_Disjointness", 
                    "This rule checks for disjointness of vocabulary of classes, properties and facts",
                    RDFOntologyValidationRuleSet.Vocabulary_Disjointness),

                //Vocabulary_Declaration
                new RDFOntologyValidationRule(
                    "Vocabulary_Declaration", 
                    "This rule checks for complete declaration of classes, properties and facts",
                    RDFOntologyValidationRuleSet.Vocabulary_Declaration),

                //Domain_Range
                new RDFOntologyValidationRule(
                    "Domain_Range", 
                    "This rule checks for consistency of rdfs:domain and rdfs:range axioms",
                    RDFOntologyValidationRuleSet.Domain_Range),

                //ClassType
                new RDFOntologyValidationRule(
                    "ClassType", 
                    "This rule checks for consistency of rdf:type axioms",
                    RDFOntologyValidationRuleSet.ClassType),

                //GlobalCardinalityConstraint
                new RDFOntologyValidationRule(
                    "GlobalCardinalityConstraint", 
                    "This rule checks for consistency of global cardinality constraints",
                    RDFOntologyValidationRuleSet.GlobalCardinalityConstraint),

                //LocalCardinalityConstraint
                new RDFOntologyValidationRule(
                    "LocalCardinalityConstraint", 
                    "This rule checks for consistency of local cardinality constraints",
                    RDFOntologyValidationRuleSet.LocalCardinalityConstraint),

                //Deprecation
                new RDFOntologyValidationRule(
                    "Deprecation", 
                    "This rule checks for usage of deprecated classes and properties",
                    RDFOntologyValidationRuleSet.Deprecation)

            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Analyzes the given ontology and produces a detailed report of found evidences
        /// </summary>
        internal RDFOntologyValidationReport AnalyzeOntology(RDFOntology ontology) {
            var report = new RDFOntologyValidationReport(ontology.Value.PatternMemberID);

            //Step 1: Create the cache for storing the evidences found by the rules
            var rDict  = new Dictionary<RDFOntologyValidationRule, List<RDFOntologyValidationEvidence>>();
            foreach (var rule in this.Rules) {
                rDict.Add(rule, new List<RDFOntologyValidationEvidence>());
            }

            //Step 2: Parallelize the execution of the rules on the given ontology
            Parallel.ForEach(this.Rules, rule => {
                rDict[rule] = rule.ExecuteRule(ontology);
            });

            //Step 3: Merge the evidences into the validator report
            foreach (var rule in rDict) {
                foreach (var evidence in rule.Value) {
                    report.Evidences.Add(evidence);
                }
            }

            return report;
        }
        #endregion

    }

}