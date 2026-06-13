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
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// ASK-form half of the SPARQL parser: the body of an ASK query — its WHERE clause — assembled into an
    /// <see cref="RDFAskQuery"/> once the 'ASK' keyword has been dispatched. ASK asks whether the pattern has at
    /// least one solution, so the model carries no projection and no solution modifiers: it is just the prologue
    /// prefixes plus the WHERE graph pattern.
    /// <para>
    /// SPARQL grammar: <c>AskQuery ::= 'ASK' DatasetClause* WhereClause SolutionModifier</c>.
    /// </para>
    /// <para>
    /// Model-imposed limits (the flat <see cref="RDFAskQuery"/> has no dataset and no modifier slots): a
    /// <c>DatasetClause</c> (FROM / FROM NAMED) and a trailing <c>SolutionModifier</c> (GROUP BY / HAVING /
    /// ORDER BY / LIMIT / OFFSET) are spec-legal but NOT representable, so each raises an explicit
    /// <see cref="RDFQueryException"/> rather than being silently dropped.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region AskQuery
        /// <summary>
        /// Parses the body of an ASK query (the 'ASK' keyword has already been consumed by the dispatcher): the
        /// optional dataset clauses (rejected as non-representable), the WHERE clause, and any trailing solution
        /// modifier (also rejected). The prologue's declared prefixes are re-attached so the query re-serializes
        /// its prologue identically.
        /// </summary>
        /// <exception cref="RDFQueryException">When a DatasetClause or a SolutionModifier is present, or the WHERE clause is malformed.</exception>
        private static RDFAskQuery ParseAskQuery(RDFQueryParserContext parserContext)
        {
            RDFAskQuery askQuery = new RDFAskQuery();

            //Carry the prologue's declared prefixes onto the query so its printed prologue matches the input
            ApplyDeclaredPrefixes(parserContext, askQuery);

            //DatasetClause* (FROM / FROM NAMED): spec-legal but the flat model has no dataset to bind them to
            RejectDatasetClause(parserContext);

            //WHERE clause (the keyword itself is optional in SPARQL)
            ParseWhereClause(parserContext, askQuery);

            //Nothing representable can follow an ASK's WHERE clause (a SolutionModifier or a trailing VALUES are
            //both non-representable), so require end-of-input rather than silently dropping whatever trails
            RejectTrailingContent(parserContext);

            return askQuery;
        }

        /// <summary>
        /// Rejects a leading <c>DatasetClause</c> (FROM / FROM NAMED): the flat model has no dataset to attach it
        /// to, so it is not representable. The keyword run is peeked and pushed back before throwing.
        /// </summary>
        /// <exception cref="RDFQueryException">When the next token is the FROM keyword.</exception>
        private static void RejectDatasetClause(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);
            string upcomingKeyword = ReadKeyword(parserContext);
            UnreadString(parserContext, upcomingKeyword);

            if (upcomingKeyword.Equals("FROM", StringComparison.OrdinalIgnoreCase))
                throw new RDFQueryException("Cannot parse SPARQL ASK query: a dataset clause (FROM / FROM NAMED) is not representable by the flat model " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Requires end-of-input after an ASK's WHERE clause. Whatever could legally follow it — a
        /// <c>SolutionModifier</c> (GROUP BY / HAVING / ORDER BY / LIMIT / OFFSET) or a trailing
        /// <c>ValuesClause</c> — is not representable by the flat model; rejecting any leftover content (rather
        /// than relying on a fixed keyword list) also avoids silently dropping it, since <see cref="ParseQuery"/>
        /// does not enforce end-of-input itself.
        /// </summary>
        /// <exception cref="RDFQueryException">When any significant content remains after the WHERE clause.</exception>
        private static void RejectTrailingContent(RDFQueryParserContext parserContext)
        {
            if (SkipWhitespace(parserContext) != -1)
                throw new RDFQueryException("Cannot parse SPARQL ASK query: unexpected trailing content after the WHERE clause (solution modifiers and VALUES clauses are not representable on an ASK query) " + GetCoordinates(parserContext));
        }
        #endregion
    }
}