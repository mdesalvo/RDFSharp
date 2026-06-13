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
    /// group graph pattern to a remote SPARQL endpoint (federated query). It maps onto the flat Mirella model as
    /// a single <see cref="RDFPatternGroup"/> flagged via <see cref="RDFPatternGroup.AsService"/> — SERVICE is NOT
    /// an algebra-tree node, it is a per-group decoration (the symmetric counterpart of GRAPH's per-pattern
    /// context decoration).
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// [59]  ServiceGraphPattern ::= 'SERVICE' 'SILENT'? VarOrIri GroupGraphPattern
    /// [107] VarOrIri            ::= Var | iri
    /// </code>
    /// </para>
    /// <para>
    /// Model-imposed limits (<see cref="RDFPatternGroup.AsService"/> needs a CONCRETE endpoint and only flags a
    /// single flat pattern group): a variable endpoint (<c>SERVICE ?ep {…}</c>), an inner pattern that collapses
    /// to anything other than one <see cref="RDFPatternGroup"/> (UNION/OPTIONAL/multiple groups/sub-SELECT are
    /// wrapped into a <c>SELECT *</c> subquery), and a nested SERVICE are all NOT representable and raise an
    /// explicit <see cref="RDFQueryException"/>. <c>SILENT</c> maps to the endpoint's
    /// <see cref="RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult"/> error behavior.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region Service
        /// <summary>
        /// Parses a SERVICE graph pattern (the <c>SERVICE</c> keyword has already been consumed by the
        /// dispatcher): the optional <c>SILENT</c> directive, the endpoint specifier, and the inner group graph
        /// pattern. Returns the inner pattern group flagged <see cref="RDFPatternGroup.AsService"/> so the engine
        /// dispatches its evaluation to the remote endpoint.
        /// </summary>
        /// <exception cref="RDFQueryException">When the endpoint is a variable/literal, or the inner pattern is not representable as a single (non-SERVICE) pattern group.</exception>
        private static RDFQueryMember ParseServiceGraphPattern(RDFQueryParserContext parserContext)
        {
            //Optional SILENT directive: suppresses remote errors by yielding an empty result instead of throwing
            bool isSilent = TryConsumeKeyword(parserContext, "SILENT");

            //The endpoint specifier: a concrete IRI (a variable endpoint is not representable by the flat model)
            RDFSPARQLEndpoint sparqlEndpoint = ParseServiceEndpoint(parserContext);

            //The inner GroupGraphPattern. It collapses (CollapseToSingle) to a single algebra member: only a bare
            //RDFPatternGroup can carry the SERVICE flag — a complex body becomes a SELECT * subquery instead.
            RDFQueryMember serviceScopeMember = ParseGroupGraphPattern(parserContext);
            if (!(serviceScopeMember is RDFPatternGroup serviceGroup))
                throw new RDFQueryException("Cannot parse SPARQL SERVICE clause: its inner pattern must be a single basic group pattern (UNION/OPTIONAL/nested groups/subqueries are not representable as a SERVICE-flagged pattern group) " + GetCoordinates(parserContext));

            //A nested SERVICE would require flagging the same pattern group with two endpoints: not representable
            if (serviceGroup.EvaluateAsService.HasValue)
                throw new RDFQueryException("Cannot parse SPARQL SERVICE clause: a nested SERVICE is not representable " + GetCoordinates(parserContext));

            //Flag the pattern group for remote evaluation. SILENT selects the empty-result error behavior; a
            //non-silent SERVICE keeps AsService's default options (which throw on remote errors).
            serviceGroup.AsService(sparqlEndpoint, isSilent
                ? new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult }
                : null);

            return serviceGroup;
        }

        /// <summary>
        /// Parses the endpoint specifier of a SERVICE clause — the <c>VarOrIri</c> between the keyword and the
        /// inner group's opening brace — into the concrete <see cref="RDFSPARQLEndpoint"/> the engine will query.
        /// <para>
        /// SPARQL grammar: <c>VarOrIri ::= Var | iri</c>. A variable endpoint is spec-legal but NOT representable
        /// (the model needs a concrete endpoint URL), and a literal/blank node is invalid: both are rejected.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the specifier is a variable, a literal, a blank node, or not a valid endpoint IRI.</exception>
        private static RDFSPARQLEndpoint ParseServiceEndpoint(RDFQueryParserContext parserContext)
        {
            //A '?' or '$' sigil starts a variable endpoint: the flat model cannot defer to a runtime-bound endpoint
            int nextSignificantCodePoint = SkipWhitespace(parserContext);
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                throw new RDFQueryException("Cannot parse SPARQL SERVICE clause: a variable endpoint is not representable (a concrete endpoint IRI is required) " + GetCoordinates(parserContext));

            //Otherwise the specifier must be an IRI (IRIREF or prefixed name): parse it through the shared
            //term-reader so prologue BASE/PREFIX resolution applies, then validate it is a usable endpoint IRI.
            RDFPatternMember endpointTerm = ParseTerm(parserContext);
            if (!(endpointTerm is RDFResource endpointResource))
                throw new RDFQueryException("Cannot parse SPARQL SERVICE clause: the endpoint must be a variable or an IRI, but a literal was found " + GetCoordinates(parserContext));
            if (endpointResource.IsBlank)
                throw new RDFQueryException("Cannot parse SPARQL SERVICE clause: the endpoint must be a variable or an IRI, but a blank node was found " + GetCoordinates(parserContext));

            try
            {
                //Promote the IRI resource to the concrete remote endpoint
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
