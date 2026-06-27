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

using System.Collections.Generic;
using System.Linq;
using static RDFSharp.Query.RDFQueryLexer;
using RDFQueryParserContext = RDFSharp.Query.RDFQueryParser.RDFQueryParserContext;

namespace RDFSharp.Query
{
    /// <summary>
    /// DATA half of the SPARQL UPDATE parser: the INSERT DATA and DELETE DATA operations — the two forms whose
    /// body is a <c>QuadData</c> (a set of GROUND triples and/or <c>GRAPH iri { … }</c> quad blocks) with no WHERE
    /// clause and no variables.
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// InsertData ::= 'INSERT DATA' QuadData
    /// DeleteData ::= 'DELETE DATA' QuadData
    /// QuadData   ::= '{' Quads '}'
    /// Quads      ::= TriplesTemplate? ( ('GRAPH' VarOrIri '{' TriplesTemplate? '}') '.'? TriplesTemplate? )*
    /// </code>
    /// </para>
    /// <para>
    /// The QuadData is collected through the context's template sink
    /// (<see cref="RDFQueryParser.RDFQueryParserContext.TemplatePatternSink"/>), reusing the whole triple machine
    /// while preserving ground triples; a <c>GRAPH iri { … }</c> block sets the active context so its triples are
    /// emitted as four-argument quads. The "DATA" forms forbid variables: each collected pattern is added through
    /// the model's ground-only <c>AddInsert/DeleteTemplate</c>, which rejects any pattern carrying a variable
    /// (including a variable graph context). Property paths are likewise rejected (QuadData has no path production).
    /// </para>
    /// </summary>
    internal static partial class RDFOperationParser
    {
        #region Data
        /// <summary>
        /// Parses the body of an INSERT DATA operation (the 'INSERT' 'DATA' keywords have already been consumed by
        /// the dispatcher): the prologue prefixes plus the QuadData ground templates.
        /// </summary>
        /// <exception cref="RDFQueryException">When the QuadData is malformed, contains a variable, or contains a property path.</exception>
        private static RDFInsertDataOperation ParseInsertDataOperation(RDFQueryParserContext parserContext)
        {
            RDFInsertDataOperation insertDataOperation = new RDFInsertDataOperation();

            //Carry the prologue's declared prefixes so the operation re-serializes its prologue identically
            RDFQueryParser.ApplyDeclaredPrefixes(parserContext, insertDataOperation);

            //QuadData: ground triples and/or GRAPH quad blocks. AddInsertTemplate enforces groundness (throws on a variable).
            foreach (RDFPattern templatePattern in ParseQuadsTemplate(parserContext, "INSERT DATA"))
                insertDataOperation.AddInsertTemplate(templatePattern);

            return insertDataOperation;
        }

        /// <summary>
        /// Parses the body of a DELETE DATA operation (the 'DELETE' 'DATA' keywords have already been consumed by
        /// the dispatcher): the prologue prefixes plus the QuadData ground templates.
        /// </summary>
        /// <exception cref="RDFQueryException">When the QuadData is malformed, contains a variable, or contains a property path.</exception>
        private static RDFDeleteDataOperation ParseDeleteDataOperation(RDFQueryParserContext parserContext)
        {
            RDFDeleteDataOperation deleteDataOperation = new RDFDeleteDataOperation();

            //Carry the prologue's declared prefixes so the operation re-serializes its prologue identically
            RDFQueryParser.ApplyDeclaredPrefixes(parserContext, deleteDataOperation);

            //QuadData: ground triples and/or GRAPH quad blocks. AddDeleteTemplate enforces groundness (throws on a variable).
            foreach (RDFPattern templatePattern in ParseQuadsTemplate(parserContext, "DELETE DATA"))
                deleteDataOperation.AddDeleteTemplate(templatePattern);

            return deleteDataOperation;
        }

        /// <summary>
        /// Parses a <c>QuadData</c> block — <c>'{' Quads '}'</c> — into a flat list of template
        /// <see cref="RDFPattern"/> instances (three-argument triples in the default graph, four-argument quads
        /// inside a <c>GRAPH iri { … }</c> block). The template sink is installed for the duration of the block
        /// and removed afterwards, regardless of outcome.
        /// <para>
        /// Property paths are not allowed (QuadData has no path production): they are caught as members of the
        /// scratch group they would have populated. The "DATA" groundness restriction is NOT enforced here — it is
        /// applied by the caller's ground-only <c>AddInsert/DeleteTemplate</c>, which gives a precise error.
        /// </para>
        /// </summary>
        /// <returns>The template patterns read from the block, in document order.</returns>
        /// <exception cref="RDFQueryException">When the block is malformed or contains a property path.</exception>
        private static List<RDFPattern> ParseQuadsTemplate(RDFQueryParserContext parserContext, string operationName)
        {
            //Opening brace of the QuadData
            ExpectChar(parserContext, '{', operationName + " quad data");

            //Install the sink (EmitPattern diverts triples here) and a scratch group catching stray property paths
            List<RDFPattern> templatePatterns = new List<RDFPattern>();
            RDFPatternGroup scratchGroupForPaths = new RDFPatternGroup();
            parserContext.TemplatePatternSink = templatePatterns;
            try
            {
                while (true)
                {
                    int nextSignificantCodePoint = SkipWhitespace(parserContext);

                    //A '}' ends the QuadData; EOF terminates so the closing-brace check below surfaces a clean error
                    if (nextSignificantCodePoint == '}' || nextSignificantCodePoint == -1)
                        break;

                    //A '.' separates a triples run / GRAPH block from the next element: consume and continue
                    if (nextSignificantCodePoint == '.')
                    {
                        ReadCodePoint(parserContext);
                        continue;
                    }

                    //A bare keyword (a letter run NOT immediately followed by ':', i.e. not a prefixed name) can
                    //only be 'GRAPH' here; any other bare keyword is illegal inside QuadData
                    string upcomingKeyword = ReadKeyword(parserContext);
                    if (upcomingKeyword.Length > 0 && PeekCodePoint(parserContext) != ':')
                    {
                        if (upcomingKeyword.Equals("GRAPH", System.StringComparison.OrdinalIgnoreCase))
                        {
                            ParseQuadsGraphBlock(parserContext, scratchGroupForPaths, operationName);
                            continue;
                        }

                        throw new RDFQueryException("Cannot parse SPARQL " + operationName + ": unexpected '" + upcomingKeyword + "' (only ground triples and 'GRAPH <iri> { … }' blocks are allowed) " + GetCoordinates(parserContext));
                    }

                    //Otherwise it is a triple: push back any prefixed-name label we peeked and read the triples run
                    if (upcomingKeyword.Length > 0)
                        UnreadString(parserContext, upcomingKeyword);
                    RDFQueryParser.ParseTriplesBlock(parserContext, scratchGroupForPaths);
                }

                //A property path is not allowed in QuadData (only plain triples)
                if (scratchGroupForPaths.GetPropertyPaths().Any())
                    throw new RDFQueryException("Cannot parse SPARQL " + operationName + ": property paths are not allowed (only plain triples) " + GetCoordinates(parserContext));
            }
            finally
            {
                //Remove the sink so nothing else accidentally diverts into it
                parserContext.TemplatePatternSink = null;
            }

            //Closing brace of the QuadData
            ExpectChar(parserContext, '}', operationName + " quad data");

            return templatePatterns;
        }

        /// <summary>
        /// Parses a <c>GRAPH iri { TriplesTemplate? }</c> quad block (the 'GRAPH' keyword has already been
        /// consumed): the graph context is fixed for the duration of the inner triples so they are emitted as
        /// four-argument quads, then the enclosing context is restored. An empty block is legal (it simply yields
        /// no quads, unlike a query GRAPH whose empty group is rejected).
        /// </summary>
        /// <exception cref="RDFQueryException">When the graph specifier or the braces are malformed.</exception>
        private static void ParseQuadsGraphBlock(RDFQueryParserContext parserContext, RDFPatternGroup scratchGroupForPaths, string operationName)
        {
            //Parse the VarOrIri graph specifier (a variable one survives into the quad and is later rejected as non-ground)
            RDFPatternMember graphContext = RDFQueryParser.ParseGraphContext(parserContext);

            //Fix this block's context for the inner triples (save → set → … → restore); no emptiness check is needed
            (RDFPatternMember ActiveContext, bool WasUsed) enclosingGraphScope = parserContext.CurrentGraphScope;
            parserContext.CurrentGraphScope = (graphContext, false);

            //'{' TriplesTemplate? '}' — the inner triples are emitted as quads carrying graphContext
            ExpectChar(parserContext, '{', operationName + " GRAPH block");
            RDFQueryParser.ParseTriplesBlock(parserContext, scratchGroupForPaths);
            ExpectChar(parserContext, '}', operationName + " GRAPH block");

            //Restore the enclosing scope for whatever follows this GRAPH block
            parserContext.CurrentGraphScope = enclosingGraphScope;
        }
        #endregion
    }
}
