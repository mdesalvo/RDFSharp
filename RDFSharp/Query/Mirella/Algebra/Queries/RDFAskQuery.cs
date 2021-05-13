/*
   Copyright 2012-2020 Marco De Salvo

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
using RDFSharp.Store;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFAskQuery is the SPARQL "ASK" query implementation.
    /// </summary>
    public class RDFAskQuery : RDFQuery
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ASK query
        /// </summary>
        public RDFAskQuery() { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the ASK query
        /// </summary>
        public override string ToString()
            => RDFQueryPrinter.PrintAskQuery(this);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFAskQuery AddPatternGroup(RDFPatternGroup patternGroup)
        {
            if (patternGroup != null)
            {
                if (!this.GetPatternGroups().Any(q => q.Equals(patternGroup)))
                    this.QueryMembers.Add(patternGroup);
            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the query
        /// </summary>
        public RDFAskQuery AddPrefix(RDFNamespace prefix)
        {
            if (prefix != null)
            {
                if (!this.Prefixes.Any(p => p.Equals(prefix)))
                    this.Prefixes.Add(prefix);
            }
            return this;
        }

        /// <summary>
        /// Adds the given subquery to the query
        /// </summary>
        public RDFAskQuery AddSubQuery(RDFSelectQuery subQuery)
        {
            if (subQuery != null)
            {
                if (!this.GetSubQueries().Any(q => q.Equals(subQuery)))
                    this.QueryMembers.Add(subQuery.SubQuery());
            }
            return this;
        }

        /// <summary>
        /// Applies the query to the given graph
        /// </summary>
        public RDFAskQueryResult ApplyToGraph(RDFGraph graph)
            => graph != null ? new RDFQueryEngine().EvaluateAskQuery(this, graph)
                             : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => this.ApplyToGraph(graph));

        /// <summary>
        /// Applies the query to the given store
        /// </summary>
        public RDFAskQueryResult ApplyToStore(RDFStore store)
            => store != null ? new RDFQueryEngine().EvaluateAskQuery(this, store)
                             : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given store
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToStoreAsync(RDFStore store)
            => Task.Run(() => this.ApplyToStore(store));

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFAskQueryResult ApplyToFederation(RDFFederation federation)
            => federation != null ? new RDFQueryEngine().EvaluateAskQuery(this, federation)
                                  : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToFederationAsync(RDFFederation federation)
            => Task.Run(() => this.ApplyToFederation(federation));

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFAskQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
        {
            string askQueryString = this.ToString();
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            if (sparqlEndpoint != null)
            {
                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient())
                {
                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(askQueryString));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/sparql-results+xml");

                    //Send querystring to SPARQL endpoint
                    byte[] sparqlResponse = webClient.DownloadData(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                            askResult = RDFAskQueryResult.FromSparqlXmlResult(sStream);
                    }
                }
            }
            return askResult;
        }

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint)
            => Task.Run(() => this.ApplyToSPARQLEndpoint(sparqlEndpoint));
        #endregion

    }

}