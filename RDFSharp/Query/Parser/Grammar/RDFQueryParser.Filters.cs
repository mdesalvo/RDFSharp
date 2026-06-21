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
    /// FILTER + expression half of the SPARQL parser (phase F6). This partial holds everything needed to turn a
    /// SPARQL <c>Constraint</c> (the body of a <c>FILTER</c>) into the engine's filter/expression object model.
    /// </para>
    /// <para>
    /// TWO COOPERATING GRAMMARS. RDFSharp models FILTER logic with a SPLIT representation, and this parser mirrors
    /// that split with two cooperating layers:
    /// <list type="bullet">
    /// <item><b>Filter skeleton</b> (produces <see cref="RDFFilter"/>) — owns the boolean connectives that have a
    ///   FILTER-level form: <c>||</c> → <see cref="RDFBooleanOrFilter"/>, <c>&amp;&amp;</c> →
    ///   <see cref="RDFBooleanAndFilter"/>, <c>!</c> → <see cref="RDFBooleanNotFilter"/>, plus <c>EXISTS</c> /
    ///   <c>NOT EXISTS</c> → <see cref="RDFExistsFilter"/> / <see cref="RDFNotExistsFilter"/>. Its leaves are
    ///   value-expressions wrapped in <see cref="RDFExpressionFilter"/>.</item>
    /// <item><b>Expression grammar</b> (produces <see cref="RDFExpression"/>) — the SPARQL operator-precedence ladder
    ///   <c>||</c> &gt; <c>&amp;&amp;</c> &gt; relational &gt; additive &gt; multiplicative &gt; unary &gt; primary.
    ///   Here <c>||</c>/<c>&amp;&amp;</c> become <see cref="RDFBooleanOrExpression"/>/<see cref="RDFBooleanAndExpression"/>,
    ///   comparisons become <see cref="RDFComparisonExpression"/>, arithmetic becomes the math expressions, and the
    ///   leaves dispatch onto the ~50 SPARQL 1.1 built-in expression classes.</item>
    /// </list>
    /// </para>
    /// <para>
    /// WHERE THE TWO MEET. A parenthesis switches worlds: at the FILTER skeleton the connectives are filters, but as
    /// soon as a <c>(</c> opens, the content is parsed by the expression grammar (so <c>(?a &amp;&amp; ?b)</c> is a
    /// boolean expression, while <c>(?x+1) &gt; 2</c> is a comparison whose left side is a bracketed arithmetic
    /// expression). The skeleton therefore enters the expression grammar at the RELATIONAL level (a single
    /// <c>ValueLogical</c>), never at <c>||</c>/<c>&amp;&amp;</c>, so top-level connectives stay with the skeleton
    /// and never get consumed twice.
    /// </para>
    /// <para>
    /// MODEL LIMITATIONS (intentional, surfaced as precise RDFQueryException). The object model has no Boolean-NOT
    /// expression and no way to embed EXISTS inside an expression, so <c>!</c> and <c>EXISTS</c> only live at the
    /// FILTER skeleton; using them deep inside a value-expression is rejected. <see cref="RDFExistsFilter"/> carries a
    /// full group graph pattern (a <see cref="RDFPatternGroup"/> or a <see cref="RDFSelectQuery"/> SubSelect), so EXISTS
    /// over multi-triple / UNION / OPTIONAL groups is supported.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region Filter
        /// <summary>
        /// Parses a SPARQL <c>FILTER</c> clause (the <c>FILTER</c> keyword has already been consumed by the caller)
        /// and attaches the resulting <see cref="RDFFilter"/> to <paramref name="targetPatternGroup"/>.
        /// <para>SPARQL grammar: <c>Filter ::= 'FILTER' Constraint</c>; <c>Constraint ::= BrackettedExpression | BuiltInCall | FunctionCall</c>.</para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the constraint is malformed or uses an unsupported construct.</exception>
        private static void ParseFilter(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup)
            => targetPatternGroup.AddFilter(ParseConstraint(parserContext));

        /// <summary>
        /// Parses a single FILTER <c>Constraint</c> into the <see cref="RDFFilter"/> it denotes.
        /// <list type="bullet">
        /// <item>A <c>(</c> opens a <c>BrackettedExpression</c>: the parenthesised content is the full boolean
        ///   skeleton (so <c>||</c>/<c>&amp;&amp;</c>/<c>!</c> nest as filters there).</item>
        /// <item>Otherwise the constraint is a bare <c>BuiltInCall</c> or <c>FunctionCall</c> — including the
        ///   <c>EXISTS</c> / <c>NOT EXISTS</c> built-ins — which is parsed and (when it is a value-expression)
        ///   wrapped in an <see cref="RDFExpressionFilter"/>.</item>
        /// </list>
        /// </summary>
        private static RDFFilter ParseConstraint(RDFQueryParserContext parserContext)
        {
            //A '(' opens a BrackettedExpression: parse the whole boolean skeleton between the parentheses
            if (SkipWhitespace(parserContext) == '(')
            {
                ExpectChar(parserContext, '(', "FILTER constraint");
                RDFFilter bracketedFilter = ParseFilterOr(parserContext);
                ExpectChar(parserContext, ')', "FILTER constraint");
                return bracketedFilter;
            }

            //A bare constraint is a BuiltInCall or FunctionCall: EXISTS / NOT EXISTS are handled as filters, every
            //other built-in/function is a value-expression that must evaluate to boolean to act as a constraint
            return ParseFilterAtom(parserContext);
        }

        /// <summary>
        /// Filter skeleton — <c>ConditionalOrExpression</c> level. Folds <c>||</c>-separated operands left-to-right
        /// into <see cref="RDFBooleanOrFilter"/> nodes (<c>A || B || C → Or(Or(A,B),C)</c>).
        /// </summary>
        private static RDFFilter ParseFilterOr(RDFQueryParserContext parserContext)
        {
            RDFFilter leftFilter = ParseFilterAnd(parserContext);
            while (TryConsumeOperator(parserContext, "||"))
                leftFilter = new RDFBooleanOrFilter(leftFilter, ParseFilterAnd(parserContext));
            return leftFilter;
        }

        /// <summary>
        /// Filter skeleton — <c>ConditionalAndExpression</c> level. Folds <c>&amp;&amp;</c>-separated operands
        /// left-to-right into <see cref="RDFBooleanAndFilter"/> nodes.
        /// </summary>
        private static RDFFilter ParseFilterAnd(RDFQueryParserContext parserContext)
        {
            RDFFilter leftFilter = ParseFilterUnary(parserContext);
            while (TryConsumeOperator(parserContext, "&&"))
                leftFilter = new RDFBooleanAndFilter(leftFilter, ParseFilterUnary(parserContext));
            return leftFilter;
        }

        /// <summary>
        /// Filter skeleton — unary <c>!</c> level. A leading <c>!</c> (not the start of the <c>!=</c> operator)
        /// negates the following unary operand into a <see cref="RDFBooleanNotFilter"/>; otherwise control passes
        /// down to the atom.
        /// </summary>
        private static RDFFilter ParseFilterUnary(RDFQueryParserContext parserContext)
        {
            //A '!' that is NOT the first character of '!=' is the boolean NOT connective
            if (SkipWhitespace(parserContext) == '!')
            {
                ReadCodePoint(parserContext);
                if (PeekCodePoint(parserContext) == '=')
                {
                    //It was actually the '!=' operator with no left operand: that is malformed at this position
                    UnreadCodePoint(parserContext, '!');
                }
                else
                {
                    return new RDFBooleanNotFilter(ParseFilterUnary(parserContext));
                }
            }

            return ParseFilterAtom(parserContext);
        }

        /// <summary>
        /// Filter skeleton — atom level. An atom is either
        /// <list type="bullet">
        /// <item>an <c>EXISTS { … }</c> / <c>NOT EXISTS { … }</c> graph-pattern test;</item>
        /// <item>a value-expression parsed at the relational (<c>ValueLogical</c>) level and wrapped in an
        ///   <see cref="RDFExpressionFilter"/>.</item>
        /// </list>
        /// Entering the expression grammar at the relational level (rather than at <c>||</c>) is what keeps the
        /// top-level <c>||</c>/<c>&amp;&amp;</c> with the skeleton while still letting parentheses and comparisons
        /// work: a leading <c>(</c> is NOT special-cased here — it flows into the relational level, where
        /// <see cref="ParsePrimaryExpression"/> treats it as a bracketed expression. That is what makes
        /// <c>(?x + 1) &gt; 10</c> parse as a comparison (numeric bracket on the left) while <c>(?a &amp;&amp; ?b)</c>
        /// parses as a boolean expression — both correctly wrapped in a single <see cref="RDFExpressionFilter"/>.
        /// </summary>
        private static RDFFilter ParseFilterAtom(RDFQueryParserContext parserContext)
        {
            //Peek for the EXISTS / NOT EXISTS built-ins, which are FILTER-level graph-pattern tests rather than
            //value-expressions. Any other keyword (BOUND, REGEX, …) is left untouched for the expression grammar.
            string peekedKeyword = PeekFilterKeyword(parserContext);
            if (peekedKeyword == "EXISTS")
            {
                ConsumeKeyword(parserContext);
                return ParseExistsFilter(parserContext, false);
            }
            if (peekedKeyword == "NOT")
            {
                ConsumeKeyword(parserContext);
                if (!TryConsumeKeyword(parserContext, "EXISTS"))
                    throw new RDFQueryException("Cannot parse SPARQL FILTER: expected 'EXISTS' after 'NOT' " + GetCoordinates(parserContext));
                return ParseExistsFilter(parserContext, true);
            }

            //Everything else is a value-expression: parse one ValueLogical and wrap it in an expression filter,
            //validating that the resulting expression is one the engine accepts as a boolean constraint
            return WrapExpressionInFilter(parserContext, ParseRelationalExpression(parserContext));
        }

        /// <summary>
        /// Parses an <c>EXISTS</c> / <c>NOT EXISTS</c> graph-pattern test (the keyword has already been consumed) into
        /// the corresponding <see cref="RDFExistsFilter"/> / <see cref="RDFNotExistsFilter"/>.
        /// <para>
        /// The operand is a full <c>GroupGraphPattern</c> (<c>'{' ( SubSelect | GroupGraphPatternSub ) '}'</c>): it is
        /// parsed by the shared group machinery and mapped onto the filter's two constructors — a
        /// <see cref="RDFPatternGroup"/> (GroupGraphPatternSub) or a <see cref="RDFSelectQuery"/> (SubSelect). A
        /// UNION/OPTIONAL/MINUS at the head of the group parses to a binary algebra tree, which is represented as the
        /// legal SubSelect alternative by wrapping it into a <c>SELECT *</c> subquery.
        /// </para>
        /// </summary>
        private static RDFFilter ParseExistsFilter(RDFQueryParserContext parserContext, bool negated)
        {
            //The operand is a GroupGraphPattern: parse it with the existing group machinery so it shares the very
            //same triple/blank-node/collection/UNION/OPTIONAL handling as ordinary group graph patterns
            RDFQueryMember existsGroupMember = ParseGroupGraphPattern(parserContext);

            //Map the parsed member onto the filter's two GroupGraphPattern alternatives. A binary algebra tree
            //(UNION/OPTIONAL/MINUS in head position) becomes a SELECT * subquery: the SubSelect alternative.
            switch (existsGroupMember)
            {
                case RDFPatternGroup patternGroup:
                    return negated ? (RDFFilter)new RDFNotExistsFilter(patternGroup) : new RDFExistsFilter(patternGroup);
                case RDFSelectQuery subSelect:
                    return negated ? (RDFFilter)new RDFNotExistsFilter(subSelect) : new RDFExistsFilter(subSelect);
                default:
                    RDFSelectQuery wrappedSubSelect = WrapIntoSubQuery(new List<RDFQueryMember> { existsGroupMember });
                    return negated ? (RDFFilter)new RDFNotExistsFilter(wrappedSubSelect) : new RDFExistsFilter(wrappedSubSelect);
            }
        }

        /// <summary>
        /// Wraps a boolean value-expression in an <see cref="RDFExpressionFilter"/>, dispatching on the concrete
        /// expression type so the right public constructor is invoked. An expression that the engine does not accept
        /// as a boolean filter (e.g. a bare arithmetic/string expression, or a built-in with no filter constructor)
        /// is rejected with a precise message.
        /// </summary>
        private static RDFFilter WrapExpressionInFilter(RDFQueryParserContext parserContext, RDFExpression valueExpression)
        {
            switch (valueExpression)
            {
                case RDFBooleanExpression booleanExpression:       return new RDFExpressionFilter(booleanExpression);
                case RDFComparisonExpression comparisonExpression: return new RDFExpressionFilter(comparisonExpression);
                case RDFBoundExpression boundExpression:           return new RDFExpressionFilter(boundExpression);
                case RDFInExpression inExpression:                 return new RDFExpressionFilter(inExpression);
                case RDFNotExpression notExpression:               return new RDFExpressionFilter(notExpression);
                case RDFIsBlankExpression isBlankExpression:       return new RDFExpressionFilter(isBlankExpression);
                case RDFIsLiteralExpression isLiteralExpression:   return new RDFExpressionFilter(isLiteralExpression);
                case RDFIsNumericExpression isNumericExpression:   return new RDFExpressionFilter(isNumericExpression);
                case RDFIsUriExpression isUriExpression:           return new RDFExpressionFilter(isUriExpression);
                case RDFLangMatchesExpression langMatchesExpr:     return new RDFExpressionFilter(langMatchesExpr);
                case RDFRegexExpression regexExpression:           return new RDFExpressionFilter(regexExpression);
                case RDFSameTermExpression sameTermExpression:     return new RDFExpressionFilter(sameTermExpression);
                case RDFContainsExpression containsExpression:     return new RDFExpressionFilter(containsExpression);
                case RDFStrStartsExpression strStartsExpression:   return new RDFExpressionFilter(strStartsExpression);
                case RDFStrEndsExpression strEndsExpression:       return new RDFExpressionFilter(strEndsExpression);
                case RDFHasLangExpression hasLangExpression:       return new RDFExpressionFilter(hasLangExpression);
                case RDFHasLangDirExpression hasLangDirExpression: return new RDFExpressionFilter(hasLangDirExpression);

                //A bare GeoSPARQL relation (geof:sfWithin(?a,?b), geof:rcc8eq(?a,?b), …) yields a boolean-typed
                //literal but has no RDFExpressionFilter constructor of its own. The engine's idiom is to test it for
                //truth, so a standalone geo constraint is wrapped as the comparison 'geoExpression = true'.
                case RDFGeoExpression geoExpression:
                    return new RDFExpressionFilter(new RDFComparisonExpression(
                        RDFQueryEnums.RDFComparisonFlavors.EqualTo, geoExpression, new RDFConstantExpression(RDFTypedLiteral.True)));

                default:
                    throw new RDFQueryException("Cannot parse SPARQL FILTER: the constraint expression does not evaluate to a boolean usable as a filter " + GetCoordinates(parserContext));
            }
        }
        #endregion

        #region Expression.Precedence
        /// <summary>
        /// Expression grammar — <c>ConditionalOrExpression ::= ConditionalAndExpression ('||' ConditionalAndExpression)*</c>.
        /// Folds into left-associative <see cref="RDFBooleanOrExpression"/> nodes. This level is only ever entered
        /// from inside a <c>(</c> bracket or a function argument; the FILTER skeleton owns the top-level <c>||</c>.
        /// </summary>
        private static RDFExpression ParseExpression(RDFQueryParserContext parserContext)
        {
            RDFExpression leftExpression = ParseConditionalAndExpression(parserContext);
            while (TryConsumeOperator(parserContext, "||"))
                leftExpression = new RDFBooleanOrExpression(leftExpression, ParseConditionalAndExpression(parserContext));
            return leftExpression;
        }

        /// <summary>
        /// Expression grammar — <c>ConditionalAndExpression ::= ValueLogical ('&amp;&amp;' ValueLogical)*</c>.
        /// Folds into left-associative <see cref="RDFBooleanAndExpression"/> nodes.
        /// </summary>
        private static RDFExpression ParseConditionalAndExpression(RDFQueryParserContext parserContext)
        {
            RDFExpression leftExpression = ParseRelationalExpression(parserContext);
            while (TryConsumeOperator(parserContext, "&&"))
                leftExpression = new RDFBooleanAndExpression(leftExpression, ParseRelationalExpression(parserContext));
            return leftExpression;
        }

        /// <summary>
        /// Expression grammar — <c>ValueLogical ::= RelationalExpression</c>. Parses a numeric expression and, if a
        /// single relational operator follows, builds the corresponding boolean node:
        /// <list type="bullet">
        /// <item><c>= != &lt; &lt;= &gt; &gt;=</c> → <see cref="RDFComparisonExpression"/>;</item>
        /// <item><c>IN ( … )</c> → <see cref="RDFInExpression"/>.</item>
        /// </list>
        /// SPARQL's <c>RelationalExpression</c> is non-associative (at most one operator), so this method does not loop.
        /// </summary>
        private static RDFExpression ParseRelationalExpression(RDFQueryParserContext parserContext)
        {
            RDFExpression leftExpression = ParseAdditiveExpression(parserContext);

            //Try each relational operator in turn. The two-character operators ('<=', '>=', '!=') MUST be probed
            //before their one-character prefixes ('<', '>') so the longer match wins.
            if (TryConsumeOperator(parserContext, "<="))
                return new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan, leftExpression, ParseAdditiveExpression(parserContext));
            if (TryConsumeOperator(parserContext, ">="))
                return new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, leftExpression, ParseAdditiveExpression(parserContext));
            if (TryConsumeOperator(parserContext, "!="))
                return new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo, leftExpression, ParseAdditiveExpression(parserContext));
            if (TryConsumeOperator(parserContext, "="))
                return new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, leftExpression, ParseAdditiveExpression(parserContext));
            if (TryConsumeOperator(parserContext, "<"))
                return new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan, leftExpression, ParseAdditiveExpression(parserContext));
            if (TryConsumeOperator(parserContext, ">"))
                return new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, leftExpression, ParseAdditiveExpression(parserContext));

            //'IN ( t1, t2, … )' membership test (terms may be constants, variables or full expressions)
            if (PeekFilterKeyword(parserContext) == "IN")
            {
                ConsumeKeyword(parserContext);
                return new RDFInExpression(leftExpression, ParseExpressionTermList(parserContext));
            }
            //'NOT IN ( … )' negated membership test, modelled as '!( … IN … )' via the logical-negation expression
            if (PeekFilterKeyword(parserContext) == "NOT")
            {
                ConsumeKeyword(parserContext);
                if (TryConsumeKeyword(parserContext, "IN"))
                    return new RDFNotExpression(new RDFInExpression(leftExpression, ParseExpressionTermList(parserContext)));
                throw new RDFQueryException("Cannot parse SPARQL expression: unexpected 'NOT' " + GetCoordinates(parserContext));
            }

            //No relational operator: the value is just the numeric/primary expression itself
            return leftExpression;
        }

        /// <summary>
        /// Expression grammar — <c>AdditiveExpression ::= MultiplicativeExpression (('+'|'-') MultiplicativeExpression)*</c>.
        /// Folds into left-associative <see cref="RDFAddExpression"/> / <see cref="RDFSubtractExpression"/> nodes.
        /// </summary>
        private static RDFExpression ParseAdditiveExpression(RDFQueryParserContext parserContext)
        {
            RDFExpression leftExpression = ParseMultiplicativeExpression(parserContext);
            while (true)
            {
                //A '+'/'-' is the additive operator ONLY when it does not directly start a signed numeric literal
                //(that case is part of the right operand and is handled by the primary term-reader instead).
                if (IsAdditiveOperatorAhead(parserContext, '+'))
                {
                    ReadCodePoint(parserContext);
                    leftExpression = new RDFAddExpression(leftExpression, ParseMultiplicativeExpression(parserContext));
                }
                else if (IsAdditiveOperatorAhead(parserContext, '-'))
                {
                    ReadCodePoint(parserContext);
                    leftExpression = new RDFSubtractExpression(leftExpression, ParseMultiplicativeExpression(parserContext));
                }
                else
                {
                    return leftExpression;
                }
            }
        }

        /// <summary>
        /// Expression grammar — <c>MultiplicativeExpression ::= UnaryExpression (('*'|'/') UnaryExpression)*</c>.
        /// Folds into left-associative <see cref="RDFMultiplyExpression"/> / <see cref="RDFDivideExpression"/> nodes.
        /// </summary>
        private static RDFExpression ParseMultiplicativeExpression(RDFQueryParserContext parserContext)
        {
            RDFExpression leftExpression = ParseUnaryExpression(parserContext);
            while (true)
            {
                if (TryConsumeChar(parserContext, '*'))
                    leftExpression = new RDFMultiplyExpression(leftExpression, ParseUnaryExpression(parserContext));
                else if (TryConsumeChar(parserContext, '/'))
                    leftExpression = new RDFDivideExpression(leftExpression, ParseUnaryExpression(parserContext));
                else
                    return leftExpression;
            }
        }

        /// <summary>
        /// Expression grammar — <c>UnaryExpression ::= ('!' | '+' | '-')? PrimaryExpression</c>.
        /// <list type="bullet">
        /// <item><c>!</c> inside an expression has no object-model representation (there is no Boolean-NOT
        ///   expression) and is rejected — it is only valid at the FILTER skeleton.</item>
        /// <item><c>+</c> is the identity and is dropped.</item>
        /// <item><c>-</c> applied to a non-literal primary becomes <c>0 - primary</c> via
        ///   <see cref="RDFSubtractExpression"/>; a <c>-</c> that directly precedes a numeric literal is left to the
        ///   term-reader so the signed literal parses as one token.</item>
        /// </list>
        /// </summary>
        private static RDFExpression ParseUnaryExpression(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //'!' is the logical-negation unary operator: consume it and negate the following unary operand. At this
            //position (the left operand has already been parsed at the relational level) it is never the '!=' operator.
            if (nextSignificantCodePoint == '!')
            {
                ReadCodePoint(parserContext);
                return new RDFNotExpression(ParseUnaryExpression(parserContext));
            }

            //'+' / '-' that directly precede a numeric literal belong to that literal (a signed number), so let the
            //primary term-reader consume the whole token rather than treating the sign as a unary operator here.
            if ((nextSignificantCodePoint == '+' || nextSignificantCodePoint == '-') && !IsSignedNumericLiteralAhead(parserContext))
            {
                ReadCodePoint(parserContext);
                RDFExpression primaryExpression = ParsePrimaryExpression(parserContext);

                //Unary '+' is the identity; unary '-' becomes (0 - primary)
                return nextSignificantCodePoint == '-'
                    ? new RDFSubtractExpression(MakeZeroConstantExpression(), primaryExpression)
                    : primaryExpression;
            }

            return ParsePrimaryExpression(parserContext);
        }
        #endregion

        #region Expression.Primary
        /// <summary>
        /// Expression grammar — <c>PrimaryExpression ::= BrackettedExpression | BuiltInCall | iriOrFunction | RDFLiteral | NumericLiteral | BooleanLiteral | Var</c>.
        /// </summary>
        private static RDFExpression ParsePrimaryExpression(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //BrackettedExpression: a parenthesised sub-expression resets precedence to the top (ConditionalOr)
            if (nextSignificantCodePoint == '(')
            {
                ExpectChar(parserContext, '(', "expression");
                RDFExpression bracketedExpression = ParseExpression(parserContext);
                ExpectChar(parserContext, ')', "expression");
                return bracketedExpression;
            }

            //Var: a '?'/'$' sigil
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return new RDFVariableExpression(ParseVariable(parserContext));

            //An ASCII-letter run may be a built-in call (BOUND, STR, …), a boolean literal (true/false), or the
            //prefix label of a prefixed-name term/function (e.g. 'geof:distance(...)' or 'ex:thing').
            if (IsAsciiLetter(nextSignificantCodePoint))
            {
                //Read the run as a built-in function NAME (letters + digits + underscore), so names like MD5,
                //SHA256 and ENCODE_FOR_URI tokenise whole rather than being cut at the first digit/underscore.
                string letterRun = ReadFunctionName(parserContext);

                //If the next significant character is ':' the run is a prefix label, not a keyword: rewind and let
                //the general term path resolve the prefixed name (and detect a following '(' for a function call).
                if (SkipWhitespace(parserContext) == ':')
                {
                    UnreadString(parserContext, letterRun);
                    return ParseIriOrFunctionExpression(parserContext);
                }

                //If the next significant character is '(' the run names a function: dispatch to the built-in parser.
                if (SkipWhitespace(parserContext) == '(')
                {
                    //Aggregate-aware contexts (free HAVING / projection expression): an aggregate function name is
                    //resolved to a reference over its (existing or hidden) aggregator column rather than being
                    //rejected as an unknown built-in. Elsewhere the sink is null and the aggregate falls through to
                    //ParseBuiltInCall, which fails loudly as it must.
                    if (parserContext.AggregateExpressionSink != null && AggregatorKeywords.Contains(letterRun))
                    {
                        UnreadString(parserContext, letterRun);
                        RDFParsedAggregator nestedAggregate = ParseAggregator(parserContext);
                        return parserContext.AggregateExpressionSink(nestedAggregate);
                    }

                    return ParseBuiltInCall(parserContext, letterRun.ToUpperInvariant());
                }

                //Otherwise it is a bareword term (true/false, or — invalid — a relative name): rewind and read it
                //through the shared term-reader, which recognises the boolean literals.
                UnreadString(parserContext, letterRun);
                return MakeConstantExpression(parserContext, ParseTerm(parserContext));
            }

            //A '<' here starts an IRIREF (iriOrFunction), every other character starts a concrete RDF term
            //(numeric/string literal). Both go through the shared term-reader.
            if (nextSignificantCodePoint == '<')
                return ParseIriOrFunctionExpression(parserContext);

            return MakeConstantExpression(parserContext, ParseTerm(parserContext));
        }

        /// <summary>
        /// Parses an <c>iriOrFunction</c> primary: an IRI (IRIREF or prefixed name) optionally followed by an
        /// argument list. A trailing <c>(</c> makes it a function call (e.g. the GeoSPARQL <c>geof:</c> functions,
        /// handled in phase F6b); without arguments it is a constant IRI term.
        /// </summary>
        private static RDFExpression ParseIriOrFunctionExpression(RDFQueryParserContext parserContext)
        {
            //Read the IRI/prefixed-name term through the shared term-reader (prologue BASE/PREFIX resolution applies)
            RDFPatternMember iriTerm = ParseTerm(parserContext);
            if (!(iriTerm is RDFResource functionOrConstantIri))
                throw new RDFQueryException("Cannot parse SPARQL expression: expected an IRI " + GetCoordinates(parserContext));

            //A '(' turns the IRI into a function call: dispatch to the IRI-function handler (GeoSPARQL geof:, …)
            if (SkipWhitespace(parserContext) == '(')
                return ParseIriFunctionCall(parserContext, functionOrConstantIri);

            //No arguments: the IRI is a plain constant term
            return new RDFConstantExpression(functionOrConstantIri);
        }

        /// <summary>
        /// Parses the parenthesised, comma-separated argument list of a SPARQL <c>ArgList</c> / <c>ExpressionList</c>
        /// into a list of <see cref="RDFExpression"/>. The opening <c>(</c> and closing <c>)</c> are consumed here.
        /// An empty list <c>()</c> yields an empty result.
        /// </summary>
        private static List<RDFExpression> ParseExpressionArgumentList(RDFQueryParserContext parserContext)
        {
            ExpectChar(parserContext, '(', "argument list");

            List<RDFExpression> argumentExpressions = new List<RDFExpression>();

            //Empty argument list '()'
            if (SkipWhitespace(parserContext) == ')')
            {
                ReadCodePoint(parserContext);
                return argumentExpressions;
            }

            //One or more comma-separated argument expressions
            while (true)
            {
                argumentExpressions.Add(ParseExpression(parserContext));
                if (SkipWhitespace(parserContext) == ',')
                {
                    ReadCodePoint(parserContext);
                    continue;
                }
                break;
            }

            ExpectChar(parserContext, ')', "argument list");
            return argumentExpressions;
        }

        /// <summary>
        /// Parses the parenthesised, comma-separated <c>ExpressionList</c> that follows an <c>IN</c> / <c>NOT IN</c>
        /// operator into a list of <see cref="RDFExpression"/>. Terms may be constants, variables or full expressions
        /// (the engine's <see cref="RDFInExpression"/> evaluates each per-row as an equality comparison).
        /// </summary>
        private static List<RDFExpression> ParseExpressionTermList(RDFQueryParserContext parserContext)
        {
            ExpectChar(parserContext, '(', "IN term list");

            List<RDFExpression> inTerms = new List<RDFExpression>();

            //Empty list 'IN ()' never matches anything but is syntactically legal
            if (SkipWhitespace(parserContext) == ')')
            {
                ReadCodePoint(parserContext);
                return inTerms;
            }

            while (true)
            {
                inTerms.Add(ParseExpression(parserContext));
                if (SkipWhitespace(parserContext) == ',')
                {
                    ReadCodePoint(parserContext);
                    continue;
                }
                break;
            }

            ExpectChar(parserContext, ')', "IN term list");
            return inTerms;
        }
        #endregion
    }
}