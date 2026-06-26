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
    /// CREATE/DROP half of the SPARQL UPDATE graph-management parser: the two operations whose body is a single
    /// graph reference (plus the optional SILENT flag), with no triple template and no source→destination pair.
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// Create      ::= 'CREATE' 'SILENT'? GraphRef
    /// Drop        ::= 'DROP'   'SILENT'? GraphRefAll
    /// GraphRef    ::= 'GRAPH' iri
    /// GraphRefAll ::= GraphRef | 'DEFAULT' | 'NAMED' | 'ALL'
    /// </code>
    /// </para>
    /// </summary>
    internal static partial class RDFOperationParser
    {
        #region CreateDrop
        /// <summary>
        /// Parses the body of a CREATE operation (the 'CREATE' keyword has already been consumed by the dispatcher):
        /// the optional SILENT flag followed by the mandatory <c>GRAPH iri</c> naming the graph to be created.
        /// </summary>
        /// <exception cref="RDFQueryException">When 'GRAPH' is missing, or the IRI is missing/malformed.</exception>
        private static RDFCreateOperation ParseCreateOperation(RDFQueryParserContext parserContext)
        {
            //'SILENT'? — hides remote errors when the operation runs against a SPARQL UPDATE endpoint
            bool isSilent = TryConsumeKeyword(parserContext, "SILENT");

            //GraphRef ::= 'GRAPH' iri — the 'GRAPH' keyword is mandatory before the named graph IRI
            if (!TryConsumeKeyword(parserContext, "GRAPH"))
                throw new RDFQueryException("Cannot parse SPARQL CREATE operation: expected 'GRAPH' " + GetCoordinates(parserContext));

            RDFCreateOperation createOperation = new RDFCreateOperation(ParseOperationIri(parserContext, "CREATE graph"));
            if (isSilent)
                createOperation.Silent();

            return createOperation;
        }

        /// <summary>
        /// Parses the body of a DROP operation (the 'DROP' keyword has already been consumed by the dispatcher):
        /// the optional SILENT flag followed by a <c>GraphRefAll</c> — the very same graph reference grammar of
        /// CLEAR, so it is parsed through the shared <see cref="ParseGraphRefAll"/> helper.
        /// </summary>
        /// <exception cref="RDFQueryException">When the GraphRefAll is missing or malformed.</exception>
        private static RDFDropOperation ParseDropOperation(RDFQueryParserContext parserContext)
        {
            //'SILENT'? — hides remote errors when the operation runs against a SPARQL UPDATE endpoint
            bool isSilent = TryConsumeKeyword(parserContext, "SILENT");

            //GraphRefAll ::= ('GRAPH' iri) | 'DEFAULT' | 'NAMED' | 'ALL'
            ParseGraphRefAll(parserContext, "DROP", out Uri fromContext, out RDFQueryEnums.RDFClearOperationFlavor operationFlavor);
            RDFDropOperation dropOperation = fromContext != null
                                                ? new RDFDropOperation(fromContext)
                                                : new RDFDropOperation(operationFlavor);
            if (isSilent)
                dropOperation.Silent();

            return dropOperation;
        }

        /// <summary>
        /// Parses a <c>GraphRefAll ::= ('GRAPH' iri) | 'DEFAULT' | 'NAMED' | 'ALL'</c> graph reference, shared by
        /// CLEAR and DROP (their graph-reference grammar is identical). On an explicit <c>GRAPH iri</c> the IRI is
        /// returned via <paramref name="fromContext"/>; on an implicit flavor the matching
        /// <see cref="RDFQueryEnums.RDFClearOperationFlavor"/> is returned via <paramref name="operationFlavor"/>.
        /// </summary>
        /// <exception cref="RDFQueryException">When the GraphRefAll is missing or malformed.</exception>
        private static void ParseGraphRefAll(RDFQueryParserContext parserContext, string operationName, out Uri fromContext, out RDFQueryEnums.RDFClearOperationFlavor operationFlavor)
        {
            fromContext = null;
            operationFlavor = default;

            SkipWhitespace(parserContext);
            string graphRefKeyword = ReadKeyword(parserContext);
            switch (graphRefKeyword.ToUpperInvariant())
            {
                //Explicit graph: the IRI follows the 'GRAPH' keyword
                case "GRAPH":
                    fromContext = ParseOperationIri(parserContext, operationName + " graph");
                    break;

                //Implicit flavors
                case "DEFAULT":
                    operationFlavor = RDFQueryEnums.RDFClearOperationFlavor.DEFAULT;
                    break;
                case "NAMED":
                    operationFlavor = RDFQueryEnums.RDFClearOperationFlavor.NAMED;
                    break;
                case "ALL":
                    operationFlavor = RDFQueryEnums.RDFClearOperationFlavor.ALL;
                    break;

                default:
                    throw new RDFQueryException("Cannot parse SPARQL " + operationName + " operation: expected 'GRAPH <iri>', 'DEFAULT', 'NAMED' or 'ALL' " + GetCoordinates(parserContext));
            }
        }
        #endregion
    }
}
