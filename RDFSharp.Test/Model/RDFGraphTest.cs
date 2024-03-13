/*
   Copyright 2012-2024 Marco De Salvo

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
using System.Linq;
using System;
using System.Collections.Generic;

namespace RDFSharp.Test.Model
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            { 
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
            });

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
        public void ShouldCreateGraphFromTriplesWithCompoundLiterals()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR)),
                new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR))
            });

            Assert.IsNotNull(graph);
            Assert.IsNotNull(graph.IndexedTriples);
            Assert.IsTrue(graph.TriplesCount == 6);
            Assert.IsNotNull(graph.GraphIndex);
            Assert.IsTrue(graph.GraphIndex.ResourcesRegister.Count == 9);
            Assert.IsTrue(graph.GraphIndex.LiteralsRegister.Count == 2);
            Assert.IsTrue(graph.GraphIndex.SubjectsIndex.Count == 3);
            Assert.IsTrue(graph.GraphIndex.PredicatesIndex.Count == 4);
            Assert.IsTrue(graph.GraphIndex.ObjectsIndex.Count == 2);
            Assert.IsTrue(graph.GraphIndex.LiteralsIndex.Count == 2);
            Assert.IsNotNull(graph.Context);
            Assert.IsTrue(graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public void ShouldDisposeGraphWithUsing()
        {
            RDFGraph graph;
            using (graph = new RDFGraph(new List<RDFTriple>() {
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit")) })) 
            {
                Assert.IsFalse(graph.Disposed);
                Assert.IsNotNull(graph.IndexedTriples);
                Assert.IsNotNull(graph.GraphIndex);
            };
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
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
            Assert.IsTrue(graph.Count(t => t.TripleMetadata is null) == 1);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsInference) == 0);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsImport) == 0);
        }

        [TestMethod]
        public void ShouldAddTripleWithCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("olleh", "en-US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL));
            graph.AddTriple(triple);

            Assert.IsTrue(graph.TriplesCount == 5);
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
            Assert.IsTrue(graph.Count(t => t.TripleMetadata is null) == 5);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsInference) == 0);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsImport) == 0);
        }

        [TestMethod]
        public void ShouldAddTripleWithCraftedCompoundLiterals()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("olleh", "en-US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("olleh", "en-US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR)); //Reification graph of this literal will not be added, since the same literal has already been reified (in RTL)
            graph.AddTriple(triple1);
            graph.AddTriple(triple2);

            Assert.IsTrue(graph.TriplesCount == 6);
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple1.TripleID));
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple2.TripleID));
            Assert.IsTrue(graph.Count(t => t.TripleMetadata is null) == 6);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsInference) == 0);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsImport) == 0);
        }

        [TestMethod]
        public void ShouldAddTripleAsInference()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))
                                    .SetMetadata(RDFModelEnums.RDFTripleMetadata.IsInference);
            graph.AddTriple(triple);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
            Assert.IsTrue(graph.Count(t => t.TripleMetadata is null) == 0);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsInference) == 1);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsImport) == 0);
        }

        [TestMethod]
        public void ShouldAddTripleAsImport()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))
                                    .SetMetadata(RDFModelEnums.RDFTripleMetadata.IsImport);
            graph.AddTriple(triple);

            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
            Assert.IsTrue(graph.Count(t => t.TripleMetadata is null) == 0);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsInference) == 0);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsImport) == 1);
        }

        [TestMethod]
        public void ShouldAddTriplesMixingMetadata()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit","en-US"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))
                                    .SetMetadata(RDFModelEnums.RDFTripleMetadata.IsInference);
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"))
                                    .SetMetadata(RDFModelEnums.RDFTripleMetadata.IsImport);
            graph.AddTriple(triple1);
            graph.AddTriple(triple2);
            graph.AddTriple(triple3);

            Assert.IsTrue(graph.TriplesCount == 3);
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple1.TripleID));
            Assert.IsTrue(graph.IndexedTriples[triple1.TripleID].TripleMetadata is null);
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple2.TripleID));
            Assert.IsTrue(graph.IndexedTriples[triple2.TripleID].TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsInference);
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple3.TripleID));
            Assert.IsTrue(graph.IndexedTriples[triple3.TripleID].TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsImport);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata is null) == 1);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsInference) == 1);
            Assert.IsTrue(graph.Count(t => t.TripleMetadata == RDFModelEnums.RDFTripleMetadata.IsImport) == 1);
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
        public void ShouldRemoveTriple()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            graph.AddTriple(triple);
            graph.RemoveTriple(triple);

            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldRemoveTriplesWithCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit","en-US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL));
            graph.AddTriple(triple1);
            graph.AddTriple(triple2);
            graph.AddTriple(triple3);
            graph.RemoveTriple(triple1);
            graph.RemoveTriple(triple2);
            graph.RemoveTriple(triple3);

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
        public void ShouldRemoveTriplesBySubjectHavingCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit","en-US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
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
        public void ShouldRemoveTriplesByPredicateHavingCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL));
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
        public void ShouldRemoveTriplesByLiteralHavingCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
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
        public void ShouldRemoveTriplesBySubjectPredicateHavingCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL));
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
        public void ShouldRemoveTriplesBySubjectLiteralHavingCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL));
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
        public void ShouldRemoveTriplesByPredicateLiteralHavingCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
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
        public void ShouldUnreifySPLTriplesHavingCompoundLiteral()
        {
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit","en-US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL));
            RDFGraph graph = triple.ReifyTriple();
            graph.UnreifyTriples();

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 5);
            Assert.IsTrue(graph.IndexedTriples.ContainsKey(triple.TripleID));
            Assert.IsTrue(graph[new RDFPlainLiteral("lit", "en-US").ReificationSubject,null,null,null].TriplesCount == 4);
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
        public void ShouldContainTripleHavingCompoundLiteral()
        {
            RDFGraph graph = new RDFGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
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
        public void ShouldIntersectGraphsHavingSameCompoundLiteral()
        {
            RDFGraph graph1 = new RDFGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
            graph1.AddTriple(triple1).AddTriple(triple2);
            RDFGraph graph2 = new RDFGraph();
            graph2.AddTriple(triple1);

            RDFGraph intersect12 = graph1.IntersectWith(graph2);
            Assert.IsNotNull(intersect12);
            Assert.IsTrue(intersect12.TriplesCount == 4); //3 triples for same compound literal reification
            Assert.IsTrue(intersect12.IndexedTriples.ContainsKey(triple1.TripleID));
            RDFGraph intersect21 = graph2.IntersectWith(graph1);
            Assert.IsNotNull(intersect21);
            Assert.IsTrue(intersect21.TriplesCount == 4); //3 triples for same compound literal reification
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
        public void ShouldUnionGraphsHavingCompoundLiterals()
        {
            RDFGraph graph1 = new RDFGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj3/"), new RDFResource("http://pred3/"), new RDFResource("http://obj3/"));
            graph1.AddTriple(triple1).AddTriple(triple2);
            RDFGraph graph2 = new RDFGraph();
            graph2.AddTriple(triple1).AddTriple(triple3);

            RDFGraph union12 = graph1.UnionWith(graph2);
            Assert.IsNotNull(union12);
            Assert.IsTrue(union12.TriplesCount == 6); //3 triples coming from reification of compound literal
            RDFGraph union21 = graph2.UnionWith(graph1);
            Assert.IsNotNull(union21);
            Assert.IsTrue(union21.TriplesCount == 6); //3 triples coming from reification of compound literal
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
        public void ShouldDifferenceGraphsHavingCompoundLiterals()
        {
            RDFGraph graph1 = new RDFGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR));
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
            graph1.AddTriple(triple1).AddTriple(triple2);
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
        public void ShouldRaiseExceptionOnImportingFromDataTableNotHaving3Columns()
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

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFGraphTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}