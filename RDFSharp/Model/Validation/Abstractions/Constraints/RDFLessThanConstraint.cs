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
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFLessThanConstraint represents a SHACL constraint on minority comparison of a given RDF term for the specified predicate
    /// </summary>
    public class RDFLessThanConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Predicate for which value nodes of a given RDF term are compared for minority
        /// </summary>
        public RDFResource LessThanPredicate { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a lessThan constraint with the given predicate
        /// </summary>
        public RDFLessThanConstraint(RDFResource lessThanPredicate) : base()
        {
            if (lessThanPredicate != null)
            {
                this.LessThanPredicate = lessThanPredicate;
            }
            else
            {
                throw new RDFModelException("Cannot create RDFLessThanConstraint because given \"lessThanPredicate\" parameter is null.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            #region Evaluation
            List<RDFPatternMember> predicateNodes = dataGraph.Where(t => t.Subject.Equals(focusNode)
                                                                            && t.Predicate.Equals(this.LessThanPredicate))
                                                             .Select(x => x.Object)
                                                             .ToList();

            foreach (RDFPatternMember valueNode in valueNodes)
            {
                foreach (RDFPatternMember predicateNode in predicateNodes)
                {
                    int comparison = RDFQueryUtilities.CompareRDFPatternMembers(valueNode, predicateNode);
                    if (comparison == -99 || comparison >= 0)
                    {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shape.Messages,
                                                                 shape.Severity));
                    }
                }
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

                //sh:lessThan
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.LESS_THAN, this.LessThanPredicate));

            }
            return result;
        }
        #endregion

    }
}