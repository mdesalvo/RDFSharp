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
using System.Threading.Tasks;

namespace RDFSharp.Test.Model;

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
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPO, triple.TripleFlavor);
        Assert.IsTrue(triple.Subject.Equals(subj));
        Assert.IsTrue(triple.Predicate.Equals(pred));
        Assert.IsTrue(triple.Object.Equals(obj));
        Assert.IsTrue(triple.ReificationSubject.Equals(new RDFResource($"bnode:{triple.TripleID}")));

        string tripleString = triple.ToString();
        Assert.IsTrue(tripleString.Equals($"{triple.Subject} {triple.Predicate} {triple.Object}"));

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
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPO, triple.TripleFlavor);
        Assert.IsTrue(((RDFResource)triple.Subject).IsBlank);
        Assert.IsTrue(triple.Predicate.Equals(pred));
        Assert.IsTrue(((RDFResource)triple.Object).IsBlank);
        Assert.IsTrue(triple.ReificationSubject.Equals(new RDFResource($"bnode:{triple.TripleID}")));
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
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPL, triple.TripleFlavor);
        Assert.IsTrue(triple.Subject.Equals(subj));
        Assert.IsTrue(triple.Predicate.Equals(pred));
        Assert.IsTrue(triple.Object.Equals(lit));
        Assert.IsTrue(triple.ReificationSubject.Equals(new RDFResource($"bnode:{triple.TripleID}")));

        string tripleString = triple.ToString();
        Assert.IsTrue(tripleString.Equals($"{triple.Subject} {triple.Predicate} {triple.Object}"));

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
        Assert.AreEqual(RDFModelEnums.RDFTripleFlavors.SPL, triple.TripleFlavor);
        Assert.IsTrue(((RDFResource)triple.Subject).IsBlank);
        Assert.IsTrue(triple.Predicate.Equals(pred));
        Assert.IsTrue(((RDFPlainLiteral)triple.Object).Equals(RDFPlainLiteral.Empty));
        Assert.IsTrue(triple.ReificationSubject.Equals(new RDFResource($"bnode:{triple.TripleID}")));
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "bnode:hdh744", "http://example.org/obj")]
    public void ShouldNotCreateSPOTripleBecauseOfBlankPredicate(string s, string p, string o)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFResource(o)));

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/obj")]
    public void ShouldNotCreateSPOTripleBecauseOfNullPredicate(string s, string o)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTriple(new RDFResource(s), null, new RDFResource(o)));

    [DataTestMethod]
    [DataRow("http://example.org/subj", "bnode:hdh744", "test")]
    public void ShouldNotCreateSPLTripleBecauseOfBlankPredicate(string s, string p, string l)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l)));

    [DataTestMethod]
    [DataRow("http://example.org/subj", "test")]
    public void ShouldNotCreateSPLTripleBecauseOfNullPredicate(string s, string l)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTriple(new RDFResource(s), null, new RDFPlainLiteral(l)));

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
        Assert.AreEqual(4, graph.TriplesCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)triple.Object)));
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
    public async Task ShouldReifySPOTripleAsync(string s, string p, string o)
    {
        RDFResource subj = new RDFResource(s);
        RDFResource pred = new RDFResource(p);
        RDFResource obj = new RDFResource(o);

        RDFTriple triple = new RDFTriple(subj, pred, obj);
        RDFGraph graph = await triple.ReifyTripleAsync();
        Assert.IsNotNull(graph);
        Assert.AreEqual(4, graph.TriplesCount);
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)triple.Object)));
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj", "http://example.org/pred2", "http://example.org/obj2")]
    public void ShouldReifySPOTripleWithAnnotations(string s, string p, string o, string p2, string o2)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFGraph graph = triple.ReifyTriple([(new RDFResource(p2), new RDFResource(o2))]);
        Assert.IsNotNull(graph);
        Assert.AreEqual(5, graph.TriplesCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)triple.Object)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, new RDFResource(p2), new RDFResource(o2))));
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj", "http://example.org/pred2", "http://example.org/obj2")]
    public async Task ShouldReifySPOTripleWithAnnotationsAsync(string s, string p, string o, string p2, string o2)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFGraph graph = await triple.ReifyTripleAsync([(new RDFResource(p2), new RDFResource(o2))]);
        Assert.IsNotNull(graph);
        Assert.AreEqual(5, graph.TriplesCount);
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)triple.Object)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, new RDFResource(p2), new RDFResource(o2))));
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
        Assert.AreEqual(4, graph.TriplesCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)triple.Object)));
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
    public async Task ShouldReifySPLTripleAsync(string s, string p, string l)
    {
        RDFResource subj = new RDFResource(s);
        RDFResource pred = new RDFResource(p);
        RDFPlainLiteral lit = new RDFPlainLiteral(l);

        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraph graph = await triple.ReifyTripleAsync();
        Assert.IsNotNull(graph);
        Assert.AreEqual(4, graph.TriplesCount);
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)triple.Object)));
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test", "http://example.org/pred2", "test2")]
    public void ShouldReifySPLTripleWithAnnotations(string s, string p, string l, string p2, string l2)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFGraph graph = triple.ReifyTriple([(new RDFResource(p2), new RDFPlainLiteral(l2))]);
        Assert.IsNotNull(graph);
        Assert.AreEqual(5, graph.TriplesCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)triple.Object)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(triple.ReificationSubject, new RDFResource(p2), new RDFPlainLiteral(l2))));
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test", "http://example.org/pred2", "test2")]
    public async Task ShouldReifySPLTripleWithAnnotationsAsync(string s, string p, string l, string p2, string l2)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFGraph graph = await triple.ReifyTripleAsync([(new RDFResource(p2), new RDFPlainLiteral(l2))]);
        Assert.IsNotNull(graph);
        Assert.AreEqual(5, graph.TriplesCount);
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)triple.Object)));
        Assert.IsTrue(await graph.ContainsTripleAsync(new RDFTriple(triple.ReificationSubject, new RDFResource(p2), new RDFPlainLiteral(l2))));
    }

    //RDF 1.2

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
    public void ShouldReifySPOTripleTerm(string s, string p, string o)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFGraph graph = triple.ReifyTripleTerm();
        Assert.IsNotNull(graph);
        Assert.AreEqual(5, graph.TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)triple.Subject, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)triple.Predicate, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)triple.Object, null].TriplesCount);
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj")]
    public async Task ShouldReifySPOTripleTermAsync(string s, string p, string o)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFGraph graph = await triple.ReifyTripleTermAsync();
        Assert.IsNotNull(graph);
        Assert.AreEqual(5, graph.TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)triple.Subject, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)triple.Predicate, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)triple.Object, null].TriplesCount);
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj", "http://example.org/pred2", "http://example.org/obj2")]
    public void ShouldReifySPOTripleTermWithAnnotations(string s, string p, string o, string p2, string o2)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFGraph graph = triple.ReifyTripleTerm([(new RDFResource(p2), new RDFResource(o2))]);
        Assert.IsNotNull(graph);
        Assert.AreEqual(6, graph.TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)triple.Subject, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)triple.Predicate, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)triple.Object, null].TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, new RDFResource(p2), new RDFResource(o2), null].TriplesCount);
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "http://example.org/obj", "http://example.org/pred2", "http://example.org/obj2")]
    public async Task ShouldReifySPOTripleTermWithAnnotationsAsync(string s, string p, string o, string p2, string o2)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFResource(o));
        RDFGraph graph = await triple.ReifyTripleTermAsync([(new RDFResource(p2), new RDFResource(o2))]);
        Assert.IsNotNull(graph);
        Assert.AreEqual(6, graph.TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)triple.Subject, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)triple.Predicate, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)triple.Object, null].TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, new RDFResource(p2), new RDFResource(o2), null].TriplesCount);
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
    public void ShouldReifySPLTripleTerm(string s, string p, string l)
    {
        RDFResource subj = new RDFResource(s);
        RDFResource pred = new RDFResource(p);
        RDFPlainLiteral lit = new RDFPlainLiteral(l);

        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraph graph = triple.ReifyTripleTerm();
        Assert.IsNotNull(graph);
        Assert.AreEqual(5, graph.TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)triple.Subject, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)triple.Predicate, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_OBJECT, null, (RDFLiteral)triple.Object].TriplesCount);
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test")]
    public async Task ShouldReifySPLTripleTermAsync(string s, string p, string l)
    {
        RDFResource subj = new RDFResource(s);
        RDFResource pred = new RDFResource(p);
        RDFPlainLiteral lit = new RDFPlainLiteral(l);

        RDFTriple triple = new RDFTriple(subj, pred, lit);
        RDFGraph graph = await triple.ReifyTripleTermAsync();
        Assert.IsNotNull(graph);
        Assert.AreEqual(5, graph.TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)triple.Subject, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)triple.Predicate, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_OBJECT, null, (RDFLiteral)triple.Object].TriplesCount);
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test", "http://example.org/pred2", "test2")]
    public void ShouldReifySPLTripleTermWithAnnotations(string s, string p, string l, string p2, string l2)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFGraph graph = triple.ReifyTripleTerm([(new RDFResource(p2), new RDFPlainLiteral(l2))]);
        Assert.IsNotNull(graph);
        Assert.AreEqual(6, graph.TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)triple.Subject, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)triple.Predicate, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_OBJECT, null, (RDFLiteral)triple.Object].TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, new RDFResource(p2), null, new RDFPlainLiteral(l2)].TriplesCount);
    }

    [DataTestMethod]
    [DataRow("http://example.org/subj", "http://example.org/pred", "test", "http://example.org/pred2", "test2")]
    public async Task ShouldReifySPLTripleTermWithAnnotationsAsync(string s, string p, string l, string p2, string l2)
    {
        RDFTriple triple = new RDFTriple(new RDFResource(s), new RDFResource(p), new RDFPlainLiteral(l));
        RDFGraph graph = await triple.ReifyTripleTermAsync([(new RDFResource(p2), new RDFPlainLiteral(l2))]);
        Assert.IsNotNull(graph);
        Assert.AreEqual(6, graph.TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, RDFVocabulary.RDF.REIFIES, null, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)triple.Subject, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)triple.Predicate, null].TriplesCount);
        Assert.AreEqual(1, graph[null, RDFVocabulary.RDF.TT_OBJECT, null, (RDFLiteral)triple.Object].TriplesCount);
        Assert.AreEqual(1, graph[triple.ReificationSubject, new RDFResource(p2), null, new RDFPlainLiteral(l2)].TriplesCount);
    }
    #endregion
}