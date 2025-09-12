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
using System.Data;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFGraphTest
{
    #region Tests (Sync)
    [TestMethod]
    public void ShouldCreateEmptyGraph()
    {
        RDFGraph graph = new RDFGraph();

        Assert.IsNotNull(graph);
        Assert.IsNotNull(graph.Triples);
        Assert.AreEqual(0, graph.TriplesCount);
        Assert.AreEqual(5, graph.Triples.Columns.Count);
        Assert.IsTrue(graph.Triples.Columns.Contains("TID"));
        Assert.IsTrue(graph.Triples.Columns.Contains("SID"));
        Assert.IsTrue(graph.Triples.Columns.Contains("PID"));
        Assert.IsTrue(graph.Triples.Columns.Contains("OID"));
        Assert.IsTrue(graph.Triples.Columns.Contains("TFV"));
        Assert.IsNotNull(graph.Triples.PrimaryKey);
        Assert.AreEqual("TID", graph.Triples.PrimaryKey[0].ColumnName);
        Assert.IsNotNull(graph.Triples.ExtendedProperties["RES"]);
        Assert.IsNotNull(graph.Triples.ExtendedProperties["LIT"]);
        Assert.IsEmpty((Dictionary<long, RDFResource>)graph.Triples.ExtendedProperties["RES"]!);
        Assert.IsEmpty((Dictionary<long, RDFLiteral>)graph.Triples.ExtendedProperties["LIT"]!);
        Assert.IsNotNull(graph.Context);
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));

        int i = graph.Count();
        Assert.AreEqual(0, i);

        int j = 0;
        IEnumerator<RDFTriple> triplesEnumerator = graph.TriplesEnumerator;
        while (triplesEnumerator.MoveNext()) j++;
        Assert.AreEqual(0, j);
    }

    [TestMethod]
    public void ShouldCreateGraphFromTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
            new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
        ]);

        Assert.IsNotNull(graph);
        Assert.IsNotNull(graph.Triples);
        Assert.AreEqual(2, graph.TriplesCount);
        Assert.HasCount(3, (Dictionary<long, RDFResource>)graph.Triples.ExtendedProperties["RES"]!);
        Assert.IsTrue(((Dictionary<long, RDFResource>)graph.Triples.ExtendedProperties["RES"]!).ContainsKey(new RDFResource("http://subj/").PatternMemberID));
        Assert.IsTrue(((Dictionary<long, RDFResource>)graph.Triples.ExtendedProperties["RES"]!).ContainsKey(new RDFResource("http://pred/").PatternMemberID));
        Assert.IsTrue(((Dictionary<long, RDFResource>)graph.Triples.ExtendedProperties["RES"]!).ContainsKey(new RDFResource("http://obj/").PatternMemberID));
        Assert.HasCount(1, (Dictionary<long, RDFLiteral>)graph.Triples.ExtendedProperties["LIT"]!);
        Assert.IsTrue(((Dictionary<long, RDFLiteral>)graph.Triples.ExtendedProperties["LIT"]!).ContainsKey(new RDFPlainLiteral("lit").PatternMemberID));
        Assert.IsNotNull(graph.Context);
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
    }

    [TestMethod]
    public void ShouldDisposeGraphWithUsing()
    {
        RDFGraph graph;
        using (graph = new RDFGraph(
        [
           new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
           new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
        ]))
        {
            Assert.IsFalse(graph.Disposed);
            Assert.IsNotNull(graph.Triples);
        }
        Assert.IsTrue(graph.Disposed);
        Assert.IsNull(graph.Triples);
    }

    [TestMethod]
    [DataRow("http://example.org/")]
    public void ShouldSetContext(string input)
    {
        RDFGraph graph = new RDFGraph().SetContext(new Uri(input));
        Assert.IsTrue(graph.Context.Equals(new Uri(input)));
    }

    [TestMethod]
    public void ShouldNotSetContextBecauseNullUri()
    {
        RDFGraph graph = new RDFGraph().SetContext(null);
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
    }

    [TestMethod]
    public void ShouldNotSetContextBecauseRelativeUri()
    {
        RDFGraph graph = new RDFGraph().SetContext(new Uri("file/system", UriKind.Relative));
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
    }

    [TestMethod]
    public void ShouldNotSetContextBecauseBlankNodeUri()
    {
        RDFGraph graph = new RDFGraph().SetContext(new Uri("bnode:12345"));
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
    }

    [TestMethod]
    public void ShouldGetDefaultStringRepresentation()
    {
        RDFGraph graph = new RDFGraph();
        Assert.IsTrue(graph.ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldGetCustomStringRepresentation()
    {
        RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
        Assert.IsTrue(graph.ToString().Equals("http://example.org/", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEnumerateGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
            new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
        ]);

        Assert.AreEqual(2, graph.Count());

        int j = 0;
        IEnumerator<RDFTriple> triplesEnumerator = graph.TriplesEnumerator;
        while(triplesEnumerator.MoveNext()) j++;
        Assert.AreEqual(2, j);
    }

    [TestMethod]
    public void ShouldCompareEquallyToEmptyGraph()
    {
        RDFGraph graphA = new RDFGraph();
        RDFGraph graphB = new RDFGraph();

        Assert.IsTrue(graphA.Equals(graphB));
    }

    [TestMethod]
    public void ShouldCompareEquallyToGraph()
    {
        RDFGraph graphA = new RDFGraph().AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
        RDFGraph graphB = new RDFGraph().AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));

        Assert.IsTrue(graphA.Equals(graphB));
    }

    [TestMethod]
    public void ShouldCompareEquallyToSelf()
    {
        RDFGraph graphA = new RDFGraph().AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));

        Assert.IsTrue(graphA.Equals(graphA));
    }

    [TestMethod]
    public void ShouldNotCompareEquallyToNullGraph()
    {
        RDFGraph graphA = new RDFGraph();

        Assert.IsFalse(graphA.Equals(null));
    }

    [TestMethod]
    public void ShouldNotCompareEquallyToMoreNumerousGraph()
    {
        RDFGraph graphA = new RDFGraph().AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
        RDFGraph graphB = new RDFGraph().AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")))
                                        .AddTriple(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));

        Assert.IsFalse(graphA.Equals(graphB));
    }

    [TestMethod]
    public void ShouldNotCompareEquallyToDifferentGraph()
    {
        RDFGraph graphA = new RDFGraph().AddTriple(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
        RDFGraph graphB = new RDFGraph().AddTriple(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));

        Assert.IsFalse(graphA.Equals(graphB));
    }

    [TestMethod]
    public void ShouldAddTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotAddDuplicateTriples()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple).AddTriple(triple);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotAddNullTriple()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(null);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldAddContainer()
    {
        RDFGraph graph = new RDFGraph();
        RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
        cont.AddItem(new RDFPlainLiteral("hello"));
        graph.AddContainer(cont);

        Assert.AreEqual(2, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldAddEmptyContainer()
    {
        RDFGraph graph = new RDFGraph();
        RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
        graph.AddContainer(cont);

        Assert.AreEqual(1, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotAddNullContainer()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddContainer(null);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldAddFilledCollection()
    {
        RDFGraph graph = new RDFGraph();
        RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
        coll.AddItem(new RDFPlainLiteral("hello"));
        graph.AddCollection(coll);

        Assert.AreEqual(3, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotAddEmptyCollection()
    {
        RDFGraph graph = new RDFGraph();
        RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
        graph.AddCollection(coll);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotAddNullCollection()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddCollection(null);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldAddDatatype()
    {
        RDFGraph graph = new RDFGraph();
        RDFDatatype exlength6 = new RDFDatatype(new Uri("ex:exlength6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFLengthFacet(6), new RDFPatternFacet("^ex") ]);
        graph.AddDatatype(exlength6);

        Assert.AreEqual(11, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldRemoveTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriple(triple);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveUnexistingTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"));
        graph.AddTriple(triple1);
        graph.RemoveTriple(triple2);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple1.TripleID));
        Assert.IsNull(graph.Triples.Rows.Find(triple2.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveNullTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriple(null);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveAllTriplesByNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples();

        Assert.IsEmpty(graph);
    }

    [TestMethod]
    public void ShouldRemoveTriplesBySubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: (RDFResource)triple.Subject);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectBecauseUnexisting()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: new RDFResource("http://subj2/"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(p: (RDFResource)triple.Predicate);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateBecauseUnexisting()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(p: new RDFResource("http://pred2/"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(o: (RDFResource)triple.Object);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByObjectBecauseUnexisting()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(o: new RDFResource("http://obj2/"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en","US"));
        graph.AddTriple(triple);
        graph.RemoveTriples(l: (RDFLiteral)triple.Object);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByLiteralBecauseUnexisting()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
        graph.AddTriple(triple);
        graph.RemoveTriples(l: new RDFPlainLiteral("en"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesBySubjectPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: (RDFResource)triple.Subject, p: (RDFResource)triple.Predicate);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: new RDFResource("http://subj2/"), p: (RDFResource)triple.Predicate);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: (RDFResource)triple.Subject, p: new RDFResource("http://pred2/"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesBySubjectObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: (RDFResource)triple.Subject, o: (RDFResource)triple.Object);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: new RDFResource("http://subj2/"), o: (RDFResource)triple.Object);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: (RDFResource)triple.Subject, o: new RDFResource("http://obj2/"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesBySubjectLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: (RDFResource)triple.Subject, l: (RDFLiteral)triple.Object);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: new RDFResource("http://subj2/"), l: (RDFLiteral)triple.Object);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriples(s: (RDFResource)triple.Subject, l: new RDFPlainLiteral("lit2"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByPredicateObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(p: (RDFResource)triple.Predicate, o: (RDFResource)triple.Object);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateObjectBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(p: new RDFResource("http://pred2/"), o: (RDFResource)triple.Object);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateObjectBecauseUnexistingObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriples(p: (RDFResource)triple.Predicate, o: new RDFResource("http://obj2/"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByPredicateLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriples(p: (RDFResource)triple.Predicate, l: (RDFLiteral)triple.Object);

        Assert.AreEqual(0, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateLiteralBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriples(p: new RDFResource("http://pred2/"), l: (RDFLiteral)triple.Object);

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateLiteralBecauseUnexistingLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriples(p: (RDFResource)triple.Predicate, l: new RDFPlainLiteral("lit2"));

        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsNotNull(graph.Triples.Rows.Find(triple.TripleID));
    }

    [TestMethod]
    public void ShouldClearTriples()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.ClearTriples();

        Assert.AreEqual(0, graph.TriplesCount);
        Assert.IsEmpty((Dictionary<long, RDFResource>)graph.Triples.ExtendedProperties["RES"]!);
        Assert.IsEmpty((Dictionary<long, RDFLiteral>)graph.Triples.ExtendedProperties["LIT"]!);
    }

    [TestMethod]
    public void ShouldContainTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);

        Assert.IsTrue(graph.ContainsTriple(triple));
    }

    [TestMethod]
    public void ShouldNotContainTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple1);

        Assert.IsFalse(graph.ContainsTriple(triple2));
    }

    [TestMethod]
    public void ShouldNotContainNullTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);

        Assert.IsFalse(graph.ContainsTriple(null));
    }

    [TestMethod]
    public void ShouldSelectSTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"));

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldNotSelectSTriplesBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj6/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectPTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(p: new RDFResource("http://pred2/"));

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldNotSelectPTriplesBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(p: new RDFResource("http://pred6/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectOTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(o: new RDFResource("http://obj2/"));

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldNotSelectOTriplesBecauseUnexistingObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(o: new RDFResource("http://obj6/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectLTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(l: new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
    }

    [TestMethod]
    public void ShouldNotSelectLTriplesBecauseUnexistingLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(l: new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), p: new RDFResource("http://pred2/"));

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
    }

    [TestMethod]
    public void ShouldNotSelectSPTriplesBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj6/"), p: new RDFResource("http://pred2/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPTriplesBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), p: new RDFResource("http://pred6/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSOTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), o: new RDFResource("http://obj2/"));

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
    }

    [TestMethod]
    public void ShouldNotSelectSOTriplesBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj6/"), o: new RDFResource("http://obj2/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSOTriplesBecauseUnexistingObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), o: new RDFResource("http://obj6/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSLTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), l: new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
    }

    [TestMethod]
    public void ShouldNotSelectSLTriplesBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj6/"), l: new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSLTriplesBecauseUnexistingLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), l: new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectPOTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(p: new RDFResource("http://pred2/"), o: new RDFResource("http://obj2/"));

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldNotSelectPOTriplesBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(p: new RDFResource("http://pred6/"), o: new RDFResource("http://obj2/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectPOTriplesBecauseUnexistingObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(p: new RDFResource("http://pred1/"), o: new RDFResource("http://obj6/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectPLTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(p: new RDFResource("http://pred2/"), l: new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
    }

    [TestMethod]
    public void ShouldNotSelectPLTriplesBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(p: new RDFResource("http://pred6/"), l: new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectPLTriplesBecauseUnexistingLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(p: new RDFResource("http://pred2/"), l: new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPOTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), p: new RDFResource("http://pred2/"), o: new RDFResource("http://obj2/"));

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj6/"), p: new RDFResource("http://pred2/"), o: new RDFResource("http://obj2/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), p: new RDFResource("http://pred6/"), o: new RDFResource("http://obj2/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBecauseUnexistingObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), p: new RDFResource("http://pred1/"), o: new RDFResource("http://obj6/"));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPLTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), p: new RDFResource("http://pred2/"), l: new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj6/"), p: new RDFResource("http://pred2/"), l: new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), p: new RDFResource("http://pred6/"), l: new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBecauseUnexistingLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = graph.SelectTriples(s: new RDFResource("http://subj1/"), p: new RDFResource("http://pred2/"), l: new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectTriplesByNullAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[null, null, null, null];
        Assert.IsNotNull(select);
        Assert.AreEqual(3, select.TriplesCount);
    }

    [TestMethod]
    public void ShouldSelectTriplesBySubjectAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[s: new RDFResource("http://subj/")];
        Assert.IsNotNull(select);
        Assert.AreEqual(2, select.TriplesCount);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnSelectingTriplesByForbiddenOLAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        Assert.ThrowsExactly<RDFModelException>(() => _ = graph[o: new RDFResource("http://obj/"), l: new RDFPlainLiteral("lit")]);
    }

    [TestMethod]
    public void ShouldIntersectGraphs()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        RDFGraph graph2 = new RDFGraph();
        graph2.AddTriple(triple1);

        RDFGraph intersect12 = graph1.IntersectWith(graph2);
        Assert.IsNotNull(intersect12);
        Assert.AreEqual(1, intersect12.TriplesCount);
        Assert.IsNotNull(intersect12.Triples.Rows.Find(triple1.TripleID));
        RDFGraph intersect21 = graph2.IntersectWith(graph1);
        Assert.IsNotNull(intersect21);
        Assert.AreEqual(1, intersect21.TriplesCount);
        Assert.IsNotNull(intersect21.Triples.Rows.Find(triple1.TripleID));
    }

    [TestMethod]
    public void ShouldIntersectGraphWithEmpty()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        RDFGraph graph2 = new RDFGraph();

        RDFGraph intersect12 = graph1.IntersectWith(graph2);
        Assert.IsNotNull(intersect12);
        Assert.AreEqual(0, intersect12.TriplesCount);
        RDFGraph intersect21 = graph2.IntersectWith(graph1);
        Assert.IsNotNull(intersect21);
        Assert.AreEqual(0, intersect21.TriplesCount);
    }

    [TestMethod]
    public void ShouldIntersectEmptyWithGraph()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFGraph graph2 = new RDFGraph();
        graph2.AddTriple(triple1).AddTriple(triple2);

        RDFGraph intersect12 = graph1.IntersectWith(graph2);
        Assert.IsNotNull(intersect12);
        Assert.AreEqual(0, intersect12.TriplesCount);
        RDFGraph intersect21 = graph2.IntersectWith(graph1);
        Assert.IsNotNull(intersect21);
        Assert.AreEqual(0, intersect21.TriplesCount);
    }

    [TestMethod]
    public void ShouldIntersectEmptyWithEmpty()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFGraph graph2 = new RDFGraph();

        RDFGraph intersect12 = graph1.IntersectWith(graph2);
        Assert.IsNotNull(intersect12);
        Assert.AreEqual(0, intersect12.TriplesCount);
        RDFGraph intersect21 = graph2.IntersectWith(graph1);
        Assert.IsNotNull(intersect21);
        Assert.AreEqual(0, intersect21.TriplesCount);
    }

    [TestMethod]
    public void ShouldIntersectGraphWithNull()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);

        RDFGraph intersect12 = graph1.IntersectWith(null);
        Assert.IsNotNull(intersect12);
        Assert.AreEqual(0, intersect12.TriplesCount);
    }

    [TestMethod]
    public void ShouldIntersectGraphWithSelf()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);

        RDFGraph intersect12 = graph1.IntersectWith(graph1);
        Assert.IsNotNull(intersect12);
        Assert.AreEqual(2, intersect12.TriplesCount);
    }

    [TestMethod]
    public void ShouldUnionGraphs()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj3/"), new RDFResource("http://pred3/"), new RDFResource("http://obj3/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        RDFGraph graph2 = new RDFGraph();
        graph2.AddTriple(triple1).AddTriple(triple3);

        RDFGraph union12 = graph1.UnionWith(graph2);
        Assert.IsNotNull(union12);
        Assert.AreEqual(3, union12.TriplesCount);
        RDFGraph union21 = graph2.UnionWith(graph1);
        Assert.IsNotNull(union21);
        Assert.AreEqual(3, union21.TriplesCount);
    }

    [TestMethod]
    public void ShouldUnionGraphWithEmpty()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        RDFGraph graph2 = new RDFGraph();

        RDFGraph union12 = graph1.UnionWith(graph2);
        Assert.IsNotNull(union12);
        Assert.AreEqual(2, union12.TriplesCount);
        RDFGraph union21 = graph2.UnionWith(graph1);
        Assert.IsNotNull(union21);
        Assert.AreEqual(2, union21.TriplesCount);
    }

    [TestMethod]
    public void ShouldUnionEmptyWithGraph()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFGraph graph2 = new RDFGraph();
        graph2.AddTriple(triple1).AddTriple(triple2);

        RDFGraph union12 = graph1.UnionWith(graph2);
        Assert.IsNotNull(union12);
        Assert.AreEqual(2, union12.TriplesCount);
        RDFGraph union21 = graph2.UnionWith(graph1);
        Assert.IsNotNull(union21);
        Assert.AreEqual(2, union21.TriplesCount);
    }

    [TestMethod]
    public void ShouldUnionGraphWithNull()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);

        RDFGraph union12 = graph1.UnionWith(null);
        Assert.IsNotNull(union12);
        Assert.AreEqual(2, union12.TriplesCount);
    }

    [TestMethod]
    public void ShouldUnionGraphWithSelf()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);

        RDFGraph union12 = graph1.UnionWith(graph1);
        Assert.IsNotNull(union12);
        Assert.AreEqual(2, union12.TriplesCount);
    }

    [TestMethod]
    public void ShouldDifferenceGraphs()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj3/"), new RDFResource("http://pred3/"), new RDFResource("http://obj3/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        RDFGraph graph2 = new RDFGraph();
        graph2.AddTriple(triple1).AddTriple(triple3);

        RDFGraph difference12 = graph1.DifferenceWith(graph2);
        Assert.IsNotNull(difference12);
        Assert.AreEqual(1, difference12.TriplesCount);
        Assert.IsNotNull(difference12.Triples.Rows.Find(triple2.TripleID));
        RDFGraph difference21 = graph2.DifferenceWith(graph1);
        Assert.IsNotNull(difference21);
        Assert.AreEqual(1, difference21.TriplesCount);
        Assert.IsNotNull(difference21.Triples.Rows.Find(triple3.TripleID));
    }

    [TestMethod]
    public void ShouldDifferenceGraphWithEmpty()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        RDFGraph graph2 = new RDFGraph();

        RDFGraph difference12 = graph1.DifferenceWith(graph2);
        Assert.IsNotNull(difference12);
        Assert.AreEqual(2, difference12.TriplesCount);
        Assert.IsNotNull(difference12.Triples.Rows.Find(triple2.TripleID));
        RDFGraph difference21 = graph2.DifferenceWith(graph1);
        Assert.IsNotNull(difference21);
        Assert.AreEqual(0, difference21.TriplesCount);
    }

    [TestMethod]
    public void ShouldDifferenceEmptyWithGraph()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFGraph graph2 = new RDFGraph();
        graph2.AddTriple(triple1).AddTriple(triple2);

        RDFGraph difference12 = graph1.DifferenceWith(graph2);
        Assert.IsNotNull(difference12);
        Assert.AreEqual(0, difference12.TriplesCount);
        RDFGraph difference21 = graph2.DifferenceWith(graph1);
        Assert.IsNotNull(difference21);
        Assert.AreEqual(2, difference21.TriplesCount);
    }

    [TestMethod]
    public void ShouldDifferenceGraphWithNull()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);

        RDFGraph difference12 = graph1.DifferenceWith(null);
        Assert.IsNotNull(difference12);
        Assert.AreEqual(2, difference12.TriplesCount);
    }

    [TestMethod]
    public void ShouldDifferenceGraphWithSelf()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);

        RDFGraph difference12 = graph1.DifferenceWith(graph1);
        Assert.IsNotNull(difference12);
        Assert.AreEqual(0, difference12.TriplesCount);
    }

    [TestMethod]
    [DataRow(".nt",RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public void ShouldExportToFile(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);
        graph.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFile{fileExtension}"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFile{fileExtension}")));
        Assert.IsGreaterThan(100, File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFile{fileExtension}")).Length);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnExportingToNullOrEmptyFilepath()
        => Assert.ThrowsExactly<RDFModelException>(() => new RDFGraph().ToFile(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    [DataRow(RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(RDFModelEnums.RDFFormats.TriX)]
    [DataRow(RDFModelEnums.RDFFormats.Turtle)]
    public void ShouldExportToStream(RDFModelEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);
        graph.ToStream(format, stream);

        Assert.IsGreaterThan(100, stream.ToArray().Length);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnExportingToNullStream()
        => Assert.ThrowsExactly<RDFModelException>(() => new RDFGraph().ToStream(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public void ShouldExportToDataTable()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit","en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);
        DataTable table = graph.ToDataTable();

        Assert.IsNotNull(table);
        Assert.AreEqual(3, table.Columns.Count);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.AreEqual(2, table.Rows.Count);
        Assert.IsTrue(table.Rows[0]["?SUBJECT"].ToString().Equals("http://subj/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?PREDICATE"].ToString().Equals("http://pred/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?SUBJECT"].ToString().Equals("http://subj/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?PREDICATE"].ToString().Equals("http://pred/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?OBJECT"].ToString().Equals("http://obj/", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldExportEmptyToDataTable()
    {
        RDFGraph graph = new RDFGraph();
        DataTable table = graph.ToDataTable();

        Assert.IsNotNull(table);
        Assert.AreEqual(3, table.Columns.Count);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.AreEqual(0, table.Rows.Count);
    }

    [TestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public void ShouldImportFromFile(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph1.AddTriple(triple1)
            .AddTriple(triple2);
        graph1.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFile{fileExtension}"));
        RDFGraph graph2 = RDFGraph.FromFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFile{fileExtension}"));

        Assert.IsNotNull(graph2);
        Assert.AreEqual(2, graph2.TriplesCount);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.AreEqual(0, graph2[p: new RDFResource("http://ex/pred/")].TriplesCount);
            Assert.AreEqual(2, graph2[p: new RDFResource("http://ex/pred")].TriplesCount);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
    }

    [TestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public void ShouldImportFromFileWithEnabledDatatypeDiscovery(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph1.AddTriple(triple1)
            .AddTriple(triple2)
            .AddDatatype(new RDFDatatype(new Uri($"ex:mydt{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFPatternFacet("^ex$") ]));
        graph1.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFile{fileExtension}WithEnabledDatatypeDiscovery"));
        RDFGraph graph2 = RDFGraph.FromFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFile{fileExtension}WithEnabledDatatypeDiscovery"), true);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(9, graph2.TriplesCount);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.AreEqual(0, graph2[p: new RDFResource("http://ex/pred/")].TriplesCount);
            Assert.AreEqual(2, graph2[p: new RDFResource("http://ex/pred")].TriplesCount);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype($"ex:mydt{(int)format}").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydt{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [TestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public void ShouldImportEmptyFromFile(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph1 = new RDFGraph();
        graph1.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportEmptyFromFile{fileExtension}"));
        RDFGraph graph2 = RDFGraph.FromFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportEmptyFromFile{fileExtension}"));

        Assert.IsNotNull(graph2);
        Assert.AreEqual(0, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullOrEmptyFilepath()
        => Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromFile(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromUnexistingFilepath()
        => Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromFile(RDFModelEnums.RDFFormats.NTriples, "blablabla"));

    [TestMethod]
    [DataRow(RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(RDFModelEnums.RDFFormats.TriX)]
    [DataRow(RDFModelEnums.RDFFormats.Turtle)]
    public void ShouldImportFromStream(RDFModelEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        graph1.ToStream(format, stream);
        RDFGraph graph2 = RDFGraph.FromStream(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(graph2);
        Assert.AreEqual(2, graph2.TriplesCount);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.AreEqual(0, graph2[p: new RDFResource("http://ex/pred/")].TriplesCount);
            Assert.AreEqual(2, graph2[p: new RDFResource("http://ex/pred")].TriplesCount);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(RDFModelEnums.RDFFormats.TriX)]
    [DataRow(RDFModelEnums.RDFFormats.Turtle)]
    public void ShouldImportFromStreamWithEnabledDatatypeDiscovery(RDFModelEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph1.AddTriple(triple1)
            .AddTriple(triple2)
            .AddDatatype(new RDFDatatype(new Uri($"ex:mydtT{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [new RDFPatternFacet("^ex$") ]));
        graph1.ToStream(format, stream);
        RDFGraph graph2 = RDFGraph.FromStream(format, new MemoryStream(stream.ToArray()), true);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(9, graph2.TriplesCount);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.AreEqual(0, graph2[p: new RDFResource("http://ex/pred/")].TriplesCount);
            Assert.AreEqual(2, graph2[p: new RDFResource("http://ex/pred")].TriplesCount);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype($"ex:mydtT{(int)format}").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtT{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(RDFModelEnums.RDFFormats.TriX)]
    [DataRow(RDFModelEnums.RDFFormats.Turtle)]
    public void ShouldImportFromEmptyStream(RDFModelEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFGraph graph1 = new RDFGraph();
        graph1.ToStream(format, stream);
        RDFGraph graph2 = RDFGraph.FromStream(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(graph2);
        Assert.AreEqual(0, graph2.TriplesCount);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullStream()
        => Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromStream(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public void ShouldImportFromDataTable()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        DataTable table = graph1.ToDataTable();
        RDFGraph graph2 = RDFGraph.FromDataTable(table);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(2, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public void ShouldImportFromDataTableWithEnabledDatatypeDiscovery()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1)
            .AddTriple(triple2)
            .AddDatatype(new RDFDatatype(new Uri("ex:mydtZ"), RDFModelEnums.RDFDatatypes.XSD_STRING, [new RDFPatternFacet("^ex$") ]));
        DataTable table = graph1.ToDataTable();
        RDFGraph graph2 = RDFGraph.FromDataTable(table, true);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(9, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype("ex:mydtZ").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype("ex:mydtZ").Facets.Single() is RDFPatternFacet { Pattern: "^ex$" });
    }

    [TestMethod]
    public void ShouldImportEmptyFromDataTable()
    {
        RDFGraph graph1 = new RDFGraph();
        DataTable table = graph1.ToDataTable();
        RDFGraph graph2 = RDFGraph.FromDataTable(table);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(0, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullDataTable()
        => Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableNotHavingMandatoryColumns()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableNotHavingExactColumns()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECTTTTT", typeof(string));

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullSubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add(null, "http://pred/", "http://obj/");

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptySubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("", "http://pred/", "http://obj/");

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralSubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("hello@en", "http://pred/", "http://obj/");

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", null, "http://obj/");

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptyPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "", "http://obj/");

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithBlankPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "bnode:12345", "http://obj/");

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "hello@en", "http://obj/");

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullObject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "http://pred/", null);

        Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldImportEmptyFromDataTableButGivingNameToGraph()
    {
        RDFGraph graph1 = new RDFGraph().SetContext(new Uri("http://context/"));
        DataTable table = graph1.ToDataTable();
        RDFGraph graph2 = RDFGraph.FromDataTable(table);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(0, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
        Assert.IsTrue(graph2.Context.Equals(new Uri("http://context/")));
    }

    [TestMethod]
    public void ShouldImportNTriplesFromUri()
    {
        RDFGraph graph = RDFGraph.FromUri(new Uri("https://w3c.github.io/rdf-tests/rdf/rdf11/rdf-n-triples/nt-syntax-uri-01.nt"));

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldImportXmlFromUri()
    {
        RDFGraph graph = RDFGraph.FromUri(new Uri("https://w3c.github.io/rdf-tests/rdf/rdf11/rdf-xml/datatypes/test001.rdf"));

        Assert.IsNotNull(graph);
        Assert.AreEqual(2, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldImportTurtleFromUri()
    {
        RDFGraph graph = RDFGraph.FromUri(new Uri("https://w3c.github.io/rdf-tests/rdf/rdf11/rdf-turtle/default_namespace_IRI.ttl"));

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullUri()
        => Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromUri(null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromRelativeUri()
        => Assert.ThrowsExactly<RDFModelException>(() => RDFGraph.FromUri(new Uri("/file/system", UriKind.Relative)));

    [TestMethod]
    public void ShouldExtractDatatypeDefinitionsFromGraph()
    {
        RDFDatatype exLength6 = new RDFDatatype(new Uri("ex:length6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFLengthFacet(6) ]);
        RDFDatatype exMinLength6 = new RDFDatatype(new Uri("ex:minlength6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFMinLengthFacet(6) ]);
        RDFDatatype exMaxLength6 = new RDFDatatype(new Uri("ex:maxlength6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFMaxLengthFacet(6) ]);
        RDFDatatype exMaxInclusive6 = new RDFDatatype(new Uri("ex:maxinclusive6"), RDFModelEnums.RDFDatatypes.XSD_DOUBLE, [new RDFMaxInclusiveFacet(6)]);
        RDFDatatype exMinInclusive6 = new RDFDatatype(new Uri("ex:mininclusive6"), RDFModelEnums.RDFDatatypes.XSD_DOUBLE, [new RDFMinInclusiveFacet(6)]);
        RDFDatatype exMaxExclusive6 = new RDFDatatype(new Uri("ex:maxexclusive6"), RDFModelEnums.RDFDatatypes.XSD_DOUBLE, [new RDFMaxExclusiveFacet(6)]);
        RDFDatatype exMinExclusive6 = new RDFDatatype(new Uri("ex:minexclusive6"), RDFModelEnums.RDFDatatypes.XSD_DOUBLE, [new RDFMinExclusiveFacet(6)]);
        RDFDatatype exPatternEx = new RDFDatatype(new Uri("ex:patternex"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFPatternFacet("^ex") ]);
        RDFDatatype exInteger = new RDFDatatype(new Uri("ex:integer"), RDFModelEnums.RDFDatatypes.XSD_INTEGER, null);
        RDFGraph graph = new RDFGraph()
            .AddDatatype(exLength6)
            .AddDatatype(exMinLength6)
            .AddDatatype(exMaxLength6)
            .AddDatatype(exMaxInclusive6)
            .AddDatatype(exMinInclusive6)
            .AddDatatype(exMaxExclusive6)
            .AddDatatype(exMinExclusive6)
            .AddDatatype(exPatternEx)
            .AddDatatype(exInteger);
        List<RDFDatatype> datatypes = graph.ExtractDatatypeDefinitions();

        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:length6", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING
                                          && dt.Facets.Single() is RDFLengthFacet { Length: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:minlength6", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING
                                          && dt.Facets.Single() is RDFMinLengthFacet { Length: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxlength6", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING
                                          && dt.Facets.Single() is RDFMaxLengthFacet { Length: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxinclusive6", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                                          && dt.Facets.Single() is RDFMaxInclusiveFacet { InclusiveUpperBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:mininclusive6", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                                          && dt.Facets.Single() is RDFMinInclusiveFacet { InclusiveLowerBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxexclusive6", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                                          && dt.Facets.Single() is RDFMaxExclusiveFacet { ExclusiveUpperBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:minexclusive6", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                                          && dt.Facets.Single() is RDFMinExclusiveFacet { ExclusiveLowerBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:patternex", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING
                                          && dt.Facets.Single() is RDFPatternFacet patternFacet
                                          && string.Equals(patternFacet.Pattern, "^ex", StringComparison.Ordinal)));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:integer", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_INTEGER
                                          && dt.Facets.Count == 0));

        Assert.IsEmpty((null as RDFGraph).ExtractDatatypeDefinitions());
    }

    [TestMethod]
    public void ShouldExtractDatatypeDefinitionsUsingOWLRationalFromGraph()
    {
        RDFDatatype exMaxInclusive6R = new RDFDatatype(new Uri("ex:maxinclusive6R"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, [new RDFMaxInclusiveFacet(6)]);
        RDFDatatype exMinInclusive6R = new RDFDatatype(new Uri("ex:mininclusive6R"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, [new RDFMinInclusiveFacet(6)]);
        RDFDatatype exMaxExclusive6R = new RDFDatatype(new Uri("ex:maxexclusive6R"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, [new RDFMaxExclusiveFacet(6)]);
        RDFDatatype exMinExclusive6R = new RDFDatatype(new Uri("ex:minexclusive6R"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, [new RDFMinExclusiveFacet(6)]);
        RDFDatatype aliasRational = new RDFDatatype(new Uri("ex:aliasRational"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, null);
        RDFGraph graph = new RDFGraph()
            .AddDatatype(exMaxInclusive6R)
            .AddDatatype(exMinInclusive6R)
            .AddDatatype(exMaxExclusive6R)
            .AddDatatype(exMinExclusive6R)
            .AddDatatype(aliasRational);
        List<RDFDatatype> datatypes = graph.ExtractDatatypeDefinitions();

        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxinclusive6R", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Single() is RDFMaxInclusiveFacet { InclusiveUpperBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:mininclusive6R", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Single() is RDFMinInclusiveFacet { InclusiveLowerBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxexclusive6R", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Single() is RDFMaxExclusiveFacet { ExclusiveUpperBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:minexclusive6R", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Single() is RDFMinExclusiveFacet { ExclusiveLowerBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:aliasRational", StringComparison.Ordinal)
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Count == 0));
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFGraphTest_Should*"))
            File.Delete(file);
    }
    #endregion

    #region Tests (Async)
    [TestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldExportToFileAsync(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);
        await graph.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFileAsync{fileExtension}"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFileAsync{fileExtension}")));
        Assert.IsGreaterThan(100, (await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFileAsync{fileExtension}"), TestContext.CancellationTokenSource.Token)).Length);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnExportingToNullOrEmptyFilepathAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(() => new RDFGraph().ToFileAsync(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    [DataRow(RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(RDFModelEnums.RDFFormats.TriX)]
    [DataRow(RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldExportToStreamAsync(RDFModelEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);
        await graph.ToStreamAsync(format, stream);

        Assert.IsGreaterThan(100, stream.ToArray().Length);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnExportingToNullStreamAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(() => new RDFGraph().ToStreamAsync(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public async Task ShouldExportToDataTableAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);
        DataTable table = await graph.ToDataTableAsync();

        Assert.IsNotNull(table);
        Assert.AreEqual(3, table.Columns.Count);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.AreEqual(2, table.Rows.Count);
        Assert.IsTrue(table.Rows[0]["?SUBJECT"]!.ToString()!.Equals("http://subj/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?PREDICATE"]!.ToString()!.Equals("http://pred/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[0]["?OBJECT"]!.ToString()!.Equals("lit@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?SUBJECT"]!.ToString()!.Equals("http://subj/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?PREDICATE"]!.ToString()!.Equals("http://pred/", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[1]["?OBJECT"]!.ToString()!.Equals("http://obj/", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ShouldExportEmptyToDataTableAsync()
    {
        RDFGraph graph = new RDFGraph();
        DataTable table = await graph.ToDataTableAsync();

        Assert.IsNotNull(table);
        Assert.AreEqual(3, table.Columns.Count);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.AreEqual(0, table.Rows.Count);
    }

    [TestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldImportFromFileAsync(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        await graph1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFileAsync{fileExtension}"));
        RDFGraph graph2 = await RDFGraph.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFileAsync{fileExtension}"));

        Assert.IsNotNull(graph2);
        Assert.AreEqual(2, graph2.TriplesCount);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.AreEqual(0, graph2[p: new RDFResource("http://ex/pred/")].TriplesCount);
            Assert.AreEqual(2, graph2[p: new RDFResource("http://ex/pred")].TriplesCount);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
    }

    [TestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldImportFromFileAsyncWithEnabledDatatypeDiscovery(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        graph1.AddDatatype(new RDFDatatype(new Uri($"ex:mydtK{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFPatternFacet("^ex$") ]));
        await graph1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFileAsync{fileExtension}WithEnabledDatatypeDiscovery"));
        RDFGraph graph2 = await RDFGraph.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFileAsync{fileExtension}WithEnabledDatatypeDiscovery"), true);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(9, graph2.TriplesCount);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.AreEqual(0, graph2[p: new RDFResource("http://ex/pred/")].TriplesCount);
            Assert.AreEqual(2, graph2[p: new RDFResource("http://ex/pred")].TriplesCount);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype($"ex:mydtK{(int)format}").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtK{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [TestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldImportEmptyFromFileAsync(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph1 = new RDFGraph();
        await graph1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportEmptyFromFileAsync{fileExtension}"));
        RDFGraph graph2 = await RDFGraph.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportEmptyFromFileAsync{fileExtension}"));

        Assert.IsNotNull(graph2);
        Assert.AreEqual(0, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullOrEmptyFilepathAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromUnexistingFilepathAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.NTriples, "blablabla"));

    [TestMethod]
    [DataRow(RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(RDFModelEnums.RDFFormats.TriX)]
    [DataRow(RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldImportFromStreamAsync(RDFModelEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        await graph1.ToStreamAsync(format, stream);
        RDFGraph graph2 = await RDFGraph.FromStreamAsync(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(graph2);
        Assert.AreEqual(2, graph2.TriplesCount);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.AreEqual(0, graph2[p: new RDFResource("http://ex/pred/")].TriplesCount);
            Assert.AreEqual(2, graph2[p: new RDFResource("http://ex/pred")].TriplesCount);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(RDFModelEnums.RDFFormats.TriX)]
    [DataRow(RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldImportFromStreamAsyncWithEnabledDatatypeDiscovery(RDFModelEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        graph1.AddDatatype(new RDFDatatype(new Uri($"ex:mydtKK{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFPatternFacet("^ex$") ]));
        await graph1.ToStreamAsync(format, stream);
        RDFGraph graph2 = await RDFGraph.FromStreamAsync(format, new MemoryStream(stream.ToArray()), true);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(9, graph2.TriplesCount);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.AreEqual(0, graph2[p: new RDFResource("http://ex/pred/")].TriplesCount);
            Assert.AreEqual(2, graph2[p: new RDFResource("http://ex/pred")].TriplesCount);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype($"ex:mydtKK{(int)format}").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtKK{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [TestMethod]
    [DataRow(RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(RDFModelEnums.RDFFormats.TriX)]
    [DataRow(RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldImportFromEmptyStreamAsync(RDFModelEnums.RDFFormats format)
    {
        MemoryStream stream = new MemoryStream();
        RDFGraph graph1 = new RDFGraph();
        await graph1.ToStreamAsync(format, stream);
        RDFGraph graph2 = await RDFGraph.FromStreamAsync(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(graph2);
        Assert.AreEqual(0, graph2.TriplesCount);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullStreamAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromStreamAsync(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public async Task ShouldImportFromDataTableAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        DataTable table = await graph1.ToDataTableAsync();
        RDFGraph graph2 = await RDFGraph.FromDataTableAsync(table);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(2, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public async Task ShouldImportFromDataTableAsyncWithEnabledDatatypeDiscovery()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph1.AddTriple(triple1).AddTriple(triple2);
        graph1.AddDatatype(new RDFDatatype(new Uri("ex:mydtR"), RDFModelEnums.RDFDatatypes.XSD_STRING, [ new RDFPatternFacet("^ex$") ]));
        DataTable table = await graph1.ToDataTableAsync();
        RDFGraph graph2 = await RDFGraph.FromDataTableAsync(table, true);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(9, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
        //Test that automatic datatype discovery happened successfully
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, RDFDatatypeRegister.GetDatatype("ex:mydtR").TargetDatatype);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype("ex:mydtR").Facets.Single() is RDFPatternFacet { Pattern: "^ex$" });
    }

    [TestMethod]
    public async Task ShouldImportEmptyFromDataTableAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        DataTable table = await graph1.ToDataTableAsync();
        RDFGraph graph2 = await RDFGraph.FromDataTableAsync(table);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(0, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullDataTableAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableNotHaving3ColumnsAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableNotHavingExactColumnsAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECTTTTT", typeof(string));

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullSubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add(null, "http://pred/", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptySubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("", "http://pred/", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralSubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("hello@en", "http://pred/", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", null, "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptyPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithBlankPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "bnode:12345", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "hello@en", "http://obj/");

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullObjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "http://pred/", null);

        await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldImportEmptyFromDataTableButGivingNameToGraphAsync()
    {
        RDFGraph graph1 = new RDFGraph().SetContext(new Uri("http://context/"));
        DataTable table = await graph1.ToDataTableAsync();
        RDFGraph graph2 = await RDFGraph.FromDataTableAsync(table);

        Assert.IsNotNull(graph2);
        Assert.AreEqual(0, graph2.TriplesCount);
        Assert.IsTrue(graph2.Equals(graph1));
        Assert.IsTrue(graph2.Context.Equals(new Uri("http://context/")));
    }

    [TestMethod]
    public async Task ShouldImportFromUriAsync()
    {
        RDFGraph graph = await RDFGraph.FromUriAsync(new Uri(RDFVocabulary.RDFS.BASE_URI));

        Assert.IsNotNull(graph);
        Assert.IsTrue(graph.Context.Equals(new Uri(RDFVocabulary.RDFS.BASE_URI)));
        Assert.IsGreaterThan(0, graph.TriplesCount);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullUriAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromUriAsync(null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromRelativeUriAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromUriAsync(new Uri("/file/system", UriKind.Relative)));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromUnreacheableUriAsync()
        => await Assert.ThrowsExactlyAsync<RDFModelException>(async () => await RDFGraph.FromUriAsync(new Uri("http://rdfsharp.test/"), 500));

    public TestContext TestContext { get; set; }
    #endregion
}