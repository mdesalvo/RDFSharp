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

using RDFSharp.Model.Validation.Abstractions.Shapes;
using RDFSharp.Model.Vocabularies;
using RDFSharp.Query;
using RDFSharp.Query.Mirella.Algebra.Abstractions;

namespace RDFSharp.Model.Validation.Abstractions.Constraints
{
    /// <summary>
    /// RDFMinLengthConstraint represents a SHACL constraint on the minimum allowed length for a given RDF term
    /// </summary>
    public class RDFMinLengthConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Indicates the minimum allowed length for a given RDF term
        /// </summary>
        public uint MinLength { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named minLength constraint
        /// </summary>
        public RDFMinLengthConstraint(RDFResource constraintName, uint minLength) : base(constraintName) {
            this.MinLength = minLength;
        }

        /// <summary>
        /// Default-ctor to build a blank minLength constraint
        /// </summary>
        public RDFMinLengthConstraint(uint minLength) : this(new RDFResource(), minLength) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport EvaluateConstraint(RDFShapesGraph shapesGraph,
                                                                 RDFShape shape,
                                                                 RDFGraph dataGraph,
                                                                 RDFResource focusNode,
                                                                 RDFPatternMember valueNode)
        {
            var report = new RDFValidationReport(new RDFResource());
            switch (valueNode) {

                //Resource
                case RDFResource valueNodeResource:
                    if (valueNodeResource.IsBlank || (this.MinLength > 0 && valueNodeResource.ToString().Length < this.MinLength)) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shape.Messages,
                                                                 new RDFResource(),
                                                                 shape.Severity));
                    }
                    break;

                //Literal
                case RDFLiteral valueNodeLiteral:
                    if (this.MinLength > 0 && valueNodeLiteral.Value.Length < this.MinLength) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shape.Messages,
                                                                 new RDFResource(),
                                                                 shape.Severity));
                    }
                    break;

            }
            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        public override RDFGraph ToRDFGraph(RDFShape shape)
        {
            var result = new RDFGraph();
            if (shape != null) {

                //sh:minLength
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MIN_LENGTH, new RDFTypedLiteral(this.MinLength.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            }
            return result;
        }
        #endregion

    }
}