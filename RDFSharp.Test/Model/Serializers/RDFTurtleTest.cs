﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using System;
using System.IO;
using System.Linq;

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
        public void ShouldSerializeGraphWithSPLQuotedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("li\"t")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLQuotedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLQuotedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLQuotedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"li\"t\"\"\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLEscapedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("li\\t")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLEscapedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLEscapedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLEscapedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"li\\\\t\". {Environment.NewLine}"));
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
        public void ShouldSerializeGraphWithSPLLQuotedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("li\"t", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLQuotedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLQuotedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLQuotedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"li\"t\"\"\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLEscapedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("li\\t", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLEscapedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLEscapedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLLEscapedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"li\\\\t\"@EN-US. {Environment.NewLine}"));
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
        public void ShouldSerializeGraphWithSPLTQuotedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("li\"t", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTQuotedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTQuotedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTQuotedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"li\"t\"\"\"^^xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTEscapedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("li\\t", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTEscapedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTEscapedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithSPLTEscapedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}<http://subj/> <http://pred/> \"li\\\\t\"^^xsd:string. {Environment.NewLine}"));
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
        public void ShouldSerializeGraphWithBPLQuotedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("li\"t")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLQuotedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLQuotedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLQuotedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"\"\"li\"t\"\"\". {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLEscapedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("li\\t")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLEscapedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLEscapedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLEscapedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"li\\\\t\". {Environment.NewLine}"));
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
        public void ShouldSerializeGraphWithBPLLQuotedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("li\"t", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLQuotedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLQuotedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLQuotedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"\"\"li\"t\"\"\"@EN-US. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLEscapedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("li\\t", "en-US")));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLEscapedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLEscapedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLLEscapedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"li\\\\t\"@EN-US. {Environment.NewLine}"));
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
        public void ShouldSerializeGraphWithBPLTQuotedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("li\"t", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTQuotedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTQuotedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTQuotedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"\"\"li\"t\"\"\"^^xsd:string. {Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTEscapedTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("li\\t", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFTurtle.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTEscapedTriple.ttl");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTEscapedTriple.ttl"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTurtleTest_ShouldSerializeGraphWithBPLTEscapedTriple.ttl");
            Assert.IsTrue(fileContent.Equals($"@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{graph.Context}>.{Environment.NewLine}{Environment.NewLine}_:12345 <http://pred/> \"li\\\\t\"^^xsd:string. {Environment.NewLine}"));
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

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}<http://subj/> <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> <http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/ <http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> <http://pred/> http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingRegisteredNamespaceInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}rdf:Alt <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseUnrecognizedNamespaceInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}flef:subj <http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> rdf:Alt <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> a <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> flef:pred <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingRegisteredNamespaceInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> rdf:Alt.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), RDFVocabulary.RDF.ALT)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseUnrecognizedNamespaceInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> <http://pred/> flef:obj.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSO()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}rdf:Alt <http://pred/> xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), RDFVocabulary.XSD.STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSP()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}rdf:Alt xsd:string <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING, new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesPO()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> rdf:Alt xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPO()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix foaf: <{RDFVocabulary.FOAF.BASE_URI}>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}foaf:age rdf:Alt xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(RDFVocabulary.FOAF.AGE, RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleUsingMultipleRegisteredNamespacesSPOAndTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix foaf: <{RDFVocabulary.FOAF.BASE_URI}>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}foaf:age a xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(RDFVocabulary.FOAF.AGE, RDFVocabulary.RDF.TYPE, RDFVocabulary.XSD.STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> _:12345.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject.Equals(new RDFResource("http://subj/")));
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> _:12345.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBAnonymousTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject.Equals(new RDFResource("http://subj/")));
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}<http://subj/> <http://pred/> _:12345.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBAnonymousTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}<http://subj/> <http://pred/> [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject.Equals(new RDFResource("http://subj/")));
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> <http://pred/> _:12345.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBAnonymousTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> <http://pred/> [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/ <http://pred/> _:12345.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBAnonymousTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/ <http://pred/> [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> http://pred/> _:12345.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBAnonymousTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> http://pred/> [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> <http://pred/> _:.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBAnonymousTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> <http://pred/> [.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTripleUsingRegisteredNamespaceInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}rdf:Alt <http://pred/> _:12345.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(RDFVocabulary.RDF.ALT, new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBAnonymousTripleUsingRegisteredNamespaceInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}rdf:Alt <http://pred/> [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBTripleBecauseUnrecognizedNamespaceInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}flef:subj <http://pred/> _:12345.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBAnonymousTripleBecauseUnrecognizedNamespaceInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}flef:subj <http://pred/> [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> rdf:Alt _:12345.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBAnonymousTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> rdf:Alt [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject.Equals(new RDFResource("http://subj/")));
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> a _:12345.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBAnonymousTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> a [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject.Equals(new RDFResource("http://subj/")));
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> flef:pred _:12345.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPBAnonymousTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}<http://subj/> flef:pred [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSP()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}rdf:Alt xsd:string _:12345.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING, new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBAnonymousTripleUsingMultipleRegisteredNamespacesSP()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}rdf:Alt xsd:string [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.XSD.STRING));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTripleUsingMultipleRegisteredNamespacesSPAndTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix foaf: <{RDFVocabulary.FOAF.BASE_URI}>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}foaf:age a _:12345.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(RDFVocabulary.FOAF.AGE, RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBAnonymousTripleUsingMultipleRegisteredNamespacesSPAndTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix foaf: <{RDFVocabulary.FOAF.BASE_URI}>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}foaf:age a [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject.Equals(RDFVocabulary.FOAF.AGE));
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousInlineTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> <http://obj/> ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"[] <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousInlineTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"[ <http://pred/> <http://obj/> ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}_:12345 <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}[] <http://pred/> <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousInlineTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}[ <http://pred/> <http://obj/> ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 <http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] <http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousInlineTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ <http://pred/> <http://obj/> ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_: <http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[]] <http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousInlineTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ <http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] http://pred/> <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousInlineTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ http://pred/> <http://obj/> ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 <http://pred/> http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] <http://pred/> http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousInlineTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ <http://pred/> http://obj/> ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 rdf:Alt <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] rdf:Alt <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousInlineTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ rdf:Alt <http://obj/> ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 a <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] a <http://obj/>.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousInlineTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ a <http://obj/> ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFResource("http://obj/")));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 flef:pred <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] flef:pred <http://obj/>.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousInlineTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ flef:pred <http://obj/> ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTripleUsingRegisteredNamespaceInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> rdf:Alt.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), RDFVocabulary.RDF.ALT)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTripleUsingRegisteredNamespaceInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] <http://pred/> rdf:Alt.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(RDFVocabulary.RDF.ALT));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousInlineTripleUsingRegisteredNamespaceInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> rdf:Alt ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(RDFVocabulary.RDF.ALT));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOTripleBecauseUnrecognizedNamespaceInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 <http://pred/> flef:obj.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousTripleBecauseUnrecognizedNamespaceInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] <http://pred/> flef:obj.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPOAnonymousInlineTripleBecauseUnrecognizedNamespaceInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ <http://pred/> flef:obj ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesPO()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 rdf:Alt xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, RDFVocabulary.XSD.STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTripleUsingMultipleRegisteredNamespacesPO()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] rdf:Alt xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Object.Equals(RDFVocabulary.XSD.STRING));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousInlineTripleUsingMultipleRegisteredNamespacesPO()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ rdf:Alt xsd:string ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Object.Equals(RDFVocabulary.XSD.STRING));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTripleUsingMultipleRegisteredNamespacesBPOAndTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 a xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, RDFVocabulary.XSD.STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousTripleUsingMultipleRegisteredNamespacesBPOAndTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] a xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsTrue(graph.Single().Object.Equals(RDFVocabulary.XSD.STRING));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOAnonymousInlineTripleUsingMultipleRegisteredNamespacesBPOAndTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@prefix xsd: <{RDFVocabulary.XSD.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ a xsd:string ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsTrue(graph.Single().Object.Equals(RDFVocabulary.XSD.STRING));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> _:54321.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:54321"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] <http://pred/> [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> [] ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> _:54321.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:54321"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"[] <http://pred/> [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousInlineTripleEvenOnMissingBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"[ <http://pred/> [] ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}_:12345 <http://pred/> _:54321.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:54321"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}[] <http://pred/> [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousInlineTripleEvenOnEmptyBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <>.{Environment.NewLine}[ <http://pred/> [] ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 <http://pred/> _:54321.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] <http://pred/> [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousInlineTripleBecauseBadFormedBaseDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ <http://pred/> [] ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_: <http://pred/> _:54321.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[]] <http://pred/> [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousInlineTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[[ <http://pred/> [] ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 http://pred/> _:54321.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] http://pred/> [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousInlineTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ http://pred/> [] ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 <http://pred/> _:.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] <http://pred/> []].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousInlineTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ <http://pred/> [[ ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 rdf:Alt _:54321.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.ALT, new RDFResource("bnode:54321"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] rdf:Alt [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousInlineTripleUsingRegisteredNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ rdf:Alt [] ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 a _:54321.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:54321"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[] a [].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBAnonymousInlineTripleUsingTypeInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ a [] ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsTrue(graph.Single().Object is RDFResource objRes && objRes.IsBlank);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}_:12345 flef:pred _:54321.");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[] flef:pred [].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBPBAnonymousInlineTripleBecauseUnrecognizedNamespaceInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}.{Environment.NewLine}[ flef:pred [] ].");
            Assert.ThrowsException<RDFModelException>(() => RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hello\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLQuotedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"hel\"lo\"\"\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel\"lo"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLEscapedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\\\lo\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel\\lo"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLEscapedShortUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\u007Elo\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel~lo"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLEscapedLongUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\U0001F603lo\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel😃lo"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hello\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello","en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLQuotedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"hel\"lo\"\"\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel\"lo","en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLEscapedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\\\lo\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel\\lo", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLEscapedShortUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\u007Elo\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel~lo","en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLEscapedLongUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\U0001F603lo\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel😃lo","en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hello\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTQuotedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"\"\"hel\"lo\"\"\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("hel\"lo", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTEscapedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\\\lo\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("hel\\lo", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTEscapedShortUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\u007Elo\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("hel~lo", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTEscapedLongUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"hel\\U0001F603lo\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("hel😃lo", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTBooleanTrueTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> true.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), RDFTypedLiteral.True)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTBooleanFalseTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> false.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), RDFTypedLiteral.False)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTIntegerTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> 25.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTIntegerNegativeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> -25.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("-25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTIntegerPositiveTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> +25.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDecimalTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> 25.0.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDecimalPositiveTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> +25.0.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDecimalNegativeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> -25.0.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("-25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoubleTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> 2.02E5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoublePositiveValueTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> +2.02E5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoublePositiveExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> 2.02E+5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoublePositiveValuePositiveExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> +2.02E+5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoublePositiveValueNegativeExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> +2.02E-5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoubleNegativeValueTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> -2.02E5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("-2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoubleNegativeExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> 2.02E-5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoubleNegativeValueNegativeExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> -2.02E-5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("-2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTDoubleNegativeValuePositiveExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> -2.02E+5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("-2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hello\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hello\" ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLQuotedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"\"\"hel\"lo\"\"\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel\"lo"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLQuotedInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"\"\"hel\"lo\"\"\" ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hel\"lo")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLEscapedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\\\lo\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel\\lo"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLEscapedInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\\\lo\" ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hel\\lo")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLEscapedShortUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\u007Elo\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel~lo"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLEscapedShortUnicodeInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\u007Elo\" ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hel~lo")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLEscapedLongUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\U0001F603lo\".");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel😃lo"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLEscapedLongUnicodeInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\U0001F603lo\" ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hel😃lo")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hello\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hello\"@en-US ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hello", "en-US")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLQuotedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"\"\"hel\"lo\"\"\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel\"lo", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLQuotedInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"\"\"hel\"lo\"\"\"@en-US ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hel\"lo", "en-US")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLEscapedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\\\lo\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel\\lo", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLEscapedInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\\\lo\"@en-US ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hel\\lo", "en-US")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLEscapedShortUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\u007Elo\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel~lo", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLEscapedShortUnicodeInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\u007Elo\"@en-US ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hel~lo", "en-US")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLEscapedLongUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\U0001F603lo\"@en-US.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hel😃lo", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLEscapedLongUnicodeInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\U0001F603lo\"@en-US ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFPlainLiteral("hel😃lo", "en-US")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hello\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hello\"^^xsd:string ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTQuotedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"\"\"hel\"lo\"\"\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("hel\"lo", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTQuotedInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"\"\"hel\"lo\"\"\"^^xsd:string ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("hel\"lo", RDFModelEnums.RDFDatatypes.XSD_STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTEscapedTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\\\lo\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("hel\\lo", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTEscapedInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\\\lo\"^^xsd:string ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("hel\\lo", RDFModelEnums.RDFDatatypes.XSD_STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTEscapedShortUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\u007Elo\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("hel~lo", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTEscapedShortUnicodeInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\u007Elo\"^^xsd:string ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("hel~lo", RDFModelEnums.RDFDatatypes.XSD_STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTEscapedLongUnicodeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> \"hel\\U0001F603lo\"^^xsd:string.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("hel😃lo", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTEscapedLongUnicodeInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> \"hel\\U0001F603lo\"^^xsd:string ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("hel😃lo", RDFModelEnums.RDFDatatypes.XSD_STRING)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTBooleanTrueTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> true.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), RDFTypedLiteral.True)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTBooleanTrueInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> true ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTBooleanFalseTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> false.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), RDFTypedLiteral.False)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTBooleanFalseInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> false ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTIntegerTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> 25.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTIntegerInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> 25 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTIntegerNegativeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> -25.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("-25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTIntegerNegativeInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> -25 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("-25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTIntegerPositiveTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> +25.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTIntegerPositiveInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> +25 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDecimalTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> 25.0.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDecimalInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> 25.0 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDecimalPositiveTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> +25.0.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDecimalPositiveInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> +25.0 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDecimalNegativeTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> -25.0.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("-25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDecimalNegativeInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> -25.0 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("-25.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> 2.02E5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> 2.02E5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoublePositiveValueTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> +2.02E5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoublePositiveValueInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> +2.02E5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoublePositiveExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> 2.02E+5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoublePositiveExponentInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> 2.02E+5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoublePositiveValuePositiveExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> +2.02E+5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoublePositiveValuePositiveExponentInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> +2.02E+5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoublePositiveValueNegativeExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> +2.02E-5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoublePositiveValueNegativeExponentInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> +2.02E-5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleNegativeValueTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> -2.02E5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("-2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleNegativeValueInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> -2.02E5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("-2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleNegativeExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> 2.02E-5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleNegativeExponentInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> 2.02E-5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleNegativeValueNegativeExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> -2.02E-5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("-2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleNegativeValueNegativeExponentInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> -2.02E-5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("-2.02E-5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleNegativeValuePositiveExponentTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}_:12345 <http://pred/> -2.02E+5.");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("-2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTDoubleNegativeValuePositiveExponentInlineAnonymousTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}[ <http://pred/> -2.02E+5 ].");
            RDFGraph graph = RDFTurtle.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Subject is RDFResource subjRes && subjRes.IsBlank);
            Assert.IsTrue(graph.Single().Predicate.Equals(new RDFResource("http://pred/")));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("-2.02E5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
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