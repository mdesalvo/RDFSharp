using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using System;
using System.IO;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFTriXTest
    {
        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyGraph()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyGraph.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyGraph.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyGraph.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeEmptyNamedGraph()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyNamedGraph.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyNamedGraph.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyNamedGraph.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPOTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPOTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPOTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPOTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPOTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPOTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPOTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFTriXTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}