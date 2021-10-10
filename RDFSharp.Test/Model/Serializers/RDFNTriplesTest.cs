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

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFNTriplesTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}