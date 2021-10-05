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
using System.Collections.Generic;
using System;

namespace RDFSharp.Test
{
    [TestClass]
    public class RDFGraphTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateEmptyGraph()
        {
            RDFGraph graph = new RDFGraph();

            Assert.IsNotNull(graph);
            Assert.IsNotNull(graph.Triples);
            Assert.IsTrue(graph.TriplesCount == 0);
            Assert.IsNotNull(graph.GraphIndex);
            Assert.IsTrue(graph.GraphIndex.Subjects.Count == 0);
            Assert.IsTrue(graph.GraphIndex.Predicates.Count == 0);
            Assert.IsTrue(graph.GraphIndex.Objects.Count == 0);
            Assert.IsTrue(graph.GraphIndex.Literals.Count == 0);
            Assert.IsNotNull(graph.Context);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public void ShouldCreateGraphFromTriples()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            { 
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
            });

            Assert.IsNotNull(graph);
            Assert.IsNotNull(graph.Triples);
            Assert.IsTrue(graph.TriplesCount == 2);
            Assert.IsNotNull(graph.GraphIndex);
            Assert.IsTrue(graph.GraphIndex.Subjects.Count == 1);
            Assert.IsTrue(graph.GraphIndex.Predicates.Count == 1);
            Assert.IsTrue(graph.GraphIndex.Objects.Count == 1);
            Assert.IsTrue(graph.GraphIndex.Literals.Count == 1);
            Assert.IsNotNull(graph.Context);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
            });

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
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotAddDuplicateTriples()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple).AddTriple(triple);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
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
            Assert.IsTrue(graph.Triples.ContainsKey(triple1.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveNullTriple()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriple(null);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
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
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectBecauseNull()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubject(null);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
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
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesByPredicateBecauseNull()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesByPredicate(null);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
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
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesByObjectBecauseNull()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesByObject(null);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
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
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesByLiteralBecauseNull()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
            graph.AddTriple(triple);
            graph.RemoveTriplesByLiteral(null);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
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
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingPredicate()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectPredicate((RDFResource)triple.Subject, new RDFResource("http://pred2/"));

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectPredicateBecauseNullSubject()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectPredicate(null, (RDFResource)triple.Predicate);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectPredicateBecauseNullPredicate()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectPredicate((RDFResource)triple.Subject, null);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
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
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingObject()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectObject((RDFResource)triple.Subject, new RDFResource("http://obj2/"));

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectObjectBecauseNullSubject()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectObject(null, (RDFResource)triple.Object);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectObjectBecauseNullObject()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectObject((RDFResource)triple.Subject, null);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
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
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectLiteral((RDFResource)triple.Subject, new RDFPlainLiteral("lit2"));

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectLiteralBecauseNullSubject()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectLiteral(null, (RDFLiteral)triple.Object);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }

        [TestMethod]
        public void ShouldNotRemoveTriplesBySubjectLiteralBecauseNullLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            graph.AddTriple(triple);
            graph.RemoveTriplesBySubjectLiteral((RDFResource)triple.Subject, null);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Triples.ContainsKey(triple.TripleID));
        }
        #endregion
    }
}