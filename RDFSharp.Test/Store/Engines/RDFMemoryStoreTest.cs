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
        #endregion
    }
}