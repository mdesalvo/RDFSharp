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

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFLanguageInConstraintTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateLanguageInConstraint()
        {
            List<string> languages = new List<string>();
            languages.Add("en-US");
            languages.Add("*");
            languages.Add(string.Empty);
            languages.Add(null); //Will be discarded since null not allowed
            languages.Add("?en-US"); //Will be discarded since bad formed
            RDFLanguageInConstraint languageinConstraint = new RDFLanguageInConstraint(languages);

            Assert.IsNotNull(languageinConstraint);
            Assert.IsNotNull(languageinConstraint.LanguageTags);
            Assert.IsTrue(languageinConstraint.LanguageTags.Count == 3);
        }

        [TestMethod]
        public void ShouldExportLanguageInConstraint()
        {
            List<string> languages = new List<string>();
            languages.Add("en-US");
            languages.Add("it-IT");
            RDFLanguageInConstraint languageinConstraint = new RDFLanguageInConstraint(languages);
            RDFGraph graph = languageinConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 7);
            Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.SubjectID.Equals(new RDFResource("ex:NodeShape").PatternMemberID)
                                                    && t.Value.PredicateID.Equals(RDFVocabulary.SHACL.LANGUAGE_IN.PatternMemberID)));
            Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.PredicateID.Equals(RDFVocabulary.RDF.TYPE.PatternMemberID)
                                                        && t.Value.ObjectID.Equals(RDFVocabulary.RDF.LIST.PatternMemberID))); //2 occurrences of this
            Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.PredicateID.Equals(RDFVocabulary.RDF.FIRST.PatternMemberID)
                                                        && t.Value.ObjectID.Equals(new RDFPlainLiteral("EN-US").PatternMemberID)));
            Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.PredicateID.Equals(RDFVocabulary.RDF.REST.PatternMemberID)));
            Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.PredicateID.Equals(RDFVocabulary.RDF.FIRST.PatternMemberID)
                                                        && t.Value.ObjectID.Equals(new RDFPlainLiteral("IT-IT").PatternMemberID)));
            Assert.IsTrue(graph.IndexedTriples.Any(t => t.Value.PredicateID.Equals(RDFVocabulary.RDF.REST.PatternMemberID)
                                                        && t.Value.ObjectID.Equals(RDFVocabulary.RDF.NIL.PatternMemberID)));
        }

        //PS-CONFORMS:TRUE

        [TestMethod]
        public void ShouldConformPropertyShapeWithClassTargetExactMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice", "en-US")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob", "en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"en-US"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        [TestMethod]
        public void ShouldConformPropertyShapeWithClassTargetSemanticMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice", "en-US")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob", "en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"en"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        [TestMethod]
        public void ShouldConformPropertyShapeWithClassTargetStarMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice", "en-US")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob", "en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"*"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        [TestMethod]
        public void ShouldConformPropertyShapeWithClassTargetEmptyMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){string.Empty}));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice", "en-US")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob", "en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"en"}));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){string.Empty}));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice", "en-US")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob", "en")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"*"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        //PS-CONFORMS:FALSE

        [TestMethod]
        public void ShouldNotConformPropertyShapeWithClassTargetExactMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice", "en")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob", "en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"en-US"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Alice", "en")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.FIRSTNAME));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        }

        [TestMethod]
        public void ShouldNotConformPropertyShapeWithClassTargetSemanticMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice", "it-IT")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob", "en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"en"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Alice", "it-IT")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.FIRSTNAME));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        }

        [TestMethod]
        public void ShouldNotConformPropertyShapeWithClassTargetStarMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob", "en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"*"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.FIRSTNAME));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        }

        [TestMethod]
        public void ShouldNotConformPropertyShapeWithClassTargetEmptyMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice","en-US")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){string.Empty}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Alice","en-US")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.FIRSTNAME));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        }

        [TestMethod]
        public void ShouldNotConformPropertyShapeWithClassTargetBecauseResourceOrTypedLiteralMatch()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Man"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Woman"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Person")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Woman")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Man")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFTypedLiteral("Alice", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob","en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Person")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"  en-US "}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFTypedLiteral("Alice", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.FIRSTNAME));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob","en")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Alice")));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){" * "}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.FIRSTNAME));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob","en")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Steve","en-US")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetSubjectsOf(RDFVocabulary.RDF.TYPE));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"en"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Alice")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.FIRSTNAME));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Alice")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Steve"), RDFVocabulary.FOAF.FIRSTNAME, new RDFPlainLiteral("Steve","en-US")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Bob")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.FOAF.KNOWS, new RDFResource("ex:Steve")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:PropertyShape"), RDFVocabulary.FOAF.FIRSTNAME);
            propertyShape.AddTarget(new RDFTargetObjectsOf(RDFVocabulary.FOAF.KNOWS));
            propertyShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){"en"}));
            shapesGraph.AddShape(propertyShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Bob")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Bob")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(RDFVocabulary.FOAF.FIRSTNAME));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropertyShape")));
        }

        //MIXED-CONFORMS:TRUE

        [TestMethod]
        public void ShouldConformNodeShapeWithPropertyShape()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Mountain"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Mountain","en")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Mountain"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Hill","en-NZ")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Mountain"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Maunga","mi")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NewZealandLanguagesShape"));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Mountain")));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Berg")));
            RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:prefLabel"));
            propShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){ "en", "mi" }));
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
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Mountain"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Mountain","en")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Mountain"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Hill","en-NZ")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Mountain"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Maunga","mi")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Berg"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Berg")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Berg"), new RDFResource("ex:prefLabel"), new RDFPlainLiteral("Berg","de")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Berg"), new RDFResource("ex:prefLabel"), new RDFResource("ex:BergLabel")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NewZealandLanguagesShape"));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Mountain")));
            nodeShape.AddTarget(new RDFTargetNode(new RDFResource("ex:Berg")));
            RDFPropertyShape propShape = new RDFPropertyShape(new RDFResource("ex:PropShape"), new RDFResource("ex:prefLabel"));
            propShape.AddConstraint(new RDFLanguageInConstraint(new List<string>(){ "en", "mi" }));
            nodeShape.AddConstraint(new RDFPropertyConstraint(propShape));
            shapesGraph.AddShape(nodeShape);
            shapesGraph.AddShape(propShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 3);
            Assert.IsTrue(validationReport.Results[0].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[0].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[0].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[0].FocusNode.Equals(new RDFResource("ex:Berg")));
            Assert.IsTrue(validationReport.Results[0].ResultValue.Equals(new RDFPlainLiteral("Berg")));
            Assert.IsTrue(validationReport.Results[0].ResultPath.Equals(new RDFResource("ex:prefLabel")));
            Assert.IsTrue(validationReport.Results[0].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[0].SourceShape.Equals(new RDFResource("ex:PropShape")));
            Assert.IsTrue(validationReport.Results[1].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[1].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[1].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[1].FocusNode.Equals(new RDFResource("ex:Berg")));
            Assert.IsTrue(validationReport.Results[1].ResultValue.Equals(new RDFPlainLiteral("Berg","de")));
            Assert.IsTrue(validationReport.Results[1].ResultPath.Equals(new RDFResource("ex:prefLabel")));
            Assert.IsTrue(validationReport.Results[1].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[1].SourceShape.Equals(new RDFResource("ex:PropShape")));
            Assert.IsTrue(validationReport.Results[2].Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results[2].ResultMessages.Count == 1);
            Assert.IsTrue(validationReport.Results[2].ResultMessages[0].Equals(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration")));
            Assert.IsTrue(validationReport.Results[2].FocusNode.Equals(new RDFResource("ex:Berg")));
            Assert.IsTrue(validationReport.Results[2].ResultValue.Equals(new RDFResource("ex:BergLabel")));
            Assert.IsTrue(validationReport.Results[2].ResultPath.Equals(new RDFResource("ex:prefLabel")));
            Assert.IsTrue(validationReport.Results[2].SourceConstraintComponent.Equals(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results[2].SourceShape.Equals(new RDFResource("ex:PropShape")));
        }
        #endregion
    }
}