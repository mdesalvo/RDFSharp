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
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFAndConstraint represents a SHACL constraint requiring all the given shapes for a given RDF term
    /// </summary>
    public class RDFAndConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Shapes required for the given RDF term
        /// </summary>
        internal Dictionary<long, RDFResource> AndShapes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an and constraint
        /// </summary>
        public RDFAndConstraint() : base()
        {
            this.AndShapes = new Dictionary<long, RDFResource>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given shape to the required shapes of this constraint
        /// </summary>
        public RDFAndConstraint AddShape(RDFResource shapeUri)
        {
            if (shapeUri != null && !this.AndShapes.ContainsKey(shapeUri.PatternMemberID))
            {
                this.AndShapes.Add(shapeUri.PatternMemberID, shapeUri);
            }
            return this;
        }

        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            //Search for given and shapes
            List<RDFShape> andShapes = new List<RDFShape>();
            foreach (RDFResource andShapeUri in this.AndShapes.Values)
            {
                RDFShape andShape = shapesGraph.SelectShape(andShapeUri.ToString());
                if (andShape != null)
                    andShapes.Add(andShape);
            }

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                bool valueNodeConforms = true;
                foreach (RDFShape andShape in andShapes)
                {
                    RDFValidationReport andShapeReport = RDFValidationEngine.ValidateShape(shapesGraph, dataGraph, andShape, new List<RDFPatternMember>() { valueNode });
                    if (!andShapeReport.Conforms)
                    {
                        valueNodeConforms = false;
                        break;
                    }
                }

                if (!valueNodeConforms)
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.AND_CONSTRAINT_COMPONENT,
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