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

using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using static RDFSharp.Query.RDFQueryUtilities;

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
            => AddPatternGroup<RDFAskQuery>(patternGroup);

        /// <summary>
        /// Adds the given prefix declaration to the query
        /// </summary>
        public RDFAskQuery AddPrefix(RDFNamespace prefix)
            => AddPrefix<RDFAskQuery>(prefix);

        /// <summary>
        /// Adds the given subquery to the query
        /// </summary>
        public RDFAskQuery AddSubQuery(RDFSelectQuery subQuery)
            => AddSubQuery<RDFAskQuery>(subQuery);

        /// <summary>
        /// Applies the query to the given graph
        /// </summary>
        public RDFAskQueryResult ApplyToGraph(RDFGraph graph)
            => graph != null ? new RDFQueryEngine().EvaluateAskQuery(this, graph) : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => ApplyToGraph(graph));

        /// <summary>
        /// Applies the query to the given store
        /// </summary>
        public RDFAskQueryResult ApplyToStore(RDFStore store)
            => store != null ? new RDFQueryEngine().EvaluateAskQuery(this, store) : new RDFAskQueryResult();

        /// <summary>
        /// Applies the query to the given asynchronous store
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToStoreAsync(RDFAsyncStore asyncStore)
            => Task.Run(() => ApplyToStore(asyncStore?.WrappedStore));

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFAskQueryResult ApplyToFederation(RDFFederation federation)
            => federation != null ? new RDFQueryEngine().EvaluateAskQuery(this, federation) : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToFederationAsync(RDFFederation federation)
            => Task.Run(() => ApplyToFederation(federation));

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFAskQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpoint(ToString(), sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFAskQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => ApplyRawToSPARQLEndpoint(ToString(), sparqlEndpoint, sparqlEndpointQueryOptions);

        /// <summary>
        /// Applies the given raw string ASK query to the given SPARQL endpoint
        /// </summary>
        public static RDFAskQueryResult ApplyRawToSPARQLEndpoint(string askQuery, RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpoint(askQuery, sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Applies the given raw string ASK query to the given SPARQL endpoint
        /// </summary>
        public static RDFAskQueryResult ApplyRawToSPARQLEndpoint(string askQuery, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => sparqlEndpoint != null ? (RDFAskQueryResult)new RDFQueryEngine().ApplyRawToSPARQLEndpoint("ASK", askQuery, sparqlEndpoint, sparqlEndpointQueryOptions) : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpointAsync(ToString(), sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFAskQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => ApplyRawToSPARQLEndpointAsync(ToString(), sparqlEndpoint, sparqlEndpointQueryOptions);

        /// <summary>
        /// Asynchronously applies the given raw string ASK query to the given SPARQL endpoint
        /// </summary>
        public static Task<RDFAskQueryResult> ApplyRawToSPARQLEndpointAsync(string askQuery, RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpointAsync(askQuery, sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Asynchronously applies the given raw string ASK query to the given SPARQL endpoint
        /// </summary>
        public static Task<RDFAskQueryResult> ApplyRawToSPARQLEndpointAsync(string askQuery, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => Task.Run(() => ApplyRawToSPARQLEndpoint(askQuery, sparqlEndpoint, sparqlEndpointQueryOptions));
        #endregion
    }
}