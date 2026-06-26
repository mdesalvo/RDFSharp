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
using static RDFSharp.Query.RDFQueryLexer;
using RDFQueryParserContext = RDFSharp.Query.RDFQueryParser.RDFQueryParserContext;

namespace RDFSharp.Query
{
    /// <summary>
    /// COPY/MOVE/ADD third of the SPARQL UPDATE graph-management parser: the source→destination operations, sharing
    /// one body grammar (a <c>GraphOrDefault</c> pair separated by 'TO', plus the optional SILENT flag) and
    /// differing only in the concrete operation they build.
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// Copy           ::= 'COPY' 'SILENT'? GraphOrDefault 'TO' GraphOrDefault
    /// Move           ::= 'MOVE' 'SILENT'? GraphOrDefault 'TO' GraphOrDefault
    /// Add            ::= 'ADD'  'SILENT'? GraphOrDefault 'TO' GraphOrDefault
    /// GraphOrDefault ::= 'DEFAULT' | 'GRAPH'? iri
    /// </code>
    /// </para>
    /// </summary>
    internal static partial class RDFOperationParser
    {
        #region CopyMoveAdd
        /// <summary>
        /// Parses the body of a COPY/MOVE/ADD operation (the operation keyword has already been consumed by the
        /// dispatcher, and is passed in via <paramref name="keyword"/>): the optional SILENT flag, then the source
        /// <c>GraphOrDefault</c>, the mandatory 'TO' separator and the destination <c>GraphOrDefault</c>. A null
        /// context denotes the DEFAULT graph.
        /// </summary>
        /// <exception cref="RDFQueryException">When 'TO' or a GraphOrDefault endpoint is missing/malformed.</exception>
        private static RDFOperation ParseCopyMoveAddOperation(RDFQueryParserContext parserContext, string keyword)
        {
            //'SILENT'? — hides remote errors when the operation runs against a SPARQL UPDATE endpoint
            bool isSilent = TryConsumeKeyword(parserContext, "SILENT");

            //GraphOrDefault 'TO' GraphOrDefault — source and destination graph references
            Uri fromContext = ParseGraphOrDefault(parserContext, keyword + " source");
            if (!TryConsumeKeyword(parserContext, "TO"))
                throw new RDFQueryException("Cannot parse SPARQL " + keyword + " operation: expected 'TO' " + GetCoordinates(parserContext));
            Uri toContext = ParseGraphOrDefault(parserContext, keyword + " destination");

            //Build the concrete operation for the dispatched keyword
            switch (keyword)
            {
                case "COPY":
                {
                    RDFCopyOperation copyOperation = new RDFCopyOperation().SetFromContext(fromContext).SetToContext(toContext);
                    if (isSilent)
                        copyOperation.Silent();
                    return copyOperation;
                }
                case "MOVE":
                {
                    RDFMoveOperation moveOperation = new RDFMoveOperation().SetFromContext(fromContext).SetToContext(toContext);
                    if (isSilent)
                        moveOperation.Silent();
                    return moveOperation;
                }
                default: //"ADD"
                {
                    RDFAddOperation addOperation = new RDFAddOperation().SetFromContext(fromContext).SetToContext(toContext);
                    if (isSilent)
                        addOperation.Silent();
                    return addOperation;
                }
            }
        }

        /// <summary>
        /// Parses a single <c>GraphOrDefault ::= 'DEFAULT' | 'GRAPH'? iri</c> endpoint, returning <c>null</c> for
        /// the DEFAULT graph or the absolute <see cref="Uri"/> of the named graph. The 'GRAPH' keyword is optional
        /// before the IRI; a bare IRIREF or prefixed name is equally accepted.
        /// </summary>
        /// <exception cref="RDFQueryException">When the IRI is missing/malformed.</exception>
        private static Uri ParseGraphOrDefault(RDFQueryParserContext parserContext, string grammarContext)
        {
            //'DEFAULT' — the default graph, modeled as a null context
            if (TryConsumeKeyword(parserContext, "DEFAULT"))
                return null;

            //'GRAPH'? iri — the 'GRAPH' keyword is optional; the IRI is mandatory
            TryConsumeKeyword(parserContext, "GRAPH");
            return ParseOperationIri(parserContext, grammarContext);
        }
        #endregion
    }
}
