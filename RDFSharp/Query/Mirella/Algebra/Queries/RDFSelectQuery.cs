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

using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFSelectQuery is the SPARQL "SELECT" query implementation.
    /// </summary>
    public class RDFSelectQuery : RDFQuery
    {
        #region Properties
        /// <summary>
        /// Dictionary of projection variables and associated ordinals
        /// </summary>
        internal Dictionary<RDFVariable, int> ProjectionVars { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SELECT query
        /// </summary>
        public RDFSelectQuery()
            => this.ProjectionVars = new Dictionary<RDFVariable, int>();
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SELECT query
        /// </summary>
        public override string ToString()
            => RDFQueryPrinter.PrintSelectQuery(this, 0, false);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFSelectQuery AddPatternGroup(RDFPatternGroup patternGroup)
            => AddPatternGroup<RDFSelectQuery>(patternGroup);

        /// <summary>
        /// Adds the given variable to the results of the query
        /// </summary>
        public RDFSelectQuery AddProjectionVariable(RDFVariable projectionVariable)
        {
            if (projectionVariable != null)
            {
                if (!this.ProjectionVars.Any(pv => pv.Key.ToString().Equals(projectionVariable.ToString(), StringComparison.OrdinalIgnoreCase)))
                    this.ProjectionVars.Add(projectionVariable, this.ProjectionVars.Count);
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFSelectQuery AddModifier(RDFModifier modifier)
        {
            if (modifier != null)
            {
                //Ensure to have only one groupby modifier in the query
                if (modifier is RDFGroupByModifier && this.GetModifiers().Any(m => m is RDFGroupByModifier))
                    return this;

                //Ensure to have only one distinct modifier in the query
                if (modifier is RDFDistinctModifier && this.GetModifiers().Any(m => m is RDFDistinctModifier))
                    return this;

                //Ensure to have only one limit modifier in the query
                if (modifier is RDFLimitModifier && this.GetModifiers().Any(m => m is RDFLimitModifier))
                    return this;

                //Ensure to have only one offset modifier in the query
                if (modifier is RDFOffsetModifier && this.GetModifiers().Any(m => m is RDFOffsetModifier))
                    return this;

                //Ensure to have only one orderby modifier per variable in the query
                if (modifier is RDFOrderByModifier && this.GetModifiers().Any(m => m is RDFOrderByModifier && ((RDFOrderByModifier)m).Variable.Equals(((RDFOrderByModifier)modifier).Variable)))
                    return this;

                //Add the modifier, avoiding duplicates
                if (!this.GetModifiers().Any(m => m.Equals(modifier)))
                    this.QueryMembers.Add(modifier);
            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the query
        /// </summary>
        public RDFSelectQuery AddPrefix(RDFNamespace prefix)
            => AddPrefix<RDFSelectQuery>(prefix);

        /// <summary>
        /// Adds the given subquery to the query
        /// </summary>
        public RDFSelectQuery AddSubQuery(RDFSelectQuery subQuery)
            => AddSubQuery<RDFSelectQuery>(subQuery);

        /// <summary>
        /// Applies the query to the given graph
        /// </summary>
        public RDFSelectQueryResult ApplyToGraph(RDFGraph graph)
            => graph != null ? new RDFQueryEngine().EvaluateSelectQuery(this, graph)
                             : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => this.ApplyToGraph(graph));

        /// <summary>
        /// Applies the query to the given store
        /// </summary>
        public RDFSelectQueryResult ApplyToStore(RDFStore store)
            => store != null ? new RDFQueryEngine().EvaluateSelectQuery(this, store)
                             : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given store
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToStoreAsync(RDFStore store)
            => Task.Run(() => this.ApplyToStore(store));

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFSelectQueryResult ApplyToFederation(RDFFederation federation)
            => federation != null ? new RDFQueryEngine().EvaluateSelectQuery(this, federation)
                                  : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToFederationAsync(RDFFederation federation)
            => Task.Run(() => this.ApplyToFederation(federation));

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFSelectQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
            => this.ApplyToSPARQLEndpoint(sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFSelectQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
        {
            RDFSelectQueryResult selResult = new RDFSelectQueryResult();
            if (sparqlEndpoint != null)
            {
                if (sparqlEndpointQueryOptions == null)
                    sparqlEndpointQueryOptions = new RDFSPARQLEndpointQueryOptions();

                //Establish a connection to the given SPARQL endpoint
                using (RDFWebClient webClient = new RDFWebClient(sparqlEndpointQueryOptions.TimeoutMilliseconds))
                {
                    //Insert reserved "query" parameter
                    string sparqlQuery = this.ToString();
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(sparqlQuery));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/sparql-results+xml");

                    //Send querystring to SPARQL endpoint
                    byte[] sparqlResponse = default;
                    try
                    {
                        sparqlResponse = webClient.DownloadData(sparqlEndpoint.BaseAddress);
                    }
                    catch (Exception ex)
                    {
                        if (sparqlEndpointQueryOptions.ErrorBehavior == RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)
                            throw new RDFQueryException($"SELECT query on SPARQL endpoint failed because: {ex.Message}", ex);
                    }

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                            selResult = RDFSelectQueryResult.FromSparqlXmlResult(sStream);
                        selResult.SelectResults.TableName = sparqlQuery;
                    }
                }

                //Eventually adjust column names (should start with "?")
                int columnsCount = selResult.SelectResults.Columns.Count;
                for (int i = 0; i < columnsCount; i++)
                {
                    if (!selResult.SelectResults.Columns[i].ColumnName.StartsWith("?"))
                        selResult.SelectResults.Columns[i].ColumnName = string.Concat("?", selResult.SelectResults.Columns[i].ColumnName);
                }
            }
            return selResult;
        }

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint)
            => this.ApplyToSPARQLEndpointAsync(sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => Task.Run(() => this.ApplyToSPARQLEndpoint(sparqlEndpoint, sparqlEndpointQueryOptions));

        /// <summary>
        /// Applies the query to the given data source
        /// </summary>
        internal RDFSelectQueryResult ApplyToDataSource(RDFDataSource dataSource)
        {
            if (dataSource != null)
            {
                switch (dataSource)
                {
                    case RDFGraph graph:
                        return this.ApplyToGraph(graph);
                    case RDFStore store:
                        return this.ApplyToStore(store);
                    case RDFFederation federation:
                        return this.ApplyToFederation(federation);
                    case RDFSPARQLEndpoint sparqlEndpoint:
                        return this.ApplyToSPARQLEndpoint(sparqlEndpoint);
                }
            }
            return new RDFSelectQueryResult();
        }

        /// <summary>
        /// Asynchronously applies the query to the given data source
        /// </summary>
        internal Task<RDFSelectQueryResult> ApplyToDataSourceAsync(RDFDataSource dataSource)
            => Task.Run(() => this.ApplyToDataSource(dataSource));

        /// <summary>
        /// Sets the query to be joined as optional with the previous query member
        /// </summary>
        public RDFSelectQuery Optional()
        {
            this.IsOptional = true;
            return this;
        }

        /// <summary>
        /// Sets the query to be joined as union with the next query member
        /// </summary>
        public RDFSelectQuery UnionWithNext()
        {
            this.JoinAsUnion = true;
            return this;
        }
        #endregion
    }
}