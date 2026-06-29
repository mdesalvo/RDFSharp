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
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// Solution-modifier half of the SPARQL parser: the trailing ORDER BY / LIMIT / OFFSET clauses of a SELECT query.
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region SolutionModifiers
        /// <summary>
        /// <para>
        /// Parses the trailing solution-modifier section of a SELECT query and attaches the resulting
        /// modifier objects via <paramref name="addModifier"/>. Scanning stops as soon as a token that is
        /// not a recognized modifier keyword is encountered; that token is pushed back so the caller can
        /// process it (or simply ignore it at end of input).
        /// </para>
        /// <para>
        /// SPARQL grammar:
        /// <code>
        /// SolutionModifier ::= GroupClause? HavingClause? OrderClause? LimitOffsetClauses?
        /// LimitOffsetClauses ::= LimitClause OffsetClause? | OffsetClause LimitClause?
        /// </code>
        /// All five clauses are handled: GROUP BY and HAVING (delegated to <see cref="ParseGroupByModifier"/> /
        /// <see cref="ParseHavingClause"/>) plus ORDER BY, LIMIT and OFFSET. Modifiers are accepted in any order
        /// for leniency; if the same modifier appears twice the object-model silently ignores the duplicate.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When a recognized modifier keyword is followed by a malformed body.</exception>
        private static void ParseSolutionModifiers(RDFQueryParserContext parserContext, Action<RDFModifier> addModifier, List<RDFAggregator> pendingAggregators)
        {
            //The single GROUP BY modifier, captured once parsed, so a later HAVING can hang its conditions off
            //the very same aggregators (HAVING follows GROUP BY in the SPARQL grammar)
            RDFGroupByModifier groupByModifier = null;

            while (true)
            {
                //Advance to the first significant character so ReadKeyword finds the keyword immediately
                SkipWhitespace(parserContext);

                //Read the upcoming keyword (may be empty at EOF or when the next token is not a keyword)
                string modifierKeyword = ReadKeyword(parserContext);

                switch (modifierKeyword.ToUpperInvariant())
                {
                    //GROUP BY clause: build the grouping modifier and absorb the projection's parked aggregates
                    case "GROUP":
                        groupByModifier = ParseGroupByModifier(parserContext, addModifier, pendingAggregators);
                        break;

                    //HAVING clause: build its free boolean condition over the grouping modifier's aggregators. When
                    //no explicit GROUP BY preceded it but the projection used aggregates, this is SPARQL implicit
                    //grouping (a single group over the whole result set): materialize the implicit modifier here so
                    //HAVING has something to hang on (and the later ParseSelectQuery check sees the aggregates absorbed)
                    case "HAVING":
                        if (groupByModifier == null && pendingAggregators.Count > 0)
                        {
                            groupByModifier = new RDFGroupByModifier();
                            AbsorbPendingAggregators(groupByModifier, pendingAggregators);
                            addModifier(groupByModifier);
                        }
                        ParseHavingClause(parserContext, groupByModifier);
                        break;

                    //ORDER BY clause: consume 'BY' and the sequence of order conditions
                    case "ORDER":
                        ParseOrderByModifier(parserContext, addModifier);
                        break;

                    //LIMIT n: parse the non-negative integer and add a RDFLimitModifier
                    case "LIMIT":
                        addModifier(new RDFLimitModifier(ParseInteger(parserContext, "LIMIT")));
                        break;

                    //OFFSET n: parse the non-negative integer and add a RDFOffsetModifier
                    case "OFFSET":
                        addModifier(new RDFOffsetModifier(ParseInteger(parserContext, "OFFSET")));
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
        /// <see cref="ParseSolutionModifiers"/>) and attaches the resulting ordering modifiers via
        /// <paramref name="addModifier"/>.
        /// </para>
        /// <para>
        /// SPARQL grammar:
        /// <code>
        /// OrderClause ::= 'ORDER' 'BY' OrderCondition+
        /// OrderCondition ::= ( ( 'ASC' | 'DESC' ) '(' Expression ')' )
        ///                  | ( Constraint | Var )
        /// </code>
        /// The full grammar is supported: an <see cref="RDFOrderByModifier"/> orders by a single ordering key, an
        /// <see cref="RDFExpression"/> — a bare variable being just its simplest form. So both expression conditions
        /// (e.g. <c>ORDER BY STRLEN(?label)</c>, <c>ASC(?x + 1)</c>) and bare variables are accepted; a bare
        /// condition (variable or expression) implies ASC. At least one order condition is required after BY;
        /// anything that is not a recognised condition (e.g. the LIMIT keyword) is pushed back so
        /// <see cref="ParseSolutionModifiers"/> can process it.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When 'BY' is missing or no order condition is found.</exception>
        private static void ParseOrderByModifier(RDFQueryParserContext parserContext, Action<RDFModifier> addModifier)
        {
            //The 'BY' keyword is mandatory and must immediately follow 'ORDER'
            if (!TryConsumeKeyword(parserContext, "BY"))
                throw new RDFQueryException("Cannot parse SPARQL ORDER BY clause: expected 'BY' after 'ORDER' " + GetCoordinates(parserContext));

            bool foundAtLeastOneOrderCondition = false;

            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);

                //A '?'/'$' (variable) or '(' (bracketted expression) starts a bare order condition: ascending by default
                if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$' || nextSignificantCodePoint == '(')
                {
                    addModifier(new RDFOrderByModifier(
                        ParseExpression(parserContext), RDFQueryEnums.RDFOrderByFlavors.ASC));
                    foundAtLeastOneOrderCondition = true;
                    continue;
                }

                //Otherwise read a keyword: it is either an ASC/DESC directive, a built-in/function call used as a
                //bare condition (keyword immediately followed by '('), or a terminator (LIMIT/OFFSET/EOF/...)
                string directionKeyword = ReadKeyword(parserContext);
                string normalizedDirectionKeyword = directionKeyword.ToUpperInvariant();

                if (normalizedDirectionKeyword == "ASC" || normalizedDirectionKeyword == "DESC")
                {
                    //Both directives wrap a single expression argument in parentheses (a bare variable being its
                    //simplest form), so reuse the expression grammar verbatim
                    ExpectChar(parserContext, '(', "ORDER BY condition");
                    RDFExpression orderingExpression = ParseExpression(parserContext);
                    ExpectChar(parserContext, ')', "ORDER BY condition");

                    //Map the direction keyword to the corresponding enum value
                    RDFQueryEnums.RDFOrderByFlavors orderingDirection = normalizedDirectionKeyword == "ASC"
                        ? RDFQueryEnums.RDFOrderByFlavors.ASC
                        : RDFQueryEnums.RDFOrderByFlavors.DESC;

                    addModifier(new RDFOrderByModifier(orderingExpression, orderingDirection));
                    foundAtLeastOneOrderCondition = true;
                    continue;
                }

                //A non-directive keyword used as a bare condition: either a built-in/function call immediately
                //followed by '(' (e.g. ORDER BY STRLEN(?x)) or an EXISTS / NOT EXISTS graph-pattern test (whose
                //operand is a '{ … }' group). Push the keyword back and let the expression grammar parse the whole.
                if (directionKeyword.Length > 0
                     && (SkipWhitespace(parserContext) == '(' || normalizedDirectionKeyword == "EXISTS" || normalizedDirectionKeyword == "NOT"))
                {
                    UnreadString(parserContext, directionKeyword);
                    addModifier(new RDFOrderByModifier(
                        ParseExpression(parserContext), RDFQueryEnums.RDFOrderByFlavors.ASC));
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
                throw new RDFQueryException("Cannot parse SPARQL ORDER BY clause: expected at least one condition to order by " + GetCoordinates(parserContext));
        }
        #endregion
    }
}
