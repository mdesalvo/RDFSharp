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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFValidationEngineTest
    {
        #region Tests
        [TestMethod]
        public void ShouldConformNodeShape()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            nodeShape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Human")));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        [TestMethod]
        public async Task ShouldConformNodeShapeAsync()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            nodeShape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Human")));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = await shapesGraph.ValidateAsync(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        [TestMethod]
        public void ShouldNotConformNodeShape()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            nodeShape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Person"))); //Alice will violate this
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results.Single().Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results.Single().ResultMessages.Single().Equals(new RDFPlainLiteral("ErrorMessage")));
            Assert.IsTrue(validationReport.Results.Single().FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results.Single().ResultValue.Equals(new RDFResource("ex:Alice")));
            Assert.IsNull(validationReport.Results.Single().ResultPath);            
            Assert.IsTrue(validationReport.Results.Single().SourceConstraintComponent.Equals(RDFVocabulary.SHACL.CLASS_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results.Single().SourceShape.Equals(new RDFResource("ex:NodeShape")));
        }

        [TestMethod]
        public void ShouldConformPropertyShape()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("21", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.AGE);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            propertyShape.AddConstraint(new RDFMaxInclusiveConstraint(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        [TestMethod]
        public async Task ShouldConformPropertyShapeAsync()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("21", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.AGE);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            propertyShape.AddConstraint(new RDFMaxInclusiveConstraint(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = await shapesGraph.ValidateAsync(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        [TestMethod]
        public void ShouldNotConformPropertyShape()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("21", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.AGE);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            propertyShape.AddConstraint(new RDFMaxInclusiveConstraint(new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_INTEGER))); //Bob will violate this
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Must have values lower or equal than <20^^http://www.w3.org/2001/XMLSchema#integer>")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFTypedLiteral("21", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.AGE));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MAX_INCLUSIVE_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        }

        //E2E: BUG#302
        [TestMethod]
        public void ShouldWorkWithInversePath()
        {
            //ShapesGraph
            string shapesData =
@"
@prefix exns:    <http://ex.com/ns#> .
@prefix rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix sh:      <http://www.w3.org/ns/shacl#> .

exns:Parent  rdf:type  sh:NodeShape ;
        sh:property     exns:Parent.Child-cardinality ;
        sh:targetClass  exns:Parent .


exns:Child  rdf:type  sh:NodeShape ;
        sh:property     exns:Child.Parent-cardinality ;
        sh:targetClass  exns:Child .

exns:Parent.Child-cardinality
        rdf:type        sh:PropertyShape ;
        sh:description  ""This constraint validates the cardinality of the association at the inverse direction."" ;
        sh:group        exns:CardinalityGroup ;
        sh:message      ""Cardinality violation. Lower bound shall be 1. exns:Parent.Child-cardinality"" ;
        sh:minCount     1 ;
        sh:name         ""Parent.Child-cardinality"" ;
        sh:order        0 ;
        sh:path         [ sh:inversePath  exns:Child.Parent ] ;
        sh:severity     sh:Violation .

exns:Child.Parent-cardinality
        rdf:type        sh:PropertyShape ;
        sh:description  ""This constraint validates the cardinality of the association at the inverse direction."" ;
        sh:group        exns:CardinalityGroup ;
        sh:message      ""Cardinality violation. A child should have at least one parent. exns:Child.Parent-cardinality"" ;
        sh:minCount     1 ;
        sh:name         ""Child.Parent-cardinality"" ;
        sh:order        0 ;
        sh:path         exns:Child.Parent ;
        sh:severity     sh:Violation .";
            MemoryStream shapeStream = new MemoryStream();
            using (StreamWriter shapeStreamWriter = new StreamWriter(shapeStream))
                shapeStreamWriter.WriteLine(shapesData);
            RDFGraph shapesGraphObject = RDFTurtle.Deserialize(new MemoryStream(shapeStream.ToArray()), null);
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(shapesGraphObject);

            //DataGraph
            string graphData =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<rdf:RDF
	xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
	xmlns:exns=""http://ex.com/ns#"">

	<exns:Child rdf:about=""http://ex.com/ns#child"">
		<exns:Child.Parent rdf:resource=""http://ex.com/ns#parent0"" />
	</exns:Child>

	<exns:Parent rdf:about=""http://ex.com/ns#parent0"" />
	<exns:Parent rdf:about=""http://ex.com/ns#parent1"" />
</rdf:RDF>";
            MemoryStream dataStream = new MemoryStream();
            using (StreamWriter dataStreamWriter = new StreamWriter(dataStream))
                dataStreamWriter.WriteLine(graphData);
            RDFGraph dataGraph = RDFXml.Deserialize(new MemoryStream(dataStream.ToArray()), null);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Cardinality violation. Lower bound shall be 1. exns:Parent.Child-cardinality")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("http://ex.com/ns#parent1")));
            Assert.IsNull(validationReport.Results[0].ResultValue);
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(new RDFResource("http://ex.com/ns#Child.Parent")));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("http://ex.com/ns#Parent.Child-cardinality")));
        }

        //E2E: BUG#304
        [TestMethod]
        public void ShouldWorkWithAlternativePath()
        {
            //ShapesGraph
            string shapesData =
@"
@prefix exns:   <http://ex.com/ns#> .
@prefix rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix sh:     <http://www.w3.org/ns/shacl#> .

exns:Person  rdf:type  sh:NodeShape ;
        sh:property     exns:Person.mobile-landline-cardinality ;
        sh:targetClass  exns:Person .

exns:Person.mobile-landline-cardinality
        rdf:type        sh:PropertyShape ;
        sh:description  ""This constraint validates that a Person has either mobileNumber or landLine set."" ;
        sh:minCount     1 ;
        sh:message      ""Either mobileNumber or landLine should be present"" ;
        sh:name         ""Person.mobileNumber-landLine-cardinality"" ;
        sh:order        0 ;
        sh:path         [ sh:alternativePath ( exns:Person.mobilePhone exns:Person.landLine ) ] ;
        sh:severity     sh:Violation .";

            MemoryStream shapeStream = new MemoryStream();
            using (StreamWriter shapeStreamWriter = new StreamWriter(shapeStream))
                shapeStreamWriter.WriteLine(shapesData);
            RDFGraph shapesGraphObject = RDFTurtle.Deserialize(new MemoryStream(shapeStream.ToArray()), null);
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(shapesGraphObject);

            //DataGraph
            string graphData =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<rdf:RDF
	xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
	xmlns:exns=""http://ex.com/ns#"">

	<exns:Person rdf:about=""http://ex.com/ns#person0"">
        <exns:Person.mobilePhone>0123456789</exns:Person.mobilePhone>
    </exns:Person>

	<exns:Person rdf:about=""http://ex.com/ns#person1"">
        <exns:Person.landLine>0123456789</exns:Person.landLine>
    </exns:Person>

	<exns:Person rdf:about=""http://ex.com/ns#person2"">
        <exns:Person.mobilePhone>0123456789</exns:Person.mobilePhone>
        <exns:Person.landLine>0123456789</exns:Person.landLine>
    </exns:Person>

	<exns:Person rdf:about=""http://ex.com/ns#person3""/>
</rdf:RDF>";
            MemoryStream dataStream = new MemoryStream();
            using (StreamWriter dataStreamWriter = new StreamWriter(dataStream))
                dataStreamWriter.WriteLine(graphData);
            RDFGraph dataGraph = RDFXml.Deserialize(new MemoryStream(dataStream.ToArray()), null);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Either mobileNumber or landLine should be present")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("http://ex.com/ns#person3")));
            Assert.IsNull(validationReport.Results[0].ResultValue);
            Assert.IsNull(validationReport.Results[0].ResultPath);
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("http://ex.com/ns#Person.mobile-landline-cardinality")));
        }

        //E2E: TASK#306
        [TestMethod]
        public void ShouldWorkWithSequencePath()
        {
            //ShapesGraph
            string shapesData =
@"
@prefix exns:   <http://ex.com/ns#> .
@prefix rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix sh:     <http://www.w3.org/ns/shacl#> .

exns:Person  rdf:type  sh:NodeShape ;
        sh:property     exns:Person.Uncle-cardinality ;
        sh:targetClass  exns:Person .

exns:Person.Uncle-cardinality
        rdf:type        sh:PropertyShape ;
        sh:description  ""This constraint validates that a Person has at least one uncle."" ;
        sh:minCount     1 ;
        sh:message      ""Person should have at least one Uncle defined as: Person.Father/Person.Brother"" ;
        sh:name         ""Person.Uncle-cardinality"" ;
        sh:order        0 ;
        sh:path         ( exns:Person.Father exns:Person.Brother ) ;
        sh:severity     sh:Violation .";

            MemoryStream shapeStream = new MemoryStream();
            using (StreamWriter shapeStreamWriter = new StreamWriter(shapeStream))
                shapeStreamWriter.WriteLine(shapesData);
            RDFGraph shapesGraphObject = RDFTurtle.Deserialize(new MemoryStream(shapeStream.ToArray()), null);
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(shapesGraphObject);

            //DataGraph
            string graphData =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<rdf:RDF
	xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
	xmlns:exns=""http://ex.com/ns#"">

	<exns:Person rdf:about=""http://ex.com/ns#person0"">
        <exns:Person.Father rdf:resource=""http://ex.com/ns#person1""/>
    </exns:Person>

	<exns:Person rdf:about=""http://ex.com/ns#person1"">
        <exns:Person.Father rdf:resource=""http://ex.com/ns#person2""/>
        <exns:Person.Brother rdf:resource=""http://ex.com/ns#person3""/>
    </exns:Person>

	<exns:Person rdf:about=""http://ex.com/ns#person2"">
        <exns:Person.Father rdf:resource=""http://ex.com/ns#person4""/>
        <exns:Person.Brother rdf:resource=""http://ex.com/ns#person3""/>
    </exns:Person>

	<exns:Person rdf:about=""http://ex.com/ns#person3""/>

    <exns:Person rdf:about=""http://ex.com/ns#person4"">
        <exns:Person.Father rdf:resource=""http://ex.com/ns#person1""/>
        <exns:Person.Brother rdf:resource=""http://ex.com/ns#person3""/>
    </exns:Person>
</rdf:RDF>";
            MemoryStream dataStream = new MemoryStream();
            using (StreamWriter dataStreamWriter = new StreamWriter(dataStream))
                dataStreamWriter.WriteLine(graphData);
            RDFGraph dataGraph = RDFXml.Deserialize(new MemoryStream(dataStream.ToArray()), null);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Person should have at least one Uncle defined as: Person.Father/Person.Brother")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("http://ex.com/ns#person3")));
            Assert.IsNull(validationReport.Results[0].ResultValue);
            Assert.IsNull(validationReport.Results[0].ResultPath);
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("http://ex.com/ns#Person.Uncle-cardinality")));
        }

        //E2E: BUG#307
        [TestMethod]
        public void ShouldWorkWithRDFXMLDeserializationOfID()
        {
            //ShapesGraph
            string shapesData =
@"
@prefix rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix sh:      <http://www.w3.org/ns/shacl#> .
@prefix exns:    <http://ex.com/ns#> .

exns:Parent  rdf:type  sh:NodeShape ;
        sh:property     exns:Parent.Child-cardinality ;
        sh:targetClass  exns:Parent .

exns:Child  rdf:type  sh:NodeShape ;
        sh:property     exns:Child.Parent-cardinality ;
        sh:targetClass  exns:Child .

exns:Parent.Child-cardinality
        rdf:type        sh:PropertyShape ;
        sh:description  ""This constraint validates the cardinality of the association at the inverse direction."" ;
        sh:group        exns:CardinalityGroup ;
        sh:message      ""Cardinality violation. Lower bound shall be 1. exns:Parent.Child-cardinality"" ;
        sh:minCount     1 ;
        sh:name         ""Parent.Child-cardinality"" ;
        sh:order        0 ;
        sh:path         [ sh:inversePath exns:Child.Parent ] ;
        sh:severity     sh:Violation .

exns:Child.Parent-cardinality
        rdf:type        sh:PropertyShape ;
        sh:description  ""This constraint validates the cardinality of the association at the inverse direction."" ;
        sh:group        exns:CardinalityGroup ;
        sh:message      ""Cardinality violation. A child should have at least one parent. exns:Parent.Child-cardinality"" ;
        sh:minCount     1 ;
        sh:name         ""Parent.Child-cardinality"" ;
        sh:order        0 ;
        sh:path         exns:Child.Parent ;
        sh:severity     sh:Violation .";
            MemoryStream shapeStream = new MemoryStream();
            using (StreamWriter shapeStreamWriter = new StreamWriter(shapeStream))
                shapeStreamWriter.WriteLine(shapesData);
            RDFGraph shapesGraphObject = RDFTurtle.Deserialize(new MemoryStream(shapeStream.ToArray()), null);
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(shapesGraphObject);

            //DataGraph
            string graphData =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<rdf:RDF
    xml:base=""http://test.com/""
	xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
	xmlns:exns=""http://ex.com/ns#""
	>

	<exns:Child rdf:ID=""child"">
		<exns:Child.Parent rdf:resource=""#parent0"" />
	</exns:Child>

	<exns:Parent rdf:ID=""parent0"" />
	<exns:Parent rdf:ID=""#parent1"" />
</rdf:RDF>";
            MemoryStream dataStream = new MemoryStream();
            using (StreamWriter dataStreamWriter = new StreamWriter(dataStream))
                dataStreamWriter.WriteLine(graphData);
            RDFGraph dataGraph = RDFXml.Deserialize(new MemoryStream(dataStream.ToArray()), null);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Cardinality violation. Lower bound shall be 1. exns:Parent.Child-cardinality")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("http://test.com/#parent1")));
            Assert.IsNull(validationReport.Results[0].ResultValue);
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(new RDFResource("http://ex.com/ns#Child.Parent")));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("http://ex.com/ns#Parent.Child-cardinality")));
        }
        [TestMethod]
        public void ShouldWorkWithRDFXMLDeserializationOfID2()
        {
            //ShapesGraph
            string shapesData =
@"
@prefix rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix sh:      <http://www.w3.org/ns/shacl#> .
@prefix exns:    <http://ex.com/ns#> .

exns:Parent  rdf:type  sh:NodeShape ;
        sh:property     exns:Parent.Child-cardinality ;
        sh:targetClass  exns:Parent .

exns:Child  rdf:type  sh:NodeShape ;
        sh:property     exns:Child.Parent-cardinality ;
        sh:targetClass  exns:Child .

exns:Parent.Child-cardinality
        rdf:type        sh:PropertyShape ;
        sh:description  ""This constraint validates the cardinality of the association at the inverse direction."" ;
        sh:group        exns:CardinalityGroup ;
        sh:message      ""Cardinality violation. Lower bound shall be 1. exns:Parent.Child-cardinality"" ;
        sh:minCount     1 ;
        sh:name         ""Parent.Child-cardinality"" ;
        sh:order        0 ;
        sh:path         [ sh:inversePath exns:Child.Parent ] ;
        sh:severity     sh:Violation .

exns:Child.Parent-cardinality
        rdf:type        sh:PropertyShape ;
        sh:description  ""This constraint validates the cardinality of the association at the inverse direction."" ;
        sh:group        exns:CardinalityGroup ;
        sh:message      ""Cardinality violation. A child should have at least one parent. exns:Parent.Child-cardinality"" ;
        sh:minCount     1 ;
        sh:name         ""Parent.Child-cardinality"" ;
        sh:order        0 ;
        sh:path         exns:Child.Parent ;
        sh:severity     sh:Violation .";
            MemoryStream shapeStream = new MemoryStream();
            using (StreamWriter shapeStreamWriter = new StreamWriter(shapeStream))
                shapeStreamWriter.WriteLine(shapesData);
            RDFGraph shapesGraphObject = RDFTurtle.Deserialize(new MemoryStream(shapeStream.ToArray()), null);
            RDFShapesGraph shapesGraph = RDFShapesGraph.FromRDFGraph(shapesGraphObject);

            //DataGraph
            string graphData =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<rdf:RDF
    xml:base=""http://test.com/#""
	xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
	xmlns:exns=""http://ex.com/ns#""
	>

	<exns:Child rdf:ID=""child"">
		<exns:Child.Parent rdf:resource=""#parent0"" />
	</exns:Child>

	<exns:Parent rdf:ID=""parent0"" />
	<exns:Parent rdf:ID=""#parent1"" />
</rdf:RDF>";
            MemoryStream dataStream = new MemoryStream();
            using (StreamWriter dataStreamWriter = new StreamWriter(dataStream))
                dataStreamWriter.WriteLine(graphData);
            RDFGraph dataGraph = RDFXml.Deserialize(new MemoryStream(dataStream.ToArray()), null);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Cardinality violation. Lower bound shall be 1. exns:Parent.Child-cardinality")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("http://test.com/#parent1")));
            Assert.IsNull(validationReport.Results[0].ResultValue);
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(new RDFResource("http://ex.com/ns#Child.Parent")));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("http://ex.com/ns#Parent.Child-cardinality")));
        }
        #endregion
    }
}
