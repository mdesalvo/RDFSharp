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
    /// RDFEqualsConstraint represents a SHACL constraint on absence of a given RDF term for the specified predicate
    /// </summary>
    public class RDFDisjointConstraint : RDFConstraint {

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
        public RDFDisjointConstraint(RDFResource disjointPredicate) : base() {
            if (disjointPredicate != null) {
                this.DisjointPredicate = disjointPredicate;
            }
            else {
                throw new RDFModelException("Cannot create RDFDisjointConstraint because given \"disjointPredicate\" parameter is null.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            #region Evaluation
            //Evaluate focus nodes
            validationContext.FocusNodes.ForEach(focusNode => {

                //Get value nodes of current focus node
                validationContext.ValueNodes[focusNode.PatternMemberID].ForEach(valueNode => {

                    //Evaluate current value node
                    if (validationContext.DataGraph.Any(t => t.Subject.Equals(focusNode) 
                                                                && t.Predicate.Equals(this.DisjointPredicate)
                                                                    && t.Object.Equals(valueNode))) {
                        report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                 RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                 valueNode,
                                                                 validationContext.Shape.Messages,
                                                                 validationContext.Shape.Severity));
                    }

                });

            });
            #endregion

            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //sh:disjoint
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.DISJOINT, this.DisjointPredicate));

            }
            return result;
        }
        #endregion

    }
}