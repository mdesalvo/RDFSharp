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
using RDFSharp.Model;
using RDFSharp.Store;
using static RDFSharp.Query.RDFQueryLexer;

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
    internal static partial class RDFQueryParser
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

            /// <summary>
            /// <para>
            /// The GRAPH scope currently in force while triple patterns are being built — a single value pairing
            /// the two pieces of state that must always live and die together:
            /// <list type="bullet">
            /// <item><b>ActiveContext</b> — the graph specifier of the innermost enclosing <c>GRAPH</c> clause.
            ///   It is <c>null</c> whenever no GRAPH clause is active (the default graph), in which case
            ///   <see cref="ParseObjectList"/> builds plain three-argument patterns; when it is non-null, every
            ///   triple pattern emitted by ParseObjectList is built with the four-argument <see cref="RDFPattern"/>
            ///   constructor so the context decorates the pattern. The value is always one of the two types the
            ///   model accepts as a pattern context: an <see cref="RDFContext"/> (a fixed graph IRI) or an
            ///   <see cref="RDFVariable"/> (a dynamic graph variable the engine binds per-quadruple).</item>
            /// <item><b>WasUsed</b> — whether at least one triple pattern was actually built under the
            ///   ActiveContext currently in force. ParseObjectList raises it every time it emits a context-decorated
            ///   pattern. It is the detection mechanism for the forbidden empty GRAPH (<c>GRAPH ?g {}</c> /
            ///   <c>GRAPH &lt;iri&gt; {}</c>): if a GRAPH scope produces no contextualised pattern, WasUsed is still
            ///   <c>false</c> on exit and the dispatcher rejects the clause.</item>
            /// </list>
            /// </para>
            /// <para>
            /// This single shared slot is written ONLY by the GRAPH dispatcher
            /// (<see cref="ParseGraphGraphPattern"/>) with a strict save → set → recurse → restore discipline.
            /// Bundling both fields into ONE value is what makes that discipline atomic: the dispatcher saves and
            /// restores a single tuple rather than juggling two variables in lock-step. The discipline gives
            /// SPARQL's innermost-shadowing semantics for free — a nested GRAPH overrides the slot for its own
            /// sub-scope and restores the outer value on exit, so each triple pattern is contextualised by the
            /// GRAPH that most tightly encloses it, and a nested GRAPH never falsely satisfies the emptiness check
            /// of its enclosing GRAPH.
            /// </para>
            /// </summary>
            internal (RDFPatternMember ActiveContext, bool WasUsed) CurrentGraphScope { get; set; }

            /// <summary>
            /// <para>
            /// Monotonic counter feeding the names of the FRESH variables that the parser synthesises for every
            /// ANONYMOUS blank node it desugars — the empty <c>[]</c>, each blank-node property list <c>[ … ]</c>,
            /// and every internal list node of a collection <c>( … )</c>. Each call to
            /// <see cref="NewAnonymousBlankNodeVariable"/> increments it, so two anonymous blank nodes in the same
            /// query never collide.
            /// </para>
            /// <para>
            /// DESIGN NOTE (decided 2026-06-11). In SPARQL a blank node inside a query is an EXISTENTIAL — it
            /// behaves like a non-distinguished variable, matching any term in that position. RDFSharp's engine,
            /// however, treats a blank-node <c>RDFResource</c> in a pattern as a CONSTANT (only a fixed-position
            /// hole that <c>is RDFVariable</c> is a join hole). Desugaring blank nodes into fresh blank-node
            /// resources would therefore never match real data. So the parser maps EVERY query blank node — the
            /// anonymous ones here and the labelled <c>_:x</c> ones in <see cref="LabeledBlankNodeVariables"/> —
            /// onto fresh <see cref="RDFVariable"/> instances, which the engine evaluates with the intended
            /// existential semantics.
            /// </para>
            /// </summary>
            internal int AnonymousBlankNodeCounter { get; set; }

            /// <summary>
            /// <para>
            /// Maps each LABELLED blank node label (the <c>x</c> of a <c>_:x</c> token, without the <c>_:</c>
            /// prefix) to the single <see cref="RDFVariable"/> that represents it throughout this query. Looking
            /// the label up here guarantees that every occurrence of the same <c>_:x</c> resolves to the SAME
            /// variable, exactly as SPARQL requires a repeated blank node label to denote the same node.
            /// </para>
            /// <para>
            /// Comparison is ordinal because blank node labels are case-sensitive in the SPARQL grammar (even
            /// though the resulting RDFVariable name is upper-cased by RDFVariable itself). See the design note on
            /// <see cref="AnonymousBlankNodeCounter"/> for why labelled blank nodes become variables too.
            /// </para>
            /// </summary>
            internal Dictionary<string, RDFVariable> LabeledBlankNodeVariables { get; }
                = new Dictionary<string, RDFVariable>(StringComparer.Ordinal);
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

        #region Query
        /// <summary>
        /// <para>
        /// Parses a complete SPARQL query string into the corresponding RDFQuery object-model instance.
        /// This is the single entry point of the engine: it builds a fresh parsing context over the text,
        /// consumes the (possibly empty) prologue of BASE/PREFIX declarations, then dispatches on the query
        /// form keyword (SELECT/ASK/CONSTRUCT/DESCRIBE) to the matching form-parser.
        /// </para>
        /// <para>
        /// Phase note: SELECT and ASK are implemented; CONSTRUCT and DESCRIBE are recognized (so that the error
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
                case "SELECT":
                    return ParseSelectQuery(parserContext);

                case "ASK":
                    return ParseAskQuery(parserContext);

                //These are valid SPARQL forms that simply have not been wired up yet. We match them explicitly so the
                //error names the exact form the author used ("'CONSTRUCT' ... not supported yet") instead of the
                //misleading "unexpected token" message of the default branch. Each gets its real parser in a later phase.
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

    }

    /// <summary>
    /// RDFQueryParserFactory is the public entry point for turning a SPARQL 1.1 query string into the matching
    /// RDFQuery object-model instance. It reads the query's prologue and form keyword and returns the concrete
    /// query type (RDFSelectQuery, RDFAskQuery, RDFConstructQuery, RDFDescribeQuery) as an RDFQuery. The strongly
    /// typed <c>FromString</c> helpers on the concrete query classes delegate here and then validate the form.
    /// </summary>
    public static class RDFQueryParserFactory
    {
        #region Methods
        /// <summary>
        /// Parses the given SPARQL 1.1 query string into its RDFQuery object-model representation.
        /// </summary>
        /// <exception cref="RDFQueryException">When the query string is null/empty or syntactically invalid.</exception>
        public static RDFQuery ParseQuery(string queryString)
            => RDFQueryParser.ParseQuery(queryString);
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