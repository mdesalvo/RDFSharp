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
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyValidationReport represents a detailed report of an ontology analysis.
    /// </summary>
    public class RDFOntologyValidationReport: IEnumerable<RDFOntologyValidationEvidence> {

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
        public IEnumerator<RDFOntologyValidationEvidence> EvidencesEnumerator {
            get { return this.Evidences.GetEnumerator(); }
        }

        /// <summary>
        /// List of evidences
        /// </summary>
        internal List<RDFOntologyValidationEvidence> Evidences { get; set; }

        /// <summary>
        /// Identifier of the validaton report
        /// </summary>
        internal Int64 ValidationReportID { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty report
        /// </summary>
        internal RDFOntologyValidationReport(Int64 reportID) {
            this.ValidationReportID = reportID;
            this.Evidences          = new List<RDFOntologyValidationEvidence>();            
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the validation report's evidences
        /// </summary>
        IEnumerator<RDFOntologyValidationEvidence> IEnumerable<RDFOntologyValidationEvidence>.GetEnumerator() {
            return this.Evidences.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the validation report's evidences
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Evidences.GetEnumerator();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the warning evidences from the validation report
        /// </summary>
        public List<RDFOntologyValidationEvidence> SelectWarnings() {
            return this.Evidences.FindAll(e => e.EvidenceCategory == RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning);
        }

        /// <summary>
        /// Gets the error evidences from the validation report
        /// </summary>
        public List<RDFOntologyValidationEvidence> SelectErrors() {
            return this.Evidences.FindAll(e => e.EvidenceCategory == RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error);
        }
        #endregion

    }

}