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

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFXmlTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}