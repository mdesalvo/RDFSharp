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
    /// RDFMinCountConstraint represents a SHACL constraint on the minimum required occurrences for a given RDF term
    /// </summary>
    public sealed class RDFMinCountConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Indicates the minimum required occurrences for a given RDF term
        /// </summary>
        public int MinCount { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a minCount constraint with the given minCount
        /// </summary>
        public RDFMinCountConstraint(int minCount)
            => MinCount = minCount < 0 ? 0 : minCount;
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
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Must have a minimum of {MinCount} occurrences"));

            #region Evaluation
            if (valueNodes.Count < MinCount)
                report.AddResult(new RDFValidationResult(shape,
                                                         RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT,
                                                         focusNode,
                                                         pShape?.Path,
                                                         null,
                                                         shapeMessages,
                                                         shape.Severity));
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
                //sh:minCount
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MIN_COUNT, new RDFTypedLiteral(MinCount.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            return result;
        }
        #endregion
    }
}
