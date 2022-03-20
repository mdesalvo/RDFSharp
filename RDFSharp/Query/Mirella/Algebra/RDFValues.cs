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
using System.Data;
using System.Linq;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFValues represents a binding of variables provided directly inside a SPARQL query.
    /// </summary>
    public class RDFValues : RDFPatternGroupMember
    {
        #region Properties
        /// <summary>
        /// Dictionary of bindings representing the SPARQL values
        /// </summary>
        internal Dictionary<string, List<RDFPatternMember>> Bindings { get; set; }

        /// <summary>
        /// Flag indicating that the SPARQL values has been injected by Mirella
        /// </summary>
        internal bool IsInjected { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SPARQL values
        /// </summary>
        public RDFValues()
        {
            this.Bindings = new Dictionary<string, List<RDFPatternMember>>();
            this.IsEvaluable = false;
            this.IsInjected = false;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SPARQL values
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>(), string.Empty);
        internal string ToString(List<RDFNamespace> prefixes, string spaces)
            => RDFQueryPrinter.PrintValues(this, prefixes, spaces);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given column of bindings to the SPARQL values
        /// </summary>
        public RDFValues AddColumn(RDFVariable variable, List<RDFPatternMember> bindings)
        {
            if (variable != null)
            {
                string variableString = variable.ToString();

                //Initialize bindings of the given variable
                if (!this.Bindings.ContainsKey(variableString))
                    this.Bindings.Add(variableString, new List<RDFPatternMember>());

                //Populate bindings of the given variable
                //(null indicates the special UNDEF binding)
                if (bindings?.Any() ?? false)
                    bindings.ForEach(b => this.Bindings[variableString].Add((b is RDFResource || b is RDFLiteral) ? b : null));
                else
                    this.Bindings[variableString].Add(null);

                //Mark the SPARQL values as evaluable
                this.IsEvaluable = true;
            }
            return this;
        }

        /// <summary>
        /// Gets the datatable representing the SPARQL values
        /// </summary>
        internal DataTable GetDataTable()
        {
            DataTable result = new DataTable();
            result.ExtendedProperties.Add("IsOptional", false);
            result.ExtendedProperties.Add("JoinAsUnion", false);

            //Create the columns of the SPARQL values
            this.Bindings.ToList()
                         .ForEach(b => RDFQueryEngine.AddColumn(result, b.Key));
            result.AcceptChanges();

            //Create the rows of the SPARQL values
            result.BeginLoadData();
            for (int i = 0; i < this.MaxBindingsLength(); i++)
            {
                Dictionary<string, string> bindings = new Dictionary<string, string>();
                this.Bindings.ToList()
                             .ForEach(b =>
                             {
                                 RDFPatternMember bindingValue = b.Value.ElementAtOrDefault(i);
                                 bindings.Add(b.Key, bindingValue?.ToString());
                                 if (bindingValue == null)
                                     result.ExtendedProperties["IsOptional"] = true;
                             });
                RDFQueryEngine.AddRow(result, bindings);
            }
            result.EndLoadData();

            return result;
        }

        /// <summary>
        /// Gets the current max length of the bindings
        /// </summary>
        internal int MaxBindingsLength()
        {
            if (this.Bindings?.Count > 0)
                return this.Bindings.Select(x => x.Value.Count).Max();
            else
                return 0;
        }

        /// <summary>
        /// Gets the filter representation of the SPARQL values
        /// </summary>
        internal RDFValuesFilter GetValuesFilter()
            => new RDFValuesFilter(this);
        #endregion
    }
}