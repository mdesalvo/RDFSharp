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
        #endregion
    }
}