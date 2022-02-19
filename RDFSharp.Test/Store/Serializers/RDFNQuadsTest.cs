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
        public void ShouldSerializeStoreWithCSPOTriple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")));
            RDFNQuads.Serialize(store, Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOTriple.nq"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOTriple.nq")));
            string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFNQuadsTest_ShouldSerializeStoreWithCSPOTriple.nq"));
            Assert.IsTrue(fileContent.Equals($"<http://subj/> <http://pred/> <http://obj/> <http://ctx/> .{Environment.NewLine}"));
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