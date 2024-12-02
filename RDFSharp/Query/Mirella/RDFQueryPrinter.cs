/*
   Copyright 2012-2024 Marco De Salvo

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

using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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
            int subqueryHeaderSpacesFunc(double indLevel)
                => subqueryBodySpacesFunc(indentLevel) - 2 < 0 ? 0 : subqueryBodySpacesFunc(indentLevel) - 2;
            int subqueryBodySpacesFunc(double indLevel)
                => Convert.ToInt32(4.0d * indentLevel);
            int subqueryUnionOrMinusSpacesFunc(bool unionOrMinus)
                => unionOrMinus ? 2 : 0;

            string subquerySpaces = new string(' ', subqueryHeaderSpacesFunc(indentLevel) + subqueryUnionOrMinusSpacesFunc(fromUnionOrMinus));
            string subqueryBodySpaces = new string(' ', subqueryBodySpacesFunc(indentLevel) + subqueryUnionOrMinusSpacesFunc(fromUnionOrMinus));
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
            sb.Append(string.Concat(subqueryBodySpaces, "SELECT"));
            #endregion

            #region DISTINCT
            selectQuery.GetModifiers()
                       .OfType<RDFDistinctModifier>()
                       .ToList()
                       .ForEach(dm => sb.Append(string.Concat(" ", dm)));
            #endregion

            #region VARIABLES/AGGREGATORS
            List<RDFModifier> modifiers = selectQuery.GetModifiers().ToList();

            //Query has GroupBy modifier => reset given projections, modifier takes control
            if (modifiers.Any(m => m is RDFGroupByModifier))
            {
                modifiers.OfType<RDFGroupByModifier>()
                         .ToList()
                         .ForEach(gm =>
                         {
                             sb.Append(' ');
                             sb.Append(string.Join(" ", gm.PartitionVariables));
                             sb.Append(' ');
                             sb.Append(string.Join(" ", gm.Aggregators.Where(ag => !(ag is RDFPartitionAggregator))));
                         });
            }

            //Query hasn't GroupBy modifier => respect given projections
            else
            {
                if (!selectQuery.ProjectionVars.Any())
                    sb.Append(" *");
                else
                {
                    foreach (KeyValuePair<RDFVariable, (int, RDFExpression)> projectionElement in selectQuery.ProjectionVars.OrderBy(pv => pv.Value.Item1))
                    {
                        //Projection Variable
                        if (projectionElement.Value.Item2 == null)
                            sb.Append($" {projectionElement.Key}");
                        //Projection Expression
                        else
                            sb.Append($" ({projectionElement.Value.Item2.ToString(prefixes)} AS {projectionElement.Key})");
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
            if (modifiers.Any(mod => mod is RDFGroupByModifier))
            {
                modifiers.OfType<RDFGroupByModifier>()
                         .ToList()
                         .ForEach(gm =>
                         {
                             //GROUP BY
                             sb.AppendLine();
                             sb.Append(subqueryBodySpaces + gm);
                             //HAVING
                             if (gm.Aggregators.Any(ag => ag.HavingClause.Item1))
                             {
                                 sb.AppendLine();
                                 sb.Append(string.Format(string.Concat(subqueryBodySpaces, "HAVING ({0})"), string.Join(" && ", gm.Aggregators.Where(ag => ag.HavingClause.Item1).Select(x => x.PrintHavingClause(selectQuery.Prefixes)))));
                             }
                         });
            }

            // ORDER BY
            if (modifiers.Any(mod => mod is RDFOrderByModifier))
            {
                sb.AppendLine();
                sb.Append(string.Concat(subqueryBodySpaces, "ORDER BY"));
                modifiers.OfType<RDFOrderByModifier>()
                         .ToList()
                         .ForEach(om => sb.Append(string.Concat(" ", om.ToString())));
            }

            // LIMIT/OFFSET
            if (modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier))
            {
                modifiers.OfType<RDFLimitModifier>()
                         .ToList()
                         .ForEach(lim => { sb.AppendLine(); sb.Append(string.Concat(subqueryBodySpaces, lim.ToString())); });
                modifiers.OfType<RDFOffsetModifier>()
                         .ToList()
                         .ForEach(off => { sb.AppendLine(); sb.Append(string.Concat(subqueryBodySpaces, off.ToString())); });
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
            StringBuilder sb = new StringBuilder();
            if (describeQuery == null)
                return sb.ToString();

            #region PREFIXES
            List<RDFNamespace> prefixes = PrintPrefixes(describeQuery, sb, true);
            #endregion

            #region DESCRIBE
            sb.Append("DESCRIBE");
            if (describeQuery.DescribeTerms.Any())
                describeQuery.DescribeTerms.ForEach(dt => sb.Append(string.Concat(" ", PrintPatternMember(dt, describeQuery.Prefixes))));
            else
                sb.Append(" *");
            sb.AppendLine();
            #endregion

            #region WHERE
            PrintWhereClause(describeQuery, sb, prefixes, string.Empty, 0, false);
            #endregion

            #region MODIFIERS
            List<RDFModifier> modifiers = describeQuery.GetModifiers().ToList();
            // LIMIT/OFFSET
            if (modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier))
            {
                modifiers.OfType<RDFLimitModifier>()
                         .ToList()
                         .ForEach(lim => { sb.AppendLine(); sb.Append(lim); });
                modifiers.OfType<RDFOffsetModifier>()
                         .ToList()
                         .ForEach(off => { sb.AppendLine(); sb.Append(off); });
            }
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
                    tpString = tpString.Replace(string.Concat("GRAPH ", tpContext, " { "), string.Empty).TrimEnd(new char[] { ' ', '}' });
                }

                //Remove Optional indicator from the template print (since it is not supported by CONSTRUCT query)
                if (tp.IsOptional)
                    tpString = tpString.Replace("OPTIONAL { ", string.Empty).TrimEnd(new char[] { ' ', '}' });

                sb.AppendLine(string.Concat("  ", tpString, " ."));
            });
            sb.AppendLine("}");
            #endregion

            #region WHERE
            PrintWhereClause(constructQuery, sb, prefixes, string.Empty, 0, false);
            #endregion

            #region MODIFIERS
            List<RDFModifier> modifiers = constructQuery.GetModifiers().ToList();
            // LIMIT/OFFSET
            if (modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier))
            {
                modifiers.OfType<RDFLimitModifier>()
                         .ToList()
                         .ForEach(lim => { sb.AppendLine(); sb.Append(lim); });
                modifiers.OfType<RDFOffsetModifier>()
                         .ToList()
                         .ForEach(off => { sb.AppendLine(); sb.Append(off); });
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
            if (enablePrinting && prefixes.Any())
            {
                prefixes.ForEach(pf => sb.AppendLine(string.Concat("PREFIX ", pf.NamespacePrefix, ": <", pf.NamespaceUri.ToString(), ">")));
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
                    sb.Append(PrintPatternGroup(patternGroup, subqueryBodySpaces.Length + 2, true, prefixes));
            }
            #endregion

            sb.AppendLine(string.Concat(subqueryBodySpaces, "WHERE {"));

            #region WhereBody
            bool printingUnion = false;
            bool printingMinus = false;
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();
            RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
            foreach (RDFQueryMember queryMember in evaluableQueryMembers)
            {
                #region PATTERNGROUP
                if (queryMember is RDFPatternGroup pgQueryMember)
                {
                    //PatternGroup is set as UNION with the next query member and it IS NOT the last one => append UNION
                    if (pgQueryMember.JoinAsUnion && !pgQueryMember.Equals(lastQueryMbr))
                    {
                        if (!printingUnion)
                        {
                            //Begin new UNION block
                            printingUnion = true;

                            //Adjust indentation level in case of active MINUS
                            if (printingMinus)
                                subqueryBodySpaces = string.Concat(subqueryBodySpaces, "  ");

                            sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                        }
                        PrintWrappedPatternGroup(pgQueryMember);
                        sb.AppendLine(string.Concat(subqueryBodySpaces, "    UNION"));
                    }

                    //PatternGroup is set as MINUS with the next query member and it IS NOT the last one => append MINUS
                    else if (pgQueryMember.JoinAsMinus && !pgQueryMember.Equals(lastQueryMbr))
                    {
                        if (!printingMinus)
                        {
                            //Begin new MINUS block
                            printingMinus = true;

                            //Adjust indentation level in case of active UNION
                            if (printingUnion)
                                subqueryBodySpaces = string.Concat(subqueryBodySpaces, "  ");

                            sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                        }
                        PrintWrappedPatternGroup(pgQueryMember);
                        sb.AppendLine(string.Concat(subqueryBodySpaces, "    MINUS"));
                    }

                    //PatternGroup is set as INTERSECT with the next query member or it IS the last one => do not append UNION/MINUS
                    else
                    {
                        if (printingUnion || printingMinus)
                        {
                            bool printingBoth = printingUnion && printingMinus;

                            //End active UNION block
                            printingUnion = false;
                            //End active MINUS block
                            printingMinus = false;

                            PrintWrappedPatternGroup(pgQueryMember);

                            //Restore indentation level in case of active UNION+MINUS
                            if (printingBoth)
                            {
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                                subqueryBodySpaces = new string(' ', subqueryBodySpaces.Length - 2);
                            }

                            sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                        }
                        else
                            sb.Append(PrintPatternGroup(pgQueryMember, subqueryBodySpaces.Length, false, prefixes));
                    }
                }
                #endregion

                #region SUBQUERY
                else if (queryMember is RDFSelectQuery sqQueryMember)
                {
                    //Merge main query prefixes
                    prefixes.ForEach(pf1 => sqQueryMember.AddPrefix(pf1));

                    //SubQuery is set as UNION with the next query member and it IS NOT the last one => append UNION
                    if (sqQueryMember.JoinAsUnion && !sqQueryMember.Equals(lastQueryMbr))
                    {
                        if (!printingUnion)
                        {
                            //Begin new UNION block
                            printingUnion = true;

                            //Adjust indentation level in case of active MINUS
                            if (printingMinus)
                            {
                                subqueryBodySpaces = string.Concat(subqueryBodySpaces, "  ");
                                indentLevel += 0.5;
                            }

                            sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                        }
                        sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), true));
                        sb.AppendLine(string.Concat(subqueryBodySpaces, "    UNION"));
                    }

                    //SubQuery is set as MINUS with the next query member and it IS NOT the last one => append MINUS
                    else if (sqQueryMember.JoinAsMinus && !sqQueryMember.Equals(lastQueryMbr))
                    {
                        if (!printingMinus)
                        {
                            //Begin new MINUS block
                            printingMinus = true;

                            //Adjust indentation level in case of active UNION
                            if (printingUnion)
                            {
                                subqueryBodySpaces = string.Concat(subqueryBodySpaces, "  ");
                                indentLevel += 0.5;
                            }

                            sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                        }
                        sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), true));
                        sb.AppendLine(string.Concat(subqueryBodySpaces, "    MINUS"));
                    }

                    //SubQuery is set as INTERSECT with the next query member or it IS the last one => do not append UNION/MINUS
                    else
                    {
                        if (printingUnion || printingMinus)
                        {
                            bool printingBoth = printingUnion && printingMinus;

                            //End active UNION block
                            printingUnion = false;
                            //End active MINUS block
                            printingMinus = false;

                            sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), true));

                            //Restore indentation level in case of active UNION+MINUS
                            if (printingBoth)
                            {
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                                subqueryBodySpaces = new string(' ', subqueryBodySpaces.Length - 2);
                                indentLevel -= 0.5;
                            }

                            sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                        }
                        else
                            sb.Append(PrintSelectQuery(sqQueryMember, indentLevel + 1 + (fromUnionOrMinus ? 0.5 : 0), false));
                    }
                }
                #endregion
            }
            #endregion

            sb.Append(string.Concat(subqueryBodySpaces, "}"));
        }

        /// <summary>
        /// Prints the string representation of a pattern group
        /// </summary>
        internal static string PrintPatternGroup(RDFPatternGroup patternGroup, int spaceIndent, bool skipOptional, List<RDFNamespace> prefixes)
        {
            string spaces = new StringBuilder().Append(' ', spaceIndent < 0 ? 0 : spaceIndent).ToString();

            #region HEADER
            StringBuilder result = new StringBuilder();
            
            //OPTIONAL
            if (patternGroup.IsOptional && !skipOptional)
            {
                result.AppendLine(string.Concat("  ", spaces, "OPTIONAL {"));
                spaces = string.Concat(spaces, "  ");
            }

            //SERVICE
            if (patternGroup.EvaluateAsService.HasValue)
            {
                bool isSilent = patternGroup.EvaluateAsService.Value.Item2.ErrorBehavior == RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult;
                string service = string.Concat("SERVICE ",  isSilent ? "SILENT " : string.Empty);
                result.AppendLine(string.Concat("  ", spaces, service, "<", patternGroup.EvaluateAsService.Value.Item1 , "> {"));
                spaces = string.Concat(spaces, "  ");
            }

            result.AppendLine(string.Concat(spaces, "  {"));
            #endregion

            #region MEMBERS
            bool printingUnion = false;
            bool printingMinus = false;
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            for (int i=0; i<evaluablePGMembers.Count; i++)
            {
                RDFPatternGroupMember pgMember = evaluablePGMembers[i];
                RDFPatternGroupMember nextPgMember = (i < evaluablePGMembers.Count-1 ? evaluablePGMembers[i+1] : null);
                bool thisIsLastPgMemberOrNextPgMemberIsBind = (nextPgMember == null || nextPgMember is RDFBind);

                #region PATTERN
                if (pgMember is RDFPattern ptPgMember)
                {
                    //Pattern is set as UNION with the next pg member and it IS NOT the last one => append UNION
                    if (ptPgMember.JoinAsUnion && !thisIsLastPgMemberOrNextPgMemberIsBind)
                    {
                        //Begin new UNION block
                        printingUnion = true;

                        //Adjust indentation level in case of active MINUS
                        if (printingMinus)
                        {
                            spaces = string.Concat(spaces, "  ");
                            result.AppendLine(string.Concat(spaces, "  {"));
                        }

                        result.AppendLine(string.Concat(spaces, "    { ", PrintPattern(ptPgMember, prefixes), " }"));
                        result.AppendLine(string.Concat(spaces, "    UNION"));
                    }

                    //Pattern is set as MINUS with the next pg member and it IS NOT the last one => append MINUS
                    else if (ptPgMember.JoinAsMinus && !thisIsLastPgMemberOrNextPgMemberIsBind)
                    {
                        //Begin new MINUS block
                        printingMinus = true;

                        result.AppendLine(string.Concat(spaces, "    { ", PrintPattern(ptPgMember, prefixes), " }"));
                        result.AppendLine(string.Concat(spaces, "    MINUS"));
                    }

                    //Pattern is set as INTERSECT with the next pg member or it IS the last one => do not append UNION/MINUS
                    else
                    {
                        if (printingUnion || printingMinus)
                        {
                            result.AppendLine(string.Concat(spaces, "    { ", PrintPattern(ptPgMember, prefixes), " }"));

                            //Restore indentation level in case of active UNION+MINUS
                            if (printingUnion && printingMinus)
                            {
                                result.AppendLine(string.Concat(spaces, "  }"));
                                spaces = new string(' ', spaces.Length - 2);
                            }

                            //End active UNION block
                            printingUnion = false;
                            //End active MINUS block
                            printingMinus = false;
                        }
                        else
                            result.AppendLine(string.Concat(spaces, "    ", PrintPattern(ptPgMember, prefixes), " ."));
                    }
                }
                #endregion

                #region PROPERTY PATH
                else if (pgMember is RDFPropertyPath ppPgMember && ppPgMember.IsEvaluable)
                {
                    if (printingUnion || printingMinus)
                    {
                        result.AppendLine(string.Concat(spaces, "    { ", PrintPropertyPath(ppPgMember, prefixes), " }"));

                        //End active UNION block
                        printingUnion = false;
                        //End active MINUS block
                        printingMinus = false;
                    }
                    else
                        result.AppendLine(string.Concat(spaces, "    ", PrintPropertyPath(ppPgMember, prefixes), " ."));
                }
                #endregion

                #region VALUES
                else if (pgMember is RDFValues vlPgMember && vlPgMember.IsEvaluable && !vlPgMember.IsInjected)
                {
                    if (printingUnion || printingMinus)
                    {
                        result.AppendLine(string.Concat(spaces, "    { ", PrintValues(vlPgMember, prefixes, spaces), " }"));

                        //End active UNION block
                        printingUnion = false;
                        //End active MINUS block
                        printingMinus = false;
                    }
                    else
                        result.AppendLine(string.Concat(spaces, "    ", PrintValues(vlPgMember, prefixes, spaces), " ."));
                }
                #endregion

                #region BIND
                else if (pgMember is RDFBind bdPgMember)
                {
                    result.AppendLine(string.Concat(spaces, "    ", PrintBind(bdPgMember, prefixes), " ."));

                    //End active UNION block
                    printingUnion = false;
                    //End active MINUS block
                    printingMinus = false;
                }
                #endregion
            }
            #endregion

            #region FILTERS
            patternGroup.GetFilters().Where(f => !(f is RDFValuesFilter))
                                     .ToList()
                                     .ForEach(f => result.AppendLine(string.Concat(spaces, "    ", f.ToString(prefixes), " ")));
            #endregion

            #region CLOSURE
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
            #endregion

            return result.ToString();
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
                if (pattern.IsOptional)
                    return string.Concat("OPTIONAL { GRAPH ", ctx, " { ", subj, " ", pred, " ", obj, " } }");
                return string.Concat("GRAPH ", ctx, " { ", subj, " ", pred, " ", obj, " }");
            }

            //SPO pattern
            if (pattern.IsOptional)
                return string.Concat("OPTIONAL { ", subj, " ", pred, " ", obj, " }");
            return string.Concat(subj, " ", pred, " ", obj);
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
            }
            #endregion

            #region Multiple Properties
            else
            {
                //Initialize printing
                bool openedParenthesis = false;

                //Iterate properties
                for (int i = 0; i < propertyPath.Steps.Count; i++)
                {
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
            StringBuilder result = new StringBuilder();

            //Compact representation
            if (values.Bindings.Keys.Count == 1)
            {
                result.Append(string.Format("VALUES {0}", values.Bindings.Keys.ElementAt(0)));
                result.Append(" { ");
                foreach (RDFPatternMember binding in values.Bindings.ElementAt(0).Value)
                {
                    if (binding == null)
                        result.Append("UNDEF");
                    else
                        result.Append(RDFQueryPrinter.PrintPatternMember(binding, prefixes));
                    result.Append(' ');
                }
                result.Append('}');
            }

            //Extended representation
            else
            {
                result.Append(string.Format("VALUES ({0})", string.Join(" ", values.Bindings.Keys)));
                result.AppendLine(" {");
                for (int i = 0; i < values.MaxBindingsLength(); i++)
                {
                    result.Append(string.Concat(spaces, "      ( "));
                    values.Bindings.ToList().ForEach(binding =>
                    {
                        RDFPatternMember bindingValue = binding.Value.ElementAtOrDefault(i);
                        if (bindingValue == null)
                            result.Append("UNDEF");
                        else
                            result.Append(RDFQueryPrinter.PrintPatternMember(bindingValue, prefixes));
                        result.Append(' ');
                    });
                    result.AppendLine(")");
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
            if (patternMember == null)
                return null;

            #region Variable
            if (patternMember is RDFVariable varPatternMember)
                return varPatternMember.ToString();
            #endregion

            #region Resource/Context
            if (patternMember is RDFResource || patternMember is RDFContext)
            {
                #region Blank
                if (patternMember is RDFResource resPatternMember && resPatternMember.IsBlank)
                    return resPatternMember.ToString().Replace("bnode:", "_:");
                #endregion

                #region NonBlank
                (bool, string) abbreviatedPM = RDFQueryUtilities.AbbreviateRDFPatternMember(patternMember, prefixes);
                if (abbreviatedPM.Item1)
                    return abbreviatedPM.Item2;
                else
                    return string.Concat("<", abbreviatedPM.Item2, ">");
                #endregion
            }
            #endregion

            #region PlainLiteral
            if (patternMember is RDFPlainLiteral plPatternMember)
            {
                if (plPatternMember.HasLanguage())
                    return string.Concat("\"", plPatternMember.Value, "\"@", plPatternMember.Language);
                return string.Concat("\"", plPatternMember.Value, "\"");
            }
            #endregion

            #region TypedLiteral
            else if (patternMember is RDFTypedLiteral tlPatternMember)
            {
                string tlDatatype = tlPatternMember.Datatype.URI.ToString();
                (bool, string) abbreviatedPM = RDFQueryUtilities.AbbreviateRDFPatternMember(RDFQueryUtilities.ParseRDFPatternMember(tlDatatype), prefixes);
                if (abbreviatedPM.Item1)
                    return string.Concat("\"", tlPatternMember.Value, "\"^^", abbreviatedPM.Item2);
                else
                    return string.Concat("\"", tlPatternMember.Value, "\"^^<", abbreviatedPM.Item2, ">");
            }
            #endregion

            return null;
        }
        #endregion
    }
}