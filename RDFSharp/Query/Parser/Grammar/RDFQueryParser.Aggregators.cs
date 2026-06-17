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
using System.Text;
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// Aggregate-function half of the SPARQL parser: reads a single SPARQL aggregate
    /// (<c>COUNT</c>/<c>SUM</c>/<c>MIN</c>/<c>MAX</c>/<c>AVG</c>/<c>SAMPLE</c>/<c>GROUP_CONCAT</c>) into a
    /// function-agnostic descriptor, then materializes it into a concrete <see cref="RDFAggregator"/> once the
    /// projection variable is known. Shared by the SELECT-projection path (<c>(AGG(?v) AS ?p)</c>) and by the
    /// HAVING path (which only needs to MATCH an aggregate, not project it).
    /// <para>
    /// SPARQL grammar:
    /// <code>
    /// [127] Aggregate ::= 'COUNT' '(' 'DISTINCT'? ( '*' | Expression ) ')'
    ///                   | ('SUM'|'MIN'|'MAX'|'AVG'|'SAMPLE') '(' 'DISTINCT'? Expression ')'
    ///                   | 'GROUP_CONCAT' '(' 'DISTINCT'? Expression ( ';' 'SEPARATOR' '=' String )? ')'
    /// </code>
    /// </para>
    /// <para>
    /// Model-imposed limits (the flat <see cref="RDFAggregator"/> can only aggregate over a SINGLE variable):
    /// <c>COUNT(*)</c> and any aggregate whose argument is an expression (e.g. <c>SUM(?x + ?y)</c>) are NOT
    /// representable and raise an explicit <see cref="RDFQueryException"/>. MIN/MAX are mapped to the
    /// <see cref="RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric"/> flavor: SPARQL has no syntax to choose
    /// between numeric and string ordering, so the common numeric case is the default.
    /// </para>
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region Aggregators
        /// <summary>
        /// The seven SPARQL aggregate function names (case-insensitive). Membership of this set is what
        /// distinguishes an aggregate projection <c>(COUNT(?x) AS ?c)</c> from an ordinary computed
        /// projection <c>(STRLEN(?x) AS ?c)</c> at the SELECT-projection dispatch point.
        /// </summary>
        private static readonly HashSet<string> AggregatorKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "COUNT", "SUM", "MIN", "MAX", "AVG", "SAMPLE", "GROUP_CONCAT" };

        /// <summary>
        /// A function-agnostic snapshot of a parsed aggregate: which function (the uppercased keyword, e.g.
        /// <c>"COUNT"</c>/<c>"GROUP_CONCAT"</c>), over which single variable, whether DISTINCT, and (GROUP_CONCAT
        /// only) the separator. The projection variable is intentionally absent — it is supplied later by the
        /// caller via <see cref="BuildAggregator"/> (projection path), or never (HAVING match path).
        /// </summary>
        private sealed class RDFParsedAggregator
        {
            internal string Function { get; set; }
            internal RDFVariable AggregatorVariable { get; set; }
            internal RDFExpression Expression { get; set; }
            internal bool IsDistinct { get; set; }
            internal bool IsCountAll { get; set; }
            internal string Separator { get; set; }
        }

        /// <summary>
        /// Parses a single aggregate (the reader must be positioned on the function name): the function keyword,
        /// the mandatory parentheses, an optional <c>DISTINCT</c>, the SINGLE-variable argument, and — for
        /// GROUP_CONCAT only — an optional <c>; SEPARATOR = "..."</c> clause. Returns the function-agnostic
        /// descriptor; the caller decides whether to materialize it (projection) or merely match it (HAVING).
        /// </summary>
        /// <exception cref="RDFQueryException">When the aggregate is malformed, or uses a form the flat model cannot represent (COUNT(*), expression argument).</exception>
        private static RDFParsedAggregator ParseAggregator(RDFQueryParserContext parserContext)
        {
            //Function name (letters AND underscore, so 'GROUP_CONCAT' is read whole)
            SkipWhitespace(parserContext);
            string functionKeyword = ReadAggregatorKeyword(parserContext);
            if (!AggregatorKeywords.Contains(functionKeyword))
                throw new RDFQueryException("Cannot parse SPARQL aggregate: expected an aggregate function name (COUNT/SUM/MIN/MAX/AVG/SAMPLE/GROUP_CONCAT) " + GetCoordinates(parserContext));

            RDFParsedAggregator parsedAggregator = new RDFParsedAggregator
            {
                Function = functionKeyword.ToUpperInvariant()
            };

            //Opening parenthesis of the aggregate
            ExpectChar(parserContext, '(', "aggregate");

            //Optional DISTINCT (applies to every aggregate function in SPARQL 1.1)
            parsedAggregator.IsDistinct = TryConsumeKeyword(parserContext, "DISTINCT");

            //Argument: the '*' wildcard (COUNT only), or a full expression that may reduce to a bare variable.
            int argumentCodePoint = SkipWhitespace(parserContext);
            if (argumentCodePoint == '*')
            {
                if (parsedAggregator.Function != "COUNT")
                    throw new RDFQueryException("Cannot parse SPARQL aggregate: the '*' argument is only allowed for COUNT " + GetCoordinates(parserContext));
                ReadCodePoint(parserContext); //consume '*'
                parsedAggregator.IsCountAll = true;
            }
            else
            {
                //Parse the argument as an expression: a bare variable stays a variable (aggregated directly), any
                //other expression (e.g. SUM(?x + ?y)) is materialized into a synthetic column by the GroupBy modifier
                RDFExpression argumentExpression = ParseExpression(parserContext);
                if (argumentExpression is RDFVariableExpression bareVariableExpression && bareVariableExpression.LeftArgument is RDFVariable bareVariable)
                    parsedAggregator.AggregatorVariable = bareVariable;
                else
                    parsedAggregator.Expression = argumentExpression;
            }

            //GROUP_CONCAT-only optional separator: '; SEPARATOR = String'
            if (parsedAggregator.Function == "GROUP_CONCAT" && SkipWhitespace(parserContext) == ';')
            {
                ReadCodePoint(parserContext); //consume ';'
                if (!TryConsumeKeyword(parserContext, "SEPARATOR"))
                    throw new RDFQueryException("Cannot parse SPARQL GROUP_CONCAT: expected 'SEPARATOR' after ';' " + GetCoordinates(parserContext));
                ExpectChar(parserContext, '=', "GROUP_CONCAT separator");

                //The separator is a String literal: reuse the term reader and keep its lexical value
                if (!(ParseTerm(parserContext) is RDFLiteral separatorLiteral))
                    throw new RDFQueryException("Cannot parse SPARQL GROUP_CONCAT: 'SEPARATOR' must be a string literal " + GetCoordinates(parserContext));
                parsedAggregator.Separator = separatorLiteral.Value;
            }

            //Closing parenthesis of the aggregate
            ExpectChar(parserContext, ')', "aggregate");

            return parsedAggregator;
        }

        /// <summary>
        /// Materializes a parsed aggregate into the matching concrete <see cref="RDFAggregator"/>, binding it to the
        /// given projection variable and propagating the DISTINCT flag. MIN/MAX default to the numeric flavor
        /// (SPARQL has no syntax to select the string flavor). GROUP_CONCAT carries its separator (default " ").
        /// </summary>
        private static RDFAggregator BuildAggregator(RDFParsedAggregator parsedAggregator, RDFVariable projectionVariable)
        {
            //An expression argument is aggregated over a synthetic column (materialized by the GroupBy modifier),
            //so the aggregator operates on that column while remembering the expression for printing.
            RDFVariable aggregatorVariable = parsedAggregator.Expression != null
                ? RDFAggregator.MakeExpressionVariable(projectionVariable)
                : parsedAggregator.AggregatorVariable;

            RDFAggregator aggregator;
            switch (parsedAggregator.Function)
            {
                case "COUNT":
                    aggregator = parsedAggregator.IsCountAll
                        ? new RDFCountAggregator(projectionVariable)
                        : new RDFCountAggregator(aggregatorVariable, projectionVariable);
                    break;
                case "SUM":
                    aggregator = new RDFSumAggregator(aggregatorVariable, projectionVariable);
                    break;
                case "AVG":
                    aggregator = new RDFAvgAggregator(aggregatorVariable, projectionVariable);
                    break;
                case "SAMPLE":
                    aggregator = new RDFSampleAggregator(aggregatorVariable, projectionVariable);
                    break;
                case "MIN":
                    aggregator = new RDFMinAggregator(aggregatorVariable, projectionVariable, RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric);
                    break;
                case "MAX":
                    aggregator = new RDFMaxAggregator(aggregatorVariable, projectionVariable, RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric);
                    break;
                default: //GROUP_CONCAT
                    aggregator = new RDFGroupConcatAggregator(aggregatorVariable, projectionVariable, parsedAggregator.Separator);
                    break;
            }

            //Carry the aggregated expression (when any) so the modifier materializes it and the printer re-emits it
            if (parsedAggregator.Expression != null)
                aggregator.AggregatorExpression = parsedAggregator.Expression;

            if (parsedAggregator.IsDistinct)
                aggregator.Distinct();

            return aggregator;
        }

        /// <summary>
        /// Returns true when the given (already built) aggregator is of the function kind described by the parsed
        /// descriptor AND aggregates over the same variable: the matching predicate used by HAVING to attach a
        /// having-clause to the aggregator that the SELECT projection already declared.
        /// </summary>
        private static bool MatchesAggregator(RDFAggregator aggregator, RDFParsedAggregator parsedAggregator)
        {
            //COUNT(*) has no aggregated variable: match it against the count-all aggregator declared by the projection
            if (parsedAggregator.IsCountAll)
                return aggregator is RDFCountAggregator countAggregator && countAggregator.IsCountAll;

            if (!aggregator.AggregatorVariable.Equals(parsedAggregator.AggregatorVariable))
                return false;

            switch (parsedAggregator.Function)
            {
                case "COUNT": return aggregator is RDFCountAggregator;
                case "SUM": return aggregator is RDFSumAggregator;
                case "AVG": return aggregator is RDFAvgAggregator;
                case "SAMPLE": return aggregator is RDFSampleAggregator;
                case "MIN": return aggregator is RDFMinAggregator;
                case "MAX": return aggregator is RDFMaxAggregator;
                default: return aggregator is RDFGroupConcatAggregator;
            }
        }

        /// <summary>
        /// Peeks at the upcoming token and returns the recognized aggregate keyword (uppercased) when it is one of
        /// the seven aggregate function names, restoring the reader to its prior position; returns the empty string
        /// otherwise. Used at the SELECT-projection '(' to choose between the aggregate path and the ordinary
        /// computed-projection (expression) path.
        /// </summary>
        private static string TryPeekAggregatorKeyword(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);
            string candidateKeyword = ReadAggregatorKeyword(parserContext);
            UnreadString(parserContext, candidateKeyword);
            return AggregatorKeywords.Contains(candidateKeyword) ? candidateKeyword.ToUpperInvariant() : string.Empty;
        }

        /// <summary>
        /// Reads a maximal run of ASCII letters AND underscores starting at the current position (the reader is left
        /// on the first character that is neither). Unlike <see cref="RDFQueryLexer.ReadKeyword"/> — which stops at
        /// the underscore — this lets <c>GROUP_CONCAT</c> be read as a single token. Does NOT skip leading whitespace.
        /// </summary>
        private static string ReadAggregatorKeyword(RDFQueryParserContext parserContext)
        {
            StringBuilder keywordRun = new StringBuilder();
            int codePoint = ReadCodePoint(parserContext);
            while (IsAsciiLetter(codePoint) || codePoint == '_')
            {
                //Aggregate keywords are pure ASCII (letters + the single underscore in GROUP_CONCAT), so a
                //direct cast is safe here — no supplementary-plane code points can occur in this run.
                keywordRun.Append((char)codePoint);
                codePoint = ReadCodePoint(parserContext);
            }
            UnreadCodePoint(parserContext, codePoint);
            return keywordRun.ToString();
        }
        #endregion
    }
}
