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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Store;

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
            DescribeTerms = new List<RDFPatternMember>();
            Variables = new List<RDFVariable>();
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
                if (!DescribeTerms.Any(dt => dt.Equals(describeTerm)))
                    DescribeTerms.Add(describeTerm);
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
                if (!DescribeTerms.Any(dt => dt.Equals(describeVar)))
                {
                    DescribeTerms.Add(describeVar);

                    //Collect the variable
                    if (!Variables.Any(v => v.Equals(describeVar)))
                        Variables.Add(describeVar);
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
            => graph != null ? new RDFQueryEngine().EvaluateDescribeQuery(this, graph) : new RDFDescribeQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => ApplyToGraph(graph));

        /// <summary>
        /// Applies the query to the given store
        /// </summary>
        public RDFDescribeQueryResult ApplyToStore(RDFStore store)
            => store != null ? new RDFQueryEngine().EvaluateDescribeQuery(this, store) : new RDFDescribeQueryResult();

        /// <summary>
        /// Applies the query to the given asynchronous store
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToStoreAsync(RDFStore store)
            => Task.Run(() => ApplyToStore(store));

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFDescribeQueryResult ApplyToFederation(RDFFederation federation)
            => federation != null ? new RDFQueryEngine().EvaluateDescribeQuery(this, federation) : new RDFDescribeQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToFederationAsync(RDFFederation federation)
            => Task.Run(() => ApplyToFederation(federation));

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFDescribeQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpoint(ToString(), sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFDescribeQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => ApplyRawToSPARQLEndpoint(ToString(), sparqlEndpoint, sparqlEndpointQueryOptions);

        /// <summary>
        /// Applies the given raw string DESCRIBE query to the given SPARQL endpoint
        /// </summary>
        public static RDFDescribeQueryResult ApplyRawToSPARQLEndpoint(string describeQuery, RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpoint(describeQuery, sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Applies the given raw string DESCRIBE query to the given SPARQL endpoint
        /// </summary>
        public static RDFDescribeQueryResult ApplyRawToSPARQLEndpoint(string describeQuery, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => sparqlEndpoint != null ? (RDFDescribeQueryResult)new RDFQueryEngine().ApplyRawToSPARQLEndpoint("DESCRIBE", describeQuery, sparqlEndpoint, sparqlEndpointQueryOptions) : new RDFDescribeQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpointAsync(ToString(), sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public Task<RDFDescribeQueryResult> ApplyToSPARQLEndpointAsync(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => ApplyRawToSPARQLEndpointAsync(ToString(), sparqlEndpoint, sparqlEndpointQueryOptions);

        /// <summary>
        /// Asynchronously applies the given raw string DESCRIBE query to the given SPARQL endpoint
        /// </summary>
        public static Task<RDFDescribeQueryResult> ApplyRawToSPARQLEndpointAsync(string describeQuery, RDFSPARQLEndpoint sparqlEndpoint)
            => ApplyRawToSPARQLEndpointAsync(describeQuery, sparqlEndpoint, new RDFSPARQLEndpointQueryOptions());

        /// <summary>
        /// Asynchronously applies the given raw string DESCRIBE query to the given SPARQL endpoint
        /// </summary>
        public static Task<RDFDescribeQueryResult> ApplyRawToSPARQLEndpointAsync(string describeQuery, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
            => Task.Run(() => ApplyRawToSPARQLEndpoint(describeQuery, sparqlEndpoint, sparqlEndpointQueryOptions));
        #endregion
    }
}