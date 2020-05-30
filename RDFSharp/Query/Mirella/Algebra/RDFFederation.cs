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
using System;
using System.Collections;
using System.Collections.Generic;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFSPARQLFederation represents a logically integrated collection of SPARQL endpoints
    /// </summary>
    public sealed class RDFSPARQLFederation : RDFDataSource, IEquatable<RDFSPARQLFederation>, IEnumerable<RDFSPARQLEndpoint>
    {

        #region Properties
        /// <summary>
        /// Name of the federation
        /// </summary>
        public String FederationName { get; internal set; }

        /// <summary>
        /// Count of the federation's endpoints
        /// </summary>
        public Int32 EndpointsCount
        {
            get { return this.Endpoints.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the federation's endpoints for iteration
        /// </summary>
        public IEnumerator<RDFSPARQLEndpoint> EndpointsEnumerator
        {
            get { return this.Endpoints.Values.GetEnumerator(); }
        }

        /// <summary>
        /// List of endpoints embedded into the federation
        /// </summary>
        internal Dictionary<Int64, RDFSPARQLEndpoint> Endpoints { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty named federation
        /// </summary>
        public RDFSPARQLFederation(String federationName)
        {
            this.FederationName = "FEDERATION|ID=" + federationName ?? Guid.NewGuid().ToString("N");
            this.Endpoints = new Dictionary<Int64, RDFSPARQLEndpoint>();
        }

        /// <summary>
        /// Default ctor to build an empty federation
        /// </summary>
        public RDFSPARQLFederation() : this(Guid.NewGuid().ToString("N")) { }
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
        public Boolean Equals(RDFSPARQLFederation other)
        {
            if (other == null || this.EndpointsCount != other.EndpointsCount)
            {
                return false;
            }
            foreach (RDFSPARQLEndpoint sparqlEndpoint in this)
            {
                if (!other.Endpoints.ContainsKey(sparqlEndpoint.EndpointID))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Exposes a typed enumerator on the federation's endpoints
        /// </summary>
        IEnumerator<RDFSPARQLEndpoint> IEnumerable<RDFSPARQLEndpoint>.GetEnumerator()
        {
            return this.EndpointsEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the federation's endpoints
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.EndpointsEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given endpoint to the federation, avoiding duplicate insertions
        /// </summary>
        public RDFSPARQLFederation AddEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
        {
            if (sparqlEndpoint != null)
            {
                if (!this.Endpoints.ContainsKey(sparqlEndpoint.EndpointID))
                {
                    this.Endpoints.Add(sparqlEndpoint.EndpointID, sparqlEndpoint);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given endpoint from the federation 
        /// </summary>
        public RDFSPARQLFederation RemoveEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
        {
            if (sparqlEndpoint != null)
            {
                if (this.Endpoints.ContainsKey(sparqlEndpoint.EndpointID))
                {
                    this.Endpoints.Remove(sparqlEndpoint.EndpointID);
                }
            }
            return this;
        }

        /// <summary>
        /// Clears the endpoints of the federation
        /// </summary>
        public void ClearEndpoints()
        {
            this.Endpoints.Clear();
        }
        #endregion

        #endregion

    }

}