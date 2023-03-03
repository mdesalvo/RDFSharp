/*
   Copyright 2012-2023 Marco De Salvo

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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFClearOperationTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        [TestCleanup]
        public void Cleanup()  { server.Stop(); server.Dispose(); }

        #region Tests
        [TestMethod]
        public void ShouldCreateClearNamedOperation()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.FromContext);
            Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:ctx")));
            Assert.IsFalse(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), "CLEAR GRAPH <ex:ctx>"));
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenCreatingClearNamedOperationBecauseNullContext()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFClearOperation(null));

        [TestMethod]
        public void ShouldCreateClearSilentNamedOperation()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx")).Silent();

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.FromContext);
            Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:ctx")));
            Assert.IsTrue(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), "CLEAR SILENT GRAPH <ex:ctx>"));
        }

        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.ALL)]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.DEFAULT)]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.NAMED)]
        public void ShouldCreateClearFlavorOperation(RDFQueryEnums.RDFClearOperationFlavor opFlavor)
        {
            RDFClearOperation operation = new RDFClearOperation(opFlavor);

            Assert.IsNotNull(operation);
            Assert.IsNull(operation.FromContext);
            Assert.IsTrue(operation.OperationFlavor == opFlavor);
            Assert.IsFalse(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), $"CLEAR {opFlavor}"));
        }

        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.ALL)]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.DEFAULT)]
        [DataRow(RDFQueryEnums.RDFClearOperationFlavor.NAMED)]
        public void ShouldCreateClearSilentFlavorOperation(RDFQueryEnums.RDFClearOperationFlavor opFlavor)
        {
            RDFClearOperation operation = new RDFClearOperation(opFlavor).Silent();

            Assert.IsNotNull(operation);
            Assert.IsNull(operation.FromContext);
            Assert.IsTrue(operation.OperationFlavor == opFlavor);
            Assert.IsTrue(operation.IsSilent);
            Assert.IsTrue(string.Equals(operation.ToString(), $"CLEAR SILENT {opFlavor}"));
        }

        [TestMethod]
        public void ShouldApplyToNullGraph()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            RDFOperationResult result = operation.ApplyToGraph(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToGraph()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            RDFOperationResult result = operation.ApplyToGraph(graph); //When applied to a graph, CLEAR behavior is ALL

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 3);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 2);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToNullGraphAsync()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            RDFOperationResult result = await operation.ApplyToGraphAsync(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToGraphAsync()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            graph.AddTriple(new RDFTriple(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            RDFOperationResult result = await operation.ApplyToGraphAsync(new RDFAsyncGraph(graph)); //When applied to a graph, CLEAR behavior is ALL

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 3);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 2);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(graph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToNullStore()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            RDFOperationResult result = operation.ApplyToStore(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToStoreWithContextBehavior()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            RDFOperationResult result = operation.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 4);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldApplyToStoreWithAllFlavorBehavior()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(RDFQueryEnums.RDFClearOperationFlavor.ALL);
            RDFOperationResult result = operation.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 4);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 2);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?CONTEXT"].ToString(), $"{RDFNamespaceRegister.DefaultNamespace}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(store.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToStoreWithDefaultFlavorBehavior()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(RDFQueryEnums.RDFClearOperationFlavor.DEFAULT);
            RDFOperationResult result = operation.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 4);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), $"{RDFNamespaceRegister.DefaultNamespace}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldApplyToStoreWithNamedFlavorBehavior()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(RDFQueryEnums.RDFClearOperationFlavor.NAMED);
            RDFOperationResult result = operation.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 4);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(store.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldApplyToNullStoreAsync()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            RDFOperationResult result = await operation.ApplyToStoreAsync(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToStoreWithContextBehaviorAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            RDFOperationResult result = await operation.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 4);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldApplyToStoreWithAllFlavorBehaviorAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(RDFQueryEnums.RDFClearOperationFlavor.ALL);
            RDFOperationResult result = await operation.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 4);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 2);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?CONTEXT"].ToString(), $"{RDFNamespaceRegister.DefaultNamespace}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToStoreWithDefaultFlavorBehaviorAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(RDFQueryEnums.RDFClearOperationFlavor.DEFAULT);
            RDFOperationResult result = await operation.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 4);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), $"{RDFNamespaceRegister.DefaultNamespace}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
        }

        [TestMethod]
        public async Task ShouldApplyToStoreWithNamedFlavorBehaviorAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFClearOperation operation = new RDFClearOperation(RDFQueryEnums.RDFClearOperationFlavor.NAMED);
            RDFOperationResult result = await operation.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 4);
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.DeleteResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsTrue(store.WrappedStore is RDFMemoryStore memStore && memStore.QuadruplesCount == 1);
        }

        [TestMethod]
        public void ShouldApplyToNullSPARQLUpdateEndpoint()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpoint()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpoint"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpoint"));

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpointWithParams()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams")
                           .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                           .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams"));
            endpoint.AddDefaultGraphUri("ex:ctx1");
            endpoint.AddNamedGraphUri("ex:ctx2");

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(250));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"));

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentType()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType")
                           .WithBody(new RegexMatcher("update=.*")))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(250));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType"));

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams")
                           .WithBody(new RegexMatcher("using-named-graph-uri=ex%3actx2&using-graph-uri=ex%3actx1&update=.*")))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(250));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams"));
            endpoint.AddDefaultGraphUri("ex:ctx1");
            endpoint.AddNamedGraphUri("ex:ctx2");

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(750));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"));

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));

            Assert.ThrowsException<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(250)));
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.InternalServerError));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"));

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));

            Assert.ThrowsException<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint));
        }

        [TestMethod]
        public async Task ShouldApplyToNullSPARQLUpdateEndpointAsync()
        {
            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ShouldApplyToSPARQLUpdateEndpointAsync()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"));

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ShouldApplyToSPARQLUpdateEndpointWithParamsAsync()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParamsAsync")
                           .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                           .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFClearOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParamsAsync"));
            endpoint.AddDefaultGraphUri("ex:ctx1");
            endpoint.AddNamedGraphUri("ex:ctx2");

            RDFClearOperation operation = new RDFClearOperation(new Uri("ex:ctx"));
            bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

            Assert.IsTrue(result);
        }
        #endregion
    }
}