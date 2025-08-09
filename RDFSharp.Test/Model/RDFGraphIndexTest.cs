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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFGraphIndexTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateGraphIndex()
    {
        RDFGraphIndex graphIndex = new RDFGraphIndex();

        Assert.IsNotNull(graphIndex);
        Assert.IsNotNull(graphIndex.Resources);
        Assert.IsEmpty(graphIndex.Resources);
        Assert.IsNotNull(graphIndex.Literals);
        Assert.IsEmpty(graphIndex.Literals);
        Assert.IsNotNull(graphIndex.IDXSubjects);
        Assert.IsEmpty(graphIndex.IDXSubjects);
        Assert.IsNotNull(graphIndex.IDXPredicates);
        Assert.IsEmpty(graphIndex.IDXPredicates);
        Assert.IsNotNull(graphIndex.IDXObjects);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.IsNotNull(graphIndex.IDXLiterals);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSPOIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);

        Assert.HasCount(3, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj.PatternMemberID));
        Assert.IsEmpty(graphIndex.Literals);
        Assert.HasCount(1, graphIndex.IDXSubjects);
        Assert.Contains(triple.TripleID, graphIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.HasCount(1, graphIndex.IDXPredicates);
        Assert.Contains(triple.TripleID, graphIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.HasCount(1, graphIndex.IDXObjects);
        Assert.Contains(triple.TripleID, graphIndex.IDXObjects[obj.PatternMemberID]);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSPLIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);

        Assert.HasCount(2, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred.PatternMemberID));
        Assert.HasCount(1, graphIndex.Literals);
        Assert.IsTrue(graphIndex.Literals.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(1, graphIndex.IDXSubjects);
        Assert.Contains(triple.TripleID, graphIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.HasCount(1, graphIndex.IDXPredicates);
        Assert.Contains(triple.TripleID, graphIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.HasCount(1, graphIndex.IDXLiterals);
        Assert.Contains(triple.TripleID, graphIndex.IDXLiterals[lit.PatternMemberID]);
    }

    [TestMethod]
    public void ShouldDisposeGraphIndexWithUsing()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFTriple triple1 = new RDFTriple(subj, pred, obj);
        RDFTriple triple2 = new RDFTriple(subj, pred, lit);
        RDFGraphIndex graphIndex;

        using (graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2))
        {
            Assert.IsFalse(graphIndex.Disposed);
            Assert.IsNotNull(graphIndex.Resources);
            Assert.IsNotNull(graphIndex.Literals);
            Assert.IsNotNull(graphIndex.IDXSubjects);
            Assert.IsNotNull(graphIndex.IDXPredicates);
            Assert.IsNotNull(graphIndex.IDXObjects);
            Assert.IsNotNull(graphIndex.IDXLiterals);
        }
        Assert.IsTrue(graphIndex.Disposed);
        Assert.IsNull(graphIndex.Resources);
    }

    [TestMethod]
    public void ShouldAddSameSubjectMultipleTimes()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFTriple triple1 = new RDFTriple(subj, pred1, obj1);
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFTriple triple2 = new RDFTriple(subj, pred2, obj2);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple1).Add(triple2);

        Assert.HasCount(5, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred2.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(graphIndex.Literals);
        Assert.HasCount(1, graphIndex.IDXSubjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXSubjects[subj.PatternMemberID]);
        Assert.HasCount(2, graphIndex.IDXPredicates);
        Assert.Contains(triple1.TripleID, graphIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXPredicates[pred2.PatternMemberID]);
        Assert.HasCount(2, graphIndex.IDXObjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXObjects[obj2.PatternMemberID]);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSamePredicateMultipleTimes()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFTriple triple1 = new RDFTriple(subj1, pred, obj1);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred, obj2);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple1).Add(triple2);

        Assert.HasCount(5, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(graphIndex.Literals);
        Assert.HasCount(2, graphIndex.IDXSubjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXSubjects[subj2.PatternMemberID]);
        Assert.HasCount(1, graphIndex.IDXPredicates);
        Assert.Contains(triple1.TripleID, graphIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXPredicates[pred.PatternMemberID]);
        Assert.HasCount(2, graphIndex.IDXObjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXObjects[obj2.PatternMemberID]);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSameObjectMultipleTimes()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, obj);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred2, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple1).Add(triple2);

        Assert.HasCount(5, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred2.PatternMemberID));
        Assert.IsEmpty(graphIndex.Literals);
        Assert.HasCount(2, graphIndex.IDXSubjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXSubjects[subj2.PatternMemberID]);
        Assert.HasCount(2, graphIndex.IDXPredicates);
        Assert.Contains(triple1.TripleID, graphIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXPredicates[pred2.PatternMemberID]);
        Assert.HasCount(1, graphIndex.IDXObjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXObjects[obj.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXObjects[obj.PatternMemberID]);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSameLiteralMultipleTimes()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, lit);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred2, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple1).Add(triple2);

        Assert.HasCount(4, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, graphIndex.Literals);
        Assert.IsTrue(graphIndex.Literals.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(2, graphIndex.IDXSubjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXSubjects[subj2.PatternMemberID]);
        Assert.HasCount(2, graphIndex.IDXPredicates);
        Assert.Contains(triple1.TripleID, graphIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXPredicates[pred2.PatternMemberID]);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.HasCount(1, graphIndex.IDXLiterals);
        Assert.Contains(triple1.TripleID, graphIndex.IDXLiterals[lit.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.IDXLiterals[lit.PatternMemberID]);
    }

    [TestMethod]
    public void ShouldRemoveSPOIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple).Remove(triple);

        Assert.IsEmpty(graphIndex.Resources);
        Assert.IsEmpty(graphIndex.Literals);
        Assert.IsEmpty(graphIndex.IDXSubjects);
        Assert.IsEmpty(graphIndex.IDXPredicates);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldRemoveSPLIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple).Remove(triple);

        Assert.IsEmpty(graphIndex.Resources);
        Assert.IsEmpty(graphIndex.Literals);
        Assert.IsEmpty(graphIndex.IDXSubjects);
        Assert.IsEmpty(graphIndex.IDXPredicates);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSameSubjectMultipleTimesAndRemoveOneOccurrence()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, obj1);
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFTriple triple2 = new RDFTriple(subj1, pred2, obj2);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2).Remove(triple2);

        Assert.HasCount(3, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(graphIndex.Literals);
        Assert.HasCount(1, graphIndex.IDXSubjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.DoesNotContain(triple2.TripleID, graphIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.HasCount(1, graphIndex.IDXPredicates);
        Assert.Contains(triple1.TripleID, graphIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.IsFalse(graphIndex.IDXPredicates.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, graphIndex.IDXObjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.IDXObjects.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSameSubjectMultipleTimesAndRemoveEveryOccurrences()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, obj1);
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFTriple triple2 = new RDFTriple(subj1, pred2, obj2);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2).Remove(triple2).Remove(triple2).Remove(triple1);

        Assert.IsEmpty(graphIndex.Resources);
        Assert.IsEmpty(graphIndex.Literals);
        Assert.IsEmpty(graphIndex.IDXSubjects);
        Assert.IsEmpty(graphIndex.IDXPredicates);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSamePredicateMultipleTimesAndRemoveOneOccurrence()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, obj1);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred1, obj2);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2).Remove(triple2);

        Assert.HasCount(3, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(graphIndex.Literals);
        Assert.HasCount(1, graphIndex.IDXSubjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.IDXSubjects.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, graphIndex.IDXPredicates);
        Assert.Contains(triple1.TripleID, graphIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.DoesNotContain(triple2.TripleID, graphIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.HasCount(1, graphIndex.IDXObjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.IDXObjects.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSamePredicateMultipleTimesAndRemoveEveryOccurrence()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFTriple triple1 = new RDFTriple(subj1, pred, obj1);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource obj2 = new RDFResource("http://obj2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred, obj2);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2).Remove(triple2).Remove(triple2).Remove(triple1);

        Assert.IsEmpty(graphIndex.Resources);
        Assert.IsEmpty(graphIndex.Literals);
        Assert.IsEmpty(graphIndex.IDXSubjects);
        Assert.IsEmpty(graphIndex.IDXPredicates);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSameObjectMultipleTimesAndRemoveOneOccurrence()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, obj1);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred2, obj1);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2).Remove(triple2);

        Assert.HasCount(3, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(graphIndex.Literals);
        Assert.HasCount(1, graphIndex.IDXSubjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.IDXSubjects.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, graphIndex.IDXPredicates);
        Assert.Contains(triple1.TripleID, graphIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.IsFalse(graphIndex.IDXPredicates.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, graphIndex.IDXObjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.DoesNotContain(triple2.TripleID, graphIndex.IDXObjects[obj1.PatternMemberID]);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSameObjectMultipleTimesAndRemoveEveryOccurrence()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFResource obj1 = new RDFResource("http://obj1/");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, obj1);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred2, obj1);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2).Remove(triple2).Remove(triple2).Remove(triple1);

        Assert.IsEmpty(graphIndex.Resources);
        Assert.IsEmpty(graphIndex.Literals);
        Assert.IsEmpty(graphIndex.IDXSubjects);
        Assert.IsEmpty(graphIndex.IDXPredicates);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldAddSameLiteralMultipleTimesAndRemoveOneOccurrence()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, lit);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred2, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2).Remove(triple2);

        Assert.HasCount(2, graphIndex.Resources);
        Assert.IsTrue(graphIndex.Resources.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.Resources.ContainsKey(pred1.PatternMemberID));
        Assert.HasCount(1, graphIndex.Literals);
        Assert.IsTrue(graphIndex.Literals.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(1, graphIndex.IDXSubjects);
        Assert.Contains(triple1.TripleID, graphIndex.IDXSubjects[subj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.IDXSubjects.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, graphIndex.IDXPredicates);
        Assert.Contains(triple1.TripleID, graphIndex.IDXPredicates[pred1.PatternMemberID]);
        Assert.IsFalse(graphIndex.IDXPredicates.ContainsKey(pred2.PatternMemberID));
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.HasCount(1, graphIndex.IDXLiterals);
        Assert.Contains(triple1.TripleID, graphIndex.IDXLiterals[lit.PatternMemberID]);
        Assert.DoesNotContain(triple2.TripleID, graphIndex.IDXLiterals[lit.PatternMemberID]);
    }

    [TestMethod]
    public void ShouldAddSameLiteralMultipleTimesAndRemoveEveryOccurrence()
    {
        RDFResource subj1 = new RDFResource("http://subj1/");
        RDFResource pred1 = new RDFResource("http://pred1/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFTriple triple1 = new RDFTriple(subj1, pred1, lit);
        RDFResource subj2 = new RDFResource("http://subj2/");
        RDFResource pred2 = new RDFResource("http://pred2/");
        RDFTriple triple2 = new RDFTriple(subj2, pred2, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2).Remove(triple2).Remove(triple2).Remove(triple1);

        Assert.IsEmpty(graphIndex.Resources);
        Assert.IsEmpty(graphIndex.Literals);
        Assert.IsEmpty(graphIndex.IDXSubjects);
        Assert.IsEmpty(graphIndex.IDXPredicates);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldClearIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit");
        RDFTriple triple1 = new RDFTriple(subj, pred, obj);
        RDFTriple triple2 = new RDFTriple(subj, pred, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple1).Add(triple2);
        graphIndex.Clear();

        Assert.IsEmpty(graphIndex.Resources);
        Assert.IsEmpty(graphIndex.Literals);
        Assert.IsEmpty(graphIndex.IDXSubjects);
        Assert.IsEmpty(graphIndex.IDXPredicates);
        Assert.IsEmpty(graphIndex.IDXObjects);
        Assert.IsEmpty(graphIndex.IDXLiterals);
    }

    [TestMethod]
    public void ShouldSelectIndexBySubject()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);
        HashSet<long> triplesWithGivenSubject = graphIndex.LookupIndexBySubject(subj);

        Assert.IsNotNull(triplesWithGivenSubject);
        Assert.HasCount(1, triplesWithGivenSubject);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundSubject()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);
        HashSet<long> triplesWithGivenSubject = graphIndex.LookupIndexBySubject(new RDFResource("http://subj2/"));

        Assert.IsNotNull(triplesWithGivenSubject);
        Assert.IsEmpty(triplesWithGivenSubject);
    }

    [TestMethod]
    public void ShouldSelectIndexByPredicate()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);
        HashSet<long> triplesWithGivenPredicate = graphIndex.LookupIndexByPredicate(pred);

        Assert.IsNotNull(triplesWithGivenPredicate);
        Assert.HasCount(1, triplesWithGivenPredicate);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundPredicate()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);
        HashSet<long> triplesWithGivenPredicate = graphIndex.LookupIndexByPredicate(new RDFResource("http://pred2/"));

        Assert.IsNotNull(triplesWithGivenPredicate);
        Assert.IsEmpty(triplesWithGivenPredicate);
    }

    [TestMethod]
    public void ShouldSelectIndexByObject()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);
        HashSet<long> triplesWithGivenObject = graphIndex.LookupIndexByObject(obj);

        Assert.IsNotNull(triplesWithGivenObject);
        Assert.HasCount(1, triplesWithGivenObject);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundObject()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);
        HashSet<long> triplesWithGivenObject = graphIndex.LookupIndexByObject(new RDFResource("http://subj2/"));

        Assert.IsNotNull(triplesWithGivenObject);
        Assert.IsEmpty(triplesWithGivenObject);
    }

    [TestMethod]
    public void ShouldSelectIndexByLiteral()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);
        HashSet<long> triplesWithGivenLiteral = graphIndex.LookupIndexByLiteral(lit);

        Assert.IsNotNull(triplesWithGivenLiteral);
        Assert.HasCount(1, triplesWithGivenLiteral);
    }

    [TestMethod]
    public void ShouldSelectEmptyIndexByNotFoundLiteral()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFPlainLiteral lit = new RDFPlainLiteral("lit", "en-US");
        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().Add(triple);
        HashSet<long> triplesWithGivenLiteral = graphIndex.LookupIndexByLiteral(new RDFPlainLiteral("lit2", "en-US"));

        Assert.IsNotNull(triplesWithGivenLiteral);
        Assert.IsEmpty(triplesWithGivenLiteral);
    }
    #endregion
}