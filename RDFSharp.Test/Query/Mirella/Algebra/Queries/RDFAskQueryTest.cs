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
using System.Net;
using System.Text;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFAskQueryTests
    {
        private WireMockServer sparqlAskServer;

        [TestInitialize]
        public void Initialize() => sparqlAskServer = WireMockServer.Start();

        [TestCleanup]
        public void Cleanup() => sparqlAskServer.Stop();
        
        #region Tests
        [TestMethod]
        public void ShouldApplyAskQueryToSPARQLEndpointAndHaveTrueResult()
        {
            sparqlAskServer
                .Given(
                    Request.Create()
                           .WithPath("/RDFAskQueryTest/ShouldApplyAskQueryToSPARQLEndpointAndHaveTrueResult/sparql")
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

            RDFAskQuery query = new RDFAskQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(sparqlAskServer.Url + "/RDFAskQueryTest/ShouldApplyAskQueryToSPARQLEndpointAndHaveTrueResult/sparql"));
            RDFAskQueryResult result = query.ApplyToSPARQLEndpoint(endpoint);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.AskResult);
        }

        [TestMethod]
        public void ShouldApplyAskQueryToSPARQLEndpointAndHaveFalseResult()
        {
            sparqlAskServer
                .Given(
                    Request.Create()
                           .WithPath("/RDFAskQueryTest/ShouldApplyAskQueryToSPARQLEndpointAndHaveFalseResult/sparql")
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

            RDFAskQuery query = new RDFAskQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(sparqlAskServer.Url + "/RDFAskQueryTest/ShouldApplyAskQueryToSPARQLEndpointAndHaveFalseResult/sparql"));
            RDFAskQueryResult result = query.ApplyToSPARQLEndpoint(endpoint);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }
        #endregion
    }
}
