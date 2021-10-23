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

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred2/"), new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred1/> _:12345; {Environment.NewLine}{" ",8}<http://pred2/> _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt _:12345; {Environment.NewLine}{" ",8}rdf:Bag _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Bag _:54321; {Environment.NewLine}{" ",8}a _:12345. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt _:12345; {Environment.NewLine}{" ",8}a _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred1/> \"lit\"; {Environment.NewLine}{" ",15}<http://pred2/> \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred1/"), new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred2/"), new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred1/> \"lit\"; {Environment.NewLine}{" ",8}<http://pred2/> \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"lit\"; {Environment.NewLine}{" ",15}rdf:Bag \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Bag \"lit2\"; {Environment.NewLine}{" ",15}a \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"lit\"; {Environment.NewLine}{" ",15}a \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred1/> \"lit\"@EN-US; {Environment.NewLine}{" ",15}<http://pred2/> \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred1/"), new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred2/"), new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred1/> \"lit\"@EN-US; {Environment.NewLine}{" ",8}<http://pred2/> \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"lit\"@EN-US; {Environment.NewLine}{" ",15}rdf:Bag \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Bag \"lit2\"@EN-US; {Environment.NewLine}{" ",15}a \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"lit\"@EN-US; {Environment.NewLine}{" ",15}a \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred1/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred1/> \"25\"^^xsd:integer; {Environment.NewLine}{" ",15}<http://pred2/> \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred1/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred2/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred1/> \"25\"^^xsd:integer; {Environment.NewLine}{" ",8}<http://pred2/> \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"25\"^^xsd:integer; {Environment.NewLine}{" ",15}rdf:Bag \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.BAG, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Bag \"25\"^^xsd:integer; {Environment.NewLine}{" ",15}a \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"25\"^^xsd:integer; {Environment.NewLine}{" ",15}a \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred1/> \"lit\"; {Environment.NewLine}{" ",8}<http://pred2/> \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"lit\"; {Environment.NewLine}{" ",8}rdf:Bag \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Bag \"lit2\"; {Environment.NewLine}{" ",8}a \"lit\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"lit\"; {Environment.NewLine}{" ",8}a \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred1/> \"lit\"@EN-US; {Environment.NewLine}{" ",8}<http://pred2/> \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"lit\"@EN-US; {Environment.NewLine}{" ",8}rdf:Bag \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Bag \"lit2\"@EN-US; {Environment.NewLine}{" ",8}a \"lit\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"lit\"@EN-US; {Environment.NewLine}{" ",8}a \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriplesHavingSameSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred2/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred1/> \"25\"^^xsd:integer; {Environment.NewLine}{" ",8}<http://pred2/> \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"25\"^^xsd:integer; {Environment.NewLine}{" ",8}rdf:Bag \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingFirstTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.BAG, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingFirstTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Bag \"25\"^^xsd:integer; {Environment.NewLine}{" ",8}a \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingLastTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndUsingLastTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"25\"^^xsd:integer; {Environment.NewLine}{" ",8}a \"25\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> <http://obj1/>, <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> <http://obj1/>, <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt <http://obj1/>, <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a <http://obj1/>, <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), RDFVocabulary.RDF.ALT));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), RDFVocabulary.RDF.BAG));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInObject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInObject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInObject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> rdf:Alt, rdf:Bag. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> _:12345, _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> _:12345, _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt _:12345, _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPBTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a _:12345, _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> <http://obj1/>, <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt <http://obj1/>, <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj2/")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPOTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a <http://obj1/>, <http://obj2/>. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> _:12345, _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt _:12345, _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:54321")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPBTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a _:12345, _:54321. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"lit\", \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> \"lit\", \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"lit\", \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a \"lit\", \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"lit\"@EN-US, \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> \"lit\"@EN-US, \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"lit\"@EN-US, \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a \"lit\"@EN-US, \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"25\"^^xsd:integer, \"26\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInSubject.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}rdf:Alt <http://pred/> \"25\"^^xsd:integer, \"26\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> rdf:Alt \"25\"^^xsd:integer, \"26\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> a \"25\"^^xsd:integer, \"26\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"lit\", \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"lit\", \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit2")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a \"lit\", \"lit2\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"lit\"@EN-US, \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"lit\"@EN-US, \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit", "en-US")));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("lit2", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a \"lit\"@EN-US, \"lit2\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"25\"^^xsd:integer, \"26\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicateAndUsingRegisteredNamespaceInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 rdf:Alt \"25\"^^xsd:integer, \"26\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTTriplesHavingSameSubjectAndPredicateAndUsingTypeInPredicate.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 a \"25\"^^xsd:integer, \"26\"^^xsd:integer. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldDeserializeEmptyGraph()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeEmptyNamedGraph()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <http://example.org/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
            Assert.IsTrue(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
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