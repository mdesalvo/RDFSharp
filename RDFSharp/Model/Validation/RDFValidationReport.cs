/*
   Copyright 2012-2019 Marco De Salvo

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

namespace RDFSharp.Model.Validation
{
    /// <summary>
    ///  RDFValidationReport represents a detailed report of a SHACL shapes graph's validation.
    /// </summary>
    public class RDFValidationReport: RDFResource, IEnumerable<RDFValidationResult> {

        #region Properties
        /// <summary>
        /// Indicates that the validation was successful (sh:conforms)
        /// </summary>
        public Boolean Conforms {
            get { return this.ResultsCount == 0; }
        }

        /// <summary>
        /// Counter of the validator results
        /// </summary>
        public Int32 ResultsCount {
            get { return this.Results.Count; }
        }

        /// <summary>
        /// Gets an enumerator on the validator results for iteration
        /// </summary>
        public IEnumerator<RDFValidationResult> ResultsEnumerator {
            get { return this.Results.GetEnumerator(); }
        }

        /// <summary>
        /// List of validator results (sh:result)
        /// </summary>
        internal List<RDFValidationResult> Results { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty validation report
        /// </summary>
        internal RDFValidationReport(RDFResource reportName): base(reportName.ToString()) {
            this.Results = new List<RDFValidationResult>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the SHACL validation report's results
        /// </summary>
        IEnumerator<RDFValidationResult> IEnumerable<RDFValidationResult>.GetEnumerator() {
            return this.ResultsEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the SHACL validation report's results
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.ResultsEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given result to this validation report
        /// </summary>
        internal void AddResult(RDFValidationResult result) {
            this.Results.Add(result);
        }

        /// <summary>
        /// Merges the results of the given validation report
        /// </summary>
        internal void MergeResults(RDFValidationReport report) {
           this.Results.AddRange(report.Results);
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new union validation report from this validation report and the given one
        /// </summary>
        internal RDFValidationReport UnionWith(RDFValidationReport validationReport) {
            var result = new RDFValidationReport(new RDFResource());

            //Add results from this validation report
            foreach (var r in this) {
                result.AddResult(r);
            }

            //Manage the given validation report
            if (validationReport != null) {

                //Add results from the given validation report
                foreach (var r in validationReport) {
                    result.AddResult(r);
                }

            }

            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this validation report
        /// </summary>
        public RDFGraph ToRDFGraph() {
            var result = new RDFGraph();

            //ValidationReport
            result.AddTriple(new RDFTriple(this, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_REPORT));

            //Conforms
            if (this.Conforms)
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.CONFORMS, new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)));
            else
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.CONFORMS, new RDFTypedLiteral("false", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)));

            //Results
            this.Results.ForEach(r => {
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.RESULT, r));
                result = result.UnionWith(r.ToRDFGraph());
            });

            result.SetContext(this.URI);
            return result;
        }
        #endregion

        #endregion

    }
}