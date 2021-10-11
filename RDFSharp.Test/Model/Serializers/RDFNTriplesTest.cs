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
        public void ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj😃/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInSubject.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInSubject.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInSubject.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj\\U0001F603/> <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred😃/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInPredicate.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred\\U0001F603/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj😃/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInObject.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInObject.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingLongUnicodeCharInObject.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj\\U0001F603/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingLongUnicodeCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Smile!😃","en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingLongUnicodeCharInLiteral.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingLongUnicodeCharInLiteral.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingLongUnicodeCharInLiteral.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Smile!\\U0001F603\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/frag#pageβ2"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInSubject.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInSubject.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInSubject.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/frag#page\\u03B22> <http://pred/> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/frag#pageβ2"), new RDFResource("http://obj/")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInPredicate.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/frag#page\\u03B22> <http://obj/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/frag#pageβ2")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInObject.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInObject.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPOTripleHavingShortUnicodeCharInObject.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/frag#page\\u03B22> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingShortUnicodeCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Beta!β", "en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingShortUnicodeCharInLiteral.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingShortUnicodeCharInLiteral.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingShortUnicodeCharInLiteral.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Beta!\\u03B2\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingCarriageReturnCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Return!\r", "en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingCarriageReturnCharInLiteral.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingCarriageReturnCharInLiteral.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingCarriageReturnCharInLiteral.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Return!\\r\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingNewlineCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("NewLine!\n", "en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingNewLineCharInLiteral.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingNewLineCharInLiteral.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingNewLineCharInLiteral.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"NewLine!\\n\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingTabCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Tab!\t", "en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingTabCharInLiteral.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingTabCharInLiteral.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingTabCharInLiteral.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Tab!\\t\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingSlashCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Slash!\\", "en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingSlashCharInLiteral.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingSlashCharInLiteral.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingSlashCharInLiteral.nt");
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Slash!\\\\\"@EN-US .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeGraphWithSPLTripleHavingDoubleQuotesCharInLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("DoubleQuotes!\"", "en-US")));
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingDoubleQuotesCharInLiteral.nt");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingDoubleQuotesCharInLiteral.nt"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeGraphWithSPLTripleHavingDoubleQuotesCharInLiteral.nt");
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

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFNTriplesTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}