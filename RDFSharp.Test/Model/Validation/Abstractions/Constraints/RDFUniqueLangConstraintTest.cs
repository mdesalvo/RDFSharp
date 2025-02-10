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
public class RDFUniqueLangConstraintTest
{
    #region Tests
    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ShouldCreateUniqueLangConstraint(bool uniqueLang)
    {
        RDFUniqueLangConstraint ulConstraint = new RDFUniqueLangConstraint(uniqueLang);

        Assert.IsNotNull(ulConstraint);
        Assert.IsTrue(ulConstraint.UniqueLang.Equals(uniqueLang));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ShouldExportUniqueLangConstraint(bool uniqueLang)
    {
        RDFUniqueLangConstraint ulConstraint = new RDFUniqueLangConstraint(uniqueLang);
        RDFGraph graph = ulConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
        if (uniqueLang)
            Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                        && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.UNIQUE_LANG.PatternMemberID)
                                                        && t.Value.ObjectID.Equals(RDFTypedLiteral.True.PatternMemberID)));
        else
            Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                        && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.UNIQUE_LANG.PatternMemberID)
                                                        && t.Value.ObjectID.Equals(RDFTypedLiteral.False.PatternMemberID)));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-UK")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(true));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformPropertyShapeWithClassTargetAndFalseConfiguration()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-UK")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(false));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldConformPropertyShapeWithClassTargetAndUnlanguagedValues()
    {
        //DataGraph
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alyce")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(false));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-UK")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(true));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-UK")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(true));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(true));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alyce", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alyse", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(true));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must not have the same language tag more than one time per value")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.NAME));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alyce", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(true));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must not have the same language tag more than one time per value")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.NAME));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alyce", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(true));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must not have the same language tag more than one time per value")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.NAME));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT));
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
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alyce", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Bob", "en-US")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

        //ShapesGraph
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.NAME);
        propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
        propertyShape.AddConstraint(new RDFUniqueLangConstraint(true));
        shapesGraph.AddShape(propertyShape);

        //Validate
        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, validationReport.Results[0].Severity);
        Assert.AreEqual(1, validationReport.Results[0].ResultMessages.Count);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must not have the same language tag more than one time per value")));
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsNull(validationReport.Results[0].ResultValue);
        Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.NAME));
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape"))); 
    }
    #endregion
}