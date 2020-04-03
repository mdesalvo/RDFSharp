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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFMaxInclusiveConstraint represents a SHACL constraint on an inclusive upper-bound value for a given RDF term
    /// </summary>
    public class RDFMaxInclusiveConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Inclusive upper-bound value required on the given RDF term
        /// </summary>
        public RDFPatternMember Value { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a maxInclusive constraint with the given resource value
        /// </summary>
        public RDFMaxInclusiveConstraint(RDFResource value) : base() {
            if (value != null) {
                this.Value = value;
            }
            else {
                throw new RDFModelException("Cannot create RDFMaxInclusiveConstraint because given \"value\" parameter is null.");
            }
        }

        /// <summary>
        /// Default-ctor to build a maxInclusive constraint with the given literal value
        /// </summary>
        public RDFMaxInclusiveConstraint(RDFLiteral value) : base() {
            if (value != null) {
                this.Value = value;
            }
            else {
                throw new RDFModelException("Cannot create RDFMaxInclusiveConstraint because given \"value\" parameter is null.");
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
                    Int32 comparison = RDFQueryUtilities.CompareRDFPatternMembers(this.Value, valueNode);
                    if (comparison == -99 || comparison < 0) {
                        report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                 RDFVocabulary.SHACL.MAX_INCLUSIVE_CONSTRAINT_COMPONENT,
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

                //sh:maxInclusive
                if (this.Value is RDFResource)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_INCLUSIVE, (RDFResource)this.Value));
                else
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_INCLUSIVE, (RDFLiteral)this.Value));

            }
            return result;
        }
        #endregion

    }
}