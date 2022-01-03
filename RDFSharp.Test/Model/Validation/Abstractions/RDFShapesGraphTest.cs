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
using System.Threading.Tasks;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFShapesGraphTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateShapesGraph()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));

            Assert.IsNotNull(shapesGraph);
            Assert.IsTrue(shapesGraph.Equals(new RDFResource("ex:shapesGraph")));
            Assert.IsNotNull(shapesGraph.Shapes);
            Assert.IsTrue(shapesGraph.ShapesCount == 0);

            RDFShapesGraph shapesGraph2 = new RDFShapesGraph();

            Assert.IsNotNull(shapesGraph2);
            Assert.IsTrue(shapesGraph2.URI.ToString().StartsWith("bnode:"));
            Assert.IsNotNull(shapesGraph2.Shapes);
            Assert.IsTrue(shapesGraph2.ShapesCount == 0);
        }

        [TestMethod]
        public void ShouldAddShapeAndEnumerate()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
            shapesGraph.AddShape(shape);
            shapesGraph.AddShape(shape); //This will not be accepted
            shapesGraph.AddShape(null); //This will not be accepted

            Assert.IsTrue(shapesGraph.ShapesCount == 1);

            int i = 0;
            IEnumerator<RDFShape> shapesEnumerator = shapesGraph.ShapesEnumerator;
            while (shapesEnumerator.MoveNext()) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            foreach (RDFShape shp in shapesGraph) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldMergeShapesGraphAndEnumerate()
        {
            RDFShapesGraph shapesGraph1 = new RDFShapesGraph(new RDFResource("ex:shapesGraph1"));
            RDFShape shape1 = new RDFShape(new RDFResource("ex:shape1"));
            shapesGraph1.AddShape(shape1);

            RDFShapesGraph shapesGraph2 = new RDFShapesGraph(new RDFResource("ex:shapesGraph2"));
            RDFShape shape2 = new RDFShape(new RDFResource("ex:shape2"));
            shapesGraph2.AddShape(shape2);
            shapesGraph1.MergeShapes(shapesGraph2);

            Assert.IsTrue(shapesGraph1.ShapesCount == 2);

            int j = 0;
            IEnumerator<RDFShape> shapesEnumerator = shapesGraph1.ShapesEnumerator;
            while (shapesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 2);
        }

        [TestMethod]
        public void ShouldRemoveShape()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
            shapesGraph.AddShape(shape);
            shapesGraph.RemoveShape(shape);

            Assert.IsTrue(shapesGraph.ShapesCount == 0);
        }

        [TestMethod]
        public void ShouldSelectShape()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
            shapesGraph.AddShape(shape);

            RDFShape selectShape = shapesGraph.SelectShape("ex:shape");
            Assert.IsNotNull(selectShape);
            RDFShape selectShape2 = shapesGraph.SelectShape("ex:shape2");
            Assert.IsNull(selectShape2);
            RDFShape selectShape3 = shapesGraph.SelectShape(null);
            Assert.IsNull(selectShape3);
        }

        [TestMethod]
        public void ShouldExportShapesGraph()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape shape = new RDFNodeShape(new RDFResource("ex:shape"));
            shape.AddMessage(new RDFPlainLiteral("This is an error", "en-US"));
            shape.AddTarget(new RDFTargetClass(new RDFResource("ex:class")));
            shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:class")));
            shapesGraph.AddShape(shape);
            RDFGraph graph = shapesGraph.ToRDFGraph();

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(new Uri("ex:shapesGraph")));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MESSAGE, new RDFPlainLiteral("This is an error", "en-US"))));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.TARGET_CLASS, new RDFResource("ex:class"))));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.CLASS, new RDFResource("ex:class"))));
        }

        [TestMethod]
        public async Task ShouldExportShapesGraphAsync()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape shape = new RDFNodeShape(new RDFResource("ex:shape"));
            shape.AddMessage(new RDFPlainLiteral("This is an error", "en-US"));
            shape.AddTarget(new RDFTargetClass(new RDFResource("ex:class")));
            shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:class")));
            shapesGraph.AddShape(shape);
            RDFGraph graph = await shapesGraph.ToRDFGraphAsync();

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(new Uri("ex:shapesGraph")));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MESSAGE, new RDFPlainLiteral("This is an error", "en-US"))));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.TARGET_CLASS, new RDFResource("ex:class"))));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(shape, RDFVocabulary.SHACL.CLASS, new RDFResource("ex:class"))));
        }

        [TestMethod]
        public void ShouldImportShapesGraph()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape shape = new RDFNodeShape(new RDFResource("ex:shape"));
            shape.AddMessage(new RDFPlainLiteral("This is an error", "en-US"));
            shape.AddTarget(new RDFTargetClass(new RDFResource("ex:class")));
            shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:class")));
            shapesGraph.AddShape(shape);
            RDFGraph graph = shapesGraph.ToRDFGraph();
            RDFShapesGraph shapesGraph2 = RDFShapesGraph.FromRDFGraph(graph);

            Assert.IsNotNull(shapesGraph2);
            Assert.IsTrue(shapesGraph2.Equals(new RDFResource("ex:shapesGraph")));
            Assert.IsNotNull(shapesGraph2.Shapes);
            Assert.IsTrue(shapesGraph2.ShapesCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape") is RDFNodeShape);
            Assert.IsFalse(shapesGraph2.SelectShape("ex:shape").Deactivated);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").MessagesCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Messages.Single().Equals(new RDFPlainLiteral("This is an error", "en-US")));
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").TargetsCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Targets.Single().TargetValue.Equals(new RDFResource("ex:class")));
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").ConstraintsCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Constraints.Single() is RDFClassConstraint classContstraint && classContstraint.ClassType.Equals(new RDFResource("ex:class")));
        }

        [TestMethod]
        public void ShouldImportShapesGraphHavingDeactivatedShape()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape shape = new RDFNodeShape(new RDFResource("ex:shape"));
            shape.Deactivate();
            shape.AddMessage(new RDFPlainLiteral("This is an error", "en-US"));
            shape.AddTarget(new RDFTargetClass(new RDFResource("ex:class")));
            shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:class")));
            shapesGraph.AddShape(shape);
            RDFGraph graph = shapesGraph.ToRDFGraph();
            RDFShapesGraph shapesGraph2 = RDFShapesGraph.FromRDFGraph(graph);

            Assert.IsNotNull(shapesGraph2);
            Assert.IsTrue(shapesGraph2.Equals(new RDFResource("ex:shapesGraph")));
            Assert.IsNotNull(shapesGraph2.Shapes);
            Assert.IsTrue(shapesGraph2.ShapesCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape") is RDFNodeShape);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Deactivated);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").MessagesCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Messages.Single().Equals(new RDFPlainLiteral("This is an error", "en-US")));
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").TargetsCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Targets.Single().TargetValue.Equals(new RDFResource("ex:class")));
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").ConstraintsCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Constraints.Single() is RDFClassConstraint classContstraint && classContstraint.ClassType.Equals(new RDFResource("ex:class")));
        }

        [TestMethod]
        public async Task ShouldImportShapesGraphAsync()
        {
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:shapesGraph"));
            RDFNodeShape shape = new RDFNodeShape(new RDFResource("ex:shape"));
            shape.AddMessage(new RDFPlainLiteral("This is an error", "en-US"));
            shape.AddTarget(new RDFTargetClass(new RDFResource("ex:class")));
            shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:class")));
            shapesGraph.AddShape(shape);
            RDFGraph graph = shapesGraph.ToRDFGraph();
            RDFShapesGraph shapesGraph2 = await RDFShapesGraph.FromRDFGraphAsync(graph);

            Assert.IsNotNull(shapesGraph2);
            Assert.IsTrue(shapesGraph2.Equals(new RDFResource("ex:shapesGraph")));
            Assert.IsNotNull(shapesGraph2.Shapes);
            Assert.IsTrue(shapesGraph2.ShapesCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape") is RDFNodeShape);
            Assert.IsFalse(shapesGraph2.SelectShape("ex:shape").Deactivated);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").MessagesCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Messages.Single().Equals(new RDFPlainLiteral("This is an error", "en-US")));
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").TargetsCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Targets.Single().TargetValue.Equals(new RDFResource("ex:class")));
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").ConstraintsCount == 1);
            Assert.IsTrue(shapesGraph2.SelectShape("ex:shape").Constraints.Single() is RDFClassConstraint classContstraint && classContstraint.ClassType.Equals(new RDFResource("ex:class")));
        }
        #endregion
    }
}