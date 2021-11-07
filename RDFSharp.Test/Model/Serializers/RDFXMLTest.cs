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
        private const string XmlNsRDF = "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"";
        private const string XmlNsXSD = "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema#\"";

        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyGraph()
        {
            RDFGraph graph = new RDFGraph();
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeEmptyGraph.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeEmptyGraph.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeEmptyGraph.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault} />"));
        }

        [TestMethod]
        public void ShouldSerializeEmptyNamedGraph()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.com/"));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeEmptyNamedGraph.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeEmptyNamedGraph.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeEmptyNamedGraph.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseExample} />"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFResource("http://obj/")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj/")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPOTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnSerializingGraphWithSPOTripleBecauseUnreduceablePredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            Assert.ThrowsException<RDFModelException>(() => RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldThrowExceptionOnSerializingGraphWithSPOTripleBecauseUnreduceablePredicate.rdf"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFResource("bnode:12345")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:nodeID=\"bnode:12345\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:nodeID=\"bnode:12345\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleHavingAltContainerOfResourcesAsObject()
        {
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource);
            cont.AddItem(new RDFResource("http://item1/"));
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), cont.ReificationSubject));
            graph.AddContainer(cont);
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfResourcesAsObject.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfResourcesAsObject.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfResourcesAsObject.rdf");
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
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfResourcesAsObject.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfResourcesAsObject.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfResourcesAsObject.rdf");
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
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfResourcesAsObject.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfResourcesAsObject.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfResourcesAsObject.rdf");
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
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfLiteralsAsObject.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfLiteralsAsObject.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingAltContainerOfLiteralsAsObject.rdf");
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
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfLiteralsAsObject.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfLiteralsAsObject.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingBagContainerOfLiteralsAsObject.rdf");
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
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfLiteralsAsObject.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfLiteralsAsObject.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPBTripleHavingSeqContainerOfLiteralsAsObject.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>{Environment.NewLine}{" ",6}<rdf:Seq>{Environment.NewLine}{" ",8}<rdf:_1>lit1</rdf:_1>{Environment.NewLine}{" ",6}</rdf:Seq>{Environment.NewLine}{" ",4}</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFResource("http://obj/")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj/")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPOTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:resource=\"http://obj/\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFResource("bnode:12345")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:nodeID=\"bnode:12345\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPBTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:nodeID=\"bnode:12345\" />{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFPlainLiteral("lit")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred>lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt>lit</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFPlainLiteral("lit", "en-US")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred xml:lang=\"EN-US\">lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLLTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt xml:lang=\"EN-US\">lit</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsXSD} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithSPLTTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsXSD} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:about=\"http://subj/\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFPlainLiteral("lit")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<autoNS1:pred>lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<rdf:Alt>lit</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFPlainLiteral("lit", "en-US")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<autoNS1:pred xml:lang=\"EN-US\">lit</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLLTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<rdf:Alt xml:lang=\"EN-US\">lit</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTripleHavingUnregisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingUnregisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingUnregisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingUnregisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsXSD} xmlns:autoNS1=\"http://pred/\" {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<autoNS1:pred rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</autoNS1:pred>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTripleHavingRegisteredPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFXml.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingRegisteredPredicate.rdf");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingRegisteredPredicate.rdf"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFXmlTest_ShouldSerializeGraphWithBPLTTripleHavingRegisteredPredicate.rdf");
            Assert.IsTrue(fileContent.Equals($"{XmlHeader}{Environment.NewLine}<rdf:RDF {XmlNsRDF} {XmlNsXSD} {XmlBaseDefault}>{Environment.NewLine}{" ",2}<rdf:Description rdf:nodeID=\"bnode:12345\">{Environment.NewLine}{" ",4}<rdf:Alt rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</rdf:Alt>{Environment.NewLine}{" ",2}</rdf:Description>{Environment.NewLine}</rdf:RDF>"));
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