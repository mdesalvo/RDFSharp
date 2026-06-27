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

            //Aggregates encountered in the projection are parked here: they cannot be attached to their
            //RDFGroupByModifier yet (GROUP BY is parsed later, after WHERE), so a SELECT-scoped list carries
            //them across to ParseSolutionModifiers. A SELECT-local list (NOT the shared parser context) keeps
            //nested subqueries — which reuse the same context — from mixing each other's aggregates.
            List<RDFAggregator> pendingAggregators = new List<RDFAggregator>();

            //Projection: either the '*' wildcard (empty ProjectionVars means "all variables") or a list of variables
            ParseSelectProjection(parserContext, selectQuery, pendingAggregators);

            //DatasetClause* (FROM / FROM NAMED): spec-legal between projection and WHERE, but the flat model has
            //no dataset to bind them to (same non-representable limit as ASK) → reject explicitly
            RejectDatasetClause(parserContext);

            //WHERE clause (the keyword itself is optional in SPARQL)
            ParseWhereClause(parserContext, selectQuery);

            //GROUP BY / HAVING / ORDER BY / LIMIT / OFFSET (any order, leniently); GROUP BY absorbs the pending aggregates
            ParseSolutionModifiers(parserContext, modifier => selectQuery.AddModifier(modifier), pendingAggregators);

            //Aggregates left unattached mean the projection used aggregates without a GROUP BY: this is SPARQL
            //implicit grouping (a single group over the whole result set), modeled by an empty-partition GroupBy.
            if (pendingAggregators.Count > 0)
            {
                RDFGroupByModifier implicitGroupByModifier = new RDFGroupByModifier();
                AbsorbPendingAggregators(implicitGroupByModifier, pendingAggregators);
                selectQuery.AddModifier(implicitGroupByModifier);
            }

            //Trailing ValuesClause (SelectQuery ::= ... SolutionModifier ValuesClause): a query-level VALUES,
            //joined with the whole WHERE solution sequence. It is SELECT-only and applies to both the top-level
            //query and a subselect (which reuses this very method), so it is consumed here at the end.
            if (TryConsumeKeyword(parserContext, "VALUES"))
                selectQuery.SetValues(ParseDataBlock(parserContext));

            return selectQuery;
        }

        /// <summary>
        /// Parses the SELECT projection: a single <c>*</c> wildcard, or a non-empty whitespace-separated list whose
        /// items are either a bare variable (<c>?v</c> / <c>$v</c>) or a computed <c>(expr AS ?var)</c> projection
        /// expression. The two item kinds may be freely interleaved, exactly as SPARQL's
        /// <c>SelectClause ::= 'SELECT' ('DISTINCT'|'REDUCED')? ( ( Var | '(' Expression 'AS' Var ')' )+ | '*' )</c> allows.
        /// </summary>
        /// <exception cref="RDFQueryException">When the projection is empty, or an '(expr AS ?var)' item is malformed.</exception>
        private static void ParseSelectProjection(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery, List<RDFAggregator> pendingAggregators)
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

                //A '(' opens either an aggregate '(AGG(?v) AS ?var)' or an ordinary '(expr AS ?var)' computed
                //projection expression: parse and attach it (aggregates are parked, not added as projection vars)
                if (nextCodePoint == '(')
                {
                    ParseProjectionExpression(parserContext, selectQuery, pendingAggregators);
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
        private static void ParseProjectionExpression(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery, List<RDFAggregator> pendingAggregators)
        {
            //Opening parenthesis of the projection item
            ExpectChar(parserContext, '(', "SELECT projection expression");

            //Peek the first token: an aggregate function name routes to the aggregate path, anything else to the
            //ordinary computed-projection (expression) path. The peek restores the reader either way.
            RDFParsedAggregator parsedAggregator = TryPeekAggregatorKeyword(parserContext).Length > 0
                ? ParseAggregator(parserContext)
                : null;

            //The value-expression to compute (only on the non-aggregate path): the same expression grammar used by
            //FILTER and BIND. A NESTED aggregate inside it (e.g. '?x + COUNT(?y)') is resolved to a reference over a
            //hidden aggregator column, parked like any other aggregate so GROUP BY (explicit or implicit) absorbs it.
            RDFExpression projectionExpression = null;
            if (parsedAggregator == null)
            {
                int hiddenProjectionAggregatorCounter = 0;
                parserContext.AggregateExpressionSink = parsedNestedAggregate =>
                    ResolveProjectionAggregateReference(pendingAggregators, parsedNestedAggregate, ref hiddenProjectionAggregatorCounter);
                try
                {
                    projectionExpression = ParseExpression(parserContext);
                }
                finally
                {
                    parserContext.AggregateExpressionSink = null;
                }
            }

            //The mandatory 'AS' keyword separating the expression/aggregate from the variable it binds
            if (!TryConsumeKeyword(parserContext, "AS"))
                throw new RDFQueryException("Cannot parse SPARQL SELECT projection: expected 'AS' inside '(expr AS ?var)' " + GetCoordinates(parserContext));

            //The result variable: it must be introduced by a '?'/'$' sigil
            int sigilCodePoint = SkipWhitespace(parserContext);
            if (sigilCodePoint != '?' && sigilCodePoint != '$')
                throw new RDFQueryException("Cannot parse SPARQL SELECT projection: expected a variable after 'AS' inside '(expr AS ?var)' " + GetCoordinates(parserContext));
            RDFVariable projectionVariable = ParseVariable(parserContext);

            //Closing parenthesis of the projection item
            ExpectChar(parserContext, ')', "SELECT projection expression");

            if (parsedAggregator != null)
                //Aggregate projection: park the built aggregator (projVar now known); GROUP BY will absorb it.
                //It is NOT added as a projection variable — the GroupBy modifier owns the projected columns.
                pendingAggregators.Add(BuildAggregator(parsedAggregator, projectionVariable));
            else
                //Computed projection: the variable carries its expression so the engine evaluates it per-solution
                selectQuery.AddProjectionVariable(projectionVariable, projectionExpression);
        }

        /// <summary>
        /// Resolves an aggregate nested inside a projection expression (e.g. the <c>COUNT(?y)</c> in
        /// <c>?x + COUNT(?y)</c>) to a reference over a HIDDEN aggregator column: a freshly built aggregator is
        /// parked in <paramref name="pendingAggregators"/> (so GROUP BY — explicit or implicit — absorbs it) and
        /// flagged hidden, so the engine keeps its column out of the output projection while the surrounding
        /// expression reads it. The returned <see cref="RDFExpression"/> re-prints the original
        /// aggregate call instead of the synthetic column name.
        /// </summary>
        private static RDFExpression ResolveProjectionAggregateReference(List<RDFAggregator> pendingAggregators, RDFParsedAggregator parsedAggregate, ref int hiddenProjectionAggregatorCounter)
        {
            RDFAggregator hiddenAggregator = BuildAggregator(parsedAggregate, MakeHiddenProjectionVariable(hiddenProjectionAggregatorCounter++));
            hiddenAggregator.Metadata.IsHidden = true;
            pendingAggregators.Add(hiddenAggregator);
            //Reference the synthetic column it produces; the printer re-prints that column as the original aggregate call
            return new RDFVariableExpression(hiddenAggregator.Metadata.ProjectionVariable);
        }

        /// <summary>
        /// Builds the internal projection variable backing a HIDDEN projection aggregator (an aggregate nested inside
        /// a projection expression): a reserved '__PROJAGG_n' name that never surfaces as an output column.
        /// </summary>
        private static RDFVariable MakeHiddenProjectionVariable(int index)
            => new RDFVariable("?__PROJAGG_" + index);
        #endregion
    }
}
