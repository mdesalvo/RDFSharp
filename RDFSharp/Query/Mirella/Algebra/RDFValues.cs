﻿/*
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

using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFValues represents a binding of variables provided directly inside a SPARQL query.
    /// </summary>
    public sealed class RDFValues : RDFPatternGroupMember
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
        /// Builds an empty SPARQL values
        /// </summary>
        public RDFValues()
        {
            Bindings = [];
            IsEvaluable = false;
            IsInjected = false;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SPARQL values
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList, string.Empty);
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
                if (!Bindings.ContainsKey(variableString))
                    Bindings.Add(variableString, []);

                //Populate bindings of the given variable
                //(null indicates the special UNDEF binding)
                if (bindings?.Count > 0)
                    bindings.ForEach(b => Bindings[variableString].Add(b is RDFResource || b is RDFLiteral ? b : null));
                else
                    Bindings[variableString].Add(null);

                //Mark the SPARQL values as evaluable
                IsEvaluable = true;
            }
            return this;
        }

        /// <summary>
        /// Gets the datatable representing the SPARQL values
        /// </summary>
        internal DataTable GetDataTable()
        {
            DataTable result = new DataTable();
            result.ExtendedProperties.Add(RDFQueryEngine.IsOptional, false);
            result.ExtendedProperties.Add(RDFQueryEngine.JoinAsUnion, false);
            result.ExtendedProperties.Add(RDFQueryEngine.JoinAsMinus, false);

            //Create the columns of the SPARQL values
            foreach (string bindingKey in Bindings.Keys)
                RDFQueryEngine.AddColumn(result, bindingKey);

            //Create the rows of the SPARQL values
            bool containsNullBindings = false;
            result.BeginLoadData();
            for (int i = 0; i < MaxBindingsLength(); i++)
            {
                Dictionary<string, string> bindings = [];
                foreach (KeyValuePair<string, List<RDFPatternMember>> binding in Bindings)
                {
                    RDFPatternMember bindingValue = binding.Value.ElementAtOrDefault(i);
                    bindings.Add(binding.Key, bindingValue?.ToString());
                    if (bindingValue == null)
                        containsNullBindings = true;
                }
                RDFQueryEngine.AddRow(result, bindings);
            }
            result.ExtendedProperties[RDFQueryEngine.IsOptional] = containsNullBindings;
            result.EndLoadData();

            return result;
        }

        /// <summary>
        /// Gets the current max length of the bindings
        /// </summary>
        internal int MaxBindingsLength()
            => Bindings?.Count > 0 ? Bindings.Max(x => x.Value.Count) : 0;

        /// <summary>
        /// Gets the filter representation of the SPARQL values
        /// </summary>
        internal RDFValuesFilter GetValuesFilter()
            => new RDFValuesFilter(this);
        #endregion
    }
}