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

using System.Collections.Generic;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFXoneConstraint represents a SHACL constraint requiring exactly one of the given shapes for a given RDF term
    /// </summary>
    public sealed class RDFXoneConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Shapes required for the given RDF term
        /// </summary>
        internal Dictionary<long, RDFResource> XoneShapes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a xone constraint
        /// </summary>
        public RDFXoneConstraint()
            => XoneShapes = new Dictionary<long, RDFResource>();
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given shape to the required shapes of this constraint
        /// </summary>
        public RDFXoneConstraint AddShape(RDFResource shapeUri)
        {
            if (shapeUri != null && !XoneShapes.ContainsKey(shapeUri.PatternMemberID))
                XoneShapes.Add(shapeUri.PatternMemberID, shapeUri);
            return this;
        }

        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();
            RDFPropertyShape pShape = shape as RDFPropertyShape;

            //Search for given xone shapes
            List<RDFShape> xoneShapes = new List<RDFShape>(XoneShapes.Count);
            foreach (RDFResource xoneShapeUri in XoneShapes.Values)
            {
                RDFShape xoneShape = shapesGraph.SelectShape(xoneShapeUri.ToString());
                if (xoneShape != null)
                    xoneShapes.Add(xoneShape);
            }

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral("Value does not have exactly one of the shapes in sh:xone enumeration"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                int valueNodeConformsCounter = 0;
                foreach (RDFShape xoneShape in xoneShapes)
                {
                    RDFValidationReport xoneShapeReport = RDFValidationEngine.ValidateShape(shapesGraph, dataGraph, xoneShape, new List<RDFPatternMember>(1) { valueNode });
                    if (xoneShapeReport.Conforms)
                    {
                        valueNodeConformsCounter++;
                        //No need to evaluate remaining shapes, since value has broken sh:xone enumeration
                        if (valueNodeConformsCounter > 1)
                            break;
                    }
                }

                if (valueNodeConformsCounter != 1)
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.XONE_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             pShape?.Path,
                                                             valueNode,
                                                             shapeMessages,
                                                             shape.Severity));
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
                //Get collection from xoneShapes
                RDFCollection xoneShapes = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource) { InternalReificationSubject = this };
                foreach (RDFResource xoneShape in XoneShapes.Values)
                    xoneShapes.AddItem(xoneShape);
                result.AddCollection(xoneShapes);

                //sh:xone
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.XONE, xoneShapes.ReificationSubject));
            }
            return result;
        }
        #endregion
    }
}