/*
   Copyright 2012-2022 Marco De Salvo

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
    /// RDFQueryPrinter is responsible for getting string representation of SPARQL query entities
    /// </summary>
    internal static class RDFQueryPrinter
    {
        #region Methods
        /// <summary>
        /// Prints the string representation of a SPARQL SELECT query
        /// </summary>
        internal static string PrintSelectQuery(RDFSelectQuery selectQuery, double indentLevel, bool fromUnion)
        {
            StringBuilder sb = new StringBuilder();
            if (selectQuery != null)
            {
                #region INDENT
                int subqueryHeaderSpacesFunc(double indLevel) { return subqueryBodySpacesFunc(indentLevel) - 2 < 0 ? 0 : subqueryBodySpacesFunc(indentLevel) - 2; }
                int subqueryBodySpacesFunc(double indLevel) { return Convert.ToInt32(4 * indentLevel); }
                int subqueryUnionSpacesFunc(bool union) { return union ? 2 : 0; }

                string subquerySpaces = new string(' ', subqueryHeaderSpacesFunc(indentLevel) + subqueryUnionSpacesFunc(fromUnion));
                string subqueryBodySpaces = new string(' ', subqueryBodySpacesFunc(indentLevel) + subqueryUnionSpacesFunc(fromUnion));
                #endregion

                #region PREFIX
                List<RDFNamespace> prefixes = selectQuery.GetPrefixes();
                if (!selectQuery.IsSubQuery)
                {
                    if (prefixes.Any())
                    {
                        prefixes.ForEach(pf => sb.AppendLine(string.Concat("PREFIX ", pf.NamespacePrefix, ": <", pf.NamespaceUri.ToString())));
                        sb.AppendLine();
                    }
                }
                #endregion

                #region HEADER

                #region BEGINSELECT
                if (selectQuery.IsSubQuery)
                {
                    if (selectQuery.IsOptional && !fromUnion)
                        sb.AppendLine(string.Concat(subquerySpaces, "OPTIONAL {"));
                    else
                        sb.AppendLine(string.Concat(subquerySpaces, "{"));
                }
                sb.Append(string.Concat(subqueryBodySpaces, "SELECT"));
                #endregion

                #region DISTINCT
                selectQuery.GetModifiers()
                           .Where(mod => mod is RDFDistinctModifier)
                           .ToList()
                           .ForEach(dm => sb.Append(string.Concat(" ", dm)));
                #endregion

                #region VARIABLES/AGGREGATORS
                List<RDFModifier> modifiers = selectQuery.GetModifiers().ToList();
                //Query has groupby modifier
                if (modifiers.Any(m => m is RDFGroupByModifier))
                {
                    modifiers.Where(mod => mod is RDFGroupByModifier)
                             .ToList()
                             .ForEach(gm =>
                             {
                                 sb.Append(" ");
                                 sb.Append(string.Join(" ", ((RDFGroupByModifier)gm).PartitionVariables));
                                 sb.Append(" ");
                                 sb.Append(string.Join(" ", ((RDFGroupByModifier)gm).Aggregators.Where(ag => !(ag is RDFPartitionAggregator))));
                             });
                }
                //Query hasn't groupby modifier
                else
                {
                    if (selectQuery.ProjectionVars.Any())
                    {
                        selectQuery.ProjectionVars.OrderBy(x => x.Value)
                                                  .ToList()
                                                  .ForEach(v => sb.Append(string.Concat(" ", v.Key)));
                    }
                    else
                    {
                        sb.Append(" *");
                    }
                }
                sb.AppendLine();
                #endregion

                #endregion

                #region BODY
                sb.AppendLine(string.Concat(subqueryBodySpaces, "WHERE {"));

                #region MEMBERS
                bool printingUnion = false;
                List<RDFQueryMember> evaluableQueryMembers = selectQuery.GetEvaluableQueryMembers().ToList();
                RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
                foreach (RDFQueryMember queryMember in evaluableQueryMembers)
                {

                    #region PATTERNGROUPS
                    if (queryMember is RDFPatternGroup)
                    {

                        //Current pattern group is set as UNION with the next one
                        if (((RDFPatternGroup)queryMember).JoinAsUnion)
                        {

                            //Current pattern group IS NOT the last of the query
                            //(so UNION keyword must be appended at last)
                            if (!queryMember.Equals(lastQueryMbr))
                            {
                                //Begin a new Union block
                                if (!printingUnion)
                                {
                                    printingUnion = true;
                                    sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                                }
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length + 2, true, prefixes));
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "    UNION"));
                            }

                            //Current pattern group IS the last of the query
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length + 2, true, prefixes));
                                    sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                                }
                                else
                                {
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length, false, prefixes));
                                }
                            }

                        }

                        //Current pattern group is set as INTERSECT with the next one
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length + 2, true, prefixes));
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length, false, prefixes));
                            }
                        }

                    }
                    #endregion

                    #region SUBQUERY
                    else if (queryMember is RDFQuery)
                    {
                        //Merge main query prefixes
                        selectQuery.GetPrefixes()
                                   .ForEach(pf1 => ((RDFSelectQuery)queryMember).AddPrefix(pf1));

                        //Current subquery is set as UNION with the next one
                        if (((RDFSelectQuery)queryMember).JoinAsUnion)
                        {

                            //Current subquery IS NOT the last of the query
                            //(so UNION keyword must be appended at last)
                            if (!queryMember.Equals(lastQueryMbr))
                            {
                                //Begin a new Union block
                                if (!printingUnion)
                                {
                                    printingUnion = true;
                                    sb.AppendLine(string.Concat(subqueryBodySpaces, "  {"));
                                }
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, indentLevel + 1 + (fromUnion ? 0.5 : 0), true));
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "    UNION"));
                            }

                            //Current query IS the last of the query
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, indentLevel + 1 + (fromUnion ? 0.5 : 0), true));
                                    sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                                }
                                else
                                {
                                    sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, indentLevel + 1 + (fromUnion ? 0.5 : 0), false));
                                }
                            }

                        }

                        //Current query is set as INTERSECT with the next one
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, indentLevel + 1 + (fromUnion ? 0.5 : 0), true));
                                sb.AppendLine(string.Concat(subqueryBodySpaces, "  }"));
                            }
                            else
                            {
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, indentLevel + 1 + (fromUnion ? 0.5 : 0), false));
                            }
                        }
                    }
                    #endregion

                }
                #endregion

                sb.Append(string.Concat(subqueryBodySpaces, "}"));
                #endregion

                #region FOOTER

                #region MODIFIERS
                //GROUP BY
                if (modifiers.Any(mod => mod is RDFGroupByModifier))
                {
                    modifiers.Where(mod => mod is RDFGroupByModifier)
                             .ToList()
                             .ForEach(gm =>
                             {
                                 //GROUP BY
                                 sb.AppendLine();
                                 sb.Append(subqueryBodySpaces + gm);
                                 //HAVING
                                 if (((RDFGroupByModifier)gm).Aggregators.Any(ag => ag.HavingClause.Item1))
                                 {
                                     sb.AppendLine();
                                     sb.Append(string.Format(string.Concat(subqueryBodySpaces, "HAVING ({0})"), string.Join(" && ", ((RDFGroupByModifier)gm).Aggregators.Where(ag => ag.HavingClause.Item1).Select(x => x.PrintHavingClause(selectQuery.Prefixes)))));
                                 }
                             });
                }

                // ORDER BY
                if (modifiers.Any(mod => mod is RDFOrderByModifier))
                {
                    sb.AppendLine();
                    sb.Append(string.Concat(subqueryBodySpaces, "ORDER BY"));
                    modifiers.Where(mod => mod is RDFOrderByModifier)
                             .ToList()
                             .ForEach(om => sb.Append(string.Concat(" ", om.ToString())));
                }

                // LIMIT/OFFSET
                if (modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier))
                {
                    modifiers.Where(mod => mod is RDFLimitModifier)
                             .ToList()
                             .ForEach(lim => { sb.AppendLine(); sb.Append(string.Concat(subqueryBodySpaces, lim.ToString())); });
                    modifiers.Where(mod => mod is RDFOffsetModifier)
                             .ToList()
                             .ForEach(off => { sb.AppendLine(); sb.Append(string.Concat(subqueryBodySpaces, off.ToString())); });
                }
                #endregion

                #region ENDSELECT
                sb.AppendLine();
                if (selectQuery.IsSubQuery)
                    sb.AppendLine(string.Concat(subquerySpaces, "}"));
                #endregion

                #endregion
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL DESCRIBE query
        /// </summary>
        internal static string PrintDescribeQuery(RDFDescribeQuery describeQuery)
        {
            StringBuilder sb = new StringBuilder();
            if (describeQuery != null)
            {
                #region PREFIXES
                List<RDFNamespace> prefixes = describeQuery.GetPrefixes();
                if (prefixes.Any())
                {
                    prefixes.ForEach(pf => sb.AppendLine(string.Concat("PREFIX ", pf.NamespacePrefix, ": <", pf.NamespaceUri.ToString())));
                    sb.AppendLine();
                }
                #endregion

                #region HEADER

                #region BEGINDESCRIBE
                sb.Append("DESCRIBE");
                #endregion

                #region TERMS
                if (describeQuery.DescribeTerms.Any())
                {
                    describeQuery.DescribeTerms.ForEach(dt =>
                    {
                        sb.Append(string.Concat(" ", PrintPatternMember(dt, describeQuery.Prefixes)));
                    });
                }
                else
                {
                    sb.Append(" *");
                }
                sb.AppendLine();
                #endregion

                #endregion

                #region BODY
                sb.AppendLine("WHERE {");

                #region MEMBERS
                bool printingUnion = false;
                List<RDFQueryMember> evaluableQueryMembers = describeQuery.GetEvaluableQueryMembers().ToList();
                RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
                foreach (RDFQueryMember queryMember in evaluableQueryMembers)
                {

                    #region PATTERNGROUPS
                    if (queryMember is RDFPatternGroup)
                    {

                        //Current pattern group is set as UNION with the next one
                        if (((RDFPatternGroup)queryMember).JoinAsUnion)
                        {

                            //Current pattern group IS NOT the last of the query
                            //(so UNION keyword must be appended at last)
                            if (!queryMember.Equals(lastQueryMbr))
                            {
                                //Begin a new Union block
                                if (!printingUnion)
                                {
                                    printingUnion = true;
                                    sb.AppendLine("  {");
                                }
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                sb.AppendLine("    UNION");
                            }

                            //Current pattern group IS the last of the query
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                    sb.AppendLine("  }");
                                }
                                else
                                {
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, prefixes));
                                }
                            }

                        }

                        //Current pattern group is set as INTERSECT with the next one
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                sb.AppendLine("  }");
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, prefixes));
                            }
                        }

                    }
                    #endregion

                    #region SUBQUERY
                    else if (queryMember is RDFQuery)
                    {
                        //Merge main query prefixes
                        describeQuery.GetPrefixes()
                                     .ForEach(pf1 => ((RDFSelectQuery)queryMember).AddPrefix(pf1));

                        //Current subquery is set as UNION with the next one
                        if (((RDFSelectQuery)queryMember).JoinAsUnion)
                        {

                            //Current subquery IS NOT the last of the query
                            //(so UNION keyword must be appended at last)
                            if (!queryMember.Equals(lastQueryMbr))
                            {
                                //Begin a new Union block
                                if (!printingUnion)
                                {
                                    printingUnion = true;
                                    sb.AppendLine("  {");
                                }
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.AppendLine("    UNION");
                            }

                            //Current query IS the last of the query
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                    sb.AppendLine("  }");
                                }
                                else
                                {
                                    sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, false));
                                }
                            }

                        }

                        //Current query is set as INTERSECT with the next one
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.AppendLine("  }");
                            }
                            else
                            {
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, false));
                            }
                        }
                    }
                    #endregion

                }
                #endregion

                sb.Append("}");
                #endregion

                #region FOOTER

                #region MODIFIERS
                List<RDFModifier> modifiers = describeQuery.GetModifiers().ToList();

                // LIMIT/OFFSET
                if (modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier))
                {
                    modifiers.Where(mod => mod is RDFLimitModifier)
                             .ToList()
                             .ForEach(lim => { sb.AppendLine(); sb.Append(lim); });
                    modifiers.Where(mod => mod is RDFOffsetModifier)
                             .ToList()
                             .ForEach(off => { sb.AppendLine(); sb.Append(off); });
                }
                #endregion

                #endregion
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL CONSTRUCT query
        /// </summary>
        internal static string PrintConstructQuery(RDFConstructQuery constructQuery)
        {
            StringBuilder sb = new StringBuilder();
            if (constructQuery != null)
            {
                #region PREFIXES
                List<RDFNamespace> prefixes = constructQuery.GetPrefixes();
                if (prefixes.Any())
                {
                    prefixes.ForEach(pf => sb.AppendLine(string.Concat("PREFIX ", pf.NamespacePrefix, ": <", pf.NamespaceUri.ToString(), ">")));
                    sb.AppendLine();
                }
                #endregion

                #region HEADER

                #region BEGINCONSTRUCT
                sb.Append("CONSTRUCT");
                #endregion

                #region TEMPLATES
                sb.AppendLine();
                sb.AppendLine("{");
                constructQuery.Templates.ForEach(tp =>
                {
                    string tpString = PrintPattern(tp, constructQuery.Prefixes);

                    //Remove the Context from the template print (since it is not supported by CONSTRUCT query)
                    if (tp.Context != null)
                        tpString = tpString.Replace(string.Concat("GRAPH ", tp.Context, " { "), string.Empty).TrimEnd(new char[] { ' ', '}' });

                    //Remove the Optional indicator from the template print (since it is not supported by CONSTRUCT query)
                    if (tp.IsOptional)
                        tpString = tpString.Replace("OPTIONAL { ", string.Empty).TrimEnd(new char[] { ' ', '}' });

                    sb.AppendLine(string.Concat("  ", tpString, " ."));
                });
                sb.AppendLine("}");
                #endregion

                #endregion

                #region BODY
                sb.AppendLine("WHERE {");

                #region MEMBERS
                bool printingUnion = false;
                List<RDFQueryMember> evaluableQueryMembers = constructQuery.GetEvaluableQueryMembers().ToList();
                RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
                foreach (RDFQueryMember queryMember in evaluableQueryMembers)
                {
                    #region PATTERNGROUPS
                    if (queryMember is RDFPatternGroup)
                    {

                        //Current pattern group is set as UNION with the next one
                        if (((RDFPatternGroup)queryMember).JoinAsUnion)
                        {

                            //Current pattern group IS NOT the last of the query
                            //(so UNION keyword must be appended at last)
                            if (!queryMember.Equals(lastQueryMbr))
                            {
                                //Begin a new Union block
                                if (!printingUnion)
                                {
                                    printingUnion = true;
                                    sb.AppendLine("  {");
                                }
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                sb.AppendLine("    UNION");
                            }

                            //Current pattern group IS the last of the query
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                    sb.AppendLine("  }");
                                }
                                else
                                {
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, prefixes));
                                }
                            }

                        }

                        //Current pattern group is set as INTERSECT with the next one
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                sb.AppendLine("  }");
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, prefixes));
                            }
                        }

                    }
                    #endregion

                    #region SUBQUERY
                    else if (queryMember is RDFQuery)
                    {
                        //Merge main query prefixes
                        constructQuery.GetPrefixes()
                                      .ForEach(pf1 => ((RDFSelectQuery)queryMember).AddPrefix(pf1));

                        //Current subquery is set as UNION with the next one
                        if (((RDFSelectQuery)queryMember).JoinAsUnion)
                        {

                            //Current subquery IS NOT the last of the query
                            //(so UNION keyword must be appended at last)
                            if (!queryMember.Equals(lastQueryMbr))
                            {
                                //Begin a new Union block
                                if (!printingUnion)
                                {
                                    printingUnion = true;
                                    sb.AppendLine("  {");
                                }
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.AppendLine("    UNION");
                            }

                            //Current query IS the last of the query
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                    sb.AppendLine("  }");
                                }
                                else
                                {
                                    sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, false));
                                }
                            }

                        }

                        //Current query is set as INTERSECT with the next one
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.AppendLine("  }");
                            }
                            else
                            {
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, false));
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                sb.Append("}");
                #endregion

                #region FOOTER

                #region MODIFIERS
                List<RDFModifier> modifiers = constructQuery.GetModifiers().ToList();

                // LIMIT/OFFSET
                if (modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier))
                {
                    modifiers.Where(mod => mod is RDFLimitModifier)
                             .ToList()
                             .ForEach(lim => { sb.AppendLine(); sb.Append(lim); });
                    modifiers.Where(mod => mod is RDFOffsetModifier)
                             .ToList()
                             .ForEach(off => { sb.AppendLine(); sb.Append(off); });
                }
                #endregion

                #endregion
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL ASK query
        /// </summary>
        internal static string PrintAskQuery(RDFAskQuery askQuery)
        {
            StringBuilder sb = new StringBuilder();
            if (askQuery != null)
            {
                #region PREFIXES
                List<RDFNamespace> prefixes = askQuery.GetPrefixes();
                if (prefixes.Any())
                {
                    prefixes.ForEach(pf => sb.AppendLine(string.Concat("PREFIX ", pf.NamespacePrefix, ": <", pf.NamespaceUri.ToString(), ">")));
                    sb.AppendLine();
                }
                #endregion

                #region HEADER

                #region BEGINASK
                sb.Append("ASK");
                #endregion

                #endregion

                #region BODY
                sb.AppendLine();
                sb.AppendLine("WHERE {");

                #region MEMBERS
                bool printingUnion = false;
                List<RDFQueryMember> evaluableQueryMembers = askQuery.GetEvaluableQueryMembers().ToList();
                RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
                foreach (RDFQueryMember queryMember in evaluableQueryMembers)
                {

                    #region PATTERNGROUPS
                    if (queryMember is RDFPatternGroup)
                    {

                        //Current pattern group is set as UNION with the next one
                        if (((RDFPatternGroup)queryMember).JoinAsUnion)
                        {

                            //Current pattern group IS NOT the last of the query
                            //(so UNION keyword must be appended at last)
                            if (!queryMember.Equals(lastQueryMbr))
                            {
                                //Begin a new Union block
                                if (!printingUnion)
                                {
                                    printingUnion = true;
                                    sb.AppendLine("  {");
                                }
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                sb.AppendLine("    UNION");
                            }

                            //Current pattern group IS the last of the query
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                    sb.AppendLine("  }");
                                }
                                else
                                {
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, prefixes));
                                }
                            }

                        }

                        //Current pattern group is set as INTERSECT with the next one
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, prefixes));
                                sb.AppendLine("  }");
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, prefixes));
                            }
                        }

                    }
                    #endregion

                    #region SUBQUERY
                    else if (queryMember is RDFQuery)
                    {
                        //Merge main query prefixes
                        askQuery.GetPrefixes()
                                .ForEach(pf1 => ((RDFSelectQuery)queryMember).AddPrefix(pf1));

                        //Current subquery is set as UNION with the next one
                        if (((RDFSelectQuery)queryMember).JoinAsUnion)
                        {

                            //Current subquery IS NOT the last of the query
                            //(so UNION keyword must be appended at last)
                            if (!queryMember.Equals(lastQueryMbr))
                            {
                                //Begin a new Union block
                                if (!printingUnion)
                                {
                                    printingUnion = true;
                                    sb.AppendLine("  {");
                                }
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.AppendLine("    UNION");
                            }

                            //Current query IS the last of the query
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                    sb.AppendLine("  }");
                                }
                                else
                                {
                                    sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, false));
                                }
                            }

                        }

                        //Current query is set as INTERSECT with the next one
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.AppendLine("  }");
                            }
                            else
                            {
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, false));
                            }
                        }
                    }
                    #endregion

                }
                #endregion

                sb.Append("}");
                #endregion
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a pattern group
        /// </summary>
        internal static string PrintPatternGroup(RDFPatternGroup patternGroup, int spaceIndent, bool skipOptional, List<RDFNamespace> prefixes)
        {
            string spaces = new StringBuilder().Append(' ', spaceIndent < 0 ? 0 : spaceIndent).ToString();

            #region HEADER
            StringBuilder result = new StringBuilder();
            if (patternGroup.IsOptional && !skipOptional)
            {
                result.AppendLine(string.Concat("  ", spaces, "OPTIONAL {"));
                spaces = string.Concat(spaces, "  ");
            }
            //result.AppendLine(string.Concat("  ", spaces, "#", patternGroup.PatternGroupName));
            result.AppendLine(string.Concat(spaces, "  {"));
            #endregion

            #region MEMBERS
            bool printingUnion = false;
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            RDFPatternGroupMember lastPGMember = evaluablePGMembers.LastOrDefault();
            foreach (RDFPatternGroupMember pgMember in evaluablePGMembers)
            {
                #region PATTERNS
                if (pgMember is RDFPattern)
                {
                    //Union pattern
                    if (((RDFPattern)pgMember).JoinAsUnion)
                    {
                        if (!pgMember.Equals(lastPGMember))
                        {
                            //Begin a new Union block
                            printingUnion = true;
                            result.AppendLine(string.Concat(spaces, "    { ", PrintPattern((RDFPattern)pgMember, prefixes), " }"));
                            result.AppendLine(string.Concat(spaces, "    UNION"));
                        }
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                result.AppendLine(string.Concat(spaces, "    { ", PrintPattern((RDFPattern)pgMember, prefixes), " }"));
                            }
                            else
                            {
                                result.AppendLine(string.Concat(spaces, "    ", PrintPattern((RDFPattern)pgMember, prefixes), " ."));
                            }
                        }
                    }
                    //Intersect pattern
                    else
                    {
                        //End the Union block
                        if (printingUnion)
                        {
                            printingUnion = false;
                            result.AppendLine(string.Concat(spaces, "    { ", PrintPattern((RDFPattern)pgMember, prefixes), " }"));
                        }
                        else
                        {
                            result.AppendLine(string.Concat(spaces, "    ", PrintPattern((RDFPattern)pgMember, prefixes), " ."));
                        }
                    }
                }
                #endregion

                #region PROPERTY PATHS
                else if (pgMember is RDFPropertyPath && pgMember.IsEvaluable)
                {
                    //End the Union block
                    if (printingUnion)
                    {
                        printingUnion = false;
                        result.AppendLine(string.Concat(spaces, "    { ", PrintPropertyPath((RDFPropertyPath)pgMember, prefixes), " }"));
                    }
                    else
                    {
                        result.AppendLine(string.Concat(spaces, "    ", PrintPropertyPath((RDFPropertyPath)pgMember, prefixes), " ."));
                    }
                }
                #endregion

                #region VALUES
                else if (pgMember is RDFValues && pgMember.IsEvaluable && !((RDFValues)pgMember).IsInjected)
                {
                    //End the Union block
                    if (printingUnion)
                    {
                        printingUnion = false;
                        result.AppendLine(string.Concat(spaces, "    { ", PrintValues((RDFValues)pgMember, prefixes, spaces), " }"));
                    }
                    else
                    {
                        result.AppendLine(string.Concat(spaces, "    ", PrintValues((RDFValues)pgMember, prefixes, spaces), " ."));
                    }
                }
                #endregion
            }
            #endregion

            #region FILTERS
            patternGroup.GetFilters().Where(f => !(f is RDFValuesFilter))
                                     .ToList()
                                     .ForEach(f => result.AppendLine(string.Concat(spaces, "    ", f.ToString(prefixes), " ")));
            #endregion

            #region FOOTER
            result.AppendLine(string.Concat(spaces, "  }"));
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
            result.Append(" ");

            #region StepString

            #region Single Property
            if (propertyPath.Steps.Count == 1)
            {
                //InversePath (will swap start/end)
                if (propertyPath.Steps[0].IsInverseStep)
                    result.Append("^");

                var propPath = propertyPath.Steps[0].StepProperty;
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
                            result.Append("(");
                        }

                        //InversePath (will swap start/end)
                        if (propertyPath.Steps[i].IsInverseStep)
                            result.Append("^");

                        var propPath = propertyPath.Steps[i].StepProperty;
                        if (i < propertyPath.Steps.Count - 1)
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                            result.Append((char)propertyPath.Steps[i].StepFlavor);
                        }
                        else
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                            result.Append(")");
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
                            result.Append("^");

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
            }
            #endregion

            #endregion

            result.Append(" ");
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
                    result.Append(" ");
                }
                result.Append("}");
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
                        result.Append(" ");
                    });
                    result.AppendLine(")");
                }
                result.Append(string.Concat(spaces, "    }"));
            }

            return result.ToString();
        }

        /// <summary>
        /// Prints the string representation of a pattern member
        /// </summary>
        internal static string PrintPatternMember(RDFPatternMember patternMember, List<RDFNamespace> prefixes)
        {
            if (patternMember != null)
            {
                #region Variable
                if (patternMember is RDFVariable)
                    return patternMember.ToString();
                #endregion

                #region Resource/Context
                if (patternMember is RDFResource || patternMember is RDFContext)
                {
                    #region Blank
                    if (patternMember is RDFResource && ((RDFResource)patternMember).IsBlank)
                        return patternMember.ToString().Replace("bnode:", "_:");
                    #endregion

                    #region NonBlank
                    var abbreviatedPM = RDFQueryUtilities.AbbreviateRDFPatternMember(patternMember, prefixes);
                    if (abbreviatedPM.Item1)
                        return abbreviatedPM.Item2;
                    else
                        return string.Concat("<", abbreviatedPM.Item2, ">");
                    #endregion
                }
                #endregion

                #region Literal
                if (patternMember is RDFLiteral)
                {
                    #region PlainLiteral
                    if (patternMember is RDFPlainLiteral)
                    {
                        if (((RDFPlainLiteral)patternMember).Language != string.Empty)
                            return string.Concat("\"", ((RDFPlainLiteral)patternMember).Value, "\"@", ((RDFPlainLiteral)patternMember).Language);
                        return string.Concat("\"", ((RDFPlainLiteral)patternMember).Value, "\"");
                    }
                    #endregion

                    #region TypedLiteral
                    else
                    {
                        var abbreviatedPM = RDFQueryUtilities.AbbreviateRDFPatternMember(RDFQueryUtilities.ParseRDFPatternMember(RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)patternMember).Datatype)), prefixes);
                        if (abbreviatedPM.Item1)
                            return string.Concat("\"", ((RDFTypedLiteral)patternMember).Value, "\"^^", abbreviatedPM.Item2);
                        else
                            return string.Concat("\"", ((RDFTypedLiteral)patternMember).Value, "\"^^<", abbreviatedPM.Item2, ">");
                    }
                    #endregion
                }
                #endregion
            }
            return null;
        }
        #endregion
    }
}