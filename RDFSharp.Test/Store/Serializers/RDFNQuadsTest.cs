/*
   Copyright 2012-2025 Marco De Salvo

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
        public void ShouldSerializeStoreWithCSPLLDirectionedQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("hello","en-US--ltr")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLLDirectionedQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLLDirectionedQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLLDirectionedQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"hello\"@EN-US--LTR <http://ctx/> .{Environment.NewLine}"));
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
        public void ShouldSerializeStoreWithCBPLLDirectionedQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"),new RDFResource("http://pred/"),new RDFPlainLiteral("hello","en--rtl")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLLDirectionedQuadruple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLLDirectionedQuadruple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCBPLLDirectionedQuadruple.nq"));
            Assert.IsTrue(fileContent.Equals($"_:12345 <http://pred/> \"hello\"@EN--RTL <http://ctx/> .{Environment.NewLine}"));
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

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInContext()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx😃/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInContext.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInContext.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInContext.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/> <http://ctx\\U0001F603/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInSubject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj😃/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInSubject.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInSubject.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInSubject.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj\\U0001F603/> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred😃/"), new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInPredicate.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInPredicate.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInPredicate.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred\\U0001F603/> <http://obj/> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj😃/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInObject.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInObject.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInObject.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj\\U0001F603/> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadrupleHavingLongUnicodeCharInLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Smile!😃","en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingLongUnicodeCharInLiteral.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingLongUnicodeCharInLiteral.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingLongUnicodeCharInLiteral.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Smile!\\U0001F603\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

                [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInContext()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/frag#pageβ2"), new RDFResource("http://subj"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInContext.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInContext.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInContext.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/> <http://ctx/frag#page\\u03B22> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInSubject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/frag#pageβ2"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInSubject.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInSubject.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInSubject.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/frag#page\\u03B22> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/frag#pageβ2"), new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInPredicate.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInPredicate.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInPredicate.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/frag#page\\u03B22> <http://obj/> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/frag#pageβ2")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInObject.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInObject.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInObject.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/frag#page\\u03B22> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadrupleHavingShortUnicodeCharInLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Beta!β", "en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingShortUnicodeCharInLiteral.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingShortUnicodeCharInLiteral.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingShortUnicodeCharInLiteral.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Beta!\\u03B2\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadrupleHavingCarriageReturnCharInLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Return!\r", "en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingCarriageReturnCharInLiteral.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingCarriageReturnCharInLiteral.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingCarriageReturnCharInLiteral.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Return!\\r\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadrupleHavingNewlineCharInLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("NewLine!\n", "en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingNewLineCharInLiteral.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingNewLineCharInLiteral.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingNewLineCharInLiteral.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"NewLine!\\n\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadrupleHavingTabCharInLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Tab!\t", "en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingTabCharInLiteral.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingTabCharInLiteral.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingTabCharInLiteral.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Tab!\\t\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadrupleHavingSlashCharInLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Slash!\\", "en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingSlashCharInLiteral.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingSlashCharInLiteral.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingSlashCharInLiteral.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"Slash!\\\\\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadrupleHavingDoubleQuotesCharInLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("DoubleQuotes!\"", "en-US")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingDoubleQuotesCharInLiteral.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingDoubleQuotesCharInLiteral.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPLQuadrupleHavingDoubleQuotesCharInLiteral.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> \"DoubleQuotes!\\\"\"@EN-US <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldSerializeEmptyStoreToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFMemoryStore store = new RDFMemoryStore();
            RDFNQuads.Serialize(store, stream);

            string fileContent;
            using (StreamReader reader = new StreamReader(new MemoryStream(stream.ToArray())))
                fileContent = reader.ReadToEnd();
            Assert.IsTrue(fileContent.Equals(string.Empty));
        }

        [TestMethod]
        public void ShouldSerializeStoreToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, stream);

            string fileContent;
            using (StreamReader reader = new StreamReader(new MemoryStream(stream.ToArray())))
                fileContent = reader.ReadToEnd();
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}"));
        }

        [TestMethod]
        public void ShouldDeserializeEmptyStoreFromFile()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldDeserializeEmptyStore.nq"));
            RDFMemoryStore store2 = RDFNQuads.Deserialize(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldDeserializeEmptyStore.nq"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreFromFile()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldDeserializeStore.nq"));
            RDFMemoryStore store2 = RDFNQuads.Deserialize(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldDeserializeStore.nq"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCSPOQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> <http://obj/> <http://ctx/> .");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> _:12345 <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCSPBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> _:12345 <http://ctx/> .");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPOQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCBPOQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> <http://obj/> <http://ctx/> .");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> _:12345 <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCBPBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> _:12345 <http://ctx/> .");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"hello\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), RDFPlainLiteral.Empty)));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCSPLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"hello\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"hello\"@en-US <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\"@en-US <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty, "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCSPLLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"hello\"@en-US <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLLDirectionedQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"hello\"@en-US--ltr <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US--ltr"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLTQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\"^^<http://www.w3.org/2001/XMLSchema#string> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral(string.Empty, RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCSPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCustomDatatypeCSPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#testdt> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), RDFPlainLiteral.Empty)));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCBPLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"hello\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"@en-US <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"\"@en-US <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty, "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCBPLLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"hello\"@en-US <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLLDirectionedQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"@en--rtl <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en--rtl"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLTQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"\"^^<http://www.w3.org/2001/XMLSchema#string> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral(string.Empty, RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedCBPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPOQuadrupleBecauseBadFormedContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> <http://obj> http://ctx/ .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPOQuadrupleBecauseBlankContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> <http://obj> _:12345 .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPOQuadrupleBecauseLiteralContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> <http://obj> \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPOQuadrupleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"http://subj <http://pred/> <http://obj> <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPOQuadrupleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> http://pred/ <http://obj> <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPOQuadrupleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> _:12345 <http://obj> <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPOQuadrupleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> http://obj <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBadFormedPLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBadFormedPLLiteral1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"@EN@US <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBadFormedPLLiteral2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"@ <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBadFormedTLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"^^ <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBadFormedContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\" http://ctx/ .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBlankContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\" _:12345 .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"http://subj <http://pred/> \"hello\" <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> http://pred/ \"hello\" <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCSPLQuadrupleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> _:12345 \"hello\" <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBadFormedSQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAttachedSQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> <http://obj/> <http://ctx/> . <http://subj> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPOQuadrupleBecauseBadFormedContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj> http://ctx/ .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPOQuadrupleBecauseBlankContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj> _:12345 .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPOQuadrupleBecauseLiteralContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj> \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPOQuadrupleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:< <http://pred/> <http://obj> <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPOQuadrupleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 http://pred/ <http://obj> <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPOQuadrupleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 _:12345 <http://obj> <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPOQuadrupleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> http://obj <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBadFormedPLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBadFormedPLLiteral1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"@EN@US <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBadFormedPLLiteral2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"@ <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBadFormedTLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"^^ <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBadFormedContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\" http://ctx/ .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBlankContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\" _:12345 .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:< <http://pred/> \"hello\" <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 http://pred/ \"hello\" <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingCBPLQuadrupleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 _:12345 \"hello\" <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBadFormedBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAttachedBQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj/> <http://ctx/> . _:12345 <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> <http://ctx\\U0001F603/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx😃/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj\\U0001F603/> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj😃/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred\\U0001F603/> <http://obj/> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred😃/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj\\U0001F603/> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj😃/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingLongUnicodeCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Smile!\\U0001F603\"@EN-US <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Smile!😃", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> <http://ctx/frag#page\\u03B22> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/frag#pageβ2"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/frag#page\\u03B22> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/frag#pageβ2"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/frag#page\\u03B22> <http://obj/> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/frag#pageβ2"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/frag#page\\u03B22> <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/frag#pageβ2"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingShortUnicodeCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Beta!\\u03B2\"@EN-US <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Beta!β", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingCarriageReturnCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\r\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\r"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingNewLineCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\n\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\n"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingTabCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\t\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\t"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingSlashCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\\\\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\\"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadrupleHavingDoubleQuotesCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\\"\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\""))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedSPOQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> <http://obj/> .");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> _:12345 .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedSPBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> _:12345 .");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithBPOQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedBPOQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> <http://obj/> .");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithBPBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> _:12345 .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedBPBQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> _:12345 .");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"hello\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), RDFPlainLiteral.Empty)));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedSPLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"hello\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPLLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"hello\"@en-US .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPLLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\"@en-US .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty, "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedSPLLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"hello\"@en-US .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPLTQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"\"^^<http://www.w3.org/2001/XMLSchema#string> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral(string.Empty, RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedSPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#<http://subj/> <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithBPLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithBPLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), RDFPlainLiteral.Empty)));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedBPLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"hello\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithBPLLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"@en-US .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithBPLLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"\"@en-US .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty, "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedBPLLQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"hello\"@en-US .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithBPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithBPLTQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"\"^^<http://www.w3.org/2001/XMLSchema#string> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral(string.Empty, RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCommentedBPLTQuadruple()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"#_:12345 <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPOQuadrupleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"http://subj <http://pred/> <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPOQuadrupleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> http://pred/ <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPOQuadrupleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> _:12345 <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPOQuadrupleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> http://obj .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLQuadrupleBecauseBadFormedPLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLQuadrupleBecauseBadFormedPLLiteral1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"@EN@US .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLQuadrupleBecauseBadFormedPLLiteral2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"@ .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLQuadrupleBecauseBadFormedTLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> \"hello\"^^ .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLQuadrupleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"http://subj <http://pred/> \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLQuadrupleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> http://pred/ \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingSPLQuadrupleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> _:12345 \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAttachedSQuadruplesWithoutContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj> <http://pred/> <http://obj/> . <http://subj> <http://pred/> <http://obj/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPOQuadrupleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:< <http://pred/> <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPOQuadrupleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 http://pred/ <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPOQuadrupleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 _:12345 <http://obj> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPOQuadrupleBecauseBadFormedObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> http://obj .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLQuadrupleBecauseBadFormedPLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLQuadrupleBecauseBadFormedPLLiteral1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"@EN@US .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLQuadrupleBecauseBadFormedPLLiteral2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"@ .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLQuadrupleBecauseBadFormedTLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> \"hello\"^^ .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLQuadrupleBecauseBadFormedSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:< <http://pred/> \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLQuadrupleBecauseBadFormedPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 http://pred/ \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingBPLQuadrupleBecauseBlankPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 _:12345 \"hello\" .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAttachedBQuadruplesWithoutContext()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"_:12345 <http://pred/> <http://obj/> . _:12345 <http://pred/> <http://obj/> .{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingLongUnicodeCharInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj\\U0001F603/> <http://pred/> <http://obj/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj😃/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingLongUnicodeCharInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred\\U0001F603/> <http://obj/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred😃/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingLongUnicodeCharInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj\\U0001F603/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj😃/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingLongUnicodeCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Smile!\\U0001F603\"@EN-US .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Smile!😃", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingShortUnicodeCharInSubject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/frag#page\\u03B22> <http://pred/> <http://obj/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/frag#pageβ2"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingShortUnicodeCharInPredicate()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/frag#page\\u03B22> <http://obj/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/frag#pageβ2"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingShortUnicodeCharInObject()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/frag#page\\u03B22> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/frag#pageβ2"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingShortUnicodeCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Beta!\\u03B2\"@EN-US .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Beta!β", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingCarriageReturnCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\r\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\r"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingNewLineCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\n\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\n"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingTabCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\t\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\t"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingSlashCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\\\\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\\"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithSPOQuadrupleHavingDoubleQuotesCharInLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\\"\" .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\""))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLQuadrupleHavingTrickyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> \"Hello\\\" \\\"\" <http://ctx/> .{Environment.NewLine}");
            RDFMemoryStore store = RDFNQuads.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("Hello\" \""))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingTrickySPOQuadruple1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> <http://ctx/> ..{Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingTrickySPOQuadruple2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine($"<http://subj/> <http://pred/> <http://obj/> <http://ctx/> {Environment.NewLine}");

            Assert.ThrowsException<RDFStoreException>(() => RDFNQuads.Deserialize(new MemoryStream(stream.ToArray())));
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