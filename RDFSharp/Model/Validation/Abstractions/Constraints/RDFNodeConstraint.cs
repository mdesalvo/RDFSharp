/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFNodeConstraint represents a SHACL constraint requiring the specified node shape for a given RDF term
    /// </summary>
    public class RDFNodeConstraint : RDFConstraint
    {
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
        public RDFNodeConstraint(RDFResource nodeShapeUri)
        {
            if (nodeShapeUri == null)
                throw new RDFModelException("Cannot create RDFNodeConstraint because given \"nodeShapeUri\" parameter is null.");
            
            this.NodeShapeUri = nodeShapeUri;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            //Search for given node shape
            RDFNodeShape nodeShape = shapesGraph.SelectShape(this.NodeShapeUri.ToString()) as RDFNodeShape;
            if (nodeShape == null)
                return report;

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Value does not have shape <{this.NodeShapeUri}>"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                RDFValidationReport nodeShapeReport = RDFValidationEngine.ValidateShape(shapesGraph, dataGraph, nodeShape, new List<RDFPatternMember>() { valueNode });
                if (!nodeShapeReport.Conforms)
                {
                    //Report evidences from linked node shape
                    report.MergeResults(nodeShapeReport);

                    //Report evidence from working shape
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.NODE_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                             valueNode,
                                                             shapeMessages,
                                                             shape.Severity));
                }
            }
            #endregion

            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape)
        {
            RDFGraph result = new RDFGraph();
            if (shape != null)
            {
                //sh:node
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NODE, this.NodeShapeUri));
            }
            return result;
        }
        #endregion
    }
}