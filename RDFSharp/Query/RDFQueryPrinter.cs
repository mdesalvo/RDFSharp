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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
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
        internal static String PrintSelectQuery(RDFSelectQuery selectQuery)
        {
            String subqueryIndent = selectQuery.IsSubQuery ? "  " : String.Empty;
            String subqueryBodyIndent = selectQuery.IsSubQuery ? "    " : String.Empty;
            StringBuilder sb = new StringBuilder();
            if (selectQuery != null)
            {

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
                    sb.Append(subqueryIndent + "{\n");
                sb.Append(subqueryBodyIndent + "SELECT");
                #endregion

                #region DISTINCT
                selectQuery.GetModifiers()
                           .Where(mod => mod is RDFDistinctModifier)
                           .ToList()
                           .ForEach(dm => sb.Append(" " + dm));
                #endregion

                #region VARIABLES
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
                sb.Append("\n");
                #endregion

                #endregion

                #region BODY
                sb.Append(subqueryBodyIndent + "WHERE\n");
                sb.Append(subqueryBodyIndent + "{\n");

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
                                    sb.Append("  {\n");
                                }
                                sb.Append(((RDFPatternGroup)queryMember).ToString(subqueryBodyIndent.Length + 2, selectQuery.Prefixes) + "    UNION\n");
                            }

                            //Current pattern group IS the last of the query 
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(((RDFPatternGroup)queryMember).ToString(subqueryBodyIndent.Length + 2, selectQuery.Prefixes));
                                    sb.Append("  }");
                                    sb.Append("\n");
                                }
                                else
                                {
                                    sb.Append(((RDFPatternGroup)queryMember).ToString(subqueryBodyIndent.Length, selectQuery.Prefixes));
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
                                sb.Append(((RDFPatternGroup)queryMember).ToString(subqueryBodyIndent.Length + 2, selectQuery.Prefixes));
                                sb.Append("  }");
                                sb.Append("\n");
                            }
                            else
                            {
                                sb.Append(((RDFPatternGroup)queryMember).ToString(subqueryBodyIndent.Length, selectQuery.Prefixes));
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
                        //End the Union block
                        if (printingUnion)
                        {
                            printingUnion = false;
                            sb.Append(((RDFSelectQuery)queryMember).ToString());
                            sb.Append(subqueryIndent + "  }");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.Append(((RDFSelectQuery)queryMember).ToString());
                        }
                    }
                    #endregion

                }
                #endregion

                sb.Append(subqueryBodyIndent + "}\n");
                #endregion

                #region FOOTER

                #region MODIFIERS
                List<RDFModifier> modifiers = selectQuery.GetModifiers().ToList();

                // ORDER BY
                if (modifiers.Any(mod => mod is RDFOrderByModifier))
                {
                    sb.Append("\n");
                    sb.Append(subqueryBodyIndent + "ORDER BY");
                    modifiers.Where(mod => mod is RDFOrderByModifier)
                             .ToList()
                             .ForEach(om => sb.Append(" " + om));
                }

                // LIMIT/OFFSET
                if (modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier))
                {
                    modifiers.Where(mod => mod is RDFLimitModifier)
                             .ToList()
                             .ForEach(lim => { sb.Append("\n"); sb.Append(subqueryBodyIndent + lim); });
                    modifiers.Where(mod => mod is RDFOffsetModifier)
                             .ToList()
                             .ForEach(off => { sb.Append("\n"); sb.Append(subqueryBodyIndent + off); });
                }
                #endregion

                #region ENDSELECT
                if (selectQuery.IsSubQuery)
                    sb.Append(subqueryIndent + "}\n");
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
                        sb.Append(" " + RDFQueryUtilities.PrintRDFPatternMember(dt, describeQuery.Prefixes));
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
                sb.Append("WHERE\n");
                sb.Append("{\n");

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
                                sb.Append(((RDFPatternGroup)queryMember).ToString(2, describeQuery.Prefixes) + "    UNION\n");
                            }

                            //Current pattern group IS the last of the query 
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(((RDFPatternGroup)queryMember).ToString(2, describeQuery.Prefixes));
                                    sb.Append("  }");
                                    sb.Append("\n");
                                }
                                else
                                {
                                    sb.Append(((RDFPatternGroup)queryMember).ToString(0, describeQuery.Prefixes));
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
                                sb.Append(((RDFPatternGroup)queryMember).ToString(2, describeQuery.Prefixes));
                                sb.Append("  }");
                                sb.Append("\n");
                            }
                            else
                            {
                                sb.Append(((RDFPatternGroup)queryMember).ToString(0, describeQuery.Prefixes));
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
                        //End the Union block
                        if (printingUnion)
                        {
                            printingUnion = false;
                            sb.Append(((RDFSelectQuery)queryMember).ToString());
                            sb.Append("  }");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.Append(((RDFSelectQuery)queryMember).ToString());
                        }
                    }
                    #endregion

                }
                #endregion

                sb.Append("}\n");
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
                    String tpString = tp.ToString(constructQuery.Prefixes);

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
                sb.Append("WHERE\n");
                sb.Append("{\n");

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
                                sb.Append(((RDFPatternGroup)queryMember).ToString(2, constructQuery.Prefixes) + "    UNION\n");
                            }

                            //Current pattern group IS the last of the query 
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(((RDFPatternGroup)queryMember).ToString(2, constructQuery.Prefixes));
                                    sb.Append("  }");
                                    sb.Append("\n");
                                }
                                else
                                {
                                    sb.Append(((RDFPatternGroup)queryMember).ToString(0, constructQuery.Prefixes));
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
                                sb.Append(((RDFPatternGroup)queryMember).ToString(2, constructQuery.Prefixes));
                                sb.Append("  }");
                                sb.Append("\n");
                            }
                            else
                            {
                                sb.Append(((RDFPatternGroup)queryMember).ToString(0, constructQuery.Prefixes));
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
                        //End the Union block
                        if (printingUnion)
                        {
                            printingUnion = false;
                            sb.Append(((RDFSelectQuery)queryMember).ToString());
                            sb.Append("  }");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.Append(((RDFSelectQuery)queryMember).ToString());
                        }
                    }
                    #endregion

                }
                #endregion

                sb.Append("}\n");
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
                sb.Append("\nWHERE\n");
                sb.Append("{\n");

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
                                sb.Append(((RDFPatternGroup)queryMember).ToString(2, askQuery.Prefixes) + "    UNION\n");
                            }

                            //Current pattern group IS the last of the query 
                            //(so UNION keyword must not be appended at last)
                            else
                            {
                                //End the Union block
                                if (printingUnion)
                                {
                                    printingUnion = false;
                                    sb.Append(((RDFPatternGroup)queryMember).ToString(2, askQuery.Prefixes));
                                    sb.Append("  }");
                                    sb.Append("\n");
                                }
                                else
                                {
                                    sb.Append(((RDFPatternGroup)queryMember).ToString(0, askQuery.Prefixes));
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
                                sb.Append(((RDFPatternGroup)queryMember).ToString(2, askQuery.Prefixes));
                                sb.Append("  }");
                                sb.Append("\n");
                            }
                            else
                            {
                                sb.Append(((RDFPatternGroup)queryMember).ToString(0, askQuery.Prefixes));
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
                        //End the Union block
                        if (printingUnion)
                        {
                            printingUnion = false;
                            sb.Append(((RDFSelectQuery)queryMember).ToString());
                            sb.Append("  }");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.Append(((RDFSelectQuery)queryMember).ToString());
                        }
                    }
                    #endregion

                }
                #endregion

                sb.Append("}\n");
                #endregion

            }
            return sb.ToString();
        }
        #endregion

    }
}