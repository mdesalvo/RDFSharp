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
using System.Threading.Tasks;

namespace RDFSharp.Test.Store;

[TestClass]
public class RDFQuadrupleTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateQuadrupleFromSPOTriple()
    {
        RDFQuadruple quadruple = new RDFQuadruple(new RDFContext(), new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
        Assert.IsNotNull(quadruple);
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPO, quadruple.TripleFlavor);
        Assert.IsTrue(quadruple.Context.Equals(new RDFContext()));
        Assert.IsTrue(quadruple.Subject.Equals(new RDFResource("http://subj/")));
        Assert.IsTrue(quadruple.Predicate.Equals(new RDFResource("http://pred/")));
        Assert.IsTrue(quadruple.Object.Equals(new RDFResource("http://obj/")));
        Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource($"bnode:{quadruple.QuadrupleID}")));

        string quadrupleString = quadruple.ToString();
        Assert.IsTrue(quadrupleString.Equals($"{quadruple.Context} {quadruple.Subject} {quadruple.Predicate} {quadruple.Object}"));

        long quadrupleID = RDFModelUtilities.CreateHash(quadrupleString);
        Assert.IsTrue(quadruple.QuadrupleID.Equals(quadrupleID));

        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        Assert.IsTrue(quadruple.Equals(quadruple2));
    }

    [TestMethod]
    public void ShouldCreateQuadrupleFromSPOTripleDefaultingContext()
    {
        RDFQuadruple quadruple = new RDFQuadruple(null, new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
        Assert.IsNotNull(quadruple);
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPO, quadruple.TripleFlavor);
        Assert.IsTrue(quadruple.Context.Equals(new RDFContext()));
        Assert.IsTrue(quadruple.Subject.Equals(new RDFResource("http://subj/")));
        Assert.IsTrue(quadruple.Predicate.Equals(new RDFResource("http://pred/")));
        Assert.IsTrue(quadruple.Object.Equals(new RDFResource("http://obj/")));
        Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource($"bnode:{quadruple.QuadrupleID}")));

        string quadrupleString = quadruple.ToString();
        Assert.IsTrue(quadrupleString.Equals($"{quadruple.Context} {quadruple.Subject} {quadruple.Predicate} {quadruple.Object}"));

        long quadrupleID = RDFModelUtilities.CreateHash(quadrupleString);
        Assert.IsTrue(quadruple.QuadrupleID.Equals(quadrupleID));

        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        Assert.IsTrue(quadruple.Equals(quadruple2));
    }

    [TestMethod]
    public void ShouldCreateQuadrupleFromSPLTriple()
    {
        RDFQuadruple quadruple = new RDFQuadruple(new RDFContext(), new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit")));
        Assert.IsNotNull(quadruple);
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPL, quadruple.TripleFlavor);
        Assert.IsTrue(quadruple.Context.Equals(new RDFContext()));
        Assert.IsTrue(quadruple.Subject.Equals(new RDFResource("http://subj/")));
        Assert.IsTrue(quadruple.Predicate.Equals(new RDFResource("http://pred/")));
        Assert.IsTrue(quadruple.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource($"bnode:{quadruple.QuadrupleID}")));

        string quadrupleString = quadruple.ToString();
        Assert.IsTrue(quadrupleString.Equals($"{quadruple.Context} {quadruple.Subject} {quadruple.Predicate} {quadruple.Object}"));

        long quadrupleID = RDFModelUtilities.CreateHash(quadrupleString);
        Assert.IsTrue(quadruple.QuadrupleID.Equals(quadrupleID));

        RDFQuadruple quadruple2 = new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        Assert.IsTrue(quadruple.Equals(quadruple2));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingQuadrupleFromTripleBecauseOfNullTriple()
        => Assert.ThrowsExactly<RDFStoreException>(() => _ = new RDFQuadruple(new RDFContext("ex:ctx"), null));

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
    public void ShouldCreateSPOQuadruple(string s, string p, string o)
    {
        RDFContext ctx = new RDFContext();
        RDFResource subj = new RDFResource(s);
        RDFResource pred = new RDFResource(p);
        RDFResource obj = new RDFResource(o);

        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        Assert.IsNotNull(quadruple);
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPO, quadruple.TripleFlavor);
        Assert.IsTrue(quadruple.Context.Equals(ctx));
        Assert.IsTrue(quadruple.Subject.Equals(subj));
        Assert.IsTrue(quadruple.Predicate.Equals(pred));
        Assert.IsTrue(quadruple.Object.Equals(obj));
        Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource($"bnode:{quadruple.QuadrupleID}")));

        string quadrupleString = quadruple.ToString();
        Assert.IsTrue(quadrupleString.Equals($"{quadruple.Context} {quadruple.Subject} {quadruple.Predicate} {quadruple.Object}"));

        long quadrupleID = RDFModelUtilities.CreateHash(quadrupleString);
        Assert.IsTrue(quadruple.QuadrupleID.Equals(quadrupleID));

        RDFQuadruple quadruple2 = new RDFQuadruple(ctx, subj, pred, obj);
        Assert.IsTrue(quadruple.Equals(quadruple2));
    }

    [TestMethod]
    [DataRow("http://example.org/pred")]
    public void ShouldCreateSPOQuadrupleFromNullInputs(string p)
    {
        RDFResource pred = new RDFResource(p);
        RDFContext ctx = new RDFContext();

        RDFQuadruple quadruple = new RDFQuadruple(null, null, pred, null as RDFResource);
        Assert.IsNotNull(quadruple);
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPO, quadruple.TripleFlavor);
        Assert.IsTrue(quadruple.Context.Equals(ctx));
        Assert.IsTrue(((RDFResource)quadruple.Subject).IsBlank);
        Assert.IsTrue(quadruple.Predicate.Equals(pred));
        Assert.IsTrue(((RDFResource)quadruple.Object).IsBlank);
        Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource($"bnode:{quadruple.QuadrupleID}")));
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
    public void ShouldCreateSPLQuadruple(string s, string p, string l)
    {
        RDFContext ctx = new RDFContext();
        RDFResource subj = new RDFResource(s);
        RDFResource pred = new RDFResource(p);
        RDFPlainLiteral lit = new RDFPlainLiteral(l);

        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        Assert.IsNotNull(quadruple);
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPL, quadruple.TripleFlavor);
        Assert.IsTrue(quadruple.Context.Equals(ctx));
        Assert.IsTrue(quadruple.Subject.Equals(subj));
        Assert.IsTrue(quadruple.Predicate.Equals(pred));
        Assert.IsTrue(quadruple.Object.Equals(lit));
        Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource($"bnode:{quadruple.QuadrupleID}")));

        string quadrupleString = quadruple.ToString();
        Assert.IsTrue(quadrupleString.Equals($"{quadruple.Context} {quadruple.Subject} {quadruple.Predicate} {quadruple.Object}"));

        long quadrupleID = RDFModelUtilities.CreateHash(quadrupleString);
        Assert.IsTrue(quadruple.QuadrupleID.Equals(quadrupleID));

        RDFQuadruple quadruple2 = new RDFQuadruple(ctx, subj, pred, lit);
        Assert.IsTrue(quadruple.Equals(quadruple2));
    }

    [TestMethod]
    [DataRow("http://example.org/pred")]
    public void ShouldCreateSPLQuadrupleFromNullInputs(string p)
    {
        RDFResource pred = new RDFResource(p);
        RDFContext ctx = new RDFContext();

        RDFQuadruple quadruple = new RDFQuadruple(null, null, pred, null as RDFPlainLiteral);
        Assert.IsNotNull(quadruple);
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPL, quadruple.TripleFlavor);
        Assert.IsTrue(quadruple.Context.Equals(ctx));
        Assert.IsTrue(((RDFResource)quadruple.Subject).IsBlank);
        Assert.IsTrue(quadruple.Predicate.Equals(pred));
        Assert.IsTrue(((RDFPlainLiteral)quadruple.Object).Equals(RDFPlainLiteral.Empty));
        Assert.IsTrue(quadruple.ReificationSubject.Equals(new RDFResource($"bnode:{quadruple.QuadrupleID}")));
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "bnode:hdh744", "http://example.org/obj")]
    public void ShouldThrowExceptionOnCreatingSPOQuadrupleBecauseOfBlankPredicate(string s, string p, string o)
        => Assert.ThrowsExactly<RDFStoreException>(() => _ = new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource(s), new RDFResource(p), new RDFResource(o)));

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/obj")]
    public void ShouldThrowExceptionOnCreatingSPOQuadrupleBecauseOfNullPredicate(string s, string o)
        => Assert.ThrowsExactly<RDFStoreException>(() => _ = new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource(s), null, new RDFResource(o)));

    [TestMethod]
    [DataRow("http://example.org/subj", "bnode:hdh744", "test")]
    public void ShouldThrowExceptionOnCreatingSPLQuadrupleBecauseOfBlankPredicate(string s, string p, string l)
        => Assert.ThrowsExactly<RDFStoreException>(() => _ = new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l)));

    [TestMethod]
    [DataRow("http://example.org/subj", "test")]
    public void ShouldThrowExceptionOnCreatingSPLQuadrupleBecauseOfNullPredicate(string s, string l)
        => Assert.ThrowsExactly<RDFStoreException>(() => _ = new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource(s), null, new RDFPlainLiteral(l)));

    [TestMethod]
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
        Assert.AreEqual(4, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)quadruple.Object)));
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
    public async Task ShouldReifySPOQuadrupleAsync(string s, string p, string o)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFResource subj = new RDFResource(s);
        RDFResource pred = new RDFResource(p);
        RDFResource obj = new RDFResource(o);

        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, obj);
        RDFMemoryStore store = await quadruple.ReifyQuadrupleAsync();
        Assert.IsNotNull(store);
        Assert.AreEqual(4, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)quadruple.Object)));
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj", "http://example.org/pred2", "http://example.org/obj2")]
    public void ShouldReifySPOQuadrupleWithAnnotations(string s, string p, string o, string p2, string o2)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFMemoryStore store = quadruple.ReifyQuadruple([(new RDFResource(p2), new RDFResource(o2))]);
        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)quadruple.Object)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, new RDFResource(p2), new RDFResource(o2))));
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj", "http://example.org/pred2", "http://example.org/obj2")]
    public async Task ShouldReifySPOQuadrupleWithAnnotationsAsync(string s, string p, string o, string p2, string o2)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFMemoryStore store = await quadruple.ReifyQuadrupleAsync([(new RDFResource(p2), new RDFResource(o2))]);
        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)quadruple.Object)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, new RDFResource(p2), new RDFResource(o2))));
    }

    [TestMethod]
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
        Assert.AreEqual(4, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)quadruple.Object)));
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
    public async Task ShouldReifySPLQuadrupleAsync(string s, string p, string l)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFResource subj = new RDFResource(s);
        RDFResource pred = new RDFResource(p);
        RDFPlainLiteral lit = new RDFPlainLiteral(l);

        RDFQuadruple quadruple = new RDFQuadruple(ctx, subj, pred, lit);
        RDFMemoryStore store = await quadruple.ReifyQuadrupleAsync();
        Assert.IsNotNull(store);
        Assert.AreEqual(4, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)quadruple.Object)));
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test", "http://example.org/pred2", "test2")]
    public void ShouldReifySPLQuadrupleWithAnnotations(string s, string p, string l, string p2, string l2)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFMemoryStore store = quadruple.ReifyQuadruple([(new RDFResource(p2), new RDFPlainLiteral(l2))]);
        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)quadruple.Object)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, new RDFResource(p2), new RDFPlainLiteral(l2))));
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test", "http://example.org/pred2", "test2")]
    public async Task ShouldReifySPLQuadrupleWithAnnotationsAsync(string s, string p, string l, string p2, string l2)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFMemoryStore store = await quadruple.ReifyQuadrupleAsync([(new RDFResource(p2), new RDFPlainLiteral(l2))]);
        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)quadruple.Subject)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)quadruple.Predicate)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)quadruple.Object)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, quadruple.ReificationSubject, new RDFResource(p2), new RDFPlainLiteral(l2))));
    }

    // RDF 1.2

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
    public void ShouldReifySPOQuadrupleTerm(string s, string p, string o)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFMemoryStore store = quadruple.ReifyQuadrupleTerm();
        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)quadruple.Subject, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)quadruple.Predicate, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)quadruple.Object, null].QuadruplesCount);
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
    public async Task ShouldReifySPOQuadrupleTermAsync(string s, string p, string o)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFMemoryStore store = await quadruple.ReifyQuadrupleTermAsync();
        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)quadruple.Subject, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)quadruple.Predicate, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)quadruple.Object, null].QuadruplesCount);
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj", "http://example.org/pred2", "http://example.org/obj2")]
    public void ShouldReifySPOQuadrupleTermWithAnnotations(string s, string p, string o, string p2, string o2)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFMemoryStore store = quadruple.ReifyQuadrupleTerm([(new RDFResource(p2), new RDFResource(o2))]);
        Assert.IsNotNull(store);
        Assert.AreEqual(6, store.QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)quadruple.Subject, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)quadruple.Predicate, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)quadruple.Object, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, new RDFResource(p2), new RDFResource(o2), null].QuadruplesCount);
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj", "http://example.org/pred2", "http://example.org/obj2")]
    public async Task ShouldReifySPOQuadrupleTermWithAnnotationsAsync(string s, string p, string o, string p2, string o2)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFMemoryStore store = await quadruple.ReifyQuadrupleTermAsync([(new RDFResource(p2), new RDFResource(o2))]);
        Assert.IsNotNull(store);
        Assert.AreEqual(6, store.QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)quadruple.Subject, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)quadruple.Predicate, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)quadruple.Object, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, new RDFResource(p2), new RDFResource(o2), null].QuadruplesCount);
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
    public void ShouldReifySPLQuadrupleTerm(string s, string p, string l)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFMemoryStore store = quadruple.ReifyQuadrupleTerm();
        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)quadruple.Subject, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)quadruple.Predicate, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_OBJECT, null, (RDFLiteral)quadruple.Object].QuadruplesCount);
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
    public async Task ShouldReifySPLQuadrupleTermAsync(string s, string p, string l)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFMemoryStore store = await quadruple.ReifyQuadrupleTermAsync();
        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)quadruple.Subject, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)quadruple.Predicate, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_OBJECT, null, (RDFLiteral)quadruple.Object].QuadruplesCount);
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test", "http://example.org/pred2", "test2")]
    public void ShouldReifySPLQuadrupleTermWithAnnotations(string s, string p, string l, string p2, string l2)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFMemoryStore store = quadruple.ReifyQuadrupleTerm([(new RDFResource(p2), new RDFPlainLiteral(l2))]);
        Assert.IsNotNull(store);
        Assert.AreEqual(6, store.QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)quadruple.Subject, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)quadruple.Predicate, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_OBJECT, null, (RDFLiteral)quadruple.Object].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, new RDFResource(p2), null, new RDFPlainLiteral(l2)].QuadruplesCount);
    }

    [TestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test", "http://example.org/pred2", "test2")]
    public async Task ShouldReifySPLQuadrupleTermWithAnnotationsAsync(string s, string p, string l, string p2, string l2)
    {
        RDFContext ctx = new RDFContext("ex:ctx");
        RDFQuadruple quadruple = new RDFQuadruple(ctx, new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFMemoryStore store = await quadruple.ReifyQuadrupleTermAsync([(new RDFResource(p2), new RDFPlainLiteral(l2))]);
        Assert.IsNotNull(store);
        Assert.AreEqual(6, store.QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)quadruple.Subject, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)quadruple.Predicate, null].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, null, RDFVocabulary.RDF.TT_OBJECT, null, (RDFLiteral)quadruple.Object].QuadruplesCount);
        Assert.AreEqual(1, store[ctx, quadruple.ReificationSubject, new RDFResource(p2), null, new RDFPlainLiteral(l2)].QuadruplesCount);
    }
    #endregion
}