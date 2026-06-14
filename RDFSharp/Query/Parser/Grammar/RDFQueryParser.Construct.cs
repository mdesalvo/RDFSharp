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

namespace RDFSharp.Query
{
    /// <summary>
    /// CONSTRUCT-form half of the SPARQL parser: the body of a CONSTRUCT query — its graph TEMPLATE plus the
    /// WHERE clause — assembled into an <see cref="RDFConstructQuery"/> once the 'CONSTRUCT' keyword has been
    /// dispatched. CONSTRUCT builds an RDF graph by instantiating the template once per WHERE solution.
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// ConstructQuery ::= 'CONSTRUCT' ( ConstructTemplate DatasetClause* WhereClause SolutionModifier
    ///                                | DatasetClause* 'WHERE' '{' TriplesTemplate? '}' SolutionModifier )
    /// ConstructTemplate ::= '{' ConstructTriples? '}'
    /// ConstructTriples  ::= TriplesSameSubject ( '.' ConstructTriples? )?
    /// </code>
    /// Both forms are handled: the LONG form <c>CONSTRUCT { template } WHERE { … }</c> and the SHORT form
    /// <c>CONSTRUCT WHERE { triples }</c>, where the template coincides with the WHERE triples.
    /// </para>
    /// <para>
    /// Model-imposed limits (the flat <see cref="RDFConstructQuery"/> has no dataset and only LIMIT/OFFSET
    /// modifier slots): a <c>DatasetClause</c> (FROM / FROM NAMED) and the ORDER BY / GROUP BY / HAVING
    /// solution modifiers are spec-legal but NOT representable, so each raises an explicit
    /// <see cref="RDFQueryException"/> rather than being silently dropped. Property paths and non-triple
    /// elements (FILTER/OPTIONAL/…) are not allowed inside a template by the grammar and are likewise rejected.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region ConstructQuery
        /// <summary>
        /// Parses the body of a CONSTRUCT query (the 'CONSTRUCT' keyword has already been consumed by the
        /// dispatcher): the graph template, the optional dataset clauses (rejected as non-representable), the
        /// WHERE clause, and the trailing solution modifiers (only LIMIT/OFFSET are representable). The prologue's
        /// declared prefixes are re-attached so the query re-serializes its prologue identically.
        /// </summary>
        /// <exception cref="RDFQueryException">When the template, WHERE clause, dataset clause or a solution modifier is malformed/non-representable.</exception>
        private static RDFConstructQuery ParseConstructQuery(RDFQueryParserContext parserContext)
        {
            RDFConstructQuery constructQuery = new RDFConstructQuery();

            //Carry the prologue's declared prefixes onto the query so its printed prologue matches the input
            ApplyDeclaredPrefixes(parserContext, constructQuery);

            //The two CONSTRUCT forms are told apart by their first significant token: a '{' opens the LONG form's
            //explicit ConstructTemplate, anything else (the 'WHERE' keyword, possibly after a dataset clause) is
            //the SHORT form whose template is the WHERE triples themselves.
            if (SkipWhitespace(parserContext) == '{')
            {
                //LONG form: 'CONSTRUCT' ConstructTemplate DatasetClause* WhereClause SolutionModifier
                ParseConstructTemplate(parserContext, constructQuery);

                //DatasetClause* (FROM / FROM NAMED): spec-legal between template and WHERE, but the flat model
                //has no dataset to bind them to (same non-representable limit as ASK/SELECT) → reject explicitly
                RejectDatasetClause(parserContext);

                //WHERE clause (the keyword itself is optional in SPARQL)
                ParseWhereClause(parserContext, constructQuery);
            }
            else
            {
                //SHORT form: 'CONSTRUCT' DatasetClause* 'WHERE' '{' TriplesTemplate? '}' SolutionModifier
                RejectDatasetClause(parserContext);
                ParseConstructShortForm(parserContext, constructQuery);
            }

            //SolutionModifier: only LIMIT/OFFSET are representable on a CONSTRUCT query (ORDER BY / GROUP BY /
            //HAVING are rejected as non-representable). Shared with DESCRIBE via ParseLimitOffsetOnlyModifiers.
            ParseLimitOffsetOnlyModifiers(parserContext, constructQuery, "CONSTRUCT");

            return constructQuery;
        }

        /// <summary>
        /// Parses an explicit <c>ConstructTemplate</c> — the <c>{ ConstructTriples? }</c> block of the LONG form —
        /// and attaches every triple it produces to <paramref name="constructQuery"/> as a TEMPLATE pattern.
        /// <para>
        /// The whole triple machinery (predicate-object/object lists, the <c>a</c> verb, blank nodes and
        /// collections) is reused verbatim via <see cref="ParseTriplesBlock"/>; emission is diverted into the
        /// context's <see cref="RDFQueryParserContext.ConstructTemplateSink"/> so ground triples survive (see the
        /// sink's documentation). Property paths and non-triple elements are not allowed by the template grammar
        /// and are rejected.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the braces are unbalanced, or the template contains a property path or a non-triple element.</exception>
        private static void ParseConstructTemplate(RDFQueryParserContext parserContext, RDFConstructQuery constructQuery)
        {
            //Opening brace of the ConstructTemplate
            ExpectChar(parserContext, '{', "CONSTRUCT template");

            //Read the template triples into a fresh sink (so they reach AddTemplate, not a pattern group)
            List<RDFPattern> templatePatterns = ParseTemplateTriples(parserContext);

            //Closing brace of the ConstructTemplate
            ExpectChar(parserContext, '}', "CONSTRUCT template");

            //Attach every parsed template pattern to the query (AddTemplate also collects their variables)
            foreach (RDFPattern templatePattern in templatePatterns)
                constructQuery.AddTemplate(templatePattern);
        }

        /// <summary>
        /// Parses the SHORT form body <c>'WHERE' '{' TriplesTemplate? '}'</c>: the same triples serve BOTH as the
        /// graph template and as the WHERE pattern. The 'WHERE' keyword is mandatory in this form.
        /// <para>
        /// The triples are read once (through the template sink, so ground triples are preserved for the
        /// template); each is then added as a template pattern and, via a pattern group, as the WHERE clause body.
        /// Ground triples are naturally dropped from the WHERE group by <see cref="RDFPatternGroup.AddPattern"/>
        /// (which keeps only variable-bearing patterns) — the correct behaviour for a graph pattern.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When 'WHERE' is missing, the braces are unbalanced, or the triples contain a property path or a non-triple element.</exception>
        private static void ParseConstructShortForm(RDFQueryParserContext parserContext, RDFConstructQuery constructQuery)
        {
            //'WHERE' is mandatory in the SHORT form (unlike the optional WHERE of a normal WhereClause)
            if (!TryConsumeKeyword(parserContext, "WHERE"))
                throw new RDFQueryException("Cannot parse SPARQL CONSTRUCT query: expected a template '{ … }' or the 'WHERE' keyword after 'CONSTRUCT' " + GetCoordinates(parserContext));

            //Opening brace of the WHERE/TriplesTemplate block
            ExpectChar(parserContext, '{', "CONSTRUCT WHERE template");

            //Read the triples once (through the sink, so ground triples are preserved for the template)
            List<RDFPattern> templatePatterns = ParseTemplateTriples(parserContext);

            //Closing brace
            ExpectChar(parserContext, '}', "CONSTRUCT WHERE template");

            //The same triples are both the template and the WHERE body
            RDFPatternGroup whereClausePatternGroup = new RDFPatternGroup();
            foreach (RDFPattern templatePattern in templatePatterns)
            {
                constructQuery.AddTemplate(templatePattern);
                whereClausePatternGroup.AddPattern(templatePattern);
            }
            constructQuery.AddPatternGroup(whereClausePatternGroup);
        }

        /// <summary>
        /// Reads a <c>ConstructTriples</c> run (the caller owns the surrounding braces) into a flat list of
        /// template <see cref="RDFPattern"/> instances. The context's template sink is installed for the duration
        /// of the read and removed afterwards, regardless of outcome.
        /// <para>
        /// Two template-only restrictions are enforced after the read: a property path
        /// (<c>:p/:q</c>) is illegal because <c>ConstructTriples</c> uses <c>TriplesSameSubject</c> (no paths) —
        /// paths are detected as members of the scratch group they would have populated; and any element that is
        /// not a plain triple (a FILTER / OPTIONAL / nested group etc., which <see cref="ParseTriplesBlock"/>
        /// leaves on the reader) is rejected because the next significant token is then not the closing brace.
        /// </para>
        /// </summary>
        /// <returns>The template patterns read from the block, in document order.</returns>
        /// <exception cref="RDFQueryException">When the block contains a property path or a non-triple element.</exception>
        private static List<RDFPattern> ParseTemplateTriples(RDFQueryParserContext parserContext)
        {
            //Install the sink that EmitPattern diverts every emitted triple into (bypassing the pattern-group
            //variable guard so ground template triples are preserved). A scratch group only catches the
            //RDFPropertyPath members (which never go through EmitPattern) so we can reject them.
            List<RDFPattern> templatePatterns = new List<RDFPattern>();
            RDFPatternGroup scratchGroupForPaths = new RDFPatternGroup();
            parserContext.ConstructTemplateSink = templatePatterns;
            try
            {
                //Reuse the full triple machinery: it fills the sink with patterns and the scratch group with paths
                ParseTriplesBlock(parserContext, scratchGroupForPaths);

                //A property path is not allowed in a template (ConstructTriples has no path production)
                if (scratchGroupForPaths.GetPropertyPaths().Any())
                    throw new RDFQueryException("Cannot parse SPARQL CONSTRUCT template: property paths are not allowed in a template (only plain triples) " + GetCoordinates(parserContext));

                //ParseTriplesBlock stops at a block boundary; in a template the only legal boundary is the closing
                //brace. Anything else (a FILTER/OPTIONAL/nested group keyword or '{') is a non-triple element,
                //which the template grammar forbids.
                if (SkipWhitespace(parserContext) != '}')
                    throw new RDFQueryException("Cannot parse SPARQL CONSTRUCT template: only plain triples are allowed (FILTER, OPTIONAL, sub-groups and other graph patterns are not) " + GetCoordinates(parserContext));

                return templatePatterns;
            }
            finally
            {
                //Remove the sink so the subsequent WHERE clause is parsed normally (into pattern groups)
                parserContext.ConstructTemplateSink = null;
            }
        }
        #endregion
    }
}
