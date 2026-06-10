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
    /// <para>
    /// RDFQueryParser is responsible for turning a SPARQL 1.1 query string into the corresponding
    /// RDFQuery/RDFOperation object-model instance. It is the single entry point of the text-to-model
    /// pipeline: callers pass a raw SPARQL command string and receive a fully-built, engine-ready query.
    /// </para>
    /// <para>
    /// The lexer reuses RDFTurtle's low-level term-parsing infrastructure (ParseURI, ParseValue, the
    /// pushback reader …) through a thin adapter (<see cref="RDFQueryParserContext"/>), so that IRI,
    /// literal and prefixed-name parsing are shared between the two parsers rather than duplicated.
    /// </para>
    /// </summary>
    internal static class RDFQueryParser
    {
        #region Context
        /// <summary>
        /// <para>
        /// RDFQueryParserContext bundles the mutable state that belongs to a single SPARQL parse run.
        /// It is created fresh for every call to <see cref="RDFQueryParser.ParseQuery"/> and is never
        /// shared across threads or across multiple parse invocations.
        /// </para>
        /// <para>
        /// The context is sealed because it has no extension points: the only reason to pass it around
        /// is to give every parser helper function access to the shared reader position and the resolver.
        /// </para>
        /// </summary>
        internal sealed class RDFQueryParserContext
        {
            #region Properties
            /// <summary>
            /// <para>
            /// The Turtle parsing context that the SPARQL parser borrows for all low-level term recognition.
            /// It carries two resources:
            /// <list type="bullet">
            /// <item><b>Reader</b> — the <c>RDFPushbackReader</c> wrapping the SPARQL command text, which
            ///   the lexer advances one code point at a time and can push characters back onto when needed.</item>
            /// <item><b>Resolver</b> — the <c>RDFTermResolver</c> wired into <see cref="Resolver"/>, so that
            ///   every Turtle term-parser (ParseURI, ParseValue, ParseQNameOrBoolean, …) automatically uses
            ///   the SPARQL prologue's BASE and PREFIX declarations for IRI resolution.</item>
            /// </list>
            /// </para>
            /// <para>
            /// By reusing RDFTurtle's context struct, the SPARQL parser can call all Turtle term-parsers
            /// verbatim without any adaptation — the only difference is that the resolver behind the context
            /// is an <see cref="RDFSPARQLTermResolver"/> instead of a graph-backed one.
            /// </para>
            /// </summary>
            internal RDFTurtle.RDFTurtleContext TermParsingContext { get; }

            /// <summary>
            /// <para>
            /// The autonomous resolver that accumulates the BASE and PREFIX declarations of this query's
            /// prologue as they are parsed. It is the same resolver instance that is wired into
            /// <see cref="TermParsingContext"/>, exposed directly here so that <see cref="ParsePrologue"/>
            /// can push new bindings into it via <see cref="RDFSPARQLTermResolver.SetBaseIri"/> and
            /// <see cref="RDFSPARQLTermResolver.RegisterPrefix"/>.
            /// </para>
            /// </summary>
            internal RDFSPARQLTermResolver Resolver { get; }
            #endregion

            #region Ctors
            /// <summary>
            /// Initialises a new parsing context over the given reader of SPARQL command text.
            /// A fresh <see cref="RDFSPARQLTermResolver"/> is created and wired into both the
            /// <see cref="Resolver"/> property (so prologue parsers can populate it) and the
            /// Turtle context's Resolver slot (so term-parsers resolve prefixed names through it).
            /// </summary>
            internal RDFQueryParserContext(TextReader sparqlCommandReader)
            {
                //Create the autonomous resolver that will accumulate this query's BASE/PREFIX prologue
                Resolver = new RDFSPARQLTermResolver();

                TermParsingContext = new RDFTurtle.RDFTurtleContext
                {
                    //Wire the SPARQL resolver into the Turtle context so that ParseURI / ParseValue /
                    //ParseQNameOrBoolean resolve IRIs and prefixes through THIS query's prologue
                    Resolver = Resolver,

                    //Wrap the SPARQL command text in a pushback reader: the lexer reads forward one
                    //code point at a time and can push back the most recently read character when it
                    //needs to "un-consume" a token (e.g. after peeking at a keyword)
                    Reader = new RDFTurtle.RDFPushbackReader(sparqlCommandReader)
                };
            }
            #endregion
        }

        /// <summary>
        /// Factory helper: wraps <paramref name="sparqlCommandText"/> in a <see cref="StringReader"/> and
        /// returns a fresh <see cref="RDFQueryParserContext"/> ready to parse. A null text is normalised to
        /// the empty string so the reader starts at EOF rather than throwing a NullReferenceException.
        /// </summary>
        internal static RDFQueryParserContext CreateContext(string sparqlCommandText)
            => new RDFQueryParserContext(new StringReader(sparqlCommandText ?? string.Empty));
        #endregion

        #region Lexer
        /// <summary>
        /// The set of SPARQL keywords that can appear at the <c>GraphPatternNotTriples</c> dispatch point
        /// (i.e. wherever a new non-triple element can begin inside a <c>GroupGraphPattern</c>).
        /// <para>
        /// This set serves two purposes:
        /// <list type="number">
        /// <item>It terminates an ongoing TriplesBlock scan — as soon as one of these keywords is spotted,
        ///   the triple reader stops so the enclosing group-pattern dispatcher can handle the keyword.</item>
        /// <item>It distinguishes a lone keyword (e.g. "UNION") from a prefixed-name local part that
        ///   happens to start with the same letters (e.g. "union:foo") — the check in
        ///   <see cref="PeekGraphPatternKeyword"/> uses this set to decide whether the letter run
        ///   should be treated as a keyword or as the beginning of a QName token.</item>
        /// </list>
        /// </para>
        /// <para>
        /// Comparison is case-insensitive (SPARQL keywords are case-insensitive per the spec).
        /// </para>
        /// <para>
        /// Phase note: OPTIONAL / UNION / MINUS are handled in F2a; the remaining keywords (FILTER, GRAPH,
        /// SERVICE, BIND, VALUES) are recognized here only so the TriplesBlock scan stops at them and the
        /// dispatcher can raise a precise "not supported yet" error until their phase lands. SELECT is listed
        /// because a SubSelect (<c>'{' SubSelect '}'</c>) can begin a group; today it is rejected as
        /// unsupported, and when the SubSelect phase lands it must be REMOVED from the throw branch in
        /// <see cref="ParseGroupGraphPatternSub"/> and routed to a dedicated SubSelect parser instead.
        /// </para>
        /// </summary>
        private static readonly HashSet<string> GraphPatternKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "OPTIONAL", "UNION", "MINUS", "FILTER", "GRAPH", "SERVICE", "BIND", "VALUES", "SELECT" };

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

        /// <summary>
        /// <para>
        /// Peeks at the upcoming letter run to determine whether it is a recognized graph-pattern keyword
        /// (OPTIONAL, UNION, MINUS, FILTER, GRAPH, SERVICE, BIND, VALUES, SELECT) that stands alone as an
        /// operator, rather than the beginning of a QName-like token such as <c>union:SomeClass</c>.
        /// </para>
        /// <para>
        /// When a recognized keyword is found, the reader is restored to exactly the position it was at
        /// before the call (i.e. the keyword is left unconsumed) and the keyword is returned in uppercase.
        /// When the letter run is either not a recognized keyword, or IS a recognized keyword but is
        /// immediately followed by a QName-continuer character (digit, '_', or '-'), the reader is equally
        /// restored and the empty string is returned.
        /// </para>
        /// <para>
        /// IMPORTANT: This method does NOT skip leading whitespace. Callers are responsible for having
        /// positioned the reader on the first significant (non-whitespace) character BEFORE calling this
        /// method — typically by calling <see cref="SkipWhitespace"/> first. If the reader is sitting on a
        /// whitespace character, <see cref="ReadKeyword"/> will return the empty string immediately (since
        /// whitespace is not an ASCII letter), and this method will therefore return the empty string too,
        /// even if a keyword follows the whitespace.
        /// </para>
        /// </summary>
        /// <returns>
        /// The recognized keyword in uppercase (e.g. "UNION"), or the empty string if the next token is
        /// not a standalone graph-pattern keyword.
        /// </returns>
        private static string PeekGraphPatternKeyword(RDFQueryParserContext parserContext)
        {
            //Read the maximal ASCII-letter run that starts at the current reader position.
            //ReadKeyword leaves the first non-letter character on the reader (it "unreads" it), so after
            //this call the reader is positioned on the character immediately after the letter run.
            string candidateKeywordRun = ReadKeyword(parserContext);

            if (GraphPatternKeywords.Contains(candidateKeywordRun))
            {
                //The letter run is one of the recognized keywords. Now check the very next character (the
                //one that ended the letter run, still sitting on the reader) to decide whether this run is
                //a standalone keyword or the beginning of a longer QName local part.
                //Characters that can CONTINUE a QName local name (PN_CHARS family) after a keyword-matching
                //prefix are: decimal digits (0-9), underscore ('_') and hyphen ('-').
                //If the terminator is any of these, "OPTIONAL123" or "union_foo" would be misidentified as
                //a keyword — we must treat the whole run as part of a longer token instead.
                int terminatorCodePoint = PeekCodePoint(parserContext);
                bool isQNameContinuation = (terminatorCodePoint >= '0' && terminatorCodePoint <= '9')
                                           || terminatorCodePoint == '_'
                                           || terminatorCodePoint == '-';
                if (!isQNameContinuation)
                {
                    //Standalone keyword: push the letter run back so the caller can consume it explicitly,
                    //and return the keyword in normalized uppercase so comparisons are case-insensitive.
                    UnreadString(parserContext, candidateKeywordRun);
                    return candidateKeywordRun.ToUpperInvariant();
                }
            }

            //Either the letter run is not a recognized keyword, or it is a keyword but is followed by a
            //QName-continuer: in both cases push it back and signal "no standalone keyword here".
            UnreadString(parserContext, candidateKeywordRun);
            return string.Empty;
        }

        /// <summary>
        /// Consumes the current graph-pattern keyword token from the reader.
        /// Callers MUST have confirmed via <see cref="PeekGraphPatternKeyword"/> that a keyword is present
        /// before calling this: the method simply advances past the letter run with no additional checks.
        /// </summary>
        private static void ConsumeKeyword(RDFQueryParserContext parserContext)
            => ReadKeyword(parserContext);
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
        /// <para>
        /// Parses the WHERE clause of a SELECT query and attaches the resulting algebra members to
        /// <paramref name="selectQuery"/>.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>WhereClause ::= 'WHERE'? GroupGraphPattern</c>.
        /// The WHERE keyword is optional: a query that opens its body with a bare <c>{</c> is perfectly
        /// valid, so this method first tries to consume WHERE and then unconditionally expects the
        /// opening brace.
        /// </para>
        /// <para>
        /// The body of the group graph pattern is parsed by <see cref="ParseGroupGraphPatternSub"/>,
        /// which implements the full graph-pattern algebra (OPTIONAL, UNION, MINUS) and returns a flat
        /// list of already-combined algebra nodes. Each node is then attached to the query via
        /// <see cref="AddQueryMember"/> so that the engine's evaluator can walk them directly.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the braces are unbalanced or a member is malformed.</exception>
        private static void ParseWhereClause(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            //WHERE is optional per the SPARQL grammar: try to consume it but proceed either way
            TryConsumeKeyword(parserContext, "WHERE");

            //The WHERE clause body is a single GroupGraphPattern: consume its opening brace …
            ExpectChar(parserContext, '{', "WHERE clause");

            //… parse the content of the group into a list of algebra members …
            List<RDFQueryMember> whereClauseMembers = ParseGroupGraphPatternSub(parserContext);

            //… and close the group
            ExpectChar(parserContext, '}', "WHERE clause");

            //Attach every algebra member produced by the body to the query
            foreach (RDFQueryMember whereClauseMember in whereClauseMembers)
                AddQueryMember(selectQuery, whereClauseMember);
        }

        /// <summary>
        /// <para>
        /// Parses the content of a <c>GroupGraphPattern</c> body (i.e. everything between its braces) and
        /// returns the ordered, flat list of algebra members it produces. The caller is responsible for
        /// consuming the surrounding <c>{</c> and <c>}</c> characters — this method only sees what is inside.
        /// </para>
        /// <para>
        /// SPARQL grammar:
        /// <code>
        /// GroupGraphPatternSub ::= TriplesBlock?
        ///     ( GraphPatternNotTriples '.'? TriplesBlock? )*
        ///
        /// GraphPatternNotTriples ::= GroupOrUnionGraphPattern
        ///                          | OptionalGraphPattern
        ///                          | MinusGraphPattern
        ///                          | GraphGraphPattern         (future)
        ///                          | ServiceGraphPattern       (future)
        ///                          | Filter                    (future)
        ///                          | Bind                      (future)
        ///                          | InlineData                (future)
        /// </code>
        /// </para>
        /// <para>
        /// Algebra accumulation rules. The grouping/binding of well-formed input is strictly SPARQL-compliant
        /// (e.g. MINUS binds the whole accumulated left side); on top of that the parser applies two RATIFIED
        /// leniencies for malformed input — a stray UNION and a leading MINUS are recovered from rather than
        /// rejected (see the corresponding items below):
        /// <list type="bullet">
        /// <item>
        ///   <b>BGP (bare triples)</b> — a triple run is read by <see cref="ParseBasicGraphPatternMember"/>
        ///   into a new <see cref="RDFPatternGroup"/> and appended to the accumulator.
        /// </item>
        /// <item>
        ///   <b>GroupOrUnionGraphPattern <c>{ … }</c></b> — delegated to
        ///   <see cref="ParseGroupOrUnionGraphPattern"/>; a UNION chain inside the braces is folded into a
        ///   single left-associative <see cref="RDFOperatorQueryMember"/> tree node before being appended.
        /// </item>
        /// <item>
        ///   <b>OPTIONAL</b> — the following <c>GroupGraphPattern</c> operand is parsed, its
        ///   <see cref="RDFQueryMember.IsOptional"/> flag is set to <c>true</c>, and it is appended to the
        ///   accumulator as an independently optional element.
        /// </item>
        /// <item>
        ///   <b>MINUS</b> — per the SPARQL spec, MINUS binds the ENTIRE left side accumulated so far
        ///   (not just the immediately preceding element). The current accumulator is therefore collapsed to
        ///   a single algebra unit — wrapping in a <c>SELECT *</c> subquery when it contains more than one
        ///   joined member — and an <see cref="RDFOperatorQueryMember"/>(<c>Minus</c>, collapsedLeft, right)
        ///   tree node replaces the entire accumulator. A leading MINUS with an empty accumulator is treated
        ///   resiliently: the right operand is kept as a plain non-MINUS element.
        /// </item>
        /// <item>
        ///   <b>Stray UNION</b> — UNION appearing at this level (rather than inside a
        ///   <c>GroupOrUnionGraphPattern</c>) has no valid left operand. It is silently discarded and parsing
        ///   continues, so that the parser is lenient toward malformed queries.
        /// </item>
        /// <item>
        ///   <b>Deferred keywords</b> (FILTER, GRAPH, SERVICE, BIND, VALUES, SELECT) — a parse error is
        ///   raised with an explicit "not supported yet" message until the corresponding phase lands.
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <returns>
        /// A list of zero or more <see cref="RDFQueryMember"/> instances (pattern groups, subqueries, and
        /// operator tree nodes) in left-to-right order, ready to be attached to the enclosing query.
        /// </returns>
        /// <exception cref="RDFQueryException">On unbalanced braces, malformed triples, or unsupported keywords.</exception>
        private static List<RDFQueryMember> ParseGroupGraphPatternSub(RDFQueryParserContext parserContext)
        {
            //The accumulator holds the algebra members produced so far within this group body.
            //Elements are appended left-to-right; a MINUS flushes and replaces the whole accumulator
            //with a single Minus tree node, enforcing SPARQL's "MINUS binds the whole left side" rule.
            List<RDFQueryMember> accumulatedMembers = new List<RDFQueryMember>();

            while (true)
            {
                //Position the reader on the next significant character and examine it without consuming it
                int nextSignificantCodePoint = SkipWhitespace(parserContext);

                //A '}' ends this group body: return control to the caller which will consume the brace.
                //EOF is treated the same way so the caller's ExpectChar('}') surfaces a clean error.
                if (nextSignificantCodePoint == '}' || nextSignificantCodePoint == -1)
                    return accumulatedMembers;

                //A '.' is the optional separator that SPARQL allows between any two consecutive elements
                //of a GroupGraphPatternSub (e.g. between a TriplesBlock and an OptionalGraphPattern).
                //Consume it and continue scanning for the next element.
                if (nextSignificantCodePoint == '.')
                {
                    ReadCodePoint(parserContext);
                    continue;
                }

                //The reader is now positioned on the first non-whitespace, non-dot character. If it is an
                //ASCII letter, it might be the start of a graph-pattern keyword. PeekGraphPatternKeyword
                //reads the letter run, checks it against the recognized set, and unreads it regardless,
                //so the reader position is unchanged after the call.
                string upcomingKeyword = PeekGraphPatternKeyword(parserContext);

                //OptionalGraphPattern ::= 'OPTIONAL' GroupGraphPattern
                //The operand of OPTIONAL is a single GroupGraphPattern (which may itself contain UNION /
                //MINUS / nested groups). Mark it optional and append it independently: unlike MINUS, OPTIONAL
                //does NOT fold the previous accumulator into a combined left side.
                //
                //ASYMMETRY NOTE (why OPTIONAL is a flat flag while MINUS/UNION are tree nodes):
                //OPTIONAL stays a plain accumulator member carrying IsOptional=true, whereas MINUS and UNION
                //become explicit RDFOperatorQueryMember binary tree nodes. This is deliberate and rests on how
                //the engine evaluates the top-level member list: RDFTableEngine.CombineTables performs a LEFT-FOLD
                //(OuterJoinTables), and when a member's table is optional it left-joins instead of inner-joins.
                //That fold therefore ALREADY binds the whole accumulated left side as OPTIONAL's left operand, for
                //free — so a flat flag is sufficient and exactly correct (e.g. {A B OPTIONAL C} folds to
                //LeftJoin(Join(A,B), C)). MINUS/UNION cannot rely on the fold: the flat flags that used to encode
                //them were removed in Mirella 2, and their operands must be made explicit so the engine can
                //evaluate each binary node's left and right as isolated, correctly-scoped sub-results.
                if (upcomingKeyword == "OPTIONAL")
                {
                    ConsumeKeyword(parserContext);
                    RDFQueryMember optionalOperand = ParseGroupGraphPattern(parserContext);
                    SetIsOptional(optionalOperand);
                    accumulatedMembers.Add(optionalOperand);
                    continue;
                }

                //MinusGraphPattern ::= 'MINUS' GroupGraphPattern
                //
                //SPARQL algebra semantics: MINUS(left, right) = Diff(left, right).
                //Crucially, the left side is the JOIN of ALL algebra members accumulated so far in this
                //group body — not just the immediately preceding one. For example:
                //    { ?a :p ?b . ?b :q ?c . MINUS { ?a :r ?d } }
                //desugars to:
                //    MINUS( JOIN(?a:p?b, ?b:q?c), ?a:r?d )
                //
                //We enforce this by collapsing the entire current accumulator to a single algebra unit before
                //building the Minus tree node. When the accumulator holds multiple joined members, they are
                //wrapped in a SELECT * subquery (WrapIntoSubQuery) so the engine evaluates them as one unit.
                //The Minus node then replaces the entire accumulator, becoming the new sole element,
                //ready to be combined with further elements that follow.
                if (upcomingKeyword == "MINUS")
                {
                    ConsumeKeyword(parserContext);
                    RDFQueryMember minusRightOperand = ParseGroupGraphPattern(parserContext);

                    if (accumulatedMembers.Count == 0)
                    {
                        //Resilient handling of a leading MINUS that has no left side to bind (e.g. the group
                        //opens with MINUS directly). The spec does not allow this, but rather than throwing we
                        //treat the right operand as a plain non-differencing member so the rest of the group
                        //can still be parsed and evaluated (the pattern group simply has no rows filtered out).
                        accumulatedMembers.Add(minusRightOperand);
                    }
                    else
                    {
                        //Normal case: collapse everything accumulated so far into a single left operand, clear
                        //the accumulator, and push the resulting Minus node as the only element in it.
                        RDFQueryMember minusLeftOperand = CollapseToSingle(accumulatedMembers);
                        accumulatedMembers.Clear();
                        accumulatedMembers.Add(new RDFOperatorQueryMember(
                            RDFQueryEnums.RDFQueryOperatorType.Minus, minusLeftOperand, minusRightOperand));
                    }
                    continue;
                }

                //A stray UNION at this level (i.e. NOT immediately following a GroupGraphPattern inside
                //ParseGroupOrUnionGraphPattern) has no valid left operand to combine with.
                //Per the SPARQL grammar, UNION only makes sense as part of GroupOrUnionGraphPattern:
                //    GroupOrUnionGraphPattern ::= GroupGraphPattern ('UNION' GroupGraphPattern)*
                //A UNION that appears here is therefore malformed. We drop it silently and continue so that
                //the parser remains lenient and can still extract the right operand as a plain element.
                if (upcomingKeyword == "UNION")
                {
                    ConsumeKeyword(parserContext);
                    continue;
                }

                //Any other recognized graph-pattern keyword (FILTER, GRAPH, SERVICE, BIND, VALUES, SELECT)
                //belongs to a parser phase that has not been implemented yet. Throw with a precise message
                //naming the exact unsupported keyword, so the caller knows what is missing.
                if (upcomingKeyword.Length > 0)
                    throw new RDFQueryException("Cannot parse SPARQL query: '" + upcomingKeyword + "' is not supported yet " + GetCoordinates(parserContext));

                //No keyword was recognised. Dispatch on the first character:
                //  '{' → a nested GroupOrUnionGraphPattern (UNION chain or bare group)
                //  anything else → a bare TriplesBlock (sequence of triple patterns)
                RDFQueryMember parsedElement = nextSignificantCodePoint == '{'
                    ? ParseGroupOrUnionGraphPattern(parserContext)
                    : (RDFQueryMember)ParseBasicGraphPatternMember(parserContext);
                accumulatedMembers.Add(parsedElement);
            }
        }

        /// <summary>
        /// <para>
        /// Parses a single <c>GroupGraphPattern</c> — a <c>{ … }</c> delimited block — and collapses its
        /// contents to a single algebra member. The surrounding braces are consumed by this method; the
        /// body is parsed by <see cref="ParseGroupGraphPatternSub"/>.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>GroupGraphPattern ::= '{' ( SubSelect | GroupGraphPatternSub ) '}'</c>.
        /// Only <c>GroupGraphPatternSub</c> is implemented in the current phase; SubSelect (a nested
        /// SELECT query) belongs to a later phase.
        /// </para>
        /// <para>
        /// When the body expands to more than one joined member, they are automatically wrapped in a
        /// <c>SELECT *</c> subquery via <see cref="CollapseToSingle"/> so that the caller always receives
        /// a single algebra unit. This is necessary for correct MINUS semantics (the whole compound group
        /// must act as one operand) and for correct UNION semantics (each branch must be a single member).
        /// </para>
        /// </summary>
        /// <returns>
        /// A single <see cref="RDFQueryMember"/> representing the entire group: a bare
        /// <see cref="RDFPatternGroup"/> for a single-element body, an <see cref="RDFSelectQuery"/> wrapping
        /// multiple joined members, or an empty <see cref="RDFPatternGroup"/> for an empty body.
        /// </returns>
        /// <exception cref="RDFQueryException">When the opening brace is missing or the body is malformed.</exception>
        private static RDFQueryMember ParseGroupGraphPattern(RDFQueryParserContext parserContext)
        {
            //Consume the opening '{' that delimits this group
            ExpectChar(parserContext, '{', "group graph pattern");

            //Parse everything inside the braces into a flat list of algebra members
            List<RDFQueryMember> groupBodyMembers = ParseGroupGraphPatternSub(parserContext);

            //Consume the closing '}' that ends this group
            ExpectChar(parserContext, '}', "group graph pattern");

            //Reduce the list to a single algebra unit: a bare element if there is exactly one, or a
            //SELECT * subquery wrapping multiple joined elements, or an empty PatternGroup if the body
            //was empty (which is legal in SPARQL — '{}' is a valid, trivially-satisfied group).
            return CollapseToSingle(groupBodyMembers);
        }

        /// <summary>
        /// <para>
        /// Parses a <c>GroupOrUnionGraphPattern</c> — the construct that starts with a <c>{</c> and may be
        /// followed by one or more <c>UNION { … }</c> continuations — and returns a single algebra member.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>GroupOrUnionGraphPattern ::= GroupGraphPattern ( 'UNION' GroupGraphPattern )*</c>.
        /// </para>
        /// <para>
        /// A bare group with no UNION continuation is returned as-is from <see cref="ParseGroupGraphPattern"/>.
        /// A chain of two or more groups connected by UNION is folded left-to-right into a binary
        /// <see cref="RDFOperatorQueryMember"/>(<c>Union</c>, left, right) tree, matching the spec's
        /// left-associative union semantics:
        /// <code>
        ///   A UNION B UNION C  →  Union( Union(A, B), C )
        /// </code>
        /// </para>
        /// <para>
        /// WHITESPACE PROTOCOL: <see cref="PeekGraphPatternKeyword"/> never skips leading whitespace
        /// (callers must ensure the reader is on a significant character before calling it). After each
        /// <see cref="ParseGroupGraphPattern"/> call the reader lands on the character immediately following
        /// the closing <c>}</c>, which is typically a space. Therefore <see cref="SkipWhitespace"/> MUST be
        /// called before every UNION peek — once before the <c>while</c> loop and once at the end of each
        /// iteration — to keep the reader positioned correctly for the keyword scan.
        /// </para>
        /// </summary>
        /// <returns>
        /// A single <see cref="RDFQueryMember"/> representing either the bare group or the root of the
        /// left-associative UNION algebra tree.
        /// </returns>
        /// <exception cref="RDFQueryException">When any operand group is malformed.</exception>
        private static RDFQueryMember ParseGroupOrUnionGraphPattern(RDFQueryParserContext parserContext)
        {
            //Parse the mandatory first (leftmost) GroupGraphPattern operand
            RDFQueryMember unionAccumulatedLeft = ParseGroupGraphPattern(parserContext);

            //After the closing '}' of the first group the reader may be sitting on whitespace, so skip it
            //before peeking for a UNION keyword. This is required because PeekGraphPatternKeyword does not
            //skip whitespace by itself — callers are contractually obligated to do so.
            SkipWhitespace(parserContext);

            //Consume as many 'UNION GroupGraphPattern' pairs as are present, folding them left-to-right
            //into a binary algebra tree. Example for three-way union A ∪ B ∪ C:
            //  iteration 1: left = Union(A, B)
            //  iteration 2: left = Union(Union(A, B), C)   ← left-associative, matches SPARQL spec
            while (PeekGraphPatternKeyword(parserContext) == "UNION")
            {
                //Consume the UNION keyword
                ConsumeKeyword(parserContext);

                //Parse the right-hand GroupGraphPattern operand for this UNION pair
                RDFQueryMember unionRightOperand = ParseGroupGraphPattern(parserContext);

                //Fold: the new left side is a Union tree node combining the running left with the new right
                unionAccumulatedLeft = new RDFOperatorQueryMember(
                    RDFQueryEnums.RDFQueryOperatorType.Union, unionAccumulatedLeft, unionRightOperand);

                //After consuming the right group, the reader may again be on whitespace — skip it before
                //the next iteration's UNION peek
                SkipWhitespace(parserContext);
            }

            return unionAccumulatedLeft;
        }

        /// <summary>
        /// <para>
        /// Parses a Basic Graph Pattern — an unbraced sequence of triple patterns — and packages it into
        /// a fresh <see cref="RDFPatternGroup"/>. A BGP is the SPARQL analogue of a set of Turtle triples:
        /// it uses the same predicate-object list (<c>;</c>) and object list (<c>,</c>) shorthands, and
        /// the same <c>a</c> verb for <c>rdf:type</c>.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>TriplesBlock ::= TriplesSameSubjectPath ('.' TriplesBlock?)?</c>.
        /// </para>
        /// <para>
        /// The triple scan terminates at any of the group-body boundaries handled by
        /// <see cref="ParseTriplesBlock"/>: a <c>}</c>, a nested <c>{</c>, a graph-pattern keyword,
        /// or end of input. The resulting pattern group is returned ready to be appended to the accumulator
        /// in <see cref="ParseGroupGraphPatternSub"/>.
        /// </para>
        /// </summary>
        private static RDFPatternGroup ParseBasicGraphPatternMember(RDFQueryParserContext parserContext)
        {
            //Allocate a fresh pattern group to collect the triples produced by this BGP scan
            RDFPatternGroup basicGraphPatternGroup = new RDFPatternGroup();

            //Delegate all triple scanning to ParseTriplesBlock, which fills the group and stops at the
            //first character that is not part of the triple sequence ('}', '{', keyword, or EOF)
            ParseTriplesBlock(parserContext, basicGraphPatternGroup);

            return basicGraphPatternGroup;
        }

        /// <summary>
        /// <para>
        /// Collapses a list of algebra members into a single <see cref="RDFQueryMember"/>, applying the
        /// minimum wrapping needed for the engine to evaluate them as one unit:
        /// <list type="bullet">
        /// <item><b>Empty list</b> — returns an empty <see cref="RDFPatternGroup"/> (trivially satisfied,
        ///   zero rows filtered). This handles the legal SPARQL case of an empty group <c>{}</c>.</item>
        /// <item><b>Single element</b> — returns the element unchanged. No wrapping overhead.</item>
        /// <item><b>Multiple elements</b> — wraps them in a <c>SELECT *</c> subquery via
        ///   <see cref="WrapIntoSubQuery"/> so the engine joins them sequentially as a single unit.
        ///   This is essential when the multi-element list must serve as one operand of a binary algebra
        ///   operator (e.g. the left side of a MINUS node): without the wrapping, the engine would not
        ///   know which subset of the query members belongs to the operator's left scope.</item>
        /// </list>
        /// </para>
        /// </summary>
        private static RDFQueryMember CollapseToSingle(List<RDFQueryMember> membersToCollapse)
        {
            switch (membersToCollapse.Count)
            {
                //An empty group body is legal in SPARQL ('{}' matches every binding) — model it as an empty
                //pattern group rather than null so the rest of the pipeline never needs null checks
                case 0: return new RDFPatternGroup();

                //A single member needs no structural wrapper: return it directly
                case 1: return membersToCollapse[0];

                //Two or more joined members must be wrapped so the engine can treat them as one combined unit
                default: return WrapIntoSubQuery(membersToCollapse);
            }
        }

        /// <summary>
        /// <para>
        /// Wraps the given list of algebra members into a fresh, wildcard-projection <c>SELECT *</c>
        /// subquery so that they are evaluated as a single joined unit by Mirella's query engine.
        /// </para>
        /// <para>
        /// This wrapping is used exclusively when a group body holding multiple joined members must
        /// serve as the single left or right operand of a binary algebra operator (e.g. the left side
        /// of a MINUS node, or an operand of UNION). Without the subquery boundary, the engine's
        /// <c>CombineTables</c> left-fold would join ALL query members of the outer query indiscriminately
        /// rather than respecting the scoping that the braces implied in the original SPARQL text.
        /// </para>
        /// </summary>
        private static RDFSelectQuery WrapIntoSubQuery(List<RDFQueryMember> membersToWrap)
        {
            //Create a wildcard SELECT * subquery: projecting all variables means the subquery acts as a
            //transparent pass-through of every binding produced by its body members
            RDFSelectQuery joiningSubQuery = new RDFSelectQuery();

            //Attach each member to the subquery using the same type-dispatch as the outer query builder
            foreach (RDFQueryMember memberToWrap in membersToWrap)
                AddQueryMember(joiningSubQuery, memberToWrap);

            return joiningSubQuery;
        }

        /// <summary>
        /// <para>
        /// Sets the <c>IsOptional</c> flag to <c>true</c> on any type of algebra member, regardless of
        /// its concrete run-time type. This is the single place where OPTIONAL semantics are applied to a
        /// parsed operand so that Mirella's evaluation engine can distinguish optional from mandatory joins.
        /// </para>
        /// <para>
        /// The method handles the three concrete member types that can appear as the operand of OPTIONAL:
        /// <list type="bullet">
        /// <item><see cref="RDFPatternGroup"/> — the common case: a plain group of triple patterns.</item>
        /// <item><see cref="RDFSelectQuery"/> — an inline subquery (multi-element group collapsed to a
        ///   single subquery by <see cref="CollapseToSingle"/>); its IsOptional property controls whether
        ///   the engine left-joins or inner-joins it.</item>
        /// <item><see cref="RDFOperatorQueryMember"/> — an OPTIONAL whose operand is itself a nested
        ///   algebra tree (e.g. OPTIONAL { { A } UNION { B } }); the tree node's IsOptional property
        ///   tells the engine to treat the whole tree as an optional branch.</item>
        /// </list>
        /// </para>
        /// </summary>
        private static void SetIsOptional(RDFQueryMember algebraMember)
        {
            if (algebraMember is RDFPatternGroup patternGroup)
                patternGroup.IsOptional = true;
            else if (algebraMember is RDFSelectQuery subQuery)
                subQuery.IsOptional = true;
            else if (algebraMember is RDFOperatorQueryMember operatorNode)
                operatorNode.IsOptional = true;
        }

        /// <summary>
        /// <para>
        /// Attaches a single algebra member to a <see cref="RDFSelectQuery"/>, routing it to the correct
        /// <c>Add…</c> method based on the member's concrete run-time type.
        /// </para>
        /// <para>
        /// The three member types that the parser can produce are:
        /// <list type="bullet">
        /// <item><see cref="RDFPatternGroup"/> — a set of triple patterns; added via
        ///   <see cref="RDFSelectQuery.AddPatternGroup"/>.</item>
        /// <item><see cref="RDFSelectQuery"/> — an inline subquery (e.g. a multi-member group body
        ///   collapsed by <see cref="CollapseToSingle"/>, or a future explicit SubSelect); added via
        ///   <see cref="RDFSelectQuery.AddSubQuery"/>.</item>
        /// <item><see cref="RDFOperatorQueryMember"/> — a binary algebra tree node (Union / Minus /
        ///   Optional-operator); added via <see cref="RDFSelectQuery.AddOperator"/>.</item>
        /// </list>
        /// </para>
        /// </summary>
        private static void AddQueryMember(RDFSelectQuery targetSelectQuery, RDFQueryMember memberToAdd)
        {
            if (memberToAdd is RDFPatternGroup patternGroup)
                targetSelectQuery.AddPatternGroup(patternGroup);
            else if (memberToAdd is RDFSelectQuery subQuery)
                targetSelectQuery.AddSubQuery(subQuery);
            else if (memberToAdd is RDFOperatorQueryMember operatorNode)
                targetSelectQuery.AddOperator(operatorNode);
        }
        #endregion

        #region TriplesBlock
        /// <summary>
        /// <para>
        /// Parses a <c>TriplesBlock</c> — a sequence of triple patterns separated by <c>.</c> — and collects
        /// the resulting <see cref="RDFPattern"/> instances into <paramref name="targetPatternGroup"/>.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>TriplesBlock ::= TriplesSameSubjectPath ('.' TriplesBlock?)?</c>.
        /// </para>
        /// <para>
        /// Each "triple" in SPARQL can in fact be a full predicate-object list (multiple predicate-object
        /// pairs on the same subject, separated by <c>;</c>) and an object list (multiple objects on the
        /// same subject+predicate, separated by <c>,</c>), exactly as in Turtle.
        /// </para>
        /// <para>
        /// The scan terminates early (without consuming the terminating character) at any of these block
        /// boundaries, leaving the character on the reader for the parent caller to process:
        /// <list type="bullet">
        /// <item><c>}</c> — the end of the enclosing <c>GroupGraphPattern</c>.</item>
        /// <item><c>{</c> — the start of a nested <c>GroupGraphPattern</c>.</item>
        /// <item>A recognized graph-pattern keyword (OPTIONAL, MINUS, UNION, …) — a
        ///   <c>GraphPatternNotTriples</c> element follows; it is left on the reader for
        ///   <see cref="ParseGroupGraphPatternSub"/> to dispatch on.</item>
        /// <item>End of input (<c>-1</c>) — the underlying reader is exhausted.</item>
        /// </list>
        /// A <c>.</c> after the last triple before a block boundary is optional (per the SPARQL grammar)
        /// and is consumed silently.
        /// </para>
        /// </summary>
        /// <param name="parserContext">The current parse state (reader + resolver).</param>
        /// <param name="targetPatternGroup">The pattern group to which parsed triple patterns are added.</param>
        /// <exception cref="RDFQueryException">When a triple inside the block is malformed.</exception>
        private static void ParseTriplesBlock(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup)
        {
            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);

                //A '}' ends the enclosing GroupGraphPattern; a '{' opens a following nested group.
                //EOF (-1) means the underlying reader is exhausted.
                //All three cases terminate the triple scan: leave the character on the reader and return.
                if (nextSignificantCodePoint == '}' || nextSignificantCodePoint == '{' || nextSignificantCodePoint == -1)
                    return;

                //A graph-pattern keyword at this position (OPTIONAL, MINUS, UNION, FILTER, …) signals that
                //a GraphPatternNotTriples element follows and the TriplesBlock is over. The keyword MUST be
                //left on the reader so that ParseGroupGraphPatternSub can pick it up and dispatch on it.
                if (PeekGraphPatternKeyword(parserContext).Length > 0)
                    return;

                //Parse the next triple: first the subject term, then its complete predicate-object list
                //(which may produce multiple RDFPattern instances via ';' and ',' shorthands)
                RDFPatternMember tripleSubject = ParseVariableOrTerm(parserContext);
                ParsePredicateObjectList(parserContext, targetPatternGroup, tripleSubject);

                //After the predicate-object list, a '.' may separate this triple from the next one.
                //Consume it and continue scanning if present; otherwise the scan stops (we must be at one
                //of the block boundaries, which the outer loop will detect on the next iteration).
                if (SkipWhitespace(parserContext) == '.')
                {
                    ReadCodePoint(parserContext);
                    continue;
                }

                //No '.': the triple list has ended; return to the caller
                return;
            }
        }

        /// <summary>
        /// <para>
        /// Parses a predicate-object list for a known <paramref name="tripleSubject"/> and emits one
        /// <see cref="RDFPattern"/> per object into <paramref name="targetPatternGroup"/>.
        /// </para>
        /// <para>
        /// SPARQL grammar (simplified):
        /// <code>
        /// PredicateObjectList ::= Verb ObjectList ( ';' ( Verb ObjectList )? )*
        /// </code>
        /// A <c>;</c> introduces another predicate-object group sharing the same subject. A trailing
        /// <c>;</c> immediately before a block boundary (<c>.</c>, <c>}</c>, keyword, EOF) is tolerated —
        /// the same leniency as in the Turtle deserialiser.
        /// </para>
        /// </summary>
        /// <param name="parserContext">The current parse state.</param>
        /// <param name="targetPatternGroup">The pattern group to which emitted patterns are added.</param>
        /// <param name="tripleSubject">The already-parsed subject that all emitted patterns share.</param>
        private static void ParsePredicateObjectList(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup, RDFPatternMember tripleSubject)
        {
            while (true)
            {
                //Parse the next predicate (verb) for this subject, then the comma-separated list of objects
                RDFPatternMember triplePredicate = ParsePredicate(parserContext);
                ParseObjectList(parserContext, targetPatternGroup, tripleSubject, triplePredicate);

                //A ';' introduces a second (or further) predicate-object group on the same subject
                if (SkipWhitespace(parserContext) == ';')
                {
                    ReadCodePoint(parserContext);

                    //After consuming the ';', check whether we are immediately at a block boundary or a keyword.
                    //If so, this was a trailing ';' — legal in SPARQL/Turtle, so we stop the predicate-object
                    //list without requiring another predicate to follow.
                    int codePointAfterSemicolon = SkipWhitespace(parserContext);
                    if (codePointAfterSemicolon == '.' || codePointAfterSemicolon == '}'
                        || codePointAfterSemicolon == '{' || codePointAfterSemicolon == -1)
                        return;
                    if (PeekGraphPatternKeyword(parserContext).Length > 0)
                        return;

                    //Another predicate-object group follows on the same subject: loop
                    continue;
                }

                //No ';': the predicate-object list for this subject is complete
                return;
            }
        }

        /// <summary>
        /// <para>
        /// Parses an object list for a known subject+predicate pair and emits one <see cref="RDFPattern"/>
        /// per object into <paramref name="targetPatternGroup"/>.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>ObjectList ::= Object ( ',' Object )*</c>.
        /// Objects sharing the same subject and predicate are separated by commas; each one produces a
        /// separate triple pattern in the enclosing pattern group.
        /// </para>
        /// </summary>
        /// <param name="parserContext">The current parse state.</param>
        /// <param name="targetPatternGroup">The pattern group to which emitted patterns are added.</param>
        /// <param name="tripleSubject">The already-parsed subject shared by all emitted patterns.</param>
        /// <param name="triplePredicate">The already-parsed predicate shared by all emitted patterns.</param>
        private static void ParseObjectList(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup, RDFPatternMember tripleSubject, RDFPatternMember triplePredicate)
        {
            while (true)
            {
                //Parse the object term (variable or RDF term)
                RDFPatternMember tripleObject = ParseVariableOrTerm(parserContext);

                //Emit the triple pattern. The RDFPattern constructor enforces all SPARQL term-position rules
                //(e.g. a literal in subject position is invalid): violations surface as RDFQueryException
                //through the parser's error contract.
                targetPatternGroup.AddPattern(new RDFPattern(tripleSubject, triplePredicate, tripleObject));

                //A ',' introduces another object sharing this subject+predicate: consume and continue
                if (SkipWhitespace(parserContext) == ',')
                {
                    ReadCodePoint(parserContext);
                    continue;
                }

                //No ',': the object list is complete
                return;
            }
        }

        /// <summary>
        /// <para>
        /// Parses an RDF term in subject or object position: either a SPARQL variable (<c>?name</c> or
        /// <c>$name</c>) or any concrete RDF term (IRI, prefixed name, blank node, literal, numeric or
        /// boolean literal) recognised by the shared Turtle term-reader.
        /// </para>
        /// <para>
        /// The caller must ensure that whitespace has been skipped before this method is invoked, or call
        /// <see cref="ParseTerm"/> (which skips whitespace internally for the Turtle reader path).
        /// </para>
        /// </summary>
        private static RDFPatternMember ParseVariableOrTerm(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //A '?' or '$' sigil is the unambiguous start of a SPARQL variable reference
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParseVariable(parserContext);

            //Any other character starts a concrete RDF term: delegate to the shared Turtle term-reader
            return ParseTerm(parserContext);
        }

        /// <summary>
        /// <para>
        /// Parses an RDF term in predicate (verb) position. In addition to variables and ordinary IRI terms,
        /// SPARQL inherits from Turtle the <c>a</c> shorthand for <c>rdf:type</c>:
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>Verb ::= VarOrIri | 'a'</c>.
        /// </para>
        /// <para>
        /// The <c>a</c> shorthand is recognized only when it stands alone — i.e. it is immediately followed
        /// by whitespace or end of input. If the character after <c>a</c> is anything else (e.g. a colon in
        /// <c>a:SomeClass</c>, or a letter in <c>abc:foo</c>), the <c>a</c> is pushed back and the whole
        /// token is re-parsed by the general Turtle term-reader.
        /// </para>
        /// </summary>
        private static RDFPatternMember ParsePredicate(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //A '?' or '$' sigil starts a variable predicate
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParseVariable(parserContext);

            //The single letter 'a' might be the rdf:type shorthand — check the character immediately after it
            if (nextSignificantCodePoint == 'a')
            {
                //Consume the 'a' and peek at the very next character (without consuming it)
                ReadCodePoint(parserContext);
                int codePointAfterA = PeekCodePoint(parserContext);

                //If 'a' is followed only by whitespace or EOF it is unambiguously the rdf:type shorthand:
                //expand it to the full rdf:type IRI right here
                if (codePointAfterA == -1 || RDFTurtle.IsWhitespace(codePointAfterA))
                    return new RDFResource(RDFVocabulary.RDF.TYPE.ToString());

                //The character after 'a' is not a whitespace delimiter, so 'a' is the first character of a
                //longer token (e.g. "a:SomeClass" or "abc:foo"). Push the 'a' back and fall through to the
                //general term-reader so the full token is parsed correctly.
                UnreadCodePoint(parserContext, 'a');
            }

            //General case: the predicate is a full IRI, prefixed name, or other term. The RDFPattern
            //constructor will later reject any term that is invalid in predicate position (blank nodes,
            //literals) surfacing the violation as RDFQueryException.
            return ParseTerm(parserContext);
        }
        #endregion

        #region SolutionModifiers
        /// <summary>
        /// <para>
        /// Parses the trailing solution-modifier section of a SELECT query and attaches the resulting
        /// modifier objects to <paramref name="selectQuery"/>. Scanning stops as soon as a token that is
        /// not a recognized modifier keyword is encountered; that token is pushed back so the caller can
        /// process it (or simply ignore it at end of input).
        /// </para>
        /// <para>
        /// SPARQL grammar:
        /// <code>
        /// SolutionModifier ::= GroupClause? HavingClause? OrderClause? LimitOffsetClauses?
        /// LimitOffsetClauses ::= LimitClause OffsetClause? | OffsetClause LimitClause?
        /// </code>
        /// Only ORDER BY, LIMIT, and OFFSET are supported in the current phase. GROUP BY and HAVING
        /// belong to a later phase. Modifiers are accepted in any order for leniency; if the same
        /// modifier appears twice the object-model silently ignores the duplicate.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When a recognized modifier keyword is followed by a malformed body.</exception>
        private static void ParseSolutionModifiers(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            while (true)
            {
                //Advance to the first significant character so ReadKeyword finds the keyword immediately
                SkipWhitespace(parserContext);

                //Read the upcoming keyword (may be empty at EOF or when the next token is not a keyword)
                string modifierKeyword = ReadKeyword(parserContext);

                switch (modifierKeyword.ToUpperInvariant())
                {
                    //ORDER BY clause: consume 'BY' and the sequence of order conditions
                    case "ORDER":
                        ParseOrderByModifier(parserContext, selectQuery);
                        break;

                    //LIMIT n: parse the non-negative integer and add a RDFLimitModifier
                    case "LIMIT":
                        selectQuery.AddModifier(new RDFLimitModifier(ParseInteger(parserContext, "LIMIT")));
                        break;

                    //OFFSET n: parse the non-negative integer and add a RDFOffsetModifier
                    case "OFFSET":
                        selectQuery.AddModifier(new RDFOffsetModifier(ParseInteger(parserContext, "OFFSET")));
                        break;

                    default:
                        //The keyword run is not a recognized modifier (or is empty at EOF): push it back so
                        //the reader position is restored, and stop scanning modifiers
                        UnreadString(parserContext, modifierKeyword);
                        return;
                }
            }
        }

        /// <summary>
        /// <para>
        /// Parses the body of an ORDER BY clause (the <c>ORDER</c> keyword has already been consumed by
        /// <see cref="ParseSolutionModifiers"/>) and attaches the resulting ordering modifiers to
        /// <paramref name="selectQuery"/>.
        /// </para>
        /// <para>
        /// SPARQL grammar:
        /// <code>
        /// OrderClause ::= 'ORDER' 'BY' OrderCondition+
        /// OrderCondition ::= ( ( 'ASC' | 'DESC' ) '(' Expression ')' )
        ///                  | ( Constraint | Var )
        /// </code>
        /// In the current phase, only variable-based conditions are supported. Expression conditions
        /// (e.g. <c>ORDER BY STRLEN(?label)</c>) belong to the expression-parser phase and are deferred.
        /// ASC and DESC directives are accepted with a variable argument. A bare variable implies ASC.
        /// At least one order condition is required after BY; anything that is not a recognised
        /// condition (e.g. the LIMIT keyword) is pushed back so <see cref="ParseSolutionModifiers"/>
        /// can process it.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When 'BY' is missing or no order condition is found.</exception>
        private static void ParseOrderByModifier(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            //The 'BY' keyword is mandatory and must immediately follow 'ORDER'
            if (!TryConsumeKeyword(parserContext, "BY"))
                throw new RDFQueryException("Cannot parse SPARQL ORDER BY clause: expected 'BY' after 'ORDER' " + GetCoordinates(parserContext));

            bool foundAtLeastOneOrderCondition = false;

            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);

                //A '?' or '$' sigil starts a bare variable order condition: ascending by default
                if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                {
                    selectQuery.AddModifier(new RDFOrderByModifier(
                        ParseVariable(parserContext), RDFQueryEnums.RDFOrderByFlavors.ASC));
                    foundAtLeastOneOrderCondition = true;
                    continue;
                }

                //Otherwise try to read an ASC(...) or DESC(...) directive
                string directionKeyword = ReadKeyword(parserContext);
                string normalizedDirectionKeyword = directionKeyword.ToUpperInvariant();

                if (normalizedDirectionKeyword == "ASC" || normalizedDirectionKeyword == "DESC")
                {
                    //Both directives require a single variable argument wrapped in parentheses
                    ExpectChar(parserContext, '(', "ORDER BY condition");
                    RDFVariable orderingVariable = ParseVariable(parserContext);
                    ExpectChar(parserContext, ')', "ORDER BY condition");

                    //Map the direction keyword to the corresponding enum value
                    RDFQueryEnums.RDFOrderByFlavors orderingDirection = normalizedDirectionKeyword == "ASC"
                        ? RDFQueryEnums.RDFOrderByFlavors.ASC
                        : RDFQueryEnums.RDFOrderByFlavors.DESC;

                    selectQuery.AddModifier(new RDFOrderByModifier(orderingVariable, orderingDirection));
                    foundAtLeastOneOrderCondition = true;
                    continue;
                }

                //The next token is not an order condition: push it back (it may be LIMIT/OFFSET or EOF)
                //and stop scanning order conditions
                if (directionKeyword.Length > 0)
                    UnreadString(parserContext, directionKeyword);
                break;
            }

            //At least one order condition is mandatory in a valid ORDER BY clause
            if (!foundAtLeastOneOrderCondition)
                throw new RDFQueryException("Cannot parse SPARQL ORDER BY clause: expected at least one variable to order by " + GetCoordinates(parserContext));
        }
        #endregion

        #region Lexer.Helpers
        /// <summary>
        /// Reads and returns the next Unicode code point from the underlying pushback reader, advancing the
        /// reader position by one code point. Returns <c>-1</c> at end of input. Thin delegation to
        /// <c>RDFTurtle.ReadCodePoint</c> so the SPARQL parser shares the identical pull-style I/O contract.
        /// </summary>
        private static int ReadCodePoint(RDFQueryParserContext parserContext)
            => RDFTurtle.ReadCodePoint(parserContext.TermParsingContext);

        /// <summary>
        /// Returns the next Unicode code point from the reader WITHOUT advancing the reader position
        /// (i.e. the same code point will be returned by the next <see cref="ReadCodePoint"/> call).
        /// Returns <c>-1</c> at end of input. Used to look ahead by a single character without consuming it.
        /// </summary>
        private static int PeekCodePoint(RDFQueryParserContext parserContext)
            => RDFTurtle.PeekCodePoint(parserContext.TermParsingContext);

        /// <summary>
        /// Pushes a single Unicode code point back onto the reader so that the next
        /// <see cref="ReadCodePoint"/> call returns it again. Used when a character has been consumed
        /// (e.g. to recognise a token boundary) but should be processed by a subsequent parsing step.
        /// </summary>
        private static void UnreadCodePoint(RDFQueryParserContext parserContext, int codePoint)
            => RDFTurtle.UnreadCodePoint(parserContext.TermParsingContext, codePoint);

        /// <summary>
        /// <para>
        /// Skips any leading whitespace and <c>#</c>-style comments, then verifies that the next
        /// significant character is <paramref name="expectedCodePoint"/> and consumes it.
        /// </para>
        /// <para>
        /// Used for the structural punctuation of the SPARQL grammar: opening/closing braces
        /// (<c>{</c>, <c>}</c>), parentheses (<c>(</c>, <c>)</c>), and similar mandatory delimiters.
        /// When the expected character is not found, a descriptive <see cref="RDFQueryException"/> is
        /// thrown naming the grammar context (e.g. "WHERE clause") so the author knows which construct
        /// is malformed.
        /// </para>
        /// </summary>
        /// <param name="parserContext">The current parse state.</param>
        /// <param name="expectedCodePoint">The Unicode code point that must be present next.</param>
        /// <param name="grammarContext">Human-readable label for the grammar construct (used in error messages).</param>
        /// <exception cref="RDFQueryException">When the next significant character is not <paramref name="expectedCodePoint"/>.</exception>
        private static void ExpectChar(RDFQueryParserContext parserContext, int expectedCodePoint, string grammarContext)
        {
            //Skip whitespace/comments and examine the first significant character
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //If the significant character is not what we expect, throw a descriptive parse error
            if (nextSignificantCodePoint != expectedCodePoint)
                throw new RDFQueryException(
                    "Cannot parse SPARQL " + grammarContext + ": expected '"
                    + (char)expectedCodePoint + "' but found "
                    + DescribeCodePoint(nextSignificantCodePoint) + " "
                    + GetCoordinates(parserContext));

            //Consume the expected character
            ReadCodePoint(parserContext);
        }

        /// <summary>
        /// <para>
        /// Skips whitespace/comments and, if the next significant character equals
        /// <paramref name="candidateCodePoint"/>, consumes it and returns <c>true</c>. Otherwise leaves the
        /// reader positioned on that character (unchanged) and returns <c>false</c>.
        /// </para>
        /// <para>
        /// Used for optional single-character tokens such as the <c>*</c> wildcard in SELECT projections.
        /// </para>
        /// </summary>
        private static bool TryConsumeChar(RDFQueryParserContext parserContext, int candidateCodePoint)
        {
            //Peek at the next significant character
            if (SkipWhitespace(parserContext) == candidateCodePoint)
            {
                //Match: consume and signal success
                ReadCodePoint(parserContext);
                return true;
            }

            //No match: the reader position is unchanged (SkipWhitespace leaves the char on the reader)
            return false;
        }

        /// <summary>
        /// <para>
        /// Skips whitespace/comments and, if the upcoming ASCII-letter run equals
        /// <paramref name="expectedKeyword"/> (case-insensitively), consumes it and returns <c>true</c>.
        /// Otherwise restores the reader to exactly the position it was at before the call and returns
        /// <c>false</c>.
        /// </para>
        /// <para>
        /// Used for keywords that are syntactically optional in the SPARQL grammar: <c>WHERE</c>,
        /// <c>DISTINCT</c>, <c>REDUCED</c>, and <c>BY</c> (in ORDER BY).
        /// </para>
        /// </summary>
        private static bool TryConsumeKeyword(RDFQueryParserContext parserContext, string expectedKeyword)
        {
            //Position the reader on the first non-whitespace character before attempting to read a keyword
            SkipWhitespace(parserContext);

            //Read the maximal ASCII-letter run starting at the current position
            string candidateKeyword = ReadKeyword(parserContext);

            //Compare case-insensitively per SPARQL keyword rules
            if (candidateKeyword.Equals(expectedKeyword, StringComparison.OrdinalIgnoreCase))
                return true;

            //Not the expected keyword: push the run back so the reader is exactly where it started,
            //and signal that the optional keyword was not present
            UnreadString(parserContext, candidateKeyword);
            return false;
        }

        /// <summary>
        /// <para>
        /// Skips whitespace/comments and parses a non-negative decimal integer literal, returning its
        /// value as a 32-bit integer. Used for the LIMIT and OFFSET clause values.
        /// </para>
        /// </summary>
        /// <param name="parserContext">The current parse state.</param>
        /// <param name="grammarContext">Human-readable label for the containing grammar construct (used in error messages).</param>
        /// <exception cref="RDFQueryException">When no decimal digit is present at the current reader position.</exception>
        private static int ParseInteger(RDFQueryParserContext parserContext, string grammarContext)
        {
            SkipWhitespace(parserContext);

            //Accumulate consecutive decimal-digit characters into a string builder
            StringBuilder digitRun = new StringBuilder();
            int codePoint = ReadCodePoint(parserContext);
            while (codePoint >= '0' && codePoint <= '9')
            {
                RDFTurtle.AppendCodePoint(digitRun, codePoint);
                codePoint = ReadCodePoint(parserContext);
            }

            //The first non-digit character terminates the number — push it back so the next reader call
            //sees it (it may be a keyword like LIMIT/OFFSET or the end of the query)
            UnreadCodePoint(parserContext, codePoint);

            //An empty digit run means no integer was present where one was required
            if (digitRun.Length == 0)
                throw new RDFQueryException(
                    "Cannot parse SPARQL " + grammarContext + ": expected a non-negative integer "
                    + GetCoordinates(parserContext));

            //Parse using the invariant culture so '.' is never misinterpreted as a decimal separator
            return int.Parse(digitRun.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Produces a human-readable description of a Unicode code point for use in parse-error messages:
        /// returns the character enclosed in single quotes (e.g. <c>'{'</c>) or the phrase
        /// <c>"end of input"</c> when the code point is <c>-1</c> (EOF sentinel).
        /// </summary>
        private static string DescribeCodePoint(int codePoint)
            => codePoint == -1 ? "end of input" : "'" + char.ConvertFromUtf32(codePoint) + "'";

        /// <summary>
        /// <para>
        /// Re-attaches the PREFIX declarations accumulated by this query's prologue to
        /// <paramref name="selectQuery"/> as <see cref="RDFNamespace"/> objects, so that the parsed query
        /// can re-serialise its prologue section identically to the original input text.
        /// </para>
        /// <para>
        /// Two categories of declared prefixes are silently skipped rather than added:
        /// <list type="bullet">
        /// <item>The <b>default namespace</b> (empty label, declared as <c>PREFIX : &lt;...&gt;</c>):
        ///   <see cref="RDFNamespace"/> forbids an empty prefix label, and the SPARQL printer has no
        ///   mechanism to re-emit it, so it cannot survive the serialisation round-trip and is omitted.</item>
        /// <item>Any prefix whose label or URI is rejected by <see cref="RDFNamespace"/> (e.g. a reserved
        ///   or otherwise invalid label): the prefix still participates in term resolution during parsing
        ///   (it lives in the resolver's internal dictionary) but it is not forwarded to the query object
        ///   model, so it will not appear in the re-serialised prologue.</item>
        /// </list>
        /// </para>
        /// </summary>
        private static void ApplyDeclaredPrefixes(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            foreach (KeyValuePair<string, string> prologuePrefixBinding in parserContext.Resolver.DeclaredPrefixes)
            {
                //Skip the default namespace: RDFNamespace forbids an empty prefix label
                if (prologuePrefixBinding.Key.Length == 0)
                    continue;

                try
                {
                    //Attempt to construct an RDFNamespace and add it as a prefix declaration to the query
                    selectQuery.AddPrefix(new RDFNamespace(prologuePrefixBinding.Key, prologuePrefixBinding.Value));
                }
                catch (RDFModelException)
                {
                    //RDFNamespace rejected this label or URI (e.g. a reserved prefix name): the binding is
                    //already in the resolver's dictionary and will still be used for term resolution, but it
                    //cannot be modelled as an RDFNamespace so we leave it out of the query's prologue list
                }
            }
        }
        #endregion

        #region Char.Check
        /// <summary>
        /// Returns <c>true</c> when <paramref name="codePoint"/> is an ASCII letter (A–Z or a–z).
        /// Used by <see cref="ReadKeyword"/> to accumulate the maximal letter run that may be a keyword.
        /// </summary>
        private static bool IsAsciiLetter(int codePoint)
            => (codePoint >= 'A' && codePoint <= 'Z') || (codePoint >= 'a' && codePoint <= 'z');

        /// <summary>
        /// <para>
        /// Returns <c>true</c> when <paramref name="codePoint"/> is a valid FIRST character of a SPARQL
        /// variable name (VARNAME).
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>VARNAME ::= ( PN_CHARS_U | [0-9] ) ( PN_CHARS_U | [0-9] | … )*</c>.
        /// The first character must be a PN_CHARS_U character (letter, underscore, or non-ASCII Unicode
        /// letter/digit in the PN_CHARS_U set) or an ASCII decimal digit.
        /// </para>
        /// </summary>
        private static bool IsVarNameStartChar(int codePoint)
            => RDFTurtle.IsPN_CHARS_U(codePoint) || RDFTurtle.IsNumber(codePoint);

        /// <summary>
        /// <para>
        /// Returns <c>true</c> when <paramref name="codePoint"/> is a valid CONTINUATION character inside
        /// a SPARQL variable name (VARNAME) — i.e. any character valid at position 2 or later.
        /// </para>
        /// <para>
        /// SPARQL grammar: the tail of a VARNAME may contain PN_CHARS_U, decimal digits, and additional
        /// Unicode ranges: U+00B7 (middle dot), U+0300–U+036F (combining diacritics), and
        /// U+203F–U+2040 (undertie / character tie). The hyphen <c>-</c> is intentionally excluded:
        /// unlike Turtle's PN_CHARS, SPARQL VARNAME does not allow hyphens after the first character.
        /// </para>
        /// </summary>
        private static bool IsVarNameChar(int codePoint)
            => RDFTurtle.IsPN_CHARS_U(codePoint)
               || RDFTurtle.IsNumber(codePoint)
               || codePoint == 0x00B7                              // MIDDLE DOT
               || (codePoint >= 0x0300 && codePoint <= 0x036F)    // Combining Diacritical Marks
               || (codePoint >= 0x203F && codePoint <= 0x2040);   // UNDERTIE, CHARACTER TIE
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