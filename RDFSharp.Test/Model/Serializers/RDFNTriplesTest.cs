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
            Assert.IsTrue(File.ReadAllText($"{Environment.CurrentDirectory}\\RDFNTriplesTest_ShouldSerializeEmptyGraph.nt").Length == 0);
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