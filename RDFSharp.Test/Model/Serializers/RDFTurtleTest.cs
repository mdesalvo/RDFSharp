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

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFTurtleTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}