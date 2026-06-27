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
using System.Linq;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// BIND / inline-data (VALUES) half of the SPARQL parser. Both are <see cref="RDFPatternGroupMember"/>s:
    /// they do not produce a sibling algebra member but are absorbed into the surrounding pattern group, exactly
    /// like FILTER. <c>BIND(expr AS ?v)</c> attaches a computed binding; <c>VALUES</c> attaches an inline data
    /// table (single-variable compact form, or multi-variable extended form, with the <c>UNDEF</c> placeholder).
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region Bind
        /// <summary>
        /// Parses a <c>BIND(expression AS ?var)</c> assignment and attaches it to the target pattern group as an
        /// <see cref="RDFBind"/> member: the engine evaluates the expression per-solution and binds its value to the
        /// variable, extending the group's solution mapping. SPARQL grammar: <c>Bind ::= 'BIND' '(' Expression 'AS' Var ')'</c>.
        /// The full expression grammar (boolean / comparison / arithmetic / built-ins / GeoSPARQL) is reused verbatim
        /// via <see cref="ParseExpression"/>, sharing the very same machinery as FILTER and the SELECT projection.
        /// </summary>
        /// <exception cref="RDFQueryException">When the parentheses, the mandatory 'AS' keyword, or the result variable are missing/malformed.</exception>
        private static void ParseBind(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup)
        {
            //Opening parenthesis of the BIND assignment
            ExpectChar(parserContext, '(', "BIND");

            //The value-expression to compute: the same expression grammar used by FILTER and the SELECT projection
            RDFExpression bindExpression = ParseExpression(parserContext);

            //The mandatory 'AS' keyword separating the expression from the variable it binds
            if (!TryConsumeKeyword(parserContext, "AS"))
                throw new RDFQueryException("Cannot parse SPARQL BIND: expected 'AS' inside 'BIND(expr AS ?var)' " + GetCoordinates(parserContext));

            //The result variable: it must be introduced by a '?'/'$' sigil
            int sigilCodePoint = SkipWhitespace(parserContext);
            if (sigilCodePoint != '?' && sigilCodePoint != '$')
                throw new RDFQueryException("Cannot parse SPARQL BIND: expected a variable after 'AS' inside 'BIND(expr AS ?var)' " + GetCoordinates(parserContext));
            RDFVariable bindVariable = ParseVariable(parserContext);

            //Closing parenthesis of the BIND assignment
            ExpectChar(parserContext, ')', "BIND");

            //Attach the computed binding to the surrounding pattern group
            targetPatternGroup.AddBind(new RDFBind(bindExpression, bindVariable));
        }
        #endregion

        #region Values
        /// <summary>
        /// Parses a <c>VALUES</c> inline-data block and attaches it to the target pattern group as an
        /// <see cref="RDFValues"/> member. SPARQL grammar:
        /// <code>
        /// InlineData       ::= 'VALUES' DataBlock
        /// DataBlock        ::= InlineDataOneVar | InlineDataFull
        /// InlineDataOneVar ::= Var '{' DataBlockValue* '}'
        /// InlineDataFull   ::= ( NIL | '(' Var* ')' ) '{' ( '(' DataBlockValue* ')' | NIL )* '}'
        /// DataBlockValue   ::= iri | RDFLiteral | NumericLiteral | BooleanLiteral | 'UNDEF'
        /// </code>
        /// (the <c>'VALUES'</c> keyword has already been consumed by the dispatcher). The compact one-variable form
        /// and the extended multi-variable form share the same column-oriented <see cref="RDFValues"/> model.
        /// </summary>
        /// <exception cref="RDFQueryException">When the block is malformed or a row's arity does not match the variable list.</exception>
        private static void ParseValues(RDFQueryParserContext parserContext, RDFPatternGroup targetPatternGroup)
            => targetPatternGroup.AddValues(ParseDataBlock(parserContext));

        /// <summary>
        /// Parses a standalone <c>DataBlock</c> (the part of <c>VALUES</c> after the keyword) and returns it as a
        /// freestanding <see cref="RDFValues"/>, without attaching it anywhere. This is the shared entry point used
        /// both by an in-pattern-group VALUES (via <see cref="ParseValues"/>) and by the trailing query-level
        /// ValuesClause (<c>SELECT ... WHERE { ... } VALUES ...</c>, parsed in ParseSelectQuery).
        /// </summary>
        /// <exception cref="RDFQueryException">When the block is malformed or a row's arity does not match the variable list.</exception>
        private static RDFValues ParseDataBlock(RDFQueryParserContext parserContext)
        {
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //InlineDataOneVar: a single bare variable followed by a brace-delimited list of plain values
            if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                return ParseInlineDataOneVar(parserContext);

            //InlineDataFull: a parenthesised variable list followed by a brace-delimited list of parenthesised rows
            if (nextSignificantCodePoint == '(')
                return ParseInlineDataFull(parserContext);

            throw new RDFQueryException("Cannot parse SPARQL VALUES: expected a variable or '(' after 'VALUES' " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Parses the compact one-variable inline-data form <c>Var '{' DataBlockValue* '}'</c> and returns the
        /// resulting single-column <see cref="RDFValues"/>. Each value becomes one row of that single column; an
        /// <c>UNDEF</c> token becomes a null (unbound) binding. An empty value list (<c>VALUES ?v { }</c>) yields a
        /// declared-but-empty column (zero rows): a legal SPARQL block whose meaning is "zero solutions".
        /// </summary>
        private static RDFValues ParseInlineDataOneVar(RDFQueryParserContext parserContext)
        {
            //The single variable whose column this block fills
            RDFVariable valuesVariable = ParseVariable(parserContext);

            //Opening brace of the value list
            ExpectChar(parserContext, '{', "VALUES");

            //Read the flat sequence of data-block values up to the closing brace
            List<RDFPatternMember> columnBindings = new List<RDFPatternMember>();
            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);
                if (nextSignificantCodePoint == '}')
                {
                    ReadCodePoint(parserContext);
                    break;
                }
                if (nextSignificantCodePoint == -1)
                    throw new RDFQueryException("Cannot parse SPARQL VALUES: unterminated data block (missing '}') " + GetCoordinates(parserContext));

                columnBindings.Add(ParseDataBlockValue(parserContext));
            }

            //Build the single-column inline data (an empty column list means zero rows: AddColumn keeps it empty)
            return new RDFValues().AddColumn(valuesVariable, columnBindings);
        }

        /// <summary>
        /// Parses the extended multi-variable inline-data form
        /// <c>'(' Var* ')' '{' ( '(' DataBlockValue* ')' | NIL )* '}'</c> and returns the resulting
        /// <see cref="RDFValues"/>. Rows are read tuple-by-tuple and then transposed into the column-oriented model
        /// (one <see cref="RDFValues.AddColumn"/> call per declared variable); each row's arity must equal the
        /// number of declared variables, and <c>UNDEF</c> tokens become null bindings. The variable list may be NIL
        /// (<c>VALUES () { ... }</c>): with zero variables the rows are empty-domain identity mappings, whose count
        /// is carried explicitly (no column to derive it from). An empty row list yields a "zero solutions" block.
        /// </summary>
        private static RDFValues ParseInlineDataFull(RDFQueryParserContext parserContext)
        {
            //Opening parenthesis of the variable list
            ExpectChar(parserContext, '(', "VALUES");

            //Read the parenthesised variable list (it may legally be NIL, i.e. zero variables)
            List<RDFVariable> valuesVariables = new List<RDFVariable>();
            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);
                if (nextSignificantCodePoint == ')')
                {
                    ReadCodePoint(parserContext);
                    break;
                }
                if (nextSignificantCodePoint == '?' || nextSignificantCodePoint == '$')
                {
                    valuesVariables.Add(ParseVariable(parserContext));
                    continue;
                }
                throw new RDFQueryException("Cannot parse SPARQL VALUES: expected a variable or ')' in the variable list " + GetCoordinates(parserContext));
            }

            //Opening brace of the row list
            ExpectChar(parserContext, '{', "VALUES");

            //Read every parenthesised row (tuple of values) up to the closing brace
            List<List<RDFPatternMember>> dataRows = new List<List<RDFPatternMember>>();
            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);
                if (nextSignificantCodePoint == '}')
                {
                    ReadCodePoint(parserContext);
                    break;
                }
                if (nextSignificantCodePoint != '(')
                    throw new RDFQueryException("Cannot parse SPARQL VALUES: expected '(' to open a data row or '}' to close the block " + GetCoordinates(parserContext));

                dataRows.Add(ParseInlineDataRow(parserContext, valuesVariables.Count));
            }

            RDFValues inlineData = new RDFValues();

            //NIL data block ('VALUES () { () () }'): no variable to bind, so the model carries the count of the
            //empty-domain identity rows explicitly. It is evaluable even with zero rows ('VALUES () { }' means
            //"zero solutions"), so the flag is raised here rather than relying on AddColumn.
            if (valuesVariables.Count == 0)
            {
                inlineData.NilRowsCount = dataRows.Count;
                inlineData.IsEvaluable = true;
                return inlineData;
            }

            //Transpose the row-major tuples into the column-oriented RDFValues model: one column per declared
            //variable (an empty row list leaves every column empty, i.e. a "zero solutions" block)
            for (int columnIndex = 0; columnIndex < valuesVariables.Count; columnIndex++)
            {
                int capturedColumnIndex = columnIndex;
                inlineData.AddColumn(valuesVariables[columnIndex], dataRows.Select(dataRow => dataRow[capturedColumnIndex]).ToList());
            }
            return inlineData;
        }

        /// <summary>
        /// Parses a single parenthesised data row <c>'(' DataBlockValue* ')'</c> of an extended VALUES block and
        /// returns its values in declaration order (an <c>UNDEF</c> token yields a null entry). The row's arity must
        /// match <paramref name="expectedColumnCount"/>, the number of variables the block declared.
        /// </summary>
        private static List<RDFPatternMember> ParseInlineDataRow(RDFQueryParserContext parserContext, int expectedColumnCount)
        {
            //Opening parenthesis of the row
            ExpectChar(parserContext, '(', "VALUES row");

            //Read the row's values up to the closing parenthesis
            List<RDFPatternMember> rowValues = new List<RDFPatternMember>();
            while (true)
            {
                int nextSignificantCodePoint = SkipWhitespace(parserContext);
                if (nextSignificantCodePoint == ')')
                {
                    ReadCodePoint(parserContext);
                    break;
                }
                if (nextSignificantCodePoint == -1)
                    throw new RDFQueryException("Cannot parse SPARQL VALUES: unterminated data row (missing ')') " + GetCoordinates(parserContext));

                rowValues.Add(ParseDataBlockValue(parserContext));
            }

            //SPARQL requires each row's arity to equal the number of declared variables
            if (rowValues.Count != expectedColumnCount)
                throw new RDFQueryException("Cannot parse SPARQL VALUES: a data row has " + rowValues.Count + " value(s) but " + expectedColumnCount + " variable(s) were declared " + GetCoordinates(parserContext));

            return rowValues;
        }

        /// <summary>
        /// Parses a single <c>DataBlockValue</c> — an IRI, an RDF literal, a numeric/boolean literal, or the
        /// <c>UNDEF</c> placeholder. UNDEF yields <c>null</c> (the model's representation of an unbound binding);
        /// every other token is read through the shared term-reader (so prologue BASE/PREFIX resolution applies).
        /// </summary>
        private static RDFPatternMember ParseDataBlockValue(RDFQueryParserContext parserContext)
        {
            //An ASCII-letter run may be the UNDEF placeholder or a bareword/prefixed term (true/false, ex:thing, ...):
            //read it, and if it is UNDEF map it to a null binding; otherwise rewind and let the term-reader handle it.
            if (IsAsciiLetter(SkipWhitespace(parserContext)))
            {
                string letterRun = ReadKeyword(parserContext);
                if (string.Equals(letterRun, "UNDEF", System.StringComparison.OrdinalIgnoreCase))
                    return null;
                UnreadString(parserContext, letterRun);
            }

            //Any concrete term: IRI (IRIREF or prefixed name), string/numeric/boolean literal
            return ParseTerm(parserContext);
        }
        #endregion
    }
}