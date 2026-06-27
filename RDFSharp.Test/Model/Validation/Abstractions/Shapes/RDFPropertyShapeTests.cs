/*
   Copyright 2012-2026 Marco De Salvo

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
using System.Linq;
using static RDFSharp.Query.RDFQueryEnums;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFPropertyShapeTests
{
    #region Tests
    [TestMethod]
    public void ShouldCreatePropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));

        Assert.IsNotNull(propertyShape);
        Assert.IsTrue(propertyShape.Equals(new RDFResource("ex:propertyShape")));
        Assert.IsFalse(propertyShape.IsBlank);
        Assert.IsNotNull(propertyShape.Path);
        Assert.IsTrue(propertyShape.Path.AsSinglePredicate().Equals(RDFVocabulary.FOAF.NAME));
        Assert.IsFalse(propertyShape.Deactivated);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, propertyShape.Severity);
        Assert.AreEqual(0, propertyShape.MessagesCount);
        Assert.AreEqual(0, propertyShape.TargetsCount);
        Assert.AreEqual(0, propertyShape.ConstraintsCount);
        Assert.IsNotNull(propertyShape.Descriptions);
        Assert.IsEmpty(propertyShape.Descriptions);
        Assert.IsNotNull(propertyShape.Names);
        Assert.IsEmpty(propertyShape.Names);
        Assert.IsNull(propertyShape.Order);
        Assert.IsNull(propertyShape.Group);
    }

    [TestMethod]

    public void ShouldCreateBlankPropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));

        Assert.IsNotNull(propertyShape);
        Assert.IsTrue(propertyShape.IsBlank);
        Assert.IsNotNull(propertyShape.Path);
        Assert.IsTrue(propertyShape.Path.AsSinglePredicate().Equals(RDFVocabulary.FOAF.NAME));
        Assert.IsFalse(propertyShape.Deactivated);
        Assert.AreEqual(RDFValidationEnums.RDFShapeSeverity.Violation, propertyShape.Severity);
        Assert.AreEqual(0, propertyShape.MessagesCount);
        Assert.AreEqual(0, propertyShape.TargetsCount);
        Assert.AreEqual(0, propertyShape.ConstraintsCount);
        Assert.IsNotNull(propertyShape.Descriptions);
        Assert.IsEmpty(propertyShape.Descriptions);
        Assert.IsNotNull(propertyShape.Names);
        Assert.IsEmpty(propertyShape.Names);
        Assert.IsNull(propertyShape.Order);
        Assert.IsNull(propertyShape.Group);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyShapeBecauseNullPath()
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFPropertyShape((RDFPropertyPath)null));

    [TestMethod]
    public void ShouldEnumeratePropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));
        int i = propertyShape.Count();

        Assert.AreEqual(0, i);
    }

    [TestMethod]

    public void ShouldDeactivatePropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));
        propertyShape.Deactivate();

        Assert.IsTrue(propertyShape.Deactivated);
    }

    [TestMethod]

    public void ShouldAddNameToPropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));
        propertyShape.AddName(new RDFPlainLiteral("hello"));
        propertyShape.AddName(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING));

        Assert.HasCount(2, propertyShape.Names);
    }

    [TestMethod]

    public void ShouldAddDescriptionToPropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));
        propertyShape.AddDescription(new RDFPlainLiteral("hello"));
        propertyShape.AddDescription(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING));

        Assert.HasCount(2, propertyShape.Descriptions);
    }

    [TestMethod]

    public void ShouldSetOrderOfPropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));
        propertyShape.SetOrder(5);

        Assert.IsTrue(propertyShape.Order.Equals(new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    [TestMethod]

    public void ShouldSetGroupOfPropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));
        propertyShape.SetGroup(new RDFResource("bnode:psGroup"));

        Assert.IsTrue(propertyShape.Group.Equals(new RDFResource("bnode:psGroup")));
    }

    [TestMethod]
    public void ShouldExportPropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));
        propertyShape.AddName(new RDFPlainLiteral("PropertyShapeName"));
        propertyShape.AddDescription(new RDFPlainLiteral("PropertyShapeDescription"));
        propertyShape.SetOrder(2);
        propertyShape.SetGroup(new RDFResource("ex:propertyShapeGroup"));
        RDFGraph pshGraph = propertyShape.ToRDFGraph();

        Assert.IsNotNull(pshGraph);
        Assert.IsTrue(pshGraph.Context.Equals(propertyShape.URI));
        Assert.AreEqual(8, pshGraph.TriplesCount);
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.PATH, RDFVocabulary.FOAF.NAME)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.NAME, new RDFPlainLiteral("PropertyShapeName"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DESCRIPTION, new RDFPlainLiteral("PropertyShapeDescription"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.ORDER, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.GROUP, new RDFResource("ex:propertyShapeGroup"))));
    }

    [TestMethod]
    public void ShouldExportPropertyShapeWithInversePath()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME, true));
        propertyShape.AddName(new RDFPlainLiteral("PropertyShapeName"));
        propertyShape.AddDescription(new RDFPlainLiteral("PropertyShapeDescription"));
        propertyShape.SetOrder(2);
        propertyShape.SetGroup(new RDFResource("ex:propertyShapeGroup"));
        RDFGraph pshGraph = propertyShape.ToRDFGraph();

        Assert.IsNotNull(pshGraph);
        Assert.IsTrue(pshGraph.Context.Equals(propertyShape.URI));
        Assert.AreEqual(9, pshGraph.TriplesCount);
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE)));
        Assert.IsTrue(pshGraph.Any(t => t.Subject.Equals(propertyShape)
                                        && t.Predicate.Equals(RDFVocabulary.SHACL.PATH)
                                        && t.Object is RDFResource { IsBlank: true } objRes
                                        && pshGraph.ContainsTriple(new RDFTriple(objRes, RDFVocabulary.SHACL.INVERSE_PATH, RDFVocabulary.FOAF.NAME))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.NAME, new RDFPlainLiteral("PropertyShapeName"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DESCRIPTION, new RDFPlainLiteral("PropertyShapeDescription"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.ORDER, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.GROUP, new RDFResource("ex:propertyShapeGroup"))));
    }

    [TestMethod]
    public void ShouldExportPropertyShapeWithAlternativePath()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"),
            RDFTestUtilities.ShaclAlternativePath(RDFVocabulary.FOAF.NAME, RDFVocabulary.FOAF.AGE));
        propertyShape.AddName(new RDFPlainLiteral("PropertyShapeName"));
        propertyShape.AddDescription(new RDFPlainLiteral("PropertyShapeDescription"));
        propertyShape.SetOrder(2);
        propertyShape.SetGroup(new RDFResource("ex:propertyShapeGroup"));
        RDFGraph pshGraph = propertyShape.ToRDFGraph();

        Assert.IsNotNull(pshGraph);
        Assert.IsTrue(pshGraph.Context.Equals(propertyShape.URI));
        Assert.AreEqual(15, pshGraph.TriplesCount);
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE)));
        Assert.IsTrue(pshGraph.Any(t => t.Subject.Equals(propertyShape)
                                        && t.Predicate.Equals(RDFVocabulary.SHACL.PATH)
                                        && t.Object is RDFResource { IsBlank: true } objRes
                                        && pshGraph.Any(t2 => t2.Subject.Equals(objRes)
                                                              && t2.Predicate.Equals(RDFVocabulary.SHACL.ALTERNATIVE_PATH)
                                                              && t2.Object is RDFResource { IsBlank: true })));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.NAME, new RDFPlainLiteral("PropertyShapeName"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DESCRIPTION, new RDFPlainLiteral("PropertyShapeDescription"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.ORDER, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.GROUP, new RDFResource("ex:propertyShapeGroup"))));
    }

    [TestMethod]
    public void ShouldExportPropertyShapeWithSequencePath()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"),
            RDFTestUtilities.ShaclSequencePath(RDFVocabulary.FOAF.NAME, RDFVocabulary.FOAF.AGE));
        propertyShape.AddName(new RDFPlainLiteral("PropertyShapeName"));
        propertyShape.AddDescription(new RDFPlainLiteral("PropertyShapeDescription"));
        propertyShape.SetOrder(2);
        propertyShape.SetGroup(new RDFResource("ex:propertyShapeGroup"));
        RDFGraph pshGraph = propertyShape.ToRDFGraph();

        Assert.IsNotNull(pshGraph);
        Assert.IsTrue(pshGraph.Context.Equals(propertyShape.URI));
        Assert.AreEqual(14, pshGraph.TriplesCount);
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE)));
        Assert.IsTrue(pshGraph.Any(t => t.Subject.Equals(propertyShape)
                                        && t.Predicate.Equals(RDFVocabulary.SHACL.PATH)
                                        && t.Object is RDFResource { IsBlank: true } objRes
                                        && pshGraph.Any(t2 => t2.Subject.Equals(objRes)
                                                              && t2.Predicate.Equals(RDFVocabulary.RDF.TYPE)
                                                              && t2.Object.Equals(RDFVocabulary.RDF.LIST))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.NAME, new RDFPlainLiteral("PropertyShapeName"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DESCRIPTION, new RDFPlainLiteral("PropertyShapeDescription"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.ORDER, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.GROUP, new RDFResource("ex:propertyShapeGroup"))));
    }

    [TestMethod]
    public void ShouldExportAndReparsePropertyShapeWithRecursivePath()
    {
        //A recursive SHACL path "knows+" (sh:oneOrMorePath), now representable by the property shape
        RDFPropertyPath recursivePath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.FOAF.KNOWS).OneOrMore());
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), recursivePath);
        RDFGraph pshGraph = propertyShape.ToRDFGraph();

        //ToRDFGraph: ex:propertyShape sh:path _:b ; _:b sh:oneOrMorePath foaf:knows
        Assert.IsTrue(pshGraph.Any(t => t.Subject.Equals(propertyShape)
                                        && t.Predicate.Equals(RDFVocabulary.SHACL.PATH)
                                        && t.Object is RDFResource { IsBlank: true } pathNode
                                        && pshGraph.ContainsTriple(new RDFTriple(pathNode, RDFVocabulary.SHACL.ONE_OR_MORE_PATH, RDFVocabulary.FOAF.KNOWS))));
        //A composite path has no single predicate to surface
        Assert.IsNull(propertyShape.Path.AsSinglePredicate());

        //Round-trip: re-parsing the serialized shape yields the same recursive path
        RDFPropertyShape reparsedShape = RDFShapesGraph.FromRDFGraph(pshGraph).SelectShape("ex:propertyShape") as RDFPropertyShape;
        Assert.IsNotNull(reparsedShape);
        Assert.AreEqual(RDFPropertyPathExpressionKinds.Link, reparsedShape.Path.Expression.Kind);
        Assert.AreEqual(RDFPropertyPathStepCardinalities.OneOrMore, reparsedShape.Path.Expression.Cardinality);
        Assert.IsTrue(reparsedShape.Path.Expression.Property.Equals(RDFVocabulary.FOAF.KNOWS));
    }

    [TestMethod]
    public void ShouldExportDeactivatedPropertyShape()
    {
        RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFTestUtilities.ShaclPath(RDFVocabulary.FOAF.NAME));
        propertyShape.Deactivate();
        propertyShape.AddName(new RDFPlainLiteral("PropertyShapeName"));
        propertyShape.AddDescription(new RDFPlainLiteral("PropertyShapeDescription"));
        propertyShape.SetOrder(2);
        propertyShape.SetGroup(new RDFResource("ex:propertyShapeGroup"));
        RDFGraph pshGraph = propertyShape.ToRDFGraph();

        Assert.IsNotNull(pshGraph);
        Assert.IsTrue(pshGraph.Context.Equals(propertyShape.URI));
        Assert.AreEqual(8, pshGraph.TriplesCount);
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.PATH, RDFVocabulary.FOAF.NAME)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.True)));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.NAME, new RDFPlainLiteral("PropertyShapeName"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DESCRIPTION, new RDFPlainLiteral("PropertyShapeDescription"))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.ORDER, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.GROUP, new RDFResource("ex:propertyShapeGroup"))));
    }
    #endregion
}