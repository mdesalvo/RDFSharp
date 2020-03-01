/*
   Copyright 2012-2020 Marco De Salvo

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
using System.Collections.Generic;
using System.Text;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFUniqueLangConstraint represents a SHACL constraint on the uniqueness of language tags for a given RDF term
    /// </summary>
    public class RDFUniqueLangConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Flag indicating that uniqueness of language tags is required or not
        /// </summary>
        public Boolean UniqueLang { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a uniqueLang constraint with the given behavior
        /// </summary>
        public RDFUniqueLangConstraint(Boolean uniqueLang) : base() {
            this.UniqueLang = uniqueLang;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            
            //TODO


            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //sh:uniqueLang
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.UNIQUE_LANG, new RDFTypedLiteral(this.UniqueLang.ToString(), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)));

            }
            return result;
        }
        #endregion

    }
}