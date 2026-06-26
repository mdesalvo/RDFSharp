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
using System.Collections.Generic;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFService is a first-class algebra node modeling a SPARQL <c>SERVICE</c> graph pattern: it delegates the
    /// evaluation of an inner group graph pattern to a remote SPARQL endpoint (federated query).
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// [59]  ServiceGraphPattern ::= 'SERVICE' 'SILENT'? VarOrIri GroupGraphPattern
    /// [107] VarOrIri            ::= Var | iri
    /// </code>
    /// </para>
    /// </summary>
    public sealed class RDFService : RDFQueryMember
    {
        #region Properties
        /// <summary>
        /// The single, reusable remote endpoint queried by this SERVICE. For a concrete (IRI) endpoint its
        /// <see cref="RDFSPARQLEndpoint.BaseAddress"/> is fixed; for a variable endpoint it is a reusable carrier
        /// whose <see cref="RDFSPARQLEndpoint.BaseAddress"/> the engine overwrites for every bound IRI (so the
        /// runtime never instantiates a fresh endpoint per binding). It also carries any authorization headers and
        /// query parameters set on it.
        /// </summary>
        public RDFSPARQLEndpoint Endpoint { get; internal set; }

        /// <summary>
        /// The variable carrying the endpoint IRI when the SERVICE specifier is a variable (<c>SERVICE ?ep {…}</c>);
        /// null when the endpoint is a concrete IRI. When set, the engine resolves it against the surrounding
        /// solution bindings and queries each distinct IRI in turn by overwriting <see cref="Endpoint"/>'s address.
        /// </summary>
        public RDFVariable EndpointVariable { get; internal set; }

        /// <summary>
        /// The inner group graph pattern delegated to the remote endpoint. As per SPARQL grammar
        /// (<c>GroupGraphPattern ::= '{' ( SubSelect | GroupGraphPatternSub ) '}'</c>) it is any
        /// <see cref="RDFQueryMember"/>: a <see cref="RDFPatternGroup"/>, an <see cref="RDFSelectQuery"/> (SubSelect),
        /// a binary algebra tree (<see cref="RDFBinaryQueryMember"/>), or a nested <see cref="RDFService"/>.
        /// </summary>
        public RDFQueryMember InnerPattern { get; internal set; }

        /// <summary>
        /// Options driving the remote query. <c>SERVICE SILENT</c> maps to
        /// <see cref="RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult"/> (errors yield an empty
        /// result instead of throwing); the default behavior throws on remote errors.
        /// </summary>
        public RDFSPARQLEndpointQueryOptions QueryOptions { get; internal set; }

        /// <summary>
        /// Flag indicating that this SERVICE participates in the surrounding join as an OPTIONAL branch
        /// (<c>OPTIONAL { SERVICE … }</c>): the engine left-joins its result table instead of inner-joining it.
        /// </summary>
        internal bool IsOptional { get; set; }

        /// <summary>
        /// Tells whether the SERVICE was declared SILENT (errors yield an empty result instead of throwing).
        /// </summary>
        internal bool IsSilent
            => QueryOptions.ErrorBehavior == RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult;

        /// <summary>
        /// Placeholder address assigned to the reusable endpoint of a variable SERVICE: it is never queried as-is,
        /// because the engine overwrites it with each concrete IRI bound to <see cref="EndpointVariable"/>.
        /// </summary>
        private static readonly Uri VariableEndpointPlaceholder = new Uri("http://rdfsharp.servicevariable.endpoint/");
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a SERVICE delegating the given inner pattern to the given concrete SPARQL endpoint.
        /// </summary>
        /// <exception cref="RDFQueryException">When the endpoint or the inner pattern is null.</exception>
        public RDFService(RDFSPARQLEndpoint endpoint, RDFQueryMember innerPattern, RDFSPARQLEndpointQueryOptions queryOptions=null)
        {
            #region Guards
            if (endpoint == null)
                throw new RDFQueryException("Cannot create RDFService because given \"endpoint\" parameter is null.");
            if (innerPattern == null)
                throw new RDFQueryException("Cannot create RDFService because given \"innerPattern\" parameter is null.");
            #endregion

            Endpoint = endpoint;
            EndpointVariable = null;
            InnerPattern = innerPattern;
            QueryOptions = queryOptions ?? new RDFSPARQLEndpointQueryOptions();
            IsEvaluable = true;
        }

        /// <summary>
        /// Builds a SERVICE delegating the given inner pattern to the endpoint bound, at runtime, to the
        /// given variable (<c>SERVICE ?ep {…}</c>). The engine resolves the variable against the surrounding solution
        /// bindings and queries each distinct IRI reusing a single endpoint instance.
        /// </summary>
        /// <exception cref="RDFQueryException">When the variable or the inner pattern is null.</exception>
        public RDFService(RDFVariable endpointVariable, RDFQueryMember innerPattern, RDFSPARQLEndpointQueryOptions queryOptions=null)
        {
            #region Guards
            if (endpointVariable == null)
                throw new RDFQueryException("Cannot create RDFService because given \"endpointVariable\" parameter is null.");
            if (innerPattern == null)
                throw new RDFQueryException("Cannot create RDFService because given \"innerPattern\" parameter is null.");
            #endregion

            Endpoint = new RDFSPARQLEndpoint(VariableEndpointPlaceholder);
            EndpointVariable = endpointVariable;
            InnerPattern = innerPattern;
            QueryOptions = queryOptions ?? new RDFSPARQLEndpointQueryOptions();
            IsEvaluable = true;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SERVICE clause
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintService(this, prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Sets the service to be joined as Optional with the previous query member
        /// </summary>
        public RDFService Optional()
        {
            IsOptional = true;
            return this;
        }

        /// <summary>
        /// Creates a Union operator combining this service with the given pattern group
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Union(RDFPatternGroup other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this service with the given subquery
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Union(RDFSelectQuery other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this service with the given operator tree
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Union(RDFBinaryQueryMember other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this service with the given service
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Union(RDFService other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Minus operator combining this service with the given pattern group
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Minus(RDFPatternGroup other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this service with the given subquery
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Minus(RDFSelectQuery other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this service with the given operator tree
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Minus(RDFBinaryQueryMember other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this service with the given service
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Minus(RDFService other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);
        #endregion
    }
}