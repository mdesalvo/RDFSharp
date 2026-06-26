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
using RDFQueryParserContext = RDFSharp.Query.RDFQueryParser.RDFQueryParserContext;

namespace RDFSharp.Query
{
    /// <summary>
    /// <para>
    /// RDFOperationParser turns a SPARQL 1.1 UPDATE command string into the corresponding <see cref="RDFOperation"/>
    /// object-model instance. It is the UPDATE-side counterpart of <see cref="RDFQueryParser"/>: it owns the
    /// operation grammar (LOAD / CLEAR / INSERT … / DELETE …) and its dispatcher, while REUSING the query parser's
    /// shared infrastructure — the lexer (<see cref="RDFQueryLexer"/>, via <c>using static</c>), the parsing
    /// context (<see cref="RDFQueryParser.RDFQueryParserContext"/>), the prologue parser
    /// (<see cref="RDFQueryParser.ParsePrologue"/>) and the term-reader — so that nothing is duplicated.
    /// </para>
    /// <para>
    /// SPARQL grammar (the UPDATE side): <c>Update ::= Prologue ( Update1 ( ';' Update )? )?</c>. The flat model
    /// represents ONE operation per object, so a chain of ';'-separated operations is not representable: a single
    /// operation is parsed and any following one is rejected (see <see cref="RejectTrailingOperationContent"/>).
    /// </para>
    /// </summary>
    internal static partial class RDFOperationParser
    {
        #region Operation
        /// <summary>
        /// <para>
        /// Parses a complete SPARQL 1.1 UPDATE command string into the corresponding <see cref="RDFOperation"/>
        /// object-model instance. Like <see cref="RDFQueryParser.ParseQuery"/> it builds a fresh parsing context,
        /// consumes the (possibly empty) BASE/PREFIX prologue, then dispatches on the operation keyword.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the text is empty, the operation keyword is missing/unknown/non-representable, or the body is malformed.</exception>
        internal static RDFOperation ParseOperation(string sparqlOperationText)
        {
            //Fail fast on a missing/blank command: there is nothing to parse
            if (string.IsNullOrWhiteSpace(sparqlOperationText))
                throw new RDFQueryException("Cannot parse SPARQL UPDATE operation because the given command text is null or empty");

            //Build the per-parse state (single-use), reusing the very same context as the query parser
            RDFQueryParserContext parserContext = RDFQueryParser.CreateContext(sparqlOperationText);

            //STEP 1 - Prologue (shared with the query path): consume the BASE/PREFIX declarations, leaving the
            //operation keyword unconsumed in the reader for the dispatch below
            RDFQueryParser.ParsePrologue(parserContext);

            //STEP 2 - Operation form. Read the keyword run that names the operation and dispatch on it
            SkipWhitespace(parserContext);
            string operationKeyword = ReadKeyword(parserContext);
            RDFOperation parsedOperation;
            switch (operationKeyword.ToUpperInvariant())
            {
                case "LOAD":
                    parsedOperation = ParseLoadOperation(parserContext);
                    break;

                case "CLEAR":
                    parsedOperation = ParseClearOperation(parserContext);
                    break;

                case "INSERT":
                    if (TryConsumeKeyword(parserContext, "DATA"))
                        parsedOperation = ParseInsertDataOperation(parserContext);
                    else
                        parsedOperation = ParseInsertWhereOperation(parserContext);
                    break;

                case "DELETE":
                    if (TryConsumeKeyword(parserContext, "DATA"))
                        parsedOperation = ParseDeleteDataOperation(parserContext);
                    else
                        parsedOperation = ParseDeleteWhereOperation(parserContext);
                    break;

                //WITH <iri> opens a Modify whose DELETE/INSERT clauses act on a fixed graph: deliberately NOT
                //supported, because WITH (like FROM/USING) tells a SPARQL endpoint which dataset to operate on,
                //whereas RDFSharp operates over the source you provide; target a graph explicitly with GRAPH instead
                case "WITH":
                    throw new RDFQueryException("Cannot parse SPARQL UPDATE operation: a WITH clause is not supported. WITH tells a SPARQL endpoint which graph to operate on, whereas RDFSharp operates over the data source you provide; target a named graph explicitly with GRAPH in the template/WHERE instead " + GetCoordinates(parserContext));

                //Valid SPARQL graph-management operations that the flat model cannot represent (no matching class):
                //reject them explicitly as non-representable rather than as a generic unexpected token
                case "CREATE":
                case "DROP":
                case "COPY":
                case "MOVE":
                case "ADD":
                    throw new RDFQueryException("Cannot parse SPARQL UPDATE operation: '" + operationKeyword.ToUpperInvariant() + "' is not representable by the flat model " + GetCoordinates(parserContext));

                //Empty keyword: the input ended right after the prologue, so no operation keyword was present
                case "":
                    throw new RDFQueryException("Cannot parse SPARQL UPDATE operation: expected an operation keyword (LOAD, CLEAR, INSERT or DELETE) " + GetCoordinates(parserContext));

                //A non-empty keyword that is none of the known operations: surface it verbatim
                default:
                    throw new RDFQueryException("Cannot parse SPARQL UPDATE operation: unexpected token '" + operationKeyword + "' where an operation keyword was expected " + GetCoordinates(parserContext));
            }

            //A single ';'-separated operation is tolerated; a second operation is not representable (one per object)
            RejectTrailingOperationContent(parserContext);

            return parsedOperation;
        }

        /// <summary>
        /// Requires that nothing significant follows the parsed operation, beyond an optional single trailing
        /// <c>;</c>. The SPARQL grammar allows a chain of <c>;</c>-separated operations, but the flat model holds
        /// exactly one operation per object, so a second operation is rejected as non-representable rather than
        /// silently dropped (<see cref="ParseOperation"/> does not enforce end-of-input itself).
        /// </summary>
        /// <exception cref="RDFQueryException">When a second operation, or any other content, follows.</exception>
        private static void RejectTrailingOperationContent(RDFQueryParserContext parserContext)
        {
            //Consume an optional trailing ';' (the legal separator of an operation chain)
            if (SkipWhitespace(parserContext) == ';')
            {
                ReadCodePoint(parserContext);

                //Anything after the ';' is a second operation: the flat model holds only one
                if (SkipWhitespace(parserContext) != -1)
                    throw new RDFQueryException("Cannot parse SPARQL UPDATE: multiple ';'-separated operations are not representable (only a single operation per parse) " + GetCoordinates(parserContext));

                return;
            }

            //No ';': any leftover significant content is unexpected
            if (SkipWhitespace(parserContext) != -1)
                throw new RDFQueryException("Cannot parse SPARQL UPDATE: unexpected trailing content after the operation " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Parses a single <c>iri</c> in operation position (a LOAD/CLEAR graph reference) and returns it as an
        /// absolute <see cref="Uri"/>. The IRI is read through the shared term-reader (so prologue BASE/PREFIX
        /// resolution applies). A variable, a literal or a blank node in this position is invalid.
        /// </summary>
        private static Uri ParseOperationIri(RDFQueryParserContext parserContext, string grammarContext)
        {
            //A variable is never a valid graph reference
            int nextSignificantCodePoint = SkipWhitespace(parserContext);
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                throw new RDFQueryException("Cannot parse SPARQL UPDATE " + grammarContext + ": expected an IRI, but a variable was found " + GetCoordinates(parserContext));

            //Read the term and validate it is an IRI (not a literal, not a blank node)
            RDFPatternMember operationTerm = ParseTerm(parserContext);
            if (!(operationTerm is RDFResource operationResource))
                throw new RDFQueryException("Cannot parse SPARQL UPDATE " + grammarContext + ": expected an IRI, but a literal was found " + GetCoordinates(parserContext));
            if (operationResource.IsBlank)
                throw new RDFQueryException("Cannot parse SPARQL UPDATE " + grammarContext + ": expected an IRI, but a blank node was found " + GetCoordinates(parserContext));

            try
            {
                return new Uri(operationResource.ToString());
            }
            catch (UriFormatException operationIriException)
            {
                throw new RDFQueryException("Cannot parse SPARQL UPDATE " + grammarContext + ": invalid IRI '" + operationResource + "' " + GetCoordinates(parserContext) + ": " + operationIriException.Message, operationIriException);
            }
        }
        #endregion
    }

    /// <summary>
    /// RDFOperationParserFactory is the public entry point for turning a SPARQL 1.1 UPDATE command string into the
    /// matching <see cref="RDFOperation"/> object-model instance. It is the operation counterpart of
    /// <see cref="RDFQueryParserFactory"/>. The strongly typed <c>FromString</c> helpers on the concrete operation
    /// classes delegate here and then validate the form.
    /// </summary>
    public static class RDFOperationParserFactory
    {
        #region Methods
        /// <summary>
        /// Parses the given SPARQL 1.1 UPDATE command string into its RDFOperation object-model representation.
        /// </summary>
        /// <exception cref="RDFQueryException">When the command string is null/empty or syntactically invalid.</exception>
        public static RDFOperation ParseOperation(string operationString)
            => RDFOperationParser.ParseOperation(operationString);
        #endregion
    }
}
