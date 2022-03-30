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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOperationPrinter is responsible for getting string representation of SPARQL UPDATE operation entities
    /// </summary>
    internal static class RDFOperationPrinter
    {
        #region Methods
        /// <summary>
        /// Prints the string representation of a SPARQL INSERT DATA operation
        /// </summary>
        internal static string PrintInsertDataOperation(RDFInsertDataOperation insertDataOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (insertDataOperation != null)
            {
                #region PREFIXES
                List<RDFNamespace> prefixes = insertDataOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));
                #endregion

                #region TEMPLATES
                sb.AppendLine("INSERT DATA {");
                insertDataOperation.InsertTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.Append("}");
                #endregion
            }

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL INSERT WHERE operation
        /// </summary>
        internal static string PrintInsertWhereOperation(RDFInsertWhereOperation insertWhereOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (insertWhereOperation != null)
            {
                #region PREFIXES
                List<RDFNamespace> prefixes = insertWhereOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));
                #endregion

                #region TEMPLATES
                sb.AppendLine("INSERT {");
                insertWhereOperation.InsertTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.AppendLine("}");
                #endregion

                #region BODY
                sb.AppendLine("WHERE {");
                sb.Append(PrintBodyMembers(prefixes, insertWhereOperation));
                sb.Append("}");
                #endregion
            }

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL DELETE DATA operation
        /// </summary>
        internal static string PrintDeleteDataOperation(RDFDeleteDataOperation deleteDataOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (deleteDataOperation != null)
            {
                #region PREFIXES
                List<RDFNamespace> prefixes = deleteDataOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));
                #endregion

                #region TEMPLATES
                sb.AppendLine("DELETE DATA {");
                deleteDataOperation.DeleteTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.Append("}");
                #endregion
            }

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL DELETE WHERE operation
        /// </summary>
        internal static string PrintDeleteWhereOperation(RDFDeleteWhereOperation deleteWhereOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (deleteWhereOperation != null)
            {
                #region PREFIXES
                List<RDFNamespace> prefixes = deleteWhereOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));
                #endregion

                #region TEMPLATES
                sb.AppendLine("DELETE {");
                deleteWhereOperation.DeleteTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.AppendLine("}");
                #endregion

                #region BODY
                sb.AppendLine("WHERE {");
                sb.Append(PrintBodyMembers(prefixes, deleteWhereOperation));
                sb.Append("}");
                #endregion
            }

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL DELETE/INSERT WHERE operation
        /// </summary>
        internal static string PrintDeleteInsertWhereOperation(RDFDeleteInsertWhereOperation deleteInsertWhereOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (deleteInsertWhereOperation != null)
            {
                #region PREFIXES
                List<RDFNamespace> prefixes = deleteInsertWhereOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));
                #endregion

                #region TEMPLATES
                sb.AppendLine("DELETE {");
                deleteInsertWhereOperation.DeleteTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.AppendLine("}");
                sb.AppendLine("INSERT {");
                deleteInsertWhereOperation.InsertTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.AppendLine("}");
                #endregion

                #region BODY
                sb.AppendLine("WHERE {");
                sb.Append(PrintBodyMembers(prefixes, deleteInsertWhereOperation));
                sb.Append("}");
                #endregion
            }

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL LOAD operation
        /// </summary>
        internal static string PrintLoadOperation(RDFLoadOperation loadOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (loadOperation != null)
            {
                sb.Append("LOAD ");

                if (loadOperation.IsSilent)
                    sb.Append("SILENT ");

                sb.Append($"<{loadOperation.FromContext}>");

                if (loadOperation.ToContext != null)
                    sb.Append($" INTO GRAPH <{loadOperation.ToContext}>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL CLEAR operation
        /// </summary>
        internal static string PrintClearOperation(RDFClearOperation clearOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (clearOperation != null)
            {
                sb.Append("CLEAR ");

                if (clearOperation.IsSilent)
                    sb.Append("SILENT ");

                if (clearOperation.FromContext != null)
                    sb.Append($"GRAPH <{clearOperation.FromContext}>");
                else
                {
                    switch (clearOperation.OperationFlavor)
                    {
                        case RDFQueryEnums.RDFClearOperationFlavor.DEFAULT:
                            sb.Append("DEFAULT");
                            break;

                        case RDFQueryEnums.RDFClearOperationFlavor.NAMED:
                            sb.Append("NAMED");
                            break;

                        case RDFQueryEnums.RDFClearOperationFlavor.ALL:
                            sb.Append("ALL");
                            break;
                    }
                }                    
            }

            return sb.ToString();
        }
        #endregion

        #region Utilities
        private static string PrintPrefixes(List<RDFNamespace> operationPrefixes)
        {
            StringBuilder sb = new StringBuilder();
            if (operationPrefixes.Any())
            {
                operationPrefixes.ForEach(pf => sb.AppendLine(string.Concat("PREFIX ", pf.NamespacePrefix, ": <", pf.NamespaceUri.ToString(), ">")));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string PrintPattern(List<RDFNamespace> operationPrefixes, RDFPattern tp)
        {
            string tpString = RDFQueryPrinter.PrintPattern(tp, operationPrefixes);

            //Remove the Optional indicator from the template print (since it is not supported in SPARQL UPDATE operations)
            if (tp.IsOptional)
                tpString = tpString.Replace("OPTIONAL { ", string.Empty).TrimEnd(new char[] { ' ', '}' });

            return string.Concat("  ", tpString, " .", Environment.NewLine);
        }

        private static string PrintBodyMembers(List<RDFNamespace> operationPrefixes, RDFOperation operation)
        {
            StringBuilder sb = new StringBuilder();

            #region MEMBERS
            bool printingUnion = false;
            List<RDFQueryMember> evaluableQueryMembers = operation.GetEvaluableQueryMembers().ToList();
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
                            sb.Append(RDFQueryPrinter.PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, operationPrefixes));
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
                                sb.Append(RDFQueryPrinter.PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, operationPrefixes));
                                sb.AppendLine("  }");
                            }
                            else
                            {
                                sb.Append(RDFQueryPrinter.PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, operationPrefixes));
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
                            sb.Append(RDFQueryPrinter.PrintPatternGroup((RDFPatternGroup)queryMember, 2, true, operationPrefixes));
                            sb.AppendLine("  }");
                        }
                        else
                        {
                            sb.Append(RDFQueryPrinter.PrintPatternGroup((RDFPatternGroup)queryMember, 0, false, operationPrefixes));
                        }
                    }

                }
                #endregion

                #region SUBQUERY
                else if (queryMember is RDFQuery)
                {
                    //Merge main query prefixes
                    operation.GetPrefixes().ForEach(pf1 => ((RDFSelectQuery)queryMember).AddPrefix(pf1));

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
                            sb.Append(RDFQueryPrinter.PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
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
                                sb.Append(RDFQueryPrinter.PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                                sb.AppendLine("  }");
                            }
                            else
                            {
                                sb.Append(RDFQueryPrinter.PrintSelectQuery((RDFSelectQuery)queryMember, 1, false));
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
                            sb.Append(RDFQueryPrinter.PrintSelectQuery((RDFSelectQuery)queryMember, 1, true));
                            sb.AppendLine("  }");
                        }
                        else
                        {
                            sb.Append(RDFQueryPrinter.PrintSelectQuery((RDFSelectQuery)queryMember, 1, false));
                        }
                    }
                }
                #endregion
            }
            #endregion

            return sb.ToString();
        }
        #endregion
    }
}