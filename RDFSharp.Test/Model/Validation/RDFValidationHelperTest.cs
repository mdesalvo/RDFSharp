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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RDFSharp.Test.Model;

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
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithoutTargets()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.AGE);
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetClass()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(3, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetClass()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(3, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetClassAndReasoning()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(4, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetClassAndReasoning()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(4, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetClassNoInstances()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Guy")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetClassNoInstances()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Guy")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetClassUnexisting()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetClass(new RDFResource("ex:People")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetClassUnexisting()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetClass(new RDFResource("ex:People")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetNode()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(1, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetNode()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(1, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetSubjectsOf()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.AGE));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(4, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetSubjectsOf()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.AGE));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(4, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetSubjectsOfUnexisting()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.BASED_NEAR));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetSubjectsOfUnexisting()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.BASED_NEAR));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetObjectsOf()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(4, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetObjectsOf()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.AGE)
            .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.HasCount(4, focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNodeShapeWithTargetObjectsOfUnexisting()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.MBOX));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfPropertyShapeWithTargetObjectsOfUnexisting()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.MBOX));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNullShape()
    {
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(null);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetFocusNodesOfNullDataGraph()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
        List<RDFPatternMember> focusNodes = RDFValidationHelper.GetFocusNodesOf(null, nShape);

        Assert.IsNotNull(focusNodes);
        Assert.IsEmpty(focusNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithoutTargets()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithoutTargets()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.AGE);
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetClass()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(3, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetClass()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(3, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetClassAndReasoning()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(4, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetClassAndReasoning()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(5, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetClassNoInstances()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Guy")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetClassNoInstances()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetClass(new RDFResource("ex:Guy")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetClassUnexisting()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetClass(new RDFResource("ex:People")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetClassUnexisting()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetClass(new RDFResource("ex:People")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetNode()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(1, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetNode()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(1, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetSubjectsOf()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.AGE));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(4, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetSubjectsOf()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.AGE));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(5, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetSubjectsOfUnexisting()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.BASED_NEAR));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetSubjectsOfUnexisting()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.BASED_NEAR));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetObjectsOf()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(4, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetObjectsOf()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.AGE)
            .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.HasCount(4, valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNodeShapeWithTargetObjectsOfUnexisting()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"))
            .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.MBOX));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(nShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(nShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfPropertyShapeWithTargetObjectsOfUnexisting()
    {
        RDFShape pShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS)
            .AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.MBOX));
        List<RDFPatternMember> focusNodes = dataGraph.GetFocusNodesOf(pShape);
        List<RDFPatternMember> valueNodes = [];
        focusNodes.ForEach(focusNode => valueNodes.AddRange(dataGraph.GetValueNodesOf(pShape, focusNode)));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNullShape()
    {
        List<RDFPatternMember> valueNodes = dataGraph.GetValueNodesOf(null, new RDFResource("ex:Alice"));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNullDataGraph()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
        List<RDFPatternMember> valueNodes = RDFValidationHelper.GetValueNodesOf(null, nShape, new RDFResource("ex:Alice"));

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetValueNodesOfNullFocusNode()
    {
        RDFShape nShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
        List<RDFPatternMember> valueNodes = dataGraph.GetValueNodesOf(nShape, null);

        Assert.IsNotNull(valueNodes);
        Assert.IsEmpty(valueNodes);
    }

    [TestMethod]
    public void ShouldGetInstancesOfClass()
    {
        List<RDFPatternMember> persons = dataGraph.GetInstancesOfClass(new RDFResource("ex:Person"));

        Assert.IsNotNull(persons);
        Assert.HasCount(3, persons);
    }

    [TestMethod]
    public void ShouldGetInstancesOfClassWithReasoning()
    {
        List<RDFPatternMember> persons = dataGraph.GetInstancesOfClass(new RDFResource("ex:Human"));

        Assert.IsNotNull(persons);
        Assert.HasCount(4, persons);
    }

    [TestMethod]
    public void ShouldGetInstancesOfUnreferencedClass()
    {
        List<RDFPatternMember> persons = dataGraph.GetInstancesOfClass(new RDFResource("ex:Guy"));

        Assert.IsNotNull(persons);
        Assert.IsEmpty(persons);
    }

    [TestMethod]
    public void ShouldGetInstancesOfUnexistingClass()
    {
        List<RDFPatternMember> persons = dataGraph.GetInstancesOfClass(new RDFResource("ex:People"));

        Assert.IsNotNull(persons);
        Assert.IsEmpty(persons);
    }

    [TestMethod]
    public void ShouldGetInstancesOfNullClass()
    {
        List<RDFPatternMember> persons = dataGraph.GetInstancesOfClass(null);

        Assert.IsNotNull(persons);
        Assert.IsEmpty(persons);
    }

    [TestMethod]
    public void ShouldGetInstancesOfClassFromNullDataGraph()
    {
        List<RDFPatternMember> persons = RDFValidationHelper.GetInstancesOfClass(null, new RDFResource("ex:Person"));

        Assert.IsNotNull(persons);
        Assert.IsEmpty(persons);
    }

    [TestMethod]
    public void ShouldParseFromNullGraph()
    {
        RDFShapesGraph shapesGraph = RDFValidationHelper.FromRDFGraph(null);

        Assert.IsNull(shapesGraph);
    }

    [TestMethod]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation, RDFValidationEnums.RDFNodeKinds.BlankNode)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning, RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Info, RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation, RDFValidationEnums.RDFNodeKinds.IRI)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning, RDFValidationEnums.RDFNodeKinds.IRIOrLiteral)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Info, RDFValidationEnums.RDFNodeKinds.Literal)]
    public void ShouldParseNodeShapeFromGraph(RDFValidationEnums.RDFShapeSeverity severity, RDFValidationEnums.RDFNodeKinds nodeKind)
    {
        RDFNodeShape shape = new RDFNodeShape(new RDFResource("ex:nodeShape"));

        //Attributes
        shape.SetSeverity(severity);
        shape.AddMessage(new RDFPlainLiteral("message", "en-US"));

        //Targets
        shape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        shape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        shape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
        shape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));

        //Constraints
        shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Human")));
        shape.AddConstraint(new RDFAndConstraint().AddShape(shape));
        shape.AddConstraint(new RDFClosedConstraint(true).AddIgnoredProperty(RDFVocabulary.FOAF.KNOWS));
        shape.AddConstraint(new RDFDatatypeConstraint(RDFModelEnums.RDFDatatypes.XSD_INTEGER));
        shape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.KNOWS));
        shape.AddConstraint(new RDFEqualsConstraint(RDFVocabulary.FOAF.KNOWS));
        shape.AddConstraint(new RDFHasValueConstraint(new RDFResource("ex:Alice")));
        shape.AddConstraint(new RDFHasValueConstraint(new RDFPlainLiteral("Alice")));
        shape.AddConstraint(new RDFInConstraint(RDFModelEnums.RDFItemTypes.Resource).AddValue(new RDFResource("ex:Alice")));
        shape.AddConstraint(new RDFInConstraint(RDFModelEnums.RDFItemTypes.Literal).AddValue(new RDFPlainLiteral("Alice")));
        shape.AddConstraint(new RDFLanguageInConstraint(["en-US"]));
        shape.AddConstraint(new RDFLessThanConstraint(new RDFResource("ex:prop")));
        shape.AddConstraint(new RDFLessThanOrEqualsConstraint(new RDFResource("ex:prop")));
        shape.AddConstraint(new RDFMaxCountConstraint(2));
        shape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        shape.AddConstraint(new RDFMaxInclusiveConstraint(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        shape.AddConstraint(new RDFMaxLengthConstraint(2));
        shape.AddConstraint(new RDFMinCountConstraint(2));
        shape.AddConstraint(new RDFMinExclusiveConstraint(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        shape.AddConstraint(new RDFMinInclusiveConstraint(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        shape.AddConstraint(new RDFMinLengthConstraint(2));
        shape.AddConstraint(new RDFNodeConstraint(new RDFResource("ex:nodeShape")));
        shape.AddConstraint(new RDFNodeKindConstraint(nodeKind));
        shape.AddConstraint(new RDFNotConstraint(new RDFResource("ex:notShape")));
        shape.AddConstraint(new RDFOrConstraint().AddShape(shape));
        shape.AddConstraint(new RDFPatternConstraint(new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace)));
        shape.AddConstraint(new RDFPropertyConstraint(new RDFResource("ex:propertyShape")));
        shape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:qvShape"), 2, 3));
        shape.AddConstraint(new RDFUniqueLangConstraint(true));
        shape.AddConstraint(new RDFXoneConstraint().AddShape(shape));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
        shapesGraph.AddShape(shape);
        RDFGraph graph = shapesGraph.ToRDFGraph();
        RDFShapesGraph shapesGraph2 = RDFValidationHelper.FromRDFGraph(graph);

        #region Asserts
        Assert.IsNotNull(shapesGraph2);
        Assert.IsTrue(shapesGraph2.Equals(new RDFResource("ex:shapesGraph")));
        Assert.AreEqual(1, shapesGraph2.ShapesCount);
        RDFNodeShape shape2 = shapesGraph2.SelectShape("ex:nodeShape") as RDFNodeShape;
        Assert.IsNotNull(shape2);

        //Attributes
        Assert.IsFalse(shape2.Deactivated);
        Assert.AreEqual(severity, shape2.Severity);
        Assert.AreEqual(1, shape2.MessagesCount);
        RDFPlainLiteral shape2Message = shape2.Messages[0] as RDFPlainLiteral;
        Assert.IsNotNull(shape2Message);
        Assert.IsTrue(shape2Message.Equals(new RDFPlainLiteral("message", "en-US")));

        //Targets
        Assert.AreEqual(4, shape2.TargetsCount);
        RDFTargetClass shape2TargetClass = shape2.Targets.First(x => x is RDFTargetClass) as RDFTargetClass;
        Assert.IsNotNull(shape2TargetClass);
        Assert.IsTrue(shape2TargetClass.TargetValue.Equals(new RDFResource("ex:Person")));
        RDFTargetNode shape2TargetNode = shape2.Targets.First(x => x is RDFTargetNode) as RDFTargetNode;
        Assert.IsNotNull(shape2TargetNode);
        Assert.IsTrue(shape2TargetNode.TargetValue.Equals(new RDFResource("ex:Alice")));
        RDFTargetSubjectsOf shape2TargetSubjectsOf = shape2.Targets.First(x => x is RDFTargetSubjectsOf) as RDFTargetSubjectsOf;
        Assert.IsNotNull(shape2TargetSubjectsOf);
        Assert.IsTrue(shape2TargetSubjectsOf.TargetValue.Equals(RDFVocabulary.FOAF.KNOWS));
        RDFTargetObjectsOf shape2TargetObjectsOf = shape2.Targets.First(x => x is RDFTargetObjectsOf) as RDFTargetObjectsOf;
        Assert.IsNotNull(shape2TargetObjectsOf);
        Assert.IsTrue(shape2TargetObjectsOf.TargetValue.Equals(RDFVocabulary.FOAF.KNOWS));

        //Constraints
        Assert.AreEqual(30, shape2.ConstraintsCount);
        RDFClassConstraint shape2ClassConstraint = shape2.Constraints.First(x => x is RDFClassConstraint) as RDFClassConstraint;
        Assert.IsNotNull(shape2ClassConstraint);
        Assert.IsTrue(shape2ClassConstraint.ClassType.Equals(new RDFResource("ex:Human")));
        RDFAndConstraint shape2AndConstraint = shape2.Constraints.First(x => x is RDFAndConstraint) as RDFAndConstraint;
        Assert.IsNotNull(shape2AndConstraint);
        Assert.IsTrue(shape2AndConstraint.AndShapes.ContainsKey(shape2.PatternMemberID));
        RDFClosedConstraint shape2ClosedConstraint = shape2.Constraints.First(x => x is RDFClosedConstraint) as RDFClosedConstraint;
        Assert.IsNotNull(shape2ClosedConstraint);
        Assert.IsTrue(shape2ClosedConstraint.Closed);
        Assert.IsTrue(shape2ClosedConstraint.IgnoredProperties.ContainsKey(RDFVocabulary.FOAF.KNOWS.PatternMemberID));
        RDFDatatypeConstraint shape2DatatypeConstraint = shape2.Constraints.First(x => x is RDFDatatypeConstraint) as RDFDatatypeConstraint;
        Assert.IsNotNull(shape2DatatypeConstraint);
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_INTEGER.GetDatatypeFromEnum(), shape2DatatypeConstraint.Datatype.ToString());
        RDFDisjointConstraint shape2DisjointConstraint = shape2.Constraints.First(x => x is RDFDisjointConstraint) as RDFDisjointConstraint;
        Assert.IsNotNull(shape2DisjointConstraint);
        Assert.IsTrue(shape2DisjointConstraint.DisjointPredicate.Equals(RDFVocabulary.FOAF.KNOWS));
        RDFEqualsConstraint shape2EqualsConstraint = shape2.Constraints.First(x => x is RDFEqualsConstraint) as RDFEqualsConstraint;
        Assert.IsNotNull(shape2EqualsConstraint);
        Assert.IsTrue(shape2EqualsConstraint.EqualsPredicate.Equals(RDFVocabulary.FOAF.KNOWS));
        RDFHasValueConstraint shape2HasValueConstraintRes = shape2.Constraints.First(x => x is RDFHasValueConstraint
        {
            Value: RDFResource
        }) as RDFHasValueConstraint;
        Assert.IsNotNull(shape2HasValueConstraintRes);
        Assert.IsTrue(shape2HasValueConstraintRes.Value.Equals(new RDFResource("ex:Alice")));
        RDFHasValueConstraint shape2HasValueConstraintLit = shape2.Constraints.First(x => x is RDFHasValueConstraint
        {
            Value: RDFLiteral
        }) as RDFHasValueConstraint;
        Assert.IsNotNull(shape2HasValueConstraintLit);
        Assert.IsTrue(shape2HasValueConstraintLit.Value.Equals(new RDFPlainLiteral("Alice")));
        RDFInConstraint shape2InConstraintRes = shape2.Constraints.First(x => x is RDFInConstraint { ItemType: RDFModelEnums.RDFItemTypes.Resource }) as RDFInConstraint;
        Assert.IsNotNull(shape2InConstraintRes);
        Assert.IsTrue(shape2InConstraintRes.InValues.ContainsKey(new RDFResource("ex:Alice").PatternMemberID));
        RDFInConstraint shape2InConstraintLit = shape2.Constraints.First(x => x is RDFInConstraint { ItemType: RDFModelEnums.RDFItemTypes.Literal }) as RDFInConstraint;
        Assert.IsNotNull(shape2InConstraintLit);
        Assert.IsTrue(shape2InConstraintLit.InValues.ContainsKey(new RDFPlainLiteral("Alice").PatternMemberID));
        RDFLanguageInConstraint shape2LanguageInConstraint = shape2.Constraints.First(x => x is RDFLanguageInConstraint) as RDFLanguageInConstraint;
        Assert.IsNotNull(shape2LanguageInConstraint);
        Assert.Contains("EN-US", shape2LanguageInConstraint.LanguageTags);
        RDFLessThanConstraint shape2LessThanConstraint = shape2.Constraints.First(x => x is RDFLessThanConstraint) as RDFLessThanConstraint;
        Assert.IsNotNull(shape2LessThanConstraint);
        Assert.IsTrue(shape2LessThanConstraint.LessThanPredicate.Equals(new RDFResource("ex:prop")));
        RDFLessThanOrEqualsConstraint shape2LessThanOrEqualsConstraint = shape2.Constraints.First(x => x is RDFLessThanOrEqualsConstraint) as RDFLessThanOrEqualsConstraint;
        Assert.IsNotNull(shape2LessThanOrEqualsConstraint);
        Assert.IsTrue(shape2LessThanOrEqualsConstraint.LessThanOrEqualsPredicate.Equals(new RDFResource("ex:prop")));
        RDFMaxCountConstraint shape2MaxCountConstraint = shape2.Constraints.First(x => x is RDFMaxCountConstraint) as RDFMaxCountConstraint;
        Assert.IsNotNull(shape2MaxCountConstraint);
        Assert.AreEqual(2, shape2MaxCountConstraint.MaxCount);
        RDFMaxExclusiveConstraint shape2MaxExclusiveConstraint = shape2.Constraints.First(x => x is RDFMaxExclusiveConstraint) as RDFMaxExclusiveConstraint;
        Assert.IsNotNull(shape2MaxExclusiveConstraint);
        Assert.IsTrue(shape2MaxExclusiveConstraint.Value.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFMaxInclusiveConstraint shape2MaxInclusiveConstraint = shape2.Constraints.First(x => x is RDFMaxInclusiveConstraint) as RDFMaxInclusiveConstraint;
        Assert.IsNotNull(shape2MaxInclusiveConstraint);
        Assert.IsTrue(shape2MaxInclusiveConstraint.Value.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFMaxLengthConstraint shape2MaxLengthConstraint = shape2.Constraints.First(x => x is RDFMaxLengthConstraint) as RDFMaxLengthConstraint;
        Assert.IsNotNull(shape2MaxLengthConstraint);
        Assert.AreEqual(2, shape2MaxLengthConstraint.MaxLength);
        RDFMinCountConstraint shape2MinCountConstraint = shape2.Constraints.First(x => x is RDFMinCountConstraint) as RDFMinCountConstraint;
        Assert.IsNotNull(shape2MinCountConstraint);
        Assert.AreEqual(2, shape2MinCountConstraint.MinCount);
        RDFMinExclusiveConstraint shape2MinExclusiveConstraint = shape2.Constraints.First(x => x is RDFMinExclusiveConstraint) as RDFMinExclusiveConstraint;
        Assert.IsNotNull(shape2MinExclusiveConstraint);
        Assert.IsTrue(shape2MinExclusiveConstraint.Value.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFMinInclusiveConstraint shape2MinInclusiveConstraint = shape2.Constraints.First(x => x is RDFMinInclusiveConstraint) as RDFMinInclusiveConstraint;
        Assert.IsNotNull(shape2MinInclusiveConstraint);
        Assert.IsTrue(shape2MinInclusiveConstraint.Value.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFMinLengthConstraint shape2MinLengthConstraint = shape2.Constraints.First(x => x is RDFMinLengthConstraint) as RDFMinLengthConstraint;
        Assert.IsNotNull(shape2MinLengthConstraint);
        Assert.AreEqual(2, shape2MinLengthConstraint.MinLength);
        RDFNodeConstraint shape2NodeConstraint = shape2.Constraints.First(x => x is RDFNodeConstraint) as RDFNodeConstraint;
        Assert.IsNotNull(shape2NodeConstraint);
        Assert.IsTrue(shape2NodeConstraint.NodeShapeUri.Equals(new RDFResource("ex:nodeShape")));
        RDFNodeKindConstraint shape2NodeKindConstraint = shape2.Constraints.First(x => x is RDFNodeKindConstraint) as RDFNodeKindConstraint;
        Assert.IsNotNull(shape2NodeKindConstraint);
        Assert.AreEqual(nodeKind, shape2NodeKindConstraint.NodeKind);
        RDFNotConstraint shape2NotConstraint = shape2.Constraints.First(x => x is RDFNotConstraint) as RDFNotConstraint;
        Assert.IsNotNull(shape2NotConstraint);
        Assert.IsTrue(shape2NotConstraint.NotShape.Equals(new RDFResource("ex:notShape")));
        RDFOrConstraint shape2OrConstraint = shape2.Constraints.First(x => x is RDFOrConstraint) as RDFOrConstraint;
        Assert.IsNotNull(shape2OrConstraint);
        Assert.IsTrue(shape2OrConstraint.OrShapes.ContainsKey(shape2.PatternMemberID));
        RDFPatternConstraint shape2PatternConstraint = shape2.Constraints.First(x => x is RDFPatternConstraint) as RDFPatternConstraint;
        Assert.IsNotNull(shape2PatternConstraint);
        Assert.IsTrue(shape2PatternConstraint.RegEx.ToString().Equals("^hello$", System.StringComparison.Ordinal));
        Assert.IsTrue(shape2PatternConstraint.RegEx.Options.HasFlag(RegexOptions.IgnoreCase));
        Assert.IsTrue(shape2PatternConstraint.RegEx.Options.HasFlag(RegexOptions.Singleline));
        Assert.IsTrue(shape2PatternConstraint.RegEx.Options.HasFlag(RegexOptions.Multiline));
        Assert.IsTrue(shape2PatternConstraint.RegEx.Options.HasFlag(RegexOptions.IgnorePatternWhitespace));
        RDFPropertyConstraint shape2PropertyConstraint = shape2.Constraints.First(x => x is RDFPropertyConstraint) as RDFPropertyConstraint;
        Assert.IsNotNull(shape2PropertyConstraint);
        Assert.IsTrue(shape2PropertyConstraint.PropertyShapeUri.Equals(new RDFResource("ex:propertyShape")));
        RDFQualifiedValueShapeConstraint shape2QualifiedValueShapeConstraint = shape2.Constraints.First(x => x is RDFQualifiedValueShapeConstraint) as RDFQualifiedValueShapeConstraint;
        Assert.IsNotNull(shape2QualifiedValueShapeConstraint);
        Assert.IsTrue(shape2QualifiedValueShapeConstraint.QualifiedValueShapeUri.Equals(new RDFResource("ex:qvShape")));
        Assert.AreEqual(2, shape2QualifiedValueShapeConstraint.QualifiedValueMinCount);
        Assert.AreEqual(3, shape2QualifiedValueShapeConstraint.QualifiedValueMaxCount);
        RDFUniqueLangConstraint shape2UniqueLangConstraint = shape2.Constraints.First(x => x is RDFUniqueLangConstraint) as RDFUniqueLangConstraint;
        Assert.IsNotNull(shape2UniqueLangConstraint);
        Assert.IsTrue(shape2UniqueLangConstraint.UniqueLang);
        RDFXoneConstraint shape2XoneConstraint = shape2.Constraints.First(x => x is RDFXoneConstraint) as RDFXoneConstraint;
        Assert.IsNotNull(shape2XoneConstraint);
        Assert.IsTrue(shape2XoneConstraint.XoneShapes.ContainsKey(shape2.PatternMemberID));
        #endregion
    }

    [TestMethod]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation, RDFValidationEnums.RDFNodeKinds.BlankNode)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning, RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Info, RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation, RDFValidationEnums.RDFNodeKinds.IRI)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning, RDFValidationEnums.RDFNodeKinds.IRIOrLiteral)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Info, RDFValidationEnums.RDFNodeKinds.Literal)]
    public void ShouldParsePropertyShapeFromGraph(RDFValidationEnums.RDFShapeSeverity severity, RDFValidationEnums.RDFNodeKinds nodeKind)
    {
        RDFPropertyShape shape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.KNOWS);

        //Attributes
        shape.SetSeverity(severity);
        shape.AddMessage(new RDFPlainLiteral("message", "en-US"));

        //NonValidating
        shape.AddDescription(new RDFPlainLiteral("description", "en-US"));
        shape.AddName(new RDFPlainLiteral("name", "en-US"));
        shape.SetOrder(1);
        shape.SetGroup(new RDFResource("ex:shapeGroup"));

        //Targets
        shape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        shape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        shape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
        shape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));

        //Constraints
        shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Human")));
        shape.AddConstraint(new RDFAndConstraint().AddShape(shape));
        shape.AddConstraint(new RDFClosedConstraint(true).AddIgnoredProperty(RDFVocabulary.FOAF.KNOWS));
        shape.AddConstraint(new RDFDatatypeConstraint(RDFDatatypeRegister.GetDatatype(RDFModelEnums.RDFDatatypes.XSD_INTEGER.GetDatatypeFromEnum())));
        shape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.KNOWS));
        shape.AddConstraint(new RDFEqualsConstraint(RDFVocabulary.FOAF.KNOWS));
        shape.AddConstraint(new RDFHasValueConstraint(new RDFResource("ex:Alice")));
        shape.AddConstraint(new RDFHasValueConstraint(new RDFPlainLiteral("Alice")));
        shape.AddConstraint(new RDFInConstraint(RDFModelEnums.RDFItemTypes.Resource).AddValue(new RDFResource("ex:Alice")));
        shape.AddConstraint(new RDFInConstraint(RDFModelEnums.RDFItemTypes.Literal).AddValue(new RDFPlainLiteral("Alice")));
        shape.AddConstraint(new RDFLanguageInConstraint(["en-US"]));
        shape.AddConstraint(new RDFLessThanConstraint(new RDFResource("ex:prop")));
        shape.AddConstraint(new RDFLessThanOrEqualsConstraint(new RDFResource("ex:prop")));
        shape.AddConstraint(new RDFMaxCountConstraint(2));
        shape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        shape.AddConstraint(new RDFMaxInclusiveConstraint(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        shape.AddConstraint(new RDFMaxLengthConstraint(2));
        shape.AddConstraint(new RDFMinCountConstraint(2));
        shape.AddConstraint(new RDFMinExclusiveConstraint(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        shape.AddConstraint(new RDFMinInclusiveConstraint(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        shape.AddConstraint(new RDFMinLengthConstraint(2));
        shape.AddConstraint(new RDFNodeConstraint(new RDFResource("ex:nodeShape")));
        shape.AddConstraint(new RDFNodeKindConstraint(nodeKind));
        shape.AddConstraint(new RDFNotConstraint(new RDFResource("ex:notShape")));
        shape.AddConstraint(new RDFOrConstraint().AddShape(shape));
        shape.AddConstraint(new RDFPatternConstraint(new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace)));
        shape.AddConstraint(new RDFPropertyConstraint(new RDFResource("ex:propertyShape")));
        shape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:qvShape"), 2, 3));
        shape.AddConstraint(new RDFUniqueLangConstraint(true));
        shape.AddConstraint(new RDFXoneConstraint().AddShape(shape));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
        shapesGraph.AddShape(shape);
        RDFGraph graph = shapesGraph.ToRDFGraph();
        RDFShapesGraph shapesGraph2 = RDFValidationHelper.FromRDFGraph(graph);

        #region Asserts
        Assert.IsNotNull(shapesGraph2);
        Assert.AreEqual(1, shapesGraph2.ShapesCount);
        Assert.IsTrue(shapesGraph2.Equals(new RDFResource("ex:shapesGraph")));
        RDFPropertyShape shape2 = shapesGraph2.SelectShape("ex:propertyShape") as RDFPropertyShape;
        Assert.IsNotNull(shape2);

        //Attributes
        Assert.IsFalse(shape2.Deactivated);
        Assert.AreEqual(severity, shape2.Severity);
        Assert.AreEqual(1, shape2.MessagesCount);
        RDFPlainLiteral shape2Message = shape2.Messages[0] as RDFPlainLiteral;
        Assert.IsNotNull(shape2Message);
        Assert.IsTrue(shape2Message.Equals(new RDFPlainLiteral("message", "en-US")));

        //NonValidating
        Assert.HasCount(1, shape2.Descriptions);
        Assert.IsTrue(shape2.Descriptions[0].Equals(new RDFPlainLiteral("description", "en-US")));
        Assert.HasCount(1, shape2.Names);
        Assert.IsTrue(shape2.Names[0].Equals(new RDFPlainLiteral("name", "en-US")));
        Assert.IsNotNull(shape2.Order);
        Assert.IsTrue(shape2.Order.Equals(RDFTypedLiteral.One));
        Assert.IsNotNull(shape2.Group);
        Assert.IsTrue(shape2.Group.Equals(new RDFResource("ex:shapeGroup")));

        //Targets
        Assert.AreEqual(4, shape2.TargetsCount);
        RDFTargetClass shape2TargetClass = shape2.Targets.First(x => x is RDFTargetClass) as RDFTargetClass;
        Assert.IsNotNull(shape2TargetClass);
        Assert.IsTrue(shape2TargetClass.TargetValue.Equals(new RDFResource("ex:Person")));
        RDFTargetNode shape2TargetNode = shape2.Targets.First(x => x is RDFTargetNode) as RDFTargetNode;
        Assert.IsNotNull(shape2TargetNode);
        Assert.IsTrue(shape2TargetNode.TargetValue.Equals(new RDFResource("ex:Alice")));
        RDFTargetSubjectsOf shape2TargetSubjectsOf = shape2.Targets.First(x => x is RDFTargetSubjectsOf) as RDFTargetSubjectsOf;
        Assert.IsNotNull(shape2TargetSubjectsOf);
        Assert.IsTrue(shape2TargetSubjectsOf.TargetValue.Equals(RDFVocabulary.FOAF.KNOWS));
        RDFTargetObjectsOf shape2TargetObjectsOf = shape2.Targets.First(x => x is RDFTargetObjectsOf) as RDFTargetObjectsOf;
        Assert.IsNotNull(shape2TargetObjectsOf);
        Assert.IsTrue(shape2TargetObjectsOf.TargetValue.Equals(RDFVocabulary.FOAF.KNOWS));

        //Constraints
        Assert.AreEqual(30, shape2.ConstraintsCount);
        RDFClassConstraint shape2ClassConstraint = shape2.Constraints.First(x => x is RDFClassConstraint) as RDFClassConstraint;
        Assert.IsNotNull(shape2ClassConstraint);
        Assert.IsTrue(shape2ClassConstraint.ClassType.Equals(new RDFResource("ex:Human")));
        RDFAndConstraint shape2AndConstraint = shape2.Constraints.First(x => x is RDFAndConstraint) as RDFAndConstraint;
        Assert.IsNotNull(shape2AndConstraint);
        Assert.IsTrue(shape2AndConstraint.AndShapes.ContainsKey(shape2.PatternMemberID));
        RDFClosedConstraint shape2ClosedConstraint = shape2.Constraints.First(x => x is RDFClosedConstraint) as RDFClosedConstraint;
        Assert.IsNotNull(shape2ClosedConstraint);
        Assert.IsTrue(shape2ClosedConstraint.Closed);
        Assert.IsTrue(shape2ClosedConstraint.IgnoredProperties.ContainsKey(RDFVocabulary.FOAF.KNOWS.PatternMemberID));
        RDFDatatypeConstraint shape2DatatypeConstraint = shape2.Constraints.First(x => x is RDFDatatypeConstraint) as RDFDatatypeConstraint;
        Assert.IsNotNull(shape2DatatypeConstraint);
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_INTEGER.GetDatatypeFromEnum(), shape2DatatypeConstraint.Datatype.ToString());
        RDFDisjointConstraint shape2DisjointConstraint = shape2.Constraints.First(x => x is RDFDisjointConstraint) as RDFDisjointConstraint;
        Assert.IsNotNull(shape2DisjointConstraint);
        Assert.IsTrue(shape2DisjointConstraint.DisjointPredicate.Equals(RDFVocabulary.FOAF.KNOWS));
        RDFEqualsConstraint shape2EqualsConstraint = shape2.Constraints.First(x => x is RDFEqualsConstraint) as RDFEqualsConstraint;
        Assert.IsNotNull(shape2EqualsConstraint);
        Assert.IsTrue(shape2EqualsConstraint.EqualsPredicate.Equals(RDFVocabulary.FOAF.KNOWS));
        RDFHasValueConstraint shape2HasValueConstraintRes = shape2.Constraints.First(x => x is RDFHasValueConstraint
        {
            Value: RDFResource
        }) as RDFHasValueConstraint;
        Assert.IsNotNull(shape2HasValueConstraintRes);
        Assert.IsTrue(shape2HasValueConstraintRes.Value.Equals(new RDFResource("ex:Alice")));
        RDFHasValueConstraint shape2HasValueConstraintLit = shape2.Constraints.First(x => x is RDFHasValueConstraint
        {
            Value: RDFLiteral
        }) as RDFHasValueConstraint;
        Assert.IsNotNull(shape2HasValueConstraintLit);
        Assert.IsTrue(shape2HasValueConstraintLit.Value.Equals(new RDFPlainLiteral("Alice")));
        RDFInConstraint shape2InConstraintRes = shape2.Constraints.First(x => x is RDFInConstraint { ItemType: RDFModelEnums.RDFItemTypes.Resource }) as RDFInConstraint;
        Assert.IsNotNull(shape2InConstraintRes);
        Assert.IsTrue(shape2InConstraintRes.InValues.ContainsKey(new RDFResource("ex:Alice").PatternMemberID));
        RDFInConstraint shape2InConstraintLit = shape2.Constraints.First(x => x is RDFInConstraint { ItemType: RDFModelEnums.RDFItemTypes.Literal }) as RDFInConstraint;
        Assert.IsNotNull(shape2InConstraintLit);
        Assert.IsTrue(shape2InConstraintLit.InValues.ContainsKey(new RDFPlainLiteral("Alice").PatternMemberID));
        RDFLanguageInConstraint shape2LanguageInConstraint = shape2.Constraints.First(x => x is RDFLanguageInConstraint) as RDFLanguageInConstraint;
        Assert.IsNotNull(shape2LanguageInConstraint);
        Assert.Contains("EN-US", shape2LanguageInConstraint.LanguageTags);
        RDFLessThanConstraint shape2LessThanConstraint = shape2.Constraints.First(x => x is RDFLessThanConstraint) as RDFLessThanConstraint;
        Assert.IsNotNull(shape2LessThanConstraint);
        Assert.IsTrue(shape2LessThanConstraint.LessThanPredicate.Equals(new RDFResource("ex:prop")));
        RDFLessThanOrEqualsConstraint shape2LessThanOrEqualsConstraint = shape2.Constraints.First(x => x is RDFLessThanOrEqualsConstraint) as RDFLessThanOrEqualsConstraint;
        Assert.IsNotNull(shape2LessThanOrEqualsConstraint);
        Assert.IsTrue(shape2LessThanOrEqualsConstraint.LessThanOrEqualsPredicate.Equals(new RDFResource("ex:prop")));
        RDFMaxCountConstraint shape2MaxCountConstraint = shape2.Constraints.First(x => x is RDFMaxCountConstraint) as RDFMaxCountConstraint;
        Assert.IsNotNull(shape2MaxCountConstraint);
        Assert.AreEqual(2, shape2MaxCountConstraint.MaxCount);
        RDFMaxExclusiveConstraint shape2MaxExclusiveConstraint = shape2.Constraints.First(x => x is RDFMaxExclusiveConstraint) as RDFMaxExclusiveConstraint;
        Assert.IsNotNull(shape2MaxExclusiveConstraint);
        Assert.IsTrue(shape2MaxExclusiveConstraint.Value.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFMaxInclusiveConstraint shape2MaxInclusiveConstraint = shape2.Constraints.First(x => x is RDFMaxInclusiveConstraint) as RDFMaxInclusiveConstraint;
        Assert.IsNotNull(shape2MaxInclusiveConstraint);
        Assert.IsTrue(shape2MaxInclusiveConstraint.Value.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFMaxLengthConstraint shape2MaxLengthConstraint = shape2.Constraints.First(x => x is RDFMaxLengthConstraint) as RDFMaxLengthConstraint;
        Assert.IsNotNull(shape2MaxLengthConstraint);
        Assert.AreEqual(2, shape2MaxLengthConstraint.MaxLength);
        RDFMinCountConstraint shape2MinCountConstraint = shape2.Constraints.First(x => x is RDFMinCountConstraint) as RDFMinCountConstraint;
        Assert.IsNotNull(shape2MinCountConstraint);
        Assert.AreEqual(2, shape2MinCountConstraint.MinCount);
        RDFMinExclusiveConstraint shape2MinExclusiveConstraint = shape2.Constraints.First(x => x is RDFMinExclusiveConstraint) as RDFMinExclusiveConstraint;
        Assert.IsNotNull(shape2MinExclusiveConstraint);
        Assert.IsTrue(shape2MinExclusiveConstraint.Value.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFMinInclusiveConstraint shape2MinInclusiveConstraint = shape2.Constraints.First(x => x is RDFMinInclusiveConstraint) as RDFMinInclusiveConstraint;
        Assert.IsNotNull(shape2MinInclusiveConstraint);
        Assert.IsTrue(shape2MinInclusiveConstraint.Value.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFMinLengthConstraint shape2MinLengthConstraint = shape2.Constraints.First(x => x is RDFMinLengthConstraint) as RDFMinLengthConstraint;
        Assert.IsNotNull(shape2MinLengthConstraint);
        Assert.AreEqual(2, shape2MinLengthConstraint.MinLength);
        RDFNodeConstraint shape2NodeConstraint = shape2.Constraints.First(x => x is RDFNodeConstraint) as RDFNodeConstraint;
        Assert.IsNotNull(shape2NodeConstraint);
        Assert.IsTrue(shape2NodeConstraint.NodeShapeUri.Equals(new RDFResource("ex:nodeShape")));
        RDFNodeKindConstraint shape2NodeKindConstraint = shape2.Constraints.First(x => x is RDFNodeKindConstraint) as RDFNodeKindConstraint;
        Assert.IsNotNull(shape2NodeKindConstraint);
        Assert.AreEqual(nodeKind, shape2NodeKindConstraint.NodeKind);
        RDFNotConstraint shape2NotConstraint = shape2.Constraints.First(x => x is RDFNotConstraint) as RDFNotConstraint;
        Assert.IsNotNull(shape2NotConstraint);
        Assert.IsTrue(shape2NotConstraint.NotShape.Equals(new RDFResource("ex:notShape")));
        RDFOrConstraint shape2OrConstraint = shape2.Constraints.First(x => x is RDFOrConstraint) as RDFOrConstraint;
        Assert.IsNotNull(shape2OrConstraint);
        Assert.IsTrue(shape2OrConstraint.OrShapes.ContainsKey(shape2.PatternMemberID));
        RDFPatternConstraint shape2PatternConstraint = shape2.Constraints.First(x => x is RDFPatternConstraint) as RDFPatternConstraint;
        Assert.IsNotNull(shape2PatternConstraint);
        Assert.IsTrue(shape2PatternConstraint.RegEx.ToString().Equals("^hello$", System.StringComparison.Ordinal));
        Assert.IsTrue(shape2PatternConstraint.RegEx.Options.HasFlag(RegexOptions.IgnoreCase));
        Assert.IsTrue(shape2PatternConstraint.RegEx.Options.HasFlag(RegexOptions.Singleline));
        Assert.IsTrue(shape2PatternConstraint.RegEx.Options.HasFlag(RegexOptions.Multiline));
        Assert.IsTrue(shape2PatternConstraint.RegEx.Options.HasFlag(RegexOptions.IgnorePatternWhitespace));
        RDFPropertyConstraint shape2PropertyConstraint = shape2.Constraints.First(x => x is RDFPropertyConstraint) as RDFPropertyConstraint;
        Assert.IsNotNull(shape2PropertyConstraint);
        Assert.IsTrue(shape2PropertyConstraint.PropertyShapeUri.Equals(new RDFResource("ex:propertyShape")));
        RDFQualifiedValueShapeConstraint shape2QualifiedValueShapeConstraint = shape2.Constraints.First(x => x is RDFQualifiedValueShapeConstraint) as RDFQualifiedValueShapeConstraint;
        Assert.IsNotNull(shape2QualifiedValueShapeConstraint);
        Assert.IsTrue(shape2QualifiedValueShapeConstraint.QualifiedValueShapeUri.Equals(new RDFResource("ex:qvShape")));
        Assert.AreEqual(2, shape2QualifiedValueShapeConstraint.QualifiedValueMinCount);
        Assert.AreEqual(3, shape2QualifiedValueShapeConstraint.QualifiedValueMaxCount);
        RDFUniqueLangConstraint shape2UniqueLangConstraint = shape2.Constraints.First(x => x is RDFUniqueLangConstraint) as RDFUniqueLangConstraint;
        Assert.IsNotNull(shape2UniqueLangConstraint);
        Assert.IsTrue(shape2UniqueLangConstraint.UniqueLang);
        RDFXoneConstraint shape2XoneConstraint = shape2.Constraints.First(x => x is RDFXoneConstraint) as RDFXoneConstraint;
        Assert.IsNotNull(shape2XoneConstraint);
        Assert.IsTrue(shape2XoneConstraint.XoneShapes.ContainsKey(shape2.PatternMemberID));
        #endregion
    }

    [TestMethod]
    public void ShouldDetectInlinePropertyShapes()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:propertyShape"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:propertyShape"), RDFVocabulary.SHACL.PATH, RDFVocabulary.FOAF.KNOWS));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:propertyShape"), RDFVocabulary.SHACL.PROPERTY, new RDFResource("bnode:inlinePropertyShape")));
        graph.AddTriple(new RDFTriple(new RDFResource("bnode:inlinePropertyShape"), RDFVocabulary.SHACL.PATH, RDFVocabulary.FOAF.AGE));
        graph.AddTriple(new RDFTriple(new RDFResource("bnode:inlinePropertyShape"), RDFVocabulary.SHACL.CLASS, new RDFResource("ex:Class")));
        RDFShapesGraph shapesGraph = RDFValidationHelper.FromRDFGraph(graph);

        Assert.IsNotNull(shapesGraph);
        Assert.AreEqual(2, shapesGraph.ShapesCount);

        RDFPropertyShape propertyShape = shapesGraph.SelectShape("ex:propertyShape") as RDFPropertyShape;
        Assert.IsNotNull(propertyShape);
        Assert.IsTrue(propertyShape.Path.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.AreEqual(1, propertyShape.ConstraintsCount);
        Assert.IsTrue(propertyShape.Constraints[0] is RDFPropertyConstraint propertyShapeConstraint
                      && propertyShapeConstraint.PropertyShapeUri.Equals(new RDFResource("bnode:inlinePropertyShape")));

        RDFPropertyShape inlinePropertyShape = shapesGraph.SelectShape("bnode:inlinePropertyShape") as RDFPropertyShape;
        Assert.IsNotNull(inlinePropertyShape);
        Assert.IsTrue(inlinePropertyShape.Path.Equals(RDFVocabulary.FOAF.AGE));
        Assert.AreEqual(1, inlinePropertyShape.ConstraintsCount);
        Assert.IsTrue(inlinePropertyShape.Constraints[0] is RDFClassConstraint classConstraint
                      && classConstraint.ClassType.Equals(new RDFResource("ex:Class")));
    }
    #endregion
}