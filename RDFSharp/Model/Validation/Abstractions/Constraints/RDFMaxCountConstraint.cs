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
    /// RDFMaxCountConstraint represents a SHACL constraint on the maximum required occurrences for a given RDF term
    /// </summary>
    public class RDFMaxCountConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Indicates the maximum required occurrences for a given RDF term
        /// </summary>
        public int MaxCount { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a maxCount constraint with the given maxCount
        /// </summary>
        public RDFMaxCountConstraint(int maxCount) : base() {
            this.MaxCount = maxCount < 0 ? 0 : maxCount;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            #region Evaluation

            //Evaluate current shape
            switch (validationContext.Shape) {

                //NodeShape (not allowed)
                case RDFNodeShape nodeShape:
                    break;

                //PropertyShape
                case RDFPropertyShape propertyShape:

                    //Evaluate focus nodes
                    validationContext.FocusNodes.ForEach(focusNode => {

                        //Get value nodes of current focus node
                        if (validationContext.ValueNodes[focusNode.PatternMemberID].Count > this.MaxCount) {
                            report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                     RDFVocabulary.SHACL.MAX_COUNT_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     ((RDFPropertyShape)validationContext.Shape).Path,
                                                                     null,
                                                                     validationContext.Shape.Messages,
                                                                     validationContext.Shape.Severity));
                        }

                    });
                    break;

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

                //sh:maxCount
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_COUNT, new RDFTypedLiteral(this.MaxCount.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            }
            return result;
        }
        #endregion

    }
}