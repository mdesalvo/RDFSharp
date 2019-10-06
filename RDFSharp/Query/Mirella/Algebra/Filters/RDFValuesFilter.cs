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

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFValuesFilter represents an explicit binding of variables provided for filtering a SPARQL query.
    /// </summary>
    public class RDFValuesFilter : RDFFilter
    {

        #region Properties
        /// <summary>
        /// Dictionary of bindings representing the SPARQL values
        /// </summary>
        internal Dictionary<String, List<RDFPatternMember>> Bindings { get; set; }
        
        /// <summary>
        /// Represents the current max length of the bindings
        /// </summary>
        internal Int32 MaxBindingsLength
        {
            get
            {
                return this.Bindings?.Select(x => x.Value.Count).Max() ?? 0;
            }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SPARQL values filter
        /// </summary>
        public RDFValuesFilter()
        {
            this.Bindings = new Dictionary<String, List<RDFPatternMember>>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SPARQL values filter
        /// </summary>
        public override String ToString()
        {
            return this.ToString(new List<RDFNamespace>());
        }
        internal override String ToString(List<RDFNamespace> prefixes)
        {
            return this.ToString(prefixes, String.Empty);
        }
        internal String ToString(List<RDFNamespace> prefixes, String spaces)
        {
            StringBuilder result = new StringBuilder();

            //Compact representation
            if (this.Bindings.Keys.Count == 1)
            {
                result.Append(String.Format("VALUES {0}", this.Bindings.Keys.ElementAt(0)));
                result.Append(" { ");
                foreach (RDFPatternMember binding in this.Bindings.ElementAt(0).Value)
                {
                    if (binding == null || binding.ToString().Equals(String.Empty, StringComparison.OrdinalIgnoreCase))
                        result.Append("UNDEF");
                    else
                        result.Append(RDFQueryPrinter.PrintPatternMember(binding, prefixes));
                    result.Append(" ");
                }
                result.Append("} ");
            }

            //Extended representation
            else
            {
                result.Append(String.Format("VALUES ({0})", String.Join(" ", this.Bindings.Keys)));
                result.Append(" {\n");
                for (int i = 0; i < this.MaxBindingsLength; i++)
                {
                    result.Append(spaces + "      ( ");
                    this.Bindings.ToList().ForEach(binding => {
                        RDFPatternMember bindingValue = binding.Value.ElementAtOrDefault(i);
                        if (bindingValue == null || bindingValue.ToString().Equals(String.Empty, StringComparison.OrdinalIgnoreCase))
                            result.Append("UNDEF");
                        else
                            result.Append(RDFQueryPrinter.PrintPatternMember(bindingValue, prefixes));
                        result.Append(" ");
                    });
                    result.Append(")\n");
                }
                result.Append(spaces + "    }");
            }

            return result.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given bindings to the given variable of the SPARQL values filter
        /// </summary>
        public RDFValuesFilter AddBindings(RDFVariable variable, List<RDFPatternMember> bindings)
        {
            if (variable != null)
            {

                //Initialize bindings of the given variable
                if (!this.Bindings.ContainsKey(variable.ToString()))
                    this.Bindings.Add(variable.ToString(), new List<RDFPatternMember>());

                //Populate bindings of the given variable (null indicates UNDEF)
                if (bindings?.Any() ?? false)
                    bindings.ForEach(b => this.Bindings[variable.ToString()].Add((b is RDFResource || b is RDFLiteral) ? b : null));
                else
                    this.Bindings[variable.ToString()].Add(null);

            }
            return this;
        }

        /// <summary>
        /// Applies the filter on the columns corresponding to the variables in the given datarow
        /// </summary>
        internal override Boolean ApplyFilter(DataRow row, Boolean applyNegation)
        {
            Boolean keepRow = true;

            //Check is performed only on columns found as bindings in the filter
            List<String> filterColumns = this.Bindings.Keys.Where(k => row.Table.Columns.Contains(k)).ToList();
            if (filterColumns.Any())
            {

                //Get the enumerable representation of the filter table
                IEnumerable<DataRow> bindingsTable = this.GetDataTable().AsEnumerable();

                //Perform the iterative check on the filter columns
                filterColumns.ForEach(filterColumn => 
                {

                    //Take the value of the column
                    String filterColumnValue = row[filterColumn]?.ToString() ?? String.Empty;

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

        /// <summary>
        /// Gets the datatable representing the SPARQL values filter
        /// </summary>
        internal DataTable GetDataTable()
        {
            DataTable result = new DataTable();

            //Create the columns of the SPARQL values
            this.Bindings.ToList()
                         .ForEach(b => RDFQueryEngine.AddColumn(result, b.Key));
            result.AcceptChanges();

            //Create the rows of the SPARQL values
            result.BeginLoadData();
            for (int i = 0; i < this.MaxBindingsLength; i++)
            {
                Dictionary<String, String> bindings = new Dictionary<String, String>();
                this.Bindings.ToList()
                             .ForEach(b => bindings.Add(b.Key, b.Value.ElementAtOrDefault(i)?.ToString()));
                RDFQueryEngine.AddRow(result, bindings);
            }
            result.EndLoadData();

            return result;
        }
        #endregion

    }

}