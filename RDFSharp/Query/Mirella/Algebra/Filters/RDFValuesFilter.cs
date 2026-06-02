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
using System.Data;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFValuesFilter represents an explicit binding of variables provided for filtering a SPARQL query.
    /// (This filter is NOT intended to be public: in fact, it is injected by RDFValues during evaluation).
    /// </summary>
    internal sealed class RDFValuesFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// SPARQL values wrapped by the filter
        /// </summary>
        internal RDFValues Values { get; set; }

        /// <summary>
        /// Tabular representation of the SPARQL values wrapped by the filter
        /// </summary>
        internal RDFTable ValuesTable { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a SPARQL values filter
        /// </summary>
        internal RDFValuesFilter(RDFValues values)
        {
            Values = values;
            //Build the values table, carrying the join flags from the DataTable's ExtendedProperties
            //(an all-UNDEF values block is tagged as optional, mirroring RDFValues.GetDataTable)
            DataTable valuesDataTable = values.GetDataTable();
            ValuesTable = RDFTable.FromDataTable(valuesDataTable);
            ValuesTable.IsOptional = valuesDataTable.ExtendedProperties[RDFQueryEngine.IsOptional] is true;
            ValuesTable.JoinAsUnion = valuesDataTable.ExtendedProperties[RDFQueryEngine.JoinAsUnion] is true;
            ValuesTable.JoinAsMinus = valuesDataTable.ExtendedProperties[RDFQueryEngine.JoinAsMinus] is true;
        }
        #endregion

        #region Interfaces
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintValues(Values, prefixes, string.Empty);
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the columns corresponding to the variables in the given datarow
        /// </summary>
        internal override bool ApplyFilter(RDFTableRow row, bool applyNegation)
        {
            bool keepRow = true;

            //Check is performed only on columns found as bindings in the filter
            List<string> filterColumns = Values.Bindings.Keys.Where(k => row.HasColumn(k)).ToList();
            if (filterColumns.Count > 0)
            {
                //Get the enumerable representation of the filter table
                IEnumerable<RDFTableRow> valuesTableEnumerable = ValuesTable.Rows;

                //Perform the iterative check on the filter columns
                filterColumns.ForEach(filterColumn =>
                {
                    //Take the value of the column
                    string filterColumnValue = (row[filterColumn] ?? string.Empty);

                    //Filter the enumerable representation of the filter table
                    valuesTableEnumerable = valuesTableEnumerable.Where(binding =>
                        binding.IsUnbound(filterColumn) || string.Equals(binding[filterColumn], filterColumnValue, StringComparison.Ordinal));
                });

                //Analyze the response of the check
                keepRow = valuesTableEnumerable.Any();
            }

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion
    }
}