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
using RDFSharp.Store;
using System;
using System.IO;

namespace RDFSharp.Test.Store
{
    [TestClass]
    public class RDFTriGTest
    {
        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyStore()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeEmptyStore.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeEmptyStore.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeEmptyStore.trig"));
            Assert.IsTrue(fileContent.Equals(string.Empty));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithDefaultGraphSPOQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruples.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruples.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruples.trig"));
            Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "<http://subj1/> <http://pred1/> <http://obj1/>. " + Environment.NewLine + "<http://subj2/> <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithDefaultGraphSPBQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("bnode:12345")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("bnode:54321")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPBQuadruples.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPBQuadruples.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPBQuadruples.trig"));
            Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "<http://subj1/> <http://pred1/> _:12345. " + Environment.NewLine + "<http://subj2/> <http://pred2/> _:54321. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithDefaultGraphBPOQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:54321"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPOQuadruples.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPOQuadruples.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPOQuadruples.trig"));
            Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "_:12345 <http://pred1/> <http://obj1/>. " + Environment.NewLine + "_:54321 <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithDefaultGraphBPBQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFResource("bnode:54321")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:54321"), new RDFResource("http://pred2/"), new RDFResource("bnode:12345")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPBQuadruples.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPBQuadruples.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPBQuadruples.trig"));
            Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "_:12345 <http://pred1/> _:54321. " + Environment.NewLine + "_:54321 <http://pred2/> _:12345. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithDefaultGraphSPLQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("hello")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("hello","en-US")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruples.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruples.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruples.trig"));
            Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "<http://subj1/> <http://pred1/> \"hello\". " + Environment.NewLine + "<http://subj2/> <http://pred2/> \"hello\"@EN-US. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithDefaultGraphSPOQuadruplesHavingPrefixes()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), RDFVocabulary.RDFS.SEE_ALSO, new RDFResource("http://obj2/")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruplesHavingPrefixes.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruplesHavingPrefixes.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruplesHavingPrefixes.trig"));
            Assert.IsTrue(fileContent.Equals("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>." + Environment.NewLine + "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>." + Environment.NewLine + Environment.NewLine + "{" + Environment.NewLine + "<http://subj1/> a <http://obj1/>. " + Environment.NewLine + "<http://subj2/> rdfs:seeAlso <http://obj2/>. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithDefaultGraphSPLQuadruplesHavingPrefixes()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("hello")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), RDFVocabulary.RDFS.SEE_ALSO, new RDFPlainLiteral("hello","en-US")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruplesHavingPrefixes.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruplesHavingPrefixes.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruplesHavingPrefixes.trig"));
            Assert.IsTrue(fileContent.Equals("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>."+ Environment.NewLine + "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>." + Environment.NewLine + "@prefix xsd: <http://www.w3.org/2001/XMLSchema#>." + Environment.NewLine + Environment.NewLine + "{" + Environment.NewLine + "<http://subj1/> a \"hello\". " + Environment.NewLine + "<http://subj2/> <http://pred2/> \"25\"^^xsd:integer; " + Environment.NewLine + "                rdfs:seeAlso \"hello\"@EN-US. " + Environment.NewLine + "}" + Environment.NewLine + ""));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruples.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruples.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruples.trig"));
            Assert.IsTrue(fileContent.Equals("{"+ Environment.NewLine + "<http://subj1/> <http://pred1/> <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + "GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "<http://subj2/> <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruplesHavingPrefixes()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), RDFVocabulary.RDFS.SEE_ALSO, new RDFResource("http://obj2/")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig"));
            Assert.IsTrue(fileContent.Equals("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>."+ Environment.NewLine + "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>." + Environment.NewLine + Environment.NewLine + "{" + Environment.NewLine + "<http://subj1/> a <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + "GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "<http://subj2/> rdfs:seeAlso <http://obj2/>. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruples.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruples.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruples.trig"));
            Assert.IsTrue(fileContent.Equals("GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "<http://subj1/> <http://pred1/> <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + "GRAPH <http://ctx2/>" + Environment.NewLine + "{" + Environment.NewLine + "<http://subj2/> <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruplesHavingPrefixes()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj2/"), RDFVocabulary.RDFS.SEE_ALSO, new RDFResource("http://obj2/")));
            RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig"));
            Assert.IsTrue(fileContent.Equals("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>."+ Environment.NewLine + "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>." + Environment.NewLine + Environment.NewLine + "GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "<http://subj1/> a <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + "GRAPH <http://ctx2/>" + Environment.NewLine + "{" + Environment.NewLine + "<http://subj2/> rdfs:seeAlso <http://obj2/>. " + Environment.NewLine + "}" + Environment.NewLine));
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFTriGTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}