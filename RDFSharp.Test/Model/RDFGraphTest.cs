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
        #endregion
    }
}