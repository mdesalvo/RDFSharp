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
    /// RDFLessThanOrEqualsConstraint represents a SHACL constraint on minority or equality comparison of a given RDF term for the specified predicate
    /// </summary>
    public sealed class RDFLessThanOrEqualsConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Predicate for which value nodes of a given RDF term are compared for minority or equality
        /// </summary>
        public RDFResource LessThanOrEqualsPredicate { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a lessThanOrEquals constraint with the given predicate
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        public RDFLessThanOrEqualsConstraint(RDFResource lessThanOrEqualsPredicate)
            => LessThanOrEqualsPredicate = lessThanOrEqualsPredicate ?? throw new RDFModelException("Cannot create RDFLessThanOrEqualsConstraint because given \"lessThanOrEqualsPredicate\" parameter is null.");
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
                shapeMessages.Add(new RDFPlainLiteral($"Must have values less than or equals to values of property <{LessThanOrEqualsPredicate}>"));

            #region Evaluation
            List<RDFPatternMember> predicateNodes = dataGraph.Where(t => t.Subject.Equals(focusNode)
                                                                          && t.Predicate.Equals(LessThanOrEqualsPredicate))
                                                             .Select(x => x.Object)
                                                             .ToList();

            foreach (RDFPatternMember valueNode in valueNodes)
                foreach (RDFPatternMember predicateNode in predicateNodes)
                {
                    int comparison = RDFQueryUtilities.CompareRDFPatternMembers(valueNode, predicateNode);
                    if (comparison == -99 || comparison > 0)
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT,
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
                //sh:lessThanOrEquals
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS, LessThanOrEqualsPredicate));
            return result;
        }
        #endregion
    }
}