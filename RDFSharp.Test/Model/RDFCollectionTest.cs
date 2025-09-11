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
using RDFSharp.Query;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFCollectionTest
{
    #region Tests
    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldCreateEmptyCollection(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);

        Assert.IsNotNull(coll);
        Assert.AreEqual(itemType, coll.ItemType);
        Assert.AreEqual(0, coll.ItemsCount);
        Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
        Assert.IsTrue(coll.InternalReificationSubject.IsBlank);
        Assert.IsFalse(coll.AcceptDuplicates);

        int i = coll.Count();
        Assert.AreEqual(0, i);

        int j = 0;
        IEnumerator<RDFPatternMember> itemsEnumerator = coll.ItemsEnumerator;
        while (itemsEnumerator.MoveNext()) j++;
        Assert.AreEqual(0, j);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldEnumerateCollection(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
            coll.AddItem(new RDFPlainLiteral("lit"));
        else
            coll.AddItem(new RDFResource("http://item/"));

        int i = coll.Count();
        Assert.AreEqual(1, i);

        int j = 0;
        IEnumerator<RDFPatternMember> itemsEnumerator = coll.ItemsEnumerator;
        while (itemsEnumerator.MoveNext()) j++;
        Assert.AreEqual(1, j);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldAddItemsToCollection(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            coll.AddItem(new RDFPlainLiteral("lit"));
            coll.AddItem(new RDFPlainLiteral("lit"));
        }
        else
        {
            coll.AddItem(new RDFResource("http://item/"));
            coll.AddItem(new RDFResource("http://item/"));
        }

        Assert.AreEqual(1, coll.ItemsCount);
        Assert.IsFalse(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldNotAddItemsToCollectionBecauseWrongType(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
            coll.AddItem(new RDFResource("http://item/"));
        else
            coll.AddItem(new RDFPlainLiteral("lit"));

        Assert.AreEqual(0, coll.ItemsCount);
        Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldNotAddItemsToCollectionBecauseNull(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
            coll.AddItem(null as RDFLiteral);
        else
            coll.AddItem(null as RDFResource);

        Assert.AreEqual(0, coll.ItemsCount);
        Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldRemoveItemsFromCollection(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            coll.AddItem(new RDFPlainLiteral("lit"));
            coll.AddItem(new RDFPlainLiteral("lit"));
            coll.RemoveItem(new RDFPlainLiteral("lit"));
        }
        else
        {
            coll.AddItem(new RDFResource("http://item/"));
            coll.AddItem(new RDFResource("http://item/"));
            coll.RemoveItem(new RDFResource("http://item/"));
        }

        Assert.AreEqual(0, coll.ItemsCount);
        Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldNotRemoveItemsFromCollectionBecauseWrongType(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            coll.AddItem(new RDFPlainLiteral("lit"));
            coll.AddItem(new RDFPlainLiteral("lit"));
            coll.RemoveItem(new RDFResource("http://item/"));
        }
        else
        {
            coll.AddItem(new RDFResource("http://item/"));
            coll.AddItem(new RDFResource("http://item/"));
            coll.RemoveItem(new RDFPlainLiteral("lit"));
        }

        Assert.AreEqual(1, coll.ItemsCount);
        Assert.IsFalse(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldNotRemoveItemsFromCollectionBecauseNull(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            coll.AddItem(new RDFPlainLiteral("lit"));
            coll.AddItem(new RDFPlainLiteral("lit"));
            coll.RemoveItem(null as RDFLiteral);
        }
        else
        {
            coll.AddItem(new RDFResource("http://item/"));
            coll.AddItem(new RDFResource("http://item/"));
            coll.RemoveItem(null as RDFResource);
        }

        Assert.AreEqual(1, coll.ItemsCount);
        Assert.IsFalse(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldClearCollection(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            coll.AddItem(new RDFPlainLiteral("lit"));
            coll.AddItem(new RDFPlainLiteral("lit"));
        }
        else
        {
            coll.AddItem(new RDFResource("http://item/"));
            coll.AddItem(new RDFResource("http://item/"));
        }

        coll.ClearItems();
        Assert.AreEqual(0, coll.ItemsCount);
        Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldReifyEmptyCollection(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        RDFGraph graph = coll.ReifyCollection();

        Assert.IsNotNull(graph);
        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldReifyCollectionWithOneItem(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
            coll.AddItem(new RDFPlainLiteral("lit1"));
        else
            coll.AddItem(new RDFResource("http://item1/"));
        RDFGraph graph = coll.ReifyCollection();

        Assert.IsNotNull(graph);
        Assert.AreEqual(3, graph.TriplesCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(coll.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST)));

        switch (itemType)
        {
            case RDFModelEnums.RDFItemTypes.Literal:
                Assert.IsTrue(graph.ContainsTriple(new RDFTriple(coll.ReificationSubject, RDFVocabulary.RDF.FIRST, new RDFPlainLiteral("lit1"))));
                break;
            case RDFModelEnums.RDFItemTypes.Resource:
                Assert.IsTrue(graph.ContainsTriple(new RDFTriple(coll.ReificationSubject, RDFVocabulary.RDF.FIRST, new RDFResource("http://item1/"))));
                break;
        }
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(coll.ReificationSubject, RDFVocabulary.RDF.REST, RDFVocabulary.RDF.NIL)));
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
    public void ShouldReifyCollectionWithTwoItems(RDFModelEnums.RDFItemTypes itemType)
    {
        RDFCollection coll = new RDFCollection(itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            coll.AddItem(new RDFPlainLiteral("lit1"));
            coll.AddItem(new RDFPlainLiteral("lit2"));
        }
        else
        {
            coll.AddItem(new RDFResource("http://item1/"));
            coll.AddItem(new RDFResource("http://item2/"));
        }
        RDFGraph graph = coll.ReifyCollection();

        Assert.IsNotNull(graph);
        Assert.AreEqual(6, graph.TriplesCount);
        Assert.AreEqual(2, graph[p: RDFVocabulary.RDF.TYPE, o: RDFVocabulary.RDF.LIST].TriplesCount);
        Assert.AreEqual(2, graph[p: RDFVocabulary.RDF.FIRST].TriplesCount);
        Assert.AreEqual(2, graph[p: RDFVocabulary.RDF.REST].TriplesCount);
    }
    #endregion
}