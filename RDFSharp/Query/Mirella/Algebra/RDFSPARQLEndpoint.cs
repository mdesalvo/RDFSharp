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
            if (baseAddress == null)
                throw new RDFQueryException("Cannot create RDFSPARQLEndpoint because given \"baseAddress\" parameter is null.");

            this.BaseAddress = baseAddress;
            this.QueryParams = new NameValueCollection();
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

    /// <summary>
    /// RDFSPARQLEndpointQueryOptions customizes the default behavior of a SPARQL endpoint query
    /// </summary>
    public class RDFSPARQLEndpointQueryOptions
    {
        #region Properties
        /// <summary>
        /// Represents the timeout observed for the query sent to the SPARQL endpoint (defaults to: -1)
        /// </summary>
        public int TimeoutMilliseconds { get; set; }

        /// <summary>
        /// Represents the behavior used by the query in case of runtime errors (defaults to: ThrowException)
        /// </summary>
        public RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors ErrorBehavior { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to configure options for a SPARQL endpoint query
        /// </summary>
        public RDFSPARQLEndpointQueryOptions()
        {
            this.TimeoutMilliseconds = -1;
            this.ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException;
        }

        /// <summary>
        /// Custom-ctor to configure options for a SPARQL endpoint query
        /// </summary>
        public RDFSPARQLEndpointQueryOptions(int timeoutMilliseconds) : this()
            => this.TimeoutMilliseconds = timeoutMilliseconds < -1 ? -1 : timeoutMilliseconds;

        /// <summary>
        /// Custom-ctor to configure options for a SPARQL endpoint query
        /// </summary>
        public RDFSPARQLEndpointQueryOptions(int timeoutMilliseconds, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors errorBehavior) : this(timeoutMilliseconds)
            => this.ErrorBehavior = errorBehavior;
        #endregion
    }

    /// <summary>
    /// RDFSPARQLEndpointOperationOptions customizes the default behavior of a SPARQL UPDATE endpoint operation
    /// </summary>
    public class RDFSPARQLEndpointOperationOptions
    {
        #region Properties
        /// <summary>
        /// Represents the Content-Type header to be used when posting the operation to the SPARQL UPDATE endpoint (defaults to: application/sparql-update)
        /// </summary>
        public RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes RequestContentType { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to configure options for a SPARQL UPDATE endpoint operation
        /// </summary>
        public RDFSPARQLEndpointOperationOptions()
            => this.RequestContentType = RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.Sparql_Update;

        /// <summary>
        /// Custom-ctor to configure options for a SPARQL UPDATE endpoint operation
        /// </summary>
        public RDFSPARQLEndpointOperationOptions(RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes requestContentType) : this()
            => this.RequestContentType = requestContentType;
        #endregion
    }
}