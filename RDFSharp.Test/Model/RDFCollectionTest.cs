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
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFCollectionTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
        [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
        public void ShouldCreateEmptyCollection(RDFModelEnums.RDFItemTypes itemType)
        {
            RDFCollection coll = new RDFCollection(itemType);

            Assert.IsNotNull(coll);
            Assert.IsTrue(coll.ItemType == itemType);
            Assert.IsTrue(coll.ItemsCount == 0);
            Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
            Assert.IsTrue(coll.InternalReificationSubject.IsBlank);
            Assert.IsFalse(coll.AcceptDuplicates);

            int i = 0;
            foreach (RDFPatternMember item in coll) i++;
            Assert.IsTrue(i == 0);

            int j = 0;
            IEnumerator<RDFPatternMember> itemsEnumerator = coll.ItemsEnumerator;
            while (itemsEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 0);
        }

        [DataTestMethod]
        [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
        [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
        public void ShouldEnumerateCollection(RDFModelEnums.RDFItemTypes itemType)
        {
            RDFCollection coll = new RDFCollection(itemType);
            if (itemType == RDFModelEnums.RDFItemTypes.Literal)
                coll.AddItem(new RDFPlainLiteral("lit"));
            else
                coll.AddItem(new RDFResource("http://item/"));

            int i = 0;
            foreach (RDFPatternMember item in coll) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFPatternMember> itemsEnumerator = coll.ItemsEnumerator;
            while (itemsEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [DataTestMethod]
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

            Assert.IsTrue(coll.ItemsCount == 1);
            Assert.IsFalse(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
        }

        [DataTestMethod]
        [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
        [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
        public void ShouldNotAddItemsToCollectionBecauseWrongType(RDFModelEnums.RDFItemTypes itemType)
        {
            RDFCollection coll = new RDFCollection(itemType);
            if (itemType == RDFModelEnums.RDFItemTypes.Literal)
                coll.AddItem(new RDFResource("http://item/"));
            else
                coll.AddItem(new RDFPlainLiteral("lit"));

            Assert.IsTrue(coll.ItemsCount == 0);
            Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
        }

        [DataTestMethod]
        [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
        [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
        public void ShouldNotAddItemsToCollectionBecauseNull(RDFModelEnums.RDFItemTypes itemType)
        {
            RDFCollection coll = new RDFCollection(itemType);
            if (itemType == RDFModelEnums.RDFItemTypes.Literal)
                coll.AddItem(null as RDFLiteral);
            else
                coll.AddItem(null as RDFResource);

            Assert.IsTrue(coll.ItemsCount == 0);
            Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
        }

        [DataTestMethod]
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

            Assert.IsTrue(coll.ItemsCount == 0);
            Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
        }

        [DataTestMethod]
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

            Assert.IsTrue(coll.ItemsCount == 1);
            Assert.IsFalse(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
        }

        [DataTestMethod]
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

            Assert.IsTrue(coll.ItemsCount == 1);
            Assert.IsFalse(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
        }

        [DataTestMethod]
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
            Assert.IsTrue(coll.ItemsCount == 0);
            Assert.IsTrue(coll.ReificationSubject.Equals(RDFVocabulary.RDF.NIL));
        }

        [DataTestMethod]
        [DataRow(RDFModelEnums.RDFItemTypes.Literal)]
        [DataRow(RDFModelEnums.RDFItemTypes.Resource)]
        public void ShouldReifyEmptyCollection(RDFModelEnums.RDFItemTypes itemType)
        {
            RDFCollection coll = new RDFCollection(itemType);
            RDFGraph graph = coll.ReifyCollection();

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [DataTestMethod]
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
            Assert.IsTrue(graph.TriplesCount == 3);
            Assert.IsTrue(graph.Triples.ContainsKey(new RDFTriple(coll.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST).TripleID));
            
            switch (itemType)
            {
                case RDFModelEnums.RDFItemTypes.Literal:
                    Assert.IsTrue(graph.Triples.ContainsKey(new RDFTriple(coll.ReificationSubject, RDFVocabulary.RDF.FIRST, new RDFPlainLiteral("lit1")).TripleID));
                    break;
                case RDFModelEnums.RDFItemTypes.Resource:
                    Assert.IsTrue(graph.Triples.ContainsKey(new RDFTriple(coll.ReificationSubject, RDFVocabulary.RDF.FIRST, new RDFResource("http://item1/")).TripleID));
                    break;
            }
            Assert.IsTrue(graph.Triples.ContainsKey(new RDFTriple(coll.ReificationSubject, RDFVocabulary.RDF.REST, RDFVocabulary.RDF.NIL).TripleID));
        }

        [DataTestMethod]
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
            Assert.IsTrue(graph.TriplesCount == 6);
            Assert.IsTrue(graph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)
                               .SelectTriplesByObject(RDFVocabulary.RDF.LIST)
                               .TriplesCount == 2);
            Assert.IsTrue(graph.SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST)
                               .TriplesCount == 2);
            Assert.IsTrue(graph.SelectTriplesByPredicate(RDFVocabulary.RDF.REST)
                               .TriplesCount == 2);
        }
        #endregion
    }
}