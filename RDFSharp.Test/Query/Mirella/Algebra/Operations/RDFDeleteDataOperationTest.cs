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
    public class RDFDeleteDataOperationTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        #region Tests
        [TestMethod]
        public void ShouldCreateDeleteDataOperation()
        {
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.InsertTemplates);
            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsNotNull(operation.DeleteTemplates);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsNotNull(operation.Variables);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 0);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"DELETE DATA {
}"));
        }

        [TestMethod]
        public void ShouldAddDeleteTemplate()
        {
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"));
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"));
            operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.InsertTemplates);
            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsNotNull(operation.DeleteTemplates);
            Assert.IsTrue(operation.DeleteTemplates.Count == 2);
            Assert.IsNotNull(operation.Variables);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 3);
            Assert.IsTrue(operation.QueryMembers.Count == 0);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX owl: <http://www.w3.org/2002/07/owl#>

DELETE DATA {
  <ex:subj> <ex:pred> <ex:obj> .
  rdfs:Class rdf:type owl:Class .
}"));
        }

        [TestMethod]
        public void ShouldApplyToNullGraph()
        {
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
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
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS)); //Duplicate triple...
            RDFOperationResult result = operation.ApplyToGraph(graph);

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
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
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
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS)); //Duplicate triple...
            RDFOperationResult result = await operation.ApplyToGraphAsync(new RDFAsyncGraph(graph));

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
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
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
        public void ShouldApplyToStore()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS)); //Duplicate triple...
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
        public async Task ShouldApplyToNullStoreAsync()
        {
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
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
        public async Task ShouldApplyToStoreAsync()
        {
            RDFAsyncStore store = new RDFAsyncStore();
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(),RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            operation.AddDeleteTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS)); //Duplicate triple...
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
        public void ShouldApplyToNullSPARQLUpdateEndpoint()
        {
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpoint()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpoint"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpoint"));

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpointWithParams()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams")
                           .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                           .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams"));
            endpoint.AddDefaultGraphUri("ex:ctx1");
            endpoint.AddNamedGraphUri("ex:ctx2");

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(250));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"));

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentType()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType")
                           .WithBody(new RegexMatcher("update=.*")))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(250));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType"));

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams")
                           .WithBody(new RegexMatcher("using-named-graph-uri=ex%3actx2&using-graph-uri=ex%3actx1&update=.*")))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(250));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams"));
            endpoint.AddDefaultGraphUri("ex:ctx1");
            endpoint.AddNamedGraphUri("ex:ctx2");

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(750));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"));

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

            Assert.ThrowsException<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(250)));
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.InternalServerError));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"));

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

            Assert.ThrowsException<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint));
        }

        [TestMethod]
        public async Task ShouldApplyToNullSPARQLUpdateEndpointAsync()
        {
            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ShouldApplyToSPARQLUpdateEndpointAsync()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"));

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ShouldApplyToSPARQLUpdateEndpointWithParamsAsync()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParamsAsync")
                           .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                           .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
                .RespondWith(
                    Response.Create()
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteDataOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParamsAsync"));
            endpoint.AddDefaultGraphUri("ex:ctx1");
            endpoint.AddNamedGraphUri("ex:ctx2");

            RDFDeleteDataOperation operation = new RDFDeleteDataOperation();
            operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

            Assert.IsTrue(result);
        }
        #endregion
    }
}