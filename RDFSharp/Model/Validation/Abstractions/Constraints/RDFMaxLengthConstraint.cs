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
    /// RDFMaxLengthConstraint represents a SHACL constraint on the maximum allowed length for a given RDF term
    /// </summary>
    public sealed class RDFMaxLengthConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Indicates the maximum allowed length for a given RDF term
        /// </summary>
        public int MaxLength { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a named maxLength constraint with the given maxLength
        /// </summary>
        public RDFMaxLengthConstraint(int maxLength)
            => MaxLength = maxLength < 0 ? 0 : maxLength;
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();
            RDFPropertyShape pShape = shape as RDFPropertyShape;

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = [.. shape.Messages];
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Must have a maximum length of {MaxLength} characters and can't be a blank node"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
                switch (valueNode)
                {
                    //Resource
                    case RDFResource valueNodeResource:
                        if (valueNodeResource.IsBlank || valueNodeResource.ToString().Length > MaxLength)
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.MAX_LENGTH_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     pShape?.Path,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        break;

                    //Literal
                    case RDFLiteral valueNodeLiteral:
                        if (valueNodeLiteral.Value.Length > MaxLength)
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.MAX_LENGTH_CONSTRAINT_COMPONENT,
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
                //sh:maxLength
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_LENGTH, new RDFTypedLiteral(MaxLength.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            return result;
        }
        #endregion
    }
}