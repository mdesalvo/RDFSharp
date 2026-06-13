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
using RDFSharp.Model;
using RDFSharp.Store;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// Graph-pattern half of the SPARQL parser: the WHERE clause and the OPTIONAL/UNION/MINUS/GRAPH group-graph-pattern algebra.
    /// </summary>
    internal static partial class RDFQueryParser
    {
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
        ///   <c>IsOptional</c> flag is set to <c>true</c>, and it is appended to the
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
        ///   <b>GRAPH</b> — delegated to <see cref="ParseGraphGraphPattern"/>: the following group is parsed
        ///   with the parsed graph specifier (a fixed <see cref="RDFContext"/> or an <see cref="RDFVariable"/>)
        ///   fixed as the per-pattern context of every triple it contains, then appended like any other element.
        /// </item>
        /// <item>
        ///   <b>Deferred keywords</b> (FILTER, SERVICE, BIND, VALUES, SELECT) — a parse error is raised with an
        ///   explicit "not supported yet" message until the corresponding phase lands.
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

                //GraphGraphPattern ::= 'GRAPH' VarOrIri GroupGraphPattern
                //GRAPH is NOT an algebra tree node: it is a per-pattern context DECORATION. The dispatcher
                //(ParseGraphGraphPattern) parses the graph specifier (a fixed IRI → RDFContext, or a variable →
                //RDFVariable), then parses the following GroupGraphPattern with that context in force so every
                //triple pattern inside the braces is built with the four-argument RDFPattern constructor. The
                //resulting member is appended to the accumulator just like any other element, which keeps GRAPH
                //fully orthogonal to the UNION/MINUS/OPTIONAL algebra: the context is already fixed on the
                //patterns BEFORE any subquery wrapping or tree-node combination happens.
                if (upcomingKeyword == "GRAPH")
                {
                    ConsumeKeyword(parserContext);
                    accumulatedMembers.Add(ParseGraphGraphPattern(parserContext));
                    continue;
                }

                //FILTER, BIND and VALUES are NOT algebra tree nodes and NOT sibling members: they are
                //RDFPatternGroupMembers that belong INSIDE a pattern group (RDFPatternGroup.AddFilter / AddBind /
                //AddValues). When one of them opens the group body (before any triple), it still has to land in a
                //pattern group, so we route it to ParseBasicGraphPatternMember — which builds a fresh pattern group
                //and absorbs the member (and any triples around it) into that single group, exactly as it does when
                //the member follows a triple run.
                if (upcomingKeyword == "FILTER" || upcomingKeyword == "BIND" || upcomingKeyword == "VALUES")
                {
                    accumulatedMembers.Add(ParseBasicGraphPatternMember(parserContext));
                    continue;
                }

                //SubSelect ::= a nested SELECT query that is the sole content of a group graph pattern
                //(GroupGraphPattern ::= '{' ( SubSelect | GroupGraphPatternSub ) '}'). It becomes a subquery
                //member (RDFSelectQuery): the engine evaluates it independently and joins its projected bindings
                //with the sibling members, just like the SELECT * subqueries the parser synthesises via
                //WrapIntoSubQuery — except here the projection/modifiers are author-specified. The dedicated
                //ParseSubSelectQuery (RDFQueryParser.SubQuery.cs) consumes the keyword and parses the nested query.
                if (upcomingKeyword == "SELECT")
                {
                    accumulatedMembers.Add(ParseSubSelectQuery(parserContext));
                    continue;
                }

                //Any other recognized graph-pattern keyword (SERVICE)
                //belongs to a parser phase that has not been implemented yet. Throw with a precise message
                //naming the exact unsupported keyword, so the caller knows what is missing.
                if (upcomingKeyword.Length > 0)
                    throw new RDFQueryException("Cannot parse SPARQL query: '" + upcomingKeyword + "' is not supported yet " + GetCoordinates(parserContext));

                //No keyword was recognised. Dispatch on the first character:
                //  '{' → a nested GroupOrUnionGraphPattern (UNION chain or bare group)
                //  anything else → a bare TriplesBlock (sequence of triple patterns)
                RDFQueryMember parsedElement = nextSignificantCodePoint == '{'
                    ? ParseGroupOrUnionGraphPattern(parserContext)
                    : ParseBasicGraphPatternMember(parserContext);
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
        /// Parses a <c>GraphGraphPattern</c> — the <c>GRAPH</c> keyword (already consumed by the caller),
        /// followed by a graph specifier and a <c>GroupGraphPattern</c> — and returns the single algebra
        /// member the braces collapse to. Every triple pattern produced inside the braces is decorated with
        /// the parsed graph context.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>GraphGraphPattern ::= 'GRAPH' VarOrIri GroupGraphPattern</c>.
        /// </para>
        /// <para>
        /// MODEL MAPPING. RDFSharp has no explicit <c>Graph(var, P)</c> algebra node: GRAPH is represented by
        /// fixing the context on each individual <see cref="RDFPattern"/> (the four-argument constructor). This
        /// is faithful to the spec because the SHARED context — same <see cref="RDFContext"/> object, or same
        /// <see cref="RDFVariable"/> — is the join key: when the specifier is a variable, the engine binds it
        /// per-quadruple for every contextualised pattern and joins on it, forcing all those patterns into the
        /// same named graph (exactly <c>∪_g eval(P against graph g)</c>); when it is a fixed IRI, the engine
        /// filters every contextualised pattern to that one graph. The runtime target of GRAPH is therefore an
        /// <see cref="RDFStore"/> (the RDF dataset); against a single <see cref="RDFGraph"/> the engine ignores
        /// the context, which is the correct SPARQL behaviour (a lone graph is just the default graph).
        /// </para>
        /// <para>
        /// CONTEXT DISCIPLINE (save → set → recurse → restore). The graph context is pushed onto the shared
        /// <see cref="RDFQueryParserContext.CurrentGraphScope"/> slot for the duration of the inner
        /// <see cref="ParseGroupGraphPattern"/> call and then popped back. This single mutable slot, bracketed
        /// per dispatch, yields SPARQL §18.4 innermost shadowing for free: a nested GRAPH overrides the slot
        /// for its own sub-scope and restores the enclosing context on exit, so each triple pattern is bound to
        /// the graph of the GRAPH clause that most tightly encloses it (e.g. in
        /// <c>GRAPH ?g { ?s ?p ?o GRAPH ?h { ?a ?b ?c } }</c> the <c>?s ?p ?o</c> pattern is contextualised by
        /// <c>?g</c> while <c>?a ?b ?c</c> is contextualised by <c>?h</c>).
        /// </para>
        /// <para>
        /// EMPTY GRAPH REJECTION. An empty group (<c>GRAPH ?g {}</c> or <c>GRAPH &lt;iri&gt; {}</c>) is rejected
        /// with an <see cref="RDFQueryException"/>. The per-pattern model cannot represent it: a SPARQL empty
        /// group is the join identity (a single empty solution), so <c>GRAPH ?g {}</c> would ENUMERATE the named
        /// graphs (one solution per graph, with <c>?g</c> bound to each graph IRI) — but with no pattern to carry
        /// the context there is nothing to bind <c>?g</c> to. Rather than silently produce a wrong result we
        /// reject. Detection is exact via the <see cref="RDFQueryParserContext.CurrentGraphScope"/> WasUsed flag, which
        /// is true iff at least one pattern was actually built under THIS clause's context (a nested GRAPH does
        /// not satisfy its parent's check, because the discipline brackets the flag too).
        /// </para>
        /// </summary>
        /// <returns>The single algebra member the GRAPH's group graph pattern collapses to.</returns>
        /// <exception cref="RDFQueryException">When the graph specifier is malformed or the group is empty.</exception>
        private static RDFQueryMember ParseGraphGraphPattern(RDFQueryParserContext parserContext)
        {
            //Parse the VarOrIri graph specifier that names the active graph for the upcoming group
            RDFPatternMember graphContext = ParseGraphContext(parserContext);

            //SAVE the enclosing scope (context + usage flag) as a single value so we can restore it after this
            //GRAPH's sub-scope. Saving the flag too is what makes the emptiness check immune to nesting: a nested
            //GRAPH resets and consumes its own flag, then hands our scope back untouched.
            (RDFPatternMember ActiveContext, bool WasUsed) enclosingGraphScope = parserContext.CurrentGraphScope;

            //SET this clause's context as the one in force, with a fresh (unused) flag so it reflects ONLY the
            //patterns produced directly by this clause's scope.
            parserContext.CurrentGraphScope = (graphContext, false);

            //RECURSE into the mandatory GroupGraphPattern operand. While this runs, every triple pattern emitted
            //by ParseObjectList is built with graphContext as its context (unless an inner GRAPH shadows it).
            RDFQueryMember graphScopeMember = ParseGroupGraphPattern(parserContext);

            //Capture whether this clause's scope produced any contextualised pattern BEFORE restoring the scope
            bool thisGraphScopeProducedAPattern = parserContext.CurrentGraphScope.WasUsed;

            //RESTORE the enclosing scope for whatever follows this GRAPH clause
            parserContext.CurrentGraphScope = enclosingGraphScope;

            //Reject the non-representable empty GRAPH: no contextualised pattern means there was nothing for the
            //graph specifier to attach to (see the EMPTY GRAPH REJECTION note above).
            if (!thisGraphScopeProducedAPattern)
                throw new RDFQueryException("Cannot parse SPARQL GRAPH clause: an empty group graph pattern ('GRAPH ... { }') is not supported, because the per-pattern model has no pattern to anchor the graph specifier to " + GetCoordinates(parserContext));

            return graphScopeMember;
        }

        /// <summary>
        /// <para>
        /// Parses the graph specifier of a <c>GRAPH</c> clause — the <c>VarOrIri</c> between the keyword and the
        /// group's opening brace — into the <see cref="RDFPatternMember"/> that will decorate the scope's
        /// patterns as their context.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>VarOrIri ::= Var | iri</c>. A variable (<c>?g</c> / <c>$g</c>) becomes an
        /// <see cref="RDFVariable"/> that the engine binds per-quadruple; an IRI (an <c>IRIREF</c> or a prefixed
        /// name resolved through the prologue) becomes a fixed <see cref="RDFContext"/>. A literal or a blank
        /// node in this position is invalid and is rejected with an <see cref="RDFQueryException"/>: the model's
        /// pattern context accepts only an RDFContext or an RDFVariable.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the specifier is missing, a literal, or a blank node.</exception>
        private static RDFPatternMember ParseGraphContext(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //A '?' or '$' sigil starts a variable graph specifier: dynamic, bound by the engine per-quadruple
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParseVariable(parserContext);

            //Otherwise the specifier must be an IRI (IRIREF or prefixed name): parse it through the shared
            //term-reader so prologue BASE/PREFIX resolution applies, then validate it is a usable graph IRI.
            RDFPatternMember graphTerm = ParseTerm(parserContext);

            //A literal can never name a graph
            if (!(graphTerm is RDFResource graphResource))
                throw new RDFQueryException("Cannot parse SPARQL GRAPH clause: the graph specifier must be a variable or an IRI, but a literal was found " + GetCoordinates(parserContext));

            //A blank node can never name a graph either
            if (graphResource.IsBlank)
                throw new RDFQueryException("Cannot parse SPARQL GRAPH clause: the graph specifier must be a variable or an IRI, but a blank node was found " + GetCoordinates(parserContext));

            try
            {
                //Promote the IRI resource to an RDFContext (the fixed-graph form of a pattern context)
                return new RDFContext(graphResource.ToString());
            }
            catch (RDFStoreException graphContextException)
            {
                //RDFContext rejected the IRI (e.g. a reserved scheme): surface it as a query-level parse error
                throw new RDFQueryException("Cannot parse SPARQL GRAPH clause: invalid graph IRI '" + graphResource + "' " + GetCoordinates(parserContext) + ": " + graphContextException.Message, graphContextException);
            }
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
            //Allocate a fresh pattern group to collect the triples (and filters) produced by this BGP scan
            RDFPatternGroup basicGraphPatternGroup = new RDFPatternGroup();

            //A pattern-group member is a maximal run of triple blocks INTERLEAVED with the inline pattern-group
            //members FILTER / BIND / VALUES. ParseTriplesBlock reads the triples and stops at any graph-pattern
            //keyword; when that keyword is one of those three inline members we absorb it into THIS SAME group and
            //resume reading more triples, so that
            //   { ?s ?p ?o FILTER(?o > 1) BIND(?o + 1 AS ?n) ?a ?b ?c }
            //collapses to one RDFPatternGroup carrying both triple runs, the filter and the bind. The loop ends as
            //soon as the next keyword is a true algebra element (OPTIONAL/UNION/MINUS/GRAPH/…) or a block boundary
            //('}','{',EOF): those are left untouched on the reader for ParseGroupGraphPatternSub to dispatch as
            //separate members.
            while (true)
            {
                //A '.' is the optional separator the SPARQL grammar allows between a TriplesBlock and a following
                //GraphPatternNotTriples (here FILTER/BIND/VALUES), and between two consecutive such members. Consume
                //it up-front so neither ParseTriplesBlock (which would choke on a leading dot) nor the peek below trips on it.
                if (SkipWhitespace(parserContext) == '.')
                    ReadCodePoint(parserContext);

                //Read the triples available at the current position into the group (may read zero, e.g. when the
                //body opens directly with FILTER/BIND/VALUES, or two such members sit back-to-back)
                ParseTriplesBlock(parserContext, basicGraphPatternGroup);

                //If the next significant token is an inline pattern-group member, consume the keyword and parse it into
                //THIS group, then loop to pick up any further triples/members of the same pattern-group member
                string upcomingKeyword = PeekGraphPatternKeyword(parserContext);
                if (upcomingKeyword == "FILTER")
                {
                    ConsumeKeyword(parserContext);
                    ParseFilter(parserContext, basicGraphPatternGroup);
                    continue;
                }
                if (upcomingKeyword == "BIND")
                {
                    ConsumeKeyword(parserContext);
                    ParseBind(parserContext, basicGraphPatternGroup);
                    continue;
                }
                if (upcomingKeyword == "VALUES")
                {
                    ConsumeKeyword(parserContext);
                    ParseValues(parserContext, basicGraphPatternGroup);
                    continue;
                }

                //Not an inline pattern-group member: this pattern-group member is complete
                return basicGraphPatternGroup;
            }
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
    }
}