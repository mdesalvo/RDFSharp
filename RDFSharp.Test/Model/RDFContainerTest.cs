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
public class RDFContainerTest
{
    #region Tests
    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldCreateEmptyContainer(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);

        Assert.IsNotNull(cont);
        Assert.AreEqual(containerType, cont.ContainerType);
        Assert.AreEqual(itemType, cont.ItemType);
        Assert.AreEqual(0, cont.ItemsCount);
        Assert.IsTrue(cont.ReificationSubject.IsBlank);

        int i = cont.Count();
        Assert.AreEqual(0, i);

        int j = 0;
        IEnumerator<RDFPatternMember> itemsEnumerator = cont.ItemsEnumerator;
        while (itemsEnumerator.MoveNext()) j++;
        Assert.AreEqual(0, j);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldEnumerateContainer(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
            cont.AddItem(new RDFPlainLiteral("lit"));
        else
            cont.AddItem(new RDFResource("http://item/"));

        int i = cont.Count();
        Assert.AreEqual(1, i);

        int j = 0;
        IEnumerator<RDFPatternMember> itemsEnumerator = cont.ItemsEnumerator;
        while (itemsEnumerator.MoveNext()) j++;
        Assert.AreEqual(1, j);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldAddItemsToContainer(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            cont.AddItem(new RDFPlainLiteral("lit"));
            cont.AddItem(new RDFPlainLiteral("lit"));
        }
        else
        {
            cont.AddItem(new RDFResource("http://item/"));
            cont.AddItem(new RDFResource("http://item/"));
        }

        switch (containerType)
        {
            case RDFModelEnums.RDFContainerTypes.Alt:
                Assert.AreEqual(1, cont.ItemsCount);
                break;

            case RDFModelEnums.RDFContainerTypes.Bag:
            case RDFModelEnums.RDFContainerTypes.Seq:
                Assert.AreEqual(2, cont.ItemsCount);
                break;
        }
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldNotAddItemsToContainerBecauseWrongType(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
            cont.AddItem(new RDFResource("http://item/"));
        else
            cont.AddItem(new RDFPlainLiteral("lit"));

        Assert.AreEqual(0, cont.ItemsCount);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldNotAddItemsToContainerBecauseNull(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
            cont.AddItem(null as RDFLiteral);
        else
            cont.AddItem(null as RDFResource);

        Assert.AreEqual(0, cont.ItemsCount);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldRemoveItemsFromContainer(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            cont.AddItem(new RDFPlainLiteral("lit"));
            cont.AddItem(new RDFPlainLiteral("lit"));
            cont.RemoveItem(new RDFPlainLiteral("lit"));
        }
        else
        {
            cont.AddItem(new RDFResource("http://item/"));
            cont.AddItem(new RDFResource("http://item/"));
            cont.RemoveItem(new RDFResource("http://item/"));
        }

        Assert.AreEqual(0, cont.ItemsCount);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldNotRemoveItemsFromContainerBecauseWrongType(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            cont.AddItem(new RDFPlainLiteral("lit"));
            cont.AddItem(new RDFPlainLiteral("lit"));
            cont.RemoveItem(new RDFResource("http://item/"));
        }
        else
        {
            cont.AddItem(new RDFResource("http://item/"));
            cont.AddItem(new RDFResource("http://item/"));
            cont.RemoveItem(new RDFPlainLiteral("lit"));
        }

        switch (containerType)
        {
            case RDFModelEnums.RDFContainerTypes.Alt:
                Assert.AreEqual(1, cont.ItemsCount);
                break;

            case RDFModelEnums.RDFContainerTypes.Bag:
            case RDFModelEnums.RDFContainerTypes.Seq:
                Assert.AreEqual(2, cont.ItemsCount);
                break;
        }
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldNotRemoveItemsFromContainerBecauseNull(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            cont.AddItem(new RDFPlainLiteral("lit"));
            cont.AddItem(new RDFPlainLiteral("lit"));
            cont.RemoveItem(null as RDFLiteral);
        }
        else
        {
            cont.AddItem(new RDFResource("http://item/"));
            cont.AddItem(new RDFResource("http://item/"));
            cont.RemoveItem(null as RDFResource);
        }

        switch (containerType)
        {
            case RDFModelEnums.RDFContainerTypes.Alt:
                Assert.AreEqual(1, cont.ItemsCount);
                break;

            case RDFModelEnums.RDFContainerTypes.Bag:
            case RDFModelEnums.RDFContainerTypes.Seq:
                Assert.AreEqual(2, cont.ItemsCount);
                break;
        }
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldClearContainer(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            cont.AddItem(new RDFPlainLiteral("lit"));
            cont.AddItem(new RDFPlainLiteral("lit"));
        }
        else
        {
            cont.AddItem(new RDFResource("http://item/"));
            cont.AddItem(new RDFResource("http://item/"));
        }

        cont.ClearItems();
        Assert.AreEqual(0, cont.ItemsCount);
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldReifyEmptyContainer(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        RDFGraph graph = cont.ReifyContainer();

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
        switch (containerType)
        {
            case RDFModelEnums.RDFContainerTypes.Alt:
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT).TripleID));
                break;
            case RDFModelEnums.RDFContainerTypes.Bag:
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.BAG).TripleID));
                break;
            case RDFModelEnums.RDFContainerTypes.Seq:
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.SEQ).TripleID));
                break;
        }
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Bag, RDFModelEnums.RDFItemTypes.Literal)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Resource)]
    [DataRow(RDFModelEnums.RDFContainerTypes.Seq, RDFModelEnums.RDFItemTypes.Literal)]
    public void ShouldReifyContainer(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
    {
        RDFContainer cont = new RDFContainer(containerType, itemType);
        if (itemType == RDFModelEnums.RDFItemTypes.Literal)
        {
            cont.AddItem(new RDFPlainLiteral("lit1"));
            cont.AddItem(new RDFPlainLiteral("lit2"));
        }
        else
        {
            cont.AddItem(new RDFResource("http://item1/"));
            cont.AddItem(new RDFResource("http://item2/"));
        }
        RDFGraph graph = cont.ReifyContainer();

        Assert.IsNotNull(graph);
        Assert.AreEqual(3, graph.TriplesCount);
        switch (containerType)
        {
            case RDFModelEnums.RDFContainerTypes.Alt:
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT).TripleID));
                break;
            case RDFModelEnums.RDFContainerTypes.Bag:
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.BAG).TripleID));
                break;
            case RDFModelEnums.RDFContainerTypes.Seq:
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.SEQ).TripleID));
                break;
        }
        switch (itemType)
        {
            case RDFModelEnums.RDFItemTypes.Literal:
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, new RDFResource($"{RDFVocabulary.RDF.BASE_URI}_1"), new RDFPlainLiteral("lit1")).TripleID));
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, new RDFResource($"{RDFVocabulary.RDF.BASE_URI}_2"), new RDFPlainLiteral("lit2")).TripleID));
                break;
            case RDFModelEnums.RDFItemTypes.Resource:
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, new RDFResource($"{RDFVocabulary.RDF.BASE_URI}_1"), new RDFResource("http://item1/")).TripleID));
                Assert.IsTrue(graph.Index.Hashes.ContainsKey(new RDFTriple(cont.ReificationSubject, new RDFResource($"{RDFVocabulary.RDF.BASE_URI}_2"), new RDFResource("http://item2/")).TripleID));
                break;
        }
    }
    #endregion
}