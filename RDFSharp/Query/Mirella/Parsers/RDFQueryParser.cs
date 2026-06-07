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