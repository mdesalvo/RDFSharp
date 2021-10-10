﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Test
{
    [TestClass]
    public class RDFContainerTest
    {
        #region Tests
        [DataTestMethod]
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
            Assert.IsTrue(cont.ContainerType == containerType);
            Assert.IsTrue(cont.ItemType == itemType);
            Assert.IsTrue(cont.ItemsCount == 0);
            Assert.IsTrue(cont.ReificationSubject.IsBlank);

            int i = 0;
            foreach (RDFPatternMember item in cont) i++;
            Assert.IsTrue(i == 0);

            int j = 0;
            IEnumerator<RDFPatternMember> itemsEnumerator = cont.ItemsEnumerator;
            while (itemsEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 0);
        }

        [DataTestMethod]
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

            int i = 0;
            foreach (RDFPatternMember item in cont) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFPatternMember> itemsEnumerator = cont.ItemsEnumerator;
            while (itemsEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [DataTestMethod]
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
                    Assert.IsTrue(cont.ItemsCount == 1);
                    break;

                case RDFModelEnums.RDFContainerTypes.Bag:
                case RDFModelEnums.RDFContainerTypes.Seq:
                    Assert.IsTrue(cont.ItemsCount == 2);
                    break;
            }
        }

        [DataTestMethod]
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
            
            Assert.IsTrue(cont.ItemsCount == 0);
        }

        [DataTestMethod]
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

            Assert.IsTrue(cont.ItemsCount == 0);
        }

        [DataTestMethod]
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

            Assert.IsTrue(cont.ItemsCount == 0);
        }

        [DataTestMethod]
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
                    Assert.IsTrue(cont.ItemsCount == 1);
                    break;

                case RDFModelEnums.RDFContainerTypes.Bag:
                case RDFModelEnums.RDFContainerTypes.Seq:
                    Assert.IsTrue(cont.ItemsCount == 2);
                    break;
            }
        }

        [DataTestMethod]
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
                    Assert.IsTrue(cont.ItemsCount == 1);
                    break;

                case RDFModelEnums.RDFContainerTypes.Bag:
                case RDFModelEnums.RDFContainerTypes.Seq:
                    Assert.IsTrue(cont.ItemsCount == 2);
                    break;
            }
        }

        [DataTestMethod]
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
            Assert.IsTrue(cont.ItemsCount == 0);
        }
        #endregion
    }
}