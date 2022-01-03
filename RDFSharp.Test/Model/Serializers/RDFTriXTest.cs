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
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeEmptyNamedGraph()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyNamedGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyNamedGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyNamedGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPOTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPOTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPOTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPOTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPOTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPOTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPOTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPBTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPBTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPBTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPBTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPBTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPBTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPBTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPOTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPOTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPOTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPOTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPOTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPOTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPOTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPBTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPBTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPBTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPBTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPBTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPBTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPBTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPLTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLLTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLLTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLLTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPLLTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLLTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLLTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLLTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLTTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLTTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithSPLTTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithSPLTTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLTTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLTTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithSPLTTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPLTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLLTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLLTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLLTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPLLTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLLTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLLTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLLTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://example.org/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLTTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLTTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeGraphWithBPLTTriple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>https://rdfsharp.codeplex.com/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeNamedGraphWithBPLTTriple()
        {
            RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLTTriple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLTTriple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeNamedGraphWithBPLTTriple.trix"));
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
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldDeserializeEmptyGraph.trix"));
            RDFGraph graph2 = RDFTriX.Deserialize(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldDeserializeEmptyGraph.trix"));

            Assert.IsNotNull(graph2);
            Assert.IsTrue(graph2.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphFromFile()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFTriX.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldDeserializeGraph.trix"));
            RDFGraph graph2 = RDFTriX.Deserialize(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldDeserializeGraph.trix"));

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
        public void ShouldDeserializeNamedGraphWithSPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
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

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleEvenOnNullGraphUri()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri/><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleEvenOnMissingGraphUri()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithMultipleGraphUri()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://uri1/</uri><uri>http://uri2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithUnrecognizedGraphDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graf><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graf></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithMultipleGraphDeclarations()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph><graph><uri>http://example2.org/</uri><triple><uri>http://subj2/</uri><uri>http://pred2/</uri><uri>http://obj2/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithMissingTripleDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithBadFormedTripleDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><traple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></traple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><id>_:12345</id></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithSPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><id>bnode:12345</id></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><id>bnode:12345</id><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithBPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><id>bnode:12345</id><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><id>bnode:12345</id><uri>http://pred/</uri><id>_:12345</id></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithBPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><id>bnode:12345</id><uri>http://pred/</uri><id>_:12345</id></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral>hello</plainLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithSPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral>hello</plainLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTripleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral/></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\">hello</plainLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLTripleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\"/></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty, "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithSPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\">hello</plainLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTTripleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#string\"/></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral(string.Empty, RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithSPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTTripleEvenOnUnrecognizedDatatype()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integers\">25</typedLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithSPLTTripleEvenOnUnrecognizedDatatype()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integers\">25</typedLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><id>_:12345</id><uri>http://pred/</uri><plainLiteral>hello</plainLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithBPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><id>_:12345</id><uri>http://pred/</uri><plainLiteral>hello</plainLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><id>_:12345</id><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\">hello</plainLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithBPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><id>_:12345</id><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\">hello</plainLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><id>_:12345</id><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithBPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><id>_:12345</id><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTTripleEvenOnUnrecognizedDatatype()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://rdfsharp.codeplex.com/</uri><triple><id>_:12345</id><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integers\">25</typedLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
        }

        [TestMethod]
        public void ShouldDeserializeNamedGraphWithBPLTTripleEvenOnUnrecognizedDatatype()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>https://example.org/</uri><triple><id>_:12345</id><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integers\">25</typedLiteral></triple></graph></TriX>");
            RDFGraph graph = RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
            Assert.IsFalse(graph.Context.Equals(new Uri("http://example.org/")));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseSubjectIsLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>hello</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseSubjectIsUnrecognizedXML()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><urz>http://subj/</urz><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseSubjectIsEmpty()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri/><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecausePredicateIsLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>hello</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecausePredicateIsBlankNode()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>bnode:12345</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecausePredicateIsUnrecognizedXML()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><urz>http://pred/</urz><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecausePredicateIsEmpty()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri/><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseObjectIsLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>hello</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseObjectIsUnrecognizedXML()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><urz>http://obj/</urz></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPOTripleBecauseObjectIsEmpty()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri/></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphBecauseMissingOneOfSPOLNodes()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPLTTripleBecauseBadFormedLiteral1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">hello</typedLiteral></triple></graph></TriX>");
            Assert.ThrowsException<RDFModelException>(() => RDFTriX.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingGraphWithSPLTTripleBecauseBadFormedLiteral2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral>25</typedLiteral></triple></graph></TriX>");
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