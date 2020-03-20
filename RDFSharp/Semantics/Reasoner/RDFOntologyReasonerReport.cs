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
    /// RDFOntologyReasonerReport represents a detailed report of an ontology reasoner's activity.
    /// </summary>
    public sealed class RDFOntologyReasonerReport: IEnumerable<RDFOntologyReasonerEvidence> {

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
        public IEnumerator<RDFOntologyReasonerEvidence> EvidencesEnumerator {
            get { return this.Evidences.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Dictionary of evidences
        /// </summary>
        internal Dictionary<Int64, RDFOntologyReasonerEvidence> Evidences { get; set; }

        /// <summary>
        /// SyncLock for evidences
        /// </summary>
        internal Object SyncLock { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty reasoner report
        /// </summary>
        internal RDFOntologyReasonerReport() {
            this.Evidences = new Dictionary<Int64, RDFOntologyReasonerEvidence>();
            this.SyncLock  = new Object();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the reasoner report's evidences
        /// </summary>
        IEnumerator<RDFOntologyReasonerEvidence> IEnumerable<RDFOntologyReasonerEvidence>.GetEnumerator() {
            return this.EvidencesEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the reasoner report's evidences
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.EvidencesEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given evidence to the reasoner report
        /// </summary>
        internal void AddEvidence(RDFOntologyReasonerEvidence evidence) {
            lock (this.SyncLock) { 
                  if (!this.Evidences.ContainsKey(evidence.EvidenceContent.TaxonomyEntryID)) { 
                       this.Evidences.Add(evidence.EvidenceContent.TaxonomyEntryID, evidence);
                  }
            }
        }

        /// <summary>
        /// Merges the evidences of the given report
        /// </summary>
        internal void Merge(RDFOntologyReasonerReport report) {
            foreach(var evidence in report) {
                this.AddEvidence(evidence);
            }
        }
        #endregion

        #region Select
        /// <summary>
        /// Gets the evidences having the given category
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByCategory(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory evidenceCategory) {
            return this.Evidences.Values.Where(e => e.EvidenceCategory.Equals(evidenceCategory)).ToList();
        }

        /// <summary>
        /// Gets the evidences having the given provenance rule
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByProvenance(String evidenceProvenance = "") {
            return this.Evidences.Values.Where(e => e.EvidenceProvenance.ToUpper().Equals(evidenceProvenance.Trim().ToUpperInvariant(), StringComparison.Ordinal)).ToList();
        }

        /// <summary>
        /// Gets the evidences having the given content subject
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByContentSubject(RDFOntologyResource evidenceContentSubject) {
            return this.Evidences.Values.Where(e => e.EvidenceContent.TaxonomySubject.Equals(evidenceContentSubject)).ToList();
        }

        /// <summary>
        /// Gets the evidences having the given content predicate
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByContentPredicate(RDFOntologyResource evidenceContentPredicate) {
            return this.Evidences.Values.Where(e => e.EvidenceContent.TaxonomyPredicate.Equals(evidenceContentPredicate)).ToList();
        }

        /// <summary>
        /// Gets the evidences having the given content object
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByContentObject(RDFOntologyResource evidenceContentObject) {
            return this.Evidences.Values.Where(e => e.EvidenceContent.TaxonomyObject.Equals(evidenceContentObject)).ToList();
        }
        #endregion

        #endregion

    }

}