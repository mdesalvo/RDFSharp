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
using System.Collections.Specialized;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFSPARQLEndpoint represents a SPARQL endpoint
    /// </summary>
    public class RDFSPARQLEndpoint : RDFDataSource
    {

        #region Properties
        /// <summary>
        /// Base address of the SPARQL endpoint
        /// </summary>
        public Uri BaseAddress { get; internal set; }

        /// <summary>
        /// Collection of query params sent to the SPARQL endpoint
        /// </summary>
        internal NameValueCollection QueryParams { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a SPARQL enpoint with given base address
        /// </summary>
        public RDFSPARQLEndpoint(Uri baseAddress)
        {
            if (baseAddress != null)
            {
                this.BaseAddress = baseAddress;
                this.QueryParams = new NameValueCollection();
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFSPARQLEndpoint because given \"baseAddress\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the variable
        /// </summary>
        public override string ToString()
            => this.BaseAddress.ToString();
        #endregion

        #region Methods
        /// <summary>
        /// Adds a "default-graph-uri" parameter to be sent to the SPARQL endpoint
        /// </summary>
        public RDFSPARQLEndpoint AddDefaultGraphUri(string defaultGraphUri)
        {
            this.QueryParams.Add("default-graph-uri", defaultGraphUri ?? string.Empty);
            return this;
        }

        /// <summary>
        /// Adds a "named-graph-uri" parameter to be sent to the SPARQL endpoint
        /// </summary>
        public RDFSPARQLEndpoint AddNamedGraphUri(string namedGraphUri)
        {
            this.QueryParams.Add("named-graph-uri", namedGraphUri ?? string.Empty);
            return this;
        }
        #endregion

    }

}