using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        #endregion
    }
}