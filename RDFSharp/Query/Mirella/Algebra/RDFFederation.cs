/*
   Copyright 2012-2026 Marco De Salvo

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

using System;
using System.Collections;
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFFederation represents a virtually integrated collection of RDF data sources
    /// </summary>
    public sealed class RDFFederation : RDFDataSource, IEnumerable<RDFDataSource>
    {
        #region Properties
        /// <summary>
        /// Name of the federation
        /// </summary>
        public string FederationName { get; }

        /// <summary>
        /// Count of the federation's data sources
        /// </summary>
        public int DataSourcesCount
            => DataSources.Count;

        /// <summary>
        /// Gets the enumerator on the federation's data sources for iteration
        /// </summary>
        public IEnumerator<RDFDataSource> DataSourcesEnumerator
            => DataSources.GetEnumerator();

        /// <summary>
        /// List of data sources of the federation
        /// </summary>
        internal List<RDFDataSource> DataSources { get; set; }

        /// <summary>
        /// Dictionary of query options for the SPARQL endpoint data sources of the federation
        /// </summary>
        internal Dictionary<string, RDFSPARQLEndpointQueryOptions> EndpointDataSourcesQueryOptions { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an empty federation
        /// </summary>
        public RDFFederation()
        {
            FederationName = $"FEDERATION|ID={Guid.NewGuid():N}";
            DataSources = new List<RDFDataSource>();
            EndpointDataSourcesQueryOptions = new Dictionary<string, RDFSPARQLEndpointQueryOptions>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the federation
        /// </summary>
        public override string ToString()
            => FederationName;

        /// <summary>
        /// Performs the equality comparison between two federations, based on their name
        /// </summary>
        public override bool Equals(object obj)
            => obj is RDFFederation other && string.Equals(FederationName, other.FederationName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Calculates the hashcode of this federation
        /// </summary>
        public override int GetHashCode()
            => StringComparer.OrdinalIgnoreCase.GetHashCode(FederationName);

        /// <summary>
        /// Exposes a typed enumerator on the federation's data sources
        /// </summary>
        IEnumerator<RDFDataSource> IEnumerable<RDFDataSource>.GetEnumerator()
            => DataSourcesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the federation's data sources
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => DataSourcesEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given graph to the federation
        /// </summary>
        public RDFFederation AddGraph(RDFGraph graph)
        {
            if (graph != null)
                DataSources.Add(graph);
            return this;
        }

        /// <summary>
        /// Adds the given store to the federation
        /// </summary>
        public RDFFederation AddStore(RDFStore store)
        {
            if (store != null)
                DataSources.Add(store);
            return this;
        }

        /// <summary>
        /// Adds the given federation to the federation
        /// </summary>
        public RDFFederation AddFederation(RDFFederation federation)
        {
            #region Utilities
            //Recursively tells whether 'candidate' is this same federation, or (at any nesting depth) already
            //contains it among its own federation members: linking 'federation' in that case would close a cycle,
            //which the recursive traversals of the query/path engines cannot survive (they are not cycle-proof).
            bool WouldIntroduceCycle(RDFFederation candidate)
            {
                if (candidate.Equals(this))
                    return true;
                foreach (RDFDataSource member in candidate.DataSources)
                    if (member is RDFFederation nestedFederation && WouldIntroduceCycle(nestedFederation))
                        return true;
                return false;
            }
            #endregion

            if (federation != null && !WouldIntroduceCycle(federation))
                DataSources.Add(federation);
            return this;
        }

        /// <summary>
        /// Adds the given SPARQL endpoint to the federation
        /// </summary>
        public RDFFederation AddSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions=null)
        {
            string sparqlEndpointString = sparqlEndpoint?.ToString();
            if (sparqlEndpoint != null && !EndpointDataSourcesQueryOptions.ContainsKey(sparqlEndpointString))
            {
                DataSources.Add(sparqlEndpoint);
                EndpointDataSourcesQueryOptions.Add(sparqlEndpointString, sparqlEndpointQueryOptions ?? new RDFSPARQLEndpointQueryOptions());
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Clears the data sources of the federation
        /// </summary>
        public void ClearDataSources()
        {
            DataSources.Clear();
            EndpointDataSourcesQueryOptions.Clear();
        }
        #endregion

        #endregion
    }
}