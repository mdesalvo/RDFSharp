/*
   Copyright 2012-2024 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace RDFSharp.Test.Store
{
    [TestClass]
    public class RDFAsyncStoreTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateAsyncStore()
        {
            RDFAsyncStore store = new RDFAsyncStore();

            Assert.IsNotNull(store);
            Assert.IsTrue(store.WrappedStore.StoreType.Equals("MEMORY"));
            Assert.IsTrue(store.WrappedStore.StoreID.Equals(RDFModelUtilities.CreateHash(store.ToString())));
            Assert.IsTrue(store.ToString().Equals(store.WrappedStore.ToString()));
        }

        [TestMethod]
        public void ShouldCreateAsyncStoreFromStore()
        {
            RDFAsyncStore store = new RDFAsyncStore(new RDFMemoryStore(
                new List<RDFQuadruple>() {
                    new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o"))
                }));

            Assert.IsNotNull(store);
            Assert.IsTrue(store.WrappedStore.StoreType.Equals("MEMORY"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(store.WrappedStore.StoreID.Equals(RDFModelUtilities.CreateHash(store.ToString())));
            Assert.IsTrue(store.ToString().Equals(store.WrappedStore.ToString()));

            int i = 0;
            foreach (RDFQuadruple quad in (RDFMemoryStore)store.WrappedStore) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFQuadruple> quads = ((RDFMemoryStore)store.WrappedStore).QuadruplesEnumerator;
            while (quads.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldEqualsAsyncStores()
        {
            RDFAsyncStore store1 = new RDFAsyncStore();
            Assert.IsTrue(store1.Equals(store1));
            Assert.IsFalse(store1.Equals(null));

            RDFAsyncStore store2 = new RDFAsyncStore();
            Assert.IsFalse(store1.Equals(store2));
        }

        [TestMethod]
        public async Task ShouldMergeGraphAsync()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>() {
                new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")),
                new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))
            }).SetContext(new Uri("ex:ctx"));
            RDFAsyncStore store = new RDFAsyncStore(new RDFMemoryStore());
            await store.MergeGraphAsync(graph);

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 2);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

            await store.MergeGraphAsync(null as RDFAsyncGraph);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldAddQuadrupleAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.AddQuadrupleAsync(null);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadrupleAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadrupleAsync(null);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextAsync(new RDFContext("ex:ctx1"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextAsync(new RDFContext("ex:ctx1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextAsync(null);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesBySubjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesBySubjectAsync(new RDFResource("ex:subj1"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesBySubjectAsync(new RDFResource("ex:subj1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesBySubjectAsync(null);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByPredicateAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByPredicateAsync(new RDFResource("ex:pred1"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByPredicateAsync(new RDFResource("ex:pred1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByPredicateAsync(null);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByObjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj1")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj2")));
            await store.RemoveQuadruplesByObjectAsync(new RDFResource("ex:obj1"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj2"))));

            await store.RemoveQuadruplesByObjectAsync(new RDFResource("ex:obj1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByObjectAsync(null);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByLiteralAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            await store.RemoveQuadruplesByLiteralAsync(new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US"))));

            await store.RemoveQuadruplesByLiteralAsync(new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByLiteralAsync(null);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextSubjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextSubjectAsync(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextSubjectAsync(new RDFContext("ex:ctx2"), new RDFResource("ex:subj1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextSubjectAsync(null, new RDFResource("ex:subj"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextPredicateAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextPredicateAsync(new RDFContext("ex:ctx1"), new RDFResource("ex:pred"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextPredicateAsync(new RDFContext("ex:ctx2"), new RDFResource("ex:pred11"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextPredicateAsync(null, new RDFResource("ex:pred"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextObjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextObjectAsync(new RDFContext("ex:ctx1"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextObjectAsync(new RDFContext("ex:ctx2"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextObjectAsync(null, new RDFResource("ex:obj"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextLiteralAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextLiteralAsync(new RDFContext("ex:ctx1"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextLiteralAsync(new RDFContext("ex:ctx2"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextLiteralAsync(null, new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextSubjectPredicateAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextSubjectPredicateAsync(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextSubjectPredicateAsync(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextSubjectPredicateAsync(null, new RDFResource("ex:subj"), new RDFResource("ex:pred"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextSubjectObjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextSubjectObjectAsync(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextSubjectObjectAsync(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextSubjectObjectAsync(null, new RDFResource("ex:subj"), new RDFResource("ex:obj"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextSubjectLiteralAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextSubjectLiteralAsync(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextSubjectLiteralAsync(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextSubjectLiteralAsync(null, new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextPredicateObjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextPredicateObjectAsync(new RDFContext("ex:ctx1"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextPredicateObjectAsync(new RDFContext("ex:ctx2"), new RDFResource("ex:pred"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextPredicateObjectAsync(null, new RDFResource("ex:pred"), new RDFResource("ex:obj"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByContextPredicateLiteralAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByContextPredicateLiteralAsync(new RDFContext("ex:ctx1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByContextPredicateLiteralAsync(new RDFContext("ex:ctx2"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByContextPredicateLiteralAsync(null, new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesBySubjectPredicateAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesBySubjectPredicateAsync(new RDFResource("ex:subj"), new RDFResource("ex:pred"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesBySubjectPredicateAsync(new RDFResource("ex:subj"), new RDFResource("ex:pred"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesBySubjectPredicateAsync(null, new RDFResource("ex:pred"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesBySubjectObjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesBySubjectObjectAsync(new RDFResource("ex:subj"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesBySubjectObjectAsync(new RDFResource("ex:subj1"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesBySubjectObjectAsync(null, new RDFResource("ex:obj"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesBySubjectLiteralAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesBySubjectLiteralAsync(new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesBySubjectLiteralAsync(new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesBySubjectLiteralAsync(null, new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByPredicateObjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByPredicateObjectAsync(new RDFResource("ex:pred"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByPredicateObjectAsync(new RDFResource("ex:pred1"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByPredicateObjectAsync(null, new RDFResource("ex:obj"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveQuadruplesByPredicateLiteralAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.RemoveQuadruplesByPredicateLiteralAsync(new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            await store.RemoveQuadruplesByPredicateLiteralAsync(new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore1 && memStore1.QuadruplesCount == 1);

            await store.RemoveQuadruplesByPredicateLiteralAsync(null, new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore2 && memStore2.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldClearQuadruplesAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            await store.ClearQuadruplesAsync();

            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldContainQuadruplesAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));
            Assert.IsTrue(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));
        }

        [TestMethod]
        public async Task ShouldNotContainQuadruplesAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

            Assert.IsFalse(await store.ContainsQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));
            Assert.IsFalse(await store.ContainsQuadrupleAsync(null));
        }

        [TestMethod]
        public async Task ShouldSelectQuadruplesByContextAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesByContextAsync(new RDFContext("ex:ctx1"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

            RDFMemoryStore store3 = await store.SelectQuadruplesByContextAsync(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldNotSelectQuadruplesByContextAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesByContextAsync(new RDFContext("ex:ctx3"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectQuadruplesBySubjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesBySubjectAsync(new RDFResource("ex:subj1"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

            RDFMemoryStore store3 = await store.SelectQuadruplesBySubjectAsync(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldNotSelectQuadruplesBySubjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesBySubjectAsync(new RDFResource("ex:subj3"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectQuadruplesByPredicateAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesByPredicateAsync(new RDFResource("ex:pred1"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFPlainLiteral("lit"))));

            RDFMemoryStore store3 = await store.SelectQuadruplesByPredicateAsync(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldNotSelectQuadruplesByPredicateAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesByPredicateAsync(new RDFResource("ex:pred3"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectQuadruplesByObjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesByObjectAsync(new RDFResource("ex:obj"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            RDFMemoryStore store3 = await store.SelectQuadruplesByObjectAsync(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldNotSelectQuadruplesByObjectAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesByObjectAsync(new RDFResource("ex:obj3"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectQuadruplesByLiteralAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesByLiteralAsync(new RDFPlainLiteral("lit"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

            RDFMemoryStore store3 = await store.SelectQuadruplesByLiteralAsync(new RDFPlainLiteral("lit"));

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotSelectQuadruplesByLiteralAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectQuadruplesByLiteralAsync(new RDFPlainLiteral("ex:obj","en-US"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectAllQuadruplesAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = await store.SelectAllQuadruplesAsync();

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldExtractContextsAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            List<RDFContext> contexts = await store.ExtractContextsAsync();

            Assert.IsNotNull(contexts);
            Assert.IsTrue(contexts.Count == 2);
            Assert.IsTrue(contexts.Any(c => c.Equals(new RDFContext("ex:ctx1"))));
            Assert.IsTrue(contexts.Any(c => c.Equals(new RDFContext("ex:ctx2"))));
        }

        [TestMethod]
        public async Task ShouldExtractGraphsAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            List<RDFGraph> graphs = await store.ExtractGraphsAsync();

            Assert.IsNotNull(graphs);
            Assert.IsTrue(graphs.Count == 2);
            Assert.IsTrue(graphs.Any(g => g.Context.Equals(new Uri("ex:ctx1"))));
            Assert.IsTrue(graphs.Any(g => g.Context.Equals(new Uri("ex:ctx2"))));
        }

        [TestMethod]
        public async Task ShouldUnreifyQuadruplesAsync()
        {
            RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));
            RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>() { quadruple1, quadruple2 });
            RDFMemoryStore storeQuadruple1 = quadruple1.ReifyQuadruple();
            RDFMemoryStore storeQuadruple2 = quadruple2.ReifyQuadruple();
            RDFAsyncStore reifiedStore = new RDFAsyncStore(storeQuadruple1.UnionWith(storeQuadruple2));

            Assert.IsFalse(reifiedStore.Equals(store));

            await reifiedStore.UnreifyQuadruplesAsync();            

            Assert.IsTrue(reifiedStore.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 2);
        }

        [DataTestMethod]
        [DataRow(".nq", RDFStoreEnums.RDFFormats.NQuads)]
        [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
        [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
        public async Task ShouldExportToFileAsync(string fileExtension, RDFStoreEnums.RDFFormats format)
        {
            RDFAsyncStore store = new RDFAsyncStore();
            RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
            await (await store.AddQuadrupleAsync(quadruple1))
                .AddQuadrupleAsync(quadruple2);
            await store.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFAsyncStoreTest_ShouldExportToFileAsync{fileExtension}"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFAsyncStoreTest_ShouldExportToFileAsync{fileExtension}")));
            Assert.IsTrue(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFAsyncStoreTest_ShouldExportToFileAsync{fileExtension}")).Length > 90);
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnExportingToNullOrEmptyFilepathAsync()
            => await Assert.ThrowsExceptionAsync<RDFStoreException>(async () => await new RDFAsyncStore().ToFileAsync(RDFStoreEnums.RDFFormats.NQuads, null));

        [DataTestMethod]
        [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
        [DataRow(RDFStoreEnums.RDFFormats.TriX)]
        [DataRow(RDFStoreEnums.RDFFormats.TriG)]
        public async Task ShouldExportToStreamAsync(RDFStoreEnums.RDFFormats format)
        {
            MemoryStream stream = new MemoryStream();
            RDFAsyncStore store = new RDFAsyncStore();
            RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
            await (await store.AddQuadrupleAsync(quadruple1))
                .AddQuadrupleAsync(quadruple2);
            await store.ToStreamAsync(format, stream);

            Assert.IsTrue(stream.ToArray().Length > 90);
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnExportingToNullStreamAsync()
            => await Assert.ThrowsExceptionAsync<RDFStoreException>(async () => await new RDFAsyncStore().ToStreamAsync(RDFStoreEnums.RDFFormats.NQuads, null));

        [TestMethod]
        public async Task ShouldExportToDataTableAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await store.AddQuadrupleAsync(quadruple1))
                .AddQuadrupleAsync(quadruple2);
            DataTable table = await store.ToDataTableAsync();

            Assert.IsNotNull(table);
            Assert.IsTrue(table.Columns.Count == 4);
            Assert.IsTrue(table.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(table.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(table.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(table.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(table.Rows.Count == 2);
            Assert.IsTrue(table.Rows[0]["?CONTEXT"].ToString().Equals("http://ctx/"));
            Assert.IsTrue(table.Rows[0]["?SUBJECT"].ToString().Equals("http://subj/"));
            Assert.IsTrue(table.Rows[0]["?PREDICATE"].ToString().Equals("http://pred/"));
            Assert.IsTrue(table.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
            Assert.IsTrue(table.Rows[1]["?CONTEXT"].ToString().Equals("http://ctx/"));
            Assert.IsTrue(table.Rows[1]["?SUBJECT"].ToString().Equals("http://subj/"));
            Assert.IsTrue(table.Rows[1]["?PREDICATE"].ToString().Equals("http://pred/"));
            Assert.IsTrue(table.Rows[1]["?OBJECT"].ToString().Equals("http://obj/"));
        }

        [TestMethod]
        public async Task ShouldExportEmptyToDataTableAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            DataTable table = await store.ToDataTableAsync();

            Assert.IsNotNull(table);
            Assert.IsTrue(table.Columns.Count == 4);
            Assert.IsTrue(table.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(table.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(table.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(table.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(table.Rows.Count == 0);
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFAsyncStoreTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}
