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
using System.Collections;
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoningReport represents a detailed report of an ontology reasoner's activity.
    /// </summary>
    public class RDFOntologyReasoningReport:IEnumerable<RDFOntologyReasoningEvidence> {

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
        public IEnumerator<RDFOntologyReasoningEvidence> EvidencesEnumerator {
            get { return this.Evidences.GetEnumerator(); }
        }

        /// <summary>
        /// List of evidences
        /// </summary>
        internal List<RDFOntologyReasoningEvidence> Evidences { get; set; }

        /// <summary>
        /// Synchronization lock
        /// </summary>
        internal Object SyncLock { get; set; }

        /// <summary>
        /// Identifier of the validaton report
        /// </summary>
        internal Int64 ReasoningReportID { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty reasoning report
        /// </summary>
        internal RDFOntologyReasoningReport(Int64 reportID) {
            this.ReasoningReportID = reportID;
            this.SyncLock          = new Object();
            this.Evidences         = new List<RDFOntologyReasoningEvidence>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the reasoning report's evidences
        /// </summary>
        IEnumerator<RDFOntologyReasoningEvidence> IEnumerable<RDFOntologyReasoningEvidence>.GetEnumerator() {
            return this.Evidences.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the reasoning report's evidences
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Evidences.GetEnumerator();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the evidences having the given category
        /// </summary>
        public List<RDFOntologyReasoningEvidence> SelectEvidencesByCategory(RDFSemanticsEnums.RDFOntologyReasoningEvidenceCategory evidenceCategory) {
            return this.Evidences.FindAll(e => e.EvidenceCategory.Equals(evidenceCategory));
        }

        /// <summary>
        /// Gets the evidences having the given provenance rule
        /// </summary>
        public List<RDFOntologyReasoningEvidence> SelectEvidencesByProvenance(String evidenceProvenance = "") {
            return this.Evidences.FindAll(e => e.EvidenceProvenance.ToUpper().Equals(evidenceProvenance.Trim().ToUpperInvariant(), StringComparison.Ordinal));
        }

        /// <summary>
        /// Gets the evidences having the given content subject
        /// </summary>
        public List<RDFOntologyReasoningEvidence> SelectEvidencesByContentSubject(RDFOntologyResource evidenceContentSubject) {
            return this.Evidences.FindAll(e => e.EvidenceContent.TaxonomySubject.Equals(evidenceContentSubject));
        }

        /// <summary>
        /// Gets the evidences having the given content predicate
        /// </summary>
        public List<RDFOntologyReasoningEvidence> SelectEvidencesByContentPredicate(RDFOntologyResource evidenceContentPredicate) {
            return this.Evidences.FindAll(e => e.EvidenceContent.TaxonomyPredicate.Equals(evidenceContentPredicate));
        }

        /// <summary>
        /// Gets the evidences having the given content object
        /// </summary>
        public List<RDFOntologyReasoningEvidence> SelectEvidencesByContentObject(RDFOntologyResource evidenceContentObject) {
            return this.Evidences.FindAll(e => e.EvidenceContent.TaxonomyObject.Equals(evidenceContentObject));
        }

        /// <summary>
        /// Adds the given evidence to the reasoning report
        /// </summary>
        internal void AddEvidence(RDFOntologyReasoningEvidence evidence) {
            lock(this.SyncLock) {
                 this.Evidences.Add(evidence);
            }
        }
        #endregion

    }

}