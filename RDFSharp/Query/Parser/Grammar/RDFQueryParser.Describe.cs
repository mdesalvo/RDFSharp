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
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// DESCRIBE-form half of the SPARQL parser: the body of a DESCRIBE query — its describe-terms (variables/IRIs
    /// or the <c>*</c> wildcard), an OPTIONAL WHERE clause, and the trailing solution modifiers — assembled into
    /// an <see cref="RDFDescribeQuery"/> once the 'DESCRIBE' keyword has been dispatched.
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// DescribeQuery ::= 'DESCRIBE' ( VarOrIri+ | '*' ) DatasetClause* WhereClause? SolutionModifier
    /// VarOrIri      ::= Var | iri
    /// </code>
    /// </para>
    /// <para>
    /// Model-imposed limits (the flat <see cref="RDFDescribeQuery"/> has no dataset and only LIMIT/OFFSET modifier
    /// slots): a <c>DatasetClause</c> (FROM / FROM NAMED) and the ORDER BY / GROUP BY / HAVING solution modifiers
    /// are spec-legal but NOT representable, so each raises an explicit <see cref="RDFQueryException"/> rather than
    /// being silently dropped.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region DescribeQuery
        /// <summary>
        /// Parses the body of a DESCRIBE query (the 'DESCRIBE' keyword has already been consumed by the
        /// dispatcher): the describe-terms (a <c>*</c> wildcard or a non-empty list of variables/IRIs), the
        /// optional dataset clauses (rejected as non-representable), the OPTIONAL WHERE clause, and the trailing
        /// solution modifiers (only LIMIT/OFFSET are representable). The prologue's declared prefixes are
        /// re-attached so the query re-serializes its prologue identically.
        /// </summary>
        /// <exception cref="RDFQueryException">When the describe-terms, WHERE clause, dataset clause or a solution modifier is malformed/non-representable.</exception>
        private static RDFDescribeQuery ParseDescribeQuery(RDFQueryParserContext parserContext)
        {
            RDFDescribeQuery describeQuery = new RDFDescribeQuery();

            //Carry the prologue's declared prefixes onto the query so its printed prologue matches the input
            ApplyDeclaredPrefixes(parserContext, describeQuery);

            //( VarOrIri+ | '*' ): the wildcard is modeled by leaving DescribeTerms empty
            ParseDescribeTerms(parserContext, describeQuery);

            //DatasetClause* (FROM / FROM NAMED): spec-legal but the flat model has no dataset to bind them to
            //(same non-representable limit as ASK/SELECT/CONSTRUCT) → reject explicitly
            RejectDatasetClause(parserContext);

            //WhereClause is OPTIONAL in DESCRIBE ('DESCRIBE <x>' with no WHERE is valid): parse it only if present
            if (IsWhereClauseAhead(parserContext))
                ParseWhereClause(parserContext, describeQuery);

            //SolutionModifier: GROUP BY / HAVING / ORDER BY / LIMIT / OFFSET, applied to the WHERE solution
            //sequence before the resources are described (SPARQL 1.1 §16.4/§18.4). No projection aggregates exist
            //on a DESCRIBE (empty pendingAggregators); aggregates may only appear inside HAVING as hidden ones.
            ParseSolutionModifiers(parserContext, modifier => describeQuery.AddModifier(modifier), new List<RDFAggregator>());

            return describeQuery;
        }

        /// <summary>
        /// Parses the describe-terms of a DESCRIBE query — either the <c>*</c> wildcard (modeled by leaving
        /// <see cref="RDFDescribeQuery.DescribeTerms"/> empty) or a non-empty list of <c>VarOrIri</c> items
        /// (variables and/or IRIs) — and attaches each item to <paramref name="describeQuery"/>.
        /// <para>
        /// DISAMBIGUATION. The term list is followed by keywords (WHERE / FROM / LIMIT / …), and a prefixed-name
        /// IRI (<c>foaf:Person</c>) starts with letters just like a keyword does. The two are told apart WITHOUT a
        /// keyword set: a letter run is a prefixed name iff the very next character is a <c>:</c> — otherwise it is
        /// a terminating keyword and ends the list.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When neither '*' nor any describe-term is present, or a term is a literal.</exception>
        private static void ParseDescribeTerms(RDFQueryParserContext parserContext, RDFDescribeQuery describeQuery)
        {
            //The '*' wildcard describes every variable bound by the WHERE clause: modeled as an empty term list
            if (TryConsumeChar(parserContext, '*'))
                return;

            bool foundAtLeastOneDescribeTerm = false;
            while (true)
            {
                int nextCodePoint = SkipWhitespace(parserContext);

                //A '?' or '$' sigil starts a variable describe-term
                if (nextCodePoint == '?' || nextCodePoint == '$')
                {
                    describeQuery.AddDescribeTerm(ParseVariable(parserContext));
                    foundAtLeastOneDescribeTerm = true;
                    continue;
                }

                //A '<' starts an IRIREF describe-term: parse it through the shared term-reader
                if (nextCodePoint == '<')
                {
                    describeQuery.AddDescribeTerm(ParseDescribeIriTerm(parserContext));
                    foundAtLeastOneDescribeTerm = true;
                    continue;
                }

                //A letter starts either a prefixed-name IRI (foaf:Person) or a terminating keyword (WHERE/FROM/…).
                //Read the letter run and disambiguate by the next character: a ':' means it is a prefixed name.
                if (IsAsciiLetter(nextCodePoint))
                {
                    string letterRun = ReadKeyword(parserContext);
                    if (PeekCodePoint(parserContext) == ':')
                    {
                        //Prefixed name: push the label back so the term-reader sees the whole 'prefix:local' token
                        UnreadString(parserContext, letterRun);
                        describeQuery.AddDescribeTerm(ParseDescribeIriTerm(parserContext));
                        foundAtLeastOneDescribeTerm = true;
                        continue;
                    }

                    //A terminating keyword (WHERE / FROM / LIMIT / …): push it back for the caller and stop
                    UnreadString(parserContext, letterRun);
                }

                //Any other character ('{', EOF, …) ends the describe-term list
                break;
            }

            //The grammar requires at least one VarOrIri when '*' was not used
            if (!foundAtLeastOneDescribeTerm)
                throw new RDFQueryException("Cannot parse SPARQL DESCRIBE query: expected '*' or at least one variable/IRI to describe " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Parses a single IRI describe-term (an IRIREF or a prefixed name resolved through the prologue) and
        /// validates it: a literal in this position is invalid, since <c>VarOrIri</c> admits only a variable or an
        /// IRI. Returns the term as an <see cref="RDFResource"/> ready for <see cref="RDFDescribeQuery.AddDescribeTerm(RDFResource)"/>.
        /// </summary>
        /// <exception cref="RDFQueryException">When the term is a literal instead of an IRI.</exception>
        private static RDFResource ParseDescribeIriTerm(RDFQueryParserContext parserContext)
        {
            RDFPatternMember describeTerm = ParseTerm(parserContext);

            //A literal can never be a describe-term (VarOrIri = Var | iri)
            if (!(describeTerm is RDFResource describeResource))
                throw new RDFQueryException("Cannot parse SPARQL DESCRIBE query: a describe-term must be a variable or an IRI, but a literal was found " + GetCoordinates(parserContext));

            return describeResource;
        }

        /// <summary>
        /// Detects whether a WHERE clause is positioned next on the reader — true when the upcoming significant
        /// token is a <c>{</c> (the WHERE keyword being optional in SPARQL) or the <c>WHERE</c> keyword itself.
        /// The reader position is left unchanged: a peeked keyword is always pushed back.
        /// </summary>
        private static bool IsWhereClauseAhead(RDFQueryParserContext parserContext)
        {
            //A bare '{' opens a WhereClause whose 'WHERE' keyword was omitted
            if (SkipWhitespace(parserContext) == '{')
                return true;

            //Otherwise the WhereClause is present iff the next keyword is 'WHERE' (peeked and pushed back)
            string upcomingKeyword = ReadKeyword(parserContext);
            UnreadString(parserContext, upcomingKeyword);
            return upcomingKeyword.ToUpperInvariant() == "WHERE";
        }
        #endregion
    }
}
