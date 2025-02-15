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

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFLoadOperationTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    #region Tests
    [TestMethod]
    public void ShouldCreateLoadOperation()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:fromCtx"));

        Assert.IsNotNull(operation);
        Assert.IsNotNull(operation.FromContext);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:fromCtx")));
        Assert.IsNull(operation.ToContext);
        Assert.IsFalse(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "LOAD <ex:fromCtx>"));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenCreatingLoadOperationBecauseNullContext()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFLoadOperation(null));

    [TestMethod]
    public void ShouldCreateLoadSilentOperation()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:fromCtx")).Silent();

        Assert.IsNotNull(operation);
        Assert.IsNotNull(operation.FromContext);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:fromCtx")));
        Assert.IsNull(operation.ToContext);
        Assert.IsTrue(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "LOAD SILENT <ex:fromCtx>"));
    }

    [TestMethod]
    public void ShouldCreateLoadOperationWithToContext()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:fromCtx")).SetContext(new Uri("ex:toCtx"));

        Assert.IsNotNull(operation);
        Assert.IsNotNull(operation.FromContext);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:fromCtx")));
        Assert.IsNotNull(operation.ToContext);
        Assert.IsTrue(operation.ToContext.Equals(new Uri("ex:toCtx")));
        Assert.IsFalse(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "LOAD <ex:fromCtx> INTO GRAPH <ex:toCtx>"));
    }

    [TestMethod]
    public void ShouldCreateLoadSilentOperationWithToContext()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:fromCtx")).SetContext(new Uri("ex:toCtx")).Silent();

        Assert.IsNotNull(operation);
        Assert.IsNotNull(operation.FromContext);
        Assert.IsTrue(operation.FromContext.Equals(new Uri("ex:fromCtx")));
        Assert.IsNotNull(operation.ToContext);
        Assert.IsTrue(operation.ToContext.Equals(new Uri("ex:toCtx")));
        Assert.IsTrue(operation.IsSilent);
        Assert.IsTrue(string.Equals(operation.ToString(), "LOAD SILENT <ex:fromCtx> INTO GRAPH <ex:toCtx>"));
    }

    [TestMethod]
    public void ShouldApplyToNullGraph()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ctx"));
        RDFOperationResult result = operation.ApplyToGraph(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
    }

    [TestMethod]
    public void ShouldApplyToGraph()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToGraph"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "application/turtle")
                    .WithBody($"@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:subj> <ex:pred> <ex:obj>."));

        RDFGraph graph = new RDFGraph();
        RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToGraph"));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(3, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(1, graph.TriplesCount);
    }

    [TestMethod]
    public async Task ShouldApplyToNullGraphAsync()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ctx"));
        RDFOperationResult result = await operation.ApplyToGraphAsync(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
    }

    [TestMethod]
    public async Task ShouldApplyToGraphAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToGraphAsync"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "application/turtle")
                    .WithBody($"@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:subj> <ex:pred> <ex:obj>."));

        RDFGraph graph = new RDFGraph();
        RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToGraphAsync"));
        RDFOperationResult result = await operation.ApplyToGraphAsync(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(3, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(1, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToNullStore()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ctx"));
        RDFOperationResult result = operation.ApplyToStore(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
    }

    [TestMethod]
    public void ShouldApplyToStore()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToStore"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "application/n-quads")
                    .WithBody("<ex:subj> <ex:pred> <ex:obj> <ex:ctx>."));

        RDFMemoryStore store = new RDFMemoryStore();
        RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToStore"));
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(4, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldApplyToStoreWithDefaultContext()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToStoreWithDefaultContext"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "application/n-quads")
                    .WithBody("<ex:subj> <ex:pred> <ex:obj>."));

        RDFMemoryStore store = new RDFMemoryStore();
        RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToStoreWithDefaultContext"));
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(4, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), $"{RDFNamespaceRegister.DefaultNamespace}"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldApplyToNullStoreAsync()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ctx"));
        RDFOperationResult result = await operation.ApplyToStoreAsync(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
    }

    [TestMethod]
    public async Task ShouldApplyToStoreAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToStoreAsync"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "application/n-quads")
                    .WithBody("<ex:subj> <ex:pred> <ex:obj> <ex:ctx>."));

        RDFMemoryStore store = new RDFMemoryStore();
        RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToStoreAsync"));
        RDFOperationResult result = await operation.ApplyToStoreAsync(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(4, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldApplyToStoreWithDefaultContextAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToStoreWithDefaultContextAsync"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "application/n-quads")
                    .WithBody("<ex:subj> <ex:pred> <ex:obj>."));

        RDFMemoryStore store = new RDFMemoryStore();
        RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToStoreWithDefaultContextAsync"));
        RDFOperationResult result = await operation.ApplyToStoreAsync(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(4, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), $"{RDFNamespaceRegister.DefaultNamespace}"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(1, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingNonSilentCrashingLoadOperation()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldThrowExceptionWhenApplyingNonSilentCrashingLoadOperation"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj ."));

        RDFGraph graph = new RDFGraph();
        RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldThrowExceptionWhenApplyingNonSilentCrashingLoadOperation"));
        Assert.ThrowsExactly<RDFModelException>(() => operation.ApplyToGraph(graph));
    }

    [TestMethod]
    public void ShouldNotThrowExceptionWhenApplyingSilentCrashingLoadOperation()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldNotThrowExceptionWhenApplyingSilentCrashingLoadOperation"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj ."));

        RDFGraph graph = new RDFGraph();
        RDFLoadOperation operation = new RDFLoadOperation(new Uri(server.Url + "/RDFLoadOperationTest/ShouldNotThrowExceptionWhenApplyingSilentCrashingLoadOperation")).Silent();
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.IsNotNull(result.InsertResultsCount == 0);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.IsNotNull(result.DeleteResultsCount == 0);
    }

    [TestMethod]
    public void ShouldApplyToNullSPARQLUpdateEndpoint()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ctx"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpoint()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("ex:ShouldApplyToSPARQLUpdateEndpoint"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj> ."));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpoint"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpoint"));

        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ShouldApplyToSPARQLUpdateEndpoint"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithParams()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("ex:ShouldApplyToSPARQLUpdateEndpointWithParams"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj> ."));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams")
                    .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                    .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams"));
        endpoint.AddDefaultGraphUri("ex:ctx1");
        endpoint.AddNamedGraphUri("ex:ctx2");

        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ShouldApplyToSPARQLUpdateEndpointWithParams"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentType()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("ex:ShouldApplyToSPARQLUpdateEndpointWithRequestContentType"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj> ."));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType")
                    .WithBody(new RegexMatcher("update=.*")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(250));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType"));

        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ShouldApplyToSPARQLUpdateEndpointWithRequestContentType"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("ex:ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj> ."));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams")
                    .WithBody(new RegexMatcher("using-named-graph-uri=ex%3actx2&using-graph-uri=ex%3actx1&update=.*")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(250));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams"));
        endpoint.AddDefaultGraphUri("ex:ctx1");
        endpoint.AddNamedGraphUri("ex:ctx2");

        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("ex:ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj> ."));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(250));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"));

        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("ex:ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj> ."));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(750));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFLoadOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"));

        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"));

        Assert.ThrowsExactly<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(250)));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("ex:ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj> ."));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.InternalServerError));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFLoadOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"));

        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"));

        Assert.ThrowsExactly<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint));
    }

    [TestMethod]
    public async Task ShouldApplyToNullSPARQLUpdateEndpointAsync()
    {
        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ctx"));
        bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ShouldApplyToSPARQLUpdateEndpointAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("ex:ShouldApplyToSPARQLUpdateEndpointAsync"))
            .RespondWith(
                Response.Create()
                    .WithHeader("ContentType", "text/turtle")
                    .WithBody("<ex:subj> a <ex:obj> ."));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFLoadOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"));

        RDFLoadOperation operation = new RDFLoadOperation(new Uri("ex:ShouldApplyToSPARQLUpdateEndpointAsync"));
        bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

        Assert.IsTrue(result);
    }
    #endregion
}