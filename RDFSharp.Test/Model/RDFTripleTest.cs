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

using RDFSharp.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFTripleTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
        public void ShouldCreateSPOTriple(string s, string p, string o)
        {
            RDFResource subj = new RDFResource(s);
            RDFResource pred = new RDFResource(p);
            RDFResource obj = new RDFResource(o);

            RDFTriple triple = new RDFTriple(subj, pred, obj);
            Assert.IsNotNull(triple);
            Assert.IsTrue(triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO);
            Assert.IsTrue(triple.Subject.Equals(subj));
            Assert.IsTrue(triple.Predicate.Equals(pred));
            Assert.IsTrue(triple.Object.Equals(obj));
            Assert.IsTrue(triple.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", triple.TripleID.ToString()))));

            string tripleString = triple.ToString();
            Assert.IsTrue(tripleString.Equals(string.Concat(triple.Subject.ToString(), " ", triple.Predicate.ToString(), " ", triple.Object.ToString())));

            long tripleID = RDFModelUtilities.CreateHash(tripleString);
            Assert.IsTrue(triple.TripleID.Equals(tripleID));

            RDFTriple triple2 = new RDFTriple(subj, pred, obj);
            Assert.IsTrue(triple.Equals(triple2));
        }

        [DataTestMethod]
        [DataRow("http://example.org/pred")]
        public void ShouldCreateSPOTripleFromNullInputs(string p)
        {
            RDFResource pred = new RDFResource(p);

            RDFTriple triple = new RDFTriple(null, pred, null as RDFResource);
            Assert.IsNotNull(triple);
            Assert.IsTrue(triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO);
            Assert.IsTrue(((RDFResource)triple.Subject).IsBlank);
            Assert.IsTrue(triple.Predicate.Equals(pred));
            Assert.IsTrue(((RDFResource)triple.Object).IsBlank);
            Assert.IsTrue(triple.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", triple.TripleID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
        public void ShouldCreateSPLTriple(string s, string p, string l)
        {
            RDFResource subj = new RDFResource(s);
            RDFResource pred = new RDFResource(p);
            RDFPlainLiteral lit = new RDFPlainLiteral(l);

            RDFTriple triple = new RDFTriple(subj, pred, lit);
            Assert.IsNotNull(triple);
            Assert.IsTrue(triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL);
            Assert.IsTrue(triple.Subject.Equals(subj));
            Assert.IsTrue(triple.Predicate.Equals(pred));
            Assert.IsTrue(triple.Object.Equals(lit));
            Assert.IsTrue(triple.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", triple.TripleID.ToString()))));

            string tripleString = triple.ToString();
            Assert.IsTrue(tripleString.Equals(string.Concat(triple.Subject.ToString(), " ", triple.Predicate.ToString(), " ", triple.Object.ToString())));

            long tripleID = RDFModelUtilities.CreateHash(tripleString);
            Assert.IsTrue(triple.TripleID.Equals(tripleID));

            RDFTriple triple2 = new RDFTriple(subj, pred, lit);
            Assert.IsTrue(triple.Equals(triple2));
        }

        [DataTestMethod]
        [DataRow("http://example.org/pred")]
        public void ShouldCreateSPLTripleFromNullInputs(string p)
        {
            RDFResource pred = new RDFResource(p);
            
            RDFTriple triple = new RDFTriple(null, pred, null as RDFPlainLiteral);
            Assert.IsNotNull(triple);
            Assert.IsTrue(triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL);
            Assert.IsTrue(((RDFResource)triple.Subject).IsBlank);
            Assert.IsTrue(triple.Predicate.Equals(pred));
            Assert.IsTrue(((RDFPlainLiteral)triple.Object).Equals(new RDFPlainLiteral(string.Empty)));
            Assert.IsTrue(triple.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", triple.TripleID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("http://example.org/subj", "bnode:hdh744", "http://example.org/obj")]
        public void ShouldNotCreateSPOTripleBecauseOfBlankPredicate(string s, string p, string o)
            => Assert.ThrowsException<RDFModelException>(() => new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFResource(o)));

        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/obj")]
        public void ShouldNotCreateSPOTripleBecauseOfNullPredicate(string s, string o)
            => Assert.ThrowsException<RDFModelException>(() => new RDFTriple(new RDFResource(s), null, new RDFResource(o)));

        [DataTestMethod]
        [DataRow("http://example.org/subj", "bnode:hdh744", "test")]
        public void ShouldNotCreateSPLTripleBecauseOfBlankPredicate(string s, string p, string l)
            => Assert.ThrowsException<RDFModelException>(() => new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l)));

        [DataTestMethod]
        [DataRow("http://example.org/subj", "test")]
        public void ShouldNotCreateSPLTripleBecauseOfNullPredicate(string s, string l)
            => Assert.ThrowsException<RDFModelException>(() => new RDFTriple(new RDFResource(s), null, new RDFPlainLiteral(l)));

        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
        public void ShouldReifySPOTriple(string s, string p, string o)
        {
            RDFResource subj = new RDFResource(s);
            RDFResource pred = new RDFResource(p);
            RDFResource obj = new RDFResource(o);

            RDFTriple triple = new RDFTriple(subj, pred, obj);
            RDFGraph graph = triple.ReifyTriple();
            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 4);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)triple.Object)));
        }

        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
        public void ShouldReifySPLTriple(string s, string p, string l)
        {
            RDFResource subj = new RDFResource(s);
            RDFResource pred = new RDFResource(p);
            RDFPlainLiteral lit = new RDFPlainLiteral(l);

            RDFTriple triple = new RDFTriple(subj, pred, lit);
            RDFGraph graph = triple.ReifyTriple();
            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 4);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)triple.Object)));
        }
        #endregion
    }
}