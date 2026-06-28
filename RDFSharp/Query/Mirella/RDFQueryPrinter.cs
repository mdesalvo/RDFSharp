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
using System.Linq;
using System.Text;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryPrinter is responsible for getting string representation of SPARQL queries
    /// </summary>
    internal static class RDFQueryPrinter
    {
        #region Methods
        /// <summary>
        /// Prints the string representation of a SPARQL SELECT query
        /// </summary>
        internal static string PrintSelectQuery(RDFSelectQuery selectQuery, double indentLevel, bool fromUnionOrMinus)
        {
            StringBuilder sb = new StringBuilder();
            if (selectQuery == null)
                return sb.ToString();

            #region INDENT
            int subqueryHeaderSpacesFunc()
                => subqueryBodySpacesFunc() < 2 ? 0 : subqueryBodySpacesFunc() - 2;
            int subqueryBodySpacesFunc()
                => Convert.ToInt32(4.0d * indentLevel);
            int subqueryUnionOrMinusSpacesFunc(bool unionOrMinus)
                => unionOrMinus ? 2 : 0;

            string subquerySpaces = new string(' ', subqueryHeaderSpacesFunc() + subqueryUnionOrMinusSpacesFunc(fromUnionOrMinus));
            string subqueryBodySpaces = new string(' ', subqueryBodySpacesFunc() + subqueryUnionOrMinusSpacesFunc(fromUnionOrMinus));
            #endregion

            #region PREFIX
            List<RDFNamespace> prefixes = PrintPrefixes(selectQuery, sb, !selectQuery.IsSubQuery);
            #endregion

            #region SELECT
            if (selectQuery.IsSubQuery)
            {
                if (selectQuery.IsOptional && !fromUnionOrMinus)
                    sb.AppendLine(string.Concat(subquerySpaces, "OPTIONAL {"));
                else
                    sb.AppendLine(string.Concat(subquerySpaces, "{"));
            }
            sb.Append($"{subqueryBodySpaces}SELECT");
            #endregion

            #region DISTINCT
            List<RDFModifier> modifiers = selectQuery.GetModifiers().ToList();
            foreach (RDFDistinctModifier dm in modifiers.OfType<RDFDistinctModifier>())
                sb.Append($" {dm}");
            #endregion

            #region VARIABLES/AGGREGATORS
            //Query has GroupBy modifier => reset given projections, this modifier takes control!
            if (modifiers.Any(mod => mod is RDFGroupByModifier))
            {
                foreach (RDFGroupByModifier gm in modifiers.OfType<RDFGroupByModifier>())
                {
                    //Visible aggregate projections. Hidden aggregators only feed HAVING / projection expressions and
                    //must NOT surface as projected columns, so they are excluded here.
                    string printedAggregators = string.Join(" ", gm.ProjectableAggregators);

                    //Computed projections wrapping an aggregate live in ProjectionVars (the GroupBy modifier owns the
                    //bare aggregate columns, but '?x + COUNT(?y) AS ?v' is a per-solution expression over them); any
                    //hidden aggregate nested inside is re-printed as its original call instead of the synthetic column
                    string printedComputedProjections = string.Join(" ", selectQuery.ProjectionVars
                        .Where(pv => pv.Value.Item2 != null)
                        .OrderBy(pv => pv.Value.Item1)
                        .Select(pv => $"({gm.ReprintHiddenAggregateCalls(pv.Value.Item2.ToString(prefixes))} AS {pv.Key})"));

                    //The non-partition projections are the visible aggregates followed by the computed projections
                    string printedNonPartitionProjections = printedComputedProjections.Length == 0
                        ? printedAggregators
                        : printedAggregators.Length == 0
                            ? printedComputedProjections
                            : string.Concat(printedAggregators, " ", printedComputedProjections);

                    //Anonymous GROUP BY expression columns ('GROUP BY (expr)') are internal scratch: never projected
                    List<RDFVariable> projectablePartitionVariables = gm.PartitionConditions
                        .Where(condition => !condition.IsSynthetic).Select(condition => condition.Variable).ToList();

                    //Explicit grouping: partition variables precede the (aggregate/computed) projections
                    if (projectablePartitionVariables.Count > 0)
                    {
                        
                        sb.Append(' ');
                        sb.Append(string.Join(" ", projectablePartitionVariables));
                    }

                    //Implicit grouping (or only anonymous group expressions): just the (aggregate/computed) projections
                    sb.Append(' ');
                    sb.Append(printedNonPartitionProjections);
                }
            }
            //Query hasn't GroupBy modifier => respect given projections
            else
            {
                if (selectQuery.ProjectionVars.Count == 0)
                {
                    sb.Append(" *");
                }
                else
                {
                    foreach (KeyValuePair<RDFVariable, (int, RDFExpression)> projectionElement in selectQuery.ProjectionVars.OrderBy(pv => pv.Value.Item1))
                    {
                        sb.Append(projectionElement.Value.Item2 == null
                            //Projection Variable
                            ? $" {projectionElement.Key}"
                            //Projection Expression
                            : $" ({projectionElement.Value.Item2.ToString(prefixes)} AS {projectionElement.Key})");
                    }
                }
            }
            sb.AppendLine();
            #endregion

            #region WHERE
            PrintWhereClause(selectQuery, sb, prefixes, subqueryBodySpaces, indentLevel, fromUnionOrMinus);
            #endregion

            #region MODIFIERS
            PrintSolutionModifiers(sb, modifiers, selectQuery.Prefixes, subqueryBodySpaces);
            #endregion

            #region TRAILING VALUES
            //Query-level VALUES (SELECT ... WHERE { ... } VALUES ...): printed after the solution modifiers,
            //at the SELECT body indentation (inside the braces for a subquery, thanks to the closure below)
            PrintTrailingValues(sb, selectQuery, prefixes, subqueryBodySpaces);
            #endregion

            //CLOSURE
            sb.AppendLine();
            if (selectQuery.IsSubQuery)
                sb.AppendLine(string.Concat(subquerySpaces, "}"));

            return sb.ToString();
        }

        /// <summary>
        /// Prints the trailing solution-modifier section shared by SELECT/CONSTRUCT/DESCRIBE in SPARQL order:
        /// GROUP BY (with its HAVING), then ORDER BY, then LIMIT, then OFFSET. <paramref name="queryPrefixes"/> is
        /// used only to render the HAVING expression with prefixed names; <paramref name="subqueryBodySpaces"/> is
        /// the indentation prefix (empty at top level, non-empty for a nested SELECT subquery).
        /// </summary>
        internal static void PrintSolutionModifiers(StringBuilder sb, List<RDFModifier> modifiers, List<RDFNamespace> queryPrefixes, string subqueryBodySpaces)
        {
            //GROUP BY (+ HAVING)
            foreach (RDFGroupByModifier gm in modifiers.OfType<RDFGroupByModifier>())
            {
                //Implicit grouping (no partition conditions) emits no GROUP BY clause: the aggregates already
                //appear in the projection and the grouping is implied by their presence
                if (gm.PartitionConditions.Count == 0)
                    continue;

                //GROUP BY
                sb.AppendLine();
                sb.Append(subqueryBodySpaces);
                sb.Append(gm);
                //HAVING: the single free boolean expression (full SPARQL HAVING)
                if (gm.HavingExpression != null)
                {
                    sb.AppendLine();
                    sb.Append(string.Concat(subqueryBodySpaces, "HAVING ("));
                    //Re-print any hidden aggregate (referenced in HAVING but not projected) as its original call
                    sb.Append(gm.ReprintHiddenAggregateCalls(gm.HavingExpression.ToString(queryPrefixes)));
                    sb.Append(')');
                }
            }

            // ORDER BY
            if (modifiers.Any(mod => mod is RDFOrderByModifier))
            {
                sb.AppendLine();
                sb.Append($"{subqueryBodySpaces}ORDER BY");
                foreach (RDFOrderByModifier om in modifiers.OfType<RDFOrderByModifier>())
                    sb.Append($" {om}");
            }

            // LIMIT/OFFSET
            foreach (RDFLimitModifier lim in modifiers.OfType<RDFLimitModifier>())
            {
                sb.AppendLine();
                sb.Append(string.Concat(subqueryBodySpaces, lim.ToString()));
            }
            foreach (RDFOffsetModifier off in modifiers.OfType<RDFOffsetModifier>())
            {
                sb.AppendLine();
                sb.Append(string.Concat(subqueryBodySpaces, off.ToString()));
            }
        }

        /// <summary>
        /// Prints the trailing query-level VALUES (<c>… WHERE { … } VALUES …</c>), if any, at the given body
        /// indentation (empty at top level, non-empty inside a nested SELECT subquery's braces). Shared by every
        /// query form (SELECT/CONSTRUCT/DESCRIBE/ASK), since the VALUES clause is query-level in SPARQL ([4]).
        /// </summary>
        internal static void PrintTrailingValues(StringBuilder sb, RDFQuery query, List<RDFNamespace> prefixes, string subqueryBodySpaces)
        {
            if (query.QueryValues != null)
            {
                sb.AppendLine();
                sb.Append(string.Concat(subqueryBodySpaces, PrintValues(query.QueryValues, prefixes, subqueryBodySpaces)));
            }
        }

        /// <summary>
        /// Prints the string representation of a SPARQL DESCRIBE query
        /// </summary>
        internal static string PrintDescribeQuery(RDFDescribeQuery describeQuery)
        {
            StringBuilder sb = new StringBuilder();
            if (describeQuery == null)
                return sb.ToString();

            #region PREFIXES
            List<RDFNamespace> prefixes = PrintPrefixes(describeQuery, sb, true);
            #endregion

            #region DESCRIBE
            sb.Append("DESCRIBE");
            if (describeQuery.DescribeTerms.Count > 0)
                describeQuery.DescribeTerms.ForEach(dt => sb.Append($" {PrintPatternMember(dt, describeQuery.Prefixes)}"));
            else
                sb.Append(" *");
            sb.AppendLine();
            #endregion

            #region WHERE
            PrintWhereClause(describeQuery, sb, prefixes, string.Empty, 0, false);
            #endregion

            #region MODIFIERS
            PrintSolutionModifiers(sb, describeQuery.GetModifiers().ToList(), describeQuery.Prefixes, string.Empty);
            #endregion

            #region TRAILING VALUES
            PrintTrailingValues(sb, describeQuery, prefixes, string.Empty);
            #endregion

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL CONSTRUCT query
        /// </summary>
        internal static string PrintConstructQuery(RDFConstructQuery constructQuery)
        {
            StringBuilder sb = new StringBuilder();
            if (constructQuery == null)
                return sb.ToString();

            #region PREFIXES
            List<RDFNamespace> prefixes = PrintPrefixes(constructQuery, sb, true);
            #endregion

            #region CONSTRUCT
            sb.AppendLine("CONSTRUCT {");
            constructQuery.Templates.ForEach(tp =>
            {
                string tpString = PrintPattern(tp, constructQuery.Prefixes);

                //Remove Context from the template print (since it is not supported by CONSTRUCT query)
                if (tp.Context != null)
                {
                    string tpContext = PrintPatternMember(tp.Context, constructQuery.Prefixes);
                    tpString = tpString.Replace(string.Concat("GRAPH ", tpContext, " { "), string.Empty).TrimEnd(' ', '}');
                }

                //Remove Optional indicator from the template print (since it is not supported by CONSTRUCT query)
                if (tp.IsOptional)
                    tpString = tpString.Replace("OPTIONAL { ", string.Empty).TrimEnd(' ', '}');

                sb.AppendLine($"  {tpString} .");
            });
            sb.Append('}').AppendLine();
            #endregion

            #region WHERE
            PrintWhereClause(constructQuery, sb, prefixes, string.Empty, 0, false);
            #endregion

            #region MODIFIERS
            PrintSolutionModifiers(sb, constructQuery.GetModifiers().ToList(), constructQuery.Prefixes, string.Empty);
            #endregion

            #region TRAILING VALUES
            PrintTrailingValues(sb, constructQuery, prefixes, string.Empty);
            #endregion

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL ASK query
        /// </summary>
        internal static string PrintAskQuery(RDFAskQuery askQuery)
        {
            if (askQuery == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            List<RDFNamespace> prefixes = PrintPrefixes(askQuery, sb, true);
            sb.AppendLine("ASK");
            PrintWhereClause(askQuery, sb, prefixes, string.Empty, 0, false);
            PrintSolutionModifiers(sb, askQuery.GetModifiers().ToList(), askQuery.Prefixes, string.Empty);
            PrintTrailingValues(sb, askQuery, prefixes, string.Empty);

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL query's prefixes
        /// </summary>
        internal static List<RDFNamespace> PrintPrefixes(RDFQuery query, StringBuilder sb, bool enablePrinting)
        {
            List<RDFNamespace> prefixes = query.GetPrefixes();
            if (enablePrinting && prefixes.Count > 0)
            {
                prefixes.ForEach(pf => sb.AppendLine($"PREFIX {pf.NamespacePrefix}: <{pf.NamespaceUri}>"));
                sb.AppendLine();
            }
            return prefixes;
        }

        /// <summary>
        /// Prints the string representation of a SPARQL query's WHERE clause
        /// </summary>
        internal static void PrintWhereClause(RDFQuery query, StringBuilder sb, List<RDFNamespace> prefixes,
            string subqueryBodySpaces, double indentLevel, bool fromUnionOrMinus)
        {
            sb.AppendLine(string.Concat(subqueryBodySpaces, "WHERE {"));
            PrintWhereClauseMembers(query, sb, prefixes, subqueryBodySpaces, indentLevel, fromUnionOrMinus);
            PrintWhereClauseFilters(query, sb, prefixes, subqueryBodySpaces);
            sb.Append(string.Concat(subqueryBodySpaces, "}"));
        }

        /// <summary>
        /// Prints the WHERE-clause-scoped filters (those ranging over the whole top-level group graph pattern)
        /// </summary>
        private static void PrintWhereClauseFilters(RDFQuery query, StringBuilder sb, List<RDFNamespace> prefixes, string subqueryBodySpaces)
        {
            //These filters are rendered after all the group members, inside the WHERE braces: re-parsing places
            //them back at WHERE-clause scope (the parser hoists group-spanning filters), keeping the round-trip stable
            foreach (RDFFilter queryFilter in query.QueryFilters)
                sb.AppendLine(string.Concat(subqueryBodySpaces, "  ", queryFilter.ToString(prefixes)));
        }
        private static void PrintWhereClauseMembers(RDFQuery query, StringBuilder sb, List<RDFNamespace> prefixes,
            string subqueryBodySpaces, double indentLevel, bool fromUnionOrMinus)
        {
            foreach (RDFQueryMember queryMember in query.GetEvaluableQueryMembers())
                PrintWhereClauseMember(queryMember, sb, prefixes, subqueryBodySpaces, indentLevel, fromUnionOrMinus);
        }

        /// <summary>
        /// Prints a single WHERE-clause query member (pattern group, inline sub-select, binary algebra tree, or
        /// SERVICE), dispatching on its concrete type. Shared by the WHERE-clause printer and the SERVICE printer
        /// (whose inner group graph pattern is itself a query member of any shape).
        /// </summary>
        private static void PrintWhereClauseMember(RDFQueryMember queryMember, StringBuilder sb, List<RDFNamespace> prefixes,
            string subqueryBodySpaces, double indentLevel, bool fromUnionOrMinus)
        {
            switch (queryMember)
            {
                case RDFPatternGroup pgQueryMember:
                    sb.Append(PrintPatternGroup(pgQueryMember, subqueryBodySpaces.Length, false, prefixes));
                    break;

                case RDFSelectQuery sqQueryMember:
                    prefixes.ForEach(pf1 => sqQueryMember.AddPrefix(pf1));
                    sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), false));
                    break;

                case RDFBinaryQueryMember opQueryMember:
                    PrintOperatorQueryMemberTree(opQueryMember, sb, prefixes,
                        subqueryBodySpaces, indentLevel, fromUnionOrMinus);
                    break;

                case RDFService svcQueryMember:
                    PrintServiceInto(svcQueryMember, sb, prefixes, subqueryBodySpaces, indentLevel, fromUnionOrMinus, false);
                    break;
            }
        }

        /// <summary>
        /// Gives the string representation of a SERVICE clause (used by <see cref="RDFService.ToString()"/>).
        /// </summary>
        internal static string PrintService(RDFService service, List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();
            PrintServiceInto(service, sb, prefixes, string.Empty, 0, false, false);
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Prints a SERVICE clause into the given builder: the optional OPTIONAL wrapping (unless skipped, as when
        /// the SERVICE is an operand of a binary tree), the <c>SERVICE [SILENT]</c> header with the endpoint
        /// specifier (a variable <c>?ep</c> or a concrete <c>&lt;iri&gt;</c>), the inner group graph pattern (any
        /// shape, rendered one indentation level deeper), and the closing brace.
        /// </summary>
        private static void PrintServiceInto(RDFService service, StringBuilder sb, List<RDFNamespace> prefixes,
            string subqueryBodySpaces, double indentLevel, bool fromUnionOrMinus, bool skipOptional)
        {
            //OPTIONAL wrapping: open bracket before the SERVICE content
            if (service.IsOptional && !skipOptional)
            {
                sb.AppendLine(string.Concat(subqueryBodySpaces, "  OPTIONAL {"));
                subqueryBodySpaces = $"{subqueryBodySpaces}  ";
            }

            //SERVICE header: SILENT directive (if any) and the endpoint specifier (variable or concrete IRI)
            string silent = service.IsSilent ? "SILENT " : string.Empty;
            string endpoint = service.EndpointVariable != null
                ? service.EndpointVariable.ToString()
                : string.Concat("<", service.Endpoint.BaseAddress.ToString(), ">");
            sb.AppendLine(string.Concat(subqueryBodySpaces, "  SERVICE ", silent, endpoint, " {"));

            //Inner group graph pattern (pattern group, sub-select, binary tree, or nested SERVICE)
            PrintWhereClauseMember(service.InnerPattern, sb, prefixes, $"{subqueryBodySpaces}  ", indentLevel + 1, fromUnionOrMinus);

            //Close the SERVICE bracket
            sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));

            //OPTIONAL wrapping: close the outer OPTIONAL bracket
            if (service.IsOptional && !skipOptional)
            {
                if (subqueryBodySpaces.Length >= 2)
                    subqueryBodySpaces = new string(' ', subqueryBodySpaces.Length - 2);
                sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
            }
        }

        /// <summary>
        /// Prints an EXISTS / NOT EXISTS group graph pattern (braces included). Per SPARQL grammar the body is either a
        /// SubSelect (<see cref="RDFSelectQuery"/>) or a GroupGraphPatternSub (<see cref="RDFPatternGroup"/>): the
        /// pattern group is rendered as a compact single-line brace block (reusing the per-element printers, so literal
        /// content is preserved verbatim), while a SubSelect is rendered by the canonical (brace-wrapped) printer.
        /// </summary>
        internal static string PrintGroupGraphPattern(RDFQueryMember groupGraphPattern, List<RDFNamespace> prefixes)
        {
            switch (groupGraphPattern)
            {
                case RDFPatternGroup patternGroup:
                    return PrintExistsPatternGroupBody(patternGroup, prefixes);
                case RDFSelectQuery subSelect:
                    //The EXISTS body SubSelect is always flagged IsSubQuery (set when the filter is built), so it is
                    //rendered brace-wrapped and without a prologue of its own
                    return PrintSelectQuery(subSelect, 0, false).Trim();
                default:
                    return "{ }";
            }
        }

        /// <summary>
        /// Renders a pattern group as the compact single-line group graph pattern body used inside EXISTS / NOT EXISTS:
        /// triple/path/values/bind members (each suffixed with " .") and filters, all wrapped in a single pair of braces.
        /// </summary>
        private static string PrintExistsPatternGroupBody(RDFPatternGroup patternGroup, List<RDFNamespace> prefixes)
        {
            List<string> bodyParts = new List<string>();

            foreach (RDFPatternGroupMember pgMember in patternGroup.GetEvaluablePatternGroupMembers())
            {
                string renderedMember = PrintExistsPatternGroupMember(pgMember, prefixes);
                if (renderedMember != null)
                    //Triple-like members carry a dot terminator; binary (UNION/MINUS) trees stand on their own
                    bodyParts.Add(pgMember is RDFBinaryPatternGroupMember ? renderedMember : string.Concat(renderedMember, " ."));
            }

            foreach (RDFFilter filter in patternGroup.GetFilters())
                bodyParts.Add(filter.ToString(prefixes));

            return bodyParts.Count == 0 ? "{ }" : string.Concat("{ ", string.Join(" ", bodyParts), " }");
        }

        /// <summary>
        /// Renders a single pattern group member (no dot terminator) for the compact EXISTS body, reusing the canonical
        /// per-element printers and recursing into binary (UNION/MINUS) trees.
        /// </summary>
        private static string PrintExistsPatternGroupMember(RDFPatternGroupMember pgMember, List<RDFNamespace> prefixes)
        {
            switch (pgMember)
            {
                case RDFPattern pattern:
                    return PrintPattern(pattern, prefixes);
                case RDFPropertyPath propertyPath when propertyPath.IsEvaluable:
                    return PrintPropertyPath(propertyPath, prefixes);
                case RDFValues values when values.IsEvaluable:
                    return PrintValues(values, prefixes, string.Empty);
                case RDFBind bind:
                    return PrintBind(bind, prefixes);
                case RDFBinaryPatternGroupMember binaryMember:
                {
                    string operatorKeyword = binaryMember.OperatorType == RDFQueryEnums.RDFBinaryOperatorType.Union ? "UNION" : "MINUS";
                    return string.Concat("{ ", PrintExistsPatternGroupMember(binaryMember.LeftOperand, prefixes), " . } ", operatorKeyword,
                                         " { ", PrintExistsPatternGroupMember(binaryMember.RightOperand, prefixes), " . }");
                }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Prints the string representation of a pattern group
        /// </summary>
        internal static string PrintPatternGroup(RDFPatternGroup patternGroup, int spaceIndent, bool skipOptional, List<RDFNamespace> prefixes)
        {
            StringBuilder result = new StringBuilder();
            string spaces = new StringBuilder().Append(' ', spaceIndent < 0 ? 0 : spaceIndent).ToString();

            //OPTIONAL
            if (patternGroup.IsOptional && !skipOptional)
            {
                result.AppendLine(string.Concat("  ", spaces, "OPTIONAL {"));
                spaces = $"{spaces}  ";
            }

            //OPEN-BRACKET
            result.AppendLine(string.Concat(spaces, "  {"));

            //MEMBERS
            PrintPatternGroupMembers(patternGroup, result, spaces, prefixes);

            //FILTERS
            foreach (RDFFilter filter in patternGroup.GetFilters())
                result.AppendLine($"{spaces}    {filter.ToString(prefixes)} ");

            //CLOSE-BRACKET
            result.AppendLine(string.Concat(spaces, "  }"));

            //OPTIONAL
            if (patternGroup.IsOptional && !skipOptional)
                result.AppendLine(string.Concat(spaces, "}"));

            return result.ToString();
        }
        private static void PrintPatternGroupMembers(RDFPatternGroup patternGroup, StringBuilder result, string spaces, List<RDFNamespace> prefixes)
        {
            foreach (RDFPatternGroupMember pgMember in patternGroup.GetEvaluablePatternGroupMembers())
                switch (pgMember)
                {
                    case RDFPattern ptPgMember:
                        result.AppendLine($"{spaces}    {PrintPattern(ptPgMember, prefixes)} .");
                        break;

                    case RDFPropertyPath ppPgMember when ppPgMember.IsEvaluable:
                        result.AppendLine($"{spaces}    {PrintPropertyPath(ppPgMember, prefixes)} .");
                        break;

                    case RDFValues vlPgMember when vlPgMember.IsEvaluable:
                        result.AppendLine($"{spaces}    {PrintValues(vlPgMember, prefixes, spaces)} .");
                        break;

                    case RDFBind bdPgMember:
                        result.AppendLine($"{spaces}    {PrintBind(bdPgMember, prefixes)} .");
                        break;

                    case RDFBinaryPatternGroupMember opPgMember:
                        PrintOperatorPatternGroupMemberTree(opPgMember, result, spaces, prefixes);
                        break;
                }
        }

        /// <summary>
        /// Prints the string representation of a pattern
        /// </summary>
        internal static string PrintPattern(RDFPattern pattern, List<RDFNamespace> prefixes)
        {
            string subj = PrintPatternMember(pattern.Subject, prefixes);
            string pred = PrintPatternMember(pattern.Predicate, prefixes);
            string obj = PrintPatternMember(pattern.Object, prefixes);

            //CSPO pattern
            if (pattern.Context != null)
            {
                string ctx = PrintPatternMember(pattern.Context, prefixes);
                return pattern.IsOptional ? string.Concat("OPTIONAL { GRAPH ", ctx, " { ", subj, " ", pred, " ", obj, " } }")
                                          : string.Concat("GRAPH ", ctx, " { ", subj, " ", pred, " ", obj, " }");
            }

            //SPO pattern
            return pattern.IsOptional ? string.Concat("OPTIONAL { ", subj, " ", pred, " ", obj, " }")
                                      : $"{subj} {pred} {obj}";
        }

        /// <summary>
        /// Returns the SPARQL cardinality suffix for the given cardinality
        /// </summary>
        private static string PrintPathCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities cardinality)
        {
            switch (cardinality)
            {
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne:  return "?";
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore:  return "+";
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore: return "*";
                default: return string.Empty;
            }
        }

        /// <summary>
        /// Operator precedence of a property path expression, used to decide when a child needs parentheses:
        /// alternative (1) binds looser than sequence (2), and an atom or any decorated node (4) binds tightest
        /// (its own decoration already parenthesizes a composite base when needed).
        /// </summary>
        private static int PathExpressionPrecedence(RDFPropertyPathExpression expression)
        {
            if (expression.IsInverse || expression.Cardinality != RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne)
                return 4;
            switch (expression.Kind)
            {
                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative: return 1;
                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence:    return 2;
                default:                                                       return 4; //Link, NegatedPropertySet
            }
        }

        /// <summary>
        /// Prints a property path expression as a child of a parent context with the given precedence, adding
        /// parentheses only when the child binds looser than the context requires.
        /// </summary>
        private static string PrintPathExpression(RDFPropertyPathExpression expression, int parentPrecedence, List<RDFNamespace> prefixes)
        {
            string rendered = PrintPathExpression(expression, prefixes);
            return PathExpressionPrecedence(expression) < parentPrecedence ? $"({rendered})" : rendered;
        }

        /// <summary>
        /// Prints a property path expression, recursively, with its inverse (<c>^</c>) and cardinality
        /// (<c>? * +</c>) decorations. A decoration wrapping a composite base (sequence/alternative) parenthesizes
        /// that base so it binds as a unit.
        /// </summary>
        private static string PrintPathExpression(RDFPropertyPathExpression expression, List<RDFNamespace> prefixes)
        {
            //Render the base (kind + children), ignoring the node's own inverse/cardinality decorations
            string baseString;
            switch (expression.Kind)
            {
                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence:
                    //A single-child sequence is the implicit group used to wrap a decorated inner sub-path
                    baseString = expression.Children.Count == 1
                        ? PrintPathExpression(expression.Children[0], prefixes)
                        : string.Join("/", expression.Children.Select(child => PrintPathExpression(child, 2, prefixes)));
                    break;

                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative:
                    baseString = string.Join("|", expression.Children.Select(child => PrintPathExpression(child, 1, prefixes)));
                    break;

                case RDFQueryEnums.RDFPropertyPathExpressionKinds.NegatedPropertySet:
                    baseString = PrintNegatedPropertySet(expression.NegatedMembers, prefixes);
                    break;

                default: // Link
                    baseString = PrintPatternMember(expression.Property, prefixes);
                    break;
            }

            //A decoration over a composite base must parenthesize the base so the decoration binds the whole group
            bool decorated = expression.IsInverse || expression.Cardinality != RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne;
            bool compositeBase = expression.Kind == RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence
                                  || expression.Kind == RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative;
            if (decorated && compositeBase)
                baseString = $"({baseString})";

            if (expression.IsInverse)
                baseString = "^" + baseString;
            return baseString + PrintPathCardinality(expression.Cardinality);
        }

        /// <summary>
        /// Prints a negated property set: <c>!iri</c> / <c>!^iri</c> for a single member, <c>!(a|^b|…)</c> otherwise.
        /// </summary>
        private static string PrintNegatedPropertySet(List<(RDFResource Property, bool IsInverse)> members, List<RDFNamespace> prefixes)
        {
            string PrintMember((RDFResource Property, bool IsInverse) member)
                => (member.IsInverse ? "^" : string.Empty) + PrintPatternMember(member.Property, prefixes);

            return members.Count == 1
                ? "!" + PrintMember(members[0])
                : "!(" + string.Join("|", members.Select(PrintMember)) + ")";
        }

        /// <summary>
        /// Prints the string representation of a property path
        /// </summary>
        internal static string PrintPropertyPath(RDFPropertyPath propertyPath, List<RDFNamespace> prefixes)
        {
            //An empty path (no steps yet) prints with an empty middle, preserving the historical double space
            string expressionString = propertyPath.Expression != null ? PrintPathExpression(propertyPath.Expression, prefixes) : string.Empty;
            string pathString = $"{PrintPatternMember(propertyPath.Start, prefixes)} {expressionString} {PrintPatternMember(propertyPath.End, prefixes)}";
            return propertyPath.IsOptional ? $"OPTIONAL {{ {pathString} }}" : pathString;
        }

        /// <summary>
        /// Prints the string representation of a SPARQL values
        /// </summary>
        internal static string PrintValues(RDFValues values, List<RDFNamespace> prefixes, string spaces)
        {
            StringBuilder result = new StringBuilder();

            //Compact representation
            if (values.Bindings.Keys.Count == 1)
            {
                result.Append($"VALUES {values.Bindings.Keys.ElementAt(0)}");
                result.Append(" { ");
                foreach (RDFPatternMember binding in values.Bindings.ElementAt(0).Value)
                {
                    result.Append(binding == null ? "UNDEF" : PrintPatternMember(binding, prefixes));
                    result.Append(' ');
                }
                result.Append('}');
            }

            //Extended representation
            else
            {
                result.Append($"VALUES ({string.Join(" ", values.Bindings.Keys)})");
                result.AppendLine(" {");
                for (int i = 0; i < values.MaxBindingsLength(); i++)
                {
                    result.Append($"{spaces}      ( ");
                    foreach (KeyValuePair<string, List<RDFPatternMember>> binding in values.Bindings)
                    {
                        RDFPatternMember bindingValue = binding.Value.ElementAtOrDefault(i);
                        result.Append(bindingValue == null ? "UNDEF" : PrintPatternMember(bindingValue, prefixes));
                        result.Append(' ');
                    }
                    result.Append(')').AppendLine();
                }
                result.Append(string.Concat(spaces, "    }"));
            }

            return result.ToString();
        }

        /// <summary>
        /// Prints the string representation of a bind operator
        /// </summary>
        internal static string PrintBind(RDFBind bind, List<RDFNamespace> prefixes)
            => $"BIND({bind.Expression.ToString(prefixes)} AS {bind.Variable})";

        /// <summary>
        /// Prints the string representation of a pattern member
        /// </summary>
        internal static string PrintPatternMember(RDFPatternMember patternMember, List<RDFNamespace> prefixes)
        {
            switch (patternMember)
            {
                default:
                    return null;
                case RDFVariable varPatternMember:
                    return varPatternMember.ToString();
                case RDFResource _:
                case RDFContext _:
                {
                    #region Blank
                    if (patternMember is RDFResource resPatternMember && resPatternMember.IsBlank)
                        return resPatternMember.ToString().Replace("bnode:", "_:");
                    #endregion

                    #region NonBlank
                    (bool, string) abbreviatedPM = RDFQueryUtilities.AbbreviateRDFPatternMember(patternMember, prefixes);
                    return abbreviatedPM.Item1 ? abbreviatedPM.Item2 : $"<{abbreviatedPM.Item2}>";
                    #endregion
                }
                case RDFPlainLiteral plPatternMember when plPatternMember.HasLanguage():
                    return $"\"{plPatternMember.Value}\"@{plPatternMember.Language}";
                case RDFPlainLiteral plPatternMember:
                    return $"\"{plPatternMember.Value}\"";
                case RDFTypedLiteral tlPatternMember:
                {
                    string tlDatatype = tlPatternMember.Datatype.URI.ToString();
                    (bool, string) abbreviatedPM = RDFQueryUtilities.AbbreviateRDFPatternMember(RDFQueryUtilities.ParseRDFPatternMember(tlDatatype), prefixes);
                    return abbreviatedPM.Item1 ? $"\"{tlPatternMember.Value}\"^^{abbreviatedPM.Item2}" : $"\"{tlPatternMember.Value}\"^^<{abbreviatedPM.Item2}>";
                }
            }
        }
        /// <summary>
        /// Prints the string representation of an operator tree node at the query-member level (UNION/MINUS between pattern groups and subqueries)
        /// </summary>
        private static void PrintOperatorQueryMemberTree(RDFBinaryQueryMember binaryNode, StringBuilder sb,
            List<RDFNamespace> prefixes, string subqueryBodySpaces, double indentLevel, bool fromUnionOrMinus)
        {
            string operatorKeyword = binaryNode.OperatorType == RDFQueryEnums.RDFBinaryOperatorType.Union ? "UNION" : "MINUS";

            //OPTIONAL wrapping: open bracket before the operator tree content
            if (binaryNode.IsOptional)
            {
                sb.AppendLine(string.Concat(subqueryBodySpaces, "  OPTIONAL {"));
                subqueryBodySpaces = $"{subqueryBodySpaces}  ";
            }

            //Open outer bracket for the operator tree
            sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));

            //Print left operand (pattern group, subquery, or nested operator tree)
            PrintOperatorQueryMemberOperand(binaryNode.LeftOperand, sb, prefixes, subqueryBodySpaces, indentLevel, fromUnionOrMinus);

            //Print operator keyword (UNION or MINUS)
            sb.AppendLine($"{subqueryBodySpaces}    {operatorKeyword}");

            //Print right operand (pattern group, subquery, or nested operator tree)
            PrintOperatorQueryMemberOperand(binaryNode.RightOperand, sb, prefixes, subqueryBodySpaces, indentLevel, fromUnionOrMinus);

            //Close outer bracket for the operator tree
            sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));

            //OPTIONAL wrapping: close the outer OPTIONAL bracket
            if (binaryNode.IsOptional)
            {
                if (subqueryBodySpaces.Length >= 2)
                    subqueryBodySpaces = new string(' ', subqueryBodySpaces.Length - 2);
                sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
            }
        }

        /// <summary>
        /// Prints a single operand of a query-level operator tree node, dispatching on the operand type
        /// </summary>
        private static void PrintOperatorQueryMemberOperand(RDFQueryMember operand, StringBuilder sb,
            List<RDFNamespace> prefixes, string subqueryBodySpaces, double indentLevel, bool fromUnionOrMinus)
        {
            switch (operand)
            {
                //Leaf: pattern group
                case RDFPatternGroup patternGroup:
                {
                    sb.Append(PrintPatternGroup(patternGroup, subqueryBodySpaces.Length + 2, true, prefixes));
                    break;
                }

                //Leaf: subquery (mark as subquery and merge prefixes before printing)
                case RDFSelectQuery selectQuery:
                {
                    prefixes.ForEach(pf => selectQuery.AddPrefix(pf));
                    selectQuery.SubQuery();
                    sb.Append(PrintSelectQuery(selectQuery, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), true));
                    break;
                }

                //Subtree: nested operator node (recurse with increased indentation)
                case RDFBinaryQueryMember innerOperator:
                {
                    PrintOperatorQueryMemberTree(innerOperator, sb, prefixes,
                        $"{subqueryBodySpaces}  ", indentLevel, fromUnionOrMinus);
                    break;
                }

                //Leaf: SERVICE (federated query), wrapped in its own group braces as a binary operand
                //(its own OPTIONAL is skipped, exactly as for a pattern-group operand)
                case RDFService service:
                {
                    sb.AppendLine(string.Concat(subqueryBodySpaces, "    {"));
                    PrintServiceInto(service, sb, prefixes, $"{subqueryBodySpaces}    ", indentLevel, fromUnionOrMinus, true);
                    sb.AppendLine(string.Concat(subqueryBodySpaces, "    }"));
                    break;
                }
            }
        }

        /// <summary>
        /// Prints the string representation of an operator tree node at the pattern-group-member level (UNION/MINUS between patterns and property paths)
        /// </summary>
        private static void PrintOperatorPatternGroupMemberTree(RDFBinaryPatternGroupMember binaryNode, StringBuilder result,
            string spaces, List<RDFNamespace> prefixes)
        {
            string operatorKeyword = binaryNode.OperatorType == RDFQueryEnums.RDFBinaryOperatorType.Union ? "UNION" : "MINUS";

            //Print left operand (pattern, property path, or nested operator tree)
            PrintOperatorPatternGroupMemberOperand(binaryNode.LeftOperand, result, spaces, prefixes);

            //Print operator keyword (UNION or MINUS)
            result.AppendLine($"{spaces}    {operatorKeyword}");

            //Print right operand (pattern, property path, or nested operator tree)
            PrintOperatorPatternGroupMemberOperand(binaryNode.RightOperand, result, spaces, prefixes);
        }

        /// <summary>
        /// Prints a single operand of a pattern-group-level operator tree node, dispatching on the operand type
        /// </summary>
        private static void PrintOperatorPatternGroupMemberOperand(RDFPatternGroupMember operand, StringBuilder result,
            string spaces, List<RDFNamespace> prefixes)
        {
            switch (operand)
            {
                //Leaf: pattern (printed as a bracketed triple)
                case RDFPattern pattern:
                {
                    result.AppendLine(string.Concat(spaces, "    { ", PrintPattern(pattern, prefixes), " }"));
                    break;
                }

                //Leaf: property path (printed as a bracketed path expression)
                case RDFPropertyPath propertyPath:
                {
                    result.AppendLine(string.Concat(spaces, "    { ", PrintPropertyPath(propertyPath, prefixes), " }"));
                    break;
                }

                //Subtree: nested operator node (print with bracket wrapping and increased indentation)
                case RDFBinaryPatternGroupMember innerOperator:
                {
                    result.AppendLine(string.Concat(spaces, "    {"));
                    PrintOperatorPatternGroupMemberTree(innerOperator, result, $"{spaces}  ", prefixes);
                    result.AppendLine(string.Concat(spaces, "    }"));
                    break;
                }
            }
        }
        #endregion
    }
}