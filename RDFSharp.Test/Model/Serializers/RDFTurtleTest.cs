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

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFTurtleTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}