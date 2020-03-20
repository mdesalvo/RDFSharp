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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFMaxLengthConstraint represents a SHACL constraint on the maximum allowed length for a given RDF term
    /// </summary>
    public class RDFMaxLengthConstraint: RDFConstraint {

        #region Properties
        /// <summary>
        /// Indicates the maximum allowed length for a given RDF term
        /// </summary>
        public int MaxLength { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named maxLength constraint with the given maxLength
        /// </summary>
        public RDFMaxLengthConstraint(int maxLength) : base() {
            this.MaxLength = maxLength < 0 ? 0 : maxLength;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            validationContext.ValueNodes.ForEach(valueNode => {

                #region Evaluate

                //Set current value node
                validationContext.ValueNode = valueNode;

                //Evaluate current value node
                switch (validationContext.ValueNode) {

                    //Resource
                    case RDFResource valueNodeResource:
                        if (valueNodeResource.IsBlank || valueNodeResource.ToString().Length > this.MaxLength) {
                            report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                     RDFVocabulary.SHACL.MAX_LENGTH_CONSTRAINT_COMPONENT,
                                                                     validationContext.FocusNode,
                                                                     validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                     validationContext.ValueNode,
                                                                     validationContext.Shape.Messages,
                                                                     new RDFResource(),
                                                                     validationContext.Shape.Severity));
                        }
                        break;

                    //Literal
                    case RDFLiteral valueNodeLiteral:
                        if (valueNodeLiteral.Value.Length > this.MaxLength) {
                            report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                     RDFVocabulary.SHACL.MAX_LENGTH_CONSTRAINT_COMPONENT,
                                                                     validationContext.FocusNode,
                                                                     validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                     validationContext.ValueNode,
                                                                     validationContext.Shape.Messages,
                                                                     new RDFResource(),
                                                                     validationContext.Shape.Severity));
                        }
                        break;

                }

                #endregion

            });
            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //sh:maxLength
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_LENGTH, new RDFTypedLiteral(this.MaxLength.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            }
            return result;
        }
        #endregion

    }
}