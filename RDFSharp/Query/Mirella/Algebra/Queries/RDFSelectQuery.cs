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

using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
        /// Flag indicating the query to be joined as Optional
        /// </summary>
        internal bool IsOptional { get; set; }

        /// <summary>
        /// Flag indicating the query to be joined as Union
        /// </summary>
        internal bool JoinAsUnion { get; set; }

        /// <summary>
        /// Flag indicating that the query is a subquery
        /// </summary>
        internal bool IsSubQuery { get; set; }

        /// <summary>
        /// Dictionary of projection variables and associated ordinals
        /// </summary>
        internal Dictionary<RDFVariable, (int, RDFExpression)> ProjectionVars { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SELECT query
        /// </summary>
        public RDFSelectQuery()
            => ProjectionVars = new Dictionary<RDFVariable, (int, RDFExpression)>();
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
        /// Adds the given variable to the results of the query (it may come from evaluation of an expression, if specified)
        /// </summary>
        public RDFSelectQuery AddProjectionVariable(RDFVariable projectionVariable, RDFExpression projectionExpression=null)
        {
            if (projectionVariable != null)
            {
                if (!ProjectionVars.Any(pv => pv.Key.Equals(projectionVariable)))
                    ProjectionVars.Add(projectionVariable, (ProjectionVars.Count, projectionExpression));
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
                List<RDFModifier> modifiers = GetModifiers().ToList();

                //Ensure to have only one groupby modifier in the query
                if (modifier is RDFGroupByModifier && modifiers.Any(m => m is RDFGroupByModifier))
                    return this;

                //Ensure to have only one distinct modifier in the query
                if (modifier is RDFDistinctModifier && modifiers.Any(m => m is RDFDistinctModifier))
                    return this;

                //Ensure to have only one limit modifier in the query
                if (modifier is RDFLimitModifier && modifiers.Any(m => m is RDFLimitModifier))
                    return this;

                //Ensure to have only one offset modifier in the query
                if (modifier is RDFOffsetModifier && modifiers.Any(m => m is RDFOffsetModifier))
                    return this;

                //Ensure to have only one orderby modifier per variable in the query
                if (modifier is RDFOrderByModifier obm && modifiers.Any(m => m is RDFOrderByModifier om && om.Variable.Equals(obm.Variable)))
                    return this;

                QueryMembers.Add(modifier);
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
            => graph != null ? new RDFQueryEngine().EvaluateSelectQuery(this, graph) : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given asynchronous graph
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToGraphAsync(RDFAsyncGraph asyncGraph)
            => Task.Run(() => ApplyToGraph(asyncGraph?.WrappedGraph));

        /// <summary>
        /// Applies the query to the given store
        /// </summary>
        public RDFSelectQueryResult ApplyToStore(RDFStore store)
            => store != null ? new RDFQueryEngine().EvaluateSelectQuery(this, store) : new RDFSelectQueryResult();

        /// <summary>
        /// Applies the query to the given asynchronous store
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToStoreAsync(RDFAsyncStore asyncStore)
            => Task.Run(() => ApplyToStore(asyncStore?.WrappedStore));

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFSelectQueryResult ApplyToFederation(RDFFederation federation)
            => federation != null ? new RDFQueryEngine().EvaluateSelectQuery(this, federation) : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToFederationAsync(RDFFederation federation)
            => Task.Run(() => ApplyToFederation(federation));

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFSelectQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpoint(ToString(), sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFSelectQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => ApplyRawToSPARQLEndpoint(ToString(), sparqlEndpoint, sparqlEndpointQueryOptions);

        /// <summary>
        /// Applies the given raw string SELECT query to the given SPARQL endpoint
        /// </summary>
        public static RDFSelectQueryResult ApplyRawToSPARQLEndpoint(string selectQuery, RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpoint(selectQuery, sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Applies the given raw string SELECT query to the given SPARQL endpoint
        /// </summary>
        public static RDFSelectQueryResult ApplyRawToSPARQLEndpoint(string selectQuery, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => sparqlEndpoint != null ? (RDFSelectQueryResult)new RDFQueryEngine().ApplyRawToSPARQLEndpoint("SELECT", selectQuery, sparqlEndpoint, sparqlEndpointQueryOptions) : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpointAsync(ToString(), sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFSelectQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => ApplyRawToSPARQLEndpointAsync(ToString(), sparqlEndpoint, sparqlEndpointQueryOptions);

        /// <summary>
        /// Asynchronously applies the given raw string SELECT query to the given SPARQL endpoint
        /// </summary>
        public static Task<RDFSelectQueryResult> ApplyRawToSPARQLEndpointAsync(string selectQuery, RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpointAsync(selectQuery, sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Asynchronously applies the given raw string SELECT query to the given SPARQL endpoint
        /// </summary>
        public static Task<RDFSelectQueryResult> ApplyRawToSPARQLEndpointAsync(string selectQuery, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => Task.Run(() => ApplyRawToSPARQLEndpoint(selectQuery, sparqlEndpoint, sparqlEndpointQueryOptions));

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
                        return ApplyToGraph(graph);
                    case RDFStore store:
                        return ApplyToStore(store);
                    case RDFFederation federation:
                        return ApplyToFederation(federation);
                    case RDFSPARQLEndpoint sparqlEndpoint:
                        return ApplyToSPARQLEndpoint(sparqlEndpoint);
                }
            }
            return new RDFSelectQueryResult();
        }

        /// <summary>
        /// Asynchronously applies the query to the given data source
        /// </summary>
        internal Task<RDFSelectQueryResult> ApplyToDataSourceAsync(RDFDataSource dataSource)
            => Task.Run(() => ApplyToDataSource(dataSource));

        /// <summary>
        /// Sets the query to be joined as Optional with the previous query member
        /// </summary>
        public RDFSelectQuery Optional()
        {
            IsOptional = true;
            JoinAsUnion = false;
            return this;
        }

        /// <summary>
        /// Sets the query to be joined as Union with the next query member
        /// </summary>
        public RDFSelectQuery UnionWithNext()
        {
            IsOptional = false;
            JoinAsUnion = true;
            return this;
        }

        /// <summary>
        /// Sets the query as a subquery
        /// </summary>
        internal RDFSelectQuery SubQuery()
        {
            IsSubQuery = true;
            return this;
        }
        #endregion
    }
}