/*
   Copyright 2012-2021 Marco De Salvo

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
using System.Linq;

namespace RDFSharp.Test
{
    [TestClass]
    public class RDFGraphIndexTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateGraphIndex()
        {
            RDFGraphIndex graphIndex = new RDFGraphIndex();

            Assert.IsNotNull(graphIndex);
            Assert.IsNotNull(graphIndex.Subjects);
            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsNotNull(graphIndex.Predicates);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsNotNull(graphIndex.Objects);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsNotNull(graphIndex.Literals);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldAddSPOTriple()
        {
            RDFResource subj = new RDFResource("http://subj/");
            RDFResource pred = new RDFResource("http://pred/");
            RDFResource obj = new RDFResource("http://obj/");
            RDFTriple triple = new RDFTriple(subj, pred, obj);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);

            Assert.IsTrue(graphIndex.Subjects.Count == 1);
            Assert.IsTrue(graphIndex.Subjects[subj.PatternMemberID].Contains(triple.TripleID));
            Assert.IsTrue(graphIndex.Predicates.Count == 1);
            Assert.IsTrue(graphIndex.Predicates[pred.PatternMemberID].Contains(triple.TripleID));
            Assert.IsTrue(graphIndex.Objects.Count == 1);
            Assert.IsTrue(graphIndex.Objects[obj.PatternMemberID].Contains(triple.TripleID));
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldAddSPLTriple()
        {
            RDFResource subj = new RDFResource("http://subj/");
            RDFResource pred = new RDFResource("http://pred/");
            RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
            RDFTriple triple = new RDFTriple(subj, pred, lit);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple);

            Assert.IsTrue(graphIndex.Subjects.Count == 1);
            Assert.IsTrue(graphIndex.Subjects[subj.PatternMemberID].Contains(triple.TripleID));
            Assert.IsTrue(graphIndex.Predicates.Count == 1);
            Assert.IsTrue(graphIndex.Predicates[pred.PatternMemberID].Contains(triple.TripleID));
            Assert.IsTrue(graphIndex.Objects.Count == 0);            
            Assert.IsTrue(graphIndex.Literals.Count == 1);
            Assert.IsTrue(graphIndex.Literals[lit.PatternMemberID].Contains(triple.TripleID));
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

            Assert.IsTrue(graphIndex.Subjects.Count == 1);
            Assert.IsTrue(graphIndex.Subjects[subj.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Subjects[subj.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Predicates.Count == 2);
            Assert.IsTrue(graphIndex.Predicates[pred1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Predicates[pred2.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Objects.Count == 2);
            Assert.IsTrue(graphIndex.Objects[obj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Objects[obj2.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Literals.Count == 0);
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

            Assert.IsTrue(graphIndex.Subjects.Count == 2);
            Assert.IsTrue(graphIndex.Subjects[subj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Subjects[subj2.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Predicates.Count == 1);
            Assert.IsTrue(graphIndex.Predicates[pred.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Predicates[pred.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Objects.Count == 2);
            Assert.IsTrue(graphIndex.Objects[obj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Objects[obj2.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Literals.Count == 0);
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

            Assert.IsTrue(graphIndex.Subjects.Count == 2);
            Assert.IsTrue(graphIndex.Subjects[subj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Subjects[subj2.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Predicates.Count == 2);
            Assert.IsTrue(graphIndex.Predicates[pred1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Predicates[pred2.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Objects.Count == 1);
            Assert.IsTrue(graphIndex.Objects[obj.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Objects[obj.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Literals.Count == 0);
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

            Assert.IsTrue(graphIndex.Subjects.Count == 2);
            Assert.IsTrue(graphIndex.Subjects[subj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Subjects[subj2.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Predicates.Count == 2);
            Assert.IsTrue(graphIndex.Predicates[pred1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Predicates[pred2.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Objects.Count == 0);            
            Assert.IsTrue(graphIndex.Literals.Count == 1);
            Assert.IsTrue(graphIndex.Literals[lit.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsTrue(graphIndex.Literals[lit.PatternMemberID].Contains(triple2.TripleID));
        }

        [TestMethod]
        public void ShouldNotAddNullTriple()
        {
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(null);

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldRemoveSPOTriple()
        {
            RDFResource subj = new RDFResource("http://subj/");
            RDFResource pred = new RDFResource("http://pred/");
            RDFResource obj = new RDFResource("http://obj/");
            RDFTriple triple = new RDFTriple(subj, pred, obj);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple).RemoveIndex(triple);

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldRemoveSPLTriple()
        {
            RDFResource subj = new RDFResource("http://subj/");
            RDFResource pred = new RDFResource("http://pred/");
            RDFTypedLiteral lit = new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING);
            RDFTriple triple = new RDFTriple(subj, pred, lit);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple).RemoveIndex(triple);

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldAddSameSubjectMultipleTimesAndRemoveOneOccurrence()
        {
            RDFResource subj = new RDFResource("http://subj/");
            RDFResource pred1 = new RDFResource("http://pred1/");
            RDFResource obj1 = new RDFResource("http://obj1/");
            RDFTriple triple1 = new RDFTriple(subj, pred1, obj1);
            RDFResource pred2 = new RDFResource("http://pred2/");
            RDFResource obj2 = new RDFResource("http://obj2/");
            RDFTriple triple2 = new RDFTriple(subj, pred2, obj2);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2);

            Assert.IsTrue(graphIndex.Subjects.Count == 1);
            Assert.IsTrue(graphIndex.Subjects[subj.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Subjects[subj.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Predicates.Count == 1);
            Assert.IsTrue(graphIndex.Predicates[pred1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Predicates.ContainsKey(pred2.PatternMemberID));
            Assert.IsTrue(graphIndex.Objects.Count == 1);
            Assert.IsTrue(graphIndex.Objects[obj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Objects.ContainsKey(obj2.PatternMemberID));
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldAddSameSubjectMultipleTimesAndRemoveEveryOccurrences()
        {
            RDFResource subj = new RDFResource("http://subj/");
            RDFResource pred1 = new RDFResource("http://pred1/");
            RDFResource obj1 = new RDFResource("http://obj1/");
            RDFTriple triple1 = new RDFTriple(subj, pred1, obj1);
            RDFResource pred2 = new RDFResource("http://pred2/");
            RDFResource obj2 = new RDFResource("http://obj2/");
            RDFTriple triple2 = new RDFTriple(subj, pred2, obj2);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple1);

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldAddSamePredicateMultipleTimesAndRemoveOneOccurrence()
        {
            RDFResource subj1 = new RDFResource("http://subj1/");
            RDFResource pred = new RDFResource("http://pred/");
            RDFResource obj1 = new RDFResource("http://obj1/");
            RDFTriple triple1 = new RDFTriple(subj1, pred, obj1);
            RDFResource subj2 = new RDFResource("http://subj2/");
            RDFResource obj2 = new RDFResource("http://obj2/");
            RDFTriple triple2 = new RDFTriple(subj2, pred, obj2);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2);

            Assert.IsTrue(graphIndex.Subjects.Count == 1);
            Assert.IsTrue(graphIndex.Subjects[subj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Subjects.ContainsKey(subj2.PatternMemberID));
            Assert.IsTrue(graphIndex.Predicates.Count == 1);
            Assert.IsTrue(graphIndex.Predicates[pred.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Predicates[pred.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Objects.Count == 1);
            Assert.IsTrue(graphIndex.Objects[obj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Objects.ContainsKey(obj2.PatternMemberID));
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldAddSamePredicateMultipleTimesAndRemoveEveryOccurrence()
        {
            RDFResource subj1 = new RDFResource("http://subj1/");
            RDFResource pred = new RDFResource("http://pred/");
            RDFResource obj1 = new RDFResource("http://obj1/");
            RDFTriple triple1 = new RDFTriple(subj1, pred, obj1);
            RDFResource subj2 = new RDFResource("http://subj2/");
            RDFResource obj2 = new RDFResource("http://obj2/");
            RDFTriple triple2 = new RDFTriple(subj2, pred, obj2);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple1);

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldAddSameObjectMultipleTimesAndRemoveOneOccurrence()
        {
            RDFResource subj1 = new RDFResource("http://subj1/");
            RDFResource pred1 = new RDFResource("http://pred1/");
            RDFResource obj = new RDFResource("http://obj/");
            RDFTriple triple1 = new RDFTriple(subj1, pred1, obj);
            RDFResource subj2 = new RDFResource("http://subj2/");
            RDFResource pred2 = new RDFResource("http://pred2/");
            RDFTriple triple2 = new RDFTriple(subj2, pred2, obj);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2);

            Assert.IsTrue(graphIndex.Subjects.Count == 1);
            Assert.IsTrue(graphIndex.Subjects[subj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Subjects.ContainsKey(subj2.PatternMemberID));
            Assert.IsTrue(graphIndex.Predicates.Count == 1);
            Assert.IsTrue(graphIndex.Predicates[pred1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Predicates.ContainsKey(pred2.PatternMemberID));
            Assert.IsTrue(graphIndex.Objects.Count == 1);
            Assert.IsTrue(graphIndex.Objects[obj.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Objects[obj.PatternMemberID].Contains(triple2.TripleID));
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldAddSameObjectMultipleTimesAndRemoveEveryOccurrence()
        {
            RDFResource subj1 = new RDFResource("http://subj1/");
            RDFResource pred1 = new RDFResource("http://pred1/");
            RDFResource obj = new RDFResource("http://obj/");
            RDFTriple triple1 = new RDFTriple(subj1, pred1, obj);
            RDFResource subj2 = new RDFResource("http://subj2/");
            RDFResource pred2 = new RDFResource("http://pred2/");
            RDFTriple triple2 = new RDFTriple(subj2, pred2, obj);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple2).RemoveIndex(triple1);

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
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

            Assert.IsTrue(graphIndex.Subjects.Count == 1);
            Assert.IsTrue(graphIndex.Subjects[subj1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Subjects.ContainsKey(subj2.PatternMemberID));
            Assert.IsTrue(graphIndex.Predicates.Count == 1);
            Assert.IsTrue(graphIndex.Predicates[pred1.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Predicates.ContainsKey(pred2.PatternMemberID));
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 1);
            Assert.IsTrue(graphIndex.Literals[lit.PatternMemberID].Contains(triple1.TripleID));
            Assert.IsFalse(graphIndex.Literals[lit.PatternMemberID].Contains(triple2.TripleID));
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

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldNotRemoveNullTriple()
        {
            RDFGraphIndex graphIndex = new RDFGraphIndex().RemoveIndex(null);

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }

        [TestMethod]
        public void ShouldClearTriples()
        {
            RDFResource subj = new RDFResource("http://subj/");
            RDFResource pred = new RDFResource("http://pred/");
            RDFResource obj = new RDFResource("http://obj/");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFTriple triple1 = new RDFTriple(subj, pred, obj);
            RDFTriple triple2 = new RDFTriple(subj, pred, lit);
            RDFGraphIndex graphIndex = new RDFGraphIndex().AddIndex(triple1).AddIndex(triple2);
            graphIndex.ClearIndex();

            Assert.IsTrue(graphIndex.Subjects.Count == 0);
            Assert.IsTrue(graphIndex.Predicates.Count == 0);
            Assert.IsTrue(graphIndex.Objects.Count == 0);
            Assert.IsTrue(graphIndex.Literals.Count == 0);
        }
        #endregion
    }
}