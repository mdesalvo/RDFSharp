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
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using Wmhelp.XPath2.yyParser;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFAsyncGraphTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateEmptyAsyncGraph()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();

            Assert.IsNotNull(asyncGraph);
            Assert.IsNotNull(asyncGraph.WrappedGraph);
            Assert.IsNotNull(asyncGraph.Context);
            Assert.IsTrue(asyncGraph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));

            int i = 0;
            foreach (RDFTriple t in asyncGraph) i++;
            Assert.IsTrue(i == 0);

            int j = 0;
            IEnumerator<RDFTriple> triplesEnumerator = asyncGraph.TriplesEnumerator;
            while (triplesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 0);
        }

        [TestMethod]
        public void ShouldCreateAsyncGraphFromTriples()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph(new List<RDFTriple>()
            { 
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
            });

            Assert.IsNotNull(asyncGraph);
            Assert.IsNotNull(asyncGraph.WrappedGraph);
            Assert.IsTrue(asyncGraph.TriplesCount == 2);
            Assert.IsNotNull(asyncGraph.Context);
            Assert.IsTrue(asyncGraph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public void ShouldCreateAsyncGraphFromGraph()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph(new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
            }));

            Assert.IsNotNull(asyncGraph);
            Assert.IsNotNull(asyncGraph.WrappedGraph);
            Assert.IsTrue(asyncGraph.TriplesCount == 2);
            Assert.IsNotNull(asyncGraph.Context);
            Assert.IsTrue(asyncGraph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public void ShouldDisposeAsyncGraphWithUsing()
        {
            RDFAsyncGraph asyncGraph;
            using (asyncGraph = new RDFAsyncGraph(new List<RDFTriple>() {
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit")) })) 
            {
                Assert.IsFalse(asyncGraph.Disposed);
                Assert.IsNotNull(asyncGraph.WrappedGraph);
            };
            Assert.IsTrue(asyncGraph.Disposed);
            Assert.IsNull(asyncGraph.WrappedGraph);
        }

        [DataTestMethod]
        [DataRow("http://example.org/")]
        public async Task ShouldSetContextAsync(string input)
        {
            RDFAsyncGraph asyncGraph = await new RDFAsyncGraph().SetContextAsync(new Uri(input));
            Assert.IsTrue(asyncGraph.Context.Equals(new Uri(input)));
        }

        [TestMethod]
        public async Task ShouldNotSetContextBecauseNullUri()
        {
            RDFAsyncGraph asyncGraph = await new RDFAsyncGraph().SetContextAsync(null);
            Assert.IsTrue(asyncGraph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public async Task ShouldNotSetContextBecauseRelativeUri()
        {
            RDFAsyncGraph asyncGraph = await new RDFAsyncGraph().SetContextAsync(new Uri("file/system", UriKind.Relative));
            Assert.IsTrue(asyncGraph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public async Task ShouldNotSetContextBecauseBlankNodeUri()
        {
            RDFAsyncGraph asyncGraph = await new RDFAsyncGraph().SetContextAsync(new Uri("bnode:12345"));
            Assert.IsTrue(asyncGraph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri));
        }

        [TestMethod]
        public void ShouldGetDefaultStringRepresentation()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            Assert.IsTrue(asyncGraph.ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
        }

        [TestMethod]
        public async Task ShouldGetCustomStringRepresentation()
        {
            RDFAsyncGraph asyncGraph = await new RDFAsyncGraph().SetContextAsync(new Uri("http://example.org/"));
            Assert.IsTrue(asyncGraph.ToString().Equals("http://example.org/"));
        }

        [TestMethod]
        public void ShouldEnumerateAsyncGraph()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFResource("http://obj/")),
                new RDFTriple(new RDFResource("http://subj/"),new RDFResource("http://pred/"),new RDFPlainLiteral("lit"))
            });

            int i = 0;
            foreach (RDFTriple t in asyncGraph) i++;
            Assert.IsTrue(i == 2);

            int j = 0;
            IEnumerator<RDFTriple> triplesEnumerator = asyncGraph.TriplesEnumerator;
            while(triplesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 2);
        }

        [TestMethod]
        public void ShouldCompareEquallyToEmptyAsyncGraph()
        {
            RDFAsyncGraph AsyncGraphA = new RDFAsyncGraph();
            RDFAsyncGraph AsyncGraphB = new RDFAsyncGraph();

            Assert.IsTrue(AsyncGraphA.Equals(AsyncGraphB));
        }

        [TestMethod]
        public async Task ShouldCompareEquallyToAsyncGraph()
        {
            RDFAsyncGraph AsyncGraphA = await new RDFAsyncGraph().AddTripleAsync(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFAsyncGraph AsyncGraphB = await new RDFAsyncGraph().AddTripleAsync(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));

            Assert.IsTrue(AsyncGraphA.Equals(AsyncGraphB));
        }

        [TestMethod]
        public async Task ShouldCompareEquallyToSelf()
        {
            RDFAsyncGraph AsyncGraphA = await new RDFAsyncGraph().AddTripleAsync(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));

            Assert.IsTrue(AsyncGraphA.Equals(AsyncGraphA));
        }

        [TestMethod]
        public void ShouldNotCompareEquallyToNullAsyncGraph()
        {
            RDFAsyncGraph asyncGraphA = new RDFAsyncGraph();

            Assert.IsFalse(asyncGraphA.Equals(null));
        }

        [TestMethod]
        public async Task ShouldNotCompareEquallyToMoreNumerousAsyncGraph()
        {
            RDFAsyncGraph asyncGraphA = await new RDFAsyncGraph().AddTripleAsync(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFAsyncGraph asyncGraphB = await (await new RDFAsyncGraph().AddTripleAsync(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))))
                                          .AddTripleAsync(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));

            Assert.IsFalse(asyncGraphA.Equals(asyncGraphB));
        }

        [TestMethod]
        public async Task ShouldNotCompareEquallyToDifferentAsyncGraph()
        {
            RDFAsyncGraph AsyncGraphA = await new RDFAsyncGraph().AddTripleAsync(new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
            RDFAsyncGraph AsyncGraphB = await new RDFAsyncGraph().AddTripleAsync(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));

            Assert.IsFalse(AsyncGraphA.Equals(AsyncGraphB));
        }

        [TestMethod]
        public async Task ShouldAddTriple()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotAddDuplicateTriples()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple)).AddTripleAsync(triple);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotAddNullTriple()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            await asyncGraph.AddTripleAsync(null);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldAddContainer()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
            cont.AddItem(new RDFPlainLiteral("hello"));
            await asyncGraph.AddContainerAsync(cont);

            Assert.IsTrue(asyncGraph.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldAddEmptyContainer()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFContainer cont = new RDFContainer(RDFModelEnums.RDFContainerTypes.Alt, RDFModelEnums.RDFItemTypes.Literal);
            await asyncGraph.AddContainerAsync(cont);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotAddNullContainer()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            await asyncGraph.AddContainerAsync(null);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldAddFilledCollection()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
            coll.AddItem(new RDFPlainLiteral("hello"));
            await asyncGraph.AddCollectionAsync(coll);

            Assert.IsTrue(asyncGraph.TriplesCount == 3);
        }

        [TestMethod]
        public async Task ShouldNotAddEmptyCollection()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFCollection coll = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
            await asyncGraph.AddCollectionAsync(coll);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotAddNullCollection()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            await asyncGraph.AddCollectionAsync(null);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldRemoveTriple()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTripleAsync(triple);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveUnexistingTriple()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"));
            await asyncGraph.AddTripleAsync(triple1);
            await asyncGraph.RemoveTripleAsync(triple2);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveNullTriple()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTripleAsync(null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesBySubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectAsync((RDFResource)triple.Subject);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectBecauseUnexisting()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectAsync(new RDFResource("http://subj2/"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectBecauseNull()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectAsync(null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesByPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateAsync((RDFResource)triple.Predicate);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateBecauseUnexisting()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateAsync(new RDFResource("http://pred2/"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateBecauseNull()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateAsync(null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesByObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByObjectAsync((RDFResource)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByObjectBecauseUnexisting()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByObjectAsync(new RDFResource("http://obj2/"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByObjectBecauseNull()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByObjectAsync(null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesByLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en","US"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByLiteralAsync((RDFLiteral)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByLiteralBecauseUnexisting()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByLiteralAsync(new RDFPlainLiteral("en"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByLiteralBecauseNull()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("en", "US"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByLiteralAsync(null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesBySubjectPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectPredicateAsync((RDFResource)triple.Subject, (RDFResource)triple.Predicate);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingSubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectPredicateAsync(new RDFResource("http://subj2/"), (RDFResource)triple.Predicate);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectPredicateBecauseUnexistingPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectPredicateAsync((RDFResource)triple.Subject, new RDFResource("http://pred2/"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectPredicateBecauseNullSubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectPredicateAsync(null, (RDFResource)triple.Predicate);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectPredicateBecauseNullPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectPredicateAsync((RDFResource)triple.Subject, null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesBySubjectObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectObjectAsync((RDFResource)triple.Subject, (RDFResource)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingSubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectObjectAsync(new RDFResource("http://subj2/"), (RDFResource)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectObjectBecauseUnexistingObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectObjectAsync((RDFResource)triple.Subject, new RDFResource("http://obj2/"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectObjectBecauseNullSubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectObjectAsync(null, (RDFResource)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectObjectBecauseNullObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectObjectAsync((RDFResource)triple.Subject, null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesBySubjectLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectLiteralAsync((RDFResource)triple.Subject, (RDFLiteral)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingSubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectLiteralAsync(new RDFResource("http://subj2/"), (RDFLiteral)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectLiteralBecauseUnexistingLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectLiteralAsync((RDFResource)triple.Subject, new RDFPlainLiteral("lit2"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectLiteralBecauseNullSubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectLiteralAsync(null, (RDFLiteral)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesBySubjectLiteralBecauseNullLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesBySubjectLiteralAsync((RDFResource)triple.Subject, null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesByPredicateObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateObjectAsync((RDFResource)triple.Predicate, (RDFResource)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateObjectBecauseUnexistingPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateObjectAsync(new RDFResource("http://pred2/"), (RDFResource)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateObjectBecauseUnexistingObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateObjectAsync((RDFResource)triple.Predicate, new RDFResource("http://obj2/"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateObjectBecauseNullPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateObjectAsync(null, (RDFResource)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateObjectBecauseNullObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateObjectAsync((RDFResource)triple.Predicate, null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldRemoveTriplesByPredicateLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateLiteralAsync((RDFResource)triple.Predicate, (RDFLiteral)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateLiteralBecauseUnexistingPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateLiteralAsync(new RDFResource("http://pred2/"), (RDFLiteral)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateLiteralBecauseUnexistingLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateLiteralAsync((RDFResource)triple.Predicate, new RDFPlainLiteral("lit2"));

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateLiteralBecauseNullPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateLiteralAsync(null, (RDFLiteral)triple.Object);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotRemoveTriplesByPredicateLiteralBecauseNullLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.RemoveTriplesByPredicateLiteralAsync((RDFResource)triple.Predicate, null);

            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldClearTriples()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);
            await asyncGraph.ClearTriplesAsync();

            Assert.IsTrue(asyncGraph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldUnreifySPOTriples()
        {
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFAsyncGraph asyncGraph = await triple.ReifyTripleAsync();
            await asyncGraph.UnreifyTriplesAsync();

            Assert.IsNotNull(asyncGraph);
            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldUnreifySPLTriples()
        {
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFAsyncGraph asyncGraph = await triple.ReifyTripleAsync();
            await asyncGraph.UnreifyTriplesAsync();

            Assert.IsNotNull(asyncGraph);
            Assert.IsTrue(asyncGraph.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldContainTriple()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);

            Assert.IsTrue(await asyncGraph.ContainsTripleAsync(triple));
        }

        [TestMethod]
        public async Task ShouldNotContainTriple()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple1);

            Assert.IsFalse(await asyncGraph.ContainsTripleAsync(triple2));
        }

        [TestMethod]
        public async Task ShouldNotContainNullTriple()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            await asyncGraph.AddTripleAsync(triple);

            Assert.IsFalse(await asyncGraph.ContainsTripleAsync(null));
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByNullAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[null, null, null, null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 3);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesBySubjectAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[new RDFResource("http://subj/"), null, null, null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesBySubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesBySubjectAsync(new RDFResource("http://subj/"));
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesBySubjectEvenIfNull()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesBySubjectAsync(null);
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesBySubjectAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph[new RDFResource("http://subj2/"), null, null, null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesBySubject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesBySubjectAsync(new RDFResource("http://subj2/"));
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByPredicateAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[null, new RDFResource("http://pred/"), null, null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByPredicateAsync(new RDFResource("http://pred/"));
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByPredicateEvenIfNull()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByPredicateAsync(null);
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesByPredicateAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph[null, new RDFResource("http://pred2/"), null, null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesByPredicate()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByPredicateAsync(new RDFResource("http://pred2/"));
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByObjectAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[null, null, new RDFResource("http://obj/"), null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByObjectAsync(new RDFResource("http://obj/"));
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByObjectEvenIfNull()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByObjectAsync(null);
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesByObjectAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph[null, null, new RDFResource("http://obj2/"), null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesByObject()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByObjectAsync(new RDFResource("http://obj2/"));
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByLiteralAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[null, null, null, new RDFPlainLiteral("lit")];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByLiteralAsync(new RDFPlainLiteral("lit"));
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByLiteralEvenIfNull()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByLiteralAsync(null);
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesByLiteralAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph[null, null, null, new RDFPlainLiteral("lit", "en-US")];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesByLiteral()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph select = await asyncGraph.SelectTriplesByLiteralAsync(new RDFPlainLiteral("lit", "en-US"));
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByComplexAccessor1()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[new RDFResource("http://subj/"), null, null, new RDFPlainLiteral("lit")];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByComplexAccessor2()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[null, new RDFResource("http://pred/"), new RDFResource("http://obj/"), null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldSelectTriplesByFullAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"), null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldNotSelectTriplesByFullAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            RDFAsyncGraph select = await asyncGraph[new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj/"), null];
            Assert.IsNotNull(select);
            Assert.IsTrue(select.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldThrowExceptionOnSelectingTriplesByIllecitAccessor()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2))
                .AddTripleAsync(triple3);

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await asyncGraph[null, null, new RDFResource("http://obj/"), new RDFPlainLiteral("lit")]);
        }

        [TestMethod]
        public async Task ShouldIntersectAsyncGraphs()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();
            await asyncGraph2.AddTripleAsync(triple1);

            RDFAsyncGraph intersect12 = await asyncGraph1.IntersectWithAsync(asyncGraph2);
            Assert.IsNotNull(intersect12);
            Assert.IsTrue(intersect12.TriplesCount == 1);
            RDFAsyncGraph intersect21 = await asyncGraph2.IntersectWithAsync(asyncGraph1);
            Assert.IsNotNull(intersect21);
            Assert.IsTrue(intersect21.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldIntersectAsyncGraphWithEmpty()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();

            RDFAsyncGraph intersect12 = await asyncGraph1.IntersectWithAsync(asyncGraph2);
            Assert.IsNotNull(intersect12);
            Assert.IsTrue(intersect12.TriplesCount == 0);
            RDFAsyncGraph intersect21 = await asyncGraph2.IntersectWithAsync(asyncGraph1);
            Assert.IsNotNull(intersect21);
            Assert.IsTrue(intersect21.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldIntersectEmptyWithAsyncGraph()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();
            await (await asyncGraph2.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph intersect12 = await asyncGraph1.IntersectWithAsync(asyncGraph2);
            Assert.IsNotNull(intersect12);
            Assert.IsTrue(intersect12.TriplesCount == 0);
            RDFAsyncGraph intersect21 = await asyncGraph2.IntersectWithAsync(asyncGraph1);
            Assert.IsNotNull(intersect21);
            Assert.IsTrue(intersect21.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldIntersectEmptyWithEmpty()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();

            RDFAsyncGraph intersect12 = await asyncGraph1.IntersectWithAsync(asyncGraph2);
            Assert.IsNotNull(intersect12);
            Assert.IsTrue(intersect12.TriplesCount == 0);
            RDFAsyncGraph intersect21 = await asyncGraph2.IntersectWithAsync(asyncGraph1);
            Assert.IsNotNull(intersect21);
            Assert.IsTrue(intersect21.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldIntersectAsyncGraphWithNull()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            
            RDFAsyncGraph intersect12 = await asyncGraph1.IntersectWithAsync(null);
            Assert.IsNotNull(intersect12);
            Assert.IsTrue(intersect12.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldIntersectAsyncGraphWithSelf()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            
            RDFAsyncGraph intersect12 = await asyncGraph1.IntersectWithAsync(asyncGraph1);
            Assert.IsNotNull(intersect12);
            Assert.IsTrue(intersect12.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldUnionAsyncGraphs()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj3/"), new RDFResource("http://pred3/"), new RDFResource("http://obj3/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();
            await (await asyncGraph2.AddTripleAsync(triple1))
                .AddTripleAsync(triple3);

            RDFAsyncGraph union12 = await asyncGraph1.UnionWithAsync(asyncGraph2);
            Assert.IsNotNull(union12);
            Assert.IsTrue(union12.TriplesCount == 3);
            RDFAsyncGraph union21 = await asyncGraph2.UnionWithAsync(asyncGraph1);
            Assert.IsNotNull(union21);
            Assert.IsTrue(union21.TriplesCount == 3);
        }

        [TestMethod]
        public async Task ShouldUnionAsyncGraphWithEmpty()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();

            RDFAsyncGraph union12 = await asyncGraph1.UnionWithAsync(asyncGraph2);
            Assert.IsNotNull(union12);
            Assert.IsTrue(union12.TriplesCount == 2);
            RDFAsyncGraph union21 = await asyncGraph2.UnionWithAsync(asyncGraph1);
            Assert.IsNotNull(union21);
            Assert.IsTrue(union21.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldUnionEmptyWithAsyncGraph()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();
            await (await asyncGraph2.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph union12 = await asyncGraph1.UnionWithAsync(asyncGraph2);
            Assert.IsNotNull(union12);
            Assert.IsTrue(union12.TriplesCount == 2);
            RDFAsyncGraph union21 = await asyncGraph2.UnionWithAsync(asyncGraph1);
            Assert.IsNotNull(union21);
            Assert.IsTrue(union21.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldUnionAsyncGraphWithNull()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph union12 = await asyncGraph1.UnionWithAsync(null);
            Assert.IsNotNull(union12);
            Assert.IsTrue(union12.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldUnionAsyncGraphWithSelf()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph union12 = await asyncGraph1.UnionWithAsync(asyncGraph1);
            Assert.IsNotNull(union12);
            Assert.IsTrue(union12.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldDifferenceAsyncGraphs()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFTriple triple3 = new RDFTriple(new RDFResource("http://subj3/"), new RDFResource("http://pred3/"), new RDFResource("http://obj3/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();
            await (await asyncGraph2.AddTripleAsync(triple1))
                .AddTripleAsync(triple3);

            RDFAsyncGraph difference12 = await asyncGraph1.DifferenceWithAsync(asyncGraph2);
            Assert.IsNotNull(difference12);
            Assert.IsTrue(difference12.TriplesCount == 1);
            RDFAsyncGraph difference21 = await asyncGraph2.DifferenceWithAsync(asyncGraph1);
            Assert.IsNotNull(difference21);
            Assert.IsTrue(difference21.TriplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldDifferenceAsyncGraphWithEmpty()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();

            RDFAsyncGraph difference12 = await asyncGraph1.DifferenceWithAsync(asyncGraph2);
            Assert.IsNotNull(difference12);
            Assert.IsTrue(difference12.TriplesCount == 2);
            RDFAsyncGraph difference21 = await asyncGraph2.DifferenceWithAsync(asyncGraph1);
            Assert.IsNotNull(difference21);
            Assert.IsTrue(difference21.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldDifferenceEmptyWithAsyncGraph()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            RDFAsyncGraph asyncGraph2 = new RDFAsyncGraph();
            await (await asyncGraph2.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph difference12 = await asyncGraph1.DifferenceWithAsync(asyncGraph2);
            Assert.IsNotNull(difference12);
            Assert.IsTrue(difference12.TriplesCount == 0);
            RDFAsyncGraph difference21 = await asyncGraph2.DifferenceWithAsync(asyncGraph1);
            Assert.IsNotNull(difference21);
            Assert.IsTrue(difference21.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldDifferenceAsyncGraphWithNull()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph difference12 = await asyncGraph1.DifferenceWithAsync(null);
            Assert.IsNotNull(difference12);
            Assert.IsTrue(difference12.TriplesCount == 2);
        }

        [TestMethod]
        public async Task ShouldDifferenceAsyncGraphWithSelf()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);

            RDFAsyncGraph difference12 = await asyncGraph1.DifferenceWithAsync(asyncGraph1);
            Assert.IsNotNull(difference12);
            Assert.IsTrue(difference12.TriplesCount == 0);
        }

        [DataTestMethod]
        [DataRow(".nt",RDFModelEnums.RDFFormats.NTriples)]
        [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
        [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
        [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
        public async Task ShouldExportToFileAsync(string fileExtension, RDFModelEnums.RDFFormats format)
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            await asyncGraph.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFAsyncGraphTest_ShouldExportToFile{fileExtension}"));

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"RDFAsyncGraphTest_ShouldExportToFile{fileExtension}")));
            Assert.IsTrue(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"RDFAsyncGraphTest_ShouldExportToFile{fileExtension}")).Length > 100);
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnExportingToNullOrEmptyFilepathAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async() => await new RDFAsyncGraph().ToFileAsync(RDFModelEnums.RDFFormats.NTriples, null));

        [DataTestMethod]
        [DataRow(RDFModelEnums.RDFFormats.NTriples)]
        [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
        [DataRow(RDFModelEnums.RDFFormats.TriX)]
        [DataRow(RDFModelEnums.RDFFormats.Turtle)]
        public async Task ShouldExportToStreamAsync(RDFModelEnums.RDFFormats format)
        {
            MemoryStream stream = new MemoryStream();
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            await asyncGraph.ToStreamAsync(format, stream);

            Assert.IsTrue(stream.ToArray().Length > 100);
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnExportingToNullStreamAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await new RDFAsyncGraph().ToStreamAsync(RDFModelEnums.RDFFormats.NTriples, null));

        [TestMethod]
        public async Task ShouldExportToDataTableAsync()
        {
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            DataTable table = await asyncGraph.ToDataTableAsync();

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
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph();
            DataTable table = await asyncGraph.ToDataTableAsync();

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
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            await asyncGraph1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFAsyncGraphTest_ShouldImportFromFile{fileExtension}"));
            RDFAsyncGraph asyncGraph2 = await RDFAsyncGraph.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFAsyncGraphTest_ShouldImportFromFile{fileExtension}"));

            Assert.IsNotNull(asyncGraph2);
            Assert.IsTrue(asyncGraph2.TriplesCount == 2);
            //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
            //triples with a predicate ending with "/" will loose this character once abbreviated:
            //this is correct (being a glitch of RDF/XML specs) so at the end the AsyncGraphs will differ
            if (format == RDFModelEnums.RDFFormats.RdfXml)
            {
                Assert.IsFalse(asyncGraph2.Equals(asyncGraph1));
                Assert.IsTrue((await asyncGraph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred/"))).TriplesCount == 0);
                Assert.IsTrue((await asyncGraph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred"))).TriplesCount == 2);
            }
            else
            {
                Assert.IsTrue(asyncGraph2.Equals(asyncGraph1));
            }
        }

        [DataTestMethod]
        [DataRow(".nt", RDFModelEnums.RDFFormats.NTriples)]
        [DataRow(".rdf", RDFModelEnums.RDFFormats.RdfXml)]
        [DataRow(".trix", RDFModelEnums.RDFFormats.TriX)]
        [DataRow(".ttl", RDFModelEnums.RDFFormats.Turtle)]
        public async Task ShouldImportEmptyFromFileAsync(string fileExtension, RDFModelEnums.RDFFormats format)
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            await asyncGraph1.ToFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFAsyncGraphTest_ShouldImportEmptyFromFile{fileExtension}"));
            RDFAsyncGraph asyncGraph2 = await RDFAsyncGraph.FromFileAsync(format, Path.Combine(Environment.CurrentDirectory, $"RDFAsyncGraphTest_ShouldImportEmptyFromFile{fileExtension}"));

            Assert.IsNotNull(asyncGraph2);
            Assert.IsTrue(asyncGraph2.TriplesCount == 0);
            Assert.IsTrue(asyncGraph2.Equals(asyncGraph1));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromNullOrEmptyFilepathAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromFileAsync(RDFModelEnums.RDFFormats.NTriples, null));

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromUnexistingFilepathAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromFileAsync(RDFModelEnums.RDFFormats.NTriples, "blablabla"));

        [DataTestMethod]
        [DataRow(RDFModelEnums.RDFFormats.NTriples)]
        [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
        [DataRow(RDFModelEnums.RDFFormats.TriX)]
        [DataRow(RDFModelEnums.RDFFormats.Turtle)]
        public async Task ShouldImportFromStreamAsync(RDFModelEnums.RDFFormats format)
        {
            MemoryStream stream = new MemoryStream();
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://ex/subj/"), new RDFResource("http://ex/pred/"), new RDFResource("http://ex/obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            await asyncGraph1.ToStreamAsync(format, stream);
            RDFAsyncGraph asyncGraph2 = await RDFAsyncGraph.FromStreamAsync(format, new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(asyncGraph2);
            Assert.IsTrue(asyncGraph2.TriplesCount == 2);
            //RDF/XML uses xsd:qname for encoding predicates. In this test we demonstrate that
            //triples with a predicate ending with "/" will loose this character once abbreviated:
            //this is correct (being a glitch of RDF/XML specs) so at the end the AsyncGraphs will differ
            if (format == RDFModelEnums.RDFFormats.RdfXml)
            {
                Assert.IsFalse(asyncGraph2.Equals(asyncGraph1));
                Assert.IsTrue((await asyncGraph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred/"))).TriplesCount == 0);
                Assert.IsTrue((await asyncGraph2.SelectTriplesByPredicateAsync(new RDFResource("http://ex/pred"))).TriplesCount == 2);
            }
            else
            {
                Assert.IsTrue(asyncGraph2.Equals(asyncGraph1));
            }
        }

        [DataTestMethod]
        [DataRow(RDFModelEnums.RDFFormats.NTriples)]
        [DataRow(RDFModelEnums.RDFFormats.RdfXml)]
        [DataRow(RDFModelEnums.RDFFormats.TriX)]
        [DataRow(RDFModelEnums.RDFFormats.Turtle)]
        public async Task ShouldImportFromEmptyStream(RDFModelEnums.RDFFormats format)
        {
            MemoryStream stream = new MemoryStream();
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            await asyncGraph1.ToStreamAsync(format, stream);
            RDFAsyncGraph asyncGraph2 = await RDFAsyncGraph.FromStreamAsync(format, new MemoryStream(stream.ToArray()));

            Assert.IsNotNull(asyncGraph2);
            Assert.IsTrue(asyncGraph2.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromNullStreamAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromStreamAsync(RDFModelEnums.RDFFormats.NTriples, null));

        [TestMethod]
        public async Task ShouldImportFromDataTableAsync()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            RDFTriple triple1 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFPlainLiteral("lit", "en-US"));
            RDFTriple triple2 = new RDFTriple(new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"));
            await (await asyncGraph1.AddTripleAsync(triple1))
                .AddTripleAsync(triple2);
            DataTable table = await asyncGraph1.ToDataTableAsync();
            RDFAsyncGraph asyncGraph2 = await RDFAsyncGraph.FromDataTableAsync(table);

            Assert.IsNotNull(asyncGraph2);
            Assert.IsTrue(asyncGraph2.TriplesCount == 2);
            Assert.IsTrue(asyncGraph2.Equals(asyncGraph1));
        }

        [TestMethod]
        public async Task ShouldImportEmptyFromDataTableAsync()
        {
            RDFAsyncGraph asyncGraph1 = new RDFAsyncGraph();
            DataTable table = await asyncGraph1.ToDataTableAsync();
            RDFAsyncGraph asyncGraph2 = await RDFAsyncGraph.FromDataTableAsync(table);

            Assert.IsNotNull(asyncGraph2);
            Assert.IsTrue(asyncGraph2.TriplesCount == 0);
            Assert.IsTrue(asyncGraph2.Equals(asyncGraph1));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromNullDataTableAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(null));

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableNotHaving3ColumnsAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableNotHavingExactColumnsAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECTTTTT", typeof(string));

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullSubjectAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECT", typeof(string));
            table.Rows.Add(null, "http://pred/", "http://obj/");

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptySubjectAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECT", typeof(string));
            table.Rows.Add("", "http://pred/", "http://obj/");

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralSubjectAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECT", typeof(string));
            table.Rows.Add("hello@en", "http://pred/", "http://obj/");

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullPredicateAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECT", typeof(string));
            table.Rows.Add("http://subj/", null, "http://obj/");

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithEmptyPredicateAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECT", typeof(string));
            table.Rows.Add("http://subj/", "", "http://obj/");

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithBlankPredicateAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECT", typeof(string));
            table.Rows.Add("http://subj/", "bnode:12345", "http://obj/");

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithLiteralPredicateAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECT", typeof(string));
            table.Rows.Add("http://subj/", "hello@en", "http://obj/");

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromDataTableHavingRowWithNullObjectAsync()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?SUBJECT", typeof(string));
            table.Columns.Add("?PREDICATE", typeof(string));
            table.Columns.Add("?OBJECT", typeof(string));
            table.Rows.Add("http://subj/", "http://pred/", null);

            await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromDataTableAsync(table));
        }

        [TestMethod]
        public async Task ShouldImportEmptyFromDataTableButGivingNameToAsyncGraphAsync()
        {
            RDFAsyncGraph asyncGraph1 = await new RDFAsyncGraph().SetContextAsync(new Uri("http://context/"));
            DataTable table = await asyncGraph1.ToDataTableAsync();
            RDFAsyncGraph asyncGraph2 = await RDFAsyncGraph.FromDataTableAsync(table);

            Assert.IsNotNull(asyncGraph2);
            Assert.IsTrue(asyncGraph2.TriplesCount == 0);
            Assert.IsTrue(asyncGraph2.Equals(asyncGraph1));
            Assert.IsTrue(asyncGraph2.Context.Equals(new Uri("http://context/")));
        }

        [TestMethod]
        public async Task ShouldImportFromUri()
        {
            RDFAsyncGraph asyncGraph = await RDFAsyncGraph.FromUriAsync(new Uri(RDFVocabulary.RDFS.BASE_URI));

            Assert.IsNotNull(asyncGraph);
            Assert.IsTrue(asyncGraph.Context.Equals(new Uri(RDFVocabulary.RDFS.BASE_URI)));
            Assert.IsTrue(asyncGraph.TriplesCount > 0);
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromNullUri()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromUriAsync(null));

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromRelativeUri()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromUriAsync(new Uri("/file/system", UriKind.Relative)));

        [TestMethod]
        public async Task ShouldImportFromUriAsync()
        {
            RDFAsyncGraph asyncGraph = await RDFAsyncGraph.FromUriAsync(new Uri(RDFVocabulary.RDFS.BASE_URI));

            Assert.IsNotNull(asyncGraph);
            Assert.IsTrue(asyncGraph.Context.Equals(new Uri(RDFVocabulary.RDFS.BASE_URI)));
            Assert.IsTrue(asyncGraph.TriplesCount > 0);
        }

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromNullUriAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromUriAsync(null));

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromRelativeUriAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromUriAsync(new Uri("/file/system", UriKind.Relative)));

        [TestMethod]
        public async Task ShouldRaiseExceptionOnImportingFromUnreacheableUriAsync()
            => await Assert.ThrowsExceptionAsync<RDFModelException>(async () => await RDFAsyncGraph.FromUriAsync(new Uri("http://rdfsharp.test/")));

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFAsyncGraphTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}