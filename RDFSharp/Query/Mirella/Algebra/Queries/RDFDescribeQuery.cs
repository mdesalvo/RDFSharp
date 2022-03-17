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
    /// RDFDescribeQuery is the SPARQL "DESCRIBE" query implementation.
    /// </summary>
    public class RDFDescribeQuery : RDFQuery
    {
        #region Properties
        /// <summary>
        /// List of RDF terms to be described by the query
        /// </summary>
        internal List<RDFPatternMember> DescribeTerms { get; set; }

        /// <summary>
        /// List of variables carried by the template patterns of the query
        /// </summary>
        internal List<RDFVariable> Variables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty DESCRIBE query
        /// </summary>
        public RDFDescribeQuery()
        {
            this.DescribeTerms = new List<RDFPatternMember>();
            this.Variables = new List<RDFVariable>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the DESCRIBE query
        /// </summary>
        public override string ToString()
            => RDFQueryPrinter.PrintDescribeQuery(this);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given resource to the describe terms of the query
        /// </summary>
        public RDFDescribeQuery AddDescribeTerm(RDFResource describeTerm)
        {
            if (describeTerm != null)
            {
                if (!this.DescribeTerms.Any(dt => dt.Equals(describeTerm)))
                    this.DescribeTerms.Add(describeTerm);
            }
            return this;
        }

        /// <summary>
        /// Adds the given variable to the describe terms of the query
        /// </summary>
        public RDFDescribeQuery AddDescribeTerm(RDFVariable describeVar)
        {
            if (describeVar != null)
            {
                if (!this.DescribeTerms.Any(dt => dt.Equals(describeVar)))
                {
                    this.DescribeTerms.Add(describeVar);

                    //Collect the variable
                    if (!this.Variables.Any(v => v.Equals(describeVar)))
                        this.Variables.Add(describeVar);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFDescribeQuery AddPatternGroup(RDFPatternGroup patternGroup)
            => AddPatternGroup<RDFDescribeQuery>(patternGroup);

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFDescribeQuery AddModifier(RDFDistinctModifier modifier)
            => AddModifier<RDFDescribeQuery>(modifier);

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFDescribeQuery AddModifier(RDFLimitModifier modifier)
            => AddModifier<RDFDescribeQuery>(modifier);

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFDescribeQuery AddModifier(RDFOffsetModifier modifier)
            => AddModifier<RDFDescribeQuery>(modifier);

        /// <summary>
        /// Adds the given prefix declaration to the query
        /// </summary>
        public RDFDescribeQuery AddPrefix(RDFNamespace prefix)
            => AddPrefix<RDFDescribeQuery>(prefix);

        /// <summary>
        /// Adds the given subquery to the query
        /// </summary>
        public RDFDescribeQuery AddSubQuery(RDFSelectQuery subQuery)
            => AddSubQuery<RDFDescribeQuery>(subQuery);

        /// <summary>
        /// Applies the query to the given graph
        /// </summary>
        public RDFDescribeQueryResult ApplyToGraph(RDFGraph graph)
            => graph != null ? new RDFQueryEngine().EvaluateDescribeQuery(this, graph)
                             : new RDFDescribeQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => this.ApplyToGraph(graph));

        /// <summary>
        /// Applies the query to the given store
        /// </summary>
        public RDFDescribeQueryResult ApplyToStore(RDFStore store)
            => store != null ? new RDFQueryEngine().EvaluateDescribeQuery(this, store)
                             : new RDFDescribeQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given store
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToStoreAsync(RDFStore store)
            => Task.Run(() => this.ApplyToStore(store));

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFDescribeQueryResult ApplyToFederation(RDFFederation federation)
            => federation != null ? new RDFQueryEngine().EvaluateDescribeQuery(this, federation)
                                  : new RDFDescribeQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToFederationAsync(RDFFederation federation)
            => Task.Run(() => this.ApplyToFederation(federation));

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFDescribeQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
            => this.ApplyToSPARQLEndpoint(sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFDescribeQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult();
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
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                    webClient.Headers.Add(HttpRequestHeader.Accept, "text/turtle");

                    //Send querystring to SPARQL endpoint
                    byte[] sparqlResponse = default;
                    try
                    {
                        sparqlResponse = webClient.DownloadData(sparqlEndpoint.BaseAddress);
                    }
                    catch (Exception ex)
                    {
                        if (sparqlEndpointQueryOptions.ErrorBehavior == RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)
                            throw new RDFQueryException($"DESCRIBE query on SPARQL endpoint failed because: {ex.Message}", ex);
                    }

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                            describeResult = RDFDescribeQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                        describeResult.DescribeResults.TableName = sparqlQuery;
                    }
                }

                //Eventually adjust column names (should start with "?")
                int columnsCount = describeResult.DescribeResults.Columns.Count;
                for (int i = 0; i < columnsCount; i++)
                {
                    if (!describeResult.DescribeResults.Columns[i].ColumnName.StartsWith("?"))
                        describeResult.DescribeResults.Columns[i].ColumnName = string.Concat("?", describeResult.DescribeResults.Columns[i].ColumnName);
                }
            }
            return describeResult;
        }

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint)
            => this.ApplyToSPARQLEndpointAsync(sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => Task.Run(() => this.ApplyToSPARQLEndpoint(sparqlEndpoint, sparqlEndpointQueryOptions));
        #endregion
    }
}