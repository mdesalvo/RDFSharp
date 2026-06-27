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
                sb.Append('}').AppendLine();

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
                sb.Append('}').AppendLine();

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
                sb.Append('}').AppendLine();
                sb.AppendLine("INSERT {");
                deleteInsertWhereOperation.InsertTemplates.ForEach(tp => sb.Append(PrintPattern(prefixes, tp)));
                sb.Append('}').AppendLine();

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
                {
                    sb.Append($"GRAPH <{clearOperation.FromContext}>");
                }
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
        /// <summary>
        /// Prints the string representation of a SPARQL CREATE operation
        /// </summary>
        internal static string PrintCreateOperation(RDFCreateOperation createOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (createOperation != null)
            {
                sb.Append("CREATE ");

                if (createOperation.IsSilent)
                    sb.Append("SILENT ");

                sb.Append($"GRAPH <{createOperation.FromContext}>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Prints the string representation of a SPARQL DROP operation (twin of CLEAR: same GraphRefAll grammar)
        /// </summary>
        internal static string PrintDropOperation(RDFDropOperation dropOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (dropOperation != null)
            {
                sb.Append("DROP ");

                if (dropOperation.IsSilent)
                    sb.Append("SILENT ");

                if (dropOperation.FromContext != null)
                {
                    sb.Append($"GRAPH <{dropOperation.FromContext}>");
                }
                else
                {
                    switch (dropOperation.OperationFlavor)
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

        /// <summary>
        /// Prints the string representation of a SPARQL ADD operation
        /// </summary>
        internal static string PrintAddOperation(RDFAddOperation addOperation)
            => PrintCopyMoveAddOperation("ADD", addOperation?.IsSilent ?? false, addOperation?.FromContext, addOperation?.ToContext, addOperation != null);

        /// <summary>
        /// Prints the string representation of a SPARQL COPY operation
        /// </summary>
        internal static string PrintCopyOperation(RDFCopyOperation copyOperation)
            => PrintCopyMoveAddOperation("COPY", copyOperation?.IsSilent ?? false, copyOperation?.FromContext, copyOperation?.ToContext, copyOperation != null);

        /// <summary>
        /// Prints the string representation of a SPARQL MOVE operation
        /// </summary>
        internal static string PrintMoveOperation(RDFMoveOperation moveOperation)
            => PrintCopyMoveAddOperation("MOVE", moveOperation?.IsSilent ?? false, moveOperation?.FromContext, moveOperation?.ToContext, moveOperation != null);

        /// <summary>
        /// Prints the shared string representation of the source→destination graph-management operations
        /// (ADD/COPY/MOVE): <c>KEYWORD 'SILENT'? GraphOrDefault TO GraphOrDefault</c>. A null context is the
        /// DEFAULT graph; a non-null context is rendered as a bare IRIREF (the 'GRAPH' keyword of GraphOrDefault
        /// is optional, so omitting it keeps the print compact while staying re-parsable).
        /// </summary>
        private static string PrintCopyMoveAddOperation(string keyword, bool isSilent, Uri fromContext, Uri toContext, bool hasOperation)
        {
            StringBuilder sb = new StringBuilder();

            if (hasOperation)
            {
                sb.Append(keyword).Append(' ');

                if (isSilent)
                    sb.Append("SILENT ");

                sb.Append(PrintGraphOrDefault(fromContext))
                  .Append(" TO ")
                  .Append(PrintGraphOrDefault(toContext));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Prints a single GraphOrDefault endpoint: <c>DEFAULT</c> for a null context, otherwise a bare IRIREF.
        /// </summary>
        private static string PrintGraphOrDefault(Uri graphContext)
            => graphContext != null ? $"<{graphContext}>" : "DEFAULT";

        /// <summary>
        /// Prints the string representation of a SPARQL UPDATE operation set, i.e. its ordered operations rendered
        /// each by its own printer and joined by the ';' separator of the SPARQL UPDATE grammar, so the chain
        /// re-parses (via <see cref="RDFOperationSet.FromString"/>) into an equivalent set.
        /// </summary>
        internal static string PrintOperationSet(RDFOperationSet operationSet)
        {
            StringBuilder sb = new StringBuilder();

            if (operationSet != null)
            {
                //Render each operation through its own ToString() and join them with the ';' separator on its own
                //line, so the chain stays readable and round-trips stably through the parser
                for (int operationIndex = 0; operationIndex < operationSet.Operations.Count; operationIndex++)
                {
                    if (operationIndex > 0)
                        sb.AppendLine().Append(';').AppendLine();
                    sb.Append(operationSet.Operations[operationIndex].ToString());
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
            if (operationPrefixes.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                operationPrefixes.ForEach(pf => sb.AppendLine($"PREFIX {pf.NamespacePrefix}: <{pf.NamespaceUri}>"));
                sb.AppendLine();
                return sb.ToString();
            }
            return string.Empty;
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

            return $"  {tpString} .{Environment.NewLine}";
        }
        #endregion
    }
}