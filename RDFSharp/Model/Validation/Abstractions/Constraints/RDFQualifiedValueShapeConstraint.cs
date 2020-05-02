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
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFQualifiedValueShapeConstraint represents a SHACL constraint requiring min/max occurrencies of the specified shape for a given RDF term
    /// </summary>
    public class RDFQualifiedValueShapeConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Identifier of the shape against which the given RDF term must be validated
        /// </summary>
        public RDFResource QualifiedValueShapeUri { get; internal set; }

        /// <summary>
        /// Indicates the minimum required occurrences for a given RDF term
        /// </summary>
        public int? QualifiedValueMinCount { get; internal set; }

        /// <summary>
        /// Indicates the maximum required occurrences for a given RDF term
        /// </summary>
        public int? QualifiedValueMaxCount { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a qualified value shape constraint with the given property shape identifier and min/max counters
        /// </summary>
        public RDFQualifiedValueShapeConstraint(RDFResource qualifiedValueShapeUri, int? qualifiedValueMinCount, int? qualifiedValueMaxCount) : base() {
            if (qualifiedValueShapeUri != null) {
                this.QualifiedValueShapeUri = qualifiedValueShapeUri;

                if (qualifiedValueMinCount.HasValue)
                    this.QualifiedValueMinCount = qualifiedValueMinCount < 0 ? 0 : qualifiedValueMinCount;

                if (qualifiedValueMaxCount.HasValue)
                    this.QualifiedValueMaxCount = qualifiedValueMaxCount < 0 ? 0 : qualifiedValueMaxCount;
            }
            else {
                throw new RDFModelException("Cannot create RDFQualifiedValueShapeConstraint because given \"qualifiedValueShapeUri\" parameter is null.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            #region Evaluation
            if (this.QualifiedValueMinCount.HasValue || this.QualifiedValueMaxCount.HasValue) {

                //Search qualified value shape
                RDFShape qualifiedValueShape = validationContext.ShapesGraph.SelectShape(this.QualifiedValueShapeUri.ToString());
                if (qualifiedValueShape == null)
                    return report;

                //Evaluate qualified value shape
                RDFValidationContext qualifiedValueShapeValidationContext =
                    new RDFValidationContext(validationContext.ShapesGraph,
                                             validationContext.DataGraph,
                                             validationContext.Shape,
                                             validationContext.FocusNodes,
                                             validationContext.ValueNodes);
                RDFValidationReport qualifiedValueShapeReport =
                    qualifiedValueShape is RDFNodeShape ? qualifiedValueShape.EvaluateShapeWithFocusAndValuesPreservation(qualifiedValueShapeValidationContext)
                                                        : qualifiedValueShape.EvaluateShapeWithFocusPreservation(qualifiedValueShapeValidationContext);
                
                //Evaluate focus nodes
                validationContext.FocusNodes.ForEach(focusNode => {
                    int conformingValues = 0;

                    //Get value nodes of current focus node
                    validationContext.ValueNodes[focusNode.PatternMemberID].ForEach(valueNode => {

                        //Evaluate current value node
                        if (qualifiedValueShapeReport.ValueNodeConforms(focusNode, valueNode))
                            conformingValues++;

                    });

                    //Report evidences (sh:qualifiedMinCount)
                    if (this.QualifiedValueMinCount.HasValue && conformingValues < this.QualifiedValueMinCount)
                        report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                 RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                 null,
                                                                 validationContext.Shape.Messages,
                                                                 validationContext.Shape.Severity));

                    //Report evidences (sh:qualifiedMaxCount)
                    if (this.QualifiedValueMaxCount.HasValue && conformingValues > this.QualifiedValueMaxCount)
                        report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                 RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                 null,
                                                                 validationContext.Shape.Messages,
                                                                 validationContext.Shape.Severity));
                });

            }
            #endregion

            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //sh:qualifiedValueShape
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE, this.QualifiedValueShapeUri));

                //sh:qualifiedMinCount
                if (this.QualifiedValueMinCount.HasValue)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT, new RDFTypedLiteral(this.QualifiedValueMinCount.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

                //sh:qualifiedMaxCount
                if (this.QualifiedValueMaxCount.HasValue)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT, new RDFTypedLiteral(this.QualifiedValueMaxCount.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            }
            return result;
        }
        #endregion

    }
}