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
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// <para>
    /// Property-path half of the SPARQL parser: the verb of a triples block can be a full SPARQL 1.1
    /// <c>Path</c> expression rather than a single predicate. This file parses the W3C SPARQL 1.1 path
    /// grammar and lowers it onto RDFSharp's <see cref="RDFPropertyPath"/> model.
    /// </para>
    /// <para>
    /// SPARQL 1.1 grammar (W3C, §19.8 — the SINGLE source of truth here, NOT the printer):
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
    /// Note that the W3C grammar has NO <c>{n,m}</c> cardinality on paths — <c>PathMod</c> is exactly
    /// <c>'?' | '*' | '+'</c>. RDFSharp's model/printer expose a non-standard BoundedRange extension, which
    /// the parser deliberately does NOT accept (it is not valid SPARQL 1.1).
    /// </para>
    /// <para>
    /// MODEL MAPPING. <see cref="RDFPropertyPath"/> stores a FLAT list of <see cref="RDFPropertyPathStep"/>:
    /// a step is a single predicate IRI carrying an optional inverse flag and an optional per-step cardinality
    /// (<c>? * +</c>), tagged either Sequence (<c>/</c>) or Alternative (<c>|</c>). The representable language
    /// is therefore exactly: a concatenation of "units", where each unit is either a single step or an
    /// alternative-run of single steps. Spec-legal shapes that exceed this expressiveness are rejected with a
    /// descriptive <see cref="RDFQueryException"/> (see the known-limits memory):
    /// <list type="bullet">
    /// <item>negated property sets (<c>!iri</c>, <c>!(…)</c>);</item>
    /// <item>a cardinality modifier applied to a parenthesized group (<c>(a/b)+</c>, <c>(a|b)*</c>);</item>
    /// <item>the inverse of a parenthesized group (<c>^(a|b)</c>);</item>
    /// <item>a sequence used as a branch of an alternative (<c>(a/b)|c</c>, <c>a/b|c/d</c>).</item>
    /// </list>
    /// Redundant/flattenable grouping (a group wrapping a pure sequence, or a group wrapping a pure alternative
    /// used as a sequence member) IS accepted and flattened, because it lowers losslessly onto the flat model.
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
        /// A path that lowers to a single bare predicate step (no inverse, no cardinality) is reported as a SIMPLE
        /// IRI predicate, not as a path: <c>?s :p ?o</c> must stay a plain <see cref="RDFPattern"/> (so plain
        /// triples keep round-tripping as triples) while <c>?s :p+ ?o</c> / <c>?s ^:p ?o</c> / <c>?s :p/:q ?o</c>
        /// become an <see cref="RDFPropertyPath"/>.
        /// </para>
        /// </summary>
        private static ParsedVerb ParseVerb(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //A '?' or '$' sigil is the unambiguous start of a variable predicate (VerbSimple)
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParsedVerb.FromVariable(ParseVariable(parserContext));

            //Otherwise the verb is a property path (VerbPath): parse the W3C path grammar into an expression
            //tree, then lower it onto the flat RDFPropertyPath step model.
            PathExpression pathExpression = ParsePathAlternative(parserContext);
            List<ParsedPathUnit> pathUnits = LowerPathExpression(parserContext, pathExpression);

            //If the whole path reduced to one plain predicate step (no inverse, exactly-one cardinality), treat
            //it as a simple IRI predicate so a plain triple stays a triple.
            if (pathUnits.Count == 1
                 && !pathUnits[0].IsAlternative
                 && pathUnits[0].Steps.Count == 1
                 && !pathUnits[0].Steps[0].IsInverse
                 && pathUnits[0].Steps[0].Cardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne)
                return ParsedVerb.FromSimpleIri(pathUnits[0].Steps[0].StepProperty);

            //Genuine property path
            return ParsedVerb.FromPath(pathUnits);
        }

        /// <summary>
        /// <para>
        /// Parses the object list following a property-path verb (<c>ObjectList ::= Object ( ',' Object )*</c>)
        /// and emits one <see cref="RDFPropertyPath"/> per object into <paramref name="targetPatternGroup"/>.
        /// Every comma-separated object shares the same start (the subject) and the same parsed path units; each
        /// object becomes the End of a freshly built property path so the parsed steps are replayed cleanly.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When an object is malformed or invalid as a path endpoint.</exception>
        private static void ParsePathObjectList(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup,
            RDFPatternMember pathStart, List<ParsedPathUnit> pathUnits)
        {
            while (true)
            {
                //Parse the object term standing for the End of the path (variable or concrete term)
                RDFPatternMember pathEnd = ParseVariableOrTerm(parserContext, targetPatternGroup);

                //Build the property path between start and end, replaying the parsed units as model steps. The
                //RDFPropertyPath constructor enforces that both endpoints are an IRI or a variable (a literal
                //endpoint, for instance, is rejected) — surfaced as RDFQueryException.
                RDFPropertyPath propertyPath = new RDFPropertyPath(pathStart, pathEnd);
                ApplyPathUnits(propertyPath, pathUnits);
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

        /// <summary>
        /// Replays the parsed, already-validated <paramref name="pathUnits"/> as model steps on
        /// <paramref name="propertyPath"/>, creating fresh <see cref="RDFPropertyPathStep"/> instances so the same
        /// units can be applied to multiple property paths (one per comma-separated object).
        /// </summary>
        private static void ApplyPathUnits(RDFPropertyPath propertyPath, List<ParsedPathUnit> pathUnits)
        {
            foreach (ParsedPathUnit pathUnit in pathUnits)
            {
                if (pathUnit.IsAlternative)
                    //Alternative-run unit: emit all its steps together via AddAlternativeSteps (OR semantics)
                    propertyPath.AddAlternativeSteps(pathUnit.Steps.Select(BuildModelStep).ToList());
                else
                    //Single-step unit: emit one sequence step (AND semantics)
                    propertyPath.AddSequenceStep(BuildModelStep(pathUnit.Steps[0]));
            }
        }

        /// <summary>
        /// Materializes one parsed step descriptor into a model <see cref="RDFPropertyPathStep"/>, applying the
        /// inverse flag and the per-step cardinality (<c>? * +</c>). The Sequence/Alternative flavor is NOT set
        /// here — it is decided by whether the caller uses AddSequenceStep or AddAlternativeSteps.
        /// </summary>
        private static RDFPropertyPathStep BuildModelStep(ParsedStep parsedStep)
        {
            RDFPropertyPathStep modelStep = new RDFPropertyPathStep(parsedStep.StepProperty);
            if (parsedStep.IsInverse)
                modelStep.Inverse();
            switch (parsedStep.Cardinality)
            {
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne:  modelStep.ZeroOrOne();  break;
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore:  modelStep.OneOrMore();  break;
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore: modelStep.ZeroOrMore(); break;
                //ExactlyOne is the constructor default — nothing to do
            }
            return modelStep;
        }
        #endregion

        #region Grammar (W3C SPARQL 1.1 path productions)
        /// <summary>
        /// <c>[89] PathAlternative ::= PathSequence ( '|' PathSequence )*</c>. Lowest-precedence path operator.
        /// </summary>
        private static PathExpression ParsePathAlternative(RDFQueryParserContext parserContext)
        {
            List<PathExpression> alternativeBranches = new List<PathExpression> { ParsePathSequence(parserContext) };

            //Collect every '|'-separated sequence into the alternative list
            while (SkipWhitespace(parserContext) == '|')
            {
                ReadCodePoint(parserContext);
                alternativeBranches.Add(ParsePathSequence(parserContext));
            }

            //A single branch is just that sequence; two or more become an alternative node
            return alternativeBranches.Count == 1
                ? alternativeBranches[0]
                : new PathExpression { Kind = PathExpressionKind.Alternative, Children = alternativeBranches };
        }

        /// <summary>
        /// <c>[90] PathSequence ::= PathEltOrInverse ( '/' PathEltOrInverse )*</c>. Binds tighter than '|'.
        /// </summary>
        private static PathExpression ParsePathSequence(RDFQueryParserContext parserContext)
        {
            List<PathExpression> sequenceElements = new List<PathExpression> { ParsePathEltOrInverse(parserContext) };

            //Collect every '/'-separated element into the sequence list
            while (SkipWhitespace(parserContext) == '/')
            {
                ReadCodePoint(parserContext);
                sequenceElements.Add(ParsePathEltOrInverse(parserContext));
            }

            //A single element is just that element; two or more become a sequence node
            return sequenceElements.Count == 1
                ? sequenceElements[0]
                : new PathExpression { Kind = PathExpressionKind.Sequence, Children = sequenceElements };
        }

        /// <summary>
        /// <c>[92] PathEltOrInverse ::= PathElt | '^' PathElt</c>. A leading '^' marks the element as inverse.
        /// </summary>
        private static PathExpression ParsePathEltOrInverse(RDFQueryParserContext parserContext)
        {
            //A leading '^' inverts the following element
            bool isInverse = false;
            if (SkipWhitespace(parserContext) == '^')
            {
                ReadCodePoint(parserContext);
                isInverse = true;
            }

            PathExpression pathElement = ParsePathElt(parserContext);
            if (isInverse)
                pathElement.IsInverse = true;
            return pathElement;
        }

        /// <summary>
        /// <c>[91] PathElt ::= PathPrimary PathMod?</c>. Parses the primary then an optional cardinality modifier.
        /// </summary>
        private static PathExpression ParsePathElt(RDFQueryParserContext parserContext)
        {
            PathExpression pathPrimary = ParsePathPrimary(parserContext);

            //Optional cardinality modifier '?' / '*' / '+' (W3C PathMod — note: NO '{n,m}'). The modifier must
            //immediately follow the element with NO intervening whitespace: this is what disambiguates a postfix
            //'?' (ZeroOrOne) from the '?' that opens a variable object (e.g. ':p?' vs ':p ?o'). So peek the very
            //next code point WITHOUT skipping whitespace.
            int modifierCodePoint = PeekCodePoint(parserContext);
            switch (modifierCodePoint)
            {
                case '?':
                    ReadCodePoint(parserContext);
                    pathPrimary.Cardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne;
                    break;
                case '*':
                    ReadCodePoint(parserContext);
                    pathPrimary.Cardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore;
                    break;
                case '+':
                    ReadCodePoint(parserContext);
                    pathPrimary.Cardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore;
                    break;
                //No modifier: ExactlyOne (the default already on the node)
            }

            return pathPrimary;
        }

        /// <summary>
        /// <c>[94] PathPrimary ::= iri | 'a' | '!' PathNegatedPropertySet | '(' Path ')'</c>. The negated property
        /// set is parsed-and-rejected (model cannot represent it); a parenthesized sub-path becomes a Group node.
        /// </summary>
        private static PathExpression ParsePathPrimary(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //'(' Path ')' — a parenthesized sub-path: parse recursively and wrap as a Group node
            if (nextSignificantCodePoint == '(')
            {
                ReadCodePoint(parserContext);
                PathExpression innerPath = ParsePathAlternative(parserContext);
                ExpectChar(parserContext, ')', "property path group");
                return new PathExpression { Kind = PathExpressionKind.Group, Children = new List<PathExpression> { innerPath } };
            }

            //'!' PathNegatedPropertySet — not representable by the RDFPropertyPath model: reject explicitly
            if (nextSignificantCodePoint == '!')
                throw new RDFQueryException("Cannot parse SPARQL property path: negated property sets ('!') are not supported " + GetCoordinates(parserContext));

            //'a' shorthand for rdf:type — recognized only when it stands alone (not the head of a longer prefixed
            //name such as 'a:Foo' or 'abc:bar'). In a path the token after a standalone 'a' is a path operator,
            //a boundary, or whitespace; if it is a prefixed-name continuation char, fall through to the term reader.
            if (nextSignificantCodePoint == 'a')
            {
                ReadCodePoint(parserContext);
                int codePointAfterA = PeekCodePoint(parserContext);
                if (!IsPrefixedNameContinuation(codePointAfterA))
                    return PathExpression.Step(new RDFResource(RDFVocabulary.RDF.TYPE.ToString()));

                //'a' was the first character of a longer token: push it back and let the term reader parse it whole
                UnreadCodePoint(parserContext, 'a');
            }

            //General case: a concrete IRI / prefixed name. A property-path step must be an IRI (resource), so a
            //literal here is a parse error.
            RDFPatternMember parsedTerm = ParseTerm(parserContext);
            if (!(parsedTerm is RDFResource stepResource))
                throw new RDFQueryException("Cannot parse SPARQL property path: a step must be an IRI " + GetCoordinates(parserContext));
            return PathExpression.Step(stepResource);
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

        #region Lowering (path expression tree -> flat model units)
        /// <summary>
        /// Lowers a parsed path expression tree into the flat list of <see cref="ParsedPathUnit"/> that the
        /// <see cref="RDFPropertyPath"/> model can hold (a concatenation of single-step / alternative-run units),
        /// throwing for spec-legal shapes the model cannot represent.
        /// </summary>
        private static List<ParsedPathUnit> LowerPathExpression(RDFQueryParserContext parserContext, PathExpression pathExpression)
        {
            switch (pathExpression.Kind)
            {
                //A bare step is a single sequence unit
                case PathExpressionKind.Step:
                    return new List<ParsedPathUnit> { ParsedPathUnit.SequenceUnit(ToParsedStep(pathExpression)) };

                //A top-level sequence: each member lowers to one (or more, when a group flattens) units
                case PathExpressionKind.Sequence:
                {
                    List<ParsedPathUnit> units = new List<ParsedPathUnit>();
                    foreach (PathExpression sequenceMember in pathExpression.Children)
                        units.AddRange(LowerSequenceMember(parserContext, sequenceMember));
                    return units;
                }

                //A top-level alternative: every branch must reduce to plain steps (no sequences), OR-combined into
                //a single alternative unit
                case PathExpressionKind.Alternative:
                {
                    List<ParsedStep> alternativeSteps = new List<ParsedStep>();
                    foreach (PathExpression alternativeBranch in pathExpression.Children)
                        alternativeSteps.AddRange(CollectAlternativeSteps(parserContext, alternativeBranch));
                    return new List<ParsedPathUnit> { ParsedPathUnit.AlternativeUnit(alternativeSteps) };
                }

                //A top-level group: a modifier/inverse on a group is not representable; otherwise flatten it
                case PathExpressionKind.Group:
                    EnsureGroupHasNoModifierOrInverse(parserContext, pathExpression);
                    return LowerPathExpression(parserContext, pathExpression.Children[0]);

                default:
                    throw new RDFQueryException("Cannot parse SPARQL property path: unexpected path expression " + GetCoordinates(parserContext));
            }
        }

        /// <summary>
        /// Lowers one member of a top-level sequence into units. A plain step → one sequence unit; an alternative
        /// of plain steps → one alternative unit; a group (no modifier/inverse) is flattened into the sequence; a
        /// nested sequence (only reachable through a flattened group) is spliced in.
        /// </summary>
        private static List<ParsedPathUnit> LowerSequenceMember(RDFQueryParserContext parserContext, PathExpression sequenceMember)
        {
            switch (sequenceMember.Kind)
            {
                case PathExpressionKind.Step:
                    return new List<ParsedPathUnit> { ParsedPathUnit.SequenceUnit(ToParsedStep(sequenceMember)) };

                case PathExpressionKind.Alternative:
                {
                    List<ParsedStep> alternativeSteps = new List<ParsedStep>();
                    foreach (PathExpression alternativeBranch in sequenceMember.Children)
                        alternativeSteps.AddRange(CollectAlternativeSteps(parserContext, alternativeBranch));
                    return new List<ParsedPathUnit> { ParsedPathUnit.AlternativeUnit(alternativeSteps) };
                }

                case PathExpressionKind.Group:
                    EnsureGroupHasNoModifierOrInverse(parserContext, sequenceMember);
                    return LowerPathExpression(parserContext, sequenceMember.Children[0]);

                case PathExpressionKind.Sequence:
                {
                    List<ParsedPathUnit> units = new List<ParsedPathUnit>();
                    foreach (PathExpression nestedMember in sequenceMember.Children)
                        units.AddRange(LowerSequenceMember(parserContext, nestedMember));
                    return units;
                }

                default:
                    throw new RDFQueryException("Cannot parse SPARQL property path: unexpected sequence member " + GetCoordinates(parserContext));
            }
        }

        /// <summary>
        /// Collects the plain steps OR-combined by a branch of an alternative. A branch must reduce to one or more
        /// plain steps: a sequence used as an alternative branch (e.g. <c>(a/b)|c</c>) is NOT representable by the
        /// flat model and is rejected.
        /// </summary>
        private static List<ParsedStep> CollectAlternativeSteps(RDFQueryParserContext parserContext, PathExpression alternativeBranch)
        {
            switch (alternativeBranch.Kind)
            {
                case PathExpressionKind.Step:
                    return new List<ParsedStep> { ToParsedStep(alternativeBranch) };

                //Nested alternative: flatten its branches into the same alternative run
                case PathExpressionKind.Alternative:
                {
                    List<ParsedStep> alternativeSteps = new List<ParsedStep>();
                    foreach (PathExpression nestedBranch in alternativeBranch.Children)
                        alternativeSteps.AddRange(CollectAlternativeSteps(parserContext, nestedBranch));
                    return alternativeSteps;
                }

                //A group with no modifier/inverse just wraps another alternative/step: unwrap it
                case PathExpressionKind.Group:
                    EnsureGroupHasNoModifierOrInverse(parserContext, alternativeBranch);
                    return CollectAlternativeSteps(parserContext, alternativeBranch.Children[0]);

                //A sequence cannot be a branch of an alternative in the flat model
                case PathExpressionKind.Sequence:
                    throw new RDFQueryException("Cannot parse SPARQL property path: a sequence cannot be a branch of an alternative ('(a/b)|c') " + GetCoordinates(parserContext));

                default:
                    throw new RDFQueryException("Cannot parse SPARQL property path: unexpected alternative branch " + GetCoordinates(parserContext));
            }
        }

        /// <summary>
        /// Guards that a Group node carries neither a cardinality modifier nor an inverse flag, since the flat
        /// model cannot apply either to a whole sub-path (only to individual steps).
        /// </summary>
        private static void EnsureGroupHasNoModifierOrInverse(RDFQueryParserContext parserContext, PathExpression groupExpression)
        {
            if (groupExpression.Cardinality != RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne)
                throw new RDFQueryException("Cannot parse SPARQL property path: a cardinality modifier on a group ('(a/b)+') is not supported " + GetCoordinates(parserContext));
            if (groupExpression.IsInverse)
                throw new RDFQueryException("Cannot parse SPARQL property path: the inverse of a group ('^(a|b)') is not supported " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Converts a leaf Step expression into the parsed-step descriptor (property + inverse + cardinality).
        /// </summary>
        private static ParsedStep ToParsedStep(PathExpression stepExpression)
            => new ParsedStep(stepExpression.StepProperty, stepExpression.IsInverse, stepExpression.Cardinality);
        #endregion

        #region Intermediate types
        /// <summary>
        /// Outcome of parsing a triples-block verb: a variable predicate, a simple IRI predicate, or a property path.
        /// </summary>
        private sealed class ParsedVerb
        {
            internal RDFVariable Variable { get; private set; }
            internal RDFResource SimpleIri { get; private set; }
            internal List<ParsedPathUnit> PathUnits { get; private set; }

            internal bool IsVariable => Variable != null;
            internal bool IsSimpleIri => SimpleIri != null;
            internal bool IsPath => PathUnits != null;

            internal static ParsedVerb FromVariable(RDFVariable variable) => new ParsedVerb { Variable = variable };
            internal static ParsedVerb FromSimpleIri(RDFResource simpleIri) => new ParsedVerb { SimpleIri = simpleIri };
            internal static ParsedVerb FromPath(List<ParsedPathUnit> pathUnits) => new ParsedVerb { PathUnits = pathUnits };
        }

        /// <summary>
        /// A lowered unit of a property path: either a single sequence step (<see cref="IsAlternative"/> false,
        /// one step) or an alternative-run of steps (<see cref="IsAlternative"/> true, two or more steps).
        /// </summary>
        private sealed class ParsedPathUnit
        {
            internal bool IsAlternative { get; private set; }
            internal List<ParsedStep> Steps { get; private set; }

            internal static ParsedPathUnit SequenceUnit(ParsedStep step)
                => new ParsedPathUnit { IsAlternative = false, Steps = new List<ParsedStep> { step } };
            internal static ParsedPathUnit AlternativeUnit(List<ParsedStep> steps)
                => new ParsedPathUnit { IsAlternative = true, Steps = steps };
        }

        /// <summary>
        /// Immutable descriptor of a single path step (predicate IRI + inverse flag + per-step cardinality),
        /// replayable into a fresh <see cref="RDFPropertyPathStep"/> for each property path it feeds.
        /// </summary>
        private readonly struct ParsedStep
        {
            internal RDFResource StepProperty { get; }
            internal bool IsInverse { get; }
            internal RDFQueryEnums.RDFPropertyPathStepCardinalities Cardinality { get; }

            internal ParsedStep(RDFResource stepProperty, bool isInverse, RDFQueryEnums.RDFPropertyPathStepCardinalities cardinality)
            {
                StepProperty = stepProperty;
                IsInverse = isInverse;
                Cardinality = cardinality;
            }
        }

        /// <summary>
        /// Node kinds of the parsed path expression tree (mirrors the W3C path productions).
        /// </summary>
        private enum PathExpressionKind { Step, Sequence, Alternative, Group }

        /// <summary>
        /// A node of the parsed path expression tree. A Step leaf carries its predicate IRI; Sequence/Alternative/
        /// Group carry their children. The inverse flag and cardinality decorate any node (the lowering pass
        /// enforces that they only survive on Step nodes, rejecting them on Group nodes).
        /// </summary>
        private sealed class PathExpression
        {
            internal PathExpressionKind Kind { get; set; }
            internal List<PathExpression> Children { get; set; }
            internal RDFResource StepProperty { get; set; }
            internal bool IsInverse { get; set; }
            internal RDFQueryEnums.RDFPropertyPathStepCardinalities Cardinality { get; set; }
                = RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne;

            internal static PathExpression Step(RDFResource stepProperty)
                => new PathExpression { Kind = PathExpressionKind.Step, StepProperty = stepProperty };
        }
        #endregion

        #endregion
    }
}
