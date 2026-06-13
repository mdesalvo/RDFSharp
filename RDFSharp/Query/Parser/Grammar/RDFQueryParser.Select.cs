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

using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// SELECT-form half of the SPARQL parser: the body of a SELECT query — the optional DISTINCT/REDUCED
    /// modifier, the projection (<c>*</c> wildcard or variable list), the WHERE clause, and the trailing
    /// solution modifiers — assembled into an RDFSelectQuery once the 'SELECT' keyword has been dispatched.
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region SelectQuery
        /// <summary>
        /// Parses the body of a SELECT query (the 'SELECT' keyword has already been consumed by the dispatcher):
        /// the optional DISTINCT/REDUCED modifier, the projection (<c>*</c> or a list of variables), the WHERE
        /// clause, and the trailing solution modifiers (ORDER BY / LIMIT / OFFSET). The PREFIX declarations
        /// accumulated by the prologue are re-attached to the query so it re-serializes its prologue identically.
        /// </summary>
        /// <exception cref="RDFQueryException">When any of the SELECT sub-clauses is malformed.</exception>
        private static RDFSelectQuery ParseSelectQuery(RDFQueryParserContext parserContext)
        {
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            //Carry the prologue's declared prefixes onto the query so its printed prologue matches the input
            ApplyDeclaredPrefixes(parserContext, selectQuery);

            //DISTINCT and REDUCED are mutually exclusive: DISTINCT becomes a modifier, REDUCED is a ratified no-op
            if (TryConsumeKeyword(parserContext, "DISTINCT"))
                selectQuery.AddModifier(new RDFDistinctModifier());
            else
                TryConsumeKeyword(parserContext, "REDUCED");

            //Projection: either the '*' wildcard (empty ProjectionVars means "all variables") or a list of variables
            ParseSelectProjection(parserContext, selectQuery);

            //WHERE clause (the keyword itself is optional in SPARQL)
            ParseWhereClause(parserContext, selectQuery);

            //ORDER BY / LIMIT / OFFSET (any order, leniently)
            ParseSolutionModifiers(parserContext, selectQuery);

            return selectQuery;
        }

        /// <summary>
        /// Parses the SELECT projection: a single <c>*</c> wildcard, or a non-empty whitespace-separated list whose
        /// items are either a bare variable (<c>?v</c> / <c>$v</c>) or a computed <c>(expr AS ?var)</c> projection
        /// expression. The two item kinds may be freely interleaved, exactly as SPARQL's
        /// <c>SelectClause ::= 'SELECT' ('DISTINCT'|'REDUCED')? ( ( Var | '(' Expression 'AS' Var ')' )+ | '*' )</c> allows.
        /// </summary>
        /// <exception cref="RDFQueryException">When the projection is empty, or an '(expr AS ?var)' item is malformed.</exception>
        private static void ParseSelectProjection(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            //The '*' wildcard projects every in-scope variable: it is modeled by leaving ProjectionVars empty
            if (TryConsumeChar(parserContext, '*'))
                return;

            //Otherwise we expect one or more projection items (bare variables and/or '(expr AS ?var)' expressions)
            bool foundAtLeastOneProjectionVariable = false;
            while (true)
            {
                int nextCodePoint = SkipWhitespace(parserContext);

                //A '?' or '$' sigil starts a bare projection variable
                if (nextCodePoint == '?' || nextCodePoint == '$')
                {
                    selectQuery.AddProjectionVariable(ParseVariable(parserContext));
                    foundAtLeastOneProjectionVariable = true;
                    continue;
                }

                //A '(' opens an '(expr AS ?var)' computed projection expression: parse and attach it
                if (nextCodePoint == '(')
                {
                    ParseProjectionExpression(parserContext, selectQuery);
                    foundAtLeastOneProjectionVariable = true;
                    continue;
                }

                //Anything else (typically the WHERE keyword or '{') ends the projection list
                break;
            }

            if (!foundAtLeastOneProjectionVariable)
                throw new RDFQueryException("Cannot parse SPARQL SELECT projection: expected '*' or at least one variable " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Parses a single <c>(expr AS ?var)</c> projection expression and attaches it to the query as a COMPUTED
        /// projection variable: the result variable carries the value-expression so the engine evaluates it
        /// per-solution and the printer re-emits the very same <c>(expr AS ?var)</c> form.
        /// SPARQL grammar: <c>'(' Expression 'AS' Var ')'</c>. The full expression grammar (boolean / comparison /
        /// arithmetic / built-ins / GeoSPARQL) is reused verbatim via <see cref="ParseExpression"/>.
        /// </summary>
        /// <exception cref="RDFQueryException">When the parentheses, the mandatory 'AS' keyword, or the result variable are missing/malformed.</exception>
        private static void ParseProjectionExpression(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            //Opening parenthesis of the projection expression
            ExpectChar(parserContext, '(', "SELECT projection expression");

            //The value-expression to compute: the same expression grammar used by FILTER and BIND
            RDFExpression projectionExpression = ParseExpression(parserContext);

            //The mandatory 'AS' keyword separating the expression from the variable it binds
            if (!TryConsumeKeyword(parserContext, "AS"))
                throw new RDFQueryException("Cannot parse SPARQL SELECT projection: expected 'AS' inside '(expr AS ?var)' " + GetCoordinates(parserContext));

            //The result variable: it must be introduced by a '?'/'$' sigil
            int sigilCodePoint = SkipWhitespace(parserContext);
            if (sigilCodePoint != '?' && sigilCodePoint != '$')
                throw new RDFQueryException("Cannot parse SPARQL SELECT projection: expected a variable after 'AS' inside '(expr AS ?var)' " + GetCoordinates(parserContext));
            RDFVariable projectionVariable = ParseVariable(parserContext);

            //Closing parenthesis of the projection expression
            ExpectChar(parserContext, ')', "SELECT projection expression");

            //Attach the computed projection: the variable carries its expression so the engine evaluates it per-solution
            selectQuery.AddProjectionVariable(projectionVariable, projectionExpression);
        }
        #endregion
    }
}
