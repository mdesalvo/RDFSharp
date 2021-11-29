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
using System.Collections.Generic;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFNodeShapeTests
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
        }
        #endregion

        #region Tests
        [TestMethod]
        public void ShouldCreateEmptyNodeShape()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            Assert.IsNotNull(nodeShape);
            Assert.IsTrue(nodeShape.Equals(new RDFResource("ex:nodeShape")));
            Assert.IsFalse(nodeShape.IsBlank);
            Assert.IsFalse(nodeShape.Deactivated);
            Assert.IsTrue(nodeShape.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(nodeShape.MessagesCount == 0);
            Assert.IsTrue(nodeShape.TargetsCount == 0);
            Assert.IsTrue(nodeShape.ConstraintsCount == 0);

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.Context.Equals(nodeShape.URI));
            Assert.IsTrue(nshGraph.TriplesCount == 3);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.DEACTIVATED, new RDFTypedLiteral("false", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN))));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 0);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }

            int i = 0;
            foreach (RDFConstraint constraint in nodeShape) i++;
            Assert.IsTrue(i == 0);
        }

        [TestMethod]
        public void ShouldCreateEmptyBlankNodeShape()
        {
            RDFNodeShape nodeShape = new RDFNodeShape();
            Assert.IsNotNull(nodeShape);
            Assert.IsTrue(nodeShape.IsBlank);
            Assert.IsFalse(nodeShape.Deactivated);
            Assert.IsTrue(nodeShape.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(nodeShape.MessagesCount == 0);
            Assert.IsTrue(nodeShape.TargetsCount == 0);
            Assert.IsTrue(nodeShape.ConstraintsCount == 0);

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.TriplesCount == 3);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 0);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldCreateEmptyNodeShapeDeactivated()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.Deactivate();

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.Context.Equals(nodeShape.URI));
            Assert.IsTrue(nshGraph.TriplesCount == 3);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.True)));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsTrue(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 0);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldCreateEmptyNodeShapeActivated()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.Deactivate();
            nodeShape.Activate();

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.Context.Equals(nodeShape.URI));
            Assert.IsTrue(nshGraph.TriplesCount == 3);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 0);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldCreateEmptyNodeShapeWithMessage()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.AddMessage(new RDFPlainLiteral("hello"));

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.Context.Equals(nodeShape.URI));
            Assert.IsTrue(nshGraph.TriplesCount == 4);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.MESSAGE, new RDFPlainLiteral("hello"))));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 1);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 0);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldCreateEmptyNodeShapeWithSeverity()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Warning);

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.Context.Equals(nodeShape.URI));
            Assert.IsTrue(nshGraph.TriplesCount == 3);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.WARNING)));
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Warning);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 0);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldCreateNodeShapeWithTargetClass()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            RDFTargetClass targetClass = new RDFTargetClass(new RDFResource("ex:targetClass"));
            nodeShape.AddTarget(targetClass);

            Assert.IsTrue(nodeShape.TargetsCount == 1);

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.TriplesCount == 4);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.TARGET_CLASS, targetClass.TargetValue)));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 1);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldCreateNodeShapeWithTargetNode()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            RDFTargetNode targetNode = new RDFTargetNode(new RDFResource("ex:targetNode"));
            nodeShape.AddTarget(targetNode);

            Assert.IsTrue(nodeShape.TargetsCount == 1);

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.TriplesCount == 4);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.TARGET_NODE, targetNode.TargetValue)));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 1);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldCreateNodeShapeWithTargetSubjectsOf()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            RDFTargetSubjectsOf targetSubjectsOf = new RDFTargetSubjectsOf(RDFVocabulary.FOAF.NAME);
            nodeShape.AddTarget(targetSubjectsOf);

            Assert.IsTrue(nodeShape.TargetsCount == 1);

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.TriplesCount == 4);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.TARGET_SUBJECTS_OF, targetSubjectsOf.TargetValue)));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 1);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldCreateNodeShapeWithTargetObjectsOf()
        {
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            RDFTargetObjectsOf targetObjectsOf = new RDFTargetObjectsOf(RDFVocabulary.FOAF.NAME);
            nodeShape.AddTarget(targetObjectsOf);

            Assert.IsTrue(nodeShape.TargetsCount == 1);

            //Export
            RDFGraph nshGraph = nodeShape.ToRDFGraph();
            Assert.IsNotNull(nshGraph);
            Assert.IsTrue(nshGraph.TriplesCount == 4);
            Assert.IsTrue(nshGraph.ContainsTriple(new RDFTriple(nodeShape, RDFVocabulary.SHACL.TARGET_OBJECTS_OF, targetObjectsOf.TargetValue)));

            //Import
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(nshGraph);
            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.ShapesCount == 1);
            IEnumerator<RDFShape> shapesEnum = shapesGraph.ShapesEnumerator;
            while (shapesEnum.MoveNext())
            {
                Assert.IsTrue(shapesEnum.Current is RDFNodeShape);
                Assert.IsFalse(shapesEnum.Current.Deactivated);
                Assert.IsTrue(shapesEnum.Current.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
                Assert.IsTrue(shapesEnum.Current.MessagesCount == 0);
                Assert.IsTrue(shapesEnum.Current.TargetsCount == 1);
                Assert.IsTrue(shapesEnum.Current.ConstraintsCount == 0);
            }
        }

        [TestMethod]
        public void ShouldConformNodeShapeWithTargetClassNoConstraints()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport report = shapesGraph.Validate(dataGraph);
            Assert.IsNotNull(report);
            Assert.IsTrue(report.Conforms);
        }

        [TestMethod]
        public void ShouldConformNodeShapeWithTargetNodeNoConstraints()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport report = shapesGraph.Validate(dataGraph);
            Assert.IsNotNull(report);
            Assert.IsTrue(report.Conforms);
        }

        [TestMethod]
        public void ShouldConformNodeShapeWithTargetSubjectsOfNoConstraints()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.FOAF.KNOWS));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport report = shapesGraph.Validate(dataGraph);
            Assert.IsNotNull(report);
            Assert.IsTrue(report.Conforms);
        }

        [TestMethod]
        public void ShouldConformNodeShapeWithTargetObjectsOfNoConstraints()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport report = shapesGraph.Validate(dataGraph);
            Assert.IsNotNull(report);
            Assert.IsTrue(report.Conforms);
        }

        [TestMethod]
        public void ShouldConformNodeShapeDeactivated()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:nodeShape"));
            nodeShape.Deactivate();
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport report = shapesGraph.Validate(dataGraph);
            Assert.IsNotNull(report);
            Assert.IsTrue(report.Conforms);
        }
        #endregion
    }
}