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
        Assert.IsNotNull(storeIndex.Contexts);
        Assert.IsEmpty(storeIndex.Contexts);
        Assert.IsNotNull(storeIndex.Resources);
        Assert.IsEmpty(storeIndex.Resources);
        Assert.IsNotNull(storeIndex.Literals);
        Assert.IsEmpty(storeIndex.Literals);
        Assert.IsNotNull(storeIndex.IDXContexts);
        Assert.IsEmpty(storeIndex.IDXContexts);
        Assert.IsNotNull(storeIndex.IDXSubjects);
        Assert.IsEmpty(storeIndex.IDXSubjects);
        Assert.IsNotNull(storeIndex.IDXPredicates);
        Assert.IsEmpty(storeIndex.IDXPredicates);
        Assert.IsNotNull(storeIndex.IDXObjects);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.IsNotNull(storeIndex.IDXLiterals);
        Assert.IsEmpty(storeIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddCSPOIndex()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);

        Assert.HasCount(1, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx.PatternMemberID));
        Assert.HasCount(3, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(1, storeIndex.IDXContexts);
        Assert.Contains(quadruple.QuadrupleID, storeIndex.IDXContexts[ctx.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXSubjects);
        Assert.Contains(quadruple.QuadrupleID, storeIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXPredicates);
        Assert.Contains(quadruple.QuadrupleID, storeIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXObjects);
        Assert.Contains(quadruple.QuadrupleID, storeIndex.IDXObjects[obj.PatternMemberID]);
        Assert.IsEmpty(storeIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddCSPLIndex()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);

        Assert.HasCount(1, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx.PatternMemberID));
        Assert.HasCount(2, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred.PatternMemberID));
        Assert.HasCount(1, storeIndex.Literals);
        Assert.IsTrue(storeIndex.Literals.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXContexts);
        Assert.Contains(quadruple.QuadrupleID, storeIndex.IDXContexts[ctx.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXSubjects);
        Assert.Contains(quadruple.QuadrupleID, storeIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXPredicates);
        Assert.Contains(quadruple.QuadrupleID, storeIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.HasCount(1, storeIndex.IDXLiterals);
        Assert.Contains(quadruple.QuadrupleID, storeIndex.IDXLiterals[lit.PatternMemberID]);
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

        using (storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2))
        {
            Assert.IsFalse(storeIndex.Disposed);
            Assert.IsNotNull(storeIndex.Contexts);
            Assert.IsNotNull(storeIndex.Resources);
            Assert.IsNotNull(storeIndex.Literals);
            Assert.IsNotNull(storeIndex.IDXContexts);
            Assert.IsNotNull(storeIndex.IDXSubjects);
            Assert.IsNotNull(storeIndex.IDXPredicates);
            Assert.IsNotNull(storeIndex.IDXObjects);
            Assert.IsNotNull(storeIndex.IDXLiterals);
        }
        Assert.IsTrue(storeIndex.Disposed);
        Assert.IsNull(storeIndex.Contexts);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple1).Add(quadruple2);

        Assert.HasCount(1, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx.PatternMemberID));
        Assert.HasCount(6, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred2.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(1, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXContexts[ctx.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXSubjects[subj2.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXPredicates[pred2.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXObjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXObjects[obj2.PatternMemberID]);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple1).Add(quadruple2);

        Assert.HasCount(2, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx1.PatternMemberID));
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx2.PatternMemberID));
        Assert.HasCount(5, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred2.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(2, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXContexts[ctx2.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXPredicates[pred2.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXObjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXObjects[obj2.PatternMemberID]);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple1).Add(quadruple2);

        Assert.HasCount(2, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx1.PatternMemberID));
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx2.PatternMemberID));
        Assert.HasCount(5, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(2, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXContexts[ctx2.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXSubjects[subj2.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXObjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXObjects[obj2.PatternMemberID]);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple1).Add(quadruple2);

        Assert.HasCount(2, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx1.PatternMemberID));
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx2.PatternMemberID));
        Assert.HasCount(5, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred2.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(2, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXContexts[ctx2.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXSubjects[subj2.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXPredicates[pred2.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXObjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXObjects[obj.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXObjects[obj.PatternMemberID]);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple1).Add(quadruple2);

        Assert.HasCount(2, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx1.PatternMemberID));
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx2.PatternMemberID));
        Assert.HasCount(4, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, storeIndex.Literals);
        Assert.IsTrue(storeIndex.Literals.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(2, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXContexts[ctx2.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXSubjects[subj2.PatternMemberID]);
        Assert.HasCount(2, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXPredicates[pred2.PatternMemberID]);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.HasCount(1, storeIndex.IDXLiterals);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXLiterals[lit.PatternMemberID]);
        Assert.Contains(quadruple2.QuadrupleID, storeIndex.IDXLiterals[lit.PatternMemberID]);
    }

    [TestMethod]
    public void ShouldRemoveCSPOIndex()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple).Remove(quadruple);

        Assert.IsEmpty(storeIndex.Contexts);
        Assert.IsEmpty(storeIndex.Resources);
        Assert.IsEmpty(storeIndex.Literals);
        Assert.IsEmpty(storeIndex.IDXContexts);
        Assert.IsEmpty(storeIndex.IDXSubjects);
        Assert.IsEmpty(storeIndex.IDXPredicates);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.IsEmpty(storeIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldRemoveCSPLIndex()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple).Remove(quadruple);

        Assert.IsEmpty(storeIndex.Contexts);
        Assert.IsEmpty(storeIndex.Resources);
        Assert.IsEmpty(storeIndex.Literals);
        Assert.IsEmpty(storeIndex.IDXContexts);
        Assert.IsEmpty(storeIndex.IDXSubjects);
        Assert.IsEmpty(storeIndex.IDXPredicates);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2);

        Assert.HasCount(1, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx.PatternMemberID));
        Assert.HasCount(3, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(1, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx.PatternMemberID]);
        Assert.DoesNotContain(quadruple2.QuadrupleID, storeIndex.IDXContexts[ctx.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.DoesNotContain(quadruple2.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXPredicates.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXObjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXObjects.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2);

        Assert.HasCount(1, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx1.PatternMemberID));
        Assert.HasCount(3, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(1, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXContexts.ContainsKey(ctx2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.DoesNotContain(quadruple2.QuadrupleID, storeIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXPredicates.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXObjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXObjects.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2).Remove(quadruple2).Remove(quadruple1);

        Assert.IsEmpty(storeIndex.Contexts);
        Assert.IsEmpty(storeIndex.Resources);
        Assert.IsEmpty(storeIndex.Literals);
        Assert.IsEmpty(storeIndex.IDXContexts);
        Assert.IsEmpty(storeIndex.IDXSubjects);
        Assert.IsEmpty(storeIndex.IDXPredicates);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2);

        Assert.HasCount(1, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx1.PatternMemberID));
        Assert.HasCount(3, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(1, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXContexts.ContainsKey(ctx2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXSubjects.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.DoesNotContain(quadruple2.QuadrupleID, storeIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.HasCount(1, storeIndex.IDXObjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXObjects.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2).Remove(quadruple2).Remove(quadruple1);

        Assert.IsEmpty(storeIndex.Contexts);
        Assert.IsEmpty(storeIndex.Resources);
        Assert.IsEmpty(storeIndex.Literals);
        Assert.IsEmpty(storeIndex.IDXContexts);
        Assert.IsEmpty(storeIndex.IDXSubjects);
        Assert.IsEmpty(storeIndex.IDXPredicates);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2);

        Assert.HasCount(1, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx1.PatternMemberID));
        Assert.HasCount(3, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(obj.PatternMemberID));
        Assert.IsEmpty(storeIndex.Literals);
        Assert.HasCount(1, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXContexts.ContainsKey(ctx2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXSubjects.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXPredicates.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXObjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXObjects[obj.PatternMemberID]);
        Assert.DoesNotContain(quadruple2.QuadrupleID, storeIndex.IDXObjects[obj.PatternMemberID]);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2).Remove(quadruple2).Remove(quadruple1);

        Assert.IsEmpty(storeIndex.Contexts);
        Assert.IsEmpty(storeIndex.Resources);
        Assert.IsEmpty(storeIndex.Literals);
        Assert.IsEmpty(storeIndex.IDXContexts);
        Assert.IsEmpty(storeIndex.IDXSubjects);
        Assert.IsEmpty(storeIndex.IDXPredicates);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2);

        Assert.HasCount(1, storeIndex.Contexts);
        Assert.IsTrue(storeIndex.Contexts.ContainsKey(ctx1.PatternMemberID));
        Assert.HasCount(2, storeIndex.Resources);
        Assert.IsTrue(storeIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(storeIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.HasCount(1, storeIndex.Literals);
        Assert.IsTrue(storeIndex.Literals.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXContexts);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXContexts[ctx1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXContexts.ContainsKey(ctx2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXSubjects);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXSubjects.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, storeIndex.IDXPredicates);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.IsFalse(storeIndex.IDXPredicates.ContainsKey(pred2.PatternMemberID));
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.HasCount(1, storeIndex.IDXLiterals);
        Assert.Contains(quadruple1.QuadrupleID, storeIndex.IDXLiterals[lit.PatternMemberID]);
        Assert.DoesNotContain(quadruple2.QuadrupleID, storeIndex.IDXLiterals[lit.PatternMemberID]);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2).Remove(quadruple2).Remove(quadruple2).Remove(quadruple1);

        Assert.IsEmpty(storeIndex.Contexts);
        Assert.IsEmpty(storeIndex.Resources);
        Assert.IsEmpty(storeIndex.Literals);
        Assert.IsEmpty(storeIndex.IDXContexts);
        Assert.IsEmpty(storeIndex.IDXSubjects);
        Assert.IsEmpty(storeIndex.IDXPredicates);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.IsEmpty(storeIndex.IDXLiterals);
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
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple1).Add(quadruple2);
        storeIndex.Clear();

        Assert.IsEmpty(storeIndex.Contexts);
        Assert.IsEmpty(storeIndex.Resources);
        Assert.IsEmpty(storeIndex.Literals);
        Assert.IsEmpty(storeIndex.IDXContexts);
        Assert.IsEmpty(storeIndex.IDXSubjects);
        Assert.IsEmpty(storeIndex.IDXPredicates);
        Assert.IsEmpty(storeIndex.IDXObjects);
        Assert.IsEmpty(storeIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldSelectIndexByContext()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenContext = storeIndex.LookupIndexByContext(ctx);

        Assert.IsNotNull(quadruplesWithGivenContext);
        Assert.HasCount(1, quadruplesWithGivenContext);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundContext()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenContext = storeIndex.LookupIndexByContext(new RDFContext("http://ctx2/"));

        Assert.IsNotNull(quadruplesWithGivenContext);
        Assert.IsEmpty(quadruplesWithGivenContext);
    }

    [TestMethod]
    public void ShouldSelectIndexBySubject()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenSubject = storeIndex.LookupIndexBySubject(subj);

        Assert.IsNotNull(quadruplesWithGivenSubject);
        Assert.HasCount(1, quadruplesWithGivenSubject);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundSubject()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenSubject = storeIndex.LookupIndexBySubject(new RDFResource("http://subj2/"));

        Assert.IsNotNull(quadruplesWithGivenSubject);
        Assert.IsEmpty(quadruplesWithGivenSubject);
    }

    [TestMethod]
    public void ShouldSelectIndexByPredicate()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenPredicate = storeIndex.LookupIndexByPredicate(pred);

        Assert.IsNotNull(quadruplesWithGivenPredicate);
        Assert.HasCount(1, quadruplesWithGivenPredicate);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundPredicate()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenPredicate = storeIndex.LookupIndexByPredicate(new RDFResource("http://pred2/"));

        Assert.IsNotNull(quadruplesWithGivenPredicate);
        Assert.IsEmpty(quadruplesWithGivenPredicate);
    }

    [TestMethod]
    public void ShouldSelectIndexByObject()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenObject = storeIndex.LookupIndexByObject(obj);

        Assert.IsNotNull(quadruplesWithGivenObject);
        Assert.HasCount(1, quadruplesWithGivenObject);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundObject()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenObject = storeIndex.LookupIndexByObject(new RDFResource("http://subj2/"));

        Assert.IsNotNull(quadruplesWithGivenObject);
        Assert.IsEmpty(quadruplesWithGivenObject);
    }

    [TestMethod]
    public void ShouldSelectIndexByLiteral()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenLiteral = storeIndex.LookupIndexByLiteral(lit);

        Assert.IsNotNull(quadruplesWithGivenLiteral);
        Assert.HasCount(1, quadruplesWithGivenLiteral);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundLiteral()
    {
        RDFContext ctx = new RDFContext("http://ctx/");
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFStoreIndex storeIndex = new RDFStoreIndex().Add(quadruple);
        HashSet<long> quadruplesWithGivenLiteral = storeIndex.LookupIndexByLiteral(new RDFPlainLiteral("lit2", "en-US"));

        Assert.IsNotNull(quadruplesWithGivenLiteral);
        Assert.IsEmpty(quadruplesWithGivenLiteral);
    }
    #endregion
}