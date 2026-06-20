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
        private static RDFGroupByModifier ParseGroupByModifier(RDFQueryParserContext parserContext, Action<RDFModifier> addModifier, List<RDFAggregator> pendingAggregators)
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

            addModifier(groupByModifier);
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
        /// <see cref="ParseSolutionModifiers"/>) into a single free boolean expression set on the
        /// <paramref name="groupByModifier"/>. Each HavingCondition is a bracketed expression
        /// (<c>'(' Expression ')'</c>); multiple space-separated conditions are conjoined (<c>HavingCondition+</c>
        /// is implicitly ANDed). The expression grammar is reused verbatim via <see cref="ParseExpression"/>, with
        /// the context's <see cref="RDFQueryParserContext.AggregateExpressionSink"/> switched on so that aggregate
        /// calls (<c>COUNT(?e)</c>, <c>AVG(?g)</c>, ...) resolve to references over their aggregator columns — which
        /// is what unlocks the full HAVING power (disjunctions, aggregate on the right-hand side, non-comparison
        /// constraints, and aggregates not present in the SELECT projection, served by a hidden aggregator).
        /// </summary>
        /// <exception cref="RDFQueryException">When HAVING appears without a group (no GROUP BY and no projected aggregate), or no condition is found.</exception>
        private static void ParseHavingClause(RDFQueryParserContext parserContext, RDFGroupByModifier groupByModifier)
        {
            //HAVING filters the grouped solutions: without a group there is no aggregator column to evaluate against
            if (groupByModifier == null)
                throw new RDFQueryException("Cannot parse SPARQL HAVING clause: HAVING requires a GROUP BY clause (or an aggregate in the projection) " + GetCoordinates(parserContext));

            //Counter for the hidden aggregators a HAVING condition may need (an aggregate referenced by HAVING but
            //NOT projected by SELECT): each gets a fresh, internal projection column the condition reads from
            int hiddenHavingAggregatorCounter = 0;

            //Switch the expression parser to aggregate-aware mode for the duration of the HAVING clause: an aggregate
            //call is resolved to a reference over its aggregator column (an existing projected one when present,
            //otherwise a freshly registered hidden aggregator). Always cleared afterwards (finally).
            parserContext.AggregateExpressionSink = parsedAggregate =>
                ResolveHavingAggregateReference(groupByModifier, parsedAggregate, ref hiddenHavingAggregatorCounter);

            RDFExpression combinedHavingExpression = null;
            try
            {
                bool foundAtLeastOneCondition = false;
                while (SkipWhitespace(parserContext) == '(')
                {
                    //Each HavingCondition is a BrackettedExpression: '(' Expression ')'
                    ExpectChar(parserContext, '(', "HAVING condition");
                    RDFExpression havingCondition = ParseExpression(parserContext);
                    ExpectChar(parserContext, ')', "HAVING condition");

                    //Multiple space-separated conditions ('HavingCondition+') are implicitly ANDed together
                    combinedHavingExpression = combinedHavingExpression == null
                        ? havingCondition
                        : new RDFBooleanAndExpression(combinedHavingExpression, havingCondition);
                    foundAtLeastOneCondition = true;
                }

                if (!foundAtLeastOneCondition)
                    throw new RDFQueryException("Cannot parse SPARQL HAVING clause: expected at least one '(...)' condition " + GetCoordinates(parserContext));
            }
            finally
            {
                parserContext.AggregateExpressionSink = null;
            }

            groupByModifier.SetHavingExpression(combinedHavingExpression);
        }

        /// <summary>
        /// Resolves an aggregate referenced inside a HAVING condition to the result-table column carrying its value:
        /// when the SAME aggregate is already projected by the SELECT clause its existing aggregator column is reused,
        /// otherwise a HIDDEN aggregator is registered on the fly (it computes the value into an internal column that
        /// is never projected as a result). Either way an <see cref="RDFAggregateReferenceExpression"/> is returned,
        /// so the condition reads the column at evaluation time yet re-prints the original aggregate call.
        /// </summary>
        private static RDFExpression ResolveHavingAggregateReference(RDFGroupByModifier groupByModifier, RDFParsedAggregator parsedAggregate, ref int hiddenHavingAggregatorCounter)
        {
            //Reuse the aggregator the SELECT projection already declared for the same function over the same argument:
            //reference its (projected) column directly — it prints faithfully as its own alias and round-trips
            RDFAggregator existingAggregator = groupByModifier.Aggregators
                .FirstOrDefault(ag => !(ag is RDFPartitionAggregator) && MatchesAggregator(ag, parsedAggregate));
            if (existingAggregator != null)
                return new RDFVariableExpression(existingAggregator.Metadata.ProjectionVariable);

            //Otherwise register a hidden aggregator: it materializes the value into a synthetic column the HAVING
            //condition reads (engine keeps it out of the output projection); the printer re-prints that column as the
            //original aggregate call, so the printed query round-trips
            RDFAggregator hiddenAggregator = BuildAggregator(parsedAggregate, MakeHiddenHavingVariable(hiddenHavingAggregatorCounter++));
            hiddenAggregator.Metadata.IsHidden = true;
            groupByModifier.AddAggregator(hiddenAggregator);
            return new RDFVariableExpression(hiddenAggregator.Metadata.ProjectionVariable);
        }

        /// <summary>
        /// Builds the internal projection variable backing a HIDDEN HAVING aggregator (an aggregate referenced by
        /// HAVING but not projected by SELECT): a reserved '__HAVINGAGG_n' name that never surfaces in the results.
        /// </summary>
        private static RDFVariable MakeHiddenHavingVariable(int index)
            => new RDFVariable("?__HAVINGAGG_" + index);
        #endregion
    }
}
