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
    public class RDFNTriplesTest
    {
        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyGraph()
        {
            RDFGraph graph = new RDFGraph();
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeEmptyGraph.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeEmptyGraph.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeEmptyGraph.nt"));
            Assert.IsTrue(fileContent.Equals(string.Empty));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPBTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPBTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPBTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> _:12345 .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPOTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPOTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPOTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"_:12345 <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPBTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPBTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPBTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"_:54321 <http://pred/> _:12345 .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"hello\" .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello","en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLLTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLLTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLLTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"hello\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"25\"^^<{RDFVocabulary.XSD.INTEGER}> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"_:54321 <http://pred/> \"hello\" .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLLTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLLTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLLTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"_:54321 <http://pred/> \"hello\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLTTriple.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLTTriple.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithBPLTTriple.nt"));
            Assert.IsTrue(fileContent.Equals($"_:54321 <http://pred/> \"25\"^^<{RDFVocabulary.XSD.INTEGER}> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj😃/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInSubject.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInSubject.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInSubject.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj\\U0001F603/> <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred😃/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred\\U0001F603/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj😃/")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInObject.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInObject.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInObject.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj\\U0001F603/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingLongUnicodeCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Smile!😃","en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingLongUnicodeCharInLiteral.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingLongUnicodeCharInLiteral.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingLongUnicodeCharInLiteral.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Smile!\\U0001F603\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/frag#pageβ2"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInSubject.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInSubject.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInSubject.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/frag#page\\u03B22> <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/frag#pageβ2"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/frag#page\\u03B22> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/frag#pageβ2")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInObject.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInObject.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInObject.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/frag#page\\u03B22> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingShortUnicodeCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Beta!β", "en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingShortUnicodeCharInLiteral.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingShortUnicodeCharInLiteral.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingShortUnicodeCharInLiteral.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Beta!\\u03B2\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingCarriageReturnCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Return!\r", "en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingCarriageReturnCharInLiteral.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingCarriageReturnCharInLiteral.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingCarriageReturnCharInLiteral.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Return!\\r\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingNewlineCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("NewLine!\n", "en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingNewLineCharInLiteral.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingNewLineCharInLiteral.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingNewLineCharInLiteral.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"NewLine!\\n\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingTabCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Tab!\t", "en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingTabCharInLiteral.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingTabCharInLiteral.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingTabCharInLiteral.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Tab!\\t\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingSlashCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Slash!\\", "en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingSlashCharInLiteral.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingSlashCharInLiteral.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingSlashCharInLiteral.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Slash!\\\\\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingDoubleQuotesCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("DoubleQuotes!\"", "en-US")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingDoubleQuotesCharInLiteral.nt"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingDoubleQuotesCharInLiteral.nt")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingDoubleQuotesCharInLiteral.nt"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"DoubleQuotes!\\\"\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeEmptyGraphToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFGraph graph = new RDFGraph();
            RDFNTriples.Serialize(graph, stream);

            string fileContent;
            using (StreamReader reader = new StreamReader(new MemoryStream(stream.ToArray())))
                fileContent = reader.ReadToEnd();
            Assert.IsTrue(fileContent.Equals(string.Empty));
        }

        [TestMethod]
        public void ShouldSerializeGraphToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, stream);

            string fileContent;
            using (StreamReader reader = new StreamReader(new MemoryStream(stream.ToArray())))
                fileContent = reader.ReadToEnd();
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldDeserializeEmptyGraphFromFile()
        {
            RDFGraph graph = new RDFGraph();
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldDeserializeEmptyGraph.nt"));
            RDFGraph graph2 = RDFNTriples.Deserialize(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldDeserializeEmptyGraph.nt"));

            Assert.IsNotNull(graph2);
            Assert.IsTrue(graph2.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphFromFile()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldDeserializeGraph.nt"));
            RDFGraph graph2 = RDFNTriples.Deserialize(Path.Combine(Environment.CurrentDirectory, $"RDFNTriplesTest_ShouldDeserializeGraph.nt"));

            Assert.IsNotNull(graph2);
            Assert.IsTrue(graph2.TriplesCount == 1);
            Assert.IsTrue(graph2.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedSPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> <http://obj/> .");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> _:12345 .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedSPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> _:12345 .");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj/> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedBPOTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> <http://obj/> .");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> _:12345 .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedBPBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> _:12345 .");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"hello\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTripleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedSPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"hello\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"hello\"@en-US .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLLTripleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\"@en-US .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty, "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedSPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"hello\"@en-US .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTTripleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\"^^<http://www.w3.org/2001/XMLSchema#string> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral(string.Empty, RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedSPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedBPLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"hello\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"@en-US .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedBPLLTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"hello\"@en-US .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithBPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithCommentedBPLTTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPOTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"http://subj <http://pred/> <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPOTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> http://pred/ <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPOTripleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <_:12345> <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPOTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> http://obj .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLTripleBecauseBadFormedPLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLTripleBecauseBadFormedPLLiteral1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"@EN@US .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLTripleBecauseBadFormedPLLiteral2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"@ .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLTripleBecauseBadFormedTLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"^^ .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLTripleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"http://subj <http://pred/> \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> http://pred/ \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLTripleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <_:12345> \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBadFormedSTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAttachedSTriples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> <http://obj/> . <http://subj> <http://pred/> <http://obj/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPOTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> http://pred/ <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPOTripleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> <_:12345> <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPOTripleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> <http://pred/> http://obj .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLTripleBecauseBadFormedPLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> <http://pred/> \"hello .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLTripleBecauseBadFormedPLLiteral1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> <http://pred/> \"hello\"@EN@US .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLTripleBecauseBadFormedPLLiteral2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> <http://pred/> \"hello\"@ .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLTripleBecauseBadFormedTLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> <http://pred/> \"hello\"^^ .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLTripleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> http://pred/ \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLTripleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<_:12345> <_:12345> \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBadFormedBTriple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAttachedBTriples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj/> . _:12345 <http://pred/> <http://obj/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingLongUnicodeCharInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj\\U0001F603/> <http://pred/> <http://obj/> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj😃/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred\\U0001F603/> <http://obj/> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred😃/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingLongUnicodeCharInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj\\U0001F603/> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj😃/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingLongUnicodeCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Smile!\\U0001F603\"@EN-US .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Smile!😃", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingShortUnicodeCharInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/frag#page\\u03B22> <http://pred/> <http://obj/> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/frag#pageβ2"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/frag#page\\u03B22> <http://obj/> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/frag#pageβ2"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingShortUnicodeCharInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/frag#page\\u03B22> .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/frag#pageβ2"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingShortUnicodeCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Beta!\\u03B2\"@EN-US .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Beta!β", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingCarriageReturnCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\r\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\r"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingNewLineCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\n\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\n"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingTabCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\t\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\t"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingSlashCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\\\\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\\"))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPOTripleHavingDoubleQuotesCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\\"\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\""))));
        }

        [TestMethod]
        public void ShouldDeserializeGraphWithSPLTripleHavingTrickyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\\" \\\"\" .{Environment.NewLine}");
            RDFGraph graph = RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null);

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\" \""))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingTrickySPOTriple1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> ..{Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingTrickySPOTriple2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> {Environment.NewLine}");

            Assert.ThrowsException<RDFModelException>(() => RDFNTriples.Deserialize(new MemoryStream(stream.ToArray()), null));
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFNTriplesTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}