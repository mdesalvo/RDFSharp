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
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// <para>
    /// Property-path half of the SPARQL parser: the verb of a triples block can be a full SPARQL 1.1
    /// <c>Path</c> expression rather than a single predicate. This file parses the W3C SPARQL 1.1 path grammar
    /// directly into RDFSharp's recursive <see cref="RDFPropertyPathExpression"/> tree (the model represents every
    /// spec-legal path shape, so there is no lowering step and no rejected forms).
    /// </para>
    /// <para>
    /// SPARQL 1.1 grammar (W3C §19.8 — the SINGLE source of truth here, NOT the printer):
    /// <code>
    /// [88]  Path                   ::= PathAlternative
    /// [89]  PathAlternative        ::= PathSequence ( '|' PathSequence )*
    /// [90]  PathSequence           ::= PathEltOrInverse ( '/' PathEltOrInverse )*
    /// [91]  PathElt                ::= PathPrimary PathMod?
    /// [92]  PathEltOrInverse       ::= PathElt | '^' PathElt
    /// [93]  PathMod                ::= '?' | '*' | '+'
    /// [94]  PathPrimary            ::= iri | 'a' | '!' PathNegatedPropertySet | '(' Path ')'
    /// [95]  PathNegatedPropertySet ::= PathOneInPropertySet | '(' ( PathOneInPropertySet ( '|' PathOneInPropertySet )* )? ')'
    /// [96]  PathOneInPropertySet   ::= iri | 'a' | '^' ( iri | 'a' )
    /// </code>
    /// The W3C grammar has NO <c>{n,m}</c> cardinality on paths — <c>PathMod</c> is exactly <c>'?' | '*' | '+'</c>.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region PropertyPath

        #region Verb dispatch
        /// <summary>
        /// <para>
        /// Parses the verb of a triples block. The verb is either a simple variable predicate
        /// (<c>VerbSimple ::= Var</c>) or a full property path (<c>VerbPath ::= Path</c>); this method peeks the
        /// leading sigil to decide and returns a <see cref="ParsedVerb"/> that the caller (TriplesBlock) dispatches on.
        /// </para>
        /// <para>
        /// A path that is a single bare predicate (no inverse, no cardinality) is reported as a SIMPLE IRI
        /// predicate, not as a path: <c>?s :p ?o</c> must stay a plain <see cref="RDFPattern"/> (so plain triples
        /// keep round-tripping as triples) while <c>?s :p+ ?o</c> / <c>?s ^:p ?o</c> / <c>?s :p/:q ?o</c> become an
        /// <see cref="RDFPropertyPath"/>.
        /// </para>
        /// </summary>
        private static ParsedVerb ParseVerb(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //A '?' or '$' sigil is the unambiguous start of a variable predicate (VerbSimple)
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParsedVerb.FromVariable(ParseVariable(parserContext));

            //Otherwise the verb is a property path (VerbPath): parse the W3C path grammar into the expression tree
            RDFPropertyPathExpression pathExpression = ParsePathAlternative(parserContext);

            //If the whole path is one plain predicate (no inverse, exactly-one cardinality), treat it as a simple
            //IRI predicate so a plain triple stays a triple
            if (pathExpression.IsPlainLink)
                return ParsedVerb.FromSimpleIri(pathExpression.Property);

            //Genuine property path
            return ParsedVerb.FromPath(pathExpression);
        }

        /// <summary>
        /// <para>
        /// Parses the object list following a property-path verb (<c>ObjectList ::= Object ( ',' Object )*</c>)
        /// and emits one <see cref="RDFPropertyPath"/> per object into <paramref name="targetPatternGroup"/>.
        /// Every comma-separated object shares the same start (the subject) and the same parsed path expression;
        /// each object becomes the End of a freshly built property path carrying that single expression as its unit.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When an object is malformed or invalid as a path endpoint.</exception>
        private static void ParsePathObjectList(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup,
            RDFPatternMember pathStart, RDFPropertyPathExpression pathExpression)
        {
            while (true)
            {
                //Parse the object term standing for the End of the path (variable or concrete term)
                RDFPatternMember pathEnd = ParseVariableOrTerm(parserContext, targetPatternGroup);

                //Build the property path between start and end, carrying the parsed expression as its single unit.
                //The RDFPropertyPath constructor enforces that both endpoints are an IRI or a variable.
                RDFPropertyPath propertyPath = new RDFPropertyPath(pathStart, pathEnd);
                propertyPath.AddSequenceStep(pathExpression);
                targetPatternGroup.AddPropertyPath(propertyPath);

                //A ',' introduces another object sharing this subject+path: consume and continue
                if (SkipWhitespace(parserContext) == ',')
                {
                    ReadCodePoint(parserContext);
                    continue;
                }

                //No ',': the object list is complete
                return;
            }
        }
        #endregion

        #region Grammar (W3C SPARQL 1.1 path productions)
        /// <summary>
        /// <c>[89] PathAlternative ::= PathSequence ( '|' PathSequence )*</c>. Lowest-precedence path operator.
        /// </summary>
        private static RDFPropertyPathExpression ParsePathAlternative(RDFQueryParserContext parserContext)
        {
            List<RDFPropertyPathExpression> alternativeBranches = new List<RDFPropertyPathExpression> { ParsePathSequence(parserContext) };

            //Collect every '|'-separated sequence into the alternative list
            while (SkipWhitespace(parserContext) == '|')
            {
                ReadCodePoint(parserContext);
                alternativeBranches.Add(ParsePathSequence(parserContext));
            }

            //A single branch is just that sequence; two or more become an alternative node
            return alternativeBranches.Count == 1
                ? alternativeBranches[0]
                : RDFPropertyPathExpression.Alternative(alternativeBranches);
        }

        /// <summary>
        /// <c>[90] PathSequence ::= PathEltOrInverse ( '/' PathEltOrInverse )*</c>. Binds tighter than '|'.
        /// </summary>
        private static RDFPropertyPathExpression ParsePathSequence(RDFQueryParserContext parserContext)
        {
            List<RDFPropertyPathExpression> sequenceElements = new List<RDFPropertyPathExpression> { ParsePathEltOrInverse(parserContext) };

            //Collect every '/'-separated element into the sequence list
            while (SkipWhitespace(parserContext) == '/')
            {
                ReadCodePoint(parserContext);
                sequenceElements.Add(ParsePathEltOrInverse(parserContext));
            }

            //A single element is just that element; two or more become a sequence node
            return sequenceElements.Count == 1
                ? sequenceElements[0]
                : RDFPropertyPathExpression.Sequence(sequenceElements);
        }

        /// <summary>
        /// <c>[92] PathEltOrInverse ::= PathElt | '^' PathElt</c>. A leading '^' marks the element as inverse.
        /// </summary>
        private static RDFPropertyPathExpression ParsePathEltOrInverse(RDFQueryParserContext parserContext)
        {
            //A leading '^' inverts the following element
            if (SkipWhitespace(parserContext) == '^')
            {
                ReadCodePoint(parserContext);
                return ParsePathElt(parserContext).Inverse();
            }

            return ParsePathElt(parserContext);
        }

        /// <summary>
        /// <c>[91] PathElt ::= PathPrimary PathMod?</c>. Parses the primary then an optional cardinality modifier.
        /// </summary>
        private static RDFPropertyPathExpression ParsePathElt(RDFQueryParserContext parserContext)
        {
            RDFPropertyPathExpression pathPrimary = ParsePathPrimary(parserContext);

            //Optional cardinality modifier '?' / '*' / '+' (W3C PathMod — note: NO '{n,m}'). The modifier must
            //immediately follow the element with NO intervening whitespace: this is what disambiguates a postfix
            //'?' (ZeroOrOne) from the '?' that opens a variable object (e.g. ':p?' vs ':p ?o'). So peek the very
            //next code point WITHOUT skipping whitespace.
            int modifierCodePoint = PeekCodePoint(parserContext);
            switch (modifierCodePoint)
            {
                case '?':
                    ReadCodePoint(parserContext);
                    return pathPrimary.ZeroOrOne();
                case '*':
                    ReadCodePoint(parserContext);
                    return pathPrimary.ZeroOrMore();
                case '+':
                    ReadCodePoint(parserContext);
                    return pathPrimary.OneOrMore();
                default:
                    //No modifier: ExactlyOne (the default already on the node)
                    return pathPrimary;
            }
        }

        /// <summary>
        /// <c>[94] PathPrimary ::= iri | 'a' | '!' PathNegatedPropertySet | '(' Path ')'</c>. A parenthesized
        /// sub-path becomes the inner expression (grouping is implicit in the tree); a negated property set
        /// becomes a NegatedPropertySet node.
        /// </summary>
        private static RDFPropertyPathExpression ParsePathPrimary(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //'(' Path ')' — a parenthesized sub-path: parse recursively and return the inner expression
            if (nextSignificantCodePoint == '(')
            {
                ReadCodePoint(parserContext);
                RDFPropertyPathExpression innerPath = ParsePathAlternative(parserContext);
                ExpectChar(parserContext, ')', "property path group");
                return innerPath;
            }

            //'!' PathNegatedPropertySet — one hop over any predicate not in the given set
            if (nextSignificantCodePoint == '!')
            {
                ReadCodePoint(parserContext);
                return ParsePathNegatedPropertySet(parserContext);
            }

            //'a' shorthand for rdf:type — recognized only when it stands alone (not the head of a longer prefixed
            //name such as 'a:Foo' or 'abc:bar'). In a path the token after a standalone 'a' is a path operator,
            //a boundary, or whitespace; if it is a prefixed-name continuation char, fall through to the term reader.
            if (nextSignificantCodePoint == 'a')
            {
                ReadCodePoint(parserContext);
                int codePointAfterA = PeekCodePoint(parserContext);
                if (!IsPrefixedNameContinuation(codePointAfterA))
                    return RDFPropertyPathExpression.Link(new RDFResource(RDFVocabulary.RDF.TYPE.ToString()));

                //'a' was the first character of a longer token: push it back and let the term reader parse it whole
                UnreadCodePoint(parserContext, 'a');
            }

            //General case: a concrete IRI / prefixed name. A property-path step must be an IRI (resource), so a
            //literal here is a parse error.
            return RDFPropertyPathExpression.Link(ReadPathPropertyIri(parserContext));
        }

        /// <summary>
        /// <c>[95]/[96]</c> PathNegatedPropertySet: either a single one-in-property-set member, or a parenthesized
        /// (possibly empty) '|'-separated list of members. Each member is <c>iri | 'a' | '^' (iri | 'a')</c>.
        /// </summary>
        private static RDFPropertyPathExpression ParsePathNegatedPropertySet(RDFQueryParserContext parserContext)
        {
            List<(RDFResource Property, bool IsInverse)> members = new List<(RDFResource, bool)>();

            //Parenthesized list: zero or more '|'-separated members
            if (SkipWhitespace(parserContext) == '(')
            {
                ReadCodePoint(parserContext);
                if (SkipWhitespace(parserContext) != ')')
                {
                    members.Add(ParsePathOneInPropertySet(parserContext));
                    while (SkipWhitespace(parserContext) == '|')
                    {
                        ReadCodePoint(parserContext);
                        members.Add(ParsePathOneInPropertySet(parserContext));
                    }
                }
                ExpectChar(parserContext, ')', "property path negated set");
            }
            //Single bare member
            else
            {
                members.Add(ParsePathOneInPropertySet(parserContext));
            }

            return RDFPropertyPathExpression.NegatedPropertySet(members);
        }

        /// <summary>
        /// <c>[96] PathOneInPropertySet ::= iri | 'a' | '^' ( iri | 'a' )</c>. Returns the predicate plus a flag
        /// telling whether the member is matched in the inverse direction.
        /// </summary>
        private static (RDFResource Property, bool IsInverse) ParsePathOneInPropertySet(RDFQueryParserContext parserContext)
        {
            bool isInverse = false;
            if (SkipWhitespace(parserContext) == '^')
            {
                ReadCodePoint(parserContext);
                isInverse = true;
            }

            //'a' shorthand for rdf:type, recognized only when standing alone
            if (SkipWhitespace(parserContext) == 'a')
            {
                ReadCodePoint(parserContext);
                int codePointAfterA = PeekCodePoint(parserContext);
                if (!IsPrefixedNameContinuation(codePointAfterA))
                    return (new RDFResource(RDFVocabulary.RDF.TYPE.ToString()), isInverse);
                UnreadCodePoint(parserContext, 'a');
            }

            return (ReadPathPropertyIri(parserContext), isInverse);
        }

        /// <summary>
        /// Reads a property-path step predicate (a concrete IRI / prefixed name). A literal here is a parse error.
        /// </summary>
        /// <exception cref="RDFQueryException">When the term is not an IRI.</exception>
        private static RDFResource ReadPathPropertyIri(RDFQueryParserContext parserContext)
        {
            RDFPatternMember parsedTerm = ParseTerm(parserContext);
            if (!(parsedTerm is RDFResource stepResource))
                throw new RDFQueryException("Cannot parse SPARQL property path: a step must be an IRI " + GetCoordinates(parserContext));
            return stepResource;
        }

        /// <summary>
        /// Tells whether <paramref name="codePoint"/> can continue a prefixed name, used to disambiguate the 'a'
        /// (rdf:type) shorthand from a longer token like 'a:Foo'/'abc:bar'. A conservative set is enough here:
        /// ASCII letters, digits, and the prefixed-name punctuation ':' '_' '-' '.' '%'.
        /// </summary>
        private static bool IsPrefixedNameContinuation(int codePoint)
            => IsAsciiLetter(codePoint)
                || (codePoint >= '0' && codePoint <= '9')
                || codePoint == ':' || codePoint == '_' || codePoint == '-' || codePoint == '.' || codePoint == '%';
        #endregion

        #region Intermediate types
        /// <summary>
        /// Outcome of parsing a triples-block verb: a variable predicate, a simple IRI predicate, or a property path.
        /// </summary>
        private sealed class ParsedVerb
        {
            internal RDFVariable Variable { get; private set; }
            internal RDFResource SimpleIri { get; private set; }
            internal RDFPropertyPathExpression PathExpression { get; private set; }

            internal bool IsVariable => Variable != null;
            internal bool IsSimpleIri => SimpleIri != null;

            internal static ParsedVerb FromVariable(RDFVariable variable) => new ParsedVerb { Variable = variable };
            internal static ParsedVerb FromSimpleIri(RDFResource simpleIri) => new ParsedVerb { SimpleIri = simpleIri };
            internal static ParsedVerb FromPath(RDFPropertyPathExpression pathExpression) => new ParsedVerb { PathExpression = pathExpression };
        }
        #endregion

        #endregion
    }
}
