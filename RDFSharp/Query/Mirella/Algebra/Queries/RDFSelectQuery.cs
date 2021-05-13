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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

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
        {
            if (patternGroup != null)
            {
                if (!this.GetPatternGroups().Any(q => q.Equals(patternGroup)))
                    this.QueryMembers.Add(patternGroup);
            }
            return this;
        }

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
        public RDFSelectQuery AddSubQuery(RDFSelectQuery subQuery)
        {
            if (subQuery != null && !this.Equals(subQuery))
            {
                if (!this.GetSubQueries().Any(q => q.Equals(subQuery)))
                    this.QueryMembers.Add(subQuery.SubQuery());
            }
            return this;
        }

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
        {
            string selectQueryString = this.ToString();
            RDFSelectQueryResult selResult = new RDFSelectQueryResult();
            if (sparqlEndpoint != null)
            {
                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient())
                {
                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(selectQueryString));

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
                            selResult = RDFSelectQueryResult.FromSparqlXmlResult(sStream);
                        selResult.SelectResults.TableName = selectQueryString;
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
            => Task.Run(() => this.ApplyToSPARQLEndpoint(sparqlEndpoint));

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