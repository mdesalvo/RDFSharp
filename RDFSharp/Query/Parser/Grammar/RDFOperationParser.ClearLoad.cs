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
    /// CLEAR/LOAD half of the SPARQL UPDATE parser: the CLEAR and LOAD operations — the two UPDATE forms
    /// that carry no triple template and no WHERE clause, only graph references and the optional SILENT flag.
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// Load        ::= 'LOAD' 'SILENT'? iri ( 'INTO' GraphRef )?
    /// Clear       ::= 'CLEAR' 'SILENT'? GraphRefAll
    /// GraphRef    ::= 'GRAPH' iri
    /// GraphRefAll ::= GraphRef | 'DEFAULT' | 'NAMED' | 'ALL'
    /// </code>
    /// </para>
    /// </summary>
    internal static partial class RDFOperationParser
    {
        #region ClearLoad
        /// <summary>
        /// Parses the body of a LOAD operation (the 'LOAD' keyword has already been consumed by the dispatcher):
        /// the optional SILENT flag, the source <c>iri</c>, and an optional <c>INTO GRAPH iri</c> destination.
        /// </summary>
        /// <exception cref="RDFQueryException">When the source IRI is missing/malformed, or 'GRAPH' is missing after 'INTO'.</exception>
        private static RDFLoadOperation ParseLoadOperation(RDFQueryParserContext parserContext)
        {
            //'SILENT'? — hides remote errors when the operation runs against a SPARQL UPDATE endpoint
            bool isSilent = TryConsumeKeyword(parserContext, "SILENT");

            //iri — the mandatory remote graph the data is fetched from
            Uri fromContext = ParseOperationIri(parserContext, "LOAD source");
            RDFLoadOperation loadOperation = new RDFLoadOperation(fromContext);
            if (isSilent)
                loadOperation.Silent();

            //( 'INTO' GraphRef )? — the optional target graph the data is loaded into
            if (TryConsumeKeyword(parserContext, "INTO"))
            {
                //GraphRef ::= 'GRAPH' iri — the 'GRAPH' keyword is mandatory after 'INTO'
                if (!TryConsumeKeyword(parserContext, "GRAPH"))
                    throw new RDFQueryException("Cannot parse SPARQL LOAD operation: expected 'GRAPH' after 'INTO' " + GetCoordinates(parserContext));

                loadOperation.SetContext(ParseOperationIri(parserContext, "LOAD target graph"));
            }

            return loadOperation;
        }

        /// <summary>
        /// Parses the body of a CLEAR operation (the 'CLEAR' keyword has already been consumed by the dispatcher):
        /// the optional SILENT flag followed by a <c>GraphRefAll</c> — either <c>GRAPH iri</c> (an explicit graph)
        /// or one of the implicit flavors <c>DEFAULT</c> / <c>NAMED</c> / <c>ALL</c>.
        /// </summary>
        /// <exception cref="RDFQueryException">When the GraphRefAll is missing or malformed.</exception>
        private static RDFClearOperation ParseClearOperation(RDFQueryParserContext parserContext)
        {
            //'SILENT'? — hides remote errors when the operation runs against a SPARQL UPDATE endpoint
            bool isSilent = TryConsumeKeyword(parserContext, "SILENT");

            //GraphRefAll ::= ('GRAPH' iri) | 'DEFAULT' | 'NAMED' | 'ALL' (shared with DROP, same grammar)
            ParseGraphRefAll(parserContext, "CLEAR", out Uri fromContext, out RDFQueryEnums.RDFClearOperationFlavor operationFlavor);
            RDFClearOperation clearOperation = fromContext != null
                                                ? new RDFClearOperation(fromContext)
                                                : new RDFClearOperation(operationFlavor);
            if (isSilent)
                clearOperation.Silent();

            return clearOperation;
        }
        #endregion
    }
}
