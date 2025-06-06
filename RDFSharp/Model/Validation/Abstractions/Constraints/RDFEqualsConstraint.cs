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
    /// RDFEqualsConstraint represents a SHACL constraint on presence of a given RDF term for the specified predicate
    /// </summary>
    public sealed class RDFEqualsConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Predicate for which value nodes of a given RDF term are checked for presence
        /// </summary>
        public RDFResource EqualsPredicate { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an equals constraint with the given predicate
        /// </summary>
        public RDFEqualsConstraint(RDFResource equalsPredicate)
            => EqualsPredicate = equalsPredicate ?? throw new RDFModelException("Cannot create RDFEqualsConstraint because given \"equalsPredicate\" parameter is null.");
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
                shapeMessages.Add(new RDFPlainLiteral($"Must have same values as property <{EqualsPredicate}>"));

            #region Evaluation
            List<RDFPatternMember> predicateNodes = dataGraph.Where(t => t.Subject.Equals(focusNode)
                                                                            && t.Predicate.Equals(EqualsPredicate))
                                                             .Select(x => x.Object)
                                                             .ToList();

            foreach (RDFPatternMember predicateNode in predicateNodes)
                if (!valueNodes.Any(v => v.Equals(predicateNode)))
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.EQUALS_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             pShape?.Path,
                                                             predicateNode,
                                                             shapeMessages,
                                                             shape.Severity));

            foreach (RDFPatternMember valueNode in valueNodes)
                if (!predicateNodes.Any(p => p.Equals(valueNode)))
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.EQUALS_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             pShape?.Path,
                                                             valueNode,
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
                //sh:equals
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.EQUALS, EqualsPredicate));
            return result;
        }
        #endregion
    }
}