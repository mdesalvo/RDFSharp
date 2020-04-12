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

using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFClassConstraint represents a SHACL constraint requiring the specified node shape for a given RDF term
    /// </summary>
    public class RDFNodeConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Identifier of the node shape against which the given RDF term must be validated
        /// </summary>
        public RDFResource NodeShapeUri { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a node constraint with the given node shape identifier
        /// </summary>
        public RDFNodeConstraint(RDFResource nodeShapeUri) : base() {
            if (nodeShapeUri != null) {
                this.NodeShapeUri = nodeShapeUri;
            }
            else {
                throw new RDFModelException("Cannot create RDFNodeConstraint because given \"nodeShapeUri\" parameter is null.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext)
        {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            #region Evaluation
            //Search node shape
            RDFNodeShape nodeShape = validationContext.ShapesGraph.SelectShape(this.NodeShapeUri.ToString()) as RDFNodeShape;
            if (nodeShape == null)
                return report;

            //Evaluate node shape
            RDFValidationReport nodeshapeReport = nodeShape.EvaluateShape(validationContext, 
                new RDFValidationOptions() { PreserveExistingContextData = true });

            //Report node shape evidences
            foreach (RDFValidationResult nodeshapeResult in nodeshapeReport)
                report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                         RDFVocabulary.SHACL.NODE_CONSTRAINT_COMPONENT,
                                                         nodeshapeResult.FocusNode,
                                                         validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                         nodeshapeResult.ResultValue,
                                                         validationContext.Shape.Messages,
                                                         validationContext.Shape.Severity));
            #endregion

            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //sh:node
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NODE, this.NodeShapeUri));

            }
            return result;
        }
        #endregion

    }
}