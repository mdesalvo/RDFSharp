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

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFSPARQLConstraintTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateSPARQLConstraint()
    {
        RDFSelectQuery selectQuery = RDFSelectQuery.FromString("SELECT ?THIS WHERE { ?THIS ?P ?O }");
        RDFSPARQLConstraint sparqlConstraint = new RDFSPARQLConstraint(selectQuery);

        Assert.IsNotNull(sparqlConstraint);
        Assert.IsNotNull(sparqlConstraint.SelectQuery);
        Assert.IsTrue(sparqlConstraint.SelectQuery.Equals(selectQuery));
        Assert.IsTrue(sparqlConstraint.IsBlank);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSPARQLConstraintBecauseNullValue()
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFSPARQLConstraint(null));

    [TestMethod]
    public void ShouldExportSPARQLConstraint()
    {
        RDFSelectQuery selectQuery = RDFSelectQuery.FromString("SELECT ?THIS WHERE { ?THIS ?P ?O }");
        RDFSPARQLConstraint sparqlConstraint = new RDFSPARQLConstraint(selectQuery);
        RDFGraph graph = sparqlConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

        Assert.IsNotNull(graph);
        Assert.AreEqual(3, graph.TriplesCount);
        //shape sh:sparql _:c
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("ex:NodeShape"), RDFVocabulary.SHACL.SPARQL, sparqlConstraint)));
        //_:c rdf:type sh:SPARQLConstraint
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(sparqlConstraint, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.SPARQL_CONSTRAINT)));
        //_:c sh:select "<query>"^^xsd:string
        Assert.IsTrue(graph.SelectTriples(s: sparqlConstraint, p: RDFVocabulary.SHACL.SELECT).Single()
                           .Object is RDFTypedLiteral selectLiteral
                      && selectLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.STRING.ToString())
                      && selectLiteral.Value.Equals(selectQuery.ToString()));
    }

    [TestMethod]
    public void ShouldExportSPARQLConstraintWithNullShape()
    {
        RDFSPARQLConstraint sparqlConstraint = new RDFSPARQLConstraint(RDFSelectQuery.FromString("SELECT ?THIS WHERE { ?THIS ?P ?O }"));
        RDFGraph graph = sparqlConstraint.ToRDFGraph(null);

        Assert.IsNotNull(graph);
        Assert.AreEqual(0, graph.TriplesCount);
    }

    //E2E

    [TestMethod]
    public void ShouldConformWithSPARQLConstraint()
    {
        //DataGraph: both persons are adults => no violations
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        //ShapesGraph: a SPARQL constraint flags underage persons
        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        nodeShape.AddConstraint(new RDFSPARQLConstraint(RDFSelectQuery.FromString(
            "PREFIX foaf: <http://xmlns.com/foaf/0.1/> SELECT ?this ?value WHERE { ?this foaf:age ?value . FILTER(?value < 18) }")));
        shapesGraph.AddShape(nodeShape);

        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsTrue(validationReport.Conforms);
    }

    [TestMethod]
    public void ShouldNotConformWithSPARQLConstraintMappingValue()
    {
        //DataGraph: Alice is underage (violation carrying her age as ?value), Bob is an adult
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        nodeShape.AddConstraint(new RDFSPARQLConstraint(RDFSelectQuery.FromString(
            "PREFIX foaf: <http://xmlns.com/foaf/0.1/> SELECT ?this ?value WHERE { ?this foaf:age ?value . FILTER(?value < 18) }")));
        shapesGraph.AddShape(nodeShape);

        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsNull(validationReport.Results[0].ResultPath);
        Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.SPARQL_CONSTRAINT_COMPONENT));
        Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:NodeShape")));
    }

    [TestMethod]
    public void ShouldNotConformWithSPARQLConstraintMappingPathAndMessage()
    {
        //DataGraph: Alice is underage
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        //The query projects ?path and ?message, which must surface on the validation result
        nodeShape.AddConstraint(new RDFSPARQLConstraint(RDFSelectQuery.FromString(
            "PREFIX foaf: <http://xmlns.com/foaf/0.1/> " +
            "SELECT ?this ?value ?path ?message WHERE { " +
            "  ?this foaf:age ?value . FILTER(?value < 18) " +
            "  BIND(foaf:age AS ?path) " +
            "  BIND(\"Person is underage\" AS ?message) }")));
        shapesGraph.AddShape(nodeShape);

        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsNotNull(validationReport);
        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
        Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(validationReport.Results[0].ResultPath.AsSinglePredicate().Equals(RDFVocabulary.FOAF.AGE));
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Person is underage")));
    }

    [TestMethod]
    public void ShouldUseShapeMessageWhenSPARQLConstraintHasNoMessageProjection()
    {
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddMessage(new RDFPlainLiteral("ShapeMessage"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        nodeShape.AddConstraint(new RDFSPARQLConstraint(RDFSelectQuery.FromString(
            "PREFIX foaf: <http://xmlns.com/foaf/0.1/> SELECT ?this WHERE { ?this foaf:age ?age . FILTER(?age < 18) }")));
        shapesGraph.AddShape(nodeShape);

        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(1, validationReport.ResultsCount);
        Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("ShapeMessage")));
    }

    [TestMethod]
    public void ShouldScopeSPARQLConstraintToEachFocusNodeViaPreBinding()
    {
        //Two underage persons: the "?this" pre-binding must keep each violation scoped to its own focus node
        //(without it, the unbound "?this" would cross-join, producing one violation per (focus,person) pair)
        RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Carol"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
        dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Carol"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("11", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
        RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
        nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
        nodeShape.AddConstraint(new RDFSPARQLConstraint(RDFSelectQuery.FromString(
            "PREFIX foaf: <http://xmlns.com/foaf/0.1/> SELECT ?this ?value WHERE { ?this foaf:age ?value . FILTER(?value < 18) }")));
        shapesGraph.AddShape(nodeShape);

        RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

        Assert.IsFalse(validationReport.Conforms);
        Assert.AreEqual(2, validationReport.ResultsCount);
        //Each violation pairs the focus node with its own age (no cross-join)
        Assert.IsTrue(validationReport.Results.Any(r => r.FocusNode.Equals(new RDFResource("ex:Alice")) && r.ResultValue.Equals(new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        Assert.IsTrue(validationReport.Results.Any(r => r.FocusNode.Equals(new RDFResource("ex:Carol")) && r.ResultValue.Equals(new RDFTypedLiteral("11", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
    }
    #endregion
}