﻿/*
   Copyright 2012-2025 Marco De Salvo

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

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDFSharp.Model
{
    /// <summary>
    ///  RDFValidationReport represents a detailed report of a shapes graph's validation.
    /// </summary>
    public sealed class RDFValidationReport : RDFResource, IEnumerable<RDFValidationResult>
    {
        #region Properties
        /// <summary>
        /// Indicates that the validation was successful (sh:conforms)
        /// </summary>
        public bool Conforms => ResultsCount == 0;

        /// <summary>
        /// Counter of the validator results
        /// </summary>
        public int ResultsCount => Results.Count;

        /// <summary>
        /// Gets an enumerator on the validator results for iteration
        /// </summary>
        public IEnumerator<RDFValidationResult> ResultsEnumerator => Results.GetEnumerator();

        /// <summary>
        /// List of validator results (sh:result)
        /// </summary>
        internal List<RDFValidationResult> Results { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a named validation report
        /// </summary>
        internal RDFValidationReport(RDFResource reportName) : base(reportName.ToString())
            => Results = [];

        /// <summary>
        /// Builds a blank validation report
        /// </summary>
        internal RDFValidationReport() : this(new RDFResource()) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the validation report's results
        /// </summary>
        IEnumerator<RDFValidationResult> IEnumerable<RDFValidationResult>.GetEnumerator() => ResultsEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the validation report's results
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => ResultsEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given result to this validation report
        /// </summary>
        internal void AddResult(RDFValidationResult result)
        {
            if (result != null)
                Results.Add(result);
        }

        /// <summary>
        /// Merges the results of the given validation report to this validation report
        /// </summary>
        internal void MergeResults(RDFValidationReport report)
        {
            if (report?.Results != null)
                Results.AddRange(report.Results);
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this validation report
        /// </summary>
        public RDFGraph ToRDFGraph()
        {
            RDFGraph result = new RDFGraph();

            //ValidationReport
            result.AddTriple(new RDFTriple(this, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_REPORT));

            //Conforms
            result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.CONFORMS, Conforms ? RDFTypedLiteral.True : RDFTypedLiteral.False));

            //Results
            Results.ForEach(res =>
            {
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.RESULT, res));
                result = result.UnionWith(res.ToRDFGraph());
            });

            result.SetContext(URI);
            return result;
        }

        /// <summary>
        /// Asynchronously gets a graph representation of this validation report
        /// </summary>
        public Task<RDFGraph> ToRDFGraphAsync()
            => Task.Run(ToRDFGraph);
        #endregion

        #endregion
    }
}