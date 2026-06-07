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
using System.Globalization;
using System.IO;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryParser is responsible for turning a SPARQL 1.1 query string into an RDFQuery object model instance
    /// </summary>
    internal static class RDFQueryParser
    {
        #region Context
        /// <summary>
        /// Holds the mutable state of a single SPARQL parse: the pull-style reader over the command text,
        /// and the autonomous resolver that accumulates the prologue's BASE/PREFIX declarations.
        /// </summary>
        internal sealed class RDFQueryParserContext
        {
            #region Properties
            /// <summary>
            /// RDFTurtle parsing context: it carries the RDFPushbackReader and the term Resolver, so
            /// that every RDFTurtle term-parser (ParseURI, ParseValue, ParseQNameOrBoolean, ...) can be invoked
            /// verbatim against this very context.
            /// </summary>
            internal RDFTurtle.RDFTurtleContext TermParsingContext { get; }

            /// <summary>
            /// Autonomous resolver populated by this query's prologue. It is the Resolver wired into
            /// <see cref="TermParsingContext"/>, exposed here so the parser can push BASE/PREFIX into it.
            /// </summary>
            internal RDFSPARQLTermResolver Resolver { get; }
            #endregion

            #region Ctors
            /// <summary>
            /// Builds a parsing context over the given reader of SPARQL command text, wiring a fresh
            /// autonomous resolver into a reused Turtle term-parsing context.
            /// </summary>
            internal RDFQueryParserContext(TextReader sparqlCommandReader)
            {
                Resolver = new RDFSPARQLTermResolver();
                TermParsingContext = new RDFTurtle.RDFTurtleContext
                {
                    //The term-parsers resolve base IRI and prefixes through the SPARQL resolver, NOT through a graph
                    Resolver = Resolver,
                    //Pull code points on demand from the SPARQL command, exactly as the Turtle deserializer does
                    Reader = new RDFTurtle.RDFPushbackReader(sparqlCommandReader)
                };
            }
            #endregion
        }

        /// <summary>
        /// Creates a parsing context over the given SPARQL command text.
        /// </summary>
        internal static RDFQueryParserContext CreateContext(string sparqlCommandText)
            => new RDFQueryParserContext(new StringReader(sparqlCommandText ?? string.Empty));
        #endregion

        #region Lexer
        /// <summary>
        /// Skips any run of whitespace and #-style comments, leaving the reader positioned on the next
        /// significant character. Returns the code point of that character (without consuming it), or -1 at EOF.
        /// Thin pass-through to RDFTurtle so SPARQL and Turtle share identical whitespace/comment handling.
        /// </summary>
        internal static int SkipWhitespace(RDFQueryParserContext parserContext)
            => RDFTurtle.SkipWhitespace(parserContext.TermParsingContext);

        /// <summary>
        /// Reads a maximal run of ASCII letters starting at the current position and returns it as a string
        /// (empty if the current character is not an ASCII letter). The first non-letter character is unread,
        /// so the reader is left exactly at the boundary of the run. SPARQL keywords (SELECT, BASE, PREFIX,
        /// ASK, ...) are pure ASCII letters, so this is the primitive used to peek/consume them; callers that
        /// only wanted to peek can push the run back with <see cref="UnreadString"/>.
        /// </summary>
        internal static string ReadKeyword(RDFQueryParserContext parserContext)
        {
            //Work directly on the reused Turtle parsing context: it owns the pushback reader that lets us
            //read the keyword run and then rewind the single character that marked its end.
            RDFTurtle.RDFTurtleContext termContext = parserContext.TermParsingContext;

            //Accumulate the consecutive ASCII letters that make up the keyword. We do NOT skip leading
            //whitespace here on purpose: callers are expected to have positioned the reader on the first
            //significant character already (typically via SkipWhitespace), so an empty result unambiguously
            //means "the cursor is not on a keyword" rather than "we skipped over some spaces".
            StringBuilder keywordRun = new StringBuilder();

            //Read the first code point; if it is not a letter the loop never runs and we return the empty string.
            int codePoint = RDFTurtle.ReadCodePoint(termContext);
            while (IsAsciiLetter(codePoint))
            {
                //This code point belongs to the keyword: append it (AppendCodePoint handles supplementary
                //planes too, though SPARQL keywords are always plain ASCII) and advance to the next one.
                RDFTurtle.AppendCodePoint(keywordRun, codePoint);
                codePoint = RDFTurtle.ReadCodePoint(termContext);
            }

            //The loop exits on the first non-letter code point (a space, punctuation, '<', '?', or even -1 at
            //EOF). That character does NOT belong to the keyword, so we push it back onto the reader: this keeps
            //ReadKeyword non-destructive past the keyword boundary and lets the caller resume exactly there
            //(e.g. ParsePrologue dispatching on the keyword, or the body parser reading the following token).
            RDFTurtle.UnreadCodePoint(termContext, codePoint);

            //Note: the keyword is returned VERBATIM, preserving its original casing. SPARQL keywords are
            //case-insensitive, so comparisons against this result must use a case-insensitive ordinal compare.
            return keywordRun.ToString();
        }

        /// <summary>
        /// Pushes the characters of the given string back onto the reader, preserving their original order,
        /// so a subsequent read returns them again. Used to "un-peek" a keyword run that turned out not to be
        /// the one we were looking for.
        /// </summary>
        internal static void UnreadString(RDFQueryParserContext parserContext, string alreadyReadText)
            => RDFTurtle.UnreadCodePoint(parserContext.TermParsingContext, alreadyReadText);

        /// <summary>
        /// Parses a SPARQL variable: the sigil <c>?</c> or <c>$</c> followed by a non-empty VARNAME.
        /// Both sigils denote the same variable, so they are interchangeable (RDFVariable normalizes the name).
        /// </summary>
        /// <exception cref="RDFQueryException">When no sigil is present or the variable name is empty.</exception>
        internal static RDFVariable ParseVariable(RDFQueryParserContext parserContext)
        {
            RDFTurtle.RDFTurtleContext termContext = parserContext.TermParsingContext;

            //A variable must open with the '?' or '$' sigil
            int sigilCodePoint = RDFTurtle.ReadCodePoint(termContext);
            if (sigilCodePoint != '?' && sigilCodePoint != '$')
                throw new RDFQueryException("Cannot parse SPARQL variable: expected '?' or '$' " + GetCoordinates(parserContext));

            //Read the VARNAME body
            StringBuilder variableName = new StringBuilder();
            int codePoint = RDFTurtle.ReadCodePoint(termContext);

            //The first VARNAME character has a slightly stricter rule than the following ones
            if (!IsVarNameStartChar(codePoint))
            {
                RDFTurtle.UnreadCodePoint(termContext, codePoint);
                throw new RDFQueryException("Cannot parse SPARQL variable: empty or invalid variable name " + GetCoordinates(parserContext));
            }
            RDFTurtle.AppendCodePoint(variableName, codePoint);

            //Consume the remaining VARNAME characters
            codePoint = RDFTurtle.ReadCodePoint(termContext);
            while (IsVarNameChar(codePoint))
            {
                RDFTurtle.AppendCodePoint(variableName, codePoint);
                codePoint = RDFTurtle.ReadCodePoint(termContext);
            }
            //Unread the first character that does not belong to the variable name
            RDFTurtle.UnreadCodePoint(termContext, codePoint);

            //RDFVariable normalizes the name (strips sigils, upper-cases): pass the bare VARNAME
            return new RDFVariable(variableName.ToString());
        }

        /// <summary>
        /// Parses a single RDF term (IRI reference, prefixed name, blank node, literal or numeric literal) by
        /// delegating to RDFTurtle's ParseValue and normalizing the result into an RDFPatternMember. Prefix and
        /// base resolution flow through the SPARQL resolver wired into the context.
        /// </summary>
        /// <exception cref="RDFQueryException">When the text at the cursor is not a valid RDF term.</exception>
        internal static RDFPatternMember ParseTerm(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);
            try
            {
                //RDFTurtle.ParseValue returns either a System.Uri (for IRIs/prefixed names) or an
                //RDFPatternMember (RDFResource for blank nodes, RDFLiteral for quoted/numeric/boolean literals)
                object parsedValue = RDFTurtle.ParseValue(parserContext.TermParsingContext);
                switch (parsedValue)
                {
                    case Uri parsedIri:
                        return new RDFResource(parsedIri.ToString());
                    case RDFPatternMember parsedPatternMember:
                        return parsedPatternMember;
                    default:
                        throw new RDFQueryException("Cannot parse SPARQL term: unexpected value '" + parsedValue + "' " + GetCoordinates(parserContext));
                }
            }
            catch (RDFModelException termParsingException)
            {
                //Surface term-level lexing failures as query-level parse errors (the parser speaks RDFQueryException)
                throw new RDFQueryException("Cannot parse SPARQL term " + GetCoordinates(parserContext) + ": " + termParsingException.Message, termParsingException);
            }
        }

        /// <summary>
        /// Returns a human-readable "[LINE:x,COL:y]" coordinate string for error messages, reusing the
        /// position tracking of the underlying reader.
        /// </summary>
        private static string GetCoordinates(RDFQueryParserContext parserContext)
            => RDFTurtle.GetTurtleContextCoordinates(parserContext.TermParsingContext);
        #endregion

        #region Prologue
        /// <summary>
        /// <para>
        /// Parses the SPARQL prologue — a (possibly empty) sequence of <c>BASE &lt;iri&gt;</c> and
        /// <c>PREFIX label: &lt;iri&gt;</c> declarations — populating the context's resolver as it goes.
        /// </para>
        /// <para>
        /// Parsing stops at the first token that is neither BASE nor PREFIX (typically the query form
        /// keyword SELECT/ASK/CONSTRUCT/DESCRIBE, or an opening brace). That token is left unconsumed in
        /// the reader so the caller can dispatch on it.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When a BASE/PREFIX declaration is malformed.</exception>
        internal static void ParsePrologue(RDFQueryParserContext parserContext)
        {
            while (true)
            {
                //Position the reader on the next significant character
                SkipWhitespace(parserContext);

                //Read the upcoming keyword run; this consumes it, so non-prologue keywords are pushed back below
                string keyword = ReadKeyword(parserContext);

                //SPARQL keywords are case-insensitive: compare ignoring case
                if (keyword.Equals("BASE", StringComparison.OrdinalIgnoreCase))
                {
                    ParseBaseDeclaration(parserContext);
                }
                else if (keyword.Equals("PREFIX", StringComparison.OrdinalIgnoreCase))
                {
                    ParsePrefixDeclaration(parserContext);
                }
                else
                {
                    //Not a prologue keyword: push it back untouched and hand control to the caller
                    UnreadString(parserContext, keyword);
                    return;
                }
            }
        }

        /// <summary>
        /// Parses the body of a <c>BASE &lt;absoluteIri&gt;</c> declaration (the BASE keyword has already been
        /// consumed) and records the base IRI into the resolver. A later BASE overrides an earlier one.
        /// </summary>
        private static void ParseBaseDeclaration(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);
            try
            {
                //Reuse the Turtle IRI reader; resolution of relative IRIs uses whatever base is currently set
                Uri baseIri = RDFTurtle.ParseURI(parserContext.TermParsingContext);
                parserContext.Resolver.SetBaseIri(baseIri.ToString());
            }
            catch (RDFModelException iriParsingException)
            {
                throw new RDFQueryException("Cannot parse SPARQL BASE declaration " + GetCoordinates(parserContext) + ": " + iriParsingException.Message, iriParsingException);
            }
        }

        /// <summary>
        /// Parses the body of a <c>PREFIX label: &lt;namespaceIri&gt;</c> declaration (the PREFIX keyword has
        /// already been consumed) and registers the prefix-to-namespace binding into the resolver. The label
        /// may be empty (the default namespace, declared as <c>PREFIX : &lt;...&gt;</c>).
        /// </summary>
        private static void ParsePrefixDeclaration(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);

            //Read the prefix label up to (and consuming) the mandatory ':' terminator. Per the SPARQL grammar
            //(PNAME_NS ::= PN_PREFIX? ':') there is no whitespace between the label and the colon.
            string prefixLabel = ReadPrefixLabel(parserContext);

            SkipWhitespace(parserContext);
            try
            {
                //The namespace is a plain IRI reference
                Uri namespaceIri = RDFTurtle.ParseURI(parserContext.TermParsingContext);
                parserContext.Resolver.RegisterPrefix(prefixLabel, namespaceIri.ToString());
            }
            catch (RDFModelException iriParsingException)
            {
                throw new RDFQueryException("Cannot parse SPARQL PREFIX declaration " + GetCoordinates(parserContext) + ": " + iriParsingException.Message, iriParsingException);
            }
        }

        /// <summary>
        /// Reads a prefix label (the part before the ':' of a PNAME_NS) and consumes the trailing ':'.
        /// Returns the label, which is the empty string for the default namespace declaration <c>PREFIX :</c>.
        /// </summary>
        /// <exception cref="RDFQueryException">When the ':' terminator is missing.</exception>
        private static string ReadPrefixLabel(RDFQueryParserContext parserContext)
        {
            RDFTurtle.RDFTurtleContext termContext = parserContext.TermParsingContext;

            StringBuilder prefixLabel = new StringBuilder();
            int codePoint = RDFTurtle.ReadCodePoint(termContext);
            while (codePoint != ':')
            {
                //A PREFIX label must be terminated by ':' before any whitespace or end of input
                if (codePoint == -1 || RDFTurtle.IsWhitespace(codePoint))
                    throw new RDFQueryException("Cannot parse SPARQL PREFIX declaration: expected ':' to terminate the prefix label " + GetCoordinates(parserContext));

                RDFTurtle.AppendCodePoint(prefixLabel, codePoint);
                codePoint = RDFTurtle.ReadCodePoint(termContext);
            }
            //codePoint is the ':' here, already consumed
            return prefixLabel.ToString();
        }
        #endregion

        #region Query
        /// <summary>
        /// <para>
        /// Parses a complete SPARQL query string into the corresponding RDFQuery object-model instance.
        /// This is the single entry point of the engine: it builds a fresh parsing context over the text,
        /// consumes the (possibly empty) prologue of BASE/PREFIX declarations, then dispatches on the query
        /// form keyword (SELECT/ASK/CONSTRUCT/DESCRIBE) to the matching form-parser.
        /// </para>
        /// <para>
        /// Phase note: only SELECT is implemented so far; the other forms are recognized (so that the error
        /// message is precise) but rejected until their phase lands.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the text is empty, the form keyword is missing/unknown, or the body is malformed.</exception>
        internal static RDFQuery ParseQuery(string sparqlQueryText)
        {
            //Fail fast on a missing/blank command: there is nothing to parse, and every downstream step assumes
            //the reader has at least one significant character to work on. We reject here rather than returning an
            //empty query so that callers get an explicit, actionable error instead of a silently degenerate result.
            if (string.IsNullOrWhiteSpace(sparqlQueryText))
                throw new RDFQueryException("Cannot parse SPARQL query because the given command text is null or empty");

            //Build the per-parse state: a pull-style reader over the command text plus the autonomous resolver
            //that the prologue will populate. The context is single-use (it carries reader position and accumulated
            //prefixes/base), so a fresh one is created for every ParseQuery invocation and never shared.
            RDFQueryParserContext parserContext = CreateContext(sparqlQueryText);

            //STEP 1 - Prologue. A SPARQL query opens with a (possibly empty) run of BASE/PREFIX declarations.
            //ParsePrologue consumes them and feeds the base IRI and prefix-to-namespace bindings into the context
            //resolver, then stops as soon as it meets a token that is neither BASE nor PREFIX, leaving that token
            //(the query form keyword) unconsumed in the reader for us to dispatch on below.
            ParsePrologue(parserContext);

            //STEP 2 - Query form. Skip to the next significant character and read the keyword run that names the
            //query form. ReadKeyword returns it VERBATIM (original casing), so we upper-case before matching to
            //honour SPARQL's case-insensitive keywords. An empty result means the reader is not on a keyword at all.
            SkipWhitespace(parserContext);
            string queryForm = ReadKeyword(parserContext);
            switch (queryForm.ToUpperInvariant())
            {
                //SELECT is the only form implemented so far: hand the rest of the input to its dedicated body-parser,
                //which returns the fully-built RDFSelectQuery (upcast to RDFQuery for the caller).
                case "SELECT":
                    return ParseSelectQuery(parserContext);

                //These are valid SPARQL forms that simply have not been wired up yet. We match them explicitly so the
                //error names the exact form the author used ("'ASK' ... not supported yet") instead of the misleading
                //"unexpected token" message of the default branch. Each gets its real parser in a later phase.
                case "ASK":
                case "CONSTRUCT":
                case "DESCRIBE":
                    throw new RDFQueryException("Cannot parse SPARQL query: '" + queryForm.ToUpperInvariant() + "' queries are not supported yet " + GetCoordinates(parserContext));

                //Empty keyword: the input ended (or hit non-letter punctuation) right after the prologue, so no form
                //keyword was present at all. Typically a prologue-only string, or a query that opens with a stray brace.
                case "":
                    throw new RDFQueryException("Cannot parse SPARQL query: expected a query form keyword (SELECT, ASK, CONSTRUCT or DESCRIBE) " + GetCoordinates(parserContext));

                //A non-empty keyword that is none of the four query forms: the author wrote some other identifier
                //where a form keyword was required (e.g. a typo). Surface it verbatim to make the mistake obvious.
                default:
                    throw new RDFQueryException("Cannot parse SPARQL query: unexpected token '" + queryForm + "' where a query form keyword was expected " + GetCoordinates(parserContext));
            }
        }
        #endregion

        #region SelectQuery
        /// <summary>
        /// Parses the body of a SELECT query (the 'SELECT' keyword has already been consumed by the dispatcher):
        /// the optional DISTINCT/REDUCED modifier, the projection (<c>*</c> or a list of variables), the WHERE
        /// clause, and the trailing solution modifiers (ORDER BY / LIMIT / OFFSET). The PREFIX declarations
        /// accumulated by the prologue are re-attached to the query so it re-serializes its prologue identically.
        /// </summary>
        /// <exception cref="RDFQueryException">When any of the SELECT sub-clauses is malformed.</exception>
        private static RDFSelectQuery ParseSelectQuery(RDFQueryParserContext parserContext)
        {
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            //Carry the prologue's declared prefixes onto the query so its printed prologue matches the input
            ApplyDeclaredPrefixes(parserContext, selectQuery);

            //DISTINCT and REDUCED are mutually exclusive: DISTINCT becomes a modifier, REDUCED is a ratified no-op
            if (TryConsumeKeyword(parserContext, "DISTINCT"))
                selectQuery.AddModifier(new RDFDistinctModifier());
            else
                TryConsumeKeyword(parserContext, "REDUCED");

            //Projection: either the '*' wildcard (empty ProjectionVars means "all variables") or a list of variables
            ParseSelectProjection(parserContext, selectQuery);

            //WHERE clause (the keyword itself is optional in SPARQL)
            ParseWhereClause(parserContext, selectQuery);

            //ORDER BY / LIMIT / OFFSET (any order, leniently)
            ParseSolutionModifiers(parserContext, selectQuery);

            return selectQuery;
        }

        /// <summary>
        /// Parses the SELECT projection: a single <c>*</c> wildcard, or a non-empty whitespace-separated list of
        /// variables. A leading '(' would introduce an <c>(expr AS ?var)</c> projection expression, which is not
        /// supported yet (it needs the full expression parser of a later phase) and is rejected with a clear message.
        /// </summary>
        /// <exception cref="RDFQueryException">When the projection is empty, or uses an unsupported '(expr AS ?var)' form.</exception>
        private static void ParseSelectProjection(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            //The '*' wildcard projects every in-scope variable: it is modeled by leaving ProjectionVars empty
            if (TryConsumeChar(parserContext, '*'))
                return;

            //Otherwise we expect one or more projection variables
            bool foundAtLeastOneProjectionVariable = false;
            while (true)
            {
                int nextCodePoint = SkipWhitespace(parserContext);

                //A '?' or '$' sigil starts a projection variable
                if (nextCodePoint == '?' || nextCodePoint == '$')
                {
                    selectQuery.AddProjectionVariable(ParseVariable(parserContext));
                    foundAtLeastOneProjectionVariable = true;
                    continue;
                }

                //A '(' would open an '(expr AS ?var)' projection expression: deferred to the expression phase
                if (nextCodePoint == '(')
                    throw new RDFQueryException("Cannot parse SPARQL SELECT projection: '(expr AS ?var)' projection expressions are not supported yet " + GetCoordinates(parserContext));

                //Anything else (typically the WHERE keyword or '{') ends the projection list
                break;
            }

            if (!foundAtLeastOneProjectionVariable)
                throw new RDFQueryException("Cannot parse SPARQL SELECT projection: expected '*' or at least one variable " + GetCoordinates(parserContext));
        }
        #endregion

        #region WhereClause
        /// <summary>
        /// Parses the WHERE clause and attaches its pattern groups to the query. The 'WHERE' keyword is optional
        /// (per the SPARQL grammar, <c>WhereClause ::= 'WHERE'? GroupGraphPattern</c>). The group graph pattern
        /// is delimited by braces and, in this phase, may contain basic graph patterns (triples) either inline or
        /// wrapped in nested <c>{ ... }</c> blocks — each block becomes one RDFPatternGroup, mirroring how the
        /// printer emits a pattern group inside its own braces.
        /// </summary>
        /// <exception cref="RDFQueryException">When the braces are unbalanced or a contained pattern is malformed.</exception>
        private static void ParseWhereClause(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            //The 'WHERE' keyword is optional: consume it if present, otherwise proceed straight to the '{'
            TryConsumeKeyword(parserContext, "WHERE");

            //Open the top-level group graph pattern
            ExpectChar(parserContext, '{', "WHERE clause");

            //Read group members until the matching '}'
            while (true)
            {
                int nextCodePoint = SkipWhitespace(parserContext);

                //End of the group graph pattern
                if (nextCodePoint == '}')
                {
                    ReadCodePoint(parserContext);
                    return;
                }

                //Unexpected end of input before the closing brace
                if (nextCodePoint == -1)
                    throw new RDFQueryException("Cannot parse SPARQL WHERE clause: expected '}' to close the group but reached end of input " + GetCoordinates(parserContext));

                //A nested '{ ... }' block, or inline triples: either way the result is one pattern group
                RDFPatternGroup patternGroup = nextCodePoint == '{'
                    ? ParseBracedPatternGroup(parserContext)
                    : ParseInlinePatternGroup(parserContext);
                selectQuery.AddPatternGroup(patternGroup);
            }
        }

        /// <summary>
        /// Parses a brace-delimited pattern group <c>{ triples }</c> into an RDFPatternGroup. This is the shape
        /// the printer emits for every pattern group, so it is the common case during round-tripping.
        /// </summary>
        private static RDFPatternGroup ParseBracedPatternGroup(RDFQueryParserContext parserContext)
        {
            ExpectChar(parserContext, '{', "pattern group");
            RDFPatternGroup patternGroup = new RDFPatternGroup();
            ParseTriplesBlock(parserContext, patternGroup);
            ExpectChar(parserContext, '}', "pattern group");
            return patternGroup;
        }

        /// <summary>
        /// Parses a run of triples written directly inside the group graph pattern (no surrounding braces) into a
        /// single RDFPatternGroup. Supports hand-written queries such as <c>WHERE { ?s ?p ?o }</c> that skip the
        /// extra nesting the printer would otherwise add.
        /// </summary>
        private static RDFPatternGroup ParseInlinePatternGroup(RDFQueryParserContext parserContext)
        {
            RDFPatternGroup patternGroup = new RDFPatternGroup();
            ParseTriplesBlock(parserContext, patternGroup);
            return patternGroup;
        }
        #endregion

        #region TriplesBlock
        /// <summary>
        /// Parses a basic graph pattern (a Turtle-style block of triples) into the given pattern group. Triples are
        /// separated by '.', and each triple may use predicate-object lists (';') and object lists (',') exactly as
        /// in Turtle. Parsing stops at the block boundary: a '}' (end of the enclosing group), a '{' (a following
        /// nested group), or end of input.
        /// </summary>
        /// <exception cref="RDFQueryException">When a triple inside the block is malformed.</exception>
        private static void ParseTriplesBlock(RDFQueryParserContext parserContext, RDFPatternGroup patternGroup)
        {
            while (true)
            {
                int nextCodePoint = SkipWhitespace(parserContext);

                //Block boundary: a closing brace, a nested group, or end of input ends the basic graph pattern
                if (nextCodePoint == '}' || nextCodePoint == '{' || nextCodePoint == -1)
                    return;

                //Subject, then its predicate-object list, produce one or more triples
                RDFPatternMember subject = ParseVariableOrTerm(parserContext);
                ParsePredicateObjectList(parserContext, patternGroup, subject);

                //A '.' separates triples; it is optional before the block boundary (last triple may omit it)
                if (SkipWhitespace(parserContext) == '.')
                {
                    ReadCodePoint(parserContext);
                    continue;
                }

                //No '.': we must be at the block boundary, which the caller will validate
                return;
            }
        }

        /// <summary>
        /// Parses a predicate-object list for the given subject: one or more <c>predicate objectList</c> groups
        /// separated by ';'. A trailing ';' before the triple terminator is tolerated, as in Turtle.
        /// </summary>
        private static void ParsePredicateObjectList(RDFQueryParserContext parserContext, RDFPatternGroup patternGroup, RDFPatternMember subject)
        {
            while (true)
            {
                //Predicate, then the comma-separated list of objects sharing this subject+predicate
                RDFPatternMember predicate = ParsePredicate(parserContext);
                ParseObjectList(parserContext, patternGroup, subject, predicate);

                //A ';' introduces another predicate-object group on the same subject
                if (SkipWhitespace(parserContext) == ';')
                {
                    ReadCodePoint(parserContext);

                    //Tolerate a trailing ';' immediately before the triple terminator or block boundary
                    int afterSemicolon = SkipWhitespace(parserContext);
                    if (afterSemicolon == '.' || afterSemicolon == '}' || afterSemicolon == '{' || afterSemicolon == -1)
                        return;

                    continue;
                }

                return;
            }
        }

        /// <summary>
        /// Parses an object list for the given subject+predicate: one or more objects separated by ',', emitting
        /// one RDFPattern per object into the pattern group.
        /// </summary>
        private static void ParseObjectList(RDFQueryParserContext parserContext, RDFPatternGroup patternGroup, RDFPatternMember subject, RDFPatternMember predicate)
        {
            while (true)
            {
                RDFPatternMember objectMember = ParseVariableOrTerm(parserContext);

                //The RDFPattern constructor enforces SPARQL term-position rules (e.g. literal subjects are rejected),
                //surfacing any violation as an RDFQueryException — exactly the parser's error contract
                patternGroup.AddPattern(new RDFPattern(subject, predicate, objectMember));

                //A ',' introduces another object sharing this subject+predicate
                if (SkipWhitespace(parserContext) == ',')
                {
                    ReadCodePoint(parserContext);
                    continue;
                }

                return;
            }
        }

        /// <summary>
        /// Parses a term in subject/object position: a variable (<c>?x</c>/<c>$x</c>) or any RDF term parsed by the
        /// shared Turtle term-reader (IRI, prefixed name, blank node, literal, numeric/boolean literal).
        /// </summary>
        private static RDFPatternMember ParseVariableOrTerm(RDFQueryParserContext parserContext)
        {
            int nextCodePoint = SkipWhitespace(parserContext);
            if (nextCodePoint == '?' || nextCodePoint == '$')
                return ParseVariable(parserContext);
            return ParseTerm(parserContext);
        }

        /// <summary>
        /// Parses a term in predicate position. In addition to variables and ordinary terms, it recognizes the
        /// SPARQL/Turtle verb <c>a</c> as a shorthand for rdf:type. The 'a' shorthand is only accepted when it
        /// stands alone (followed by whitespace or end of input); otherwise the leading 'a' is treated as the start
        /// of a longer token (e.g. a prefixed name like <c>a:b</c>) and handed to the general term-reader.
        /// </summary>
        private static RDFPatternMember ParsePredicate(RDFQueryParserContext parserContext)
        {
            int nextCodePoint = SkipWhitespace(parserContext);

            //A variable predicate
            if (nextCodePoint == '?' || nextCodePoint == '$')
                return ParseVariable(parserContext);

            //Possibly the 'a' verb (rdf:type)
            if (nextCodePoint == 'a')
            {
                //Consume the 'a' and look at what immediately follows it (without consuming that)
                ReadCodePoint(parserContext);
                int afterA = PeekCodePoint(parserContext);

                //'a' standing alone (delimited by whitespace or EOF) is the rdf:type shorthand
                if (afterA == -1 || RDFTurtle.IsWhitespace(afterA))
                    return new RDFResource(RDFVocabulary.RDF.TYPE.ToString());

                //Not standalone: 'a' is the first character of a longer token; push it back and parse normally
                UnreadCodePoint(parserContext, 'a');
            }

            //Any other predicate is an ordinary term (IRI or prefixed name); the RDFPattern constructor will
            //reject terms that are invalid in predicate position (blank nodes, literals)
            return ParseTerm(parserContext);
        }
        #endregion

        #region SolutionModifiers
        /// <summary>
        /// Parses the trailing solution modifiers of a query — ORDER BY, LIMIT and OFFSET — until a token that is
        /// none of them is reached (which is left unconsumed for the caller). The modifiers are accepted in any
        /// order for leniency; the object-model deduplicates repeats (e.g. a second LIMIT is ignored).
        /// </summary>
        /// <exception cref="RDFQueryException">When a recognized modifier has a malformed body.</exception>
        private static void ParseSolutionModifiers(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            while (true)
            {
                SkipWhitespace(parserContext);
                string keyword = ReadKeyword(parserContext);
                switch (keyword.ToUpperInvariant())
                {
                    case "ORDER":
                        ParseOrderByModifier(parserContext, selectQuery);
                        break;

                    case "LIMIT":
                        selectQuery.AddModifier(new RDFLimitModifier(ParseInteger(parserContext, "LIMIT")));
                        break;

                    case "OFFSET":
                        selectQuery.AddModifier(new RDFOffsetModifier(ParseInteger(parserContext, "OFFSET")));
                        break;

                    default:
                        //Not a modifier keyword: push it back untouched and hand control back to the caller
                        UnreadString(parserContext, keyword);
                        return;
                }
            }
        }

        /// <summary>
        /// Parses an ORDER BY clause (the 'ORDER' keyword has already been consumed): the mandatory 'BY' keyword
        /// followed by one or more order conditions. Each condition is either a bare variable (ascending by default)
        /// or an <c>ASC(?var)</c> / <c>DESC(?var)</c> directive. In this phase only variable conditions are
        /// supported (expression conditions belong to a later phase, and the modifier itself only carries a variable).
        /// </summary>
        /// <exception cref="RDFQueryException">When 'BY' is missing or no order condition is present.</exception>
        private static void ParseOrderByModifier(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            if (!TryConsumeKeyword(parserContext, "BY"))
                throw new RDFQueryException("Cannot parse SPARQL ORDER BY clause: expected 'BY' after 'ORDER' " + GetCoordinates(parserContext));

            bool foundAtLeastOneOrderCondition = false;
            while (true)
            {
                int nextCodePoint = SkipWhitespace(parserContext);

                //A bare variable orders ascending by default
                if (nextCodePoint == '?' || nextCodePoint == '$')
                {
                    selectQuery.AddModifier(new RDFOrderByModifier(ParseVariable(parserContext), RDFQueryEnums.RDFOrderByFlavors.ASC));
                    foundAtLeastOneOrderCondition = true;
                    continue;
                }

                //Otherwise the only accepted conditions are the ASC(...) / DESC(...) directives
                string directionKeyword = ReadKeyword(parserContext);
                string upperDirectionKeyword = directionKeyword.ToUpperInvariant();
                if (upperDirectionKeyword == "ASC" || upperDirectionKeyword == "DESC")
                {
                    ExpectChar(parserContext, '(', "ORDER BY condition");
                    RDFVariable orderVariable = ParseVariable(parserContext);
                    ExpectChar(parserContext, ')', "ORDER BY condition");

                    RDFQueryEnums.RDFOrderByFlavors orderFlavor = upperDirectionKeyword == "ASC"
                        ? RDFQueryEnums.RDFOrderByFlavors.ASC
                        : RDFQueryEnums.RDFOrderByFlavors.DESC;
                    selectQuery.AddModifier(new RDFOrderByModifier(orderVariable, orderFlavor));
                    foundAtLeastOneOrderCondition = true;
                    continue;
                }

                //Not an order condition: push back what we read (likely LIMIT/OFFSET) and stop
                if (directionKeyword.Length > 0)
                    UnreadString(parserContext, directionKeyword);
                break;
            }

            if (!foundAtLeastOneOrderCondition)
                throw new RDFQueryException("Cannot parse SPARQL ORDER BY clause: expected at least one variable to order by " + GetCoordinates(parserContext));
        }
        #endregion

        #region Lexer.Helpers
        /// <summary>
        /// Reads and returns the next code point from the underlying reader (or -1 at end of input).
        /// Thin pass-through to RDFTurtle so the SPARQL parser shares the exact same pull-style reading.
        /// </summary>
        private static int ReadCodePoint(RDFQueryParserContext parserContext)
            => RDFTurtle.ReadCodePoint(parserContext.TermParsingContext);

        /// <summary>
        /// Peeks the next code point without consuming it (or -1 at end of input).
        /// </summary>
        private static int PeekCodePoint(RDFQueryParserContext parserContext)
            => RDFTurtle.PeekCodePoint(parserContext.TermParsingContext);

        /// <summary>
        /// Pushes a single code point back onto the reader so a subsequent read returns it again.
        /// </summary>
        private static void UnreadCodePoint(RDFQueryParserContext parserContext, int codePoint)
            => RDFTurtle.UnreadCodePoint(parserContext.TermParsingContext, codePoint);

        /// <summary>
        /// Skips whitespace/comments and verifies that the next significant character is the expected one,
        /// consuming it. Used for the structural punctuation of the grammar ('{', '}', '(', ')').
        /// </summary>
        /// <exception cref="RDFQueryException">When the expected character is not found.</exception>
        private static void ExpectChar(RDFQueryParserContext parserContext, int expectedCodePoint, string grammarContext)
        {
            int nextCodePoint = SkipWhitespace(parserContext);
            if (nextCodePoint != expectedCodePoint)
                throw new RDFQueryException("Cannot parse SPARQL " + grammarContext + ": expected '" + (char)expectedCodePoint + "' but found " + DescribeCodePoint(nextCodePoint) + " " + GetCoordinates(parserContext));
            ReadCodePoint(parserContext);
        }

        /// <summary>
        /// Skips whitespace/comments and, if the next significant character is the given candidate, consumes it and
        /// returns true; otherwise leaves the reader untouched and returns false. Used for optional punctuation.
        /// </summary>
        private static bool TryConsumeChar(RDFQueryParserContext parserContext, int candidateCodePoint)
        {
            if (SkipWhitespace(parserContext) == candidateCodePoint)
            {
                ReadCodePoint(parserContext);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Skips whitespace/comments and, if the upcoming keyword run case-insensitively equals the expected keyword,
        /// consumes it and returns true; otherwise pushes the run back and returns false. Used for optional keywords
        /// (WHERE, DISTINCT, REDUCED, BY) and for case-insensitive keyword matching in general.
        /// </summary>
        private static bool TryConsumeKeyword(RDFQueryParserContext parserContext, string expectedKeyword)
        {
            SkipWhitespace(parserContext);
            string keyword = ReadKeyword(parserContext);
            if (keyword.Equals(expectedKeyword, StringComparison.OrdinalIgnoreCase))
                return true;

            //Not the expected keyword: restore the reader to exactly where it was
            UnreadString(parserContext, keyword);
            return false;
        }

        /// <summary>
        /// Skips whitespace/comments and parses a non-negative decimal integer literal (used by LIMIT/OFFSET).
        /// </summary>
        /// <exception cref="RDFQueryException">When no digit is present at the cursor.</exception>
        private static int ParseInteger(RDFQueryParserContext parserContext, string grammarContext)
        {
            SkipWhitespace(parserContext);

            StringBuilder digits = new StringBuilder();
            int codePoint = ReadCodePoint(parserContext);
            while (codePoint >= '0' && codePoint <= '9')
            {
                RDFTurtle.AppendCodePoint(digits, codePoint);
                codePoint = ReadCodePoint(parserContext);
            }
            //The first non-digit code point does not belong to the number: push it back
            UnreadCodePoint(parserContext, codePoint);

            if (digits.Length == 0)
                throw new RDFQueryException("Cannot parse SPARQL " + grammarContext + ": expected a non-negative integer " + GetCoordinates(parserContext));

            return int.Parse(digits.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Renders a code point for diagnostics: either the quoted character or the words "end of input" at EOF.
        /// </summary>
        private static string DescribeCodePoint(int codePoint)
            => codePoint == -1 ? "end of input" : "'" + char.ConvertFromUtf32(codePoint) + "'";

        /// <summary>
        /// Re-attaches the prologue's explicitly-declared prefixes to the given query so that the parsed query
        /// re-serializes its prologue identically to the input. The default namespace (empty label) is skipped,
        /// because RDFNamespace forbids an empty prefix and the printer cannot emit it. Any prefix that RDFNamespace
        /// rejects (e.g. a reserved label) is silently skipped: it still resolves terms, it just is not re-declared.
        /// </summary>
        private static void ApplyDeclaredPrefixes(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            foreach (KeyValuePair<string, string> declaredPrefix in parserContext.Resolver.DeclaredPrefixes)
            {
                //The default namespace cannot be modeled as an RDFNamespace (empty prefix is disallowed)
                if (declaredPrefix.Key.Length == 0)
                    continue;

                try
                {
                    selectQuery.AddPrefix(new RDFNamespace(declaredPrefix.Key, declaredPrefix.Value));
                }
                catch (RDFModelException)
                {
                    //RDFNamespace rejected this label/uri (e.g. a reserved prefix): keep it for resolution only
                }
            }
        }
        #endregion

        #region Char.Check
        /// <summary>
        /// Checks whether the given code point is an ASCII letter (A-Z or a-z).
        /// </summary>
        private static bool IsAsciiLetter(int codePoint)
            => (codePoint >= 'A' && codePoint <= 'Z') || (codePoint >= 'a' && codePoint <= 'z');

        /// <summary>
        /// Checks whether the given code point may START a SPARQL VARNAME.
        /// Per the grammar: VARNAME ::= ( PN_CHARS_U | [0-9] ) ( ... )* — i.e. a letter, underscore or digit.
        /// </summary>
        private static bool IsVarNameStartChar(int codePoint)
            => RDFTurtle.IsPN_CHARS_U(codePoint) || RDFTurtle.IsNumber(codePoint);

        /// <summary>
        /// Checks whether the given code point may CONTINUE a SPARQL VARNAME.
        /// Per the grammar: the tail allows PN_CHARS_U, digits, and a handful of combining/connector ranges.
        /// Note that '-' is intentionally excluded (unlike Turtle PN_CHARS), as SPARQL VARNAME forbids it.
        /// </summary>
        private static bool IsVarNameChar(int codePoint)
            => RDFTurtle.IsPN_CHARS_U(codePoint)
                || RDFTurtle.IsNumber(codePoint)
                || codePoint == 0x00B7
                || (codePoint >= 0x0300 && codePoint <= 0x036F)
                || (codePoint >= 0x203F && codePoint <= 0x2040);
        #endregion
    }
    
    /// <summary>
    /// <para>
    /// RDFSPARQLTermResolver supplies base-IRI and prefix-to-namespace resolution to the Turtle
    /// term-level parsers (ParseURI, ParseQNameOrBoolean, ...) while the SPARQL parser is running.
    /// It is the SPARQL counterpart of RDFGraphTermResolver: where the graph-backed resolver reads
    /// base/prefixes LIVE out of an RDFGraph, this one is fully AUTONOMOUS and accumulates the state
    /// declared by the query's own prologue:
    /// <list type="bullet">
    /// <item>the base IRI set by a <c>BASE &lt;...&gt;</c> directive;</item>
    /// <item>the prefix-to-namespace bindings introduced by every <c>PREFIX label: &lt;...&gt;</c> directive.</item>
    /// </list>
    /// </para>
    /// </summary>
    internal sealed class RDFSPARQLTermResolver : RDFTermResolver
    {
        #region Fields
        /// <summary>
        /// Absolute base IRI declared by a SPARQL <c>BASE</c> directive, used to resolve relative IRIs.
        /// Remains the empty string until a BASE directive is parsed (SPARQL allows queries with no base).
        /// </summary>
        private string declaredBaseIri = string.Empty;

        /// <summary>
        /// Prefix-label => namespace-URI bindings declared by the SPARQL <c>PREFIX</c> directives.
        /// The empty string key represents the default namespace declared as <c>PREFIX : &lt;...&gt;</c>.
        /// Comparison is ordinal because prefix labels are case-sensitive in SPARQL.
        /// </summary>
        private readonly Dictionary<string, string> prefixLabelToNamespaceUri =
            new Dictionary<string, string>(StringComparer.Ordinal);
        #endregion

        #region Properties
        /// <summary>
        /// Base IRI against which the Turtle term-parsers resolve relative IRI references.
        /// Mirrors the value last supplied through a SPARQL BASE directive (empty when none was given).
        /// </summary>
        internal override string BaseUri => declaredBaseIri;

        /// <summary>
        /// The prefix-label => namespace-URI bindings explicitly declared by this query's PREFIX directives,
        /// in declaration order. The SPARQL parser reads these back to re-attach them to the parsed query
        /// (via RDFSelectQuery.AddPrefix), so that the resulting query re-serializes its prologue identically.
        /// The leniency fallbacks (well-known register prefixes) are deliberately NOT included here: only the
        /// prefixes the query author actually wrote belong in its prologue.
        /// </summary>
        internal IEnumerable<KeyValuePair<string, string>> DeclaredPrefixes
            => prefixLabelToNamespaceUri;
        #endregion

        #region Methods
        /// <summary>
        /// Records the absolute base IRI carried by a SPARQL <c>BASE &lt;absoluteBaseIri&gt;</c> directive.
        /// A later BASE directive overrides an earlier one, matching SPARQL's "last declaration wins" semantics.
        /// </summary>
        internal void SetBaseIri(string absoluteBaseIri)
            => declaredBaseIri = absoluteBaseIri ?? string.Empty;

        /// <summary>
        /// Records a prefix-to-namespace binding carried by a SPARQL <c>PREFIX label: &lt;namespaceUri&gt;</c>
        /// directive. A null/empty label is the default namespace. A later declaration of the same label
        /// overrides the earlier one, matching SPARQL's "last declaration wins" semantics.
        /// </summary>
        internal void RegisterPrefix(string prefixLabel, string namespaceUri)
            => prefixLabelToNamespaceUri[prefixLabel ?? string.Empty] = namespaceUri;

        /// <summary>
        /// Resolves a prefix label to its namespace URI for the Turtle term-parsers.
        /// Resolution order:
        /// <list type="number">
        /// <item>a namespace explicitly declared by a <c>PREFIX</c> directive of this very query (always wins);</item>
        /// <item>RATIFIED LENIENCY: for a non-empty label that was never declared, fall back to the well-known
        /// prefixes of the global RDFNamespaceRegister (rdf, rdfs, xsd, owl, ...) so that common vocabularies
        /// can be used without an explicit PREFIX declaration.</item>
        /// </list>
        /// Returns null when the label cannot be resolved (e.g. the default namespace was never declared, or
        /// an unknown prefix is used): the caller surfaces this as a parse error.
        /// </summary>
        internal override string ResolveNamespace(string prefixLabel)
        {
            //Normalize a null label to the empty-string key used for the default namespace
            string normalizedPrefixLabel = prefixLabel ?? string.Empty;

            //1) A prefix explicitly declared by THIS query's prologue always takes precedence
            if (prefixLabelToNamespaceUri.TryGetValue(normalizedPrefixLabel, out string declaredNamespaceUri))
                return declaredNamespaceUri;

            //2) Leniency for well-known, never-declared prefixes (rdf/rdfs/xsd/owl/...): the default
            //   namespace (empty label) is deliberately NOT auto-resolved, only genuinely named prefixes are
            if (normalizedPrefixLabel.Length > 0)
                return RDFNamespaceRegister.GetByPrefix(normalizedPrefixLabel)?.ToString();

            //3) Undeclared default namespace: unresolved
            return null;
        }
        #endregion
    }
}