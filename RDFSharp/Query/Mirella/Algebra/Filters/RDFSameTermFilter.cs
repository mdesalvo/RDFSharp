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
using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFSameTermFilter represents an equality filter between a variable and a RDF term.
    /// </summary>
    public sealed class RDFSameTermFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Name of the variable to be filtered
        /// </summary>
        public string VariableName { get; internal set; }

        /// <summary>
        /// RDF Term to be filtered (can be a RDFResource, a RDFLiteral or a RDFVariable)
        /// </summary>
        public RDFPatternMember RDFTerm { get; internal set; }
        internal string RDFTermString { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an equality filter between the given variable and the given RDF term
        /// </summary>
        public RDFSameTermFilter(RDFVariable variable, RDFPatternMember rdfTerm)
        {
            VariableName = variable?.ToString() ?? throw new RDFQueryException("Cannot create RDFSameTermFilter because given \"variable\" parameter is null.");
            RDFTerm = rdfTerm ?? throw new RDFQueryException("Cannot create RDFSameTermFilter because given \"rdfTerm\" parameter is null.");
            RDFTermString = rdfTerm.ToString();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat("FILTER ( SAMETERM(", VariableName, ", ", RDFQueryPrinter.PrintPatternMember(RDFTerm, prefixes), ") )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(VariableName))
            {
                string variableValue = row[VariableName].ToString();

                //Equality comparison with a RDFTerm being RDFVariable
                if (RDFTerm is RDFVariable)
                {
                    //Check is performed only if the row contains a column named like the filter's RDFTerm
                    if (row.Table.Columns.Contains(RDFTermString))
                    {
                        string rdfTermValue = row[RDFTermString].ToString();
                        keepRow = string.Equals(variableValue, rdfTermValue, StringComparison.Ordinal);
                    }
                }

                //Equality comparison with a RDFTerm being RDFResource/RDFLiteral
                else
                    keepRow = string.Equals(variableValue, RDFTermString, StringComparison.Ordinal);

                //Apply the eventual negation
                if (applyNegation)
                    keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion
    }
}