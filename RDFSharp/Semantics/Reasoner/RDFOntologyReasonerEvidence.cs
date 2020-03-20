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

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFOntologyReasonerEvidence represents an inference evidence generated during execution of a rule
    /// </summary>
    public class RDFOntologyReasonerEvidence {

        #region Properties
        /// <summary>
        /// Category of the evidence
        /// </summary>
        public RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory EvidenceCategory { get; internal set; }

        /// <summary>
        /// Provenance rule of the evidence
        /// </summary>
        public String EvidenceProvenance { get; internal set; }

        /// <summary>
        /// Content of the evidence
        /// </summary>
        public RDFOntologyTaxonomyEntry EvidenceContent { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an evidence with given category, provenance and message
        /// </summary>
        internal RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory evidenceCategory,
                                             String evidenceProvenance,
                                             RDFOntologyTaxonomyEntry evidenceContent) {
            this.EvidenceCategory   = evidenceCategory;
            this.EvidenceProvenance = evidenceProvenance;
            this.EvidenceContent    = evidenceContent;
        }
        #endregion

    }

}
