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

using RDFSharp.Model;
using RDFSharp.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace RDFSharp.Test.Store
{
    [TestClass]
    public class RDFNQuadsTest
    {
        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyStore()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeEmptyStore.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeEmptyStore.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeEmptyStore.nq"));
            Assert.IsTrue(fileContent.Equals(string.Empty));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPBQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("bnode:12345")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPBQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPBQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPBQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> _:12345 <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPOQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"),new RDFResource("http://pred/"),new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPOQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPOQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPOQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"_:12345 <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPBQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"),new RDFResource("http://pred/"),new RDFResource("bnode:12345")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPBQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPBQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPBQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"_:12345 <http://pred/> _:12345 <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("hello")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"hello\" <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLLQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("hello","en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLLQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLLQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLLQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"hello\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLTQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLTQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLTQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLTQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"25\"^^<{RDFVocabulary.XSD.INTEGER}> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"),new RDFResource("http://pred/"),new RDFPlainLiteral("hello")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"_:12345 <http://pred/> \"hello\" <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLLQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"),new RDFResource("http://pred/"),new RDFPlainLiteral("hello","en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLLQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLLQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLLQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"_:12345 <http://pred/> \"hello\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLTQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"),new RDFResource("http://pred/"),new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLTQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLTQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLTQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"_:12345 <http://pred/> \"25\"^^<{RDFVocabulary.XSD.INTEGER}> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFNQuadsTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}