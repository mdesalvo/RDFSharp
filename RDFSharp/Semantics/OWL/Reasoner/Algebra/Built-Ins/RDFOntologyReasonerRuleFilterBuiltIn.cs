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

using RDFSharp.Query;
using System.Collections;
using System.Data;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRuleFilterBuiltIn represents a predefined kind of filter-based atom filtering inferences of a rule's antecedent
    /// </summary>
    public abstract class RDFOntologyReasonerRuleFilterBuiltIn : RDFOntologyReasonerRuleBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the built-in equivalent SPARQL filter
        /// </summary>
        internal RDFFilter BuiltInFilter { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a built-in with given predicate and arguments
        /// </summary>
        internal RDFOntologyReasonerRuleFilterBuiltIn(RDFOntologyResource predicate, RDFPatternMember leftArgument, RDFPatternMember rightArgument)
            : base(predicate, leftArgument, rightArgument) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the built-in in the context of the given antecedent results
        /// </summary>
        internal override DataTable Evaluate(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options)
        {
            DataTable filteredTable = antecedentResults.Clone();
            IEnumerator rowsEnum = antecedentResults.Rows.GetEnumerator();

            //Iterate the rows of the antecedent result table
            while (rowsEnum.MoveNext())
            {
                //Apply the built-in filter on the row
                bool keepRow = this.BuiltInFilter.ApplyFilter((DataRow)rowsEnum.Current, false);

                //If the row has passed the filter, keep it in the filtered result table
                if (keepRow)
                {
                    DataRow newRow = filteredTable.NewRow();
                    newRow.ItemArray = ((DataRow)rowsEnum.Current).ItemArray;
                    filteredTable.Rows.Add(newRow);
                }
            }

            return filteredTable;
        }
        #endregion
    }
}