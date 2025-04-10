/*
   Copyright 2012-2025 Marco De Salvo

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

using System.Collections.Generic;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFNodeKindConstraint represents a SHACL constraint on the specified type for a given RDF term
    /// </summary>
    public sealed class RDFNodeKindConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Allowed type of node for the given RDF term
        /// </summary>
        public RDFValidationEnums.RDFNodeKinds NodeKind { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a nodeKind constraint of the given kind
        /// </summary>
        public RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds nodeKind)
            => NodeKind = nodeKind;
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();
            RDFPropertyShape pShape = shape as RDFPropertyShape;

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Value is not of required kind <{NodeKind}>"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
                switch (valueNode)
                {
                    //Resource
                    case RDFResource valueNodeResource:
                        if (valueNodeResource.IsBlank)
                        {
                            if (NodeKind == RDFValidationEnums.RDFNodeKinds.IRI
                                 || NodeKind == RDFValidationEnums.RDFNodeKinds.IRIOrLiteral
                                 || NodeKind == RDFValidationEnums.RDFNodeKinds.Literal)
                            {
                                report.AddResult(new RDFValidationResult(shape,
                                                                         RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT,
                                                                         focusNode,
                                                                         pShape?.Path,
                                                                         valueNode,
                                                                         shapeMessages,
                                                                         shape.Severity));
                            }
                        }
                        else
                        {
                            if (NodeKind == RDFValidationEnums.RDFNodeKinds.BlankNode
                                 || NodeKind == RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral
                                 || NodeKind == RDFValidationEnums.RDFNodeKinds.Literal)
                            {
                                report.AddResult(new RDFValidationResult(shape,
                                                                         RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT,
                                                                         focusNode,
                                                                         pShape?.Path,
                                                                         valueNode,
                                                                         shapeMessages,
                                                                         shape.Severity));
                            }
                        }
                        break;

                    //Literal
                    case RDFLiteral _:
                        if (NodeKind == RDFValidationEnums.RDFNodeKinds.BlankNode
                             || NodeKind == RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI
                             || NodeKind == RDFValidationEnums.RDFNodeKinds.IRI)
                        {
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     pShape?.Path,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        }
                        break;
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
                //sh:nodeKind
                switch (NodeKind)
                {
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