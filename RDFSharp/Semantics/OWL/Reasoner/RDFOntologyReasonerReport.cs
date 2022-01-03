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

using RDFSharp.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasonerReport represents a detailed report of an ontology reasoner's activity.
    /// </summary>
    public class RDFOntologyReasonerReport : IEnumerable<RDFOntologyReasonerEvidence>
    {
        #region Properties
        /// <summary>
        /// Counter of the evidences
        /// </summary>
        public int EvidencesCount => this.Evidences.Count;

        /// <summary>
        /// Gets an enumerator on the evidences for iteration
        /// </summary>
        public IEnumerator<RDFOntologyReasonerEvidence> EvidencesEnumerator => this.Evidences.Values.GetEnumerator();

        /// <summary>
        /// Dictionary of evidences
        /// </summary>
        internal Dictionary<long, RDFOntologyReasonerEvidence> Evidences { get; set; }

        /// <summary>
        /// SyncLock for evidences
        /// </summary>
        internal object SyncLock { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty reasoner report
        /// </summary>
        public RDFOntologyReasonerReport()
        {
            this.Evidences = new Dictionary<long, RDFOntologyReasonerEvidence>();
            this.SyncLock = new object();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the reasoner report's evidences
        /// </summary>
        IEnumerator<RDFOntologyReasonerEvidence> IEnumerable<RDFOntologyReasonerEvidence>.GetEnumerator() => this.EvidencesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the reasoner report's evidences
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => this.EvidencesEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given evidence to the reasoner report
        /// </summary>
        public void AddEvidence(RDFOntologyReasonerEvidence evidence)
        {
            if (evidence != null)
            {
                lock (this.SyncLock)
                {
                    if (!this.Evidences.ContainsKey(evidence.EvidenceContent.TaxonomyEntryID))
                        this.Evidences.Add(evidence.EvidenceContent.TaxonomyEntryID, evidence);
                }
            }
        }

        /// <summary>
        /// Merges the evidences of the given report
        /// </summary>
        internal void Merge(RDFOntologyReasonerReport report)
        {
            foreach (RDFOntologyReasonerEvidence evidence in report)
                this.AddEvidence(evidence);
        }
        #endregion

        #region Select
        /// <summary>
        /// Gets the evidences having the given category
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByCategory(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory evidenceCategory) =>
            this.Evidences.Values.Where(e => e.EvidenceCategory.Equals(evidenceCategory)).ToList();

        /// <summary>
        /// Gets the evidences having the given provenance rule
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByProvenance(string evidenceProvenance) =>
            this.Evidences.Values.Where(e => e.EvidenceProvenance.Equals(evidenceProvenance?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Gets the evidences having the given destination taxonomy
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByDestination(string evidenceDestination) =>
            this.Evidences.Values.Where(e => e.EvidenceDestination.Equals(evidenceDestination?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)).ToList();
        
        /// <summary>
        /// Gets the evidences having the given content subject
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByContentSubject(RDFOntologyResource evidenceContentSubject) =>
            this.Evidences.Values.Where(e => e.EvidenceContent.TaxonomySubject.Equals(evidenceContentSubject)).ToList();
        
        /// <summary>
        /// Gets the evidences having the given content predicate
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByContentPredicate(RDFOntologyResource evidenceContentPredicate) =>
            this.Evidences.Values.Where(e => e.EvidenceContent.TaxonomyPredicate.Equals(evidenceContentPredicate)).ToList();
        
        /// <summary>
        /// Gets the evidences having the given content object
        /// </summary>
        public List<RDFOntologyReasonerEvidence> SelectEvidencesByContentObject(RDFOntologyResource evidenceContentObject) =>
            this.Evidences.Values.Where(e => e.EvidenceContent.TaxonomyObject.Equals(evidenceContentObject)).ToList();
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this reasoner report
        /// </summary>
        public RDFGraph ToRDFGraph()
        {
            RDFGraph result = new RDFGraph();
            foreach (RDFOntologyReasonerEvidence evidence in this)
                result.AddTriple(evidence.ToRDFTriple());
            return result;
        }

        /// <summary>
        /// Asynchronously gets a graph representation of this reasoner report
        /// </summary>
        public Task<RDFGraph> ToRDFGraphAsync()
            => Task.Run(() => ToRDFGraph());

        /// <summary>
        /// Joins the reasoner evidences of this report into the given ontology
        /// </summary>
        public void JoinEvidences(RDFOntology ontology)
        {
            foreach (RDFOntologyReasonerEvidence evidence in this)
            {
                switch (evidence.EvidenceCategory)
                {
                    case RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.ClassModel:
                        switch (evidence.EvidenceDestination)
                        {
                            case nameof(RDFOntologyClassModel.Relations.SubClassOf):
                                ontology.Model.ClassModel.Relations.SubClassOf.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyClassModel.Relations.DisjointWith):
                                ontology.Model.ClassModel.Relations.DisjointWith.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyClassModel.Relations.EquivalentClass):
                                ontology.Model.ClassModel.Relations.EquivalentClass.AddEntry(evidence.EvidenceContent);
                                break;

                            //OWL2
                            case nameof(RDFOntologyClassModel.Relations.HasKey):
                                ontology.Model.ClassModel.Relations.HasKey.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyClassModel.Relations.OneOf):
                                ontology.Model.ClassModel.Relations.OneOf.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyClassModel.Relations.IntersectionOf):
                                ontology.Model.ClassModel.Relations.IntersectionOf.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyClassModel.Relations.UnionOf):
                                ontology.Model.ClassModel.Relations.UnionOf.AddEntry(evidence.EvidenceContent);
                                break;
                        }
                        break;

                    case RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.PropertyModel:
                        switch (evidence.EvidenceDestination)
                        {
                            case nameof(RDFOntologyPropertyModel.Relations.SubPropertyOf):
                                ontology.Model.PropertyModel.Relations.SubPropertyOf.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyPropertyModel.Relations.EquivalentProperty):
                                ontology.Model.PropertyModel.Relations.EquivalentProperty.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyPropertyModel.Relations.InverseOf):
                                ontology.Model.PropertyModel.Relations.InverseOf.AddEntry(evidence.EvidenceContent);
                                break;

                            //OWL2
                            case nameof(RDFOntologyPropertyModel.Relations.PropertyDisjointWith):
                                ontology.Model.PropertyModel.Relations.PropertyDisjointWith.AddEntry(evidence.EvidenceContent);
                                break;

                            //OWL2
                            case nameof(RDFOntologyPropertyModel.Relations.PropertyChainAxiom):
                                ontology.Model.PropertyModel.Relations.PropertyChainAxiom.AddEntry(evidence.EvidenceContent);
                                break;
                        }
                        break;

                    case RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data:
                        switch (evidence.EvidenceDestination)
                        {
                            case nameof(RDFOntologyData.Relations.ClassType):
                                ontology.Data.Relations.ClassType.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyData.Relations.SameAs):
                                ontology.Data.Relations.SameAs.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyData.Relations.DifferentFrom):
                                ontology.Data.Relations.DifferentFrom.AddEntry(evidence.EvidenceContent);
                                break;

                            case nameof(RDFOntologyData.Relations.Assertions):
                                ontology.Data.Relations.Assertions.AddEntry(evidence.EvidenceContent);
                                break;

                            //OWL2
                            case nameof(RDFOntologyData.Relations.NegativeAssertions):
                                ontology.Data.Relations.NegativeAssertions.AddEntry(evidence.EvidenceContent);
                                break;

                            //SKOS
                            case nameof(RDFOntologyData.Relations.Member):
                                ontology.Data.Relations.Member.AddEntry(evidence.EvidenceContent);
                                break;

                            //SKOS
                            case nameof(RDFOntologyData.Relations.MemberList):
                                ontology.Data.Relations.MemberList.AddEntry(evidence.EvidenceContent);
                                break;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Asynchronously joins the reasoner evidences of this report into the given ontology
        /// </summary>
        public Task JoinEvidencesAsync(RDFOntology ontology)
            => Task.Run(() => JoinEvidences(ontology));
        #endregion

        #endregion
    }

}