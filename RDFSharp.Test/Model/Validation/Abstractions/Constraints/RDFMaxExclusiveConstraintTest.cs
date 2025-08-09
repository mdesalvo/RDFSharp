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
public class RDFMaxExclusiveConstraintTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateMaxExclusiveResourceConstraint()
    {
        RDFMaxExclusiveConstraint maxExclusiveConstraint = new RDFMaxExclusiveConstraint(new RDFResource("ex:value"));

        Assert.IsNotNull(maxExclusiveConstraint);
        Assert.IsTrue(maxExclusiveConstraint.Value.Equals(new RDFResource("ex:value")));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMaxExclusiveResourceConstraint()
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFMaxExclusiveConstraint(null as RDFResource));

    [TestMethod]
    public void ShouldExportMaxExclusiveResourceConstraint()
    {
        RDFMaxExclusiveConstraint maxExclusiveConstraint = new RDFMaxExclusiveConstraint(new RDFResource("ex:value"));
        RDFGraph graph = maxExclusiveConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsTrue(graph.Index.Hashes.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                    && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE.PatternMemberID)
                                                    && t.Value.ObjectID.Equals(new RDFResource("ex:value").PatternMemberID)));
    }

    [TestMethod]
    public void ShouldCreateMaxExclusiveLiteralConstraint()
    {
        RDFMaxExclusiveConstraint maxExclusiveConstraint = new RDFMaxExclusiveConstraint(new RDFPlainLiteral("value"));

        Assert.IsNotNull(maxExclusiveConstraint);
        Assert.IsTrue(maxExclusiveConstraint.Value.Equals(new RDFPlainLiteral("value")));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingMaxExclusiveLiteralConstraint()
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFMaxExclusiveConstraint(null as RDFLiteral));

    [TestMethod]
    public void ShouldExportMaxExclusiveLiteralConstraint()
    {
        RDFMaxExclusiveConstraint maxExclusiveConstraint = new RDFMaxExclusiveConstraint(new RDFPlainLiteral("value"));
        RDFGraph graph = maxExclusiveConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsTrue(graph.Index.Hashes.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                    && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE.PatternMemberID)
                                                    && t.Value.ObjectID.Equals(new RDFPlainLiteral("value").PatternMemberID)));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        nodeShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Steven")));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        nodeShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Alicez")));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bobz")));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bobz")));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bobz")));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        propertyShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bobz")));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bobz")));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bobz")));
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
        nodeShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Steve")));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <ex:Steve>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Steve")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Steve")));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
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
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Steve")));
        nodeShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Steve")));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <ex:Steve>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Steve")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Steve")));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
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
        nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
        nodeShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bob")));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <ex:Bob>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
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
        nodeShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bob")));
        shapesGraph.AddShape(nodeShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <ex:Bob>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bob")));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <ex:Bob>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        propertyShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bob")));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <ex:Bob>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bob")));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <ex:Bob>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
        propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFResource("ex:Bob")));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <ex:Bob>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
    }

    //MIXED-CONFORMS:TRUE

    [TestMethod]
    public void ShouldConformNodeShapeWithPropertyShape()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), new RDFResource("ex:birthDate"), new RDFTypedLiteral("1999-12-24", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), new RDFResource("ex:birthDate"), new RDFTypedLiteral("1998-07-30", RDFModelEnums.RDFDatatypes.XSD_DATE)));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:BeforeMillennialsShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Bob")));
        RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:birthDate"));
        propShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFTypedLiteral("1999-12-31", RDFModelEnums.RDFDatatypes.XSD_DATE)));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), new RDFResource("ex:birthDate"), new RDFTypedLiteral("1999-12-31", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), new RDFResource("ex:birthDate"), new RDFTypedLiteral("1998-07-30", RDFModelEnums.RDFDatatypes.XSD_DATE)));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:BeforeMillennialsShape"));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Bob")));
        RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:birthDate"));
        propShape.AddConstraint(new RDFMaxExclusiveConstraint(new RDFTypedLiteral("1999-12-31", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        nodeShape.AddConstraint(new RDFPropertyConstraint(propShape));
        shapesGraph.AddShape(nodeShape);
        shapesGraph.AddShape(propShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.HasCount(1, validationReport.Results[0].ResultMessages);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower than <1999-12-31Z^^http://www.w3.org/2001/XMLSchema#date>")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFTypedLiteral("1999-12-31", RDFModelEnums.RDFDatatypes.XSD_DATE)));
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(new RDFResource("ex:birthDate")));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropShape")));
    }
    #endregion
}