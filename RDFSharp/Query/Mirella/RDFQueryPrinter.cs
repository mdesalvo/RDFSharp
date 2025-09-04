/*
   Copyright 2012-2025 Marco De Salvo

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
            StringBuilder sb = new StringBuilder(512);
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
            List<RDFModifier> modifiers = [.. selectQuery.GetModifiers()];
            foreach (RDFDistinctModifier dm in modifiers.OfType<RDFDistinctModifier>())
                sb.Append($" {dm}");
            #endregion

            #region VARIABLES/AGGREGATORS
            //Query has GroupBy modifier => reset given projections, this modifier takes control!
            if (modifiers.Any(mod => mod is RDFGroupByModifier))
            {
                foreach (RDFGroupByModifier gm in modifiers.OfType<RDFGroupByModifier>())
                {
                    sb.Append(' ');
                    sb.Append(string.Join(" ", gm.PartitionVariables));
                    sb.Append(' ');
                    sb.Append(string.Join(" ", gm.Aggregators.Where(ag => !(ag is RDFPartitionAggregator))));
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
            //GROUP BY
            foreach (RDFGroupByModifier gm in modifiers.OfType<RDFGroupByModifier>())
            {
                //GROUP BY
                sb.AppendLine();
                sb.Append(subqueryBodySpaces);
                sb.Append(gm);
                //HAVING
                if (gm.Aggregators.Any(ag => ag.HavingClause.Item1))
                {
                    sb.AppendLine();
                    sb.AppendFormat(string.Concat(subqueryBodySpaces, "HAVING ({0})"), string.Join(" && ", gm.Aggregators.Where(ag => ag.HavingClause.Item1).Select(x => x.PrintHavingClause(selectQuery.Prefixes))));
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
            #endregion

            //CLOSURE
            sb.AppendLine();
            if (selectQuery.IsSubQuery)
                sb.AppendLine(string.Concat(subquerySpaces, "}"));

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL DESCRIBE query
        /// </summary>
        internal static string PrintDescribeQuery(RDFDescribeQuery describeQuery)
        {
            StringBuilder sb = new StringBuilder(512);
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
            List<RDFModifier> modifiers = [.. describeQuery.GetModifiers()];
            // LIMIT/OFFSET
            foreach (RDFLimitModifier lim in modifiers.OfType<RDFLimitModifier>())
            {
                sb.AppendLine();
                sb.Append(lim);
            }
            foreach (RDFOffsetModifier off in modifiers.OfType<RDFOffsetModifier>())
            {
                sb.AppendLine();
                sb.Append(off);
            }
            #endregion

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL CONSTRUCT query
        /// </summary>
        internal static string PrintConstructQuery(RDFConstructQuery constructQuery)
        {
            StringBuilder sb = new StringBuilder(512);
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
            List<RDFModifier> modifiers = [.. constructQuery.GetModifiers()];
            // LIMIT/OFFSET
            foreach (RDFLimitModifier lim in modifiers.OfType<RDFLimitModifier>())
            {
                sb.AppendLine();
                sb.Append(lim);
            }
            foreach (RDFOffsetModifier off in modifiers.OfType<RDFOffsetModifier>())
            {
                sb.AppendLine();
                sb.Append(off);
            }
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

            StringBuilder sb = new StringBuilder(512);
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
            #region Facilities
            void PrintWrappedPatternGroup(RDFPatternGroup patternGroup)
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
            }
            #endregion

            bool printingUnion = false;
            bool printingMinus = false;
            List<RDFQueryMember> evaluableQueryMembers = [.. query.GetEvaluableQueryMembers()];
            for (int i=0; i<evaluableQueryMembers.Count; i++)
            {
                RDFQueryMember queryMember = evaluableQueryMembers[i];
                RDFQueryMember nextQueryMember = i < evaluableQueryMembers.Count-1 ? evaluableQueryMembers[i+1] : null;
                bool isLastQueryMember = nextQueryMember == null;

                #region PATTERNGROUP
                switch (queryMember)
                {
                    case RDFPatternGroup pgQueryMember:
                    {
                        //PatternGroup is set as UNION with the next query member and it IS NOT the last one => append UNION
                        if (pgQueryMember.JoinAsUnion && !isLastQueryMember)
                        {
                            //In case we are opening UNION semantic, we need to print its opening bracket
                            if (!printingUnion)
                            {
                                //Signal UNION semantic
                                printingUnion = true;

                                //In case we are already under MINUS semantic, keep care of reflecting this in the indentation spaces (+2)
                                if (printingMinus)
                                    subqueryBodySpaces = $"{subqueryBodySpaces}  ";
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                            }

                            //Then we can print the pattern group, along with its UNION operator
                            PrintWrappedPatternGroup(pgQueryMember);
                            sb.AppendLine($"{subqueryBodySpaces}    UNION");
                        }

                        //PatternGroup is set as MINUS with the next query member and it IS NOT the last one => append MINUS
                        else if (pgQueryMember.JoinAsMinus && !isLastQueryMember)
                        {
                            //In case we are opening MINUS semantic, we need to print its opening bracket
                            if (!printingMinus)
                            {
                                //Signal MINUS semantic
                                printingMinus = true;

                                //In case we are already under UNION semantic, keep care of reflecting this in the indentation spaces (+2)
                                if (printingUnion)
                                    subqueryBodySpaces = $"{subqueryBodySpaces}  ";
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                            }

                            //Then we can print the pattern group, along with its MINUS operator
                            PrintWrappedPatternGroup(pgQueryMember);
                            sb.AppendLine($"{subqueryBodySpaces}    MINUS");
                        }

                        //PatternGroup is set as INTERSECT with the next query member or it IS the last one => do not append UNION/MINUS
                        else
                        {
                            //In case we are under MINUS or UNION semantic, we need to print their closing brackets to complete the grammar
                            if (printingUnion || printingMinus)
                            {
                                //At first we can print the pattern group
                                PrintWrappedPatternGroup(pgQueryMember);

                                //In case we are under both MINUS and UNION semantic, keep care of reflecting this in the closing brackets and indentation spaces (-2)
                                if (printingUnion && printingMinus)
                                {
                                    sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                                    if (subqueryBodySpaces.Length >= 2)
                                        subqueryBodySpaces = new string(' ', subqueryBodySpaces.Length - 2);
                                }
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));

                                //Unsignal UNION/MINUS semantic
                                printingUnion = false;
                                printingMinus = false;
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup(pgQueryMember, subqueryBodySpaces.Length, false, prefixes));
                            }
                        }
                        break;
                    }
                    case RDFSelectQuery sqQueryMember:
                    {
                        //Merge main query prefixes
                        prefixes.ForEach(pf1 => sqQueryMember.AddPrefix(pf1));

                        //SubQuery is set as UNION with the next query member and it IS NOT the last one => append UNION
                        if (sqQueryMember.JoinAsUnion && !isLastQueryMember)
                        {
                            //In case we are opening UNION semantic, we need to print its opening bracket
                            if (!printingUnion)
                            {
                                //Signal UNION semantic
                                printingUnion = true;

                                //In case we are already under MINUS semantic, keep care of reflecting this in the indentation spaces (+2)
                                if (printingMinus)
                                {
                                    subqueryBodySpaces = $"{subqueryBodySpaces}  ";
                                    indentLevel += 0.5;
                                }
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                            }

                            //Then we can print the subquery, along with its UNION operator
                            sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), true));
                            sb.AppendLine($"{subqueryBodySpaces}    UNION");
                        }

                        //SubQuery is set as MINUS with the next query member and it IS NOT the last one => append MINUS
                        else if (sqQueryMember.JoinAsMinus && !isLastQueryMember)
                        {
                            //In case we are opening MINUS semantic, we need to print its opening bracket
                            if (!printingMinus)
                            {
                                //Signal MINUS semantic
                                printingMinus = true;

                                //In case we are already under UNION semantic, keep care of reflecting this in the indentation spaces (+2)
                                if (printingUnion)
                                {
                                    subqueryBodySpaces = $"{subqueryBodySpaces}  ";
                                    indentLevel += 0.5;
                                }
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                            }

                            //Then we can print the subquery, along with its MINUS operator
                            sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), true));
                            sb.AppendLine($"{subqueryBodySpaces}    MINUS");
                        }

                        //SubQuery is set as INTERSECT with the next query member or it IS the last one => do not append UNION/MINUS
                        else
                        {
                            //In case we are under MINUS or UNION semantic, we need to print their closing brackets to complete the grammar
                            if (printingUnion || printingMinus)
                            {
                                //At first we can print the subquery
                                sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), true));

                                //In case we are under both MINUS and UNION semantic, keep care of reflecting this in the closing brackets and indentation spaces (-2)
                                if (printingUnion && printingMinus)
                                {
                                    sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                                    if (subqueryBodySpaces.Length >= 2)
                                    {
                                        subqueryBodySpaces = new string(' ', subqueryBodySpaces.Length - 2);
                                        indentLevel -= 0.5;
                                    }
                                }
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));

                                //Unsignal UNION/MINUS semantic
                                printingUnion = false;
                                printingMinus = false;
                            }
                            else
                            {
                                sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), false));
                            }
                        }
                        break;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Prints the string representation of a pattern group
        /// </summary>
        internal static string PrintPatternGroup(RDFPatternGroup patternGroup, int spaceIndent, bool skipOptional, List<RDFNamespace> prefixes)
        {
            StringBuilder result = new StringBuilder(256);
            string spaces = new StringBuilder(spaceIndent).Append(' ', spaceIndent < 0 ? 0 : spaceIndent).ToString();

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
            int openedBrackets = 0;
            bool printingUnion = false;
            bool printingMinus = false;
            List<RDFPatternGroupMember> evaluablePGMembers = [.. patternGroup.GetEvaluablePatternGroupMembers()];
            for (int i=0; i<evaluablePGMembers.Count; i++)
            {
                RDFPatternGroupMember pgMember = evaluablePGMembers[i];
                RDFPatternGroupMember nextPgMember = i < evaluablePGMembers.Count-1 ? evaluablePGMembers[i+1] : null;
                bool isLastPgMemberOrNextPgMemberIsBind = nextPgMember == null || nextPgMember is RDFBind;

                #region PATTERN
                switch (pgMember)
                {
                    case RDFPattern ptPgMember:
                    {
                        //Pattern is set as UNION with the next pg member and it IS NOT the last one => append UNION
                        if (ptPgMember.JoinAsUnion && !isLastPgMemberOrNextPgMemberIsBind)
                        {
                            //In case we are opening UNION semantic, but we are also under MINUS semantic, we need to print its opening bracket
                            //and to keep care of reflecting this in the indentation spaces (+2)
                            if (!printingUnion && printingMinus)
                            {
                                openedBrackets++;
                                spaces = $"{spaces}  ";
                                result.AppendLine(string.Concat(spaces, "  {"));
                            }

                            //Then we can print the bracketed pattern, along with its UNION operator
                            result.AppendLine(string.Concat(spaces, "    { ", PrintPattern(ptPgMember, prefixes), " }"));
                            result.AppendLine($"{spaces}    UNION");

                            //Signal UNION semantic
                            printingUnion = true;
                        }

                        //Pattern is set as MINUS with the next pg member and it IS NOT the last one => append MINUS
                        else if (ptPgMember.JoinAsMinus && !isLastPgMemberOrNextPgMemberIsBind)
                        {
                            //We can directly print the bracketed pattern, along with its MINUS operator
                            result.AppendLine(string.Concat(spaces, "    { ", PrintPattern(ptPgMember, prefixes), " }"));
                            result.AppendLine($"{spaces}    MINUS");

                            //Signal MINUS semantic
                            printingMinus = true;
                            //Unsignal UNION semantic
                            printingUnion = false;
                        }

                        //Pattern is set as INTERSECT with the next pg member or it IS the last one => do not append UNION/MINUS
                        else
                        {
                            //In case we are under MINUS or UNION semantic, we need to print all their closing brackets to complete the grammar
                            if (printingUnion || printingMinus)
                            {
                                //We can directly print the bracketed pattern
                                result.AppendLine(string.Concat(spaces, "    { ", PrintPattern(ptPgMember, prefixes), " }"));

                                //Then we need to print all the pending brackets and to keep care of
                                //reflecting this in the indentation spaces (-2) which must be consumed
                                while (openedBrackets > 0)
                                {
                                    openedBrackets--;
                                    result.AppendLine(string.Concat(spaces, "  }"));
                                    if (spaces.Length >= 2)
                                        spaces = new string(' ', spaces.Length - 2);
                                }

                                //Unsignal UNION/MINUS semantic
                                printingUnion = false;
                                printingMinus = false;
                            }
                            else
                            {
                                result.AppendLine($"{spaces}    {PrintPattern(ptPgMember, prefixes)} .");
                            }
                        }
                        break;
                    }
                    case RDFPropertyPath ppPgMember when ppPgMember.IsEvaluable:
                    {
                        //In case we are under MINUS or UNION semantic, we need to print all their closing brackets to complete the grammar
                        if (printingUnion || printingMinus)
                        {
                            //We can directly print the bracketed property path
                            result.AppendLine(string.Concat(spaces, "    { ", PrintPropertyPath(ppPgMember, prefixes), " }"));

                            //Then we need to print all the pending brackets and to keep care of
                            //reflecting this in the indentation spaces (-2) which must be consumed
                            while (openedBrackets > 0)
                            {
                                openedBrackets--;
                                result.AppendLine(string.Concat(spaces, "  }"));
                                if (spaces.Length >= 2)
                                    spaces = new string(' ', spaces.Length - 2);
                            }

                            //Unsignal UNION/MINUS semantic
                            printingUnion = false;
                            printingMinus = false;
                        }
                        else
                        {
                            result.AppendLine($"{spaces}    {PrintPropertyPath(ppPgMember, prefixes)} .");
                        }
                        break;
                    }
                    case RDFValues vlPgMember when vlPgMember.IsEvaluable && !vlPgMember.IsInjected:
                    {
                        //In case we are under MINUS or UNION semantic, we need to print all their closing brackets to complete the grammar
                        if (printingUnion || printingMinus)
                        {
                            //We can directly print the bracketed values
                            result.AppendLine(string.Concat(spaces, "    { ", PrintValues(vlPgMember, prefixes, spaces), " }"));

                            //Then we need to print all the pending brackets and to keep care of
                            //reflecting this in the indentation spaces (-2) which must be consumed
                            while (openedBrackets > 0)
                            {
                                openedBrackets--;
                                result.AppendLine(string.Concat(spaces, "  }"));
                                if (spaces.Length >= 2)
                                    spaces = new string(' ', spaces.Length - 2);
                            }

                            //Unsignal UNION/MINUS semantic
                            printingUnion = false;
                            printingMinus = false;
                        }
                        else
                        {
                            result.AppendLine($"{spaces}    {PrintValues(vlPgMember, prefixes, spaces)} .");
                        }
                        break;
                    }
                    case RDFBind bdPgMember:
                    {
                        //We can directly print the bind
                        result.AppendLine($"{spaces}    {PrintBind(bdPgMember, prefixes)} .");

                        //Then we need to print all the pending brackets and to keep care of
                        //reflecting this in the indentation spaces (-2) which must be consumed
                        while (openedBrackets > 0)
                        {
                            openedBrackets--;
                            result.AppendLine(string.Concat(spaces, "  }"));
                            if (spaces.Length >= 2)
                                spaces = new string(' ', spaces.Length - 2);
                        }

                        //Unsignal UNION/MINUS semantic
                        printingUnion = false;
                        printingMinus = false;
                        break;
                    }
                }
                #endregion
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
        /// Prints the string representation of a property path
        /// </summary>
        internal static string PrintPropertyPath(RDFPropertyPath propertyPath, List<RDFNamespace> prefixes)
        {
            StringBuilder result = new StringBuilder(64);
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
                            result.Append((char)propertyPath.Steps[i].StepFlavor);
                        }
                        else
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
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
                            result.Append((char)propertyPath.Steps[i].StepFlavor);
                        }
                        else
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                        }
                    }
            }
            #endregion

            result.Append(' ');
            result.Append(PrintPatternMember(propertyPath.End, prefixes));
            return result.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL values
        /// </summary>
        internal static string PrintValues(RDFValues values, List<RDFNamespace> prefixes, string spaces)
        {
            StringBuilder result = new StringBuilder(64);

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
        #endregion
    }
}