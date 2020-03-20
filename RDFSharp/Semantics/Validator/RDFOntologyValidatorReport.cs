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
using System.Collections;
using System.Collections.Generic;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFOntologyValidatorReport represents a detailed report of an ontology analysis.
    /// </summary>
    public sealed class RDFOntologyValidatorReport: IEnumerable<RDFOntologyValidatorEvidence> {

        #region Properties
        /// <summary>
        /// Counter of the evidences
        /// </summary>
        public Int32 EvidencesCount {
            get { return this.Evidences.Count; }
        }

        /// <summary>
        /// Gets an enumerator on the evidences for iteration
        /// </summary>
        public IEnumerator<RDFOntologyValidatorEvidence> EvidencesEnumerator {
            get { return this.Evidences.GetEnumerator(); }
        }

        /// <summary>
        /// List of evidences
        /// </summary>
        internal List<RDFOntologyValidatorEvidence> Evidences { get; set; }

        /// <summary>
        /// SyncLock for evidences
        /// </summary>
        internal Object SyncLock { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty report
        /// </summary>
        internal RDFOntologyValidatorReport() {
            this.Evidences = new List<RDFOntologyValidatorEvidence>();
            this.SyncLock  = new Object();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the validation report's evidences
        /// </summary>
        IEnumerator<RDFOntologyValidatorEvidence> IEnumerable<RDFOntologyValidatorEvidence>.GetEnumerator() {
            return this.EvidencesEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the validation report's evidences
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.EvidencesEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given evidence to the validation report
        /// </summary>
        internal void AddEvidence(RDFOntologyValidatorEvidence evidence) {
            lock (this.SyncLock) { 
                  this.Evidences.Add(evidence);
            }
        }

        /// <summary>
        /// Merges the evidences of the given report
        /// </summary>
        internal void MergeEvidences(RDFOntologyValidatorReport report) {
            lock (this.SyncLock) {
                  this.Evidences.AddRange(report.Evidences);
            }
        }
        #endregion

        #region Select
        /// <summary>
        /// Enlist the names of the rules applied by the validator
        /// </summary>
        public List<String> EnlistRuleNames() {
            return new List<String>() {
                "Vocabulary_Reservation",
                "Vocabulary_Disjointness",
                "Vocabulary_Declaration",
                "Domain_Range",
                "InverseOf",
                "SymmetricProperty",
                "ClassType",
                "GlobalCardinalityConstraint",
                "LocalCardinalityConstraint",
                "Deprecation"
            };
        }

        /// <summary>
        /// Gets the warning evidences from the validation report
        /// </summary>
        public List<RDFOntologyValidatorEvidence> SelectWarnings() {
            return this.Evidences.FindAll(e => e.EvidenceCategory == RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning);
        }

        /// <summary>
        /// Gets the warning evidences of the given validation rule
        /// </summary>
        public List<RDFOntologyValidatorEvidence> SelectWarningsByRule(String rulename="") {
            return this.Evidences.FindAll(e => e.EvidenceProvenance.ToUpperInvariant().Equals(rulename.Trim().ToUpperInvariant(), StringComparison.Ordinal) && 
                                               e.EvidenceCategory.Equals(RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning));
        }

        /// <summary>
        /// Gets the error evidences from the validation report
        /// </summary>
        public List<RDFOntologyValidatorEvidence> SelectErrors() {
            return this.Evidences.FindAll(e => e.EvidenceCategory == RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error);
        }

        /// <summary>
        /// Gets the error evidences of the given validation rule
        /// </summary>
        public List<RDFOntologyValidatorEvidence> SelectErrorsByRule(String rulename = "") {
            return this.Evidences.FindAll(e => e.EvidenceProvenance.ToUpperInvariant().Equals(rulename.Trim().ToUpperInvariant(), StringComparison.Ordinal) &&
                                               e.EvidenceCategory.Equals(RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error));
        }
        #endregion

        #endregion

    }

}