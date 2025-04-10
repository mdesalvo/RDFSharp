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
using System.Linq;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFClassConstraint represents a SHACL constraint on the specified class for a given RDF term
    /// </summary>
    public sealed class RDFClassConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// ClassType of the given RDF term
        /// </summary>
        public RDFResource ClassType { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a class constraint with the given class
        /// </summary>
        public RDFClassConstraint(RDFResource classType)
            => ClassType = classType ?? throw new RDFModelException("Cannot create RDFClassConstraint because given \"classType\" parameter is null.");
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();
            List<RDFPatternMember> classInstances = dataGraph.GetInstancesOfClass(ClassType);
            RDFPropertyShape pShape = shape as RDFPropertyShape;

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Value does not have class <{ClassType}>"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
                switch (valueNode)
                {
                    //Resource
                    case RDFResource valueNodeResource:
                        if (!classInstances.Any(x => x.Equals(valueNodeResource)))
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.CLASS_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     pShape?.Path,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        break;

                    //Literal
                    case RDFLiteral _:
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.CLASS_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 pShape?.Path,
                                                                 valueNode,
                                                                 shapeMessages,
                                                                 shape.Severity));
                        break;
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
                //sh:class
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.CLASS, ClassType));
            }
            return result;
        }
        #endregion
    }
}