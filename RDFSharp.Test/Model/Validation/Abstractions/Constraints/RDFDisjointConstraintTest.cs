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
using System.Linq;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFDisjointConstraintTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateDisjointConstraint()
        {
            RDFDisjointConstraint disjointConstraint = new RDFDisjointConstraint(new RDFResource("ex:prop"));

            Assert.IsNotNull(disjointConstraint);
            Assert.IsTrue(disjointConstraint.DisjointPredicate.Equals(new RDFResource("ex:prop")));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingDisjointConstraint()
            => Assert.ThrowsException<RDFModelException>(() => new RDFDisjointConstraint(null));

        [TestMethod]
        public void ShouldExportDisjointConstraint()
        {
            RDFDisjointConstraint disjointConstraint = new RDFDisjointConstraint(new RDFResource("ex:prop"));
            RDFGraph graph = disjointConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                    && t.Value.Predicate.Equals(RDFVocabulary.SHACL.DISJOINT)
                                                        && t.Value.Object.Equals(new RDFResource("ex:prop"))));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            nodeShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            nodeShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
            nodeShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            nodeShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
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

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
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

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            nodeShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Alice")));
            Assert.IsNull(validationReport.Results[0].ResultPath);
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            nodeShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Alice")));
            Assert.IsNull(validationReport.Results[0].ResultPath);
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
            nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
            nodeShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Alice")));
            Assert.IsNull(validationReport.Results[0].ResultPath);
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
            nodeShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            nodeShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ErrorMessage")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Alice")));
            Assert.IsNull(validationReport.Results[0].ResultPath);
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must not have common values with property <{RDFVocabulary.FOAF.AGENT}>")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        }

        [TestMethod]
        public void ShouldNotConformPropertyShapeWithClassTargetThreatingLiteralValueNodes()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.DEPICTS, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.AGE);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.DEPICTS));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must not have common values with property <{RDFVocabulary.FOAF.DEPICTS}>")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.AGE));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must not have common values with property <{RDFVocabulary.FOAF.AGENT}>")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Steve")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must not have common values with property <{RDFVocabulary.FOAF.AGENT}>")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Bob")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGENT, new RDFResource("ex:Alice")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.KNOWS);
            propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            propertyShape.AddConstraint(new RDFDisjointConstraint(RDFVocabulary.FOAF.AGENT));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must not have common values with property <{RDFVocabulary.FOAF.AGENT}>")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.KNOWS));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        }

        //MIXED-CONFORMS:FALSE

        [TestMethod]
        public void ShouldNotConformNodeShapeWithPropertyShape()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:USA"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("USA")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:USA"), new RDFResource("ex:altLabel"), new RDFPlainLiteral("United States")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Germany"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Germany")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Germany"), new RDFResource("ex:altLabel"), new RDFPlainLiteral("Germany")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:DisjointExampleShape"));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:USA")));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Germany")));
            RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:prefLabel"));
            propShape.AddConstraint(new RDFDisjointConstraint(new RDFResource("ex:altLabel")));
            nodeShape.AddConstraint(new RDFPropertyConstraint(propShape));
            shapesGraph.AddShape(nodeShape);
            shapesGraph.AddShape(propShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral($"Must not have common values with property <ex:altLabel>")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Germany")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Germany")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(new RDFResource("ex:prefLabel")));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropShape")));
        }
        #endregion
    }
}