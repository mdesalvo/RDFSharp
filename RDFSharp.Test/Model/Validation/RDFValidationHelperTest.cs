/*
   Copyright 2012-2021 Marco De Salvo

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
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFValidationHelperTest
    {
        #region Properties
        private RDFGraph dataGraph;
        #endregion

        #region Ctors
        [TestInitialize]
        public void Initialize()
        {
            dataGraph = new RDFGraph();
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("17", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Jane")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Jane"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Jane"), RDFVocabulary.FOAF.NAME, new RDFTypedLiteral("Jane", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Jane"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("11", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Jane"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Carl")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Carl"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Carl"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Carl", "en-us")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Carl"), RDFVocabulary.FOAF.NICK, new RDFPlainLiteral("Carl", "en")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Carl"), RDFVocabulary.FOAF.OPEN_ID, new RDFPlainLiteral("Carl")));

            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Guy"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        }
        #endregion

        #region Tests
        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithoutTargets()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithoutTargets()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.AGE);
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetClass()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 3);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetClass()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 3);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetClassAndReasoning()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetClassAndReasoning()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetClassNoInstances()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Guy")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetClassNoInstances()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Guy")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetClassUnexisting()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:People")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetClassUnexisting()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:People")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetNode()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 1);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetNode()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 1);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetSubjectsOf()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.AGE));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetSubjectsOf()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.AGE));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetSubjectsOfUnexisting()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.BASED_NEAR));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetSubjectsOfUnexisting()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.BASED_NEAR));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetObjectsOf()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetObjectsOf()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.AGE)
                                    .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNodeShapeWithTargetObjectsOfUnexisting()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.MBOX));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfPropertyShapeWithTargetObjectsOfUnexisting()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.MBOX));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }
        #endregion
    }
}