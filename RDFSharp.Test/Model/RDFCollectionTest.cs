﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Test
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
        #endregion
    }
}