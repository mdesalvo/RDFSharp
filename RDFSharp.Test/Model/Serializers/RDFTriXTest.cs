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
            RDFNTriples.Serialize(graph, $"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyGraph.trix");

            Assert.IsTrue(File.Exists($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyGraph.trix"));
            string fileContent = File.ReadAllText($"{Environment.CurrentDirectory}\\RDFTriXTest_ShouldSerializeEmptyGraph.trix");
            Assert.IsTrue(fileContent.Equals(string.Empty));
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