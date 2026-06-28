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
        /// Dictionary of bindings representing the SPARQL values (one entry per declared variable, mapping it
        /// to its column of values; a null entry in a column is the special UNDEF binding). It is EMPTY for the
        /// degenerate NIL data block (<c>VALUES () { ... }</c>), which declares no variable at all.
        /// </summary>
        internal Dictionary<string, List<RDFPatternMember>> Bindings { get; set; }

        /// <summary>
        /// Number of empty-domain rows of a NIL data block (<c>VALUES () { () () }</c>): with zero declared
        /// variables the row cardinality cannot be derived from any column, so it is carried explicitly here.
        /// It is meaningful ONLY when <see cref="Bindings"/> is empty (otherwise the row count is the column
        /// length). Each such row is an identity solution mapping: 0 rows annihilate the join (zero solutions),
        /// 1 row is the identity (no-op), N rows duplicate every solution N-fold (multiset semantics).
        /// </summary>
        internal int NilRowsCount { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an empty SPARQL values
        /// </summary>
        public RDFValues()
        {
            Bindings = new Dictionary<string, List<RDFPatternMember>>();
            IsEvaluable = false;
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
                    Bindings.Add(variableString, new List<RDFPatternMember>());

                //Populate bindings of the given variable: each concrete term becomes a row of this column,
                //anything else (including the explicit UNDEF placeholder) becomes a null row. An EMPTY column
                //is left empty (zero rows): it represents the empty data block 'VALUES ?v { }', whose meaning
                //is "zero solutions" — it must NOT be coerced into a single all-UNDEF row.
                if (bindings?.Count > 0)
                    bindings.ForEach(b => Bindings[variableString].Add(b is RDFResource || b is RDFLiteral ? b : null));

                //Mark the SPARQL values as evaluable (the column has been declared, even if it has no rows)
                IsEvaluable = true;
            }
            return this;
        }

        /// <summary>
        /// Gets the table representing the SPARQL values
        /// </summary>
        internal RDFTable GetRDFTable()
        {
            RDFTable result = new RDFTable();

            //Create the columns of the SPARQL values
            foreach (string bindingKey in Bindings.Keys)
                result.AddColumn(bindingKey);

            //NIL data block ('VALUES () { ... }'): zero columns, NilRowsCount empty-domain rows. Each row is an
            //empty cells array; the existing join then yields the correct multiset on its own (0 -> annihilate /
            //zero solutions, 1 -> identity, N -> N-fold duplication). No column means no UNDEF, so it is never
            //optional. This is the single place that recognizes the NIL shape (no special branch in the engine).
            if (Bindings.Count == 0)
            {
                for (int i = 0; i < NilRowsCount; i++)
                    result.AddRow(Array.Empty<string>());
                return result;
            }

            //Create the rows of the SPARQL values
            bool containsNullBindings = false;
            for (int i = 0; i < MaxBindingsLength(); i++)
            {
                Dictionary<string, string> bindings = new Dictionary<string, string>();
                foreach (KeyValuePair<string, List<RDFPatternMember>> binding in Bindings)
                {
                    RDFPatternMember bindingValue = binding.Value.ElementAtOrDefault(i);
                    bindings.Add(binding.Key, bindingValue?.ToString());
                    if (bindingValue == null)
                        containsNullBindings = true;
                }
                result.AddRow(bindings);
            }

            //An all-UNDEF values block behaves as optional
            result.IsOptional = containsNullBindings;

            return result;
        }

        /// <summary>
        /// Gets the current row count of the SPARQL values: the longest column when at least one variable is
        /// declared, otherwise the explicit NIL row count (the variable-less data block has no column to count).
        /// </summary>
        internal int MaxBindingsLength()
            => Bindings?.Count > 0 ? Bindings.Max(x => x.Value.Count) : NilRowsCount;
        #endregion
    }
}