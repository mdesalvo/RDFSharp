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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasonerEvidence represents an inference evidence generated during execution of a rule
    /// </summary>
    public class RDFOntologyReasonerEvidence
    {
        #region Properties
        /// <summary>
        /// Category of the evidence
        /// </summary>
        public RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory EvidenceCategory { get; internal set; }

        /// <summary>
        /// Provenance rule of the evidence
        /// </summary>
        public string EvidenceProvenance { get; internal set; }

        /// <summary>
        /// Destination taxonomy of the evidence
        /// </summary>
        public string EvidenceDestination { get; internal set; }

        /// <summary>
        /// Content of the evidence
        /// </summary>
        public RDFOntologyTaxonomyEntry EvidenceContent { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an evidence with given category, provenance, destination and content
        /// </summary>
        public RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory evidenceCategory,
                                           string evidenceProvenance,
                                           string evidenceDestination,
                                           RDFOntologyTaxonomyEntry evidenceContent)
        {
            this.EvidenceCategory = evidenceCategory;

            if (!string.IsNullOrEmpty(evidenceProvenance))
                this.EvidenceProvenance = evidenceProvenance;
            else
                throw new RDFSemanticsException("Cannot create reasoner evidence without evidenceProvenance!");

            if (!string.IsNullOrEmpty(evidenceDestination))
                this.EvidenceDestination = evidenceDestination;
            else
                throw new RDFSemanticsException("Cannot create reasoner evidence without evidenceDestination!");

            if (evidenceContent != null)
                this.EvidenceContent = evidenceContent;
            else
                throw new RDFSemanticsException("Cannot create reasoner evidence without evidenceContent!");
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a triple representation of this reasoner evidence
        /// </summary>
        /// <returns></returns>
        public RDFTriple ToRDFTriple() => EvidenceContent.ToRDFTriple();
        #endregion
    }

}
