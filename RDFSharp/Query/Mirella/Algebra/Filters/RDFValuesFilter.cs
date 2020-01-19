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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFValuesFilter represents an explicit binding of variables provided for filtering a SPARQL query.
    /// (This filter is NOT intended to be public: in fact, it is injected by RDFValues during evaluation).
    /// </summary>
    internal class RDFValuesFilter : RDFFilter
    {

        #region Properties
        /// <summary>
        /// SPARQL values wrapped by the filter
        /// </summary>
        internal RDFValues Values { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a SPARQL values filter
        /// </summary>
        internal RDFValuesFilter(RDFValues values)
        {
            this.Values = values;
        }
        #endregion

        #region Interfaces
        public override String ToString()
        {
            return this.ToString(new List<RDFNamespace>());
        }
        internal override String ToString(List<RDFNamespace> prefixes)
        {
            return RDFQueryPrinter.PrintValues(this.Values, prefixes, String.Empty);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the columns corresponding to the variables in the given datarow
        /// </summary>
        internal override Boolean ApplyFilter(DataRow row, Boolean applyNegation)
        {
            Boolean keepRow = true;

            //Check is performed only on columns found as bindings in the filter
            List<String> filterColumns = this.Values.Bindings.Keys.Where(k => row.Table.Columns.Contains(k)).ToList();
            if (filterColumns.Any())
            {

                //Get the enumerable representation of the filter table
                IEnumerable<DataRow> bindingsTable = this.Values.GetDataTable().AsEnumerable();

                //Perform the iterative check on the filter columns
                filterColumns.ForEach(filterColumn => 
                {

                    //Take the value of the column
                    String filterColumnValue = row[filterColumn].ToString();

                    //Filter the enumerable representation of the filter table
                    bindingsTable = bindingsTable.Where(binding => binding.IsNull(filterColumn) || binding[filterColumn].ToString().Equals(filterColumnValue));

                });

                //Analyze the response of the check
                keepRow = bindingsTable.Any();

            }

            //Apply the eventual negation
            if (applyNegation)
            {
                keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion

    }

}