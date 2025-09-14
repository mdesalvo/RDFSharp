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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Test.Store;

[TestClass]
public class RDFMemoryStoreTest
{
    #region Tests (Sync)
    [TestMethod]
    public void ShouldCreateMemoryStore()
    {
        RDFMemoryStore store = new RDFMemoryStore();

        Assert.IsNotNull(store);
        Assert.IsTrue(store.StoreType.Equals("MEMORY", StringComparison.Ordinal));
        Assert.IsNotNull(store.Index);
        Assert.IsNotNull(store.Index.Hashes);
        Assert.AreEqual(0, store.QuadruplesCount);
        Assert.IsEmpty(store.Index.Contexts);
        Assert.IsEmpty(store.Index.Resources);
        Assert.IsEmpty(store.Index.Literals);
        Assert.IsEmpty(store.Index.IDXContexts);
        Assert.IsEmpty(store.Index.IDXSubjects);
        Assert.IsEmpty(store.Index.IDXPredicates);
        Assert.IsEmpty(store.Index.IDXObjects);
        Assert.IsEmpty(store.Index.IDXLiterals);
        Assert.IsTrue(store.StoreID.Equals(RDFModelUtilities.CreateHash(store.ToString())));
        Assert.IsTrue(store.ToString().Equals($"MEMORY|ID={store.StoreGUID}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateMemoryStoreWithQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")),
            new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFPlainLiteral("lit"))
        ]);

        Assert.IsNotNull(store);
        Assert.IsTrue(store.StoreType.Equals("MEMORY", StringComparison.Ordinal));
        Assert.IsNotNull(store.Index);
        Assert.IsNotNull(store.Index.Hashes);
        Assert.AreEqual(2, store.QuadruplesCount);
        Assert.HasCount(1, store.Index.Contexts);
        Assert.HasCount(3, store.Index.Resources);
        Assert.HasCount(1, store.Index.Literals);
        Assert.HasCount(1, store.Index.IDXContexts);
        Assert.HasCount(1, store.Index.IDXSubjects);
        Assert.HasCount(1, store.Index.IDXPredicates);
        Assert.HasCount(1, store.Index.IDXObjects);
        Assert.HasCount(1, store.Index.IDXLiterals);
        Assert.IsTrue(store.StoreID.Equals(RDFModelUtilities.CreateHash(store.ToString())));
        Assert.IsTrue(store.ToString().Equals($"MEMORY|ID={store.StoreGUID}", StringComparison.Ordinal));

        int i = store.Count();
        Assert.AreEqual(2, i);

        int j = 0;
        IEnumerator<RDFQuadruple> quads = store.QuadruplesEnumerator;
        while (quads.MoveNext()) j++;
        Assert.AreEqual(2, j);
    }

    [TestMethod]
    public void ShouldDisposeMemoryStoreWithUsing()
    {
        RDFMemoryStore store;
        using (store = new RDFMemoryStore([
                   new RDFQuadruple(new RDFContext("ex:c"), new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")) ]))
        {
            Assert.IsFalse(store.Disposed);
            Assert.IsNotNull(store.Index);
        }
        Assert.IsTrue(store.Disposed);
        Assert.IsNull(store.Index);
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
        RDFGraph graph = new RDFGraph([
            new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")),
            new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))
        ]).SetContext(new Uri("ex:ctx"));
        RDFMemoryStore store = new RDFMemoryStore();
        store.MergeGraph(graph);

        Assert.AreEqual(2, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"))));

        store.MergeGraph(null);
        Assert.AreEqual(2, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldAddQuadruple()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.AddQuadruple(null);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruple()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        Assert.AreEqual(1, store.QuadruplesCount);

        store.RemoveQuadruple(null);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContext()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesBySubject()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(null, new RDFResource("ex:subj1"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(null, new RDFResource("ex:subj1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByPredicate()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(null, null, new RDFResource("ex:pred1"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(null, null, new RDFResource("ex:pred1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByObject()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj1")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj2")));
        store.RemoveQuadruples(null, null, null, new RDFResource("ex:obj1"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj2"))));

        store.RemoveQuadruples(null, null, null, new RDFResource("ex:obj1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByLiteral()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")));
        store.RemoveQuadruples(null, null, null, null, new RDFPlainLiteral("lit"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"))));

        store.RemoveQuadruples(null, null, null, null, new RDFPlainLiteral("lit"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextSubject()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx2"), new RDFResource("ex:subj1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextPredicate()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), null, new RDFResource("ex:pred"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx2"), null, new RDFResource("ex:pred11"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextObject()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), null, null, new RDFResource("ex:obj"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx1"), null, null, new RDFResource("ex:obj"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextLiteral()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), null, null, null, new RDFPlainLiteral("lit"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx1"), null, null, null, new RDFPlainLiteral("lit"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextSubjectPredicate()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextSubjectObject()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), null, new RDFResource("ex:obj"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), null, new RDFResource("ex:obj1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextSubjectLiteral()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), null, null, new RDFPlainLiteral("lit"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), null, null, new RDFPlainLiteral("lit"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextPredicateObject()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), null, new RDFResource("ex:pred"), new RDFResource("ex:obj"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx2"), null, new RDFResource("ex:pred"), new RDFResource("ex:obj1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByContextPredicateLiteral()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(new RDFContext("ex:ctx1"), null, new RDFResource("ex:pred"), null, new RDFPlainLiteral("lit"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(new RDFContext("ex:ctx2"), null, new RDFResource("ex:pred"), null, new RDFPlainLiteral("lit"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesBySubjectPredicate()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(null, new RDFResource("ex:subj"), new RDFResource("ex:pred"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(null, new RDFResource("ex:subj"), new RDFResource("ex:pred"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesBySubjectObject()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(null, new RDFResource("ex:subj"), null, new RDFResource("ex:obj"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(null, new RDFResource("ex:subj1"), null, new RDFResource("ex:obj1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesBySubjectLiteral()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(null, new RDFResource("ex:subj"), null, null, new RDFPlainLiteral("lit"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(null, new RDFResource("ex:subj"), null, null, new RDFPlainLiteral("lit"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByPredicateObject()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(null, null, new RDFResource("ex:pred"), new RDFResource("ex:obj"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx3"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(null, null, new RDFResource("ex:pred1"), new RDFResource("ex:obj1"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRemoveQuadruplesByPredicateLiteral()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.RemoveQuadruples(null, null, new RDFResource("ex:pred"), null, new RDFPlainLiteral("lit"));

        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"))));

        store.RemoveQuadruples(null, null, new RDFResource("ex:pred"), null, new RDFPlainLiteral("lit"));
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldClearQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        store.ClearQuadruples();

        Assert.AreEqual(0, store.QuadruplesCount);
        Assert.IsEmpty(store.Index.IDXContexts);
        Assert.IsEmpty(store.Index.IDXSubjects);
        Assert.IsEmpty(store.Index.IDXPredicates);
        Assert.IsEmpty(store.Index.IDXObjects);
        Assert.IsEmpty(store.Index.IDXLiterals);
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
    public void ShouldSelectQuadruplesByNullParameters()
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFResource subj = new RDFResource("ex:subj");
        RDFResource pred = new RDFResource("ex:pred");
        RDFResource obj = new RDFResource("ex:obj");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx, subj, pred, obj),
                new RDFQuadruple(ctx, subj, pred, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(); //select *

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContext()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesBySubject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, subj1);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByPredicate()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, null, pred1);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, null, null, obj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit1 = new RDFPlainLiteral("lit");
        RDFPlainLiteral lit2 = new RDFPlainLiteral("lit", "en-US");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit1),
                new RDFQuadruple(ctx2, subj1, pred2, lit2)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, null, null, null, lit2);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextSubject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, subj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextPredicate()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, null, pred1);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, null, null, obj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, null, null, null, lit);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextSubjectPredicate()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, subj1, pred1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextSubjectObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, subj1, null, obj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextSubjectLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, subj2, null, null, lit);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextPredicateObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, null, pred1, obj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextPredicateLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, null, pred1, null, lit);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextSubjectPredicateObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, subj1, pred1, obj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextSubjectPredicateLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, subj2, pred1, null, lit);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesBySubjectPredicate()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, subj1, pred1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesBySubjectObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, subj1, null, obj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesBySubjectLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, subj2, null, null, lit);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByPredicateObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, null, pred1, obj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByPredicateLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, null, pred1, null, lit);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesBySubjectPredicateObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, subj1, pred1, obj1);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesBySubjectPredicateLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, subj2, pred1, null, lit);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByContext()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx1, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx2);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesBySubject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj1, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred2, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, subj2);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByPredicate()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred1, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, null, pred2);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByObject()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFResource obj2 = new RDFResource("ex:obj2");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred2, lit),
                new RDFQuadruple(ctx2, subj1, pred1, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, null, null, obj2);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByLiteral()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource subj2 = new RDFResource("ex:subj2");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFPlainLiteral lit2 = new RDFPlainLiteral("lit2");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj2, pred2, lit),
                new RDFQuadruple(ctx2, subj1, pred1, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(null, null, null, null, lit2);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesAtMiddle()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource pred2 = new RDFResource("ex:pred2");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj1, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred1, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, subj1, pred2, null, lit);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesAtLast()
    {
        RDFContext ctx1 = new RDFContext("ex:ctx1");
        RDFContext ctx2 = new RDFContext("ex:ctx2");
        RDFResource subj1 = new RDFResource("ex:subj1");
        RDFResource pred1 = new RDFResource("ex:pred1");
        RDFResource obj1 = new RDFResource("ex:obj1");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFPlainLiteral lit2 = new RDFPlainLiteral("lit2");
        RDFMemoryStore data = new RDFMemoryStore(
            [
                new RDFQuadruple(ctx1, subj1, pred1, obj1),
                new RDFQuadruple(ctx1, subj1, pred1, lit),
                new RDFQuadruple(ctx2, subj1, pred1, lit)
            ]
        );
        List<RDFQuadruple> result = data.SelectQuadruples(ctx1, subj1, pred1, null, lit2);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByNullAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, null, null, null, null];

        Assert.IsNotNull(store2);
        Assert.AreEqual(2, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByContextAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[new RDFContext("ex:ctx1")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(1, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByContextAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[new RDFContext("ex:ctx3")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesBySubjectAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, new RDFResource("ex:subj1")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(1, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesBySubjectAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj1"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, new RDFResource("ex:subj3")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByPredicateAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, null, new RDFResource("ex:pred1")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(1, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByPredicateAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred1"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, null, new RDFResource("ex:pred3")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByObjectAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, null, null, new RDFResource("ex:obj")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(1, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByObjectAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, null, null, new RDFResource("ex:obj3")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByLiteralAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, null, null, null, new RDFPlainLiteral("lit")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(1, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByLiteralAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[null, null, null, null, new RDFPlainLiteral("lit","en")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldSelectQuadruplesByFullAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), null, new RDFPlainLiteral("lit")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(1, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldNotSelectQuadruplesByFullAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));
        RDFMemoryStore store2 = store[new RDFContext("ex:ctx1"), new RDFResource("ex:subj2"), new RDFResource("ex:pred"), null, new RDFPlainLiteral("lit")];

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnSelectingQuadruplesByIllecitAccessor()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

        Assert.ThrowsExactly<RDFStoreException>(() => _ = store[null, null, null, new RDFResource("http://obj/"), new RDFPlainLiteral("lit")]);
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
        Assert.HasCount(2, contexts);
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
        Assert.HasCount(2, graphs);
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
        Assert.AreEqual(2, intersection12.QuadruplesCount);

        RDFMemoryStore intersection21 = store2.IntersectWith(store1);

        Assert.IsNotNull(intersection21);
        Assert.AreEqual(2, intersection21.QuadruplesCount);

        RDFMemoryStore intersectionEmptyA = store1.IntersectWith(new RDFMemoryStore());
        Assert.IsNotNull(intersectionEmptyA);
        Assert.AreEqual(0, intersectionEmptyA.QuadruplesCount);

        RDFMemoryStore intersectionEmptyB = new RDFMemoryStore().IntersectWith(store1);
        Assert.IsNotNull(intersectionEmptyB);
        Assert.AreEqual(0, intersectionEmptyB.QuadruplesCount);

        RDFMemoryStore intersectionNull = store1.IntersectWith(null);
        Assert.IsNotNull(intersectionNull);
        Assert.AreEqual(0, intersectionNull.QuadruplesCount);
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
        Assert.AreEqual(4, union12.QuadruplesCount);

        RDFMemoryStore union21 = store2.UnionWith(store1);

        Assert.IsNotNull(union21);
        Assert.AreEqual(4, union21.QuadruplesCount);

        RDFMemoryStore unionEmptyA = store1.UnionWith(new RDFMemoryStore());
        Assert.IsNotNull(unionEmptyA);
        Assert.AreEqual(3, unionEmptyA.QuadruplesCount);

        RDFMemoryStore unionEmptyB = new RDFMemoryStore().UnionWith(store1);
        Assert.IsNotNull(unionEmptyB);
        Assert.AreEqual(3, unionEmptyB.QuadruplesCount);

        RDFMemoryStore unionNull = store1.UnionWith(null);
        Assert.IsNotNull(unionNull);
        Assert.AreEqual(3, unionNull.QuadruplesCount);
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
        Assert.AreEqual(1, difference12.QuadruplesCount);

        RDFMemoryStore difference21 = store2.DifferenceWith(store1);

        Assert.IsNotNull(difference21);
        Assert.AreEqual(1, difference21.QuadruplesCount);

        RDFMemoryStore differenceEmptyA = store1.DifferenceWith(new RDFMemoryStore());
        Assert.IsNotNull(differenceEmptyA);
        Assert.AreEqual(3, differenceEmptyA.QuadruplesCount);

        RDFMemoryStore differenceEmptyB = new RDFMemoryStore().DifferenceWith(store1);
        Assert.IsNotNull(differenceEmptyB);
        Assert.AreEqual(0, differenceEmptyB.QuadruplesCount);

        RDFMemoryStore differenceNull = store1.DifferenceWith(null);
        Assert.IsNotNull(differenceNull);
        Assert.AreEqual(3, differenceNull.QuadruplesCount);
    }

    [TestMethod]
    [DataRow(".nq",RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
    public void ShouldExportToFile(string fileExtension, RDFStoreEnums.RDFFormats format)
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        store.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldExportToFile{fileExtension}"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldExportToFile{fileExtension}")));
        Assert.IsGreaterThan(90, File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldExportToFile{fileExtension}")).Length);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnExportingToNullOrEmptyFilepath()
        => Assert.ThrowsExactly<RDFStoreException>(() => new RDFMemoryStore().ToFile(RDFStoreEnums.RDFFormats.NQuads, null));

    [TestMethod]
    [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(RDFStoreEnums.RDFFormats.TriG)]
    public void ShouldExportToStream(RDFStoreEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFMemoryStore store = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        store.ToStream(format, stream);

        Assert.IsGreaterThan(90, stream.ToArray().Length);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnExportingToNullStream()
        => Assert.ThrowsExactly<RDFStoreException>(() => new RDFMemoryStore().ToStream(RDFStoreEnums.RDFFormats.NQuads, null));

    [TestMethod]
    public void ShouldExportToDataTable()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit","en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        store.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        DataTable table = store.ToDataTable();

        Assert.IsNotNull(table);
        Assert.AreEqual(4, table.Columns.Count);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.AreEqual(2, table.Rows.Count);
        Assert.IsTrue(table.Rows[0]["?CONTEXT"].ToString().Equals("http://ctx/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?SUBJECT"].ToString().Equals("http://subj/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?PREDICATE"].ToString().Equals("http://pred/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?CONTEXT"].ToString().Equals("http://ctx/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?SUBJECT"].ToString().Equals("http://subj/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?PREDICATE"].ToString().Equals("http://pred/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?OBJECT"].ToString().Equals("http://obj/", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldExportEmptyToDataTable()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        DataTable table = store.ToDataTable();

        Assert.IsNotNull(table);
        Assert.AreEqual(4, table.Columns.Count);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.AreEqual(0, table.Rows.Count);
    }

    [TestMethod]
    [DataRow(".nq", RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
    public void ShouldImportFromFile(string fileExtension, RDFStoreEnums.RDFFormats format)
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store1.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        store1.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportFromFile{fileExtension}"));
        RDFMemoryStore store2 = RDFMemoryStore.FromFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportFromFile{fileExtension}"));

        Assert.IsNotNull(store2);
        Assert.AreEqual(2, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    [DataRow(".nq", RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
    public void ShouldImportFromFileWithEnabledDatatypeDiscovery(string fileExtension, RDFStoreEnums.RDFFormats format)
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store1.AddQuadruple(quadruple1)
            .AddQuadruple(quadruple2)
            .MergeGraph(new RDFGraph()
                .AddDatatype(new RDFDatatype(new Uri($"ex:mydtPP{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
                    new RDFPatternFacet("^ex$") ])));
        store1.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportFromFile{fileExtension}WithEnabledDatatypeDiscovery"));
        RDFMemoryStore store2 = RDFMemoryStore.FromFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportFromFile{fileExtension}WithEnabledDatatypeDiscovery"), true);

        Assert.IsNotNull(store2);
        Assert.AreEqual(9, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype($"ex:mydtPP{(int)format}").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtPP{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [TestMethod]
    [DataRow(".nq", RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
    public void ShouldImportEmptyFromFile(string fileExtension, RDFStoreEnums.RDFFormats format)
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        store1.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportEmptyFromFile{fileExtension}"));
        RDFMemoryStore store2 = RDFMemoryStore.FromFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportEmptyFromFile{fileExtension}"));

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullOrEmptyFilepath()
        => Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromFile(RDFStoreEnums.RDFFormats.NQuads, null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromUnexistingFilepath()
        => Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromFile(RDFStoreEnums.RDFFormats.NQuads, "blablabla"));

    [TestMethod]
    [DataRow(".nq", RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
    public async Task ShouldImportFromFileAsync(string fileExtension, RDFStoreEnums.RDFFormats format)
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store1.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        await store1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportFromFileAsync{fileExtension}"));
        RDFMemoryStore store2 = await RDFMemoryStore.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportFromFileAsync{fileExtension}"));

        Assert.IsNotNull(store2);
        Assert.AreEqual(2, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    [DataRow(".nq", RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
    public async Task ShouldImportFromFileAsyncWithEnabledDatatypeDiscovery(string fileExtension, RDFStoreEnums.RDFFormats format)
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store1.AddQuadruple(quadruple1)
              .AddQuadruple(quadruple2)
              .MergeGraph(new RDFGraph()
                .AddDatatype(new RDFDatatype(new Uri($"ex:mydtP{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFPatternFacet("^ex$") ])));
        await store1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportFromFileAsync{fileExtension}WithEnabledDatatypeDiscovery"));
        RDFMemoryStore store2 = await RDFMemoryStore.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportFromFileAsync{fileExtension}WithEnabledDatatypeDiscovery"), true);

        Assert.IsNotNull(store2);
        Assert.AreEqual(9, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype($"ex:mydtP{(int)format}").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtP{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [TestMethod]
    [DataRow(".nq", RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
    public async Task ShouldImportEmptyFromFileAsync(string fileExtension, RDFStoreEnums.RDFFormats format)
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        await store1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportEmptyFromFileAsync{fileExtension}"));
        RDFMemoryStore store2 = await RDFMemoryStore.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldImportEmptyFromFileAsync{fileExtension}"));

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullOrEmptyFilepathAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.NQuads, null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromUnexistingFilepathAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.NQuads, "blablabla"));

    [TestMethod]
    [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(RDFStoreEnums.RDFFormats.TriG)]
    public void ShouldImportFromStream(RDFStoreEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store1.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        store1.ToStream(format, stream);
        RDFMemoryStore store2 = RDFMemoryStore.FromStream(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store2);
        Assert.AreEqual(2, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(RDFStoreEnums.RDFFormats.TriG)]
    public void ShouldImportFromStreamWithEnabledDatatypeDiscovery(RDFStoreEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store1.AddQuadruple(quadruple1)
            .AddQuadruple(quadruple2)
            .MergeGraph(new RDFGraph()
                .AddDatatype(new RDFDatatype(new Uri($"ex:mydtQQ{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
                    new RDFPatternFacet("^ex$") ])));
        store1.ToStream(format, stream);
        RDFMemoryStore store2 = RDFMemoryStore.FromStream(format, new MemoryStream(stream.ToArray()), true);

        Assert.IsNotNull(store2);
        Assert.AreEqual(9, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype($"ex:mydtQQ{(int)format}").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtQQ{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [TestMethod]
    [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(RDFStoreEnums.RDFFormats.TriG)]
    public void ShouldImportFromEmptyStream(RDFStoreEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFMemoryStore store1 = new RDFMemoryStore();
        store1.ToStream(format, stream);
        RDFMemoryStore store2 = RDFMemoryStore.FromStream(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullStream()
        => Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromStream(RDFStoreEnums.RDFFormats.NQuads, null));

    [TestMethod]
    [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(RDFStoreEnums.RDFFormats.TriG)]
    public async Task ShouldImportFromStreamAsync(RDFStoreEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store1.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        await store1.ToStreamAsync(format, stream);
        RDFMemoryStore store2 = await RDFMemoryStore.FromStreamAsync(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store2);
        Assert.AreEqual(2, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(RDFStoreEnums.RDFFormats.TriG)]
    public async Task ShouldImportFromStreamAsyncWithEnabledDatatypeDiscovery(RDFStoreEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store1.AddQuadruple(quadruple1)
              .AddQuadruple(quadruple2)
              .MergeGraph(new RDFGraph()
                .AddDatatype(new RDFDatatype(new Uri($"ex:mydtQ{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFPatternFacet("^ex$") ])));
        await store1.ToStreamAsync(format, stream);
        RDFMemoryStore store2 = await RDFMemoryStore.FromStreamAsync(format, new MemoryStream(stream.ToArray()), true);

        Assert.IsNotNull(store2);
        Assert.AreEqual(9, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype($"ex:mydtQ{(int)format}").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtQ{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [TestMethod]
    [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(RDFStoreEnums.RDFFormats.TriG)]
    public async Task ShouldImportFromEmptyStreamAsync(RDFStoreEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFMemoryStore store1 = new RDFMemoryStore();
        await store1.ToStreamAsync(format, stream);
        RDFMemoryStore store2 = await RDFMemoryStore.FromStreamAsync(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullStreamAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromStreamAsync(RDFStoreEnums.RDFFormats.NQuads, null));

    [TestMethod]
    public void ShouldImportFromDataTable()
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        store1.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        DataTable table = store1.ToDataTable();
        RDFMemoryStore store2 = RDFMemoryStore.FromDataTable(table);

        Assert.IsNotNull(store2);
        Assert.AreEqual(2, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    public void ShouldImportFromDataTableWithEnabledDatatypeDiscovery()
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        store1.AddQuadruple(quadruple1)
            .AddQuadruple(quadruple2)
            .MergeGraph(new RDFGraph()
                .AddDatatype(new RDFDatatype(new Uri("ex:mydtQ"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
                    new RDFPatternFacet("^ex$") ])));
        DataTable table = store1.ToDataTable();
        RDFMemoryStore store2 = RDFMemoryStore.FromDataTable(table, true);

        Assert.IsNotNull(store2);
        Assert.AreEqual(9, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype("ex:mydtQ").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype("ex:mydtQ").Facets.Single() is RDFPatternFacet { Pattern: "^ex$" });
    }

    [TestMethod]
    public void ShouldImportEmptyFromDataTable()
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        DataTable table = store1.ToDataTable();
        RDFMemoryStore store2 = RDFMemoryStore.FromDataTable(table);

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullDataTable()
        => Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableNotHavingMandatoryColumns()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableNotHavingExactColumns()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECTTTTT", typeof(string));

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldImportFromDataTableHavingRowWithNullContext()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add(null, "http://subj/", "http://pred/", "http://obj/");

        RDFMemoryStore store = RDFMemoryStore.FromDataTable(table);

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.AreEqual(1, store[new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")].QuadruplesCount);
    }

    [TestMethod]
    public void ShouldImportFromDataTableHavingRowWithEmptyContext()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("", "http://subj/", "http://pred/", "http://obj/");

        RDFMemoryStore store = RDFMemoryStore.FromDataTable(table);

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.AreEqual(1, store[new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")].QuadruplesCount);
    }

    [TestMethod]
    public void ShouldImportFromDataTableHavingRowWithoutContext()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "http://pred/", "http://obj/");

        RDFMemoryStore store = RDFMemoryStore.FromDataTable(table);

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.AreEqual(1, store[new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")].QuadruplesCount);
    }

    [TestMethod]
    public void ShouldImportFromDataTableHavingRowWithValorizedContext()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "http://pred/", "http://obj/");

        RDFMemoryStore store = RDFMemoryStore.FromDataTable(table);

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.AreEqual(1, store[new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")].QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingBlankContext()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("bnode:12345", "http://subj/", "http://pred/", "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralContext()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("hello@en", "http://subj/", "http://pred/", "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullSubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", null, "http://pred/", "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptySubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx", "", "http://pred/", "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralSubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "hello@en", "http://pred/", "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", null, "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptyPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "", "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithBlankPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "bnode:12345", "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "hello@en", "http://obj/");

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullObject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "http://pred/", null);

        Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromDataTable(table));
    }

    [TestMethod]
    public async Task ShouldImportFromDataTableAsync()
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        store1.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        DataTable table = await store1.ToDataTableAsync();
        RDFMemoryStore store2 = await RDFMemoryStore.FromDataTableAsync(table);

        Assert.IsNotNull(store2);
        Assert.AreEqual(2, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    public async Task ShouldImportFromDataTableAsyncWithEnabledDatatypeDiscovery()
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        store1.AddQuadruple(quadruple1)
              .AddQuadruple(quadruple2)
              .MergeGraph(new RDFGraph()
                .AddDatatype(new RDFDatatype(new Uri("ex:mydtQG"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFPatternFacet("^ex$") ])));
        DataTable table = await store1.ToDataTableAsync();
        RDFMemoryStore store2 = await RDFMemoryStore.FromDataTableAsync(table, true);

        Assert.IsNotNull(store2);
        Assert.AreEqual(9, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype("ex:mydtQG").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype("ex:mydtQG").Facets.Single() is RDFPatternFacet { Pattern: "^ex$" });
    }

    [TestMethod]
    public async Task ShouldImportEmptyFromDataTableAsync()
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        DataTable table = await store1.ToDataTableAsync();
        RDFMemoryStore store2 = await RDFMemoryStore.FromDataTableAsync(table);

        Assert.IsNotNull(store2);
        Assert.AreEqual(0, store2.QuadruplesCount);
        Assert.IsTrue(store2.Equals(store1));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullDataTableAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableNotHavingMandatoryColumnsAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableNotHavingExactColumnsAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECTTTTT", typeof(string));

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldImportFromDataTableHavingRowWithNullContextAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add(null, "http://subj/", "http://pred/", "http://obj/");

        RDFMemoryStore store = await RDFMemoryStore.FromDataTableAsync(table);

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.AreEqual(1, store[new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")].QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldImportFromDataTableHavingRowWithEmptyContextAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("", "http://subj/", "http://pred/", "http://obj/");

        RDFMemoryStore store = await RDFMemoryStore.FromDataTableAsync(table);

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.AreEqual(1, store[new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")].QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldImportFromDataTableHavingRowWithValorizedContextAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "http://pred/", "http://obj/");

        RDFMemoryStore store = await RDFMemoryStore.FromDataTableAsync(table);

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.AreEqual(1, store[new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")].QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldImportFromDataTableHavingRowWithLiteralContextAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("hello@en", "http://subj/", "http://pred/", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullSubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", null, "http://pred/", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptySubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "", "http://pred/", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralSubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "hello@en", "http://pred/", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", null, "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptyPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithBlankPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "bnode:12345", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "hello@en", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullObjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?CONTEXT", typeof(string));
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://ctx/", "http://subj/", "http://pred/", null);

        await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromDataTableAsync(table));
    }

    [TestMethod]
    public void ShouldImportNQuadsFromUri()
    {
        RDFMemoryStore store = RDFMemoryStore.FromUri(new Uri("https://w3c.github.io/rdf-tests/rdf/rdf11/rdf-n-quads/nq-syntax-uri-01.nq"));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldImportTrigFromUri()
    {
        RDFMemoryStore store = RDFMemoryStore.FromUri(new Uri("https://w3c.github.io/rdf-tests/rdf/rdf11/rdf-trig/alternating_iri_graphs.trig"));

        Assert.IsNotNull(store);
        Assert.AreEqual(4, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullUri()
        => Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromUri(null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromRelativeUri()
        => Assert.ThrowsExactly<RDFStoreException>(() => RDFMemoryStore.FromUri(new Uri("/file/system", UriKind.Relative)));

    [TestMethod]
    public async Task ShouldImportFromUriAsync()
    {
        RDFMemoryStore store = await RDFMemoryStore.FromUriAsync(new Uri("https://w3c.github.io/rdf-tests/rdf/rdf11/rdf-n-quads/nq-syntax-uri-01.nq"));

        Assert.IsNotNull(store);
        Assert.IsGreaterThan(0, store.QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullUriAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromUriAsync(null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromRelativeUriAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromUriAsync(new Uri("/file/system", UriKind.Relative)));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromUnreacheableUriAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => RDFMemoryStore.FromUriAsync(new Uri("http://rdfsharp.test/"), 500));
    #endregion

    #region Tests (Async)
    [TestMethod]
    [DataRow(".nq", RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(".trix", RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(".trig", RDFStoreEnums.RDFFormats.TriG)]
    public async Task ShouldExportToFileAsync(string fileExtension, RDFStoreEnums.RDFFormats format)
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        await store.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldExportToFileAsync{fileExtension}"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldExportToFileAsync{fileExtension}")));
        Assert.IsGreaterThan(90, (await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, $"RDFMemoryStoreTest_ShouldExportToFileAsync{fileExtension}"), TestContext.CancellationTokenSource.Token)).Length);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnExportingToNullOrEmptyFilepathAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => new RDFMemoryStore().ToFileAsync(RDFStoreEnums.RDFFormats.NQuads, null));

    [TestMethod]
    [DataRow(RDFStoreEnums.RDFFormats.NQuads)]
    [DataRow(RDFStoreEnums.RDFFormats.TriX)]
    [DataRow(RDFStoreEnums.RDFFormats.TriG)]
    public async Task ShouldExportToStreamAsync(RDFStoreEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFMemoryStore store = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ex/ctx/"), new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        store.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        await store.ToStreamAsync(format, stream);

        Assert.IsGreaterThan(90, stream.ToArray().Length);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnExportingToNullStreamAsync()
        => await Assert.ThrowsExactlyAsync<RDFStoreException>(() => new RDFMemoryStore().ToStreamAsync(RDFStoreEnums.RDFFormats.NQuads, null));

    [TestMethod]
    public async Task ShouldExportToDataTableAsync()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFQuadruple quadruple1 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext("http://ctx/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        store.AddQuadruple(quadruple1).AddQuadruple(quadruple2);
        DataTable table = await store.ToDataTableAsync();

        Assert.IsNotNull(table);
        Assert.AreEqual(4, table.Columns.Count);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.AreEqual(2, table.Rows.Count);
        Assert.IsTrue(table.Rows[0]["?CONTEXT"].ToString().Equals("http://ctx/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?SUBJECT"].ToString().Equals("http://subj/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?PREDICATE"].ToString().Equals("http://pred/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?CONTEXT"].ToString().Equals("http://ctx/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?SUBJECT"].ToString().Equals("http://subj/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?PREDICATE"].ToString().Equals("http://pred/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?OBJECT"].ToString().Equals("http://obj/", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ShouldExportEmptyToDataTableAsync()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        DataTable table = await store.ToDataTableAsync();

        Assert.IsNotNull(table);
        Assert.AreEqual(4, table.Columns.Count);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.AreEqual(0, table.Rows.Count);
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFMemoryStoreTest_Should*"))
            File.Delete(file);
    }

    public TestContext TestContext { get; set; }
    #endregion
}