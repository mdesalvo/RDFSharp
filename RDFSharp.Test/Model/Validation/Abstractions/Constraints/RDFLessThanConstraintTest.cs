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
using System;
using System.Linq;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFLessThanConstraintTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateLessThanConstraint()
    {
        RDFLessThanConstraint lessThanConstraint = new RDFLessThanConstraint(new RDFResource("ex:prop"));

        Assert.IsNotNull(lessThanConstraint);
        Assert.IsTrue(lessThanConstraint.LessThanPredicate.Equals(new RDFResource("ex:prop")));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLessThanConstraint()
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFLessThanConstraint(null));

    [TestMethod]
    public void ShouldExportLessThanConstraint()
    {
        RDFLessThanConstraint lessThanConstraint = new RDFLessThanConstraint(new RDFResource("ex:prop"));
        RDFGraph graph = lessThanConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                    && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.LESS_THAN.PatternMemberID)
                                                    && t.Value.ObjectID.Equals(new RDFResource("ex:prop").PatternMemberID)));
    }

    //NS-CONFORMS:TRUE

    [TestMethod]
    public void ShouldConformNodeShapeWithClassTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        nodeShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.KNOWS));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformNodeShapeWithNodeTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        nodeShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.KNOWS));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformNodeShapeWithSubjectsOfTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
        nodeShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.KNOWS));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformNodeShapeWithObjectsOfTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.KNOWS));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    //PS-CONFORMS:TRUE

    [TestMethod]
    public void ShouldConformPropertyShapeWithClassTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.AGENT));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformPropertyShapeWithNodeTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        propertyShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.AGENT));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformPropertyShapeWithSubjectsOfTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.INTEREST, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.INTEREST);
        propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.AGENT));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformPropertyShapeWithObjectsOfTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.INTEREST, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.INTEREST);
        propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.AGENT));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    //NS-CONFORMS:FALSE

    [TestMethod]
    public void ShouldNotConformNodeShapeWithClassTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        nodeShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:NodeShape")));
    }

    [TestMethod]
    public void ShouldNotConformNodeShapeWithNodeTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Bob")));
        nodeShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:NodeShape")));
    }

    [TestMethod]
    public void ShouldNotConformNodeShapeWithSubjectsOfTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
        nodeShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:NodeShape")));
    }

    [TestMethod]
    public void ShouldNotConformNodeShapeWithObjectsOfTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:NodeShape")));
    }

    //PS-CONFORMS:FALSE

    [TestMethod]
    public void ShouldNotConformPropertyShapeWithClassTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.AGENT));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must have values less than values of property <{RDFVocabulary.FOAF.AGENT}>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Steve")));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
    }

    [TestMethod]
    public void ShouldNotConformPropertyShapeWithNodeTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Bob")));
        propertyShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.AGENT));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must have values less than values of property <{RDFVocabulary.FOAF.AGENT}>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Steve")));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
    }

    [TestMethod]
    public void ShouldNotConformPropertyShapeWithSubjectsOfTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
        propertyShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.AGENT));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must have values less than values of property <{RDFVocabulary.FOAF.AGENT}>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Steve")));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
    }

    [TestMethod]
    public void ShouldNotConformPropertyShapeWithObjectsOfTarget()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFLessThanConstraint(RDFVocabulary.FOAF.AGENT));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must have values less than values of property <{RDFVocabulary.FOAF.AGENT}>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Steve")));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
    }

    //MIXED-CONFORMS:TRUE

    [TestMethod]
    public void ShouldConformNodeShapeWithPropertyShape()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Event1"), new RDFResource("ex:startDate"), new RDFTypedLiteral("1990-12-12", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Event1"), new RDFResource("ex:endDate"), new RDFTypedLiteral("1999-10-12", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Event2"), new RDFResource("ex:startDate"), new RDFTypedLiteral("1992-01-04", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Event2"), new RDFResource("ex:endDate"), new RDFTypedLiteral("1998-10-04", RDFModelEnums.RDFDatatypes.XSD_DATE)));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Event1")));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Event2")));
        RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:startDate"));
        propShape.AddConstraint(new RDFLessThanConstraint(new RDFResource("ex:endDate")));
        nodeShape.AddConstraint(new RDFPropertyConstraint(propShape));
        shapesGraph.AddShape(nodeShape);
        shapesGraph.AddShape(propShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    //MIXED-CONFORMS:FALSE

    [TestMethod]
    public void ShouldNotConformNodeShapeWithPropertyShape()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Event1"), new RDFResource("ex:startDate"), new RDFTypedLiteral("1990-12-12", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Event1"), new RDFResource("ex:endDate"), new RDFTypedLiteral("1999-10-12", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Event2"), new RDFResource("ex:startDate"), new RDFTypedLiteral("1992-01-04", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Event2"), new RDFResource("ex:endDate"), new RDFTypedLiteral("1989-10-07", RDFModelEnums.RDFDatatypes.XSD_DATE)));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Event1")));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Event2")));
        RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:startDate"));
        propShape.AddConstraint(new RDFLessThanConstraint(new RDFResource("ex:endDate")));
        nodeShape.AddConstraint(new RDFPropertyConstraint(propShape));
        shapesGraph.AddShape(nodeShape);
        shapesGraph.AddShape(propShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values less than values of property <ex:endDate>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Event2")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFTypedLiteral("1992-01-04", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(new RDFResource("ex:startDate")));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropShape")));
    }
    #endregion
}