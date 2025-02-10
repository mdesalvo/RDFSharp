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
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFShapeTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateShape()
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        Assert.IsNotNull(shape);
        Assert.IsFalse(shape.Deactivated);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, shape.Severity);
        Assert.IsNotNull(shape.Messages);
        Assert.AreEqual(0, shape.MessagesCount);
        Assert.IsNotNull(shape.Targets);
        Assert.AreEqual(0, shape.TargetsCount);
        Assert.IsNotNull(shape.Constraints);
        Assert.AreEqual(0, shape.ConstraintsCount);
    }

    [TestMethod]
    public void ShouldActivateShape()
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        shape.Activate();
        Assert.IsFalse(shape.Deactivated);
    }

    [TestMethod]
    public void ShouldDeactivateShape()
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        shape.Deactivate();
        Assert.IsTrue(shape.Deactivated);
    }

    [TestMethod]
    public void ShouldSetShapeSeverity()
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Info);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Info, shape.Severity);
    }

    [TestMethod]
    public void ShouldAddShapeMessageAndEnumerate()
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        shape.AddMessage(new RDFPlainLiteral("This is an error", "en-US"));
        shape.AddMessage(new RDFTypedLiteral("This is an error", RDFModelEnums.RDFDatatypes.XSD_STRING));
        shape.AddMessage(new RDFTypedLiteral("This is an error", RDFModelEnums.RDFDatatypes.RDFS_LITERAL)); //This will not be accepted
        Assert.AreEqual(2, shape.MessagesCount);

        int j = 0;
        IEnumerator<RDFLiteral> messagesEnumerator = shape.MessagesEnumerator;
        while (messagesEnumerator.MoveNext()) j++;
        Assert.AreEqual(2, j);
    }

    [TestMethod]
    public void ShouldAddShapeTargetAndEnumerate()
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        shape.AddTarget(new RDFTargetClass(new RDFResource("ex:class")));
        shape.AddTarget(null); //This will not be accepted
        Assert.AreEqual(1, shape.TargetsCount);

        int j = 0;
        IEnumerator<RDFTarget> targetsEnumerator = shape.TargetsEnumerator;
        while (targetsEnumerator.MoveNext()) j++;
        Assert.AreEqual(1, j);
    }

    [TestMethod]
    public void ShouldAddShapeConstraintAndEnumerate()
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:class")));
        shape.AddConstraint(null); //This will not be accepted
        Assert.AreEqual(1, shape.ConstraintsCount);

        int i = 0;
        IEnumerator<RDFConstraint> constraintsEnumerator = shape.ConstraintsEnumerator;
        while (constraintsEnumerator.MoveNext()) i++;
        Assert.AreEqual(1, i);

        int j = shape.Count();
        Assert.AreEqual(1, j);
    }

    [TestMethod]
    public void ShouldExportEmptyShapeToGraph()
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        RDFGraph shapeGraph = shape.ToRDFGraph();

        Assert.IsNotNull(shapeGraph);
        Assert.IsTrue(shapeGraph.Context.Equals(new Uri("ex:shape")));
        Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.SEVERITY_PROPERTY) && t.Object.Equals(RDFVocabulary.SHACL.VIOLATION)));
        Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.DEACTIVATED) && t.Object.Equals(RDFTypedLiteral.False)));
    }

    [DataTestMethod]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Info)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning)]
    [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation)]
    public void ShouldExportShapeToGraph(RDFValidationEnums.RDFShapeSeverity severity)
    {
        RDFShape shape = new RDFShape(new RDFResource("ex:shape"));
        shape.SetSeverity(severity);
        shape.AddMessage(new RDFPlainLiteral("This is an error", "en-US"));
        shape.AddTarget(new RDFTargetClass(new RDFResource("ex:class")));
        shape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:class")));
        RDFGraph shapeGraph = shape.ToRDFGraph();

        Assert.IsNotNull(shapeGraph);
        Assert.IsTrue(shapeGraph.Context.Equals(new Uri("ex:shape")));
        switch (severity)
        {
            case RDFValidationEnums.RDFShapeSeverity.Info:
                Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.SEVERITY_PROPERTY) && t.Object.Equals(RDFVocabulary.SHACL.INFO)));
                break;
            case RDFValidationEnums.RDFShapeSeverity.Warning:
                Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.SEVERITY_PROPERTY) && t.Object.Equals(RDFVocabulary.SHACL.WARNING)));
                break;
            case RDFValidationEnums.RDFShapeSeverity.Violation:
                Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.SEVERITY_PROPERTY) && t.Object.Equals(RDFVocabulary.SHACL.VIOLATION)));
                break;
        }
        Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.DEACTIVATED) && t.Object.Equals(RDFTypedLiteral.False)));
        Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.MESSAGE) && t.Object.Equals(new RDFPlainLiteral("This is an error", "en-US"))));
        Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.TARGET_CLASS) && t.Object.Equals(new RDFResource("ex:class"))));
        Assert.IsTrue(shapeGraph.Any(t => t.Subject.Equals(new RDFResource("ex:shape")) && t.Predicate.Equals(RDFVocabulary.SHACL.CLASS) && t.Object.Equals(new RDFResource("ex:class"))));
    }
    #endregion
}