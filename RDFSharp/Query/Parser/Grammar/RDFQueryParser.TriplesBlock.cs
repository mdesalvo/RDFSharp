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

using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// Triples half of the SPARQL parser: the basic graph pattern (predicate-object/object lists) and the blank-node / collection desugaring.
    /// </summary>
    internal static partial class RDFQueryParser
    {
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

                //A subject that begins with '[' or '(' is a TriplesNode (a blank-node property list or a
                //collection). Per the SPARQL grammar these two productions differ in whether the trailing
                //property list is mandatory:
                //  TriplesSameSubject ::= VarOrTerm PropertyListNotEmpty   (plain subject → property list REQUIRED)
                //                       | TriplesNode  PropertyList         (bnode/collection → property list OPTIONAL)
                //A TriplesNode can already have emitted its own triples (the bnode's nested list, or the
                //collection's rdf:first/rdf:rest chain), so it may legitimately stand alone with no outer verb.
                bool subjectIsTriplesNode = nextSignificantCodePoint == '[' || nextSignificantCodePoint == '(';

                //Parse the subject term. For a TriplesNode this also emits its internal triples into the group
                //and returns the fresh variable standing for its (head) node.
                RDFPatternMember tripleSubject = ParseVariableOrTerm(parserContext, targetPatternGroup);

                //Decide whether an outer predicate-object list follows. It is mandatory after a plain subject,
                //optional after a TriplesNode: if the TriplesNode is immediately followed by a block boundary
                //(., }, {, keyword or EOF) we skip the property list and keep only the triples it already emitted.
                int codePointAfterSubject = SkipWhitespace(parserContext);
                bool atTripleBoundary = codePointAfterSubject == '.' || codePointAfterSubject == '}'
                                        || codePointAfterSubject == '{' || codePointAfterSubject == -1
                                        || PeekGraphPatternKeyword(parserContext).Length > 0;
                if (!(subjectIsTriplesNode && atTripleBoundary))
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
                //Parse the next verb for this subject. The verb is either a simple predicate (variable or IRI) —
                //yielding plain triple patterns — or a full SPARQL property path — yielding RDFPropertyPath members.
                ParsedVerb verb = ParseVerb(parserContext);
                if (verb.IsVariable)
                    ParseObjectList(parserContext, targetPatternGroup, tripleSubject, verb.Variable);
                else if (verb.IsSimpleIri)
                    ParseObjectList(parserContext, targetPatternGroup, tripleSubject, verb.SimpleIri);
                else
                    ParsePathObjectList(parserContext, targetPatternGroup, tripleSubject, verb.PathUnits);

                //A ';' introduces a second (or further) predicate-object group on the same subject
                if (SkipWhitespace(parserContext) == ';')
                {
                    ReadCodePoint(parserContext);

                    //After consuming the ';', check whether we are immediately at a block boundary or a keyword.
                    //If so, this was a trailing ';' — legal in SPARQL/Turtle, so we stop the predicate-object
                    //list without requiring another predicate to follow. The ']' boundary covers a trailing ';'
                    //inside a blank-node property list, e.g. '[ :p :o ; ]'.
                    int codePointAfterSemicolon = SkipWhitespace(parserContext);
                    if (codePointAfterSemicolon == '.' || codePointAfterSemicolon == '}'
                        || codePointAfterSemicolon == '{' || codePointAfterSemicolon == ']'
                        || codePointAfterSemicolon == -1)
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
                //Parse the object term: a variable, a concrete RDF term, or a TriplesNode ('[' blank-node
                //property list / '(' collection) which emits its own internal triples and returns the fresh
                //variable standing for its (head) node.
                RDFPatternMember tripleObject = ParseVariableOrTerm(parserContext, targetPatternGroup);

                //Emit the subject-predicate-object triple pattern into the group, with GRAPH context if active.
                EmitPattern(parserContext, targetPatternGroup, tripleSubject, triplePredicate, tripleObject);

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
        /// Emits a single triple pattern into <paramref name="targetPatternGroup"/>, applying the active GRAPH
        /// context if one is in force. This is the ONE place where the per-pattern context decoration lives, so
        /// both ordinary object-list triples and the triples synthesised by blank-node / collection desugaring
        /// share identical contextualisation behaviour.
        /// </para>
        /// <para>
        /// GRAPH contextualisation: when <see cref="RDFQueryParserContext.CurrentGraphScope"/>.ActiveContext is
        /// non-null (an RDFContext fixed IRI or an RDFVariable), the FOUR-argument <see cref="RDFPattern"/>
        /// constructor is used so the context decorates the triple, and the scope's WasUsed flag is flipped so
        /// the GRAPH dispatcher can tell its scope was non-empty (the scope is a value tuple, so flipping WasUsed
        /// means re-assigning the slot while keeping the same ActiveContext). Outside any GRAPH clause the
        /// THREE-argument constructor is used (default graph). The RDFPattern constructor also enforces SPARQL's
        /// term-position rules (e.g. a literal in subject position is rejected), surfaced as RDFQueryException.
        /// </para>
        /// </summary>
        private static void EmitPattern(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup,
            RDFPatternMember tripleSubject, RDFPatternMember triplePredicate, RDFPatternMember tripleObject)
        {
            if (parserContext.CurrentGraphScope.ActiveContext == null)
            {
                targetPatternGroup.AddPattern(new RDFPattern(tripleSubject, triplePredicate, tripleObject));
            }
            else
            {
                targetPatternGroup.AddPattern(new RDFPattern(parserContext.CurrentGraphScope.ActiveContext, tripleSubject, triplePredicate, tripleObject));
                parserContext.CurrentGraphScope = (parserContext.CurrentGraphScope.ActiveContext, true);
            }
        }

        /// <summary>
        /// <para>
        /// Parses an RDF term in subject or object position and returns the <see cref="RDFPatternMember"/> that
        /// stands for it. The dispatch covers every form the SPARQL grammar allows in those positions:
        /// </para>
        /// <list type="bullet">
        /// <item><b>Variable</b> (<c>?name</c> / <c>$name</c>) → an <see cref="RDFVariable"/>.</item>
        /// <item><b>Labelled blank node</b> (<c>_:x</c>) → a fresh-but-stable <see cref="RDFVariable"/> shared by
        ///   every occurrence of the same label (see <see cref="ParseLabeledBlankNodeVariable"/>).</item>
        /// <item><b>Blank-node property list</b> (<c>[ … ]</c> / <c>[]</c>) → desugared by
        ///   <see cref="ParseBlankNodePropertyList"/>, which emits the nested triples and returns the fresh
        ///   variable for the blank node.</item>
        /// <item><b>Collection</b> (<c>( … )</c> / <c>()</c>) → desugared by <see cref="ParseCollection"/> into an
        ///   rdf:first/rdf:rest/rdf:nil chain; returns the head variable, or rdf:nil for the empty collection.</item>
        /// <item><b>Concrete term</b> (IRI, prefixed name, literal, numeric/boolean) → delegated to the shared
        ///   Turtle term-reader via <see cref="ParseTerm"/>.</item>
        /// </list>
        /// <para>
        /// The three desugaring forms ('[', '(', '_:') need the enclosing <paramref name="targetPatternGroup"/>
        /// because they emit extra triple patterns into it; the simple forms ignore it.
        /// </para>
        /// </summary>
        private static RDFPatternMember ParseVariableOrTerm(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //A '?' or '$' sigil is the unambiguous start of a SPARQL variable reference
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParseVariable(parserContext);

            //A '[' opens a blank-node property list (or the empty anonymous blank node '[]')
            if (nextSignificantCodePoint == '[')
                return ParseBlankNodePropertyList(parserContext, targetPatternGroup);

            //A '(' opens an RDF collection
            if (nextSignificantCodePoint == '(')
                return ParseCollection(parserContext, targetPatternGroup);

            //A '_' opens a labelled blank node '_:x' (the only token that can start with '_' here)
            if (nextSignificantCodePoint == '_')
                return ParseLabeledBlankNodeVariable(parserContext);

            //Any other character starts a concrete RDF term: delegate to the shared Turtle term-reader
            return ParseTerm(parserContext);
        }

        /// <summary>
        /// <para>
        /// Parses a blank-node property list — <c>[ PredicateObjectList ]</c> — or the empty anonymous blank
        /// node <c>[]</c>. A fresh <see cref="RDFVariable"/> is synthesised for the blank node; when the brackets
        /// contain a predicate-object list, those triples are emitted into <paramref name="targetPatternGroup"/>
        /// with that variable as their subject. The variable is returned so the caller can use the blank node as
        /// the subject or object of an enclosing triple.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>BlankNodePropertyList ::= '[' PropertyListNotEmpty ']'</c>, plus the
        /// <c>ANON ::= '[' WS* ']'</c> token for the empty form.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the brackets are unbalanced or the inner list is malformed.</exception>
        private static RDFVariable ParseBlankNodePropertyList(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup)
        {
            //Consume the opening '['
            ExpectChar(parserContext, '[', "blank node property list");

            //Synthesise the fresh variable that represents this anonymous blank node
            RDFVariable blankNodeVariable = NewAnonymousBlankNodeVariable(parserContext);

            //The empty form '[]' has no nested list: consume the ']' and return the bare variable
            if (SkipWhitespace(parserContext) == ']')
            {
                ReadCodePoint(parserContext);
                return blankNodeVariable;
            }

            //Non-empty form: the blank node is the subject of the bracketed predicate-object list
            ParsePredicateObjectList(parserContext, targetPatternGroup, blankNodeVariable);

            //Consume the closing ']'
            ExpectChar(parserContext, ']', "blank node property list");

            return blankNodeVariable;
        }

        /// <summary>
        /// <para>
        /// Parses an RDF collection — <c>( item1 item2 … )</c> — and desugars it into the canonical
        /// rdf:first/rdf:rest/rdf:nil linked-list triples (SPARQL §17.4 / Turtle collection translation). Each
        /// list node is a fresh <see cref="RDFVariable"/>; the head variable is returned so the collection can be
        /// used as the subject or object of an enclosing triple. The empty collection <c>()</c> desugars to no
        /// triples at all and is represented directly by <c>rdf:nil</c>.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>Collection ::= '(' GraphNode+ ')'</c>; <c>GraphNode ::= VarOrTerm | TriplesNode</c>,
        /// so an item may itself be a variable, a term, a nested collection or a blank-node property list.
        /// </para>
        /// <para>
        /// SPEC FIDELITY: unlike RDFSharp's Turtle deserialiser, the desugaring deliberately does NOT emit any
        /// <c>rdf:type rdf:List</c> typing triple — the SPARQL collection translation is purely
        /// rdf:first/rdf:rest/rdf:nil. Adding the typing triple would over-constrain the pattern, making it match
        /// only lists explicitly typed as rdf:List in the data.
        /// </para>
        /// </summary>
        /// <returns>The fresh head variable of the list, or <c>rdf:nil</c> for the empty collection.</returns>
        /// <exception cref="RDFQueryException">When the parentheses are unbalanced or an item is malformed.</exception>
        private static RDFPatternMember ParseCollection(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup)
        {
            //Consume the opening '('
            ExpectChar(parserContext, '(', "collection");

            //The empty collection '()' is exactly rdf:nil and emits no triples
            if (SkipWhitespace(parserContext) == ')')
            {
                ReadCodePoint(parserContext);
                return new RDFResource(RDFVocabulary.RDF.NIL.ToString());
            }

            //The head variable that the caller will receive (subject of the first rdf:first/rdf:rest pair)
            RDFVariable listHeadVariable = NewAnonymousBlankNodeVariable(parserContext);
            RDFVariable currentListNode = listHeadVariable;

            while (true)
            {
                //Parse one collection item (variable, term, nested collection or blank-node property list)
                RDFPatternMember collectionItem = ParseVariableOrTerm(parserContext, targetPatternGroup);

                //currentNode rdf:first item
                EmitPattern(parserContext, targetPatternGroup, currentListNode,
                    new RDFResource(RDFVocabulary.RDF.FIRST.ToString()), collectionItem);

                //A ')' closes the collection: link the final node's rdf:rest to rdf:nil and stop
                if (SkipWhitespace(parserContext) == ')')
                {
                    ReadCodePoint(parserContext);
                    EmitPattern(parserContext, targetPatternGroup, currentListNode,
                        new RDFResource(RDFVocabulary.RDF.REST.ToString()), new RDFResource(RDFVocabulary.RDF.NIL.ToString()));
                    break;
                }

                //More items follow: link the current node's rdf:rest to a fresh node and advance to it
                RDFVariable nextListNode = NewAnonymousBlankNodeVariable(parserContext);
                EmitPattern(parserContext, targetPatternGroup, currentListNode,
                    new RDFResource(RDFVocabulary.RDF.REST.ToString()), nextListNode);
                currentListNode = nextListNode;
            }

            return listHeadVariable;
        }

        /// <summary>
        /// <para>
        /// Parses a labelled blank node <c>_:x</c> and maps it onto the single <see cref="RDFVariable"/> that
        /// represents that label throughout the query (the first occurrence creates it, later occurrences reuse
        /// it). The underlying label is read by the shared Turtle node-id reader, then stripped of its
        /// <c>bnode:</c> internal prefix to recover the original label.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the <c>_:x</c> token is malformed.</exception>
        private static RDFVariable ParseLabeledBlankNodeVariable(RDFQueryParserContext parserContext)
        {
            try
            {
                //Reuse the Turtle node-id reader: it returns an RDFResource whose URI is "bnode:<label>"
                RDFResource blankNodeResource = RDFTurtle.ParseNodeID(parserContext.TermParsingContext);
                string blankNodeLabel = blankNodeResource.ToString().Substring("bnode:".Length);

                //Resolve (or create) the stable variable that this label denotes within the query
                if (!parserContext.LabeledBlankNodeVariables.TryGetValue(blankNodeLabel, out RDFVariable labeledBlankNodeVariable))
                {
                    //Use a reserved-looking, valid VARNAME so it round-trips as a plain variable and is unlikely
                    //to collide with author-written variables: "_BNODE_" + the original label.
                    labeledBlankNodeVariable = new RDFVariable("_BNODE_" + blankNodeLabel);
                    parserContext.LabeledBlankNodeVariables[blankNodeLabel] = labeledBlankNodeVariable;
                }
                return labeledBlankNodeVariable;
            }
            catch (RDFModelException blankNodeParsingException)
            {
                throw new RDFQueryException("Cannot parse SPARQL blank node " + GetCoordinates(parserContext) + ": " + blankNodeParsingException.Message, blankNodeParsingException);
            }
        }

        /// <summary>
        /// Synthesises a fresh <see cref="RDFVariable"/> for an anonymous blank node (the <c>[]</c> /
        /// <c>[ … ]</c> node, or an internal collection list node). The name uses a reserved-looking, valid
        /// VARNAME prefix and a monotonic counter so each anonymous node is distinct and round-trips as a plain
        /// variable: <c>_BNODE0</c>, <c>_BNODE1</c>, …
        /// </summary>
        private static RDFVariable NewAnonymousBlankNodeVariable(RDFQueryParserContext parserContext)
        {
            RDFVariable anonymousBlankNodeVariable = new RDFVariable("_BNODE" + parserContext.AnonymousBlankNodeCounter);
            parserContext.AnonymousBlankNodeCounter++;
            return anonymousBlankNodeVariable;
        }

        #endregion
    }
}