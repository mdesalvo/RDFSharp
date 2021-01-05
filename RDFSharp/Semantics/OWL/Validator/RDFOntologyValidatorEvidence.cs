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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyValidatorEvidence represents an evidence reported by a validation rule.
    /// </summary>
    public class RDFOntologyValidatorEvidence
    {

        #region Properties
        /// <summary>
        /// Category of this evidence
        /// </summary>
        public RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory EvidenceCategory { get; internal set; }

        /// <summary>
        /// Rule which has reported this evidence
        /// </summary>
        public string EvidenceProvenance { get; internal set; }

        /// <summary>
        /// Message of the evidence
        /// </summary>
        public string EvidenceMessage { get; internal set; }

        /// <summary>
        /// Proposed action for solving or mitigating the evidence
        /// </summary>
        public string EvidenceSuggestion { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an evidence with given category, provenance, message and suggestion
        /// </summary>
        internal RDFOntologyValidatorEvidence(RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory evidenceCategory,
                                              string evidenceProvenance,
                                              string evidenceMessage,
                                              string evidenceSuggestion)
        {
            this.EvidenceCategory = evidenceCategory;
            this.EvidenceProvenance = evidenceProvenance;
            this.EvidenceMessage = evidenceMessage;
            this.EvidenceSuggestion = evidenceSuggestion;
        }
        #endregion

    }

}