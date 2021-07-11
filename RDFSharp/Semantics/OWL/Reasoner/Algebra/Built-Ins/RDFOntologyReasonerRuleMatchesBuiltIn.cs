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
    /// RDFOntologyReasonerRuleMatchesBuiltIn represents a built-in of type swrlb:matches
    /// </summary>
    public class RDFOntologyReasonerRuleMatchesBuiltIn : RDFOntologyReasonerRuleBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the Uri of the built-in (swrlb:matches)
        /// </summary>
        private static RDFResource BuiltInUri = new RDFResource($"swrlb:matches");
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a swrlb:matches built-in with given arguments
        /// </summary>
        public RDFOntologyReasonerRuleMatchesBuiltIn(RDFVariable leftArgument, Regex matchesRegex)
            : base(new RDFOntologyResource() { Value = BuiltInUri }, leftArgument, null)
        {
            if (matchesRegex == null)
                throw new RDFSemanticsException("Cannot create built-in because given \"matchesRegex\" parameter is null.");

            //For printing, this built-in requires simulation of the right argument as plain literal
            StringBuilder regexFlags = new StringBuilder();
            if (matchesRegex.Options.HasFlag(RegexOptions.IgnoreCase))
                regexFlags.Append("i");
            if (matchesRegex.Options.HasFlag(RegexOptions.Singleline))
                regexFlags.Append("s");
            if (matchesRegex.Options.HasFlag(RegexOptions.Multiline))
                regexFlags.Append("m");
            if (matchesRegex.Options.HasFlag(RegexOptions.IgnorePatternWhitespace))
                regexFlags.Append("x");
            this.RightArgument = regexFlags.ToString() != string.Empty ? new RDFOntologyLiteral(new RDFPlainLiteral($"{matchesRegex}\",\"{regexFlags}"))
                                                                       : new RDFOntologyLiteral(new RDFPlainLiteral($"{matchesRegex}"));

            this.BuiltInFilter = new RDFRegexFilter(leftArgument, matchesRegex);
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