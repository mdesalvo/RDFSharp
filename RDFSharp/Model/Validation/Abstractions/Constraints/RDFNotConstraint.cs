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
    /// RDFNotConstraint represents a SHACL constraint not allowing the given shape for a given RDF term
    /// </summary>
    public class RDFNotConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Shape not allowed for the given RDF term
        /// </summary>
        public RDFResource NotShape { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a not constraint with the given shape
        /// </summary>
        public RDFNotConstraint(RDFResource notShape)
        {
            if (notShape == null)
                throw new RDFModelException("Cannot create RDFNotConstraint because given \"notShape\" parameter is null.");
            
            this.NotShape = notShape;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            //Search for given not shape
            RDFShape notShape = shapesGraph.SelectShape(this.NotShape.ToString());
            if (notShape == null)
                return report;

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Value does have shape <{this.NotShape}>"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                RDFValidationReport notShapeReport = RDFValidationEngine.ValidateShape(shapesGraph, dataGraph, notShape, new List<RDFPatternMember>() { valueNode });
                if (notShapeReport.Conforms)
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.NOT_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
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
                //sh:not
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NOT, this.NotShape));
            }
            return result;
        }
        #endregion
    }
}
