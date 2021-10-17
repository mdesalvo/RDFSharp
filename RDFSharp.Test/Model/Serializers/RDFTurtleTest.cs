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

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFTurtleTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}