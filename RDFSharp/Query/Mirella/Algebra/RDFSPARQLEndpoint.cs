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

using System;
using System.Collections.Specialized;
using System.Net;
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFSPARQLEndpoint represents a SPARQL endpoint
    /// </summary>
    public sealed class RDFSPARQLEndpoint : RDFDataSource
    {
        #region Properties
        /// <summary>
        /// Base address of the SPARQL endpoint
        /// </summary>
        public Uri BaseAddress { get; internal set; }

        /// <summary>
        /// Flag indicating the type of authorization header which will eventually be sent to the SPARQL endpoint
        /// </summary>
        public RDFQueryEnums.RDFSPARQLEndpointAuthorizationTypes AuthorizationType { get; internal set; }

        /// <summary>
        /// Value of the authorization header which will eventually be sent to the SPARQL endpoint
        /// </summary>
        internal string AuthorizationValue { get; set; }

        /// <summary>
        /// Collection of query params sent to the SPARQL endpoint
        /// </summary>
        internal NameValueCollection QueryParams { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a SPARQL enpoint with given base address
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFSPARQLEndpoint(Uri baseAddress)
        {
            BaseAddress = baseAddress ?? throw new RDFQueryException("Cannot create RDFSPARQLEndpoint because given \"baseAddress\" parameter is null.");
            QueryParams = [];
            AuthorizationType = RDFQueryEnums.RDFSPARQLEndpointAuthorizationTypes.None;
            AuthorizationValue = null;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SPARQL enpoint
        /// </summary>
        public override string ToString()
            => BaseAddress.ToString();
        #endregion

        #region Methods
        /// <summary>
        /// Sets the "Basic {basicAuthHeaderValue}" authorization header which will be sent to the SPARQL endpoint
        /// </summary>
        public RDFSPARQLEndpoint SetBasicAuthorizationHeader(string basicAuthHeaderValue)
        {
            if (!string.IsNullOrEmpty(basicAuthHeaderValue))
            {
                AuthorizationType = RDFQueryEnums.RDFSPARQLEndpointAuthorizationTypes.Basic;
                AuthorizationValue = basicAuthHeaderValue;
            }
            return this;
        }

        /// <summary>
        /// Sets the "Bearer {bearerAuthHeaderValue}" authorization header which will be sent to the SPARQL endpoint
        /// </summary>
        public RDFSPARQLEndpoint SetBearerAuthorizationHeader(string bearerAuthHeaderValue)
        {
            if (!string.IsNullOrEmpty(bearerAuthHeaderValue))
            {
                AuthorizationType = RDFQueryEnums.RDFSPARQLEndpointAuthorizationTypes.Bearer;
                AuthorizationValue = bearerAuthHeaderValue;
            }
            return this;
        }

        /// <summary>
        /// Adds a "default-graph-uri" parameter to be sent to the SPARQL endpoint
        /// </summary>
        public RDFSPARQLEndpoint AddDefaultGraphUri(string defaultGraphUri)
        {
            QueryParams.Add("default-graph-uri", defaultGraphUri ?? string.Empty);
            return this;
        }

        /// <summary>
        /// Adds a "named-graph-uri" parameter to be sent to the SPARQL endpoint
        /// </summary>
        public RDFSPARQLEndpoint AddNamedGraphUri(string namedGraphUri)
        {
            QueryParams.Add("named-graph-uri", namedGraphUri ?? string.Empty);
            return this;
        }

        /// <summary>
        /// Adds the proper authorization header to the given RDF WebClient
        /// </summary>
        internal void FillWebClientAuthorization(RDFWebClient webClient)
        {
            switch (AuthorizationType)
            {
                //Basic
                case RDFQueryEnums.RDFSPARQLEndpointAuthorizationTypes.Basic:
                    webClient.Headers.Add(HttpRequestHeader.Authorization, $"Basic {AuthorizationValue}");
                    break;

                //Bearer
                case RDFQueryEnums.RDFSPARQLEndpointAuthorizationTypes.Bearer:
                    webClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {AuthorizationValue}");
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// RDFSPARQLEndpointQueryOptions customizes the default behavior of a SPARQL endpoint query
    /// </summary>
    public sealed class RDFSPARQLEndpointQueryOptions
    {
        #region Properties
        /// <summary>
        /// Represents the timeout observed for the query sent to the SPARQL endpoint (default: -1)
        /// </summary>
        public int TimeoutMilliseconds { get; set; }

        /// <summary>
        /// Represents the behavior used by the query in case of runtime errors (default: ThrowException)
        /// </summary>
        public RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors ErrorBehavior { get; set; }

        /// <summary>
        /// Represents the HTTP method used by the query to contact the SPARQL endpoint (default: Get)
        /// </summary>
        public RDFQueryEnums.RDFSPARQLEndpointQueryMethods QueryMethod { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds the default options for a SPARQL endpoint query
        /// </summary>
        public RDFSPARQLEndpointQueryOptions()
        {
            TimeoutMilliseconds = -1;
            ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException;
            QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Get;
        }

        /// <summary>
        /// Builds custom options for a SPARQL endpoint query
        /// </summary>
        public RDFSPARQLEndpointQueryOptions(int timeoutMilliseconds) : this()
            => TimeoutMilliseconds = timeoutMilliseconds < -1 ? -1 : timeoutMilliseconds;

        /// <summary>
        ///Builds custom options for a SPARQL endpoint query
        /// </summary>
        public RDFSPARQLEndpointQueryOptions(int timeoutMilliseconds,
            RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors? errorBehavior=null,
            RDFQueryEnums.RDFSPARQLEndpointQueryMethods? queryMethod=null) : this(timeoutMilliseconds)
         {
            ErrorBehavior = errorBehavior ?? RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException;
            QueryMethod = queryMethod ?? RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Get;
         }
        #endregion
    }

    /// <summary>
    /// RDFSPARQLEndpointOperationOptions customizes the default behavior of a SPARQL UPDATE endpoint operation
    /// </summary>
    public sealed class RDFSPARQLEndpointOperationOptions
    {
        #region Properties
        /// <summary>
        /// Represents the timeout observed for the query sent to the SPARQL UPDATE endpoint (defaults to: -1)
        /// </summary>
        public int TimeoutMilliseconds { get; set; }

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
        {
            TimeoutMilliseconds = -1;
            RequestContentType = RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.Sparql_Update;
        }

        /// <summary>
        /// Custom-ctor to configure options for a SPARQL UPDATE endpoint operation
        /// </summary>
        public RDFSPARQLEndpointOperationOptions(int timeoutMilliseconds) : this()
            => TimeoutMilliseconds = timeoutMilliseconds < -1 ? -1 : timeoutMilliseconds;

        /// <summary>
        /// Custom-ctor to configure options for a SPARQL UPDATE endpoint operation
        /// </summary>
        public RDFSPARQLEndpointOperationOptions(int timeoutMilliseconds, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes requestContentType) : this(timeoutMilliseconds)
            => RequestContentType = requestContentType;
        #endregion
    }
}