/*
   Copyright 2012-2020 Marco De Salvo

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
                if (prefixes.Any())
                {
                    prefixes.ForEach(pf => sb.Append(string.Concat("PREFIX ", pf.NamespacePrefix, ": <", pf.NamespaceUri.ToString(), ">\n")));
                    sb.Append("\n");
                }
                #endregion

                #region TEMPLATES
                sb.Append("INSERT DATA\n{\n");
                insertDataOperation.InsertTemplates.ForEach(tp => sb.Append(PrintPattern(insertDataOperation, tp)));
                sb.Append("}\n");
                #endregion
            }

            return sb.ToString();
        }
        #endregion

        #region Utilities
        //Prints the given pattern
        private static string PrintPattern(RDFInsertDataOperation insertDataOperation, RDFPattern tp)
        {
            string tpString = RDFQueryPrinter.PrintPattern(tp, insertDataOperation.Prefixes);

            //Remove the Optional indicator from the template print (since it is not supported in SPARQL UPDATE operations)
            if (tp.IsOptional)
                tpString = tpString.Replace("OPTIONAL { ", string.Empty).TrimEnd(new char[] { ' ', '}' });

            return string.Concat("  ", tpString, " .\n");
        }
        #endregion
    }
}