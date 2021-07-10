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
using RDFSharp.Query;
using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRuleContainsBuiltIn represents a built-in of type swrlb:contains
    /// </summary>
    public class RDFOntologyReasonerRuleContainsBuiltIn : RDFOntologyReasonerRuleBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the Uri of the built-in (swrlb:contains)
        /// </summary>
        private static RDFResource BuiltInUri = new RDFResource($"swrlb:contains");
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a swrlb:contains built-in with given arguments
        /// </summary>
        public RDFOntologyReasonerRuleContainsBuiltIn(RDFVariable leftArgument, string containString)
            : base(new RDFOntologyResource() { Value = BuiltInUri }, leftArgument, null)
        {
            if (containString == null)
                throw new RDFSemanticsException("Cannot create built-in because given \"endString\" parameter is null.");

            //For printing, this built-in requires simulation of the right argument as plain literal
            this.RightArgument = new RDFOntologyLiteral(new RDFPlainLiteral($"\"{containString}\""));

            this.BuiltInFilter = new RDFRegexFilter(leftArgument, new Regex($"{containString}"));
        }
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

        internal override DataTable EvaluateOnAntecedent(RDFOntology ontology, RDFOntologyReasonerOptions options) => null;
        internal override RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options) => null;
        #endregion
    }
}