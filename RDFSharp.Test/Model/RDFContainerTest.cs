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
        #endregion
    }
}