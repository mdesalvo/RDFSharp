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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Test.Store
{
    [TestClass]
    public class RDFMemoryStoreTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateMemoryStore()
        {
            RDFMemoryStore store = new RDFMemoryStore();

            Assert.IsNotNull(store);
            Assert.IsTrue(store.StoreType.Equals("MEMORY"));
            Assert.IsNotNull(store.StoreIndex);
            Assert.IsNotNull(store.Quadruples);
            Assert.IsTrue(store.QuadruplesCount == 0);
            Assert.IsTrue(store.StoreID.Equals(RDFModelUtilities.CreateHash(store.ToString())));
            Assert.IsTrue(store.ToString().Equals($"MEMORY|ID={store.StoreGUID}"));
        }

        [TestMethod]
        public void ShouldCreateMemoryStoreWithQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore(
                new List<RDFQuadruple>() {
                    new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o"))
                });

            Assert.IsNotNull(store);
            Assert.IsTrue(store.StoreType.Equals("MEMORY"));
            Assert.IsNotNull(store.StoreIndex);
            Assert.IsNotNull(store.Quadruples);
            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.StoreID.Equals(RDFModelUtilities.CreateHash(store.ToString())));
            Assert.IsTrue(store.ToString().Equals($"MEMORY|ID={store.StoreGUID}"));

            int i = 0;
            foreach (RDFQuadruple quad in store) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFQuadruple> quads = store.QuadruplesEnumerator;
            while (quads.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldEqualsMemoryStores()
        {
            RDFMemoryStore store1 = new RDFMemoryStore();
            Assert.IsTrue(store1.Equals(store1));
            Assert.IsFalse(store1.Equals(null));

            RDFMemoryStore store2 = new RDFMemoryStore();
            Assert.IsTrue(store1.Equals(store2));

            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")));
            Assert.IsFalse(store1.Equals(store2));

            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")));
            Assert.IsTrue(store1.Equals(store2));

            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o1")));
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o2")));
            Assert.IsFalse(store1.Equals(store2));
        }

        [TestMethod]
        public void ShouldMergeGraph()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>() {
                new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")),
                new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))
            }).SetContext(new Uri("ex:ctx"));
            RDFMemoryStore store = new RDFMemoryStore();
            store.MergeGraph(graph);

            Assert.IsTrue(store.QuadruplesCount == 2);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

            store.MergeGraph(null);
            Assert.IsTrue(store.QuadruplesCount == 2);
        }

        [TestMethod]
        public void ShouldAddQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.AddQuadruple(null);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruple()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruple(null);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContext()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContext(new RDFContext("ex:ctx1"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContext(new RDFContext("ex:ctx1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContext(null);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesBySubject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesBySubject(new RDFResource("ex:subj1"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesBySubject(new RDFResource("ex:subj1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesBySubject(null);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByPredicate(new RDFResource("ex:pred1"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByPredicate(new RDFResource("ex:pred1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByPredicate(null);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj1")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj2")));
            store.RemoveQuadruplesByObject(new RDFResource("ex:obj1"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj2"))));

            store.RemoveQuadruplesByObject(new RDFResource("ex:obj1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByObject(null);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store.RemoveQuadruplesByLiteral(new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US"))));

            store.RemoveQuadruplesByLiteral(new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByLiteral(null);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextSubject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextSubject(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextSubject(new RDFContext("ex:ctx2"), new RDFResource("ex:subj1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextSubject(null, new RDFResource("ex:subj"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextPredicate(new RDFContext("ex:ctx1"), new RDFResource("ex:pred"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextPredicate(new RDFContext("ex:ctx2"), new RDFResource("ex:pred11"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextPredicate(null, new RDFResource("ex:pred"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextObject(new RDFContext("ex:ctx1"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextObject(new RDFContext("ex:ctx2"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextObject(null, new RDFResource("ex:obj"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextLiteral(new RDFContext("ex:ctx1"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextLiteral(new RDFContext("ex:ctx2"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextLiteral(null, new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextSubjectPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextSubjectPredicate(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextSubjectPredicate(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextSubjectPredicate(null, new RDFResource("ex:subj"), new RDFResource("ex:pred"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextSubjectObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextSubjectObject(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextSubjectObject(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextSubjectObject(null, new RDFResource("ex:subj"), new RDFResource("ex:obj"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextSubjectLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextSubjectLiteral(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextSubjectLiteral(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextSubjectLiteral(null, new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextPredicateObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextPredicateObject(new RDFContext("ex:ctx1"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextPredicateObject(new RDFContext("ex:ctx2"), new RDFResource("ex:pred"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextPredicateObject(null, new RDFResource("ex:pred"), new RDFResource("ex:obj"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByContextPredicateLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByContextPredicateLiteral(new RDFContext("ex:ctx1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByContextPredicateLiteral(new RDFContext("ex:ctx2"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByContextPredicateLiteral(null, new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

                [TestMethod]
        public void ShouldRemoveQuadruplesBySubjectPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesBySubjectPredicate(new RDFResource("ex:subj"), new RDFResource("ex:pred"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesBySubjectPredicate(new RDFResource("ex:subj"), new RDFResource("ex:pred"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesBySubjectPredicate(null, new RDFResource("ex:pred"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesBySubjectObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesBySubjectObject(new RDFResource("ex:subj"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesBySubjectObject(new RDFResource("ex:subj1"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesBySubjectObject(null, new RDFResource("ex:obj"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesBySubjectLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesBySubjectLiteral(new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesBySubjectLiteral(new RDFResource("ex:subj"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesBySubjectLiteral(null, new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByPredicateObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByPredicateObject(new RDFResource("ex:pred"), new RDFResource("ex:obj"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByPredicateObject(new RDFResource("ex:pred1"), new RDFResource("ex:obj1"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByPredicateObject(null, new RDFResource("ex:obj"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldRemoveQuadruplesByPredicateLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.RemoveQuadruplesByPredicateLiteral(new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

            Assert.IsTrue(store.QuadruplesCount == 1);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            store.RemoveQuadruplesByPredicateLiteral(new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);

            store.RemoveQuadruplesByPredicateLiteral(null, new RDFPlainLiteral("lit"));
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldClearQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            store.ClearQuadruples();

            Assert.IsTrue(store.QuadruplesCount == 0);
            Assert.IsTrue(store.StoreIndex.Contexts.Count == 0);
            Assert.IsTrue(store.StoreIndex.Subjects.Count == 0);
            Assert.IsTrue(store.StoreIndex.Predicates.Count == 0);
            Assert.IsTrue(store.StoreIndex.Objects.Count == 0);
            Assert.IsTrue(store.StoreIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldContainQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));
        }

        [TestMethod]
        public void ShouldNotContainQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

            Assert.IsFalse(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));
            Assert.IsFalse(store.ContainsQuadruple(null));
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContext()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesByContext(new RDFContext("ex:ctx1"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

            RDFMemoryStore store3 = store.SelectQuadruplesByContext(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesByContext()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesByContext(new RDFContext("ex:ctx3"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesBySubject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesBySubject(new RDFResource("ex:subj1"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

            RDFMemoryStore store3 = store.SelectQuadruplesBySubject(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesBySubject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesBySubject(new RDFResource("ex:subj3"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesByPredicate(new RDFResource("ex:pred1"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFPlainLiteral("lit"))));

            RDFMemoryStore store3 = store.SelectQuadruplesByPredicate(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesByPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesByPredicate(new RDFResource("ex:pred3"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesByObject(new RDFResource("ex:obj"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

            RDFMemoryStore store3 = store.SelectQuadruplesByObject(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesByObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesByObject(new RDFResource("ex:obj3"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesByLiteral(new RDFPlainLiteral("lit"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 1);
            Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

            RDFMemoryStore store3 = store.SelectQuadruplesByLiteral(null);

            Assert.IsNotNull(store3);
            Assert.IsTrue(store3.QuadruplesCount == 2);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesByLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectQuadruplesByLiteral(new RDFPlainLiteral("ex:obj","en-US"));

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldSelectAllQuadruples()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = store.SelectAllQuadruples();

            Assert.IsNotNull(store2);
            Assert.IsTrue(store2.QuadruplesCount == 2);
            Assert.IsTrue(store.Equals(store2));
        }

        [TestMethod]
        public void ShouldExtractContexts()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            List<RDFContext> contexts = store.ExtractContexts();

            Assert.IsNotNull(contexts);
            Assert.IsTrue(contexts.Count == 2);
            Assert.IsTrue(contexts.Any(c => c.Equals(new RDFContext("ex:ctx1"))));
            Assert.IsTrue(contexts.Any(c => c.Equals(new RDFContext("ex:ctx2"))));
        }

        [TestMethod]
        public void ShouldExtractGraphs()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            List<RDFGraph> graphs = store.ExtractGraphs();

            Assert.IsNotNull(graphs);
            Assert.IsTrue(graphs.Count == 2);
            Assert.IsTrue(graphs.Any(g => g.Context.Equals(new Uri("ex:ctx1"))));
            Assert.IsTrue(graphs.Any(g => g.Context.Equals(new Uri("ex:ctx2"))));
        }

        [TestMethod]
        public void ShouldIntersectStores()
        {
            RDFMemoryStore store1 = new RDFMemoryStore();
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = new RDFMemoryStore();
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore intersection12 = store1.IntersectWith(store2);

            Assert.IsNotNull(intersection12);
            Assert.IsTrue(intersection12.QuadruplesCount == 2);

            RDFMemoryStore intersection21 = store2.IntersectWith(store1);

            Assert.IsNotNull(intersection21);
            Assert.IsTrue(intersection21.QuadruplesCount == 2);

            RDFMemoryStore intersectionEmptyA = store1.IntersectWith(new RDFMemoryStore());
            Assert.IsNotNull(intersectionEmptyA);
            Assert.IsTrue(intersectionEmptyA.QuadruplesCount == 0);

            RDFMemoryStore intersectionEmptyB = (new RDFMemoryStore()).IntersectWith(store1);
            Assert.IsNotNull(intersectionEmptyB);
            Assert.IsTrue(intersectionEmptyB.QuadruplesCount == 0);

            RDFMemoryStore intersectionNull = store1.IntersectWith(null);
            Assert.IsNotNull(intersectionNull);
            Assert.IsTrue(intersectionNull.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldUnionStores()
        {
            RDFMemoryStore store1 = new RDFMemoryStore();
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = new RDFMemoryStore();
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore union12 = store1.UnionWith(store2);

            Assert.IsNotNull(union12);
            Assert.IsTrue(union12.QuadruplesCount == 4);

            RDFMemoryStore union21 = store2.UnionWith(store1);

            Assert.IsNotNull(union21);
            Assert.IsTrue(union21.QuadruplesCount == 4);

            RDFMemoryStore unionEmptyA = store1.UnionWith(new RDFMemoryStore());
            Assert.IsNotNull(unionEmptyA);
            Assert.IsTrue(unionEmptyA.QuadruplesCount == 3);

            RDFMemoryStore unionEmptyB = (new RDFMemoryStore()).UnionWith(store1);
            Assert.IsNotNull(unionEmptyB);
            Assert.IsTrue(unionEmptyB.QuadruplesCount == 3);

            RDFMemoryStore unionNull = store1.UnionWith(null);
            Assert.IsNotNull(unionNull);
            Assert.IsTrue(unionNull.QuadruplesCount == 3);
        }

        [TestMethod]
        public void ShouldDifferenceStores()
        {
            RDFMemoryStore store1 = new RDFMemoryStore();
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore store2 = new RDFMemoryStore();
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-US")));
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit","en-UK")));
            store2.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
            RDFMemoryStore difference12 = store1.DifferenceWith(store2);

            Assert.IsNotNull(difference12);
            Assert.IsTrue(difference12.QuadruplesCount == 1);

            RDFMemoryStore difference21 = store2.DifferenceWith(store1);

            Assert.IsNotNull(difference21);
            Assert.IsTrue(difference21.QuadruplesCount == 1);

            RDFMemoryStore differenceEmptyA = store1.DifferenceWith(new RDFMemoryStore());
            Assert.IsNotNull(differenceEmptyA);
            Assert.IsTrue(differenceEmptyA.QuadruplesCount == 3);

            RDFMemoryStore differenceEmptyB = (new RDFMemoryStore()).DifferenceWith(store1);
            Assert.IsNotNull(differenceEmptyB);
            Assert.IsTrue(differenceEmptyB.QuadruplesCount == 0);

            RDFMemoryStore differenceNull = store1.DifferenceWith(null);
            Assert.IsNotNull(differenceNull);
            Assert.IsTrue(differenceNull.QuadruplesCount == 3);
        }

        [TestMethod]
        public void ShouldUnreifyQuadruples()
        {
            RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));
            RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>() { quadruple1, quadruple2 });
            RDFMemoryStore storeQuadruple1 = quadruple1.ReifyQuadruple();
            RDFMemoryStore storeQuadruple2 = quadruple2.ReifyQuadruple();
            RDFMemoryStore reifiedStore = storeQuadruple1.UnionWith(storeQuadruple2);

            Assert.IsFalse(reifiedStore.Equals(store));

            reifiedStore.UnreifyQuadruples();            

            Assert.IsTrue(reifiedStore.Equals(store));
        }
        #endregion
    }
}
