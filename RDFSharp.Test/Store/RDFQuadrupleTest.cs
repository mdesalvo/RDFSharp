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
using RDFSharp.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RDFSharp.Test.Store
{
    [TestClass]
    public class RDFQuadrupleTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
        public void ShouldCreateSPOQuadruple(string s, string p, string o)
        {
            RDFContext ctx = new RDFContext();
            RDFResource subj = new RDFResource(s);
            RDFResource pred = new RDFResource(p);
            RDFResource obj = new RDFResource(o);

            RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
            Assert.IsNotNull(quadruple);
            Assert.IsTrue(quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO);
            Assert.IsTrue(quadruple.Context.Equals(ctx));
            Assert.IsTrue(quadruple.Subject.Equals(subj));
            Assert.IsTrue(quadruple.Predicate.Equals(pred));
            Assert.IsTrue(quadruple.Object.Equals(obj));
            Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", quadruple.QuadrupleID.ToString()))));

            string quadrupleString = quadruple.ToString();
            Assert.IsTrue(quadrupleString.Equals(string.Concat(quadruple.Context.ToString(), " ", quadruple.Subject.ToString(), " ", quadruple.Predicate.ToString(), " ", quadruple.Object.ToString())));

            long quadrupleID = RDFModelUtilities.CreateHash(quadrupleString);
            Assert.IsTrue(quadruple.QuadrupleID.Equals(quadrupleID));

            RDFQuadruple quadruple2 = new RDFQuadruple(ctx, subj, pred, obj);
            Assert.IsTrue(quadruple.Equals(quadruple2));
        }

        [DataTestMethod]
        [DataRow("http://example.org/pred")]
        public void ShouldCreateSPOQuadrupleFromNullInputs(string p)
        {
            RDFResource pred = new RDFResource(p);
            RDFContext ctx = new RDFContext();

            RDFQuadruple quadruple = new RDFQuadruple(null, null, pred, null as RDFResource);
            Assert.IsNotNull(quadruple);
            Assert.IsTrue(quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO);
            Assert.IsTrue(quadruple.Context.Equals(ctx));
            Assert.IsTrue(((RDFResource)quadruple.Subject).IsBlank);
            Assert.IsTrue(quadruple.Predicate.Equals(pred));
            Assert.IsTrue(((RDFResource)quadruple.Object).IsBlank);
            Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", quadruple.QuadrupleID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
        public void ShouldCreateSPLQuadruple(string s, string p, string l)
        {
            RDFContext ctx = new RDFContext();
            RDFResource subj = new RDFResource(s);
            RDFResource pred = new RDFResource(p);
            RDFPlainLiteral lit = new RDFPlainLiteral(l);

            RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
            Assert.IsNotNull(quadruple);
            Assert.IsTrue(quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL);
            Assert.IsTrue(quadruple.Context.Equals(ctx));
            Assert.IsTrue(quadruple.Subject.Equals(subj));
            Assert.IsTrue(quadruple.Predicate.Equals(pred));
            Assert.IsTrue(quadruple.Object.Equals(lit));
            Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", quadruple.QuadrupleID.ToString()))));

            string quadrupleString = quadruple.ToString();
            Assert.IsTrue(quadrupleString.Equals(string.Concat(quadruple.Context.ToString(), " ", quadruple.Subject.ToString(), " ", quadruple.Predicate.ToString(), " ", quadruple.Object.ToString())));

            long quadrupleID = RDFModelUtilities.CreateHash(quadrupleString);
            Assert.IsTrue(quadruple.QuadrupleID.Equals(quadrupleID));

            RDFQuadruple quadruple2 = new RDFQuadruple(ctx, subj, pred, lit);
            Assert.IsTrue(quadruple.Equals(quadruple2));
        }

        [DataTestMethod]
        [DataRow("http://example.org/pred")]
        public void ShouldCreateSPLQuadrupleFromNullInputs(string p)
        {
            RDFResource pred = new RDFResource(p);
            RDFContext ctx = new RDFContext();

            RDFQuadruple quadruple = new RDFQuadruple(null, null, pred, null as RDFPlainLiteral);
            Assert.IsNotNull(quadruple);
            Assert.IsTrue(quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL);
            Assert.IsTrue(quadruple.Context.Equals(ctx));
            Assert.IsTrue(((RDFResource)quadruple.Subject).IsBlank);
            Assert.IsTrue(quadruple.Predicate.Equals(pred));
            Assert.IsTrue(((RDFPlainLiteral)quadruple.Object).Equals(new RDFPlainLiteral(string.Empty)));
            Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", quadruple.QuadrupleID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("http://example.org/subj", "bnode:hdh744", "http://example.org/obj")]
        public void ShouldNotCreateSPOQuadrupleBecauseOfBlankPredicate(string s, string p, string o)
            => Assert.ThrowsException<RDFStoreException>(() => new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource(s), new RDFResource(p), new RDFResource(o)));

        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/obj")]
        public void ShouldNotCreateSPOQuadrupleBecauseOfNullPredicate(string s, string o)
            => Assert.ThrowsException<RDFStoreException>(() => new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource(s), null, new RDFResource(o)));

        [DataTestMethod]
        [DataRow("http://example.org/subj", "bnode:hdh744", "test")]
        public void ShouldNotCreateSPLQuadrupleBecauseOfBlankPredicate(string s, string p, string l)
            => Assert.ThrowsException<RDFStoreException>(() => new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l)));

        [DataTestMethod]
        [DataRow("http://example.org/subj", "test")]
        public void ShouldNotCreateSPLQuadrupleBecauseOfNullPredicate(string s, string l)
            => Assert.ThrowsException<RDFStoreException>(() => new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource(s), null, new RDFPlainLiteral(l)));

        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
        public void ShouldReifySPOQuadruple(string s, string p, string o)
        {
            RDFContext ctx = new RDFContext("ex:ctx");
            RDFResource subj = new RDFResource(s);
            RDFResource pred = new RDFResource(p);
            RDFResource obj = new RDFResource(o);

            RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
            RDFMemoryStore store = quadruple.ReifyQuadruple();
            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 4);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)quadruple.Object)));
        }

        [DataTestMethod]
        [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
        public void ShouldReifySPLQuadruple(string s, string p, string l)
        {
            RDFContext ctx = new RDFContext("ex:ctx");
            RDFResource subj = new RDFResource(s);
            RDFResource pred = new RDFResource(p);
            RDFPlainLiteral lit = new RDFPlainLiteral(l);

            RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
            RDFMemoryStore store = quadruple.ReifyQuadruple();
            Assert.IsNotNull(store);
            Assert.IsTrue(store.QuadruplesCount == 4);
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
            Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)quadruple.Object)));
        }
        #endregion
    }
}
