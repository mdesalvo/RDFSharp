﻿/*
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
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFDatatypeConstraint represents a SHACL constraint on the specified datatype for a given RDF term
    /// </summary>
    public class RDFDatatypeConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Allowed datatype for the given RDF term
        /// </summary>
        public RDFModelEnums.RDFDatatypes Datatype { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a datatype constraint with the given datatype
        /// </summary>
        public RDFDatatypeConstraint(RDFModelEnums.RDFDatatypes datatype) : base() {
            this.Datatype = datatype;
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
                    switch (valueNode) {

                        //Resource
                        case RDFResource valueNodeResource:
                            report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                     RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                     valueNode,
                                                                     validationContext.Shape.Messages,
                                                                     validationContext.Shape.Severity));
                            break;

                        //PlainLiteral
                        case RDFPlainLiteral valueNodePlainLiteral:
                            if (this.Datatype != RDFModelEnums.RDFDatatypes.XSD_STRING
                                    || !string.IsNullOrEmpty(valueNodePlainLiteral.Language)) {
                                report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                         RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT,
                                                                         focusNode,
                                                                         validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                         valueNode,
                                                                         validationContext.Shape.Messages,
                                                                         validationContext.Shape.Severity));
                            }
                            break;

                        //TypedLiteral
                        case RDFTypedLiteral valueNodeTypedLiteral:
                            if (this.Datatype != valueNodeTypedLiteral.Datatype) {
                                report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                         RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT,
                                                                         focusNode,
                                                                         validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                         valueNode,
                                                                         validationContext.Shape.Messages,
                                                                         validationContext.Shape.Severity));
                            }
                            break;

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

                //sh:datatype
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.DATATYPE, new RDFResource(RDFModelUtilities.GetDatatypeFromEnum(this.Datatype))));

            }
            return result;
        }
        #endregion

    }
}