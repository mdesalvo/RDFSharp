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
    public class RDFQualifiedValueShapeConstraintTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateQualifiedValueShapeConstraint()
        {
            RDFQualifiedValueShapeConstraint qvsConstraint = new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2);

            Assert.IsNotNull(qvsConstraint);
            Assert.IsNotNull(qvsConstraint.QualifiedValueShapeUri);
            Assert.IsTrue(qvsConstraint.QualifiedValueShapeUri.Equals(new RDFResource("ex:QVShape")));
            Assert.IsTrue(qvsConstraint.QualifiedValueMinCount.HasValue);
            Assert.IsTrue(qvsConstraint.QualifiedValueMinCount.Value == 1);
            Assert.IsTrue(qvsConstraint.QualifiedValueMaxCount.HasValue);
            Assert.IsTrue(qvsConstraint.QualifiedValueMaxCount.Value == 2);
        }

        [TestMethod]
        public void ShouldCreateQualifiedValueShapeConstraintWithOutBoundValues()
        {
            RDFQualifiedValueShapeConstraint qvsConstraint = new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), -1, -2);

            Assert.IsNotNull(qvsConstraint);
            Assert.IsNotNull(qvsConstraint.QualifiedValueShapeUri);
            Assert.IsTrue(qvsConstraint.QualifiedValueShapeUri.Equals(new RDFResource("ex:QVShape")));
            Assert.IsTrue(qvsConstraint.QualifiedValueMinCount.HasValue);
            Assert.IsTrue(qvsConstraint.QualifiedValueMinCount.Value == 0);
            Assert.IsTrue(qvsConstraint.QualifiedValueMaxCount.HasValue);
            Assert.IsTrue(qvsConstraint.QualifiedValueMaxCount.Value == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingQualifiedValueShapeConstraint()
            => Assert.ThrowsException<RDFModelException>(() => new RDFQualifiedValueShapeConstraint(null, 0, 0));

        [TestMethod]
        public void ShouldExportQualifiedValueShapeConstraint()
        {
            RDFQualifiedValueShapeConstraint qvsConstraint = new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2);
            RDFGraph graph = qvsConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 3);
            Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                    && t.Value.Predicate.Equals(RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE)
                                                        && t.Value.Object.Equals(new RDFResource("ex:QVShape"))));
            Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                    && t.Value.Predicate.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT)
                                                        && t.Value.Object.Equals(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
            Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                    && t.Value.Predicate.Equals(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT)
                                                        && t.Value.Object.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        //NS-CONFORMS:TRUE



        //PS-CONFORMS:TRUE



        //NS-CONFORMS:FALSE



        //PS-CONFORMS:FALSE


        #endregion
    }
}