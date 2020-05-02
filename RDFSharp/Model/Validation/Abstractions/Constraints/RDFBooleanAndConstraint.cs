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
    /// RDFBooleanAndConstraint represents a SHACL constraint on the required shapes for a given RDF term
    /// </summary>
    public class RDFBooleanAndConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Shapes required for the given RDF term
        /// </summary>
        internal Dictionary<Int64, RDFResource> AndShapes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a boolean and constraint
        /// </summary>
        public RDFBooleanAndConstraint() : base() {
            this.AndShapes = new Dictionary<Int64, RDFResource>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given shape to the required shapes of this constraint
        /// </summary>
        public RDFBooleanAndConstraint AddShape(RDFResource shapeUri) {
            if (shapeUri != null && !this.AndShapes.ContainsKey(shapeUri.PatternMemberID)) {
                this.AndShapes.Add(shapeUri.PatternMemberID, shapeUri);
            }
            return this;
        }

        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            #region Evaluation
            //Search and shapes
            List<RDFShape> andShapes = new List<RDFShape>();
            foreach (RDFResource andShapeUri in this.AndShapes.Values) {
                RDFShape andShape = validationContext.ShapesGraph.SelectShape(andShapeUri.ToString());
                if (andShape != null)
                    andShapes.Add(andShape);
            }

            //Evaluate and shapes
            List<RDFValidationReport> andShapesReport = new List<RDFValidationReport>();
            foreach (RDFShape andShape in andShapes) {

                //Evaluate shape and generate validation report
                RDFValidationContext andShapeValidationContext =
                new RDFValidationContext(validationContext.ShapesGraph,
                                         validationContext.DataGraph,
                                         validationContext.Shape,
                                         validationContext.FocusNodes,
                                         validationContext.ValueNodes);
                RDFValidationReport andShapeReport =
                    andShape is RDFNodeShape ? andShape.EvaluateShapeWithFocusAndValuesPreservation(andShapeValidationContext)
                                             : andShape.EvaluateShapeWithFocusPreservation(andShapeValidationContext);

                //Collect generated validation report
                andShapesReport.Add(andShapeReport);

            }

            //Evaluate focus nodes
            validationContext.FocusNodes.ForEach(focusNode => {

                //Get value nodes of current focus node
                validationContext.ValueNodes[focusNode.PatternMemberID].ForEach(valueNode => {

                    //Evaluate current value node
                    if (!andShapesReport.TrueForAll(andShapeReport => andShapeReport.Conforms(focusNode, valueNode)))
                        report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                 RDFVocabulary.SHACL.AND_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                 valueNode,
                                                                 validationContext.Shape.Messages,
                                                                 validationContext.Shape.Severity));

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

                //Get collection from andShapes
                RDFCollection andShapes = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource) { InternalReificationSubject = this };
                foreach (RDFResource andShape in this.AndShapes.Values)
                    andShapes.AddItem(andShape);
                result.AddCollection(andShapes);

                //sh:and
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.AND, andShapes.ReificationSubject));

            }
            return result;
        }
        #endregion

    }
}