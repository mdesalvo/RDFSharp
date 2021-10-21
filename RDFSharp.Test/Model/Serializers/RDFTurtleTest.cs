using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using System;
using System.IO;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFTurtleTest
    {
        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyGraph()
        {
            RDFGraph graph = new RDFGraph();
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeEmptyGraph.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeEmptyGraph.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeEmptyGraph.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> <http://obj/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> <http://obj/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt <http://obj/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a <http://obj/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), RDFVocabulary.RDF.ALT));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInObject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInObject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingRegisteredNamespaceInObject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> rdf:Alt. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSO()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), RDFVocabulary.XSD.STRING));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSO.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSO.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSO.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSP()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING, new RDFResource("http://obj/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSP.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSP.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSP.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt xsd:string <http://obj/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesPO()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesPO.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesPO.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesPO.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPO()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.FOAF.AGE, RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPO.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPO.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPO.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix foaf: <{RDFVocabulary.FOAF.BASE_URI}>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}foaf:age rdf:Alt xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPOAndTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.FOAF.AGE, RDFVocabulary.RDF.TYPE, RDFVocabulary.XSD.STRING));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPOAndTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPOAndTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPOAndTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix foaf: <{RDFVocabulary.FOAF.BASE_URI}>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}foaf:age a xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSP()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING, new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSP.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSP.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSP.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt xsd:string _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSPAndTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.FOAF.AGE, RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSPAndTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSPAndTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSPAndTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix foaf: <{RDFVocabulary.FOAF.BASE_URI}>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}foaf:age a _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> <http://obj/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt <http://obj/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a <http://obj/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleUsingRegisteredNamespaceInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), RDFVocabulary.RDF.ALT));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingRegisteredNamespaceInObject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingRegisteredNamespaceInObject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingRegisteredNamespaceInObject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> rdf:Alt. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPO()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPO.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPO.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPO.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPOAndTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, RDFVocabulary.XSD.STRING));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPOAndTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPOAndTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPOAndTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFPlainLiteral("lit")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleUsingMultipleRegisteredNamespacesSP()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING, new RDFPlainLiteral("lit")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingMultipleRegisteredNamespacesSP.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingMultipleRegisteredNamespacesSP.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingMultipleRegisteredNamespacesSP.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt xsd:string \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleUsingLongLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("What a \"long literal")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingLongLiteral.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingLongLiteral.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTripleUsingLongLiteral.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"What a \"long literal\"\"\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleUsingMultipleRegisteredNamespacesSP()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING, new RDFPlainLiteral("lit", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingMultipleRegisteredNamespacesSP.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingMultipleRegisteredNamespacesSP.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingMultipleRegisteredNamespacesSP.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt xsd:string \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTripleUsingLongLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("What a \"long literal", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingLongLiteral.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingLongLiteral.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTripleUsingLongLiteral.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"What a \"long literal\"\"\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleUsingMultipleRegisteredNamespacesSP()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingMultipleRegisteredNamespacesSP.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingMultipleRegisteredNamespacesSP.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingMultipleRegisteredNamespacesSP.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt xsd:string \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTripleUsingLongLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("What a \"long literal", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingLongLiteral.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingLongLiteral.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTripleUsingLongLiteral.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"What a \"long literal\"\"\"^^xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTripleUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTripleUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTripleUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTripleUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTripleUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTripleUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTripleUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTripleUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred1/> <http://obj1/>; {Environment.NewLine}{" ",15}<http://pred2/> <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred1/> <http://obj1/>; {Environment.NewLine}{" ",8}<http://pred2/> <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt <http://obj1/>; {Environment.NewLine}{" ",15}rdf:Bag <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Bag <http://obj2/>; {Environment.NewLine}{" ",15}a <http://obj1/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt <http://obj1/>; {Environment.NewLine}{" ",15}a <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred1/"), RDFVocabulary.RDF.ALT));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), RDFVocabulary.RDF.ALT));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInObject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInObject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInObject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred1/> rdf:Alt; {Environment.NewLine}{" ",15}<http://pred2/> rdf:Alt. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred1/"), new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred1/> _:12345; {Environment.NewLine}{" ",15}<http://pred2/> _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred1/"), new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred2/"), new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred1/> _:12345; {Environment.NewLine}{" ",8}<http://pred2/> _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt _:12345; {Environment.NewLine}{" ",15}rdf:Bag _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Bag _:54321; {Environment.NewLine}{" ",15}a _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt _:12345; {Environment.NewLine}{" ",15}a _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred1/> <http://obj1/>; {Environment.NewLine}{" ",8}<http://pred2/> <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt <http://obj1/>; {Environment.NewLine}{" ",8}rdf:Bag <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Bag <http://obj2/>; {Environment.NewLine}{" ",8}a <http://obj1/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt <http://obj1/>; {Environment.NewLine}{" ",8}a <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), RDFVocabulary.RDF.ALT));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred2/"), RDFVocabulary.RDF.ALT));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInObject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInObject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndUsingRegisteredNamespaceInObject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred1/> rdf:Alt; {Environment.NewLine}{" ",8}<http://pred2/> rdf:Alt. {Environment.NewLine}"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFTurtleTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}