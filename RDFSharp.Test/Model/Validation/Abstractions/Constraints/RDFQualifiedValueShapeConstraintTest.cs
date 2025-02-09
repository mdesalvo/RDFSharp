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
        Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                    && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE.PatternMemberID)
                                                    && t.Value.ObjectID.Equals(new RDFResource("ex:QVShape").PatternMemberID)));
        Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                    && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT.PatternMemberID)
                                                    && t.Value.ObjectID.Equals(RDFTypedLiteral.One.PatternMemberID)));
        Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                    && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT.PatternMemberID)
                                                    && t.Value.ObjectID.Equals(new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER).PatternMemberID)));
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
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(6));
        shapesGraph.AddShape(qvShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformNodeShapeWithClassTargetWhenQVShapeNotFound()
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
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(6));

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
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(6));
        shapesGraph.AddShape(qvShape);

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
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(6));
        shapesGraph.AddShape(qvShape);

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
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(6));
        shapesGraph.AddShape(qvShape);

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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Larry")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShapeConstraint"), 1, 2));
        shapesGraph.AddShape(propertyShape);
        RDFPropertyShape qvShapeConstraint = new RDFPropertyShape(new RDFResource("ex:QVShapeConstraint"), RDFVocabulary.FOAF.KNOWS);
        qvShapeConstraint.AddConstraint(new RDFMinLengthConstraint(6));
        shapesGraph.AddShape(qvShapeConstraint);

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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        propertyShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShapeConstraint"), 1, 2));
        shapesGraph.AddShape(propertyShape);
        RDFPropertyShape qvShapeConstraint = new RDFPropertyShape(new RDFResource("ex:QVShapeConstraint"), RDFVocabulary.FOAF.KNOWS);
        qvShapeConstraint.AddConstraint(new RDFMinLengthConstraint(6));
        shapesGraph.AddShape(qvShapeConstraint);

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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
        propertyShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShapeConstraint"), null, 2));
        shapesGraph.AddShape(propertyShape);
        RDFPropertyShape qvShapeConstraint = new RDFPropertyShape(new RDFResource("ex:QVShapeConstraint"), RDFVocabulary.FOAF.KNOWS);
        qvShapeConstraint.AddConstraint(new RDFMinLengthConstraint(6));
        shapesGraph.AddShape(qvShapeConstraint);

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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShapeConstraint"), 1, null));
        shapesGraph.AddShape(propertyShape);
        RDFPropertyShape qvShapeConstraint = new RDFPropertyShape(new RDFResource("ex:QVShapeConstraint"), RDFVocabulary.RDF.TYPE);
        qvShapeConstraint.AddConstraint(new RDFMinLengthConstraint(5));
        shapesGraph.AddShape(qvShapeConstraint);

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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(7));
        shapesGraph.AddShape(qvShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(7));
        shapesGraph.AddShape(qvShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(7));
        shapesGraph.AddShape(qvShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShape"), 1, 2));
        nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
        shapesGraph.AddShape(nodeShape);
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QVShape"), RDFVocabulary.FOAF.KNOWS);
        qvShape.AddConstraint(new RDFMinLengthConstraint(9));
        shapesGraph.AddShape(qvShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Larry")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Harry")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShapeConstraint"), null, 2));
        shapesGraph.AddShape(propertyShape);
        RDFPropertyShape qvShapeConstraint = new RDFPropertyShape(new RDFResource("ex:QVShapeConstraint"), RDFVocabulary.FOAF.KNOWS);
        qvShapeConstraint.AddConstraint(new RDFMinLengthConstraint(9));
        shapesGraph.AddShape(qvShapeConstraint);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have a maximum of 2 conforming values for the shape <ex:QVShapeConstraint>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Larry")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Harry")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        propertyShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShapeConstraint"), 4, null));
        shapesGraph.AddShape(propertyShape);
        RDFPropertyShape qvShapeConstraint = new RDFPropertyShape(new RDFResource("ex:QVShapeConstraint"), RDFVocabulary.FOAF.KNOWS);
        qvShapeConstraint.AddConstraint(new RDFMinLengthConstraint(9));
        shapesGraph.AddShape(qvShapeConstraint);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have a minimum of 4 conforming values for the shape <ex:QVShapeConstraint>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Larry")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
        propertyShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShapeConstraint"), 1, 1));
        shapesGraph.AddShape(propertyShape);
        RDFPropertyShape qvShapeConstraint = new RDFPropertyShape(new RDFResource("ex:QVShapeConstraint"), RDFVocabulary.FOAF.KNOWS);
        qvShapeConstraint.AddConstraint(new RDFMinLengthConstraint(5));
        shapesGraph.AddShape(qvShapeConstraint);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have a minimum of 1 and a maximum of 1 conforming values for the shape <ex:QVShapeConstraint>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Larry")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QVShapeConstraint"), 1, null));
        shapesGraph.AddShape(propertyShape);
        RDFPropertyShape qvShapeConstraint = new RDFPropertyShape(new RDFResource("ex:QVShapeConstraint"), RDFVocabulary.FOAF.KNOWS);
        qvShapeConstraint.AddConstraint(new RDFMinLengthConstraint(5));
        shapesGraph.AddShape(qvShapeConstraint);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results.Count == 2);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have a minimum of 1 conforming values for the shape <ex:QVShapeConstraint>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Steve")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        Assert.IsTrue(validationReport.Results[1].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[1].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[1].ResultMessages[0].Equals(new RDFPlainLiteral("Must have a minimum of 1 conforming values for the shape <ex:QVShapeConstraint>")));
        Assert.IsTrue(validationReport.Results[1].FocusNode.Equals(new RDFResource("ex:Larry")));
        Assert.IsNull(validationReport.Results[1].ResultValue);
        Assert.IsTrue(validationReport.Results[1].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[1].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[1].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
    }

    //MIXED-CONFORMS:TRUE

    [TestMethod]
    public void ShouldConformNodeShapeWithPropertyShape()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Brittany"), new RDFResource("ex:parent"), new RDFResource("ex:John")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Brittany"), new RDFResource("ex:parent"), new RDFResource("ex:Jane")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:John"), new RDFResource("ex:gender"), new RDFResource("ex:male")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Jane"), new RDFResource("ex:gender"), new RDFResource("ex:female")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:QualifiedValueShapeExampleShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Brittany")));
        nodeShape.AddConstraint(new RDFPropertyConstraint(new RDFResource("ex:PropShape")));
        RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:parent"));
        propShape.AddConstraint(new RDFMinCountConstraint(2));
        propShape.AddConstraint(new RDFMaxCountConstraint(2));
        propShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QualifiedValueShapeConstraint"), 1, null));
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QualifiedValueShapeConstraint"), new RDFResource("ex:gender"));
        qvShape.AddConstraint(new RDFHasValueConstraint(new RDFResource("ex:female")));
        shapesGraph.AddShape(nodeShape);
        shapesGraph.AddShape(propShape);
        shapesGraph.AddShape(qvShape);

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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Brittany"), new RDFResource("ex:parent"), new RDFResource("ex:John")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Brittany"), new RDFResource("ex:parent"), new RDFResource("ex:Jane")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:John"), new RDFResource("ex:gender"), new RDFResource("ex:male")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Jane"), new RDFResource("ex:gender"), new RDFResource("ex:female")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:QualifiedValueShapeExampleShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Brittany")));
        nodeShape.AddConstraint(new RDFPropertyConstraint(new RDFResource("ex:PropShape")));
        RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:parent"));
        propShape.AddConstraint(new RDFMinCountConstraint(2));
        propShape.AddConstraint(new RDFMaxCountConstraint(2));
        propShape.AddConstraint(new RDFQualifiedValueShapeConstraint(new RDFResource("ex:QualifiedValueShapeConstraint"), 2, null));
        RDFPropertyShape qvShape = new RDFPropertyShape(new RDFResource("ex:QualifiedValueShapeConstraint"), new RDFResource("ex:gender"));
        qvShape.AddConstraint(new RDFHasValueConstraint(new RDFResource("ex:female")));
        shapesGraph.AddShape(nodeShape);
        shapesGraph.AddShape(propShape);
        shapesGraph.AddShape(qvShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.IsTrue(validationReport.Results.Count == 1);
        Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
        Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have a minimum of 2 conforming values for the shape <ex:QualifiedValueShapeConstraint>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Brittany")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(new RDFResource("ex:parent")));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropShape")));
    }
    #endregion
}