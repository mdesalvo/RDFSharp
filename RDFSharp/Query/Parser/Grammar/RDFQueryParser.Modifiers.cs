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
        /// modifier objects to <paramref name="selectQuery"/>. Scanning stops as soon as a token that is
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
        private static void ParseSolutionModifiers(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery, List<RDFAggregator> pendingAggregators)
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
                        groupByModifier = ParseGroupByModifier(parserContext, selectQuery, pendingAggregators);
                        break;

                    //HAVING clause: attach its conditions, as having-clauses, to the grouping modifier's aggregators
                    case "HAVING":
                        ParseHavingClause(parserContext, groupByModifier);
                        break;

                    //ORDER BY clause: consume 'BY' and the sequence of order conditions
                    case "ORDER":
                        ParseOrderByModifier(parserContext, selectQuery);
                        break;

                    //LIMIT n: parse the non-negative integer and add a RDFLimitModifier
                    case "LIMIT":
                        selectQuery.AddModifier(new RDFLimitModifier(ParseInteger(parserContext, "LIMIT")));
                        break;

                    //OFFSET n: parse the non-negative integer and add a RDFOffsetModifier
                    case "OFFSET":
                        selectQuery.AddModifier(new RDFOffsetModifier(ParseInteger(parserContext, "OFFSET")));
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
        /// <see cref="ParseSolutionModifiers"/>) and attaches the resulting ordering modifiers to
        /// <paramref name="selectQuery"/>.
        /// </para>
        /// <para>
        /// SPARQL grammar:
        /// <code>
        /// OrderClause ::= 'ORDER' 'BY' OrderCondition+
        /// OrderCondition ::= ( ( 'ASC' | 'DESC' ) '(' Expression ')' )
        ///                  | ( Constraint | Var )
        /// </code>
        /// Only variable-based conditions are representable: an <see cref="RDFOrderByModifier"/> orders by a
        /// single variable, so expression conditions (e.g. <c>ORDER BY STRLEN(?label)</c>) are a known limit
        /// (not representable). ASC and DESC directives are accepted with a variable argument. A bare variable implies ASC.
        /// At least one order condition is required after BY; anything that is not a recognised
        /// condition (e.g. the LIMIT keyword) is pushed back so <see cref="ParseSolutionModifiers"/>
        /// can process it.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When 'BY' is missing or no order condition is found.</exception>
        private static void ParseOrderByModifier(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            //The 'BY' keyword is mandatory and must immediately follow 'ORDER'
            if (!TryConsumeKeyword(parserContext, "BY"))
                throw new RDFQueryException("Cannot parse SPARQL ORDER BY clause: expected 'BY' after 'ORDER' " + GetCoordinates(parserContext));

            bool foundAtLeastOneOrderCondition = false;

            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);

                //A '?' or '$' sigil starts a bare variable order condition: ascending by default
                if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                {
                    selectQuery.AddModifier(new RDFOrderByModifier(
                        ParseVariable(parserContext), RDFQueryEnums.RDFOrderByFlavors.ASC));
                    foundAtLeastOneOrderCondition = true;
                    continue;
                }

                //Otherwise try to read an ASC(...) or DESC(...) directive
                string directionKeyword = ReadKeyword(parserContext);
                string normalizedDirectionKeyword = directionKeyword.ToUpperInvariant();

                if (normalizedDirectionKeyword == "ASC" || normalizedDirectionKeyword == "DESC")
                {
                    //Both directives require a single variable argument wrapped in parentheses
                    ExpectChar(parserContext, '(', "ORDER BY condition");
                    RDFVariable orderingVariable = ParseVariable(parserContext);
                    ExpectChar(parserContext, ')', "ORDER BY condition");

                    //Map the direction keyword to the corresponding enum value
                    RDFQueryEnums.RDFOrderByFlavors orderingDirection = normalizedDirectionKeyword == "ASC"
                        ? RDFQueryEnums.RDFOrderByFlavors.ASC
                        : RDFQueryEnums.RDFOrderByFlavors.DESC;

                    selectQuery.AddModifier(new RDFOrderByModifier(orderingVariable, orderingDirection));
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
                throw new RDFQueryException("Cannot parse SPARQL ORDER BY clause: expected at least one variable to order by " + GetCoordinates(parserContext));
        }
        #endregion
    }
}
