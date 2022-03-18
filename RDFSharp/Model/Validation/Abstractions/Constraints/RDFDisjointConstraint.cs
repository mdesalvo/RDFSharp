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
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFDisjointConstraint represents a SHACL constraint on absence of a given RDF term for the specified predicate
    /// </summary>
    public class RDFDisjointConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Predicate for which value nodes of a given RDF term are checked for absence
        /// </summary>
        public RDFResource DisjointPredicate { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a disjoint constraint with the given predicate
        /// </summary>
        public RDFDisjointConstraint(RDFResource disjointPredicate)
        {
            if (disjointPredicate == null)
                throw new RDFModelException("Cannot create RDFDisjointConstraint because given \"disjointPredicate\" parameter is null.");
            
            this.DisjointPredicate = disjointPredicate;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Must not have common values with property <{this.DisjointPredicate}>"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                if (dataGraph.Any(t => t.Subject.Equals(focusNode)
                                           && t.Predicate.Equals(this.DisjointPredicate)
                                               && t.Object.Equals(valueNode)))
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT,
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
                //sh:disjoint
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.DISJOINT, this.DisjointPredicate));
            }
            return result;
        }
        #endregion
    }
}