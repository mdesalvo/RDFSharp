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
using System.Linq;
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// Grouping half of the SPARQL parser: the GROUP BY clause (which materializes the single
    /// <see cref="RDFGroupByModifier"/> and absorbs the aggregates parked by the SELECT projection) and the
    /// HAVING clause (whose restricted comparison conditions are attached, as having-clauses, to the matching
    /// aggregators).
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// [19] GroupClause     ::= 'GROUP' 'BY' GroupCondition+
    /// [20] GroupCondition  ::= BuiltInCall | FunctionCall | '(' Expression ('AS' Var)? ')' | Var
    /// [21] HavingClause    ::= 'HAVING' HavingCondition+
    /// [22] HavingCondition ::= Constraint   // = BrackettedExpression | BuiltInCall | FunctionCall
    /// </code>
    /// </para>
    /// <para>
    /// Model-imposed limits (the flat model groups on bare variables and filters via per-aggregator
    /// having-clauses): a GroupCondition that is not a bare <c>Var</c> (an <c>(expr AS ?v)</c>, a bare
    /// expression, a built-in or a function call) is NOT representable; a HAVING condition that is not a
    /// conjunction of <c>(AGGREGATE OP value)</c> comparisons (e.g. disjunctions, non-aggregate constraints,
    /// or an aggregate not present in the SELECT projection) is NOT representable. Each such case raises an
    /// explicit <see cref="RDFQueryException"/>.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region GroupBy
        /// <summary>
        /// The keywords that may legally FOLLOW a GROUP BY clause (HavingClause? OrderClause? LimitOffsetClauses?).
        /// Used to tell the end of the grouping-variable list from a non-representable function/built-in
        /// GroupCondition: any other bare keyword after the variables is a GroupCondition the flat model cannot
        /// represent, and must fail loudly rather than be silently left as unconsumed trailing input.
        /// </summary>
        private static readonly HashSet<string> GroupByFollowerKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "HAVING", "ORDER", "LIMIT", "OFFSET" };

        /// <summary>
        /// Parses the body of a GROUP BY clause (the <c>GROUP</c> keyword has already been consumed by
        /// <see cref="ParseSolutionModifiers"/>): the mandatory <c>BY</c>, then one or more bare grouping
        /// variables. Builds the single <see cref="RDFGroupByModifier"/>, absorbs the aggregates parked by the
        /// SELECT projection (<paramref name="pendingAggregators"/>), attaches the modifier to the query, and
        /// returns it so a later HAVING clause can hang its conditions off the very same aggregators.
        /// </summary>
        /// <exception cref="RDFQueryException">When 'BY' is missing, no grouping variable is found, or a non-representable (non-variable) GroupCondition is encountered.</exception>
        private static RDFGroupByModifier ParseGroupByModifier(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery, List<RDFAggregator> pendingAggregators)
        {
            //The 'BY' keyword is mandatory and must immediately follow 'GROUP'
            if (!TryConsumeKeyword(parserContext, "BY"))
                throw new RDFQueryException("Cannot parse SPARQL GROUP BY clause: expected 'BY' after 'GROUP' " + GetCoordinates(parserContext));

            RDFGroupByModifier groupByModifier = new RDFGroupByModifier();
            int anonymousConditionCounter = 0;
            bool foundAtLeastOneCondition = false;
            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);

                //A '?' or '$' sigil is a bare grouping variable
                if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                {
                    groupByModifier.AddPartitionVariable(ParseVariable(parserContext));
                    foundAtLeastOneCondition = true;
                    continue;
                }

                //A '(' opens a '(Expression ('AS' Var)?)' grouping condition
                if (nextSignificantCodePoint == '(')
                {
                    ExpectChar(parserContext, '(', "GROUP BY condition");
                    RDFExpression groupExpression = ParseExpression(parserContext);
                    if (TryConsumeKeyword(parserContext, "AS"))
                    {
                        //Named: '(expr AS ?v)' => ?v is a real, projectable grouping variable
                        int sigilCodePoint = SkipWhitespace(parserContext);
                        if (sigilCodePoint != '?' && sigilCodePoint != '$')
                            throw new RDFQueryException("Cannot parse SPARQL GROUP BY clause: expected a variable after 'AS' inside '(expr AS ?v)' " + GetCoordinates(parserContext));
                        groupByModifier.AddPartitionExpression(ParseVariable(parserContext), groupExpression, true);
                    }
                    else
                    {
                        //Anonymous: '(expr)' => an internal grouping column not projectable by name
                        groupByModifier.AddPartitionExpression(MakeAnonymousGroupVariable(anonymousConditionCounter++), groupExpression, false);
                    }
                    ExpectChar(parserContext, ')', "GROUP BY condition");
                    foundAtLeastOneCondition = true;
                    continue;
                }

                //A bare keyword: either a clause that legally follows GROUP BY (stop) or a built-in/function
                //grouping condition (e.g. 'GROUP BY STR(?x)'), which is parsed as an anonymous expression condition
                string followerKeyword = ReadKeyword(parserContext);
                UnreadString(parserContext, followerKeyword);
                if (followerKeyword.Length > 0 && !GroupByFollowerKeywords.Contains(followerKeyword))
                {
                    RDFExpression groupExpression = ParseExpression(parserContext);
                    groupByModifier.AddPartitionExpression(MakeAnonymousGroupVariable(anonymousConditionCounter++), groupExpression, false);
                    foundAtLeastOneCondition = true;
                    continue;
                }
                break;
            }

            //At least one grouping condition is mandatory
            if (!foundAtLeastOneCondition)
                throw new RDFQueryException("Cannot parse SPARQL GROUP BY clause: expected at least one grouping condition " + GetCoordinates(parserContext));

            //Absorb the aggregates the projection parked while waiting for GROUP BY (registering their computed columns)
            AbsorbPendingAggregators(groupByModifier, pendingAggregators);

            selectQuery.AddModifier(groupByModifier);
            return groupByModifier;
        }

        /// <summary>
        /// Builds the internal (synthetic) variable backing an anonymous GROUP BY expression condition ('GROUP BY
        /// (expr)'): it has no projectable name, so a reserved '__GROUPEXPR_n' name is used and later dropped.
        /// </summary>
        private static RDFVariable MakeAnonymousGroupVariable(int index)
            => new RDFVariable("?__GROUPEXPR_" + index);

        /// <summary>
        /// Attaches the parked aggregates to the GroupBy modifier (AddAggregator itself registers the computed column
        /// for any aggregate-over-expression, so the modifier materializes it before partitioning).
        /// </summary>
        private static void AbsorbPendingAggregators(RDFGroupByModifier groupByModifier, List<RDFAggregator> pendingAggregators)
        {
            foreach (RDFAggregator pendingAggregator in pendingAggregators)
                groupByModifier.AddAggregator(pendingAggregator);
            pendingAggregators.Clear();
        }

        /// <summary>
        /// Parses the body of a HAVING clause (the <c>HAVING</c> keyword has already been consumed by
        /// <see cref="ParseSolutionModifiers"/>) and attaches its conditions, as having-clauses, to the
        /// aggregators of <paramref name="groupByModifier"/>. Each HavingCondition is a bracketed expression
        /// whose content must reduce to a conjunction (<c>&amp;&amp;</c>) of <c>(AGGREGATE OP value)</c>
        /// comparisons; multiple space-separated conditions are likewise conjoined (the flat model ANDs all
        /// having-clauses together).
        /// </summary>
        /// <exception cref="RDFQueryException">When HAVING appears without a GROUP BY, no condition is found, or a condition is not representable.</exception>
        private static void ParseHavingClause(RDFQueryParserContext parserContext, RDFGroupByModifier groupByModifier)
        {
            //HAVING filters the grouped solutions: without a GROUP BY there is no group (and no aggregator to hang it on)
            if (groupByModifier == null)
                throw new RDFQueryException("Cannot parse SPARQL HAVING clause: HAVING requires a GROUP BY clause " + GetCoordinates(parserContext));

            bool foundAtLeastOneCondition = false;
            while (SkipWhitespace(parserContext) == '(')
            {
                //Each HavingCondition is a BrackettedExpression: '(' Expression ')'
                ExpectChar(parserContext, '(', "HAVING condition");
                ParseHavingExpression(parserContext, groupByModifier);
                ExpectChar(parserContext, ')', "HAVING condition");
                foundAtLeastOneCondition = true;
            }

            if (!foundAtLeastOneCondition)
                throw new RDFQueryException("Cannot parse SPARQL HAVING clause: expected at least one '(...)' condition " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Parses a HAVING expression as a conjunction of conjuncts (<c>Conjunct ('&amp;&amp;' Conjunct)*</c>),
        /// attaching each resolved comparison to its aggregator. A disjunction (<c>||</c>) is rejected: the flat
        /// model can only AND having-clauses together.
        /// </summary>
        /// <exception cref="RDFQueryException">When a disjunction is encountered, or a conjunct is not representable.</exception>
        private static void ParseHavingExpression(RDFQueryParserContext parserContext, RDFGroupByModifier groupByModifier)
        {
            ParseHavingConjunct(parserContext, groupByModifier);
            while (TryConsumeOperator(parserContext, "&&"))
                ParseHavingConjunct(parserContext, groupByModifier);

            //'||' would require an OR of having-clauses, which the flat model cannot represent: fail loudly
            if (SkipWhitespace(parserContext) == '|')
                throw new RDFQueryException("Cannot parse SPARQL HAVING clause: disjunctive ('||') conditions are not representable " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Parses a single HAVING conjunct: either a parenthesized sub-expression (peeled recursively, which also
        /// absorbs the printer's redundant grouping such as <c>((AVG(?g) &gt;= 24))</c>) or a bare
        /// <c>AGGREGATE OP value</c> comparison.
        /// </summary>
        private static void ParseHavingConjunct(RDFQueryParserContext parserContext, RDFGroupByModifier groupByModifier)
        {
            if (SkipWhitespace(parserContext) == '(')
            {
                ExpectChar(parserContext, '(', "HAVING condition");
                ParseHavingExpression(parserContext, groupByModifier);
                ExpectChar(parserContext, ')', "HAVING condition");
                return;
            }

            ParseHavingComparison(parserContext, groupByModifier);
        }

        /// <summary>
        /// Parses a single <c>AGGREGATE OP value</c> comparison and attaches it, as a having-clause, to the
        /// aggregator that the SELECT projection already declared for the same function over the same variable.
        /// The right operand is a constant (literal/IRI) or a variable; <c>value OP AGGREGATE</c> (aggregate on
        /// the right) and aggregates absent from the projection are not representable.
        /// </summary>
        /// <exception cref="RDFQueryException">When the comparison is malformed or its aggregate is not present in the SELECT projection.</exception>
        private static void ParseHavingComparison(RDFQueryParserContext parserContext, RDFGroupByModifier groupByModifier)
        {
            //Left operand: must be an aggregate (ParseAggregator throws otherwise)
            RDFParsedAggregator parsedAggregator = ParseAggregator(parserContext);

            //Comparison operator
            RDFQueryEnums.RDFComparisonFlavors comparisonFlavor = ParseComparisonFlavor(parserContext);

            //Right operand: a variable or a constant term
            RDFPatternMember comparisonValue = ParseHavingValue(parserContext);

            //Attach to the aggregator declared by the projection (matched by function kind + aggregated variable)
            RDFAggregator targetAggregator = groupByModifier.Aggregators.FirstOrDefault(ag => MatchesAggregator(ag, parsedAggregator));
            if (targetAggregator == null)
                throw new RDFQueryException("Cannot parse SPARQL HAVING clause: the referenced aggregate must also appear in the SELECT projection " + GetCoordinates(parserContext));

            targetAggregator.SetHavingClause(comparisonFlavor, comparisonValue);
        }

        /// <summary>
        /// Reads one of the six SPARQL comparison operators and maps it to the corresponding
        /// <see cref="RDFQueryEnums.RDFComparisonFlavors"/>. The two-character operators are probed before their
        /// one-character prefixes so the longer match wins (mirrors <see cref="ParseRelationalExpression"/>).
        /// </summary>
        /// <exception cref="RDFQueryException">When no comparison operator is present.</exception>
        private static RDFQueryEnums.RDFComparisonFlavors ParseComparisonFlavor(RDFQueryParserContext parserContext)
        {
            if (TryConsumeOperator(parserContext, "<="))
                return RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan;
            if (TryConsumeOperator(parserContext, ">="))
                return RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan;
            if (TryConsumeOperator(parserContext, "!="))
                return RDFQueryEnums.RDFComparisonFlavors.NotEqualTo;
            if (TryConsumeOperator(parserContext, "="))
                return RDFQueryEnums.RDFComparisonFlavors.EqualTo;
            if (TryConsumeOperator(parserContext, "<"))
                return RDFQueryEnums.RDFComparisonFlavors.LessThan;
            if (TryConsumeOperator(parserContext, ">"))
                return RDFQueryEnums.RDFComparisonFlavors.GreaterThan;

            throw new RDFQueryException("Cannot parse SPARQL HAVING clause: expected a comparison operator (=, !=, <, <=, >, >=) " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Parses the right operand of a HAVING comparison: a bare variable (<c>?v</c>/<c>$v</c>) or a constant
        /// RDF term (literal or IRI). Both are <see cref="RDFPatternMember"/>s, as the having-clause expects.
        /// </summary>
        private static RDFPatternMember ParseHavingValue(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParseVariable(parserContext);

            return ParseTerm(parserContext);
        }
        #endregion
    }
}
