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
using System.IO;
using System.Linq;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFXmlTest
    {
        private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
        private const string XmlBaseDefault = "xml:base=\"https://rdfsharp.codeplex.com/\"";
        private const string XmlBaseExample = "xml:base=\"http://example.com/\"";
        private const string XmlNsDefault = "xmlns=\"https://rdfsharp.codeplex.com/\"";
        private const string XmlNsRDF = "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"";
        private const string XmlNsXSD = "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema#\"";

        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyGraph()
        {
            RDFGraph graph = new RDFGraph();
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeEmptyGraph.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeEmptyGraph.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeEmptyGraph.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault} />"));
        }

        [TestMethod]
        public void ShouldSerializeEmptyNamedGraph()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.com/"));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeEmptyNamedGraph.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeEmptyNamedGraph.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeEmptyNamedGraph.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseExample} />"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFResource("http://obj/")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj/")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnSerializingGraphWithSPOTripleBecauseUnreduceablePredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            Assert.ThrowsException<RDFModelException>(() => RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldThrowExceptionOnSerializingGraphWithSPOTripleBecauseUnreduceablePredicate.rdf")));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnSerializingGraphWithSPOTripleBecauseInvalidQNameWhenReducedPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#1Alt"), new RDFResource("http://obj/")));
            Assert.ThrowsException<RDFModelException>(() => RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldThrowExceptionOnSerializingGraphWithSPOTripleBecauseInvalidQNameWhenReducedPredicate.rdf")));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFResource("bnode:12345")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:nodeID=\"12345\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:nodeID=\"12345\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingAltContainerOfResourcesAsObject()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource);
            cont.AddItem(new RDFResource("http://item1/"));
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), cont.ReificationSubject));
            graph.AddContainer(cont);
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfResourcesAsObject.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfResourcesAsObject.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfResourcesAsObject.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingBagContainerOfResourcesAsObject()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource);
            cont.AddItem(new RDFResource("http://item1/"));
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), cont.ReificationSubject));
            graph.AddContainer(cont);
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfResourcesAsObject.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfResourcesAsObject.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfResourcesAsObject.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Bag>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</rdf:Bag>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfResourcesAsObject()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource);
            cont.AddItem(new RDFResource("http://item1/"));
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), cont.ReificationSubject));
            graph.AddContainer(cont);
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfResourcesAsObject.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfResourcesAsObject.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfResourcesAsObject.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Seq>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</rdf:Seq>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingAltContainerOfLiteralsAsObject()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
            cont.AddItem(new RDFPlainLiteral("lit1"));
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), cont.ReificationSubject));
            graph.AddContainer(cont);
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfLiteralsAsObject.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfLiteralsAsObject.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfLiteralsAsObject.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1>lit1</rdf:_1>{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingBagContainerOfLiteralsAsObject()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal);
            cont.AddItem(new RDFPlainLiteral("lit1"));
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), cont.ReificationSubject));
            graph.AddContainer(cont);
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfLiteralsAsObject.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfLiteralsAsObject.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfLiteralsAsObject.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Bag>{Environment.NewLine}{" ",8}<rdf:_1>lit1</rdf:_1>{Environment.NewLine}{" ",6}</rdf:Bag>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfLiteralsAsObject()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal);
            cont.AddItem(new RDFPlainLiteral("lit1"));
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), cont.ReificationSubject));
            graph.AddContainer(cont);
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfLiteralsAsObject.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfLiteralsAsObject.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfLiteralsAsObject.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Seq>{Environment.NewLine}{" ",8}<rdf:_1>lit1</rdf:_1>{Environment.NewLine}{" ",6}</rdf:Seq>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingCollectionOfResourcesAsObject()
        {
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            coll.AddItem(new RDFResource("http://item1/"));
            coll.AddItem(new RDFResource("bnode:item2"));
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), coll.ReificationSubject));
            graph.AddCollection(coll);
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingCollectionOfResourcesAsObject.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingCollectionOfResourcesAsObject.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingCollectionOfResourcesAsObject.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:parseType=\"Collection\">{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item1/\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:nodeID=\"item2\" />{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingCollectionOfLiteralsAsObject()
        {
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
            coll.AddItem(new RDFPlainLiteral("lit1"));
            coll.ReificationSubject = new RDFResource("http://coll/");
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), coll.ReificationSubject));
            graph.AddCollection(coll);
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingCollectionOfLiteralsAsObject.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingCollectionOfLiteralsAsObject.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingCollectionOfLiteralsAsObject.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://coll/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://coll/\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#List\" />{Environment.NewLine}{" ",4}<rdf:first>lit1</rdf:first>{Environment.NewLine}{" ",4}<rdf:rest rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#nil\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnSerializingGraphWithSPOTripleBecauseBadFormedCollectionAsObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.FIRST, new RDFResource("http://item1/")));
            Assert.ThrowsException<RDFModelException>(() => RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldThrowExceptionOnSerializingGraphWithSPOTripleBecauseBadFormedCollectionAsObject.rdf")));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFResource("http://obj/")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj/")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFResource("bnode:12345")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:nodeID=\"12345\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:nodeID=\"12345\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFPlainLiteral("lit")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt>lit</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFPlainLiteral("lit", "en-US")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred xml:lang=\"EN-US\">lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt xml:lang=\"EN-US\">lit</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsXSD} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsXSD} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFPlainLiteral("lit")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred>lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:Alt>lit</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFPlainLiteral("lit", "en-US")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred xml:lang=\"EN-US\">lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:Alt xml:lang=\"EN-US\">lit</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingUnregisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingUnregisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingUnregisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsXSD} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingRegisteredPredicate.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingRegisteredPredicate.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingRegisteredPredicate.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsXSD} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithFloatingContainerOfResources()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource);
            cont.AddItem(new RDFResource("http://item1/"));
            cont.AddItem(new RDFResource("http://item2/"));
            cont.ReificationSubject = new RDFResource("bnode:12345");
            RDFXml.Serialize(cont.ReifyContainer(), Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfResources.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfResources.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfResources.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt\" />{Environment.NewLine}{" ",4}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",4}<rdf:_2 rdf:resource=\"http://item2/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithFloatingContainerOfBlankResources()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource);
            cont.AddItem(new RDFResource("bnode:item1"));
            cont.AddItem(new RDFResource("bnode:item2"));
            cont.ReificationSubject = new RDFResource("bnode:12345");
            RDFXml.Serialize(cont.ReifyContainer(), Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfBlankResources.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfBlankResources.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfBlankResources.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt\" />{Environment.NewLine}{" ",4}<rdf:_1 rdf:nodeID=\"item1\" />{Environment.NewLine}{" ",4}<rdf:_2 rdf:nodeID=\"item2\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfResources()
        {
            RDFGraph graph = new RDFGraph();
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource);
            cont.AddItem(new RDFResource("http://item1/"));
            cont.AddItem(new RDFResource("http://item2/"));
            cont.ReificationSubject = new RDFResource("bnode:12345");
            graph.AddContainer(cont);
            RDFContainer cont2 = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource);
            cont2.AddItem(new RDFResource("http://item1/"));
            cont2.AddItem(new RDFResource("http://item2/"));
            cont2.ReificationSubject = new RDFResource("bnode:54321");
            graph.AddContainer(cont2);
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"),cont2.ReificationSubject));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfResources.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfResources.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfResources.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt\" />{Environment.NewLine}{" ",4}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",4}<rdf:_2 rdf:resource=\"http://item2/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",8}<rdf:_2 rdf:resource=\"http://item2/\" />{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfBlankResources()
        {
            RDFGraph graph = new RDFGraph();
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource);
            cont.AddItem(new RDFResource("bnode:item1"));
            cont.AddItem(new RDFResource("bnode:item2"));
            cont.ReificationSubject = new RDFResource("bnode:12345");
            graph.AddContainer(cont);
            RDFContainer cont2 = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource);
            cont2.AddItem(new RDFResource("bnode:item1"));
            cont2.AddItem(new RDFResource("bnode:item2"));
            cont2.ReificationSubject = new RDFResource("bnode:54321");
            graph.AddContainer(cont2);
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), cont2.ReificationSubject));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfBlankResources.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfBlankResources.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfBlankResources.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt\" />{Environment.NewLine}{" ",4}<rdf:_1 rdf:nodeID=\"item1\" />{Environment.NewLine}{" ",4}<rdf:_2 rdf:nodeID=\"item2\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1 rdf:nodeID=\"item1\" />{Environment.NewLine}{" ",8}<rdf:_2 rdf:nodeID=\"item2\" />{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithFloatingContainerOfLiterals()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
            cont.AddItem(new RDFPlainLiteral("item1"));
            cont.AddItem(new RDFPlainLiteral("item2"));
            cont.ReificationSubject = new RDFResource("bnode:12345");
            RDFXml.Serialize(cont.ReifyContainer(), Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfLiterals.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfLiterals.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingContainerOfLiterals.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt\" />{Environment.NewLine}{" ",4}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",4}<rdf:_2>item2</rdf:_2>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfLiterals()
        {
            RDFGraph graph = new RDFGraph();
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
            cont.AddItem(new RDFPlainLiteral("item1"));
            cont.AddItem(new RDFPlainLiteral("item2"));
            cont.ReificationSubject = new RDFResource("bnode:12345");
            graph.AddContainer(cont);
            RDFContainer cont2 = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
            cont2.AddItem(new RDFPlainLiteral("item1"));
            cont2.AddItem(new RDFPlainLiteral("item2"));
            cont2.ReificationSubject = new RDFResource("bnode:54321");
            graph.AddContainer(cont2);
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), cont2.ReificationSubject));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfLiterals.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfLiterals.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNotFloatingContainersOfLiterals.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt\" />{Environment.NewLine}{" ",4}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",4}<rdf:_2>item2</rdf:_2>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_2>item2</rdf:_2>{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithFloatingCollectionOfResources()
        {
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            coll.AddItem(new RDFResource("http://item1/"));
            coll.AddItem(new RDFResource("http://item2/"));
            coll.AddItem(new RDFResource("http://item3/"));
            coll.ReificationSubject = new RDFResource("bnode:12345");
            RDFXml.Serialize(coll.ReifyCollection(), Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfResources.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfResources.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfResources.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#List\" />{Environment.NewLine}{" ",4}<rdf:first rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",4}<rdf:rest rdf:parseType=\"Collection\">{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item2/\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item3/\" />{Environment.NewLine}{" ",4}</rdf:rest>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithFloatingCollectionOfBlankResources()
        {
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            coll.AddItem(new RDFResource("bnode:item1"));
            coll.AddItem(new RDFResource("bnode:item2"));
            coll.AddItem(new RDFResource("bnode:item3"));
            coll.ReificationSubject = new RDFResource("bnode:12345");
            RDFXml.Serialize(coll.ReifyCollection(), Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfBlankResources.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfBlankResources.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfBlankResources.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#List\" />{Environment.NewLine}{" ",4}<rdf:first rdf:nodeID=\"item1\" />{Environment.NewLine}{" ",4}<rdf:rest rdf:parseType=\"Collection\">{Environment.NewLine}{" ",6}<rdf:Description rdf:nodeID=\"item2\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:nodeID=\"item3\" />{Environment.NewLine}{" ",4}</rdf:rest>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfResources()
        {
            RDFGraph graph = new RDFGraph();
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            coll.AddItem(new RDFResource("http://item1/"));
            coll.AddItem(new RDFResource("http://item2/"));
            coll.AddItem(new RDFResource("http://item3/"));
            coll.ReificationSubject = new RDFResource("bnode:12345");
            graph.AddCollection(coll);
            RDFCollection coll2 = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            coll2.AddItem(new RDFResource("http://item1/"));
            coll2.AddItem(new RDFResource("http://item2/"));
            coll2.AddItem(new RDFResource("http://item3/"));
            coll2.ReificationSubject = new RDFResource("bnode:54321");
            graph.AddCollection(coll2);
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), coll2.ReificationSubject));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfResources.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfResources.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfResources.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#List\" />{Environment.NewLine}{" ",4}<rdf:first rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",4}<rdf:rest rdf:parseType=\"Collection\">{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item2/\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item3/\" />{Environment.NewLine}{" ",4}</rdf:rest>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:parseType=\"Collection\">{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item1/\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item2/\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item3/\" />{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfBlankResources()
        {
            RDFGraph graph = new RDFGraph();
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            coll.AddItem(new RDFResource("bnode:item1"));
            coll.AddItem(new RDFResource("bnode:item2"));
            coll.AddItem(new RDFResource("bnode:item3"));
            coll.ReificationSubject = new RDFResource("bnode:12345");
            graph.AddCollection(coll);
            RDFCollection coll2 = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            coll2.AddItem(new RDFResource("bnode:item1"));
            coll2.AddItem(new RDFResource("bnode:item2"));
            coll2.AddItem(new RDFResource("bnode:item3"));
            coll2.ReificationSubject = new RDFResource("bnode:54321");
            graph.AddCollection(coll2);
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), coll2.ReificationSubject));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfBlankResources.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfBlankResources.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfBlankResources.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#List\" />{Environment.NewLine}{" ",4}<rdf:first rdf:nodeID=\"item1\" />{Environment.NewLine}{" ",4}<rdf:rest rdf:parseType=\"Collection\">{Environment.NewLine}{" ",6}<rdf:Description rdf:nodeID=\"item2\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:nodeID=\"item3\" />{Environment.NewLine}{" ",4}</rdf:rest>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:parseType=\"Collection\">{Environment.NewLine}{" ",6}<rdf:Description rdf:nodeID=\"item1\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:nodeID=\"item2\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:nodeID=\"item3\" />{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithFloatingCollectionOfLiterals()
        {
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
            coll.AddItem(new RDFPlainLiteral("item1"));
            coll.ReificationSubject = new RDFResource("bnode:12345");
            RDFXml.Serialize(coll.ReifyCollection(), Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfLiterals.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfLiterals.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithFloatingCollectionOfLiterals.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#List\" />{Environment.NewLine}{" ",4}<rdf:first>item1</rdf:first>{Environment.NewLine}{" ",4}<rdf:rest rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#nil\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfLiterals()
        {
            RDFGraph graph = new RDFGraph();
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
            coll.AddItem(new RDFPlainLiteral("item1"));
            coll.ReificationSubject = new RDFResource("bnode:12345");
            graph.AddCollection(coll);
            RDFCollection coll2 = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
            coll2.AddItem(new RDFPlainLiteral("item1"));
            coll2.ReificationSubject = new RDFResource("bnode:54321");
            graph.AddCollection(coll2);
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), coll2.ReificationSubject));
            RDFXml.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfLiterals.rdf"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfLiterals.rdf")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFXmlTest_ShouldSerializeGraphWithBothFloatingAndNonFloatingCollectionsOfLiterals.rdf"));
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#List\" />{Environment.NewLine}{" ",4}<rdf:first>item1</rdf:first>{Environment.NewLine}{" ",4}<rdf:rest rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#nil\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"54321\">{Environment.NewLine}{" ",4}<rdf:type rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#List\" />{Environment.NewLine}{" ",4}<rdf:first>item1</rdf:first>{Environment.NewLine}{" ",4}<rdf:rest rdf:resource=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#nil\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:nodeID=\"54321\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        //DESERIALIZE

        [TestMethod]
        public void ShouldDeserializeEmptyGraph()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault} />");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeEmptyNamedGraph()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseExample} />");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(new Uri("http://example.com/")));
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeEmptyGraphEvenOnXmlnsAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsDefault} />");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeEmptyGraphEvenOnMissingBaseAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} />");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseInvalidRDFRootNode()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDH {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDH>");
            Assert.ThrowsException<RDFModelException>(() => RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingRDFTypeAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:type=\"http://type/\" rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), new RDFResource("http://obj/"))));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://type/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingDirectTypeDescription()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Alt rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Alt>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingUnprefixedDirectTypeDescription()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<Example rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</Example>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource(RDFNamespaceRegister.DefaultNamespace + "Example"))));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingLiteralAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:typex=\"lit\" rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), new RDFResource("http://obj/"))));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource(RDFVocabulary.RDF.BASE_URI + "typex"), new RDFPlainLiteral("lit"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingLiteralAttributeWithoutNamespace()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description typex=\"lit\" rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred"), new RDFResource("http://obj/"))));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource(RDFNamespaceRegister.DefaultNamespace + "typex"), new RDFPlainLiteral("lit"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingPredicateWithoutNamespace()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource(RDFNamespaceRegister.DefaultNamespace + "pred"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("bnode:12345")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Description>{Environment.NewLine}{" ",8}<autoNS1:pred2 rdf:nodeID=\"12345\" />{Environment.NewLine}{" ",6}</rdf:Description>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred"))  && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred2")) && t.Object is RDFResource objRes && objRes.IsBlank));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingAltContainer()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFResource("http://item1/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingAltContainerOfBlankResources()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1 rdf:nodeID=\"12345\" />{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingUnprefixedAltContainer()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<Alt>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFResource("http://item1/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingBagContainer()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Bag>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</rdf:Bag>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.BAG)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFResource("http://item1/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingUnprefixedBagContainer()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<Bag>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</Bag>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.BAG)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFResource("http://item1/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingSeqContainer()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Seq>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</rdf:Seq>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.SEQ)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFResource("http://item1/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingUnprefixedSeqContainer()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<Seq>{Environment.NewLine}{" ",8}<rdf:_1 rdf:resource=\"http://item1/\" />{Environment.NewLine}{" ",6}</Seq>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.SEQ)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFResource("http://item1/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingAltContainerOfLiterals()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_2>item2</rdf:_2>{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFPlainLiteral("item1"))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_2")) && t.Object.Equals(new RDFPlainLiteral("item2"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingAltContainerOfLiteralsWithSubjectScopedLanguage()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description xml:lang=\"en-US\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_2>item2</rdf:_2>{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFPlainLiteral("item1", "en-US"))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_2")) && t.Object.Equals(new RDFPlainLiteral("item2", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingAltContainerOfLiteralsWithPredicateScopedLanguage()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred xml:lang=\"en-US\">{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_2>item2</rdf:_2>{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFPlainLiteral("item1", "en-US"))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_2")) && t.Object.Equals(new RDFPlainLiteral("item2", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingAltContainerOfLiteralsWithAltScopedLanguage()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt xml:lang=\"en-US\">{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_2>item2</rdf:_2>{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFPlainLiteral("item1", "en-US"))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_2")) && t.Object.Equals(new RDFPlainLiteral("item2", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingAltContainerOfLiteralsWithItemScopedLanguage()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1 xml:lang=\"en-US\">item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_1 xml:lang=\"en-US\">item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_2 xml:lang=\"en-US\">item2</rdf:_2>{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFPlainLiteral("item1", "en-US"))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_2")) && t.Object.Equals(new RDFPlainLiteral("item2", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingAltContainerOfLiteralsWithDatatype()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Alt>{Environment.NewLine}{" ",8}<rdf:_1 rdf:datatype=\"{RDFVocabulary.XSD.STRING}\">item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_1 rdf:datatype=\"{RDFVocabulary.XSD.STRING}\">item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_2 rdf:datatype=\"{RDFVocabulary.XSD.STRING}\">item2</rdf:_2>{Environment.NewLine}{" ",6}</rdf:Alt>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.ALT)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFTypedLiteral("item1", RDFModelEnums.RDFDatatypes.XSD_STRING))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_2")) && t.Object.Equals(new RDFTypedLiteral("item2", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingBagContainerOfLiterals()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Bag>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_1>item1</rdf:_1>{Environment.NewLine}{" ",8}<rdf:_2>item2</rdf:_2>{Environment.NewLine}{" ",6}</rdf:Bag>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.BAG)));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_1")) && t.Object.Equals(new RDFPlainLiteral("item1"))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.BASE_URI + "_2")) && t.Object.Equals(new RDFPlainLiteral("item2"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBTripleUsingParseTypeResourceAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:parseType=\"Resource\">{Environment.NewLine}{" ",6}<autoNS1:pred2 rdf:parseType=\"Resource\" />{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("bnode:12345")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred2")) && t.Object is RDFResource objRes && objRes.IsBlank));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred>Hello</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("bnode:12345")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFPlainLiteral("Hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTripleUsingParseTypeLiteralAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:parseType=\"Literal\"><example>XML Literal!</example></autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("bnode:12345")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFTypedLiteral("<example>XML Literal!</example>", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTripleUsingDatatypeAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:datatype=\"http://www.w3.org/2001/XMLSchema#string\">Hello</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("bnode:12345")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFTypedLiteral("Hello", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTripleUsingLangAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\">{Environment.NewLine}{" ",4}<autoNS1:pred xml:lang=\"en-US\">Hello</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("bnode:12345")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFPlainLiteral("Hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTripleUsingParentLangAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\" xml:lang=\"en-US\">{Environment.NewLine}{" ",4}<autoNS1:pred>Hello</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("bnode:12345")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFPlainLiteral("Hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTripleUsingMixedLangAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"12345\" xml:lang=\"en-US\">{Environment.NewLine}{" ",4}<autoNS1:pred xml:lang=\"en-UK\">Hello</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("bnode:12345")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFPlainLiteral("Hello", "en-UK"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeAtSubjectLevel()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:ID=\"snack\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"fruit/apple\"/>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "snack")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "fruit/apple"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeAtPredicateLevelSPO()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\" rdf:resource=\"http://obj/\"/>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("http://subj/")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFResource("http://obj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.SUBJECT) && t.Object.Equals(new RDFResource("http://subj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.PREDICATE) && t.Object.Equals(new RDFResource("http://pred/pred"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.OBJECT) && t.Object.Equals(new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeAtPredicateLevelSPB()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\" rdf:nodeID=\"bnode:12345\"/>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("http://subj/")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFResource("bnode:12345"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.SUBJECT) && t.Object.Equals(new RDFResource("http://subj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.PREDICATE) && t.Object.Equals(new RDFResource("http://pred/pred"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.OBJECT) && t.Object.Equals(new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeAtPredicateLevelSPL()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\">lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("http://subj/")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFPlainLiteral("lit"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.SUBJECT) && t.Object.Equals(new RDFResource("http://subj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.PREDICATE) && t.Object.Equals(new RDFResource("http://pred/pred"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.OBJECT) && t.Object.Equals(new RDFPlainLiteral("lit"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeAtPredicateLevelSPLL1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\" xml:lang=\"en-US\">lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("http://subj/")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFPlainLiteral("lit","en-US"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.SUBJECT) && t.Object.Equals(new RDFResource("http://subj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.PREDICATE) && t.Object.Equals(new RDFResource("http://pred/pred"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.OBJECT) && t.Object.Equals(new RDFPlainLiteral("lit", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeAtPredicateLevelSPLL2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\" xml:lang=\"en-US\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\">lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("http://subj/")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFPlainLiteral("lit", "en-US"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.SUBJECT) && t.Object.Equals(new RDFResource("http://subj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.PREDICATE) && t.Object.Equals(new RDFResource("http://pred/pred"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.OBJECT) && t.Object.Equals(new RDFPlainLiteral("lit", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeAtPredicateLevelSPLT()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\" rdf:datatype=\"{RDFVocabulary.XSD.STRING}\">lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("http://subj/")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.SUBJECT) && t.Object.Equals(new RDFResource("http://subj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.PREDICATE) && t.Object.Equals(new RDFResource("http://pred/pred"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.OBJECT) && t.Object.Equals(new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeAtPredicateLevelSPLTParseTypeLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\" rdf:parseType=\"Literal\"><hello>lit</hello></autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource("http://subj/")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFTypedLiteral("<hello>lit</hello>", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.SUBJECT) && t.Object.Equals(new RDFResource("http://subj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.PREDICATE) && t.Object.Equals(new RDFResource("http://pred/pred"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.OBJECT) && t.Object.Equals(new RDFTypedLiteral("<hello>lit</hello>", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithTripleUsingIdAttributeAtPredicateLevelBecauseUnsupportedAboutAttribute()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\" rdf:about=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            Assert.ThrowsException<RDFModelException>(() => RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithTripleUsingIdAttributeBothAtSubjectAndPredicateLevel()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:ID=\"snack\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:ID=\"triple1\" rdf:resource=\"http://obj/\"/>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "snack")) && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object.Equals(new RDFResource("http://obj/"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.SUBJECT) && t.Object.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "snack"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.PREDICATE) && t.Object.Equals(new RDFResource("http://pred/pred"))));
            Assert.IsTrue(graph.Any(t => t.Subject.Equals(new RDFResource(RDFNamespaceRegister.DefaultNamespace + "triple1")) && t.Predicate.Equals(RDFVocabulary.RDF.OBJECT) && t.Object.Equals(new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousNestedTriplesHavingCollection()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description>{Environment.NewLine}{" ",4}<autoNS1:pred rdf:parseType=\"Collection\">{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item1/\" />{Environment.NewLine}{" ",6}<rdf:Description rdf:about=\"http://item2/\" />{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>");
            RDFGraph graph = RDFXml.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(new RDFResource("http://pred/pred")) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Count(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.TYPE) && t.Object.Equals(RDFVocabulary.RDF.LIST)) == 2);
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.FIRST) && t.Object.Equals(new RDFResource("http://item1/"))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.REST) && t.Object is RDFResource objRes && objRes.IsBlank));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.FIRST) && t.Object.Equals(new RDFResource("http://item2/"))));
            Assert.IsTrue(graph.Any(t => t.Subject is RDFResource subjRes && subjRes.IsBlank && t.Predicate.Equals(RDFVocabulary.RDF.REST) && t.Object.Equals(RDFVocabulary.RDF.NIL)));
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFXmlTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}