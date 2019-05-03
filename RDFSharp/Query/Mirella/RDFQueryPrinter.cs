/*
   Copyright 2012-2019 Marco De Salvo

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
using System.Data;
using System.Linq;
using System.Text;
using RDFSharp.Model;
using RDFSharp.Store;

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
        internal static String PrintSelectQuery(RDFSelectQuery selectQuery, Double indentLevel, Boolean fromUnion)
        {
            StringBuilder sb = new StringBuilder();
            if (selectQuery != null)
            {

                #region INDENT
                Int32 subqueryHeaderSpacesFunc(Double indLevel) { return subqueryBodySpacesFunc(indentLevel) - 2 < 0 ? 0 : subqueryBodySpacesFunc(indentLevel) - 2; }
                Int32 subqueryBodySpacesFunc(Double indLevel) { return Convert.ToInt32(4 * indentLevel); }
                Int32 subqueryUnionSpacesFunc(Boolean union) { return union ? 2 : 0; }

                String subquerySpaces = new String(' ', subqueryHeaderSpacesFunc(indentLevel) + subqueryUnionSpacesFunc(fromUnion));
                String subqueryBodySpaces = new String(' ', subqueryBodySpacesFunc(indentLevel) + subqueryUnionSpacesFunc(fromUnion));
                #endregion

                #region PREFIX
                if (!selectQuery.IsSubQuery)
                {
                    if (selectQuery.Prefixes.Any())
                    {
                        selectQuery.Prefixes.ForEach(pf =>
                        {
                            sb.Append("PREFIX " + pf.NamespacePrefix + ": <" + pf.NamespaceUri + ">\n");
                        });
                        sb.Append("\n");
                    }
                }
                #endregion

                #region HEADER

                #region BEGINSELECT
                if (selectQuery.IsSubQuery)
                {
                    if (selectQuery.IsOptional && !fromUnion)
                    {
                        sb.Append(subquerySpaces + "OPTIONAL {\n");
                    }
                    else
                    {
                        sb.Append(subquerySpaces + "{\n");
                    }                    
                }
                sb.Append(subqueryBodySpaces + "SELECT");
                #endregion

                #region DISTINCT
                selectQuery.GetModifiers()
                           .Where(mod => mod is RDFDistinctModifier)
                           .ToList()
                           .ForEach(dm => sb.Append(" " + dm));
                #endregion

                #region VARIABLES/AGGREGATORS
                List<RDFModifier> modifiers = selectQuery.GetModifiers().ToList();
                //Query has groupby modifier
                if (modifiers.Any(m => m is RDFGroupByModifier))
                {
                    modifiers.Where(mod => mod is RDFGroupByModifier)
                         .ToList()
                         .ForEach(gm => {
                             sb.Append(" ");
                             sb.Append(String.Join(" ", ((RDFGroupByModifier)gm).PartitionVariables));
                             sb.Append(" ");
                             sb.Append(String.Join(" ", ((RDFGroupByModifier)gm).Aggregators));
                         });
                }
                //Query hasn't groupby modifier
                else
                {
                    if (selectQuery.ProjectionVars.Any())
                    {
                        selectQuery.ProjectionVars.OrderBy(x => x.Value)
                                                  .ToList()
                                                  .ForEach(v => sb.Append(" " + v.Key));
                    }
                    else
                    {
                        sb.Append(" *");
                    }
                }                
                sb.Append("\n");
                #endregion

                #endregion

                #region BODY
                sb.Append(subqueryBodySpaces + "WHERE {\n");

                #region MEMBERS
                Boolean printingUnion = false;
                List<RDFQueryMember> evaluableQueryMembers = selectQuery.GetEvaluableQueryMembers().ToList();
                RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
                foreach (var queryMember in evaluableQueryMembers)
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
                                    sb.Append(subqueryBodySpaces + "  {\n");
                                }
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length + 2, true, selectQuery.Prefixes));
                                sb.Append(subqueryBodySpaces + "    UNION\n");
                            }

                            //Current pattern group IS the last of the query 
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length + 2, true, selectQuery.Prefixes));
                                    sb.Append(subqueryBodySpaces + "  }\n");
                                }
                                else
                                {
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length, false, selectQuery.Prefixes));
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
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length + 2, true, selectQuery.Prefixes));
                                sb.Append(subqueryBodySpaces + "  }\n");
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, subqueryBodySpaces.Length, false, selectQuery.Prefixes));
                            }
                        }

                    }
                    #endregion

                    #region SUBQUERY
                    else if (queryMember is RDFQuery)
                    {
                        //Merge main query prefixes
                        selectQuery.Prefixes.ForEach(pf1 => {
                            if (!((RDFSelectQuery)queryMember).Prefixes.Any(pf2 => pf2.Equals(pf1)))
                            {
                                ((RDFSelectQuery)queryMember).AddPrefix(pf1);
                            }
                        });
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
                                    sb.Append(subqueryBodySpaces + "  {\n");
                                }
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, indentLevel + 1 + (fromUnion ? 0.5 : 0), true));
                                sb.Append(subqueryBodySpaces + "    UNION\n");
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
                                    sb.Append(subqueryBodySpaces + "  }\n");
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
                                sb.Append(subqueryBodySpaces + "  }\n");
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

                sb.Append(subqueryBodySpaces + "}");
                #endregion

                #region FOOTER

                #region MODIFIERS
                //GROUP BY
                if (modifiers.Any(mod => mod is RDFGroupByModifier))
                {
                    modifiers.Where(mod => mod is RDFGroupByModifier)
                             .ToList()
                             .ForEach(gm => { sb.Append("\n"); sb.Append(subqueryBodySpaces + gm); });
                }

                // ORDER BY
                if (modifiers.Any(mod => mod is RDFOrderByModifier))
                {
                    sb.Append("\n");
                    sb.Append(subqueryBodySpaces + "ORDER BY");
                    modifiers.Where(mod => mod is RDFOrderByModifier)
                             .ToList()
                             .ForEach(om => sb.Append(" " + om));
                }

                // LIMIT/OFFSET
                if (modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier))
                {
                    modifiers.Where(mod => mod is RDFLimitModifier)
                             .ToList()
                             .ForEach(lim => { sb.Append("\n"); sb.Append(subqueryBodySpaces + lim); });
                    modifiers.Where(mod => mod is RDFOffsetModifier)
                             .ToList()
                             .ForEach(off => { sb.Append("\n"); sb.Append(subqueryBodySpaces + off); });
                }
                #endregion

                #region ENDSELECT
                sb.Append("\n");
                if (selectQuery.IsSubQuery)
                    sb.Append(subquerySpaces + "}\n");
                #endregion

                #endregion

            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL DESCRIBE query
        /// </summary>
        internal static String PrintDescribeQuery(RDFDescribeQuery describeQuery)
        {
            StringBuilder sb = new StringBuilder();
            if (describeQuery != null)
            {

                #region PREFIXES
                if (describeQuery.Prefixes.Any())
                {
                    describeQuery.Prefixes.ForEach(pf =>
                    {
                        sb.Append("PREFIX " + pf.NamespacePrefix + ": <" + pf.NamespaceUri + ">\n");
                    });
                    sb.Append("\n");
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
                        sb.Append(" " + PrintPatternMember(dt, describeQuery.Prefixes));
                    });
                }
                else
                {
                    sb.Append(" *");
                }
                sb.Append("\n");
                #endregion

                #endregion

                #region BODY
                sb.Append("WHERE {\n");

                #region MEMBERS
                Boolean printingUnion = false;
                List<RDFQueryMember> evaluableQueryMembers = describeQuery.GetEvaluableQueryMembers().ToList();
                RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
                foreach (var queryMember in evaluableQueryMembers)
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
                                    sb.Append("  {\n");
                                }
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, describeQuery.Prefixes));
                                sb.Append("    UNION\n");
                            }

                            //Current pattern group IS the last of the query 
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, describeQuery.Prefixes));
                                    sb.Append("  }\n");
                                }
                                else
                                {
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, describeQuery.Prefixes));
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
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, describeQuery.Prefixes));
                                sb.Append("  }\n");
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, describeQuery.Prefixes));
                            }
                        }

                    }
                    #endregion

                    #region SUBQUERY
                    else if (queryMember is RDFQuery)
                    {
                        //Merge main query prefixes
                        describeQuery.Prefixes.ForEach(pf1 => {
                            if (!((RDFSelectQuery)queryMember).Prefixes.Any(pf2 => pf2.Equals(pf1)))
                            {
                                ((RDFSelectQuery)queryMember).AddPrefix(pf1);
                            }
                        });
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
                                    sb.Append("  {\n");
                                }
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.Append("    UNION\n");
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
                                    sb.Append("  }\n");
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
                                sb.Append("  }\n");
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
                             .ForEach(lim => { sb.Append("\n"); sb.Append(lim); });
                    modifiers.Where(mod => mod is RDFOffsetModifier)
                             .ToList()
                             .ForEach(off => { sb.Append("\n"); sb.Append(off); });
                }
                #endregion

                #endregion

            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL CONSTRUCT query
        /// </summary>
        internal static String PrintConstructQuery(RDFConstructQuery constructQuery)
        {
            StringBuilder sb = new StringBuilder();
            if (constructQuery != null)
            {

                #region PREFIXES
                if (constructQuery.Prefixes.Any())
                {
                    constructQuery.Prefixes.ForEach(pf =>
                    {
                        sb.Append("PREFIX " + pf.NamespacePrefix + ": <" + pf.NamespaceUri + ">\n");
                    });
                    sb.Append("\n");
                }
                #endregion

                #region HEADER

                #region BEGINCONSTRUCT
                sb.Append("CONSTRUCT");
                #endregion

                #region TEMPLATES
                sb.Append("\n{\n");
                constructQuery.Templates.ForEach(tp =>
                {
                    String tpString = PrintPattern(tp, constructQuery.Prefixes);

                    //Remove the Context from the template print (since it is not supported by CONSTRUCT query)
                    if (tp.Context != null)
                    {
                        tpString = tpString.Replace("GRAPH " + tp.Context + " { ", String.Empty).TrimEnd(new Char[] { ' ', '}' });
                    }

                    //Remove the Optional indicator from the template print (since it is not supported by CONSTRUCT query)
                    if (tp.IsOptional)
                    {
                        tpString = tpString.Replace("OPTIONAL { ", String.Empty).TrimEnd(new Char[] { ' ', '}' });
                    }

                    sb.Append("  " + tpString + " .\n");
                });
                sb.Append("}\n");
                #endregion

                #endregion

                #region BODY
                sb.Append("WHERE {\n");

                #region MEMBERS
                Boolean printingUnion = false;
                List<RDFQueryMember> evaluableQueryMembers = constructQuery.GetEvaluableQueryMembers().ToList();
                RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
                foreach (var queryMember in evaluableQueryMembers)
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
                                    sb.Append("  {\n");
                                }
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, constructQuery.Prefixes));
                                sb.Append("    UNION\n");
                            }

                            //Current pattern group IS the last of the query 
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, constructQuery.Prefixes));
                                    sb.Append("  }\n");
                                }
                                else
                                {
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, constructQuery.Prefixes));
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
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, constructQuery.Prefixes));
                                sb.Append("  }\n");
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, constructQuery.Prefixes));
                            }
                        }

                    }
                    #endregion

                    #region SUBQUERY
                    else if (queryMember is RDFQuery)
                    {
                        //Merge main query prefixes
                        constructQuery.Prefixes.ForEach(pf1 => {
                            if (!((RDFSelectQuery)queryMember).Prefixes.Any(pf2 => pf2.Equals(pf1)))
                            {
                                ((RDFSelectQuery)queryMember).AddPrefix(pf1);
                            }
                        });
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
                                    sb.Append("  {\n");
                                }
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.Append("    UNION\n");
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
                                    sb.Append("  }\n");
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
                                sb.Append("  }\n");
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
                             .ForEach(lim => { sb.Append("\n"); sb.Append(lim); });
                    modifiers.Where(mod => mod is RDFOffsetModifier)
                             .ToList()
                             .ForEach(off => { sb.Append("\n"); sb.Append(off); });
                }
                #endregion

                #endregion

            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL ASK query
        /// </summary>
        internal static String PrintAskQuery(RDFAskQuery askQuery)
        {
            StringBuilder sb = new StringBuilder();
            if (askQuery != null)
            {

                #region PREFIXES
                if (askQuery.Prefixes.Any())
                {
                    askQuery.Prefixes.ForEach(pf =>
                    {
                        sb.Append("PREFIX " + pf.NamespacePrefix + ": <" + pf.NamespaceUri + ">\n");
                    });
                    sb.Append("\n");
                }
                #endregion

                #region HEADER

                #region BEGINASK
                sb.Append("ASK");
                #endregion

                #endregion

                #region BODY
                sb.Append("\nWHERE {\n");

                #region MEMBERS
                Boolean printingUnion = false;
                List<RDFQueryMember> evaluableQueryMembers = askQuery.GetEvaluableQueryMembers().ToList();
                RDFQueryMember lastQueryMbr = evaluableQueryMembers.LastOrDefault();
                foreach (var queryMember in evaluableQueryMembers)
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
                                    sb.Append("  {\n");
                                }
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, askQuery.Prefixes));
                                sb.Append("    UNION\n");
                            }

                            //Current pattern group IS the last of the query 
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, askQuery.Prefixes));
                                    sb.Append("  }\n");
                                }
                                else
                                {
                                    sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, askQuery.Prefixes));
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
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, askQuery.Prefixes));
                                sb.Append("  }\n");
                            }
                            else
                            {
                                sb.Append(PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, askQuery.Prefixes));
                            }
                        }

                    }
                    #endregion

                    #region SUBQUERY
                    else if (queryMember is RDFQuery)
                    {
                        //Merge main query prefixes
                        askQuery.Prefixes.ForEach(pf1 => {
                            if (!((RDFSelectQuery)queryMember).Prefixes.Any(pf2 => pf2.Equals(pf1)))
                            {
                                ((RDFSelectQuery)queryMember).AddPrefix(pf1);
                            }
                        });
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
                                    sb.Append("  {\n");
                                }
                                sb.Append(PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.Append("    UNION\n");
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
                                    sb.Append("  }\n");
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
                                sb.Append("  }\n");
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
        internal static String PrintPatternGroup(RDFPatternGroup patternGroup, Int32 spaceIndent, Boolean skipOptional, List<RDFNamespace> prefixes)
        {
            String spaces = new StringBuilder().Append(' ', spaceIndent < 0 ? 0 : spaceIndent).ToString();

            #region HEADER
            StringBuilder result = new StringBuilder();
            if (patternGroup.IsOptional && !skipOptional)
            {
                result.Append("  " + spaces + "OPTIONAL {\n");
                spaces = spaces + "  ";
            }
            //result.Append("  " + spaces + "#" + patternGroup.PatternGroupName + "\n");
            result.Append(spaces + "  {\n");
            #endregion

            #region MEMBERS
            Boolean printingUnion = false;
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            RDFPatternGroupMember lastPGMember = evaluablePGMembers.LastOrDefault();
            foreach(var pgMember in evaluablePGMembers)
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
                            result.Append(spaces + "    { " + PrintPattern((RDFPattern)pgMember, prefixes) + " }\n" + spaces + "    UNION\n");
                        }
                        else
                        {
                            //End the Union block
                            if (printingUnion)
                            {
                                printingUnion = false;
                                result.Append(spaces + "    { " + PrintPattern((RDFPattern)pgMember, prefixes) + " }\n");
                            }
                            else
                            {
                                result.Append(spaces + "    " + PrintPattern((RDFPattern)pgMember, prefixes) + " .\n");
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
                            result.Append(spaces + "    { " + PrintPattern((RDFPattern)pgMember, prefixes) + " }\n");
                        }
                        else
                        {
                            result.Append(spaces + "    " + PrintPattern((RDFPattern)pgMember, prefixes) + " .\n");
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
                        result.Append(spaces + "    { " + PrintPropertyPath((RDFPropertyPath)pgMember, prefixes) + " }\n");
                    }
                    else
                    {
                        result.Append(spaces + "    " + PrintPropertyPath((RDFPropertyPath)pgMember, prefixes) + " .\n");
                    }
                }
                #endregion

            }
            #endregion

            #region FILTERS
            patternGroup.GroupMembers.Where(m => m is RDFFilter)
                                     .ToList()
                                     .ForEach(f => result.Append(spaces + "    " + ((RDFFilter)f).ToString(prefixes) + " \n"));
            #endregion

            #region FOOTER
            result.Append(spaces + "  }\n");
            if (patternGroup.IsOptional && !skipOptional)
            {
                result.Append(spaces + "}\n");
            }
            #endregion

            return result.ToString();
        }

        /// <summary>
        /// Prints the string representation of a pattern
        /// </summary>
        internal static String PrintPattern(RDFPattern pattern, List<RDFNamespace> prefixes)
        {
            String subj = PrintPatternMember(pattern.Subject, prefixes);
            String pred = PrintPatternMember(pattern.Predicate, prefixes);
            String obj = PrintPatternMember(pattern.Object, prefixes);

            //CSPO pattern
            if (pattern.Context != null)
            {
                String ctx = PrintPatternMember(pattern.Context, prefixes);
                if (pattern.IsOptional)
                {
                    return "OPTIONAL { GRAPH " + ctx + " { " + subj + " " + pred + " " + obj + " } }";
                }
                return "GRAPH " + ctx + " { " + subj + " " + pred + " " + obj + " }";
            }

            //SPO pattern
            if (pattern.IsOptional)
            {
                return "OPTIONAL { " + subj + " " + pred + " " + obj + " }";
            }
            return subj + " " + pred + " " + obj;
        }

        /// <summary>
        /// Prints the string representation of a property path
        /// </summary>
        internal static String PrintPropertyPath(RDFPropertyPath propertyPath, List<RDFNamespace> prefixes)
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
                {
                    result.Append("^");
                }

                var propPath = propertyPath.Steps[0].StepProperty;
                result.Append(PrintPatternMember(propPath, prefixes));

            }
            #endregion

            #region Multiple Properties
            else
            {

                //Initialize printing
                Boolean openedParenthesis = false;

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
                        {
                            result.Append("^");
                        }

                        var propPath = propertyPath.Steps[i].StepProperty;
                        if (i < propertyPath.Steps.Count - 1)
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                            result.Append((Char)propertyPath.Steps[i].StepFlavor);
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
                        {
                            result.Append("^");
                        }

                        var propPath = propertyPath.Steps[i].StepProperty;
                        if (i < propertyPath.Steps.Count - 1)
                        {
                            result.Append(PrintPatternMember(propPath, prefixes));
                            result.Append((Char)propertyPath.Steps[i].StepFlavor);
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
        /// Prints the string representation of a pattern member
        /// </summary>
        internal static String PrintPatternMember(RDFPatternMember patternMember, List<RDFNamespace> prefixes)
        {

            if (patternMember != null)
            {

                #region Variable
                if (patternMember is RDFVariable)
                {
                    return patternMember.ToString();
                }
                #endregion

                #region Resource/Context
                if (patternMember is RDFResource || patternMember is RDFContext)
                {
                    #region Blank
                    if (patternMember is RDFResource && ((RDFResource)patternMember).IsBlank)
                    {
                        return patternMember.ToString().Replace("bnode:", "_:");
                    }
                    #endregion

                    #region NonBlank
                    var abbreviatedPM = RDFQueryUtilities.AbbreviateRDFPatternMember(patternMember, prefixes);
                    if (abbreviatedPM.Item1)
                    {
                        return abbreviatedPM.Item2;
                    }
                    else
                    {
                        return "<" + abbreviatedPM.Item2 + ">";
                    }
                    #endregion
                }
                #endregion

                #region Literal
                if (patternMember is RDFLiteral)
                {
                    #region PlainLiteral
                    if (patternMember is RDFPlainLiteral)
                    {
                        if (((RDFPlainLiteral)patternMember).Language != String.Empty)
                        {
                            return "\"" + ((RDFPlainLiteral)patternMember).Value + "\"@" + ((RDFPlainLiteral)patternMember).Language;
                        }
                        return "\"" + ((RDFPlainLiteral)patternMember).Value + "\"";
                    }
                    #endregion

                    #region TypedLiteral
                    else
                    {
                        var abbreviatedPM = RDFQueryUtilities.AbbreviateRDFPatternMember(RDFQueryUtilities.ParseRDFPatternMember(RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)patternMember).Datatype)), prefixes);
                        if (abbreviatedPM.Item1)
                        {
                            return "\"" + ((RDFTypedLiteral)patternMember).Value + "\"^^" + abbreviatedPM.Item2;
                        }
                        else
                        {
                            return "\"" + ((RDFTypedLiteral)patternMember).Value + "\"^^<" + abbreviatedPM.Item2 + ">";
                        }
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