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
    /// ASK-form half of the SPARQL parser: the body of an ASK query — its WHERE clause plus any trailing solution
    /// modifiers and inline data — assembled into an <see cref="RDFAskQuery"/> once the 'ASK' keyword has been
    /// dispatched. ASK asks whether the (modified) solution sequence has at least one solution.
    /// <para>
    /// SPARQL grammar: <c>AskQuery ::= 'ASK' DatasetClause* WhereClause SolutionModifier</c>, followed by the
    /// query-level <c>ValuesClause</c> ([4] Query). The solution modifiers do affect the boolean answer (e.g.
    /// <c>LIMIT 0</c>, an OFFSET past the last row, or a HAVING that drops every group all make ASK false), so they
    /// are parsed and applied rather than rejected.
    /// </para>
    /// <para>
    /// Model-imposed limit: a <c>DatasetClause</c> (FROM / FROM NAMED) is spec-legal but NOT representable (RDFSharp
    /// is an evaluation engine over a given data source, not an endpoint), so it raises an explicit
    /// <see cref="RDFQueryException"/> rather than being silently dropped.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region AskQuery
        /// <summary>
        /// Parses the body of an ASK query (the 'ASK' keyword has already been consumed by the dispatcher): the
        /// optional dataset clauses (rejected as non-representable), the WHERE clause, the trailing solution modifiers
        /// (GROUP BY / HAVING / ORDER BY / LIMIT / OFFSET) and the trailing query-level VALUES. The prologue's
        /// declared prefixes are re-attached so the query re-serializes its prologue identically.
        /// </summary>
        /// <exception cref="RDFQueryException">When a DatasetClause is present, the WHERE clause is malformed, or unexpected content trails the query.</exception>
        private static RDFAskQuery ParseAskQuery(RDFQueryParserContext parserContext)
        {
            RDFAskQuery askQuery = new RDFAskQuery();

            //Carry the prologue's declared prefixes onto the query so its printed prologue matches the input
            ApplyDeclaredPrefixes(parserContext, askQuery);

            //DatasetClause* (FROM / FROM NAMED): spec-legal but the flat model has no dataset to bind them to
            RejectDatasetClause(parserContext);

            //WHERE clause (the keyword itself is optional in SPARQL)
            ParseWhereClause(parserContext, askQuery);

            //SolutionModifier: GROUP BY / HAVING / ORDER BY / LIMIT / OFFSET. ASK has no projection, so there are no
            //pending projection aggregates (HAVING may still introduce hidden ones); they shape the solution sequence
            //before the existence check.
            ParseSolutionModifiers(parserContext, modifier => askQuery.AddModifier(modifier), new List<RDFAggregator>());

            //Trailing ValuesClause (Query ::= ... AskQuery ValuesClause): a query-level VALUES joined with the whole
            //WHERE solution sequence
            if (TryConsumeKeyword(parserContext, "VALUES"))
                askQuery.SetValues(ParseDataBlock(parserContext));

            //Require end-of-input: anything left over (after modifiers and VALUES were consumed) is genuinely
            //unexpected, and ParseQuery does not enforce end-of-input itself, so reject it rather than drop it silently
            RejectTrailingContent(parserContext);

            return askQuery;
        }

        /// <summary>
        /// Rejects a leading <c>DatasetClause</c> (FROM / FROM NAMED) — the clause between the SELECT/ASK header
        /// and the WHERE clause. It is deliberately NOT supported: FROM/FROM NAMED instruct a SPARQL endpoint to
        /// procure (dereference/compose) its dataset on-the-fly, whereas RDFSharp is the evaluation engine over the
        /// data source you provide (graph/store/federation), not an endpoint. The equivalent scoping is expressed
        /// with GRAPH patterns over a store, so the clause is redundant here.
        /// </summary>
        /// <exception cref="RDFQueryException">When the next token is the FROM keyword.</exception>
        private static void RejectDatasetClause(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);
            string upcomingKeyword = ReadKeyword(parserContext);
            UnreadString(parserContext, upcomingKeyword);

            if (upcomingKeyword.Equals("FROM", StringComparison.OrdinalIgnoreCase))
                throw new RDFQueryException("Cannot parse SPARQL query: a dataset clause (FROM / FROM NAMED) is not supported. These clauses tell a SPARQL endpoint to procure its dataset on-the-fly, whereas RDFSharp evaluates queries over the data source you provide (graph/store/federation); use GRAPH patterns over a store to scope named graphs instead " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Requires end-of-input after an ASK query's WHERE clause, solution modifiers and trailing VALUES have all
        /// been consumed. Anything still left is genuinely unexpected; rejecting it (rather than relying on a fixed
        /// keyword list) avoids silently dropping it, since <see cref="ParseQuery"/> does not enforce end-of-input itself.
        /// </summary>
        /// <exception cref="RDFQueryException">When any significant content remains at the end of the ASK query.</exception>
        private static void RejectTrailingContent(RDFQueryParserContext parserContext)
        {
            if (SkipWhitespace(parserContext) != -1)
                throw new RDFQueryException("Cannot parse SPARQL ASK query: unexpected trailing content at the end of the query " + GetCoordinates(parserContext));
        }
        #endregion
    }
}