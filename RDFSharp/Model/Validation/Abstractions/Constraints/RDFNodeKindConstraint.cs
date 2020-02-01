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
using System;

namespace RDFSharp.Model.Validation.Abstractions.Constraints
{
    /// <summary>
    /// RDFNodeKindConstraint represents a SHACL constraint on the specified type for a given RDF term
    /// </summary>
    public class RDFNodeKindConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Allowed type of node for the given RDF term
        /// </summary>
        public RDFValidationEnums.RDFNodeKinds NodeKind { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named nodeKind constraint
        /// </summary>
        public RDFNodeKindConstraint(RDFResource constraintName, RDFValidationEnums.RDFNodeKinds nodeKind) : base(constraintName) {
            this.NodeKind = nodeKind;
        }

        /// <summary>
        /// Default-ctor to build a blank nodeKind constraint
        /// </summary>
        public RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds nodeKind) : this(new RDFResource(), nodeKind) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport EvaluateConstraint(RDFShapesGraph shapesGraph,
                                                                 RDFShape shape,
                                                                 RDFGraph dataGraph,
                                                                 RDFResource focusNode,
                                                                 RDFPatternMember valueNode) {
            var report = new RDFValidationReport(new RDFResource());
            switch (valueNode) {

                //Resource
                case RDFResource valueNodeResource:
                    if (valueNodeResource.IsBlank) {
                        if (this.NodeKind == RDFValidationEnums.RDFNodeKinds.IRI
                                || this.NodeKind == RDFValidationEnums.RDFNodeKinds.IRIOrLiteral
                                    || this.NodeKind == RDFValidationEnums.RDFNodeKinds.Literal) {
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                     valueNode,
                                                                     shape.Messages,
                                                                     new RDFResource(),
                                                                     shape.Severity));
                        }
                    }
                    else {
                        if (this.NodeKind == RDFValidationEnums.RDFNodeKinds.BlankNode
                                || this.NodeKind == RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral
                                    || this.NodeKind == RDFValidationEnums.RDFNodeKinds.Literal) {
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                     valueNode,
                                                                     shape.Messages,
                                                                     new RDFResource(),
                                                                     shape.Severity));
                        }
                    }
                    break;

                //Literal
                case RDFLiteral valueNodeLiteral:
                    if (this.NodeKind == RDFValidationEnums.RDFNodeKinds.BlankNode
                            || this.NodeKind == RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI
                                || this.NodeKind == RDFValidationEnums.RDFNodeKinds.IRI) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT,
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
        public override RDFGraph ToRDFGraph(RDFShape shape) {
            var result = new RDFGraph();
            if (shape != null) {

                //sh:nodeKind
                switch (this.NodeKind) {
                    case RDFValidationEnums.RDFNodeKinds.BlankNode:
                        result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NODE_KIND, RDFVocabulary.SHACL.BLANK_NODE));
                        break;
                    case RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI:
                        result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NODE_KIND, RDFVocabulary.SHACL.BLANK_NODE_OR_IRI));
                        break;
                    case RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral:
                        result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NODE_KIND, RDFVocabulary.SHACL.BLANK_NODE_OR_LITERAL));
                        break;
                    case RDFValidationEnums.RDFNodeKinds.IRI:
                        result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NODE_KIND, RDFVocabulary.SHACL.IRI));
                        break;
                    case RDFValidationEnums.RDFNodeKinds.IRIOrLiteral:
                        result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NODE_KIND, RDFVocabulary.SHACL.IRI_OR_LITERAL));
                        break;
                    case RDFValidationEnums.RDFNodeKinds.Literal:
                        result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.NODE_KIND, RDFVocabulary.SHACL.LITERAL));
                        break;
                }

            }
            return result;
        }
        #endregion

    }
}