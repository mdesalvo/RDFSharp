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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFXoneConstraint represents a SHACL constraint requiring exactly one of the given shapes for a given RDF term
    /// </summary>
    public class RDFXoneConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Shapes required for the given RDF term
        /// </summary>
        internal Dictionary<long, RDFResource> XoneShapes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a xone constraint
        /// </summary>
        public RDFXoneConstraint() : base()
        {
            this.XoneShapes = new Dictionary<long, RDFResource>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given shape to the required shapes of this constraint
        /// </summary>
        public RDFXoneConstraint AddShape(RDFResource shapeUri)
        {
            if (shapeUri != null && !this.XoneShapes.ContainsKey(shapeUri.PatternMemberID))
            {
                this.XoneShapes.Add(shapeUri.PatternMemberID, shapeUri);
            }
            return this;
        }

        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            //Search for given xone shapes
            List<RDFShape> xoneShapes = new List<RDFShape>();
            foreach (RDFResource xoneShapeUri in this.XoneShapes.Values)
            {
                RDFShape xoneShape = shapesGraph.SelectShape(xoneShapeUri.ToString());
                if (xoneShape != null)
                    xoneShapes.Add(xoneShape);
            }

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                int valueNodeConformsCounter = 0;
                foreach (RDFShape xoneShape in xoneShapes)
                {
                    RDFValidationReport xoneShapeReport = RDFValidationEngine.ValidateShape(shapesGraph, dataGraph, xoneShape, new List<RDFPatternMember>() { valueNode });
                    if (xoneShapeReport.Conforms)
                        valueNodeConformsCounter++;
                }

                if (valueNodeConformsCounter != 1)
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.XONE_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                             valueNode,
                                                             shape.Messages,
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
                foreach (RDFResource xoneShape in this.XoneShapes.Values)
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