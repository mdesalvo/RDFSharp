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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFMinLengthConstraint represents a SHACL constraint on the minimum allowed length for a given RDF term
    /// </summary>
    public class RDFMinLengthConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Indicates the minimum allowed length for a given RDF term
        /// </summary>
        public int MinLength { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a minLength constraint with the given minLength
        /// </summary>
        public RDFMinLengthConstraint(int minLength)
            => this.MinLength = minLength < 0 ? 0 : minLength;
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
                shapeMessages.Add(new RDFPlainLiteral($"Must have a minimum length of {this.MinLength} characters and can't be a blank node"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                switch (valueNode)
                {
                    //Resource
                    case RDFResource valueNodeResource:
                        if (valueNodeResource.IsBlank
                                || (this.MinLength > 0 && valueNodeResource.ToString().Length < this.MinLength))
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        break;

                    //Literal
                    case RDFLiteral valueNodeLiteral:
                        if (this.MinLength > 0 && valueNodeLiteral.Value.Length < this.MinLength)
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        break;
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
                //sh:minLength
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MIN_LENGTH, new RDFTypedLiteral(this.MinLength.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            }
            return result;
        }
        #endregion
    }
}
