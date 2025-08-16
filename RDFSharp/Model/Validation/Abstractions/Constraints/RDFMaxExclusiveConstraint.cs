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
    /// RDFMaxExclusiveConstraint represents a SHACL constraint on an exclusive upper-bound value for a given RDF term
    /// </summary>
    public sealed class RDFMaxExclusiveConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Exclusive upper-bound value required on the given RDF term
        /// </summary>
        public RDFPatternMember Value { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a maxExclusive constraint with the given resource value
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        public RDFMaxExclusiveConstraint(RDFResource value)
            => Value = value ?? throw new RDFModelException("Cannot create RDFMaxExclusiveConstraint because given \"value\" parameter is null.");

        /// <summary>
        /// Default-ctor to build a maxExclusive constraint with the given literal value
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        public RDFMaxExclusiveConstraint(RDFLiteral value)
            => Value = value ?? throw new RDFModelException("Cannot create RDFMaxExclusiveConstraint because given \"value\" parameter is null.");
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
                shapeMessages.Add(new RDFPlainLiteral($"Must have values lower than <{Value}>"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                int comparison = RDFQueryUtilities.CompareRDFPatternMembers(Value, valueNode);
                if (comparison == -99 || comparison <= 0)
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT,
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
                //sh:maxExclusive
                if (Value is RDFResource valRes)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_EXCLUSIVE, valRes));
                else
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_EXCLUSIVE, (RDFLiteral)Value));
            }
            return result;
        }
        #endregion
    }
}