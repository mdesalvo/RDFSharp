using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDFSharp.Test
{
    [TestClass]
    public class RDFNTriplesTest
    {
        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyGraph()
        {
            RDFGraph graph = new RDFGraph();
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeEmptyGraph.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeEmptyGraph.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeEmptyGraph.nt");
            Assert.IsTrue(fileContent.Equals(string.Empty));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTriple.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPBTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPBTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPBTriple.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> _:12345 .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPOTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPOTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPOTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPOTriple.nt");
            Assert.IsTrue(fileContent.Equals($"_:12345 <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPBTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPBTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPBTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPBTriple.nt");
            Assert.IsTrue(fileContent.Equals($"_:54321 <http://pred/> _:12345 .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTriple.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"hello\" .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello","en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLLTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLLTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLLTriple.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"hello\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTTriple.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"25\"^^<{RDFVocabulary.XSD.INTEGER}> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLTriple.nt");
            Assert.IsTrue(fileContent.Equals($"_:54321 <http://pred/> \"hello\" .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLLTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLLTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLLTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLLTriple.nt");
            Assert.IsTrue(fileContent.Equals($"_:54321 <http://pred/> \"hello\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithBPLTTriple()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLTTriple.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLTTriple.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithBPLTTriple.nt");
            Assert.IsTrue(fileContent.Equals($"_:54321 <http://pred/> \"25\"^^<{RDFVocabulary.XSD.INTEGER}> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj😃/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInSubject.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInSubject.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInSubject.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj\\U0001F603/> <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred😃/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInPredicate.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInPredicate.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInPredicate.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred\\U0001F603/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj😃/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInObject.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInObject.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingUnicodeCharInObject.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj\\U0001F603/> .{Environment.NewLine}"));
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