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
using System.Linq;
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

        [TestMethod]
        public void ShouldGetFocusNodesOfNullShape()
        {
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, null);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetFocusNodesOfNullDataGraph()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(null, nShape);

            Assert.IsNotNull(focusNodes);
            Assert.IsTrue(focusNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithoutTargets()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithoutTargets()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.AGE);
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetClass()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 3);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetClass()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 3);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetClassAndReasoning()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetClassAndReasoning()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 5);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetClassNoInstances()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Guy")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetClassNoInstances()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:Guy")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetClassUnexisting()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:People")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetClassUnexisting()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetClass(new RDFResource("ex:People")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetNode()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 1);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetNode()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 1);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetSubjectsOf()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.AGE));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetSubjectsOf()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.AGE));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 5);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetSubjectsOfUnexisting()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.BASED_NEAR));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetSubjectsOfUnexisting()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.BASED_NEAR));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetObjectsOf()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetObjectsOf()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.AGE)
                                    .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 4);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNodeShapeWithTargetObjectsOfUnexisting()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
                                    .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.MBOX));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, nShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfPropertyShapeWithTargetObjectsOfUnexisting()
        {
            RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
                                    .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.MBOX));
            List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(dataGraph, pShape);
            List<RDFPatternMember> valueNodes = new List<RDFPatternMember>();
            focusNodes.ForEach(focusNode => valueNodes.AddRange(RDFValidationHelper.GetValueNodesOf(dataGraph, pShape, focusNode)));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNullShape()
        {
            List<RDFPatternMember> valueNodes = RDFValidationHelper.GetValueNodesOf(dataGraph, null, new RDFResource("ex:Alice"));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNullDataGraph()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            List<RDFPatternMember> valueNodes = RDFValidationHelper.GetValueNodesOf(null, nShape, new RDFResource("ex:Alice"));

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetValueNodesOfNullFocusNode()
        {
            RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            List<RDFPatternMember> valueNodes = RDFValidationHelper.GetValueNodesOf(dataGraph, nShape, null);

            Assert.IsNotNull(valueNodes);
            Assert.IsTrue(valueNodes.Count == 0);
        }

        [TestMethod]
        public void ShouldGetInstancesOfClass()
        {
            List<RDFPatternMember> persons = RDFValidationHelper.GetInstancesOfClass(dataGraph, new RDFResource("ex:Person"));

            Assert.IsNotNull(persons);
            Assert.IsTrue(persons.Count == 3);
        }

        [TestMethod]
        public void ShouldGetInstancesOfClassWithReasoning()
        {
            List<RDFPatternMember> persons = RDFValidationHelper.GetInstancesOfClass(dataGraph, new RDFResource("ex:Human"));

            Assert.IsNotNull(persons);
            Assert.IsTrue(persons.Count == 4);
        }

        [TestMethod]
        public void ShouldGetInstancesOfUnreferencedClass()
        {
            List<RDFPatternMember> persons = RDFValidationHelper.GetInstancesOfClass(dataGraph, new RDFResource("ex:Guy"));

            Assert.IsNotNull(persons);
            Assert.IsTrue(persons.Count == 0);
        }

        [TestMethod]
        public void ShouldGetInstancesOfUnexistingClass()
        {
            List<RDFPatternMember> persons = RDFValidationHelper.GetInstancesOfClass(dataGraph, new RDFResource("ex:People"));

            Assert.IsNotNull(persons);
            Assert.IsTrue(persons.Count == 0);
        }

        [TestMethod]
        public void ShouldGetInstancesOfNullClass()
        {
            List<RDFPatternMember> persons = RDFValidationHelper.GetInstancesOfClass(dataGraph, null);

            Assert.IsNotNull(persons);
            Assert.IsTrue(persons.Count == 0);
        }

        [TestMethod]
        public void ShouldGetInstancesOfClassFromNullDataGraph()
        {
            List<RDFPatternMember> persons = RDFValidationHelper.GetInstancesOfClass(null, new RDFResource("ex:Person"));

            Assert.IsNotNull(persons);
            Assert.IsTrue(persons.Count == 0);
        }

        [TestMethod]
        public void ShouldParseFromNullGraph()
        {
            RDFShapesGraph shapesGraph = RDFValidationHelper.FromRDFGraph(null);

            Assert.IsNull(shapesGraph);
        }

        [DataTestMethod]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Info)]
        public void ShouldParseNodeShapeWithClassConstraintFromGraph(RDFValidationEnums.RDFShapeSeverity severity)
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.SetSeverity(severity);
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
            nodeShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            nodeShape.AddMessage(new RDFPlainLiteral("message", "en-US"));
            nodeShape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Human")));
            shapesGraph.AddShape(nodeShape);
            RDFGraph graph = shapesGraph.ToRDFGraph();
            RDFShapesGraph shapesGraph2 = RDFValidationHelper.FromRDFGraph(graph);

            Assert.IsNotNull(shapesGraph2);
            Assert.IsTrue(shapesGraph2.Equals(new RDFResource("ex:shapesGraph")));
            Assert.IsTrue(shapesGraph2.ShapesCount == 1);
            RDFNodeShape nodeShape2 = shapesGraph2.SelectShape("ex:nodeShape") as RDFNodeShape;
            Assert.IsNotNull(nodeShape2);
            Assert.IsFalse(nodeShape2.Deactivated);
            Assert.IsTrue(nodeShape2.Severity == severity);
            Assert.IsTrue(nodeShape2.ConstraintsCount == 1);
            RDFClassConstraint nodeShape2ClassConstraint = nodeShape2.Constraints.Single() as RDFClassConstraint;
            Assert.IsNotNull(nodeShape2ClassConstraint);
            Assert.IsTrue(nodeShape2ClassConstraint.ClassType.Equals(new RDFResource("ex:Human")));
            Assert.IsTrue(nodeShape2.TargetsCount == 4);
            RDFTargetClass nodeShape2TargetClass = nodeShape2.Targets.Single(x => x is RDFTargetClass) as RDFTargetClass;
            Assert.IsNotNull(nodeShape2TargetClass);
            Assert.IsTrue(nodeShape2TargetClass.TargetValue.Equals(new RDFResource("ex:Person")));
            RDFTargetNode nodeShape2TargetNode = nodeShape2.Targets.Single(x => x is RDFTargetNode) as RDFTargetNode;
            Assert.IsNotNull(nodeShape2TargetNode);
            Assert.IsTrue(nodeShape2TargetNode.TargetValue.Equals(new RDFResource("ex:Alice")));
            RDFTargetSubjectsOf nodeShape2TargetSubjectsOf = nodeShape2.Targets.Single(x => x is RDFTargetSubjectsOf) as RDFTargetSubjectsOf;
            Assert.IsNotNull(nodeShape2TargetSubjectsOf);
            Assert.IsTrue(nodeShape2TargetSubjectsOf.TargetValue.Equals(RDFVocabulary.FOAF.KNOWS));
            RDFTargetObjectsOf nodeShape2TargetObjectsOf = nodeShape2.Targets.Single(x => x is RDFTargetObjectsOf) as RDFTargetObjectsOf;
            Assert.IsNotNull(nodeShape2TargetObjectsOf);
            Assert.IsTrue(nodeShape2TargetObjectsOf.TargetValue.Equals(RDFVocabulary.FOAF.KNOWS));
            Assert.IsTrue(nodeShape2.MessagesCount == 1);
            RDFPlainLiteral nodeShape2Message = nodeShape2.Messages.Single() as RDFPlainLiteral;
            Assert.IsNotNull(nodeShape2Message);
            Assert.IsTrue(nodeShape2Message.Equals(new RDFPlainLiteral("message", "en-US")));
        }

        [DataTestMethod]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Info)]
        public void ShouldParsePropertyShapeWithClassConstraintFromGraph(RDFValidationEnums.RDFShapeSeverity severity)
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.SetSeverity(severity);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
            propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            propertyShape.AddMessage(new RDFPlainLiteral("message", "en-US"));
            propertyShape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Human")));
            propertyShape.AddDescription(new RDFPlainLiteral("description", "en-US"));
            propertyShape.AddName(new RDFPlainLiteral("name", "en-US"));
            propertyShape.SetOrder(1);
            propertyShape.SetGroup(new RDFResource("ex:shapeGroup"));
            shapesGraph.AddShape(propertyShape);
            RDFGraph graph = shapesGraph.ToRDFGraph();
            RDFShapesGraph shapesGraph2 = RDFValidationHelper.FromRDFGraph(graph);

            Assert.IsNotNull(shapesGraph2);
            Assert.IsTrue(shapesGraph2.ShapesCount == 1);
            Assert.IsTrue(shapesGraph2.Equals(new RDFResource("ex:shapesGraph")));
            RDFPropertyShape propertyShape2 = shapesGraph2.SelectShape("ex:propertyShape") as RDFPropertyShape;
            Assert.IsNotNull(propertyShape2);
            Assert.IsFalse(propertyShape2.Deactivated);
            Assert.IsTrue(propertyShape2.Severity == severity);
            Assert.IsTrue(propertyShape2.Descriptions.Count == 1);
            Assert.IsTrue(propertyShape2.Descriptions.Single().Equals(new RDFPlainLiteral("description", "en-US")));
            Assert.IsTrue(propertyShape2.Names.Count == 1);
            Assert.IsTrue(propertyShape2.Names.Single().Equals(new RDFPlainLiteral("name", "en-US")));
            Assert.IsNotNull(propertyShape2.Order);
            Assert.IsTrue(propertyShape2.Order.Equals(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsNotNull(propertyShape2.Group);
            Assert.IsTrue(propertyShape2.Group.Equals(new RDFResource("ex:shapeGroup")));
            Assert.IsTrue(propertyShape2.ConstraintsCount == 1);
            RDFClassConstraint propertyShape2ClassConstraint = propertyShape2.Constraints.Single() as RDFClassConstraint;
            Assert.IsNotNull(propertyShape2ClassConstraint);
            Assert.IsTrue(propertyShape2ClassConstraint.ClassType.Equals(new RDFResource("ex:Human")));
            Assert.IsTrue(propertyShape2.TargetsCount == 4);
            RDFTargetClass propertyShape2TargetClass = propertyShape2.Targets.Single(x => x is RDFTargetClass) as RDFTargetClass;
            Assert.IsNotNull(propertyShape2TargetClass);
            Assert.IsTrue(propertyShape2TargetClass.TargetValue.Equals(new RDFResource("ex:Person")));
            RDFTargetNode propertyShape2TargetNode = propertyShape2.Targets.Single(x => x is RDFTargetNode) as RDFTargetNode;
            Assert.IsNotNull(propertyShape2TargetNode);
            Assert.IsTrue(propertyShape2TargetNode.TargetValue.Equals(new RDFResource("ex:Alice")));
            RDFTargetSubjectsOf propertyShape2TargetSubjectsOf = propertyShape2.Targets.Single(x => x is RDFTargetSubjectsOf) as RDFTargetSubjectsOf;
            Assert.IsNotNull(propertyShape2TargetSubjectsOf);
            Assert.IsTrue(propertyShape2TargetSubjectsOf.TargetValue.Equals(RDFVocabulary.FOAF.KNOWS));
            RDFTargetObjectsOf propertyShape2TargetObjectsOf = propertyShape2.Targets.Single(x => x is RDFTargetObjectsOf) as RDFTargetObjectsOf;
            Assert.IsNotNull(propertyShape2TargetObjectsOf);
            Assert.IsTrue(propertyShape2TargetObjectsOf.TargetValue.Equals(RDFVocabulary.FOAF.KNOWS));
            Assert.IsTrue(propertyShape2.MessagesCount == 1);
            RDFPlainLiteral propertyShape2Message = propertyShape2.Messages.Single() as RDFPlainLiteral;
            Assert.IsNotNull(propertyShape2Message);
            Assert.IsTrue(propertyShape2Message.Equals(new RDFPlainLiteral("message", "en-US")));
        }
        #endregion
    }
}