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
    public sealed class RDFFederation : RDFDataSource, IEnumerable<RDFDataSource>
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
            get { return this.DataSources.GetEnumerator(); }
        }

        /// <summary>
        /// List of data sources of the federation
        /// </summary>
        internal List<RDFDataSource> DataSources { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty federation
        /// </summary>
        public RDFFederation()
        {
            this.FederationName = "FEDERATION|ID=" + Guid.NewGuid().ToString("N");
            this.DataSources = new List<RDFDataSource>();
        }
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
                this.DataSources.Add(graph);
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
                this.DataSources.Add(store);
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
                this.DataSources.Add(federation);
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
                this.DataSources.Add(sparqlEndpoint);
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
            this.DataSources.Clear();
        }
        #endregion

        #endregion

    }

}