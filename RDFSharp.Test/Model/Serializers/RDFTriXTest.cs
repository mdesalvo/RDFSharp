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

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPBTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPBTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPBTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPBTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPBTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPBTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPBTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPOTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPOTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPOTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPOTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPOTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPOTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPOTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPBTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPBTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPBTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPBTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPBTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPBTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPBTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPLTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLLTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLLTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLLTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPLLTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLLTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLLTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLLTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLTTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLTTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithSPLTTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPLTTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLTTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLTTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithSPLTTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPLTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLLTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLLTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLLTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPLLTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLLTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLLTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLLTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLTTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLTTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeGraphWithBPLTTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPLTTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLTTriple.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLTTriple.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeNamedGraphWithBPLTTriple.trix");
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeEmptyGraphToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFGraph graph = new RDFGraph();
            RDFTriX.Serialize(graph, stream);

            string fileContent;
            using (StreamReader reader = new StreamReader(new MemoryStream(stream.ToArray())))
                fileContent = reader.ReadToEnd();
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, stream);

            string fileContent;
            using (StreamReader reader = new StreamReader(new MemoryStream(stream.ToArray())))
                fileContent = reader.ReadToEnd();
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldDeserializeEmptyGraphFromFile()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldDeserializeEmptyGraph.trix");
            RDFGraph graph2 = RDFTriX.Deserialize($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldDeserializeEmptyGraph.trix");

            Assert.IsNotNull(graph2);
            Assert.IsTrue(graph2.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphFromFile()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldDeserializeGraph.trix");
            RDFGraph graph2 = RDFTriX.Deserialize($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldDeserializeGraph.trix");

            Assert.IsNotNull(graph2);
            Assert.IsTrue(graph2.TriplesCount == 1);
            Assert.IsTrue(graph2.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleEvenOnMissingXmlDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithMissingTriXDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBadFormedTriXNameDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><trix xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></trix>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBadFormedTriXUriDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
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