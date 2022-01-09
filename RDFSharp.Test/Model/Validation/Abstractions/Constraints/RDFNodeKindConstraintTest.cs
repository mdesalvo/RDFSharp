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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFNodeKindConstraintTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow(RDFValidationEnums.RDFNodeKinds.BlankNode)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.IRI)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.IRIOrLiteral)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.Literal)]
        public void ShouldCreateNodeKindConstraint(RDFValidationEnums.RDFNodeKinds nodeKind)
        {
            RDFNodeKindConstraint nodeKindConstraint = new RDFNodeKindConstraint(nodeKind);

            Assert.IsNotNull(nodeKindConstraint);
            Assert.IsTrue(nodeKindConstraint.NodeKind == nodeKind);
        }

        [DataTestMethod]
        [DataRow(RDFValidationEnums.RDFNodeKinds.BlankNode)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.IRI)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.IRIOrLiteral)]
        [DataRow(RDFValidationEnums.RDFNodeKinds.Literal)]
        public void ShouldExportNodeKindConstraint(RDFValidationEnums.RDFNodeKinds nodeKind)
        {
            RDFNodeKindConstraint nodeKindConstraint = new RDFNodeKindConstraint(nodeKind);
            RDFGraph graph = nodeKindConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            switch (nodeKind)
            {
                case RDFValidationEnums.RDFNodeKinds.BlankNode:
                    Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                        && t.Value.Predicate.Equals(RDFVocabulary.SHACL.NODE_KIND)
                                                            && t.Value.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE)));
                    break;
                case RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI:
                    Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                        && t.Value.Predicate.Equals(RDFVocabulary.SHACL.NODE_KIND)
                                                            && t.Value.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_IRI)));
                    break;
                case RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral:
                    Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                        && t.Value.Predicate.Equals(RDFVocabulary.SHACL.NODE_KIND)
                                                            && t.Value.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_LITERAL)));
                    break;
                case RDFValidationEnums.RDFNodeKinds.IRI:
                    Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                        && t.Value.Predicate.Equals(RDFVocabulary.SHACL.NODE_KIND)
                                                            && t.Value.Object.Equals(RDFVocabulary.SHACL.IRI)));
                    break;
                case RDFValidationEnums.RDFNodeKinds.IRIOrLiteral:
                    Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                        && t.Value.Predicate.Equals(RDFVocabulary.SHACL.NODE_KIND)
                                                            && t.Value.Object.Equals(RDFVocabulary.SHACL.IRI_OR_LITERAL)));
                    break;
                case RDFValidationEnums.RDFNodeKinds.Literal:
                    Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                        && t.Value.Predicate.Equals(RDFVocabulary.SHACL.NODE_KIND)
                                                            && t.Value.Object.Equals(RDFVocabulary.SHACL.LITERAL)));
                    break;
            }            
        }

        //NS-CONFORMS: TRUE



        //PS-CONFORMS: TRUE



        //NS-CONFORMS: FALSE



        //PS-CONFORMS: FALSE



        //MIXED-CONFORMS: TRUE



        //MIXED-CONFORMS: FALSE


        #endregion
    }
}