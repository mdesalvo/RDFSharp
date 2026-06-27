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
    /// SubSelect half of the SPARQL parser: a nested SELECT query used as the sole content of a group graph
    /// pattern (<c>GroupGraphPattern ::= '{' ( SubSelect | GroupGraphPatternSub ) '}'</c>). A SubSelect becomes
    /// a subquery member (<see cref="RDFSelectQuery"/>): the engine evaluates it independently and joins its
    /// projected bindings with the sibling members of the enclosing group.
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region SubQuery
        /// <summary>
        /// <para>
        /// Parses a <c>SubSelect</c> — the nested SELECT query that may stand in for a group graph pattern's body.
        /// The <c>'SELECT'</c> keyword has been confirmed by the caller (via <see cref="PeekGraphPatternKeyword"/>)
        /// but NOT yet consumed; this method consumes it and then delegates the rest of the SELECT surface to
        /// <see cref="ParseSelectQuery"/>, so the subquery gets the full SELECT grammar for free: the
        /// DISTINCT/REDUCED modifier, the <c>*</c> / variable / <c>(expr AS ?v)</c> projection, the WHERE clause,
        /// and the trailing ORDER BY / LIMIT / OFFSET solution modifiers.
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>SubSelect ::= SelectClause WhereClause SolutionModifier ValuesClause</c>. A SubSelect
        /// carries no prologue of its own (BASE/PREFIX are top-level only), which is exactly why this method does
        /// NOT parse a prologue — it enters directly at the SELECT clause. The returned query is flagged as a
        /// subquery (<see cref="RDFSelectQuery.IsSubQuery"/>) when the caller attaches it via
        /// <see cref="RDFSelectQuery.AddSubQuery"/>, which is what makes the printer omit the prologue for it.
        /// </para>
        /// </summary>
        /// <returns>The parsed nested SELECT query, ready to be appended to the enclosing group's member list.</returns>
        /// <exception cref="RDFQueryException">When any of the nested SELECT sub-clauses is malformed.</exception>
        private static RDFSelectQuery ParseSubSelectQuery(RDFQueryParserContext parserContext)
        {
            //Consume the 'SELECT' keyword that opens the SubSelect (the caller only peeked at it)
            ConsumeKeyword(parserContext);

            //Reuse the full SELECT body parser: projection + WHERE + solution modifiers. The result is upcast to
            //a query member by the caller and marked IsSubQuery when attached through AddSubQuery.
            return ParseSelectQuery(parserContext);
        }
        #endregion
    }
}