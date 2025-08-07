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
        Assert.IsNotNull(graphIndex.ResourcesRegister);
        Assert.IsEmpty(graphIndex.ResourcesRegister);
        Assert.IsNotNull(graphIndex.LiteralsRegister);
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.IsNotNull(graphIndex.SubjectsIndex);
        Assert.IsEmpty(graphIndex.SubjectsIndex);
        Assert.IsNotNull(graphIndex.PredicatesIndex);
        Assert.IsEmpty(graphIndex.PredicatesIndex);
        Assert.IsNotNull(graphIndex.ObjectsIndex);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.IsNotNull(graphIndex.LiteralsIndex);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
    }

    [TestMethod]
    public void ShouldAddSPOIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);

        Assert.HasCount(3, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.HasCount(1, graphIndex.SubjectsIndex);
        Assert.Contains(triple.TripleID, graphIndex.SubjectsIndex[subj.PatternMemberID]);
        Assert.HasCount(1, graphIndex.PredicatesIndex);
        Assert.Contains(triple.TripleID, graphIndex.PredicatesIndex[pred.PatternMemberID]);
        Assert.HasCount(1, graphIndex.ObjectsIndex);
        Assert.Contains(triple.TripleID, graphIndex.ObjectsIndex[obj.PatternMemberID]);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
    }

    [TestMethod]
    public void ShouldAddSPLIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);

        Assert.HasCount(2, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred.PatternMemberID));
        Assert.HasCount(1, graphIndex.LiteralsRegister);
        Assert.IsTrue(graphIndex.LiteralsRegister.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(1, graphIndex.SubjectsIndex);
        Assert.Contains(triple.TripleID, graphIndex.SubjectsIndex[subj.PatternMemberID]);
        Assert.HasCount(1, graphIndex.PredicatesIndex);
        Assert.Contains(triple.TripleID, graphIndex.PredicatesIndex[pred.PatternMemberID]);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.HasCount(1, graphIndex.LiteralsIndex);
        Assert.Contains(triple.TripleID, graphIndex.LiteralsIndex[lit.PatternMemberID]);
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

        using (graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2))
        {
            Assert.IsFalse(graphIndex.Disposed);
            Assert.IsNotNull(graphIndex.ResourcesRegister);
            Assert.IsNotNull(graphIndex.LiteralsRegister);
            Assert.IsNotNull(graphIndex.SubjectsIndex);
            Assert.IsNotNull(graphIndex.PredicatesIndex);
            Assert.IsNotNull(graphIndex.ObjectsIndex);
            Assert.IsNotNull(graphIndex.LiteralsIndex);
        }
        Assert.IsTrue(graphIndex.Disposed);
        Assert.IsNull(graphIndex.ResourcesRegister);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple1).AddIndex(triple2);

        Assert.HasCount(5, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred2.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.HasCount(1, graphIndex.SubjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.SubjectsIndex[subj.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.SubjectsIndex[subj.PatternMemberID]);
        Assert.HasCount(2, graphIndex.PredicatesIndex);
        Assert.Contains(triple1.TripleID, graphIndex.PredicatesIndex[pred1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.PredicatesIndex[pred2.PatternMemberID]);
        Assert.HasCount(2, graphIndex.ObjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.ObjectsIndex[obj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.ObjectsIndex[obj2.PatternMemberID]);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple1).AddIndex(triple2);

        Assert.HasCount(5, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.HasCount(2, graphIndex.SubjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.SubjectsIndex[subj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.SubjectsIndex[subj2.PatternMemberID]);
        Assert.HasCount(1, graphIndex.PredicatesIndex);
        Assert.Contains(triple1.TripleID, graphIndex.PredicatesIndex[pred.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.PredicatesIndex[pred.PatternMemberID]);
        Assert.HasCount(2, graphIndex.ObjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.ObjectsIndex[obj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.ObjectsIndex[obj2.PatternMemberID]);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple1).AddIndex(triple2);

        Assert.HasCount(5, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred2.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.HasCount(2, graphIndex.SubjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.SubjectsIndex[subj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.SubjectsIndex[subj2.PatternMemberID]);
        Assert.HasCount(2, graphIndex.PredicatesIndex);
        Assert.Contains(triple1.TripleID, graphIndex.PredicatesIndex[pred1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.PredicatesIndex[pred2.PatternMemberID]);
        Assert.HasCount(1, graphIndex.ObjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.ObjectsIndex[obj.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.ObjectsIndex[obj.PatternMemberID]);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple1).AddIndex(triple2);

        Assert.HasCount(4, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj2.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, graphIndex.LiteralsRegister);
        Assert.IsTrue(graphIndex.LiteralsRegister.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(2, graphIndex.SubjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.SubjectsIndex[subj1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.SubjectsIndex[subj2.PatternMemberID]);
        Assert.HasCount(2, graphIndex.PredicatesIndex);
        Assert.Contains(triple1.TripleID, graphIndex.PredicatesIndex[pred1.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.PredicatesIndex[pred2.PatternMemberID]);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.HasCount(1, graphIndex.LiteralsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.LiteralsIndex[lit.PatternMemberID]);
        Assert.Contains(triple2.TripleID, graphIndex.LiteralsIndex[lit.PatternMemberID]);
    }

    [TestMethod]
    public void ShouldRemoveSPOIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple).RemoveIndex(triple);

        Assert.IsEmpty(graphIndex.ResourcesRegister);
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.IsEmpty(graphIndex.SubjectsIndex);
        Assert.IsEmpty(graphIndex.PredicatesIndex);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
    }

    [TestMethod]
    public void ShouldRemoveSPLIndex()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple).RemoveIndex(triple);

        Assert.IsEmpty(graphIndex.ResourcesRegister);
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.IsEmpty(graphIndex.SubjectsIndex);
        Assert.IsEmpty(graphIndex.PredicatesIndex);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2);

        Assert.HasCount(3, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.HasCount(1, graphIndex.SubjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.SubjectsIndex[subj1.PatternMemberID]);
        Assert.DoesNotContain(triple2.TripleID, graphIndex.SubjectsIndex[subj1.PatternMemberID]);
        Assert.HasCount(1, graphIndex.PredicatesIndex);
        Assert.Contains(triple1.TripleID, graphIndex.PredicatesIndex[pred1.PatternMemberID]);
        Assert.IsFalse(graphIndex.PredicatesIndex.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, graphIndex.ObjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.ObjectsIndex[obj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.ObjectsIndex.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple1);

        Assert.IsEmpty(graphIndex.ResourcesRegister);
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.IsEmpty(graphIndex.SubjectsIndex);
        Assert.IsEmpty(graphIndex.PredicatesIndex);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2);

        Assert.HasCount(3, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.HasCount(1, graphIndex.SubjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.SubjectsIndex[subj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.SubjectsIndex.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, graphIndex.PredicatesIndex);
        Assert.Contains(triple1.TripleID, graphIndex.PredicatesIndex[pred1.PatternMemberID]);
        Assert.DoesNotContain(triple2.TripleID, graphIndex.PredicatesIndex[pred1.PatternMemberID]);
        Assert.HasCount(1, graphIndex.ObjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.ObjectsIndex[obj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.ObjectsIndex.ContainsKey(obj2.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple1);

        Assert.IsEmpty(graphIndex.ResourcesRegister);
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.IsEmpty(graphIndex.SubjectsIndex);
        Assert.IsEmpty(graphIndex.PredicatesIndex);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2);

        Assert.HasCount(3, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(obj1.PatternMemberID));
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.HasCount(1, graphIndex.SubjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.SubjectsIndex[subj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.SubjectsIndex.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, graphIndex.PredicatesIndex);
        Assert.Contains(triple1.TripleID, graphIndex.PredicatesIndex[pred1.PatternMemberID]);
        Assert.IsFalse(graphIndex.PredicatesIndex.ContainsKey(pred2.PatternMemberID));
        Assert.HasCount(1, graphIndex.ObjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.ObjectsIndex[obj1.PatternMemberID]);
        Assert.DoesNotContain(triple2.TripleID, graphIndex.ObjectsIndex[obj1.PatternMemberID]);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple1);

        Assert.IsEmpty(graphIndex.ResourcesRegister);
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.IsEmpty(graphIndex.SubjectsIndex);
        Assert.IsEmpty(graphIndex.PredicatesIndex);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2);

        Assert.HasCount(2, graphIndex.ResourcesRegister);
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(subj1.PatternMemberID));
        Assert.IsTrue(graphIndex.ResourcesRegister.ContainsKey(pred1.PatternMemberID));
        Assert.HasCount(1, graphIndex.LiteralsRegister);
        Assert.IsTrue(graphIndex.LiteralsRegister.ContainsKey(lit.PatternMemberID));
        Assert.HasCount(1, graphIndex.SubjectsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.SubjectsIndex[subj1.PatternMemberID]);
        Assert.IsFalse(graphIndex.SubjectsIndex.ContainsKey(subj2.PatternMemberID));
        Assert.HasCount(1, graphIndex.PredicatesIndex);
        Assert.Contains(triple1.TripleID, graphIndex.PredicatesIndex[pred1.PatternMemberID]);
        Assert.IsFalse(graphIndex.PredicatesIndex.ContainsKey(pred2.PatternMemberID));
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.HasCount(1, graphIndex.LiteralsIndex);
        Assert.Contains(triple1.TripleID, graphIndex.LiteralsIndex[lit.PatternMemberID]);
        Assert.DoesNotContain(triple2.TripleID, graphIndex.LiteralsIndex[lit.PatternMemberID]);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple1);

        Assert.IsEmpty(graphIndex.ResourcesRegister);
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.IsEmpty(graphIndex.SubjectsIndex);
        Assert.IsEmpty(graphIndex.PredicatesIndex);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2);
        graphIndex.ClearIndex();

        Assert.IsEmpty(graphIndex.ResourcesRegister);
        Assert.IsEmpty(graphIndex.LiteralsRegister);
        Assert.IsEmpty(graphIndex.SubjectsIndex);
        Assert.IsEmpty(graphIndex.PredicatesIndex);
        Assert.IsEmpty(graphIndex.ObjectsIndex);
        Assert.IsEmpty(graphIndex.LiteralsIndex);
    }

    [TestMethod]
    public void ShouldSelectIndexBySubject()
    {
        RDFResource subj = new RDFResource("http://subj/");
        RDFResource pred = new RDFResource("http://pred/");
        RDFResource obj = new RDFResource("http://obj/");
        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);
        HashSet<long> triplesWithGivenSubject = graphIndex.SelectIndexBySubject(subj);

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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);
        HashSet<long> triplesWithGivenSubject = graphIndex.SelectIndexBySubject(new RDFResource("http://subj2/"));

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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);
        HashSet<long> triplesWithGivenPredicate = graphIndex.SelectIndexByPredicate(pred);

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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);
        HashSet<long> triplesWithGivenPredicate = graphIndex.SelectIndexByPredicate(new RDFResource("http://pred2/"));

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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);
        HashSet<long> triplesWithGivenObject = graphIndex.SelectIndexByObject(obj);

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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);
        HashSet<long> triplesWithGivenObject = graphIndex.SelectIndexByObject(new RDFResource("http://subj2/"));

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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);
        HashSet<long> triplesWithGivenLiteral = graphIndex.SelectIndexByLiteral(lit);

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
        RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);
        HashSet<long> triplesWithGivenLiteral = graphIndex.SelectIndexByLiteral(new RDFPlainLiteral("lit2", "en-US"));

        Assert.IsNotNull(triplesWithGivenLiteral);
        Assert.IsEmpty(triplesWithGivenLiteral);
    }
    #endregion
}