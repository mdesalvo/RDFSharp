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
using System.Linq;

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

            //Only property shapes are allowed to use uniqueLang constraint
            if (this.UniqueLang && validationContext.Shape is RDFPropertyShape) {
                switch (validationContext.ValueNode) {

                    //Resource/TypedLiteral
                    case RDFResource valueNodeResource:
                    case RDFTypedLiteral valueNodeTypedLiteral:
                        break;

                    //PlainLiteral
                    case RDFPlainLiteral plainLiteralValueNode:
                        if (!String.IsNullOrEmpty(plainLiteralValueNode.Language)) {
                            List<RDFLiteral> plainLiteralValueNodes = validationContext.ValueNodes.Where(vn => vn is RDFPlainLiteral && !String.IsNullOrEmpty(((RDFPlainLiteral)vn).Language))
                                                                                                  .OfType<RDFLiteral>()
                                                                                                  .ToList();
                            if (RDFValidationHelper.CheckLanguageTagInUse(plainLiteralValueNodes, plainLiteralValueNode.Language))
                                report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                         RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT,
                                                                         validationContext.FocusNode,
                                                                         ((RDFPropertyShape)validationContext.Shape).Path,
                                                                         validationContext.ValueNode,
                                                                         validationContext.Shape.Messages,
                                                                         new RDFResource(),
                                                                         validationContext.Shape.Severity));
                        }
                        break;

                }
            }
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