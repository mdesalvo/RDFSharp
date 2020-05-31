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
using System.Collections;
using System.Collections.Generic;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFFederation represents a logically integrated collection of RDF data sources
    /// </summary>
    public sealed class RDFFederation : RDFDataSource, IEquatable<RDFFederation>, IEnumerable<RDFDataSource>
    {

        #region Properties
        /// <summary>
        /// Name of the federation
        /// </summary>
        public String FederationName { get; internal set; }

        /// <summary>
        /// Count of the federation's data sources
        /// </summary>
        public Int32 DataSourcesCount
        {
            get { return this.DataSources.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the federation's data sources for iteration
        /// </summary>
        public IEnumerator<RDFDataSource> DataSourcesEnumerator
        {
            get { return this.DataSources.Values.GetEnumerator(); }
        }

        /// <summary>
        /// List of data sources of the federation
        /// </summary>
        internal Dictionary<Int64, RDFDataSource> DataSources { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty named federation
        /// </summary>
        public RDFFederation(String federationName)
        {
            this.FederationName = "FEDERATION|ID=" + federationName ?? Guid.NewGuid().ToString("N");
            this.DataSources = new Dictionary<Int64, RDFDataSource>();
            this.DataSourceID = RDFModelUtilities.CreateHash(this.FederationName);
        }

        /// <summary>
        /// Default ctor to build an empty federation
        /// </summary>
        public RDFFederation() : this(Guid.NewGuid().ToString("N")) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the federation
        /// </summary>
        public override String ToString()
        {
            return this.FederationName;
        }

        /// <summary>
        /// Performs the equality comparison between two federations
        /// </summary>
        public Boolean Equals(RDFFederation other)
        {
            if (other == null || this.DataSourcesCount != other.DataSourcesCount)
            {
                return false;
            }
            foreach (RDFDataSource dataSource in this)
            {
                if (!other.DataSources.ContainsKey(dataSource.DataSourceID))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Exposes a typed enumerator on the federation's data sources
        /// </summary>
        IEnumerator<RDFDataSource> IEnumerable<RDFDataSource>.GetEnumerator()
        {
            return this.DataSourcesEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the federation's data sources
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.DataSourcesEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given graph to the federation, avoiding duplicate insertions
        /// </summary>
        public RDFFederation AddGraph(RDFGraph graph)
        {
            if (graph != null)
            {
                if (!this.DataSources.ContainsKey(graph.DataSourceID))
                {
                    this.DataSources.Add(graph.DataSourceID, graph);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given store to the federation, avoiding duplicate insertions
        /// </summary>
        public RDFFederation AddStore(RDFStore store)
        {
            if (store != null)
            {
                if (!this.DataSources.ContainsKey(store.DataSourceID))
                {
                    this.DataSources.Add(store.DataSourceID, store);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given federation to the federation, avoiding duplicate insertions
        /// </summary>
        public RDFFederation AddFederation(RDFFederation federation)
        {
            if (federation != null)
            {
                if (!this.DataSources.ContainsKey(federation.DataSourceID))
                {
                    this.DataSources.Add(federation.DataSourceID, federation);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given SPARQL endpoint to the federation, avoiding duplicate insertions
        /// </summary>
        public RDFFederation AddSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
        {
            if (sparqlEndpoint != null)
            {
                if (!this.DataSources.ContainsKey(sparqlEndpoint.DataSourceID))
                {
                    this.DataSources.Add(sparqlEndpoint.DataSourceID, sparqlEndpoint);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given graph from the federation 
        /// </summary>
        public RDFFederation RemoveGraph(RDFGraph graph)
        {
            if (graph != null)
            {
                if (this.DataSources.ContainsKey(graph.DataSourceID))
                {
                    this.DataSources.Remove(graph.DataSourceID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given store from the federation 
        /// </summary>
        public RDFFederation RemoveStore(RDFStore store)
        {
            if (store != null)
            {
                if (this.DataSources.ContainsKey(store.DataSourceID))
                {
                    this.DataSources.Remove(store.DataSourceID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given federation from the federation 
        /// </summary>
        public RDFFederation RemoveFederation(RDFFederation federation)
        {
            if (federation != null)
            {
                if (this.DataSources.ContainsKey(federation.DataSourceID))
                {
                    this.DataSources.Remove(federation.DataSourceID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given SPARQL endpoint from the federation 
        /// </summary>
        public RDFFederation RemoveSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
        {
            if (sparqlEndpoint != null)
            {
                if (this.DataSources.ContainsKey(sparqlEndpoint.DataSourceID))
                {
                    this.DataSources.Remove(sparqlEndpoint.DataSourceID);
                }
            }
            return this;
        }

        /// <summary>
        /// Clears the data sources of the federation
        /// </summary>
        public void ClearDataSources()
        {
            this.DataSources.Clear();
        }
        #endregion

        #endregion

    }

}