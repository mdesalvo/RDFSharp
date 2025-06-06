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
    /// RDFOrConstraint represents a SHACL constraint requiring at least one of the given shapes for a given RDF term
    /// </summary>
    public sealed class RDFOrConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Shapes required for the given RDF term
        /// </summary>
        internal Dictionary<long, RDFResource> OrShapes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an or constraint
        /// </summary>
        public RDFOrConstraint()
            => OrShapes = new Dictionary<long, RDFResource>();
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given shape to the required shapes of this constraint
        /// </summary>
        public RDFOrConstraint AddShape(RDFResource shapeUri)
        {
            if (shapeUri != null && !OrShapes.ContainsKey(shapeUri.PatternMemberID))
                OrShapes.Add(shapeUri.PatternMemberID, shapeUri);
            return this;
        }

        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();
            RDFPropertyShape pShape = shape as RDFPropertyShape;

            //Search for given or shapes
            List<RDFShape> orShapes = new List<RDFShape>();
            foreach (RDFResource orShapeUri in OrShapes.Values)
            {
                RDFShape orShape = shapesGraph.SelectShape(orShapeUri.ToString());
                if (orShape != null)
                    orShapes.Add(orShape);
            }

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral("Value does not have at least one of the shapes in sh:or enumeration"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                bool valueNodeConforms = false;
                foreach (RDFShape orShape in orShapes)
                {
                    RDFValidationReport orShapeReport = RDFValidationEngine.ValidateShape(shapesGraph, dataGraph, orShape, new List<RDFPatternMember> { valueNode });
                    if (orShapeReport.Conforms)
                    {
                        valueNodeConforms = true;
                        //No need to evaluate remaining shapes, since value has succeeded sh:or enumeration
                        break;
                    }
                }

                if (!valueNodeConforms)
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.OR_CONSTRAINT_COMPONENT,
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
                //Get collection from OrShapes
                RDFCollection orShapes = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource) { InternalReificationSubject = this };
                foreach (RDFResource orShape in OrShapes.Values)
                    orShapes.AddItem(orShape);
                result.AddCollection(orShapes);

                //sh:or
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.OR, orShapes.ReificationSubject));
            }
            return result;
        }
        #endregion
    }
}
