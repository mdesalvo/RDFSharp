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
using RDFQueryParserContext = RDFSharp.Query.RDFQueryParser.RDFQueryParserContext;

namespace RDFSharp.Query
{
    /// <summary>
    /// Modify/WHERE half of the SPARQL UPDATE parser: the template-plus-WHERE operations that delete and/or insert
    /// the bindings produced by a WHERE clause. Three model classes are produced: <see cref="RDFDeleteWhereOperation"/>
    /// (DELETE-only), <see cref="RDFInsertWhereOperation"/> (INSERT-only), and
    /// <see cref="RDFDeleteInsertWhereOperation"/> (both).
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// DeleteWhere  ::= 'DELETE WHERE' QuadPattern
    /// Modify       ::= ('WITH' iri)? ( DeleteClause InsertClause? | InsertClause ) UsingClause* 'WHERE' GroupGraphPattern
    /// DeleteClause ::= 'DELETE' QuadPattern
    /// InsertClause ::= 'INSERT' QuadPattern
    /// UsingClause  ::= 'USING' ( 'NAMED'? iri )
    /// </code>
    /// </para>
    /// <para>
    /// The QuadPattern templates (which, unlike QuadData, may carry variables) are read with the shared
    /// <see cref="ParseQuadsTemplate"/> and attached through the model's NON-ground <c>AddDelete/InsertTemplate</c>;
    /// the WHERE clause reuses the full query <see cref="RDFQueryParser.ParseWhereClause{TQuery}"/>. Model-imposed
    /// limit: <c>WITH</c> and <c>USING [NAMED]</c> address a dataset the flat operation model has no slot for, so
    /// both are rejected as non-representable.
    /// </para>
    /// </summary>
    internal static partial class RDFOperationParser
    {
        #region Modify
        /// <summary>
        /// Parses an INSERT-only Modify operation (the 'INSERT' keyword has already been consumed and is known NOT
        /// to be 'INSERT DATA'): <c>INSERT QuadPattern UsingClause* 'WHERE' GroupGraphPattern</c>.
        /// </summary>
        /// <exception cref="RDFQueryException">When the template/WHERE is malformed, or a USING clause is present.</exception>
        private static RDFInsertWhereOperation ParseInsertWhereOperation(RDFQueryParserContext parserContext)
        {
            RDFInsertWhereOperation insertWhereOperation = new RDFInsertWhereOperation();
            RDFQueryParser.ApplyDeclaredPrefixes(parserContext, insertWhereOperation);

            //INSERT template (QuadPattern: triples/quads, variables allowed)
            foreach (RDFPattern insertTemplate in ParseQuadsTemplate(parserContext, "INSERT"))
                insertWhereOperation.AddInsertTemplate(insertTemplate);

            //UsingClause* is not representable, then the mandatory WHERE GroupGraphPattern
            RejectUsingClause(parserContext);
            RDFQueryParser.ParseWhereClause(parserContext, insertWhereOperation);

            return insertWhereOperation;
        }

        /// <summary>
        /// Parses a DELETE-led Modify operation (the 'DELETE' keyword has already been consumed and is known NOT to
        /// be 'DELETE DATA'). Handles all three shapes:
        /// <list type="bullet">
        /// <item><c>DELETE WHERE QuadPattern</c> — the short form: the template IS the WHERE pattern.</item>
        /// <item><c>DELETE QuadPattern UsingClause* 'WHERE' GroupGraphPattern</c> — delete-only Modify.</item>
        /// <item><c>DELETE QuadPattern INSERT QuadPattern UsingClause* 'WHERE' GroupGraphPattern</c> — delete+insert.</item>
        /// </list>
        /// </summary>
        /// <exception cref="RDFQueryException">When the template/WHERE is malformed, or a USING clause is present.</exception>
        private static RDFOperation ParseDeleteWhereOperation(RDFQueryParserContext parserContext)
        {
            //DELETE WHERE QuadPattern — the short form, where the template doubles as the WHERE pattern
            if (TryConsumeKeyword(parserContext, "WHERE"))
                return ParseDeleteWhereShortForm(parserContext);

            //Otherwise a DELETE QuadPattern follows (delete-only or delete+insert Modify)
            List<RDFPattern> deleteTemplates = ParseQuadsTemplate(parserContext, "DELETE");

            //An INSERT clause after the DELETE template makes it the combined DELETE/INSERT … WHERE form
            if (TryConsumeKeyword(parserContext, "INSERT"))
            {
                RDFDeleteInsertWhereOperation deleteInsertWhereOperation = new RDFDeleteInsertWhereOperation();
                RDFQueryParser.ApplyDeclaredPrefixes(parserContext, deleteInsertWhereOperation);

                foreach (RDFPattern deleteTemplate in deleteTemplates)
                    deleteInsertWhereOperation.AddDeleteTemplate(deleteTemplate);
                foreach (RDFPattern insertTemplate in ParseQuadsTemplate(parserContext, "INSERT"))
                    deleteInsertWhereOperation.AddInsertTemplate(insertTemplate);

                RejectUsingClause(parserContext);
                RDFQueryParser.ParseWhereClause(parserContext, deleteInsertWhereOperation);

                return deleteInsertWhereOperation;
            }

            //Delete-only Modify: DELETE QuadPattern UsingClause* 'WHERE' GroupGraphPattern
            RDFDeleteWhereOperation deleteWhereOperation = new RDFDeleteWhereOperation();
            RDFQueryParser.ApplyDeclaredPrefixes(parserContext, deleteWhereOperation);

            foreach (RDFPattern deleteTemplate in deleteTemplates)
                deleteWhereOperation.AddDeleteTemplate(deleteTemplate);

            RejectUsingClause(parserContext);
            RDFQueryParser.ParseWhereClause(parserContext, deleteWhereOperation);

            return deleteWhereOperation;
        }

        /// <summary>
        /// Parses the short form <c>DELETE WHERE QuadPattern</c> (the 'DELETE' 'WHERE' keywords have already been
        /// consumed): the QuadPattern triples serve BOTH as the delete templates and as the WHERE pattern. Ground
        /// triples are naturally dropped from the WHERE group by <see cref="RDFPatternGroup.AddPattern"/> (which
        /// keeps only variable-bearing patterns), the correct behaviour for a graph pattern.
        /// </summary>
        /// <exception cref="RDFQueryException">When the QuadPattern is malformed or contains a property path.</exception>
        private static RDFDeleteWhereOperation ParseDeleteWhereShortForm(RDFQueryParserContext parserContext)
        {
            RDFDeleteWhereOperation deleteWhereOperation = new RDFDeleteWhereOperation();
            RDFQueryParser.ApplyDeclaredPrefixes(parserContext, deleteWhereOperation);

            //Read the QuadPattern once: the same triples are both the delete templates and the WHERE body
            RDFPatternGroup whereClausePatternGroup = new RDFPatternGroup();
            foreach (RDFPattern templatePattern in ParseQuadsTemplate(parserContext, "DELETE WHERE"))
            {
                deleteWhereOperation.AddDeleteTemplate(templatePattern);
                whereClausePatternGroup.AddPattern(templatePattern);
            }
            deleteWhereOperation.AddPatternGroup(whereClausePatternGroup);

            return deleteWhereOperation;
        }

        /// <summary>
        /// Rejects a <c>UsingClause</c> (<c>USING [NAMED] iri</c>): deliberately NOT supported, because USING (like
        /// FROM/WITH) tells a SPARQL endpoint which dataset the operation's WHERE should match, whereas RDFSharp
        /// evaluates over the source you provide; the same scoping is expressed with GRAPH patterns. The keyword is
        /// peeked and pushed back before throwing.
        /// </summary>
        /// <exception cref="RDFQueryException">When the next token is the USING keyword.</exception>
        private static void RejectUsingClause(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);
            string upcomingKeyword = ReadKeyword(parserContext);
            UnreadString(parserContext, upcomingKeyword);

            if (upcomingKeyword.Equals("USING", System.StringComparison.OrdinalIgnoreCase))
                throw new RDFQueryException("Cannot parse SPARQL UPDATE operation: a USING clause (USING / USING NAMED) is not supported. USING tells a SPARQL endpoint which dataset the WHERE should match, whereas RDFSharp evaluates over the data source you provide; use GRAPH patterns to scope named graphs instead " + GetCoordinates(parserContext));
        }
        #endregion
    }
}
