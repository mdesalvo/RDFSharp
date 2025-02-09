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
        Assert.IsNotNull(graph.IndexedTriples);
        Assert.IsTrue(graph.TriplesCount == 0);
        Assert.IsNotNull(graph.GraphIndex);
        Assert.IsTrue(graph.GraphIndex.ResourcesRegister.Count == 0);
        Assert.IsTrue(graph.GraphIndex.LiteralsRegister.Count == 0);
        Assert.IsTrue(graph.GraphIndex.SubjectsIndex.Count == 0);
        Assert.IsTrue(graph.GraphIndex.PredicatesIndex.Count == 0);
        Assert.IsTrue(graph.GraphIndex.ObjectsIndex.Count == 0);
        Assert.IsTrue(graph.GraphIndex.LiteralsIndex.Count == 0);
        Assert.IsNotNull(graph.Context);
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));

        int i = 0;
        foreach (RDFTriple t in graph) i++;
        Assert.IsTrue(i == 0);

        int j = 0;
        IEnumerator<RDFTriple> triplesEnumerator = graph.TriplesEnumerator;
        while (triplesEnumerator.MoveNext()) j++;
        Assert.IsTrue(j == 0);
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
        Assert.IsNotNull(graph.IndexedTriples);
        Assert.IsTrue(graph.TriplesCount == 2);
        Assert.IsNotNull(graph.GraphIndex);
        Assert.IsTrue(graph.GraphIndex.ResourcesRegister.Count == 3);
        Assert.IsTrue(graph.GraphIndex.LiteralsRegister.Count == 1);
        Assert.IsTrue(graph.GraphIndex.SubjectsIndex.Count == 1);
        Assert.IsTrue(graph.GraphIndex.PredicatesIndex.Count == 1);
        Assert.IsTrue(graph.GraphIndex.ObjectsIndex.Count == 1);
        Assert.IsTrue(graph.GraphIndex.LiteralsIndex.Count == 1);
        Assert.IsNotNull(graph.Context);
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
    }

    [TestMethod]
    public void ShouldDisposeGraphWithUsing()
    {
        RDFGraph graph;
        using (graph = new RDFGraph([
                   new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                   new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit")) ])) 
        {
            Assert.IsFalse(graph.Disposed);
            Assert.IsNotNull(graph.IndexedTriples);
            Assert.IsNotNull(graph.GraphIndex);
        }
        Assert.IsTrue(graph.Disposed);
        Assert.IsNull(graph.IndexedTriples);
        Assert.IsNull(graph.GraphIndex);
    }

    [DataTestMethod]
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
        Assert.IsTrue(graph.ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
    }

    [TestMethod]
    public void ShouldGetCustomStringRepresentation()
    {
        RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/"));
        Assert.IsTrue(graph.ToString().Equals("http://example.org/"));
    }

    [TestMethod]
    public void ShouldEnumerateGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
            new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
        ]);

        int i = 0;
        foreach (RDFTriple t in graph) i++;
        Assert.IsTrue(i == 2);

        int j = 0;
        IEnumerator<RDFTriple> triplesEnumerator = graph.TriplesEnumerator;
        while(triplesEnumerator.MoveNext()) j++;
        Assert.IsTrue(j == 2);
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

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotAddDuplicateTriples()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple).AddTriple(triple);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotAddNullTriple()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(null);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldAddContainer()
    {
        RDFGraph graph = new RDFGraph();
        RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
        cont.AddItem(new RDFPlainLiteral("hello"));
        graph.AddContainer(cont);

        Assert.IsTrue(graph.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldAddEmptyContainer()
    {
        RDFGraph graph = new RDFGraph();
        RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
        graph.AddContainer(cont);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public void ShouldNotAddNullContainer()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddContainer(null);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldAddFilledCollection()
    {
        RDFGraph graph = new RDFGraph();
        RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
        coll.AddItem(new RDFPlainLiteral("hello"));
        graph.AddCollection(coll);

        Assert.IsTrue(graph.TriplesCount == 3);
    }

    [TestMethod]
    public void ShouldNotAddEmptyCollection()
    {
        RDFGraph graph = new RDFGraph();
        RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
        graph.AddCollection(coll);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotAddNullCollection()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddCollection(null);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldAddDatatype()
    {
        RDFGraph graph = new RDFGraph();
        RDFDatatype exlength6 = new RDFDatatype(new Uri("ex:exlength6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFLengthFacet(6), new RDFPatternFacet("^ex") ]);
        graph.AddDatatype(exlength6);

        Assert.IsTrue(graph.TriplesCount == 11);
    }

    [TestMethod]
    public void ShouldRemoveTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriple(triple);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveUnexistingTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"));
        graph.AddTriple(triple1);
        graph.RemoveTriple(triple2);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple1.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveNullTriple()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriple(null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesBySubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubject((RDFResource)triple.Subject);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectBecauseUnexisting()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubject(new RDFResource("http://subj2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectBecauseNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubject(null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicate((RDFResource)triple.Predicate);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateBecauseUnexisting()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicate(new RDFResource("http://pred2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateBecauseNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicate(null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByObject((RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByObjectBecauseUnexisting()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByObject(new RDFResource("http://obj2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByObjectBecauseNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByObject(null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en","US"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByLiteral((RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByLiteralBecauseUnexisting()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByLiteral(new RDFPlainLiteral("en"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByLiteralBecauseNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByLiteral(null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesBySubjectPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectPredicate((RDFResource)triple.Subject, (RDFResource)triple.Predicate);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectPredicate(new RDFResource("http://subj2/"), (RDFResource)triple.Predicate);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectPredicate((RDFResource)triple.Subject, new RDFResource("http://pred2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectPredicateBecauseNullSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectPredicate(null, (RDFResource)triple.Predicate);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectPredicateBecauseNullPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectPredicate((RDFResource)triple.Subject, null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesBySubjectObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectObject((RDFResource)triple.Subject, (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectObject(new RDFResource("http://subj2/"), (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectObject((RDFResource)triple.Subject, new RDFResource("http://obj2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectObjectBecauseNullSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectObject(null, (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectObjectBecauseNullObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectObject((RDFResource)triple.Subject, null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesBySubjectLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectLiteral((RDFResource)triple.Subject, (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectLiteral(new RDFResource("http://subj2/"), (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectLiteral((RDFResource)triple.Subject, new RDFPlainLiteral("lit2"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectLiteralBecauseNullSubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectLiteral(null, (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesBySubjectLiteralBecauseNullLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesBySubjectLiteral((RDFResource)triple.Subject, null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByPredicateObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateObject((RDFResource)triple.Predicate, (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateObjectBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateObject(new RDFResource("http://pred2/"), (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateObjectBecauseUnexistingObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateObject((RDFResource)triple.Predicate, new RDFResource("http://obj2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateObjectBecauseNullPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateObject(null, (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateObjectBecauseNullObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateObject((RDFResource)triple.Predicate, null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldRemoveTriplesByPredicateLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateLiteral((RDFResource)triple.Predicate, (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateLiteralBecauseUnexistingPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateLiteral(new RDFResource("http://pred2/"), (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateLiteralBecauseUnexistingLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateLiteral((RDFResource)triple.Predicate, new RDFPlainLiteral("lit2"));

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateLiteralBecauseNullPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateLiteral(null, (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldNotRemoveTriplesByPredicateLiteralBecauseNullLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.RemoveTriplesByPredicateLiteral((RDFResource)triple.Predicate, null);

        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldClearTriples()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        graph.AddTriple(triple);
        graph.ClearTriples();

        Assert.IsTrue(graph.TriplesCount == 0);
        Assert.IsTrue(graph.GraphIndex.SubjectsIndex.Count == 0);
        Assert.IsTrue(graph.GraphIndex.PredicatesIndex.Count == 0);
        Assert.IsTrue(graph.GraphIndex.ObjectsIndex.Count == 0);
        Assert.IsTrue(graph.GraphIndex.LiteralsIndex.Count == 0);
    }

    [TestMethod]
    public void ShouldUnreifySPOTriples()
    {
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFGraph graph = triple.ReifyTriple();
        graph.UnreifyTriples();

        Assert.IsNotNull(graph);
        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
    }

    [TestMethod]
    public void ShouldUnreifySPLTriples()
    {
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFGraph graph = triple.ReifyTriple();
        graph.UnreifyTriples();

        Assert.IsNotNull(graph);
        Assert.IsTrue(graph.TriplesCount == 1);
        Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
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
    public void ShouldSelectTriplesByNullAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[null, null, null, null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 3);
    }

    [TestMethod]
    public void ShouldSelectTriplesBySubjectAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[new RDFResource("http://subj/"), null, null, null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldSelectTriplesBySubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph.SelectTriplesBySubject(new RDFResource("http://subj/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldSelectTriplesBySubjectEvenIfNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph.SelectTriplesBySubject(null);
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesBySubjectAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph[new RDFResource("http://subj2/"), null, null, null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesBySubject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph.SelectTriplesBySubject(new RDFResource("http://subj2/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldSelectTriplesByPredicateAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[null, new RDFResource("http://pred/"), null, null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldSelectTriplesByPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph.SelectTriplesByPredicate(new RDFResource("http://pred/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldSelectTriplesByPredicateEvenIfNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph.SelectTriplesByPredicate(null);
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesByPredicateAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph[null, new RDFResource("http://pred2/"), null, null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesByPredicate()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph.SelectTriplesByPredicate(new RDFResource("http://pred2/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldSelectTriplesByObjectAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[null, null, new RDFResource("http://obj/"), null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldSelectTriplesByObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph.SelectTriplesByObject(new RDFResource("http://obj/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldSelectTriplesByObjectEvenIfNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph.SelectTriplesByObject(null);
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesByObjectAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph[null, null, new RDFResource("http://obj2/"), null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesByObject()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph.SelectTriplesByObject(new RDFResource("http://obj2/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldSelectTriplesByLiteralAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[null, null, null, new RDFPlainLiteral("lit")];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 1);
    }

    [TestMethod]
    public void ShouldSelectTriplesByLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph.SelectTriplesByLiteral(new RDFPlainLiteral("lit"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 1);
    }

    [TestMethod]
    public void ShouldSelectTriplesByLiteralEvenIfNull()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph.SelectTriplesByLiteral(null);
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesByLiteralAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph[null, null, null, new RDFPlainLiteral("lit", "en-US")];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesByLiteral()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);

        RDFGraph select = graph.SelectTriplesByLiteral(new RDFPlainLiteral("lit", "en-US"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldSelectTriplesByComplexAccessor1()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[new RDFResource("http://subj/"), null, null, new RDFPlainLiteral("lit")];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 1);
    }

    [TestMethod]
    public void ShouldSelectTriplesByComplexAccessor2()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[null, new RDFResource("http://pred/"), new RDFResource("http://obj/"), null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public void ShouldSelectTriplesByFullAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"), null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 1);
    }

    [TestMethod]
    public void ShouldNotSelectTriplesByFullAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        RDFGraph select = graph[new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj/"), null];
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnSelectingTriplesByIllecitAccessor()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2).AddTriple(triple3);

        Assert.ThrowsException<RDFModelException>(() => graph[null, null, new RDFResource("http://obj/"), new RDFPlainLiteral("lit")]);
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
        Assert.IsTrue(intersect12.TriplesCount == 1);
        Assert.IsTrue(intersect12.IndexedTriples.ContainsKey(triple1.TripleID));
        RDFGraph intersect21 = graph2.IntersectWith(graph1);
        Assert.IsNotNull(intersect21);
        Assert.IsTrue(intersect21.TriplesCount == 1);
        Assert.IsTrue(intersect21.IndexedTriples.ContainsKey(triple1.TripleID));
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
        Assert.IsTrue(intersect12.TriplesCount == 0);
        RDFGraph intersect21 = graph2.IntersectWith(graph1);
        Assert.IsNotNull(intersect21);
        Assert.IsTrue(intersect21.TriplesCount == 0);
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
        Assert.IsTrue(intersect12.TriplesCount == 0);
        RDFGraph intersect21 = graph2.IntersectWith(graph1);
        Assert.IsNotNull(intersect21);
        Assert.IsTrue(intersect21.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldIntersectEmptyWithEmpty()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFGraph graph2 = new RDFGraph();

        RDFGraph intersect12 = graph1.IntersectWith(graph2);
        Assert.IsNotNull(intersect12);
        Assert.IsTrue(intersect12.TriplesCount == 0);
        RDFGraph intersect21 = graph2.IntersectWith(graph1);
        Assert.IsNotNull(intersect21);
        Assert.IsTrue(intersect21.TriplesCount == 0);
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
        Assert.IsTrue(intersect12.TriplesCount == 0);
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
        Assert.IsTrue(intersect12.TriplesCount == 2);
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
        Assert.IsTrue(union12.TriplesCount == 3);
        RDFGraph union21 = graph2.UnionWith(graph1);
        Assert.IsNotNull(union21);
        Assert.IsTrue(union21.TriplesCount == 3);
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
        Assert.IsTrue(union12.TriplesCount == 2);
        RDFGraph union21 = graph2.UnionWith(graph1);
        Assert.IsNotNull(union21);
        Assert.IsTrue(union21.TriplesCount == 2);
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
        Assert.IsTrue(union12.TriplesCount == 2);
        RDFGraph union21 = graph2.UnionWith(graph1);
        Assert.IsNotNull(union21);
        Assert.IsTrue(union21.TriplesCount == 2);
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
        Assert.IsTrue(union12.TriplesCount == 2);
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
        Assert.IsTrue(union12.TriplesCount == 2);
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
        Assert.IsTrue(difference12.TriplesCount == 1);
        Assert.IsTrue(difference12.IndexedTriples.ContainsKey(triple2.TripleID));
        RDFGraph difference21 = graph2.DifferenceWith(graph1);
        Assert.IsNotNull(difference21);
        Assert.IsTrue(difference21.TriplesCount == 1);
        Assert.IsTrue(difference21.IndexedTriples.ContainsKey(triple3.TripleID));
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
        Assert.IsTrue(difference12.TriplesCount == 2);
        Assert.IsTrue(difference12.IndexedTriples.ContainsKey(triple2.TripleID));
        RDFGraph difference21 = graph2.DifferenceWith(graph1);
        Assert.IsNotNull(difference21);
        Assert.IsTrue(difference21.TriplesCount == 0);
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
        Assert.IsTrue(difference12.TriplesCount == 0);
        RDFGraph difference21 = graph2.DifferenceWith(graph1);
        Assert.IsNotNull(difference21);
        Assert.IsTrue(difference21.TriplesCount == 2);
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
        Assert.IsTrue(difference12.TriplesCount == 2);
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
        Assert.IsTrue(difference12.TriplesCount == 0);
    }

    [DataTestMethod]
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
        Assert.IsTrue(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFile{fileExtension}")).Length > 100);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnExportingToNullOrEmptyFilepath()
        => Assert.ThrowsException<RDFModelException>(() => new RDFGraph().ToFile(RDFModelEnums.RDFFormats.NTriples, null));

    [DataTestMethod]
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

        Assert.IsTrue(stream.ToArray().Length > 100);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnExportingToNullStream()
        => Assert.ThrowsException<RDFModelException>(() => new RDFGraph().ToStream(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public void ShouldExportToDataTable()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit","en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        graph.AddTriple(triple1).AddTriple(triple2);
        DataTable table = graph.ToDataTable();

        Assert.IsNotNull(table);
        Assert.IsTrue(table.Columns.Count == 3);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?SUBJECT"));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?PREDICATE"));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?OBJECT"));
        Assert.IsTrue(table.Rows.Count == 2);
        Assert.IsTrue(table.Rows[0]["?SUBJECT"].ToString().Equals("http://subj/"));
        Assert.IsTrue(table.Rows[0]["?PREDICATE"].ToString().Equals("http://pred/"));
        Assert.IsTrue(table.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
        Assert.IsTrue(table.Rows[1]["?SUBJECT"].ToString().Equals("http://subj/"));
        Assert.IsTrue(table.Rows[1]["?PREDICATE"].ToString().Equals("http://pred/"));
        Assert.IsTrue(table.Rows[1]["?OBJECT"].ToString().Equals("http://obj/"));
    }

    [TestMethod]
    public void ShouldExportEmptyToDataTable()
    {
        RDFGraph graph = new RDFGraph();
        DataTable table = graph.ToDataTable();

        Assert.IsNotNull(table);
        Assert.IsTrue(table.Columns.Count == 3);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?SUBJECT"));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?PREDICATE"));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?OBJECT"));
        Assert.IsTrue(table.Rows.Count == 0);
    }

    [DataTestMethod]
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
        Assert.IsTrue(graph2.TriplesCount == 2);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        { 
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.IsTrue(graph2.SelectTriplesByPredicate(new RDFResource("http://ex/pred/")).TriplesCount == 0);
            Assert.IsTrue(graph2.SelectTriplesByPredicate(new RDFResource("http://ex/pred")).TriplesCount == 2);
        }
        else
        { 
            Assert.IsTrue(graph2.Equals(graph1));
        }
    }

    [DataTestMethod]
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
            .AddDatatype(new RDFDatatype(new Uri($"ex:mydt{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
                new RDFPatternFacet("^ex$") ]));
        graph1.ToFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFile{fileExtension}WithEnabledDatatypeDiscovery"));
        RDFGraph graph2 = RDFGraph.FromFile(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFile{fileExtension}WithEnabledDatatypeDiscovery"), true);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 9);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.IsTrue(graph2.SelectTriplesByPredicate(new RDFResource("http://ex/pred/")).TriplesCount == 0);
            Assert.IsTrue(graph2.SelectTriplesByPredicate(new RDFResource("http://ex/pred")).TriplesCount == 2);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
        //Test that automatic datatype discovery happened successfully
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydt{(int)format}").TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydt{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [DataTestMethod]
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
        Assert.IsTrue(graph2.TriplesCount == 0);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullOrEmptyFilepath()
        => Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromFile(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromUnexistingFilepath()
        => Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromFile(RDFModelEnums.RDFFormats.NTriples, "blablabla"));

    [DataTestMethod]
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
        Assert.IsTrue(graph2.TriplesCount == 2);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.IsTrue(graph2.SelectTriplesByPredicate(new RDFResource("http://ex/pred/")).TriplesCount == 0);
            Assert.IsTrue(graph2.SelectTriplesByPredicate(new RDFResource("http://ex/pred")).TriplesCount == 2);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
    }

    [DataTestMethod]
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
            .AddDatatype(new RDFDatatype(new Uri($"ex:mydtT{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
                new RDFPatternFacet("^ex$") ]));
        graph1.ToStream(format, stream);
        RDFGraph graph2 = RDFGraph.FromStream(format, new MemoryStream(stream.ToArray()), true);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 9);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.IsTrue(graph2.SelectTriplesByPredicate(new RDFResource("http://ex/pred/")).TriplesCount == 0);
            Assert.IsTrue(graph2.SelectTriplesByPredicate(new RDFResource("http://ex/pred")).TriplesCount == 2);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
        //Test that automatic datatype discovery happened successfully
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtT{(int)format}").TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtT{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [DataTestMethod]
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
        Assert.IsTrue(graph2.TriplesCount == 0);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullStream()
        => Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromStream(RDFModelEnums.RDFFormats.NTriples, null));

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
        Assert.IsTrue(graph2.TriplesCount == 2);
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
            .AddDatatype(new RDFDatatype(new Uri("ex:mydtZ"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
                new RDFPatternFacet("^ex$") ]));
        DataTable table = graph1.ToDataTable();
        RDFGraph graph2 = RDFGraph.FromDataTable(table, true);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 9);
        Assert.IsTrue(graph2.Equals(graph1));
        //Test that automatic datatype discovery happened successfully
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype("ex:mydtZ").TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype("ex:mydtZ").Facets.Single() is RDFPatternFacet { Pattern: "^ex$" });
    }

    [TestMethod]
    public void ShouldImportEmptyFromDataTable()
    {
        RDFGraph graph1 = new RDFGraph();
        DataTable table = graph1.ToDataTable();
        RDFGraph graph2 = RDFGraph.FromDataTable(table);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 0);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullDataTable()
        => Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableNotHavingMandatoryColumns()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableNotHavingExactColumns()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECTTTTT", typeof(string));

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullSubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add(null, "http://pred/", "http://obj/");

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptySubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("", "http://pred/", "http://obj/");

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralSubject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("hello@en", "http://pred/", "http://obj/");

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", null, "http://obj/");

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptyPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "", "http://obj/");

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithBlankPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "bnode:12345", "http://obj/");

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralPredicate()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "hello@en", "http://obj/");

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullObject()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "http://pred/", null);

        Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromDataTable(table));
    }

    [TestMethod]
    public void ShouldImportEmptyFromDataTableButGivingNameToGraph()
    {
        RDFGraph graph1 = new RDFGraph().SetContext(new Uri("http://context/"));
        DataTable table = graph1.ToDataTable();
        RDFGraph graph2 = RDFGraph.FromDataTable(table);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 0);
        Assert.IsTrue(graph2.Equals(graph1));
        Assert.IsTrue(graph2.Context.Equals(new Uri("http://context/")));
    }

    [TestMethod]
    public void ShouldImportFromUri()
    {
        RDFGraph graph = RDFGraph.FromUri(new Uri(RDFVocabulary.RDFS.BASE_URI));

        Assert.IsNotNull(graph);
        Assert.IsTrue(graph.Context.Equals(new Uri(RDFVocabulary.RDFS.BASE_URI)));
        Assert.IsTrue(graph.TriplesCount > 0);
    }

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromNullUri()
        => Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromUri(null));

    [TestMethod]
    public void ShouldRaiseExceptionOnImportingFromRelativeUri()
        => Assert.ThrowsException<RDFModelException>(() => RDFGraph.FromUri(new Uri("/file/system", UriKind.Relative)));

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
        RDFDatatype exMaxInclusive6R = new RDFDatatype(new Uri("ex:maxinclusive6R"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, [new RDFMaxInclusiveFacet(6)]);
        RDFDatatype exMinInclusive6R = new RDFDatatype(new Uri("ex:mininclusive6R"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, [new RDFMinInclusiveFacet(6)]);
        RDFDatatype exMaxExclusive6R = new RDFDatatype(new Uri("ex:maxexclusive6R"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, [new RDFMaxExclusiveFacet(6)]);
        RDFDatatype exMinExclusive6R = new RDFDatatype(new Uri("ex:minexclusive6R"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, [new RDFMinExclusiveFacet(6)]);
        RDFDatatype aliasRational = new RDFDatatype(new Uri("ex:aliasRational"), RDFModelEnums.RDFDatatypes.OWL_RATIONAL, null);
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
            .AddDatatype(exMaxInclusive6R)
            .AddDatatype(exMinInclusive6R)
            .AddDatatype(exMaxExclusive6R)
            .AddDatatype(exMinExclusive6R)
            .AddDatatype(aliasRational)
            .AddDatatype(exPatternEx)
            .AddDatatype(exInteger);
        List<RDFDatatype> datatypes = graph.ExtractDatatypeDefinitions();

        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:length6")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING
                                          && dt.Facets.Single() is RDFLengthFacet { Length: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:minlength6")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING
                                          && dt.Facets.Single() is RDFMinLengthFacet { Length: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxlength6")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING
                                          && dt.Facets.Single() is RDFMaxLengthFacet { Length: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxinclusive6")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                                          && dt.Facets.Single() is RDFMaxInclusiveFacet { InclusiveUpperBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:mininclusive6")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                                          && dt.Facets.Single() is RDFMinInclusiveFacet { InclusiveLowerBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxexclusive6")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                                          && dt.Facets.Single() is RDFMaxExclusiveFacet { ExclusiveUpperBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:minexclusive6")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                                          && dt.Facets.Single() is RDFMinExclusiveFacet { ExclusiveLowerBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxinclusive6R")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Single() is RDFMaxInclusiveFacet { InclusiveUpperBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:mininclusive6R")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Single() is RDFMinInclusiveFacet { InclusiveLowerBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:maxexclusive6R")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Single() is RDFMaxExclusiveFacet { ExclusiveUpperBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:minexclusive6R")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Single() is RDFMinExclusiveFacet { ExclusiveLowerBound: 6 }));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:aliasRational")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                          && dt.Facets.Count == 0));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:patternex")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING
                                          && dt.Facets.Single() is RDFPatternFacet patternFacet
                                          && string.Equals(patternFacet.Pattern, "^ex")));
        Assert.IsTrue(datatypes.Any(dt => string.Equals(dt.URI.ToString(), "ex:integer")
                                          && dt.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_INTEGER
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
    [DataTestMethod]
    [DataRow("http://example.org/")]
    public async Task ShouldSetContextAsync(string input)
    {
        RDFGraph graph = await new RDFGraph().SetContextAsync(new Uri(input));
        Assert.IsTrue(graph.Context.Equals(new Uri(input)));
    }

    [TestMethod]
    public async Task ShouldNotSetContextBecauseNullUriAsync()
    {
        RDFGraph graph = await new RDFGraph().SetContextAsync(null);
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
    }

    [TestMethod]
    public async Task ShouldNotSetContextBecauseRelativeUriAsync()
    {
        RDFGraph graph = await new RDFGraph().SetContextAsync(new Uri("file/system", UriKind.Relative));
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
    }

    [TestMethod]
    public async Task ShouldNotSetContextBecauseBlankNodeUriAsync()
    {
        RDFGraph graph = await new RDFGraph().SetContextAsync(new Uri("bnode:12345"));
        Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
    }

    [TestMethod]
    public async Task ShouldGetCustomStringRepresentationAsync()
    {
        RDFGraph graph = await new RDFGraph().SetContextAsync(new Uri("http://example.org/"));
        Assert.IsTrue(graph.ToString().Equals("http://example.org/"));
    }

    [TestMethod]
    public async Task ShouldAddTripleAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotAddDuplicateTriplesAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple)).AddTripleAsync(triple);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotAddNullTripleAsync()
    {
        RDFGraph graph = new RDFGraph();
        await graph.AddTripleAsync(null);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldAddContainerAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
        cont.AddItem(new RDFPlainLiteral("hello"));
        await graph.AddContainerAsync(cont);

        Assert.IsTrue(graph.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldAddEmptyContainerAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
        await graph.AddContainerAsync(cont);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotAddNullContainerAsync()
    {
        RDFGraph graph = new RDFGraph();
        await graph.AddContainerAsync(null);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldAddFilledCollectionAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
        coll.AddItem(new RDFPlainLiteral("hello"));
        await graph.AddCollectionAsync(coll);

        Assert.IsTrue(graph.TriplesCount == 3);
    }

    [TestMethod]
    public async Task ShouldNotAddEmptyCollectionAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
        await graph.AddCollectionAsync(coll);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotAddNullCollectionAsync()
    {
        RDFGraph graph = new RDFGraph();
        await graph.AddCollectionAsync(null);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldAddDatatypeAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFDatatype exlength6 = new RDFDatatype(new Uri("ex:exlength6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFLengthFacet(6), new RDFPatternFacet("^ex") ]);
        await graph.AddDatatypeAsync(exlength6);

        Assert.IsTrue(graph.TriplesCount == 11);
    }

    [TestMethod]
    public async Task ShouldRemoveTripleAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTripleAsync(triple);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveUnexistingTripleAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"));
        await graph.AddTripleAsync(triple1);
        await graph.RemoveTripleAsync(triple2);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveNullTripleAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTripleAsync(null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesBySubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectAsync((RDFResource)triple.Subject);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectBecauseUnexistingAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectAsync(new RDFResource("http://subj2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectBecauseNullAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectAsync(null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesByPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateAsync((RDFResource)triple.Predicate);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateBecauseUnexistingAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateAsync(new RDFResource("http://pred2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateBecauseNullAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateAsync(null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesByObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByObjectAsync((RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByObjectBecauseUnexistingAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByObjectAsync(new RDFResource("http://obj2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByObjectBecauseNullAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByObjectAsync(null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesByLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByLiteralAsync((RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByLiteralBecauseUnexistingAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByLiteralAsync(new RDFPlainLiteral("en"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByLiteralBecauseNullAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByLiteralAsync(null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesBySubjectPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectPredicateAsync((RDFResource)triple.Subject, (RDFResource)triple.Predicate);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingSubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectPredicateAsync(new RDFResource("http://subj2/"), (RDFResource)triple.Predicate);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectPredicateAsync((RDFResource)triple.Subject, new RDFResource("http://pred2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectPredicateBecauseNullSubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectPredicateAsync(null, (RDFResource)triple.Predicate);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectPredicateBecauseNullPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectPredicateAsync((RDFResource)triple.Subject, null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesBySubjectObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectObjectAsync((RDFResource)triple.Subject, (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingSubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectObjectAsync(new RDFResource("http://subj2/"), (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectObjectAsync((RDFResource)triple.Subject, new RDFResource("http://obj2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectObjectBecauseNullSubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectObjectAsync(null, (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectObjectBecauseNullObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectObjectAsync((RDFResource)triple.Subject, null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesBySubjectLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectLiteralAsync((RDFResource)triple.Subject, (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingSubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectLiteralAsync(new RDFResource("http://subj2/"), (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectLiteralAsync((RDFResource)triple.Subject, new RDFPlainLiteral("lit2"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectLiteralBecauseNullSubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectLiteralAsync(null, (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesBySubjectLiteralBecauseNullLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesBySubjectLiteralAsync((RDFResource)triple.Subject, null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesByPredicateObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateObjectAsync((RDFResource)triple.Predicate, (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateObjectBecauseUnexistingPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateObjectAsync(new RDFResource("http://pred2/"), (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateObjectBecauseUnexistingObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateObjectAsync((RDFResource)triple.Predicate, new RDFResource("http://obj2/"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateObjectBecauseNullPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateObjectAsync(null, (RDFResource)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateObjectBecauseNullObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateObjectAsync((RDFResource)triple.Predicate, null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldRemoveTriplesByPredicateLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateLiteralAsync((RDFResource)triple.Predicate, (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateLiteralBecauseUnexistingPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateLiteralAsync(new RDFResource("http://pred2/"), (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateLiteralBecauseUnexistingLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateLiteralAsync((RDFResource)triple.Predicate, new RDFPlainLiteral("lit2"));

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateLiteralBecauseNullPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateLiteralAsync(null, (RDFLiteral)triple.Object);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldNotRemoveTriplesByPredicateLiteralBecauseNullLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.RemoveTriplesByPredicateLiteralAsync((RDFResource)triple.Predicate, null);

        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldClearTriplesAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);
        await graph.ClearTriplesAsync();

        Assert.IsTrue(graph.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldUnreifySPOTriplesAsync()
    {
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFGraph graph = await triple.ReifyTripleAsync();
        await graph.UnreifyTriplesAsync();

        Assert.IsNotNull(graph);
        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldUnreifySPLTriplesAsync()
    {
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFGraph graph = await triple.ReifyTripleAsync();
        await graph.UnreifyTriplesAsync();

        Assert.IsNotNull(graph);
        Assert.IsTrue(graph.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldContainTripleAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);

        Assert.IsTrue(await graph.ContainsTripleAsync(triple));
    }

    [TestMethod]
    public async Task ShouldNotContainTripleAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple1);

        Assert.IsFalse(await graph.ContainsTripleAsync(triple2));
    }

    [TestMethod]
    public async Task ShouldNotContainNullTripleAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        await graph.AddTripleAsync(triple);

        Assert.IsFalse(await graph.ContainsTripleAsync(null));
    }

    [TestMethod]
    public async Task ShouldSelectTriplesBySubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await (await graph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
            .AddTripleAsync(triple3);

        RDFGraph select = await graph.SelectTriplesBySubjectAsync(new RDFResource("http://subj/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldSelectTriplesBySubjectEvenIfNullAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph select = await graph.SelectTriplesBySubjectAsync(null);
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldNotSelectTriplesBySubjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph select = await graph.SelectTriplesBySubjectAsync(new RDFResource("http://subj2/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldSelectTriplesByPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj/"));
        await (await (await graph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
            .AddTripleAsync(triple3);

        RDFGraph select = await graph.SelectTriplesByPredicateAsync(new RDFResource("http://pred/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldSelectTriplesByPredicateEvenIfNullAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph select = await graph.SelectTriplesByPredicateAsync(null);
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldNotSelectTriplesByPredicateAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph select = await graph.SelectTriplesByPredicateAsync(new RDFResource("http://pred2/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldSelectTriplesByObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await (await graph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
            .AddTripleAsync(triple3);

        RDFGraph select = await graph.SelectTriplesByObjectAsync(new RDFResource("http://obj/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldSelectTriplesByObjectEvenIfNullAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph select = await graph.SelectTriplesByObjectAsync(null);
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldNotSelectTriplesByObjectAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph select = await graph.SelectTriplesByObjectAsync(new RDFResource("http://obj2/"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldSelectTriplesByLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await (await graph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
            .AddTripleAsync(triple3);

        RDFGraph select = await graph.SelectTriplesByLiteralAsync(new RDFPlainLiteral("lit"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldSelectTriplesByLiteralEvenIfNullAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph select = await graph.SelectTriplesByLiteralAsync(null);
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldNotSelectTriplesByLiteralAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph select = await graph.SelectTriplesByLiteralAsync(new RDFPlainLiteral("lit", "en-US"));
        Assert.IsNotNull(select);
        Assert.IsTrue(select.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldIntersectGraphsAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        RDFGraph graph2 = new RDFGraph();
        await graph2.AddTripleAsync(triple1);

        RDFGraph intersect12 = await graph1.IntersectWithAsync(graph2);
        Assert.IsNotNull(intersect12);
        Assert.IsTrue(intersect12.TriplesCount == 1);
        RDFGraph intersect21 = await graph2.IntersectWithAsync(graph1);
        Assert.IsNotNull(intersect21);
        Assert.IsTrue(intersect21.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldIntersectGraphWithEmptyAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        RDFGraph graph2 = new RDFGraph();

        RDFGraph intersect12 = await graph1.IntersectWithAsync(graph2);
        Assert.IsNotNull(intersect12);
        Assert.IsTrue(intersect12.TriplesCount == 0);
        RDFGraph intersect21 = await graph2.IntersectWithAsync(graph1);
        Assert.IsNotNull(intersect21);
        Assert.IsTrue(intersect21.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldIntersectEmptyWithGraphAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFGraph graph2 = new RDFGraph();
        await (await graph2.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph intersect12 = await graph1.IntersectWithAsync(graph2);
        Assert.IsNotNull(intersect12);
        Assert.IsTrue(intersect12.TriplesCount == 0);
        RDFGraph intersect21 = await graph2.IntersectWithAsync(graph1);
        Assert.IsNotNull(intersect21);
        Assert.IsTrue(intersect21.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldIntersectEmptyWithEmptyAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFGraph graph2 = new RDFGraph();

        RDFGraph intersect12 = await graph1.IntersectWithAsync(graph2);
        Assert.IsNotNull(intersect12);
        Assert.IsTrue(intersect12.TriplesCount == 0);
        RDFGraph intersect21 = await graph2.IntersectWithAsync(graph1);
        Assert.IsNotNull(intersect21);
        Assert.IsTrue(intersect21.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldIntersectGraphWithNullAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph intersect12 = await graph1.IntersectWithAsync(null);
        Assert.IsNotNull(intersect12);
        Assert.IsTrue(intersect12.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldIntersectGraphWithSelfAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph intersect12 = await graph1.IntersectWithAsync(graph1);
        Assert.IsNotNull(intersect12);
        Assert.IsTrue(intersect12.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldUnionGraphsAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj3/"), new RDFResource("http://pred3/"), new RDFResource("http://obj3/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        RDFGraph graph2 = new RDFGraph();
        await (await graph2.AddTripleAsync(triple1)).AddTripleAsync(triple3);

        RDFGraph union12 = await graph1.UnionWithAsync(graph2);
        Assert.IsNotNull(union12);
        Assert.IsTrue(union12.TriplesCount == 3);
        RDFGraph union21 = await graph2.UnionWithAsync(graph1);
        Assert.IsNotNull(union21);
        Assert.IsTrue(union21.TriplesCount == 3);
    }

    [TestMethod]
    public async Task ShouldUnionGraphWithEmptyAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        RDFGraph graph2 = new RDFGraph();

        RDFGraph union12 = await graph1.UnionWithAsync(graph2);
        Assert.IsNotNull(union12);
        Assert.IsTrue(union12.TriplesCount == 2);
        RDFGraph union21 = await graph2.UnionWithAsync(graph1);
        Assert.IsNotNull(union21);
        Assert.IsTrue(union21.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldUnionEmptyWithGraphAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFGraph graph2 = new RDFGraph();
        await (await graph2.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph union12 = await graph1.UnionWithAsync(graph2);
        Assert.IsNotNull(union12);
        Assert.IsTrue(union12.TriplesCount == 2);
        RDFGraph union21 = await graph2.UnionWithAsync(graph1);
        Assert.IsNotNull(union21);
        Assert.IsTrue(union21.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldUnionGraphWithNullAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph union12 = await graph1.UnionWithAsync(null);
        Assert.IsNotNull(union12);
        Assert.IsTrue(union12.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldUnionGraphWithSelfAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph union12 = await graph1.UnionWithAsync(graph1);
        Assert.IsNotNull(union12);
        Assert.IsTrue(union12.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldDifferenceGraphsAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj3/"), new RDFResource("http://pred3/"), new RDFResource("http://obj3/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        RDFGraph graph2 = new RDFGraph();
        await (await graph2.AddTripleAsync(triple1)).AddTripleAsync(triple3);

        RDFGraph difference12 = await graph1.DifferenceWithAsync(graph2);
        Assert.IsNotNull(difference12);
        Assert.IsTrue(difference12.TriplesCount == 1);
        RDFGraph difference21 = await graph2.DifferenceWithAsync(graph1);
        Assert.IsNotNull(difference21);
        Assert.IsTrue(difference21.TriplesCount == 1);
    }

    [TestMethod]
    public async Task ShouldDifferenceGraphWithEmptyAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        RDFGraph graph2 = new RDFGraph();

        RDFGraph difference12 = await graph1.DifferenceWithAsync(graph2);
        Assert.IsNotNull(difference12);
        Assert.IsTrue(difference12.TriplesCount == 2);
        RDFGraph difference21 = await graph2.DifferenceWithAsync(graph1);
        Assert.IsNotNull(difference21);
        Assert.IsTrue(difference21.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldDifferenceEmptyWithGraphAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        RDFGraph graph2 = new RDFGraph();
        await (await graph2.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph difference12 = await graph1.DifferenceWithAsync(graph2);
        Assert.IsNotNull(difference12);
        Assert.IsTrue(difference12.TriplesCount == 0);
        RDFGraph difference21 = await graph2.DifferenceWithAsync(graph1);
        Assert.IsNotNull(difference21);
        Assert.IsTrue(difference21.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldDifferenceGraphWithNullAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph difference12 = await graph1.DifferenceWithAsync(null);
        Assert.IsNotNull(difference12);
        Assert.IsTrue(difference12.TriplesCount == 2);
    }

    [TestMethod]
    public async Task ShouldDifferenceGraphWithSelfAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);

        RDFGraph difference12 = await graph1.DifferenceWithAsync(graph1);
        Assert.IsNotNull(difference12);
        Assert.IsTrue(difference12.TriplesCount == 0);
    }

    [DataTestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldExportToFileAsync(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        await graph.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFileAsync{fileExtension}"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFileAsync{fileExtension}")));
        Assert.IsTrue(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldExportToFileAsync{fileExtension}")).Length > 100);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnExportingToNullOrEmptyFilepathAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await new RDFGraph().ToFileAsync(RDFModelEnums.RDFFormats.NTriples, null));

    [DataTestMethod]
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
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        await graph.ToStreamAsync(format, stream);

        Assert.IsTrue(stream.ToArray().Length > 100);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnExportingToNullStreamAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await new RDFGraph().ToStreamAsync(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public async Task ShouldExportToDataTableAsync()
    {
        RDFGraph graph = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        DataTable table = await graph.ToDataTableAsync();

        Assert.IsNotNull(table);
        Assert.IsTrue(table.Columns.Count == 3);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?SUBJECT"));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?PREDICATE"));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?OBJECT"));
        Assert.IsTrue(table.Rows.Count == 2);
        Assert.IsTrue(table.Rows[0]["?SUBJECT"].ToString().Equals("http://subj/"));
        Assert.IsTrue(table.Rows[0]["?PREDICATE"].ToString().Equals("http://pred/"));
        Assert.IsTrue(table.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
        Assert.IsTrue(table.Rows[1]["?SUBJECT"].ToString().Equals("http://subj/"));
        Assert.IsTrue(table.Rows[1]["?PREDICATE"].ToString().Equals("http://pred/"));
        Assert.IsTrue(table.Rows[1]["?OBJECT"].ToString().Equals("http://obj/"));
    }

    [TestMethod]
    public async Task ShouldExportEmptyToDataTableAsync()
    {
        RDFGraph graph = new RDFGraph();
        DataTable table = await graph.ToDataTableAsync();

        Assert.IsNotNull(table);
        Assert.IsTrue(table.Columns.Count == 3);
        Assert.IsTrue(table.Columns[0].ColumnName.Equals("?SUBJECT"));
        Assert.IsTrue(table.Columns[1].ColumnName.Equals("?PREDICATE"));
        Assert.IsTrue(table.Columns[2].ColumnName.Equals("?OBJECT"));
        Assert.IsTrue(table.Rows.Count == 0);
    }

    [DataTestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldImportFromFileAsync(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        await graph1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFileAsync{fileExtension}"));
        RDFGraph graph2 = await RDFGraph.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFileAsync{fileExtension}"));

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 2);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.IsTrue((await graph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred/"))).TriplesCount == 0);
            Assert.IsTrue((await graph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred"))).TriplesCount == 2);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
    }

    [DataTestMethod]
    [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
    [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
    [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
    [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
    public async Task ShouldImportFromFileAsyncWithEnabledDatatypeDiscovery(string fileExtension, RDFModelEnums.RDFFormats format)
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        await graph1.AddDatatypeAsync(new RDFDatatype(new Uri($"ex:mydtK{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFPatternFacet("^ex$") ]));
        await graph1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFileAsync{fileExtension}WithEnabledDatatypeDiscovery"));
        RDFGraph graph2 = await RDFGraph.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFGraphTest_ShouldImportFromFileAsync{fileExtension}WithEnabledDatatypeDiscovery"), true);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 9);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.IsTrue((await graph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred/"))).TriplesCount == 0);
            Assert.IsTrue((await graph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred"))).TriplesCount == 2);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
        //Test that automatic datatype discovery happened successfully
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtK{(int)format}").TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtK{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [DataTestMethod]
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
        Assert.IsTrue(graph2.TriplesCount == 0);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullOrEmptyFilepathAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromUnexistingFilepathAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.NTriples, "blablabla"));

    [DataTestMethod]
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
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        await graph1.ToStreamAsync(format, stream);
        RDFGraph graph2 = await RDFGraph.FromStreamAsync(format, new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 2);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.IsTrue((await graph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred/"))).TriplesCount == 0);
            Assert.IsTrue((await graph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred"))).TriplesCount == 2);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
    }

    [DataTestMethod]
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
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        await graph1.AddDatatypeAsync(new RDFDatatype(new Uri($"ex:mydtKK{(int)format}"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFPatternFacet("^ex$") ]));
        await graph1.ToStreamAsync(format, stream);
        RDFGraph graph2 = await RDFGraph.FromStreamAsync(format, new MemoryStream(stream.ToArray()), true);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 9);
        //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
        //triples with a predicate ending with "/" will loose this character once abbreviated:
        //this is correct (being a glitch of RDF/XML specs) so at the end the graphs will differ
        if (format == RDFModelEnums.RDFFormats.RdfXml)
        {
            Assert.IsFalse(graph2.Equals(graph1));
            Assert.IsTrue((await graph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred/"))).TriplesCount == 0);
            Assert.IsTrue((await graph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred"))).TriplesCount == 2);
        }
        else
        {
            Assert.IsTrue(graph2.Equals(graph1));
        }
        //Test that automatic datatype discovery happened successfully
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtKK{(int)format}").TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype($"ex:mydtKK{(int)format}").Facets.Single() is RDFPatternFacet
        {
            Pattern: "^ex$"
        });
    }

    [DataTestMethod]
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
        Assert.IsTrue(graph2.TriplesCount == 0);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullStreamAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromStreamAsync(RDFModelEnums.RDFFormats.NTriples, null));

    [TestMethod]
    public async Task ShouldImportFromDataTableAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        DataTable table = await graph1.ToDataTableAsync();
        RDFGraph graph2 = await RDFGraph.FromDataTableAsync(table);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 2);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public async Task ShouldImportFromDataTableAsyncWithEnabledDatatypeDiscovery()
    {
        RDFGraph graph1 = new RDFGraph();
        RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
        RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
        await (await graph1.AddTripleAsync(triple1)).AddTripleAsync(triple2);
        await graph1.AddDatatypeAsync(new RDFDatatype(new Uri("ex:mydtR"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFPatternFacet("^ex$") ]));
        DataTable table = await graph1.ToDataTableAsync();
        RDFGraph graph2 = await RDFGraph.FromDataTableAsync(table, true);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 9);
        Assert.IsTrue(graph2.Equals(graph1));
        //Test that automatic datatype discovery happened successfully
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype("ex:mydtR").TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_STRING);
        Assert.IsTrue(RDFDatatypeRegister.GetDatatype("ex:mydtR").Facets.Single() is RDFPatternFacet { Pattern: "^ex$" });
    }

    [TestMethod]
    public async Task ShouldImportEmptyFromDataTableAsync()
    {
        RDFGraph graph1 = new RDFGraph();
        DataTable table = await graph1.ToDataTableAsync();
        RDFGraph graph2 = await RDFGraph.FromDataTableAsync(table);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 0);
        Assert.IsTrue(graph2.Equals(graph1));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullDataTableAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableNotHaving3ColumnsAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableNotHavingExactColumnsAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECTTTTT", typeof(string));

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullSubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add(null, "http://pred/", "http://obj/");

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptySubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("", "http://pred/", "http://obj/");

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralSubjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("hello@en", "http://pred/", "http://obj/");

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", null, "http://obj/");

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptyPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "", "http://obj/");

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithBlankPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "bnode:12345", "http://obj/");

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralPredicateAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "hello@en", "http://obj/");

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullObjectAsync()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?SUBJECT", typeof(string));
        table.Columns.Add("?PREDICATE", typeof(string));
        table.Columns.Add("?OBJECT", typeof(string));
        table.Rows.Add("http://subj/", "http://pred/", null);

        await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromDataTableAsync(table));
    }

    [TestMethod]
    public async Task ShouldImportEmptyFromDataTableButGivingNameTographAsync()
    {
        RDFGraph graph1 = await new RDFGraph().SetContextAsync(new Uri("http://context/"));
        DataTable table = await graph1.ToDataTableAsync();
        RDFGraph graph2 = await RDFGraph.FromDataTableAsync(table);

        Assert.IsNotNull(graph2);
        Assert.IsTrue(graph2.TriplesCount == 0);
        Assert.IsTrue(graph2.Equals(graph1));
        Assert.IsTrue(graph2.Context.Equals(new Uri("http://context/")));
    }

    [TestMethod]
    public async Task ShouldImportFromUriAsync()
    {
        RDFGraph graph = await RDFGraph.FromUriAsync(new Uri(RDFVocabulary.RDFS.BASE_URI));

        Assert.IsNotNull(graph);
        Assert.IsTrue(graph.Context.Equals(new Uri(RDFVocabulary.RDFS.BASE_URI)));
        Assert.IsTrue(graph.TriplesCount > 0);
    }

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromNullUriAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromUriAsync(null));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromRelativeUriAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromUriAsync(new Uri("/file/system", UriKind.Relative)));

    [TestMethod]
    public async Task ShouldRaiseExceptionOnImportingFromUnreacheableUriAsync()
        => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFGraph.FromUriAsync(new Uri("http://rdfsharp.test/")));
    #endregion
}