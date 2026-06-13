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
        /// Parses the SELECT projection: a single <c>*</c> wildcard, or a non-empty whitespace-separated list of
        /// variables. A leading '(' would introduce an <c>(expr AS ?var)</c> projection expression, which is not
        /// supported yet (it needs the full expression parser of a later phase) and is rejected with a clear message.
        /// </summary>
        /// <exception cref="RDFQueryException">When the projection is empty, or uses an unsupported '(expr AS ?var)' form.</exception>
        private static void ParseSelectProjection(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            //The '*' wildcard projects every in-scope variable: it is modeled by leaving ProjectionVars empty
            if (TryConsumeChar(parserContext, '*'))
                return;

            //Otherwise we expect one or more projection variables
            bool foundAtLeastOneProjectionVariable = false;
            while (true)
            {
                int nextCodePoint = SkipWhitespace(parserContext);

                //A '?' or '$' sigil starts a projection variable
                if (nextCodePoint == '?' || nextCodePoint == '$')
                {
                    selectQuery.AddProjectionVariable(ParseVariable(parserContext));
                    foundAtLeastOneProjectionVariable = true;
                    continue;
                }

                //A '(' would open an '(expr AS ?var)' projection expression: deferred to the expression phase
                if (nextCodePoint == '(')
                    throw new RDFQueryException("Cannot parse SPARQL SELECT projection: '(expr AS ?var)' projection expressions are not supported yet " + GetCoordinates(parserContext));

                //Anything else (typically the WHERE keyword or '{') ends the projection list
                break;
            }

            if (!foundAtLeastOneProjectionVariable)
                throw new RDFQueryException("Cannot parse SPARQL SELECT projection: expected '*' or at least one variable " + GetCoordinates(parserContext));
        }
        #endregion
    }
}
