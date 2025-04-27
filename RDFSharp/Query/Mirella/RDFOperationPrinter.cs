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
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOperationPrinter is responsible for getting string representation of SPARQL UPDATE operations
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
                List<RDFNamespace> prefixes = insertDataOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));

                sb.AppendLine("INSERT DATA {");
                insertDataOperation.InsertTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.Append('}');
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
                List<RDFNamespace> prefixes = insertWhereOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));

                sb.AppendLine("INSERT {");
                insertWhereOperation.InsertTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.AppendLine("}");

                RDFQueryPrinter.PrintWhereClause(insertWhereOperation, sb, prefixes, string.Empty, 0, false);
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
                List<RDFNamespace> prefixes = deleteDataOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));

                sb.AppendLine("DELETE DATA {");
                deleteDataOperation.DeleteTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.Append('}');
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
                List<RDFNamespace> prefixes = deleteWhereOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));

                sb.AppendLine("DELETE {");
                deleteWhereOperation.DeleteTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.AppendLine("}");

                RDFQueryPrinter.PrintWhereClause(deleteWhereOperation, sb, prefixes, string.Empty, 0, false);
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
                List<RDFNamespace> prefixes = deleteInsertWhereOperation.GetPrefixes();
                sb.Append(PrintPrefixes(prefixes));

                sb.AppendLine("DELETE {");
                deleteInsertWhereOperation.DeleteTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.AppendLine("}");
                sb.AppendLine("INSERT {");
                deleteInsertWhereOperation.InsertTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.AppendLine("}");

                RDFQueryPrinter.PrintWhereClause(deleteInsertWhereOperation, sb, prefixes, string.Empty, 0, false);
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

            return sb.ToString();
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Prints the string representation of a SPARQL operation's prefixes
        /// </summary>
        private static string PrintPrefixes(List<RDFNamespace> operationPrefixes)
        {
            StringBuilder sb = new StringBuilder();
            if (operationPrefixes.Count > 0)
            {
                operationPrefixes.ForEach(pf => sb.AppendLine(string.Concat("PREFIX ", pf.NamespacePrefix, ": <", pf.NamespaceUri.ToString(), ">")));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL operation's patterns
        /// </summary>
        private static string PrintPattern(List<RDFNamespace> operationPrefixes, RDFPattern tp)
        {
            string tpString = RDFQueryPrinter.PrintPattern(tp, operationPrefixes);

            //Remove the Optional indicator from the template print (since it is not supported in SPARQL UPDATE operations)
            if (tp.IsOptional)
                tpString = tpString.Replace("OPTIONAL { ", string.Empty).TrimEnd(' ', '}');

            return string.Concat("  ", tpString, " .", Environment.NewLine);
        }
        #endregion
    }
}