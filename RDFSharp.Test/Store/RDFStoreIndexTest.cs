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

using RDFSharp.Model;
using RDFSharp.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace RDFSharp.Test.Store;

[TestClass]
public class RDFStoreIndexTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateStoreIndex()
    {
        RDFStoreIndex storeIndex = new RDFStoreIndex();

        Assert.IsNotNull(storeIndex);
        Assert.IsNotNull(storeIndex.ContextsRegister);
        Assert.AreEqual(0, storeIndex.ContextsRegister.Count);
        Assert.IsNotNull(storeIndex.ResourcesRegister);
        Assert.AreEqual(0, storeIndex.ResourcesRegister.Count);
        Assert.IsNotNull(storeIndex.LiteralsRegister);
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.IsNotNull(storeIndex.ContextsIndex);
        Assert.AreEqual(0, storeIndex.ContextsIndex.Count);
        Assert.IsNotNull(storeIndex.SubjectsIndex);
        Assert.AreEqual(0, storeIndex.SubjectsIndex.Count);
        Assert.IsNotNull(storeIndex.PredicatesIndex);
        Assert.AreEqual(0, storeIndex.PredicatesIndex.Count);
        Assert.IsNotNull(storeIndex.ObjectsIndex);
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.IsNotNull(storeIndex.LiteralsIndex);
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddCSPOIndex()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);

        Assert.AreEqual(1, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx.PatternMemberID));
        Assert.AreEqual(3, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(1, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx.PatternMemberID].Contains(quadruple.QuadrupleID));
        Assert.AreEqual(1, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj.PatternMemberID].Contains(quadruple.QuadrupleID));
        Assert.AreEqual(1, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred.PatternMemberID].Contains(quadruple.QuadrupleID));
        Assert.AreEqual(1, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj.PatternMemberID].Contains(quadruple.QuadrupleID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddCSPLIndex()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);

        Assert.AreEqual(1, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx.PatternMemberID));
        Assert.AreEqual(2, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred.PatternMemberID));
        Assert.AreEqual(1, storeIndex.LiteralsRegister.Count);
        Assert.IsTrue(storeIndex.LiteralsRegister.ContainsKey(lit.PatternMemberID));
        Assert.AreEqual(1, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx.PatternMemberID].Contains(quadruple.QuadrupleID));
        Assert.AreEqual(1, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj.PatternMemberID].Contains(quadruple.QuadrupleID));
        Assert.AreEqual(1, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred.PatternMemberID].Contains(quadruple.QuadrupleID));
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(1, storeIndex.LiteralsIndex.Count);
        Assert.IsTrue(storeIndex.LiteralsIndex[lit.PatternMemberID].Contains(quadruple.QuadrupleID));
    }

    [TestMethod]
    public void ShouldDisposeStoreIndexWithUsing()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx, subj, pred, obj);
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex;

        using (storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2))
        {
            Assert.IsFalse(storeIndex.Disposed);
            Assert.IsNotNull(storeIndex.ContextsRegister);
            Assert.IsNotNull(storeIndex.ResourcesRegister);
            Assert.IsNotNull(storeIndex.LiteralsRegister);
            Assert.IsNotNull(storeIndex.ContextsIndex);
            Assert.IsNotNull(storeIndex.SubjectsIndex);
            Assert.IsNotNull(storeIndex.PredicatesIndex);
            Assert.IsNotNull(storeIndex.ObjectsIndex);
            Assert.IsNotNull(storeIndex.LiteralsIndex);
        }
        Assert.IsTrue(storeIndex.Disposed);
        Assert.IsNull(storeIndex.ContextsRegister);
        Assert.IsNull(storeIndex.ResourcesRegister);
        Assert.IsNull(storeIndex.LiteralsRegister);
        Assert.IsNull(storeIndex.ContextsIndex);
        Assert.IsNull(storeIndex.SubjectsIndex);
        Assert.IsNull(storeIndex.PredicatesIndex);
        Assert.IsNull(storeIndex.ObjectsIndex);
        Assert.IsNull(storeIndex.LiteralsIndex);
    }

    [TestMethod]
    public void ShouldAddSameContextMultipleTimes()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx, subj1, pred1, obj1);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx, subj2, pred2, obj2);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple1).AddIndex(quadruple2);

        Assert.AreEqual(1, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx.PatternMemberID));
        Assert.AreEqual(6, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred2.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj2.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(1, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ContextsIndex[ctx.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.SubjectsIndex[subj2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.PredicatesIndex[pred2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ObjectsIndex[obj2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameSubjectMultipleTimes()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj, pred1, obj1);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj, pred2, obj2);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple1).AddIndex(quadruple2);

        Assert.AreEqual(2, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx1.PatternMemberID));
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx2.PatternMemberID));
        Assert.AreEqual(5, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred2.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj2.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(2, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ContextsIndex[ctx2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(1, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.SubjectsIndex[subj.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.PredicatesIndex[pred2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ObjectsIndex[obj2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSamePredicateMultipleTimes()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred, obj1);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred, obj2);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple1).AddIndex(quadruple2);

        Assert.AreEqual(2, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx1.PatternMemberID));
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx2.PatternMemberID));
        Assert.AreEqual(5, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj2.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(2, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ContextsIndex[ctx2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.SubjectsIndex[subj2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(1, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.PredicatesIndex[pred.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ObjectsIndex[obj2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameObjectMultipleTimes()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred1, obj);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred2, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple1).AddIndex(quadruple2);

        Assert.AreEqual(2, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx1.PatternMemberID));
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx2.PatternMemberID));
        Assert.AreEqual(5, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred2.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(2, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ContextsIndex[ctx2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.SubjectsIndex[subj2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.PredicatesIndex[pred2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(1, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ObjectsIndex[obj.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameLiteralMultipleTimes()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred1, lit);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred2, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple1).AddIndex(quadruple2);

        Assert.AreEqual(2, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx1.PatternMemberID));
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx2.PatternMemberID));
        Assert.AreEqual(4, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.LiteralsRegister.Count);
        Assert.IsTrue(storeIndex.LiteralsRegister.ContainsKey(lit.PatternMemberID));
        Assert.AreEqual(2, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.ContextsIndex[ctx2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.SubjectsIndex[subj2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(2, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.PredicatesIndex[pred2.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);            
        Assert.AreEqual(1, storeIndex.LiteralsIndex.Count);
        Assert.IsTrue(storeIndex.LiteralsIndex[lit.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsTrue(storeIndex.LiteralsIndex[lit.PatternMemberID].Contains(quadruple2.QuadrupleID));
    }

    [TestMethod]
    public void ShouldRemoveCSPOIndex()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple).RemoveIndex(quadruple);

        Assert.AreEqual(0, storeIndex.ContextsRegister.Count);
        Assert.AreEqual(0, storeIndex.ResourcesRegister.Count);
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(0, storeIndex.ContextsIndex.Count);
        Assert.AreEqual(0, storeIndex.SubjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.PredicatesIndex.Count);
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldRemoveCSPLIndex()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple).RemoveIndex(quadruple);

        Assert.AreEqual(0, storeIndex.ContextsRegister.Count);
        Assert.AreEqual(0, storeIndex.ResourcesRegister.Count);
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(0, storeIndex.ContextsIndex.Count);
        Assert.AreEqual(0, storeIndex.SubjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.PredicatesIndex.Count);
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameContextMultipleTimesAndRemoveOneOccurrence()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx, subj1, pred1, obj1);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx, subj2, pred2, obj2);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2);

        Assert.AreEqual(1, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx.PatternMemberID));
        Assert.AreEqual(3, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(1, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ContextsIndex[ctx.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(1, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(1, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.PredicatesIndex.ContainsKey(pred2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ObjectsIndex.ContainsKey(obj2.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameSubjectMultipleTimesAndRemoveOneOccurrence()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj, pred1, obj1);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj, pred2, obj2);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2);

        Assert.AreEqual(1, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx1.PatternMemberID));
        Assert.AreEqual(3, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(1, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ContextsIndex.ContainsKey(ctx2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.SubjectsIndex[subj.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(1, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.PredicatesIndex.ContainsKey(pred2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ObjectsIndex.ContainsKey(obj2.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameSubjectMultipleTimesAndRemoveEveryOccurrences()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj, pred1, obj1);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj, pred2, obj2);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2).RemoveIndex(quadruple2).RemoveIndex(quadruple1);

        Assert.AreEqual(0, storeIndex.ContextsRegister.Count);
        Assert.AreEqual(0, storeIndex.ResourcesRegister.Count);
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(0, storeIndex.ContextsIndex.Count);
        Assert.AreEqual(0, storeIndex.SubjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.PredicatesIndex.Count);
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSamePredicateMultipleTimesAndRemoveOneOccurrence()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred, obj1);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred, obj2);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2);

        Assert.AreEqual(1, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx1.PatternMemberID));
        Assert.AreEqual(3, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(1, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ContextsIndex.ContainsKey(ctx2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.SubjectsIndex.ContainsKey(subj2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.PredicatesIndex[pred.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(1, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ObjectsIndex.ContainsKey(obj2.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSamePredicateMultipleTimesAndRemoveEveryOccurrence()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred, obj1);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred, obj2);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2).RemoveIndex(quadruple2).RemoveIndex(quadruple1);

        Assert.AreEqual(0, storeIndex.ContextsRegister.Count);
        Assert.AreEqual(0, storeIndex.ResourcesRegister.Count);
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(0, storeIndex.ContextsIndex.Count);
        Assert.AreEqual(0, storeIndex.SubjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.PredicatesIndex.Count);
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameObjectMultipleTimesAndRemoveOneOccurrence()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred1, obj);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred2, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2);

        Assert.AreEqual(1, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx1.PatternMemberID));
        Assert.AreEqual(3, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(obj.PatternMemberID));
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(1, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ContextsIndex.ContainsKey(ctx2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.SubjectsIndex.ContainsKey(subj2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.PredicatesIndex.ContainsKey(pred2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.ObjectsIndex.Count);
        Assert.IsTrue(storeIndex.ObjectsIndex[obj.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ObjectsIndex[obj.PatternMemberID].Contains(quadruple2.QuadrupleID));
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameObjectMultipleTimesAndRemoveEveryOccurrence()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred1, obj);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred2, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2).RemoveIndex(quadruple2).RemoveIndex(quadruple1);

        Assert.AreEqual(0, storeIndex.ContextsRegister.Count);
        Assert.AreEqual(0, storeIndex.ResourcesRegister.Count);
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(0, storeIndex.ContextsIndex.Count);
        Assert.AreEqual(0, storeIndex.SubjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.PredicatesIndex.Count);
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldAddSameLiteralMultipleTimesAndRemoveOneOccurrence()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred1, lit);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred2, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2);

        Assert.AreEqual(1, storeIndex.ContextsRegister.Count);
        Assert.IsTrue(storeIndex.ContextsRegister.ContainsKey(ctx1.PatternMemberID));
        Assert.AreEqual(2, storeIndex.ResourcesRegister.Count);
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.AreEqual(1, storeIndex.LiteralsRegister.Count);
        Assert.IsTrue(storeIndex.LiteralsRegister.ContainsKey(lit.PatternMemberID));
        Assert.AreEqual(1, storeIndex.ContextsIndex.Count);
        Assert.IsTrue(storeIndex.ContextsIndex[ctx1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.ContextsIndex.ContainsKey(ctx2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.SubjectsIndex.Count);
        Assert.IsTrue(storeIndex.SubjectsIndex[subj1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.SubjectsIndex.ContainsKey(subj2.PatternMemberID));
        Assert.AreEqual(1, storeIndex.PredicatesIndex.Count);
        Assert.IsTrue(storeIndex.PredicatesIndex[pred1.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.PredicatesIndex.ContainsKey(pred2.PatternMemberID));
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(1, storeIndex.LiteralsIndex.Count);
        Assert.IsTrue(storeIndex.LiteralsIndex[lit.PatternMemberID].Contains(quadruple1.QuadrupleID));
        Assert.IsFalse(storeIndex.LiteralsIndex[lit.PatternMemberID].Contains(quadruple2.QuadrupleID));
    }

    [TestMethod]
    public void ShouldAddSameLiteralMultipleTimesAndRemoveEveryOccurrence()
    {
        RDFContext ctx1 = new RDFContext("http://ctx1/");
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx1, subj1, pred1, lit);
        RDFContext ctx2 = new RDFContext("http://ctx2/");
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx2, subj2, pred2, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2).RemoveIndex(quadruple2).RemoveIndex(quadruple2).RemoveIndex(quadruple1);

        Assert.AreEqual(0, storeIndex.ContextsRegister.Count);
        Assert.AreEqual(0, storeIndex.ResourcesRegister.Count);
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(0, storeIndex.ContextsIndex.Count);
        Assert.AreEqual(0, storeIndex.SubjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.PredicatesIndex.Count);
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldClearIndex()
    {
        RDFContext ctx = new RDFContext();
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFQuadruple quadruple1 = new RDFQuadruple(ctx, subj, pred, obj);
        RDFQuadruple quadruple2 = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple1).AddIndex(quadruple2);
        storeIndex.ClearIndex();

        Assert.AreEqual(0, storeIndex.ContextsRegister.Count);
        Assert.AreEqual(0, storeIndex.ResourcesRegister.Count);
        Assert.AreEqual(0, storeIndex.LiteralsRegister.Count);
        Assert.AreEqual(0, storeIndex.ContextsIndex.Count);
        Assert.AreEqual(0, storeIndex.SubjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.PredicatesIndex.Count);
        Assert.AreEqual(0, storeIndex.ObjectsIndex.Count);
        Assert.AreEqual(0, storeIndex.LiteralsIndex.Count);
    }

    [TestMethod]
    public void ShouldSelectIndexByContext()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenContext = storeIndex.SelectIndexByContext(ctx);

        Assert.IsNotNull(quadruplesWithGivenContext);
        Assert.AreEqual(1, quadruplesWithGivenContext.Count);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundContext()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenContext = storeIndex.SelectIndexByContext(new RDFContext("http://ctx2/"));

        Assert.IsNotNull(quadruplesWithGivenContext);
        Assert.AreEqual(0, quadruplesWithGivenContext.Count);
    }

    [TestMethod]
    public void ShouldSelectIndexBySubject()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenSubject = storeIndex.SelectIndexBySubject(subj);

        Assert.IsNotNull(quadruplesWithGivenSubject);
        Assert.AreEqual(1, quadruplesWithGivenSubject.Count);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundSubject()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenSubject = storeIndex.SelectIndexBySubject(new RDFResource("http://subj2/"));

        Assert.IsNotNull(quadruplesWithGivenSubject);
        Assert.AreEqual(0, quadruplesWithGivenSubject.Count);
    }

    [TestMethod]
    public void ShouldSelectIndexByPredicate()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenPredicate = storeIndex.SelectIndexByPredicate(pred);

        Assert.IsNotNull(quadruplesWithGivenPredicate);
        Assert.AreEqual(1, quadruplesWithGivenPredicate.Count);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundPredicate()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenPredicate = storeIndex.SelectIndexByPredicate(new RDFResource("http://pred2/"));

        Assert.IsNotNull(quadruplesWithGivenPredicate);
        Assert.AreEqual(0, quadruplesWithGivenPredicate.Count);
    }

    [TestMethod]
    public void ShouldSelectIndexByObject()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenObject = storeIndex.SelectIndexByObject(obj);

        Assert.IsNotNull(quadruplesWithGivenObject);
        Assert.AreEqual(1, quadruplesWithGivenObject.Count);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundObject()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenObject = storeIndex.SelectIndexByObject(new RDFResource("http://subj2/"));

        Assert.IsNotNull(quadruplesWithGivenObject);
        Assert.AreEqual(0, quadruplesWithGivenObject.Count);
    }

    [TestMethod]
    public void ShouldSelectIndexByLiteral()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenLiteral = storeIndex.SelectIndexByLiteral(lit);

        Assert.IsNotNull(quadruplesWithGivenLiteral);
        Assert.AreEqual(1, quadruplesWithGivenLiteral.Count);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundLiteral()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().AddIndex(quadruple);
        HashSet<long> quadruplesWithGivenLiteral = storeIndex.SelectIndexByLiteral(new RDFPlainLiteral("lit2", "en-US"));

        Assert.IsNotNull(quadruplesWithGivenLiteral);
        Assert.AreEqual(0, quadruplesWithGivenLiteral.Count);
    }
    #endregion
}