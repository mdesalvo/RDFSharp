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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFMinLengthConstraint represents a SHACL constraint on the minimum allowed length for a given RDF term
    /// </summary>
    public class RDFMinLengthConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Indicates the minimum allowed length for a given RDF term
        /// </summary>
        public int MinLength { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a minLength constraint with the given minLength
        /// </summary>
        public RDFMinLengthConstraint(int minLength) : base() {
            this.MinLength = minLength < 0 ? 0 : minLength;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            #region Evaluation
            //Evaluate focus nodes
            validationContext.FocusNodes.ForEach(focusNode => {

                //Get value nodes of current focus node
                validationContext.ValueNodes[focusNode.PatternMemberID].ForEach(valueNode => {

                    //Evaluate current value node
                    switch (valueNode) {

                        //Resource
                        case RDFResource valueNodeResource:
                            if (valueNodeResource.IsBlank || (this.MinLength > 0 && valueNodeResource.ToString().Length < this.MinLength)) {
                                report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                         RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT,
                                                                         focusNode,
                                                                         validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                         valueNode,
                                                                         validationContext.Shape.Messages,
                                                                         validationContext.Shape.Severity));
                            }
                            break;

                        //Literal
                        case RDFLiteral valueNodeLiteral:
                            if (this.MinLength > 0 && valueNodeLiteral.Value.Length < this.MinLength) {
                                report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                         RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT,
                                                                         focusNode,
                                                                         validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                         valueNode,
                                                                         validationContext.Shape.Messages,
                                                                         validationContext.Shape.Severity));
                            }
                            break;

                    }

                });

            });
            #endregion

            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //sh:minLength
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MIN_LENGTH, new RDFTypedLiteral(this.MinLength.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            }
            return result;
        }
        #endregion

    }
}