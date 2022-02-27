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
    public class RDFTriXTest
    {
        #region Tests
        [TestMethod]
        public void ShouldSerializeEmptyStore()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyStore.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyStore.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeEmptyStore.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\" />"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPOQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPOQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPOQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPBQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPBQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPBQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPBQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPOQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPOQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPOQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPOQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPBQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPBQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPBQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPBQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLLQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLLQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLLQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLLQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLTQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLTQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLTQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLTQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLLQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLLQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLLQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLLQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLTQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLTQuadruple.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLTQuadruple.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLTQuadruple.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx2/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPOQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPOQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPOQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPOQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj1/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj2/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPBQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPBQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPBQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPBQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj1/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj2/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPOQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPOQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPOQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPOQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:54321</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPBQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFResource("bnode:12345")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPBQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPBQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPBQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:54321</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj1/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj2/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLLQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLLQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLLQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLLQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj1/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj2/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCSPLTQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLTQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLTQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCSPLTQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj1/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj2/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:54321</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral>hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLLQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLLQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLLQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLLQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:54321</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <plainLiteral xml:lang=\"EN-US\">hello</plainLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeStoreWithCBPLTQuadruplesOnSameGraph()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:54321"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLTQuadruplesOnSameGraph.trix"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLTQuadruplesOnSameGraph.trix")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldSerializeStoreWithCBPLTQuadruplesOnSameGraph.trix"));
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx1/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:12345</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}    <triple>{Environment.NewLine}      <id>bnode:54321</id>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldSerializeEmptyStoreToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFMemoryStore store = new RDFMemoryStore();
            RDFSharp.Store.RDFTriX.Serialize(store, stream);

            string fileContent;
            using (StreamReader reader = new StreamReader(new MemoryStream(stream.ToArray())))
                fileContent = reader.ReadToEnd();
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\" />"));
        }

        [TestMethod]
        public void ShouldSerializeStoreToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFSharp.Store.RDFTriX.Serialize(store, stream);

            string fileContent;
            using (StreamReader reader = new StreamReader(new MemoryStream(stream.ToArray())))
                fileContent = reader.ReadToEnd();
            Assert.IsTrue(fileContent.Equals($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\">{Environment.NewLine}  <graph>{Environment.NewLine}    <uri>http://ctx/</uri>{Environment.NewLine}    <triple>{Environment.NewLine}      <uri>http://subj/</uri>{Environment.NewLine}      <uri>http://pred/</uri>{Environment.NewLine}      <uri>http://obj/</uri>{Environment.NewLine}    </triple>{Environment.NewLine}  </graph>{Environment.NewLine}</TriX>"));
        }

        [TestMethod]
        public void ShouldDeserializeEmptyStoreFromFile()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldDeserializeEmptyStore.trix"));
            RDFMemoryStore store2 = RDFSharp.Store.RDFTriX.Deserialize(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldDeserializeEmptyStore.trix"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeStoreFromFile()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFSharp.Store.RDFTriX.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldDeserializeStore.trix"));
            RDFMemoryStore store2 = RDFSharp.Store.RDFTriX.Deserialize(Path.Combine(Environment.CurrentDirectory, $"RDFTriXTest_ShouldDeserializeStore.trix"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadruplesEvenOnMissingXmlDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadruplesOnSameGraph()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx/</uri><triple><uri>http://subj1/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple><triple><uri>http://subj2/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj1/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadruplesOnSameGraphEvenOnMissingXmlDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx/</uri><triple><uri>http://subj1/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple><triple><uri>http://subj2/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj1/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithMissingTriXDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><graph><uri>http://ctx/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithBadFormedTriXNameDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><trix xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></trix>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithBadFormedTriXUriDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03\"><graph><uri>http://ctx/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPOQuadruplesEvenOnMissingStoreUri()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><triple><uri>http://subj1/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph><graph><triple><uri>http://subj2/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithMultipleStoreUri()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://uri1/</uri><uri>http://uri2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithUnrecognizedStoreDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graf><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graf></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithMissingTripleDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithBadFormedTripleDeclaration()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><traple><uri>http://subj/</uri><uri>http://pred/</uri><uri>http://obj/</uri></traple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPBQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><id>_:12345</id></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><id>_:12345</id></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPOQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><id>bnode:12345</id><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph><graph><uri>http://ctx2/</uri><triple><id>bnode:12345</id><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPBQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><id>bnode:12345</id><uri>http://pred/</uri><id>_:12345</id></triple></graph><graph><uri>http://ctx2/</uri><triple><id>bnode:12345</id><uri>http://pred/</uri><id>_:12345</id></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFResource("bnode:12345"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral>hello</plainLiteral></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral>hello</plainLiteral></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral/></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral/></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLLQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\">hello</plainLiteral></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\">hello</plainLiteral></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLLQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\"/></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\"/></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty, "en-US"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral(string.Empty, "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLTQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLTQuadrupleEvenIfEmptyLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#string\"/></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#string\"/></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral(string.Empty, RDFModelEnums.RDFDatatypes.XSD_STRING))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral(string.Empty, RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCSPLTQuadrupleEvenOnUnrecognizedDatatype()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integers\">25</typedLiteral></triple></graph><graph><uri>http://ctx2/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integers\">25</typedLiteral></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><id>_:12345</id><uri>http://pred/</uri><plainLiteral>hello</plainLiteral></triple></graph><graph><uri>http://ctx2/</uri><triple><id>_:12345</id><uri>http://pred/</uri><plainLiteral>hello</plainLiteral></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLLQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><id>_:12345</id><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\">hello</plainLiteral></triple></graph><graph><uri>http://ctx2/</uri><triple><id>_:12345</id><uri>http://pred/</uri><plainLiteral xml:lang=\"en-US\">hello</plainLiteral></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "en-US"))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLTQuadruples()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><id>_:12345</id><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral></triple></graph><graph><uri>http://ctx2/</uri><triple><id>_:12345</id><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">25</typedLiteral></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldDeserializeStoreWithCBPLTQuadrupleEvenOnUnrecognizedDatatype()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://ctx1/</uri><triple><id>_:12345</id><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integers\">25</typedLiteral></triple></graph><graph><uri>http://ctx2/</uri><triple><id>_:12345</id><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integers\">25</typedLiteral></triple></graph></TriX>");
            RDFMemoryStore store = RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecauseSubjectIsLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>hello</uri><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecauseSubjectIsUnrecognizedXML()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><urz>http://subj/</urz><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecauseSubjectIsEmpty()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri/><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecausePredicateIsLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>hello</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecausePredicateIsBlankNode()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>bnode:12345</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecausePredicateIsUnrecognizedXML()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><urz>http://pred/</urz><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecausePredicateIsEmpty()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri/><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecauseObjectIsLiteral()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri>hello</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecauseObjectIsUnrecognizedXML()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><urz>http://obj/</urz></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPOQuadrupleBecauseObjectIsEmpty()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><uri/></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreBecauseMissingOneOfSPOLNodes()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://pred/</uri><uri>http://obj/</uri></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPLTQuadrupleBecauseBadFormedLiteral1()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral datatype=\"http://www.w3.org/2001/XMLSchema#integer\">hello</typedLiteral></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingStoreWithCSPLTQuadrupleBecauseBadFormedLiteral2()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><TriX xmlns=\"http://www.w3.org/2004/03/trix/trix-1/\"><graph><uri>http://example.org/</uri><triple><uri>http://subj/</uri><uri>http://pred/</uri><typedLiteral>25</typedLiteral></triple></graph></TriX>");
            Assert.ThrowsException<RDFStoreException>(() => RDFSharp.Store.RDFTriX.Deserialize(new MemoryStream(stream.ToArray())));
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