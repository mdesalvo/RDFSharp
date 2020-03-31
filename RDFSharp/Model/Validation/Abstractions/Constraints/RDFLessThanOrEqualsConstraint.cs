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
    /// RDFLessThanOrEqualsConstraint represents a SHACL constraint on minority or equality comparison of a given RDF term for the specified predicate
    /// </summary>
    public class RDFLessThanOrEqualsConstraint : RDFConstraint
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
        public RDFLessThanOrEqualsConstraint(RDFResource lessThanOrEqualsPredicate) : base()
        {
            if (lessThanOrEqualsPredicate != null) {
                this.LessThanOrEqualsPredicate = lessThanOrEqualsPredicate;
            }
            else {
                throw new RDFModelException("Cannot create RDFLessThanOrEqualsConstraint because given \"lessThanOrEqualsPredicate\" parameter is null.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext)
        {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            #region Evaluation
            //Evaluate focus nodes
            validationContext.FocusNodes.ForEach(focusNode => {

                //Get predicate nodes of current focus node
                List<RDFPatternMember> predicateNodes = validationContext.DataGraph.SelectTriplesBySubject(focusNode)
                                                                                   .SelectTriplesByPredicate(this.LessThanOrEqualsPredicate)
                                                                                   .Select(x => x.Object)
                                                                                   .ToList();

                //Get value nodes of current focus node
                validationContext.ValueNodes[focusNode.PatternMemberID].ForEach(valueNode => {

                    //Evaluate current value node
                    predicateNodes.ForEach(predicateNode => {
                        Int32 comparison = RDFQueryUtilities.CompareRDFPatternMembers(valueNode, predicateNode);
                        if (comparison == -99 || comparison > 0) {
                            report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                     RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                     valueNode,
                                                                     validationContext.Shape.Messages,
                                                                     validationContext.Shape.Severity));
                        }
                    });

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

                //sh:lessThanOrEquals
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS, this.LessThanOrEqualsPredicate));

            }
            return result;
        }
        #endregion

    }
}