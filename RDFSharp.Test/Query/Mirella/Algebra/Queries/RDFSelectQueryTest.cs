/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFSelectQueryTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        [TestCleanup]
        public void Cleanup()  { server.Stop(); server.Dispose(); }
        
        #region Tests
        [TestMethod]
        public void ShouldCreateSelectQuery()
        {
            RDFSelectQuery query = new RDFSelectQuery();

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.QueryMembers);
            Assert.IsTrue(query.QueryMembers.Count == 0);
            Assert.IsNotNull(query.Prefixes);
            Assert.IsTrue(query.Prefixes.Count == 0);
            Assert.IsNotNull(query.ProjectionVars);
            Assert.IsTrue(query.ProjectionVars.Count == 0);
            Assert.IsTrue(query.IsEvaluable);
            Assert.IsFalse(query.IsOptional);
            Assert.IsFalse(query.JoinAsUnion);
            Assert.IsFalse(query.IsSubQuery);
            Assert.IsTrue(query.ToString().Equals("SELECT *" + Environment.NewLine + "WHERE {" + Environment.NewLine + "}" + Environment.NewLine));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 0);
            Assert.IsTrue(query.GetPatternGroups().Count() == 0);
            Assert.IsTrue(query.GetSubQueries().Count() == 0);
            Assert.IsTrue(query.GetValues().Count() == 0);
            Assert.IsTrue(query.GetModifiers().Count() == 0);
            Assert.IsTrue(query.GetPrefixes().Count() == 0);
        }

        [TestMethod]
        public void ShouldCreateSelectQueryWithQueryMembers()
        {
            RDFSelectQuery query = new RDFSelectQuery();
            query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            query.AddPatternGroup(
                new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddFilter(new RDFIsUriFilter(new RDFVariable("?S"))));
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup("PG1")
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S")));
            query.AddModifier(new RDFGroupByModifier(new List<RDFVariable>() { new RDFVariable("?S") }));
            query.AddModifier(new RDFGroupByModifier(new List<RDFVariable>() { new RDFVariable("?P") })); //Will be discarded, since duplicate modifiers are not allowed
            query.AddModifier(new RDFDistinctModifier());
            query.AddModifier(new RDFDistinctModifier()); //Will be discarded, since duplicate modifiers are not allowed
            query.AddModifier(new RDFLimitModifier(100));
            query.AddModifier(new RDFLimitModifier(75)); //Will be discarded, since duplicate modifiers are not allowed
            query.AddModifier(new RDFOffsetModifier(20));
            query.AddModifier(new RDFOffsetModifier(25)); //Will be discarded, since duplicate modifiers are not allowed
            query.AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.DESC));
            query.AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC)); //Will be discarded, since duplicate modifiers are not allowed

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+Environment.NewLine+"SELECT DISTINCT ?S "+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  {"+Environment.NewLine+"    SELECT ?S"+Environment.NewLine+"    WHERE {"+Environment.NewLine+"      {"+Environment.NewLine+"        ?S ?P owl:Class ."+Environment.NewLine+"        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"GROUP BY ?S"+Environment.NewLine+"ORDER BY DESC(?S)"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"+Environment.NewLine));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 3); //SPARQL Values is managed by Mirella
            Assert.IsTrue(query.GetPatternGroups().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Count() == 1);
            Assert.IsTrue(query.GetValues().Count() == 1);
            Assert.IsTrue(query.GetModifiers().Count() == 5);
            Assert.IsTrue(query.GetPrefixes().Count() == 2);
        }

        [TestMethod]
        public void ShouldApplySelectQueryToGraphAndHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddProjectionVariable(new RDFVariable("?S"));
            RDFSelectQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResults.Columns[0].ColumnName.Equals("?S"));
            Assert.IsTrue(result.SelectResults.Rows.Count == 1);
            Assert.IsTrue(result.SelectResults.Rows[0]["?S"].Equals("ex:flower"));
        }

        [TestMethod]
        public void ShouldApplySelectQueryToGraphAndNotHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)))
                .AddProjectionVariable(new RDFVariable("?S"));
            RDFSelectQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResultsCount == 0);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResults.Columns[0].ColumnName.Equals("?S"));
            Assert.IsTrue(result.SelectResults.Rows.Count == 0);
        }
        
        [TestMethod]
        public void ShouldApplySelectQueryToNullGraphAndNotHaveResults()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddProjectionVariable(new RDFVariable("?S"));
            RDFSelectQueryResult result = query.ApplyToGraph(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResultsCount == 0);
            Assert.IsTrue(result.SelectResults.Columns.Count == 0);
        }

        [TestMethod]
        public void ShouldApplySelectQueryToStoreAndHaveResults()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddProjectionVariable(new RDFVariable("?C"))
                .AddProjectionVariable(new RDFVariable("?S"));
            RDFSelectQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResults.Columns[0].ColumnName.Equals("?C"));
            Assert.IsTrue(result.SelectResults.Columns[1].ColumnName.Equals("?S"));
            Assert.IsTrue(result.SelectResults.Rows.Count == 1);
            Assert.IsTrue(result.SelectResults.Rows[0]["?C"].Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
            Assert.IsTrue(result.SelectResults.Rows[0]["?S"].Equals("ex:flower"));
        }

        [TestMethod]
        public void ShouldApplySelectQueryToStoreAndNotHaveResults()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddProjectionVariable(new RDFVariable("?S"));
            RDFSelectQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResultsCount == 0);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResults.Columns[0].ColumnName.Equals("?S"));
            Assert.IsTrue(result.SelectResults.Rows.Count == 0);
        }
        
        [TestMethod]
        public void ShouldApplySelectQueryToNullStoreAndNotHaveResults()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddProjectionVariable(new RDFVariable("?S"));
            RDFSelectQueryResult result = query.ApplyToStore(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResultsCount == 0);
            Assert.IsTrue(result.SelectResults.Columns.Count == 0);
        }

        /*
        [TestMethod]
        public void ShouldApplySelectQueryToFederationAndHaveTrueResult()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation().AddGraph(graph);
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.AskResult);
        }

        [TestMethod]
        public void ShouldApplySelectQueryToFederationAndHaveFalseResult()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.DATATYPE));
            RDFFederation federation = new RDFFederation().AddGraph(graph);
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public void ShouldApplySelectQueryToNullFederationAndHaveFalseResult()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = query.ApplyToFederation(null);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public void ShouldApplySelectQueryToSPARQLEndpointAndHaveTrueResult()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointAndHaveTrueResult/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody(
@"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head />
    <boolean>true</boolean>
</sparql>", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/sparql-results+xml")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointAndHaveTrueResult/sparql"));
            RDFSelectQueryResult result = query.ApplyToSPARQLEndpoint(endpoint);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.AskResult);
        }

        [TestMethod]
        public void ShouldApplySelectQueryToSPARQLEndpointAndHaveFalseResult()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointAndHaveFalseResult/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody(
@"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head />
    <boolean>false</boolean>
</sparql>", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/sparql-results+xml")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointAndHaveFalseResult/sparql"));
            RDFSelectQueryResult result = query.ApplyToSPARQLEndpoint(endpoint);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public void ShouldApplySelectQueryToNullSPARQLEndpointAndHaveFalseResult()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = query.ApplyToSPARQLEndpoint(null);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenApplyingSelectQueryToSPARQLEndpointAccordingToTimingAndBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFSelectQueryTest/ShouldThrowExceptionWhenApplyingSelectQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody(
@"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head />
    <boolean>false</boolean>
</sparql>", encoding: Encoding.UTF8)
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(1000));

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFSelectQueryTest/ShouldThrowExceptionWhenApplyingSelectQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql"));

            Assert.ThrowsException<RDFQueryException>(() => query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(250, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)));
        }

        [TestMethod]
        public void ShouldGiveEmptyResultWhenApplyingSelectQueryToSPARQLEndpointAccordingToTimingAndBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFSelectQueryTest/ShouldThrowExceptionWhenApplyingSelectQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody(
@"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head />
    <boolean>false</boolean>
</sparql>", encoding: Encoding.UTF8)
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(1000));

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFSelectQueryTest/ShouldThrowExceptionWhenApplyingSelectQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql"));

            RDFSelectQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(250, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult));

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenApplyingSelectQueryToSPARQLEndpointAccordingToBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointWithOptionsToThrowException/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody("Internal Server Error")
                            .WithStatusCode(HttpStatusCode.InternalServerError)
                            .WithFault(FaultType.NONE));

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointWithOptionsToThrowException/sparql"));

            Assert.ThrowsException<RDFQueryException>(() => query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(1000, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)));
        }

        [TestMethod]
        public void ShouldGiveEmptyResultWhenApplyingSelectQueryToSPARQLEndpointAccordingToBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFSelectQueryTest/ShouldGiveEmptyResultWhenApplyingSelectQueryToSPARQLEndpointAccordingToBehavior/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody("Internal Server Error")
                            .WithStatusCode(HttpStatusCode.InternalServerError)
                            .WithFault(FaultType.NONE));

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFSelectQueryTest/ShouldGiveEmptyResultWhenApplyingSelectQueryToSPARQLEndpointAccordingToBehavior/sparql"));

            RDFSelectQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(1000, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult));

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        //ASYNC

        [TestMethod]
        public async Task ShouldApplySelectQueryToGraphAsyncAndHaveTrueResult()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToGraphAsync(graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToGraphAsyncAndHaveFalseResult()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.DATATYPE));
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToGraphAsync(graph);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToNullGraphAsyncAndHaveFalseResult()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToGraphAsync(null);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToStoreAsyncAndHaveTrueResult()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToStoreAsyncAndHaveFalseResult()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToNullStoreAsyncAndHaveFalseResult()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToStoreAsync(null);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToFederationAsyncAndHaveTrueResult()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation().AddGraph(graph);
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToFederationAsync(federation);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToFederationAsyncAndHaveFalseResult()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.DATATYPE));
            RDFFederation federation = new RDFFederation().AddGraph(graph);
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToFederationAsync(federation);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToNullFederationAsyncAndHaveFalseResult()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToFederationAsync(null);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToSPARQLEndpointAsyncAndHaveTrueResult()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointAsyncAndHaveTrueResult/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody(
@"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head />
    <boolean>true</boolean>
</sparql>", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/sparql-results+xml")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointAsyncAndHaveTrueResult/sparql"));
            RDFSelectQueryResult result = await query.ApplyToSPARQLEndpointAsync(endpoint);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToSPARQLEndpointAsyncAndHaveFalseResult()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointAsyncAndHaveFalseResult/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody(
@"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head />
    <boolean>false</boolean>
</sparql>", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/sparql-results+xml")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFSelectQueryTest/ShouldApplySelectQueryToSPARQLEndpointAsyncAndHaveFalseResult/sparql"));
            RDFSelectQueryResult result = await query.ApplyToSPARQLEndpointAsync(endpoint);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public async Task ShouldApplySelectQueryToNullSPARQLEndpointAsyncAndHaveFalseResult()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSelectQueryResult result = await query.ApplyToSPARQLEndpointAsync(null);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }
        */
        #endregion
    }
}
