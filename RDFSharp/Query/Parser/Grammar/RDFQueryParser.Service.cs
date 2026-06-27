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
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// SERVICE half of the SPARQL parser: a <c>ServiceGraphPattern</c> delegates the evaluation of an inner
    /// group graph pattern to a remote SPARQL endpoint (federated query). It maps onto the first-class algebra
    /// node <see cref="RDFService"/>, modeling the grammar in full.
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// [59]  ServiceGraphPattern ::= 'SERVICE' 'SILENT'? VarOrIri GroupGraphPattern
    /// [107] VarOrIri            ::= Var | iri
    /// </code>
    /// </para>
    /// <para>
    /// All three forms are representable: a concrete IRI endpoint or a variable endpoint (<c>SERVICE ?ep {…}</c>,
    /// resolved at runtime from the surrounding bindings), an inner pattern of any shape (pattern group, sub-select,
    /// UNION/OPTIONAL/MINUS tree), and a nested SERVICE. <c>SILENT</c> maps to the endpoint's
    /// <see cref="RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult"/> error behavior.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region Service
        /// <summary>
        /// Parses a SERVICE graph pattern (the <c>SERVICE</c> keyword has already been consumed by the
        /// dispatcher): the optional <c>SILENT</c> directive, the endpoint specifier (a concrete IRI or a
        /// variable), and the inner group graph pattern (any shape). Returns the resulting <see cref="RDFService"/>
        /// algebra node so the engine dispatches its evaluation to the remote endpoint.
        /// </summary>
        /// <exception cref="RDFQueryException">When the endpoint specifier is a literal/blank node or not a valid endpoint IRI.</exception>
        private static RDFQueryMember ParseServiceGraphPattern(RDFQueryParserContext parserContext)
        {
            //Optional SILENT directive: suppresses remote errors by yielding an empty result instead of throwing
            bool isSilent = TryConsumeKeyword(parserContext, "SILENT");

            //The endpoint specifier (VarOrIri): a concrete IRI or a variable bound at runtime
            RDFPatternMember endpointSpecifier = ParseServiceEndpoint(parserContext);

            //The inner GroupGraphPattern, of ANY shape: ParseGroupGraphPattern returns it as-is (a pattern group,
            //a SELECT * subquery wrapping a compound body, a binary tree, or a nested SERVICE)
            RDFQueryMember innerPattern = ParseGroupGraphPattern(parserContext);

            //SILENT selects the empty-result error behavior; otherwise the default options (which throw on errors)
            RDFSPARQLEndpointQueryOptions queryOptions = isSilent
                ? new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult }
                : null;

            //Build the SERVICE node: a variable specifier yields the variable ctor, an IRI yields the concrete ctor
            return endpointSpecifier is RDFVariable endpointVariable
                ? new RDFService(endpointVariable, innerPattern, queryOptions)
                : new RDFService(BuildServiceEndpoint((RDFResource)endpointSpecifier, parserContext), innerPattern, queryOptions);
        }

        /// <summary>
        /// Parses the endpoint specifier of a SERVICE clause — the <c>VarOrIri</c> between the keyword and the
        /// inner group's opening brace — into either an <see cref="RDFVariable"/> (variable endpoint) or an
        /// <see cref="RDFResource"/> (concrete IRI endpoint).
        /// <para>
        /// SPARQL grammar: <c>VarOrIri ::= Var | iri</c>. A literal or a blank node in this position is invalid
        /// and is rejected.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the specifier is a literal or a blank node.</exception>
        private static RDFPatternMember ParseServiceEndpoint(RDFQueryParserContext parserContext)
        {
            //A '?' or '$' sigil starts a variable endpoint: bound, at runtime, from the surrounding solutions
            int nextSignificantCodePoint = SkipWhitespace(parserContext);
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParseVariable(parserContext);

            //Otherwise the specifier must be an IRI (IRIREF or prefixed name): parse it through the shared
            //term-reader so prologue BASE/PREFIX resolution applies, then validate it is a usable endpoint IRI.
            RDFPatternMember endpointTerm = ParseTerm(parserContext);
            if (!(endpointTerm is RDFResource endpointResource))
                throw new RDFQueryException("Cannot parse SPARQL SERVICE clause: the endpoint must be a variable or an IRI, but a literal was found " + GetCoordinates(parserContext));
            if (endpointResource.IsBlank)
                throw new RDFQueryException("Cannot parse SPARQL SERVICE clause: the endpoint must be a variable or an IRI, but a blank node was found " + GetCoordinates(parserContext));
            return endpointResource;
        }

        /// <summary>
        /// Promotes a concrete endpoint IRI resource to the <see cref="RDFSPARQLEndpoint"/> the engine will query.
        /// </summary>
        /// <exception cref="RDFQueryException">When the IRI is not a usable absolute endpoint URL.</exception>
        private static RDFSPARQLEndpoint BuildServiceEndpoint(RDFResource endpointResource, RDFQueryParserContext parserContext)
        {
            try
            {
                return new RDFSPARQLEndpoint(new Uri(endpointResource.ToString()));
            }
            catch (Exception endpointException) when (endpointException is UriFormatException || endpointException is ArgumentException)
            {
                //An IRI that is not a usable absolute endpoint URL: surface it as a query-level parse error
                throw new RDFQueryException("Cannot parse SPARQL SERVICE clause: invalid endpoint IRI '" + endpointResource + "' " + GetCoordinates(parserContext) + ": " + endpointException.Message, endpointException);
            }
        }
        #endregion
    }
}
