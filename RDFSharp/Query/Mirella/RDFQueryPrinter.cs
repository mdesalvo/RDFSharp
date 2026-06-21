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
            if (selectQuery.QueryValues != null)
            {
                sb.AppendLine();
                sb.Append(string.Concat(subqueryBodySpaces, PrintValues(selectQuery.QueryValues, prefixes, subqueryBodySpaces)));
            }
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
            sb.Append(string.Concat(subqueryBodySpaces, "}"));
        }
        private static void PrintWhereClauseMembers(RDFQuery query, StringBuilder sb, List<RDFNamespace> prefixes,
            string subqueryBodySpaces, double indentLevel, bool fromUnionOrMinus)
        {
            foreach (RDFQueryMember queryMember in query.GetEvaluableQueryMembers())
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

            //SERVICE
            if (patternGroup.EvaluateAsService.HasValue)
            {
                bool isSilent = patternGroup.EvaluateAsService.Value.Item2.ErrorBehavior == RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult;
                string service = $"SERVICE {(isSilent ? "SILENT " : string.Empty)}";
                result.AppendLine(string.Concat("  ", spaces, service, "<", patternGroup.EvaluateAsService.Value.Item1 , "> {"));
                spaces = $"{spaces}  ";
            }

            //OPEN-BRACKET
            result.AppendLine(string.Concat(spaces, "  {"));

            //MEMBERS
            PrintPatternGroupMembers(patternGroup, result, spaces, prefixes);

            //FILTERS
            foreach (RDFFilter filter in patternGroup.GetFilters().Where(f => !(f is RDFValuesFilter)))
                result.AppendLine($"{spaces}    {filter.ToString(prefixes)} ");

            //CLOSE-BRACKET
            result.AppendLine(string.Concat(spaces, "  }"));

            //SERVICE
            if (patternGroup.EvaluateAsService.HasValue)
            {
                result.AppendLine(string.Concat(spaces, "}"));
                if (spaces.Length > 2)
                    spaces = spaces.Substring(2);
            }

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
        /// Returns the SPARQL cardinality suffix for a property path step
        /// </summary>
        private static string PrintStepCardinality(RDFPropertyPathStep step)
        {
            switch (step.StepCardinality)
            {
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne:  return "?";
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore:  return "+";
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore: return "*";
                default: return string.Empty;
            }
        }

        /// <summary>
        /// Prints the string representation of a property path
        /// </summary>
        internal static string PrintPropertyPath(RDFPropertyPath propertyPath, List<RDFNamespace> prefixes)
        {
            StringBuilder result = new StringBuilder();
            result.Append(PrintPatternMember(propertyPath.Start, prefixes));
            result.Append(' ');

            #region Single Property
            if (propertyPath.Steps.Count == 1)
            {
                //InversePath (will swap start/end)
                if (propertyPath.Steps[0].IsInverseStep)
                    result.Append('^');

                RDFResource propPath = propertyPath.Steps[0].StepProperty;
                result.Append(PrintPatternMember(propPath, prefixes));
                result.Append(PrintStepCardinality(propertyPath.Steps[0]));
            }
            #endregion

            #region Multiple Properties
            else
            {
                //Initialize printing
                bool openedParenthesis = false;

                //Iterate properties
                for (int i = 0; i < propertyPath.Steps.Count; i++)
                    //Alternative: generate union pattern
                    if (propertyPath.Steps[i].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                    {
                        if (!openedParenthesis)
                        {
                            openedParenthesis = true;
                            result.Append('(');
                        }

                        //InversePath (will swap start/end)
                        if (propertyPath.Steps[i].IsInverseStep)
                            result.Append('^');

                        var propPath = propertyPath.Steps[i].StepProperty;
                        if (i < propertyPath.Steps.Count - 1)
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                            result.Append(PrintStepCardinality(propertyPath.Steps[i]));
                            result.Append((char)propertyPath.Steps[i].StepFlavor);
                        }
                        else
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                            result.Append(PrintStepCardinality(propertyPath.Steps[i]));
                            result.Append(')');
                        }
                    }

                    //Sequence: generate pattern
                    else
                    {
                        if (openedParenthesis)
                        {
                            result.Remove(result.Length - 1, 1);
                            openedParenthesis = false;
                            result.Append(")/");
                        }

                        //InversePath (will swap start/end)
                        if (propertyPath.Steps[i].IsInverseStep)
                            result.Append('^');

                        var propPath = propertyPath.Steps[i].StepProperty;
                        if (i < propertyPath.Steps.Count - 1)
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                            result.Append(PrintStepCardinality(propertyPath.Steps[i]));
                            result.Append((char)propertyPath.Steps[i].StepFlavor);
                        }
                        else
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                            result.Append(PrintStepCardinality(propertyPath.Steps[i]));
                        }
                    }
            }
            #endregion

            result.Append(' ');
            result.Append(PrintPatternMember(propertyPath.End, prefixes));
            string pathString = result.ToString();
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
                //Leaf: pattern group (may have SERVICE wrapping)
                case RDFPatternGroup patternGroup:
                {
                    if (patternGroup.EvaluateAsService.HasValue)
                    {
                        sb.AppendLine(string.Concat(subqueryBodySpaces, "    {"));
                        sb.Append(PrintPatternGroup(patternGroup, subqueryBodySpaces.Length + 4, true, prefixes));
                        sb.AppendLine(string.Concat(subqueryBodySpaces, "    }"));
                    }
                    else
                    {
                        sb.Append(PrintPatternGroup(patternGroup, subqueryBodySpaces.Length + 2, true, prefixes));
                    }
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