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
using System.Globalization;
using System.Text;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRuleAddBuiltIn represents a built-in of type swrlb:add
    /// </summary>
    public class RDFOntologyReasonerRuleAddBuiltIn : RDFOntologyReasonerRuleBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the Uri of the built-in (swrlb:add)
        /// </summary>
        private static RDFResource BuiltInUri = new RDFResource($"swrlb:add");

        /// <summary>
        /// Represents the numeric value to be added to the RightArgument for checking equality of the leftArgument
        /// </summary>
        private double AddValue { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a swrlb:add built-in with given arguments
        /// </summary>
        public RDFOntologyReasonerRuleAddBuiltIn(RDFVariable leftArgument, RDFVariable rightArgument, double addValue)
            : base(new RDFOntologyResource() { Value = BuiltInUri }, leftArgument, rightArgument)
                => this.AddValue = addValue;
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the built-in
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            //Predicate
            sb.Append(RDFModelUtilities.GetShortUri(((RDFResource)this.Predicate.Value).URI));

            //Arguments
            RDFTypedLiteral addValueTypedLiteral = new RDFTypedLiteral(this.AddValue.ToString(), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
            sb.Append($"({this.LeftArgument},{this.RightArgument},{RDFQueryPrinter.PrintPatternMember(addValueTypedLiteral, RDFNamespaceRegister.Instance.Register)})");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the built-in in the context of the given antecedent results
        /// </summary>
        internal override DataTable Evaluate(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options)
        {
            DataTable filteredTable = antecedentResults.Clone();

            //Preliminary checks for built-in's applicability (requires arguments to be well-known variables)
            string leftArgumentString = this.LeftArgument.ToString();
            if (!antecedentResults.Columns.Contains(leftArgumentString))
                return filteredTable;
            string rightArgumentString = this.RightArgument.ToString();
            if (!antecedentResults.Columns.Contains(rightArgumentString))
                return filteredTable;

            //Iterate the rows of the antecedent result table
            IEnumerator rowsEnum = antecedentResults.Rows.GetEnumerator();
            while (rowsEnum.MoveNext())
            {
                try
                {
                    //Fetch data corresponding to the built-in's arguments
                    DataRow currentRow = (DataRow)rowsEnum.Current;
                    string leftArgumentValue = currentRow[leftArgumentString].ToString();
                    string rightArgumentValue = currentRow[rightArgumentString].ToString();

                    //Transform fetched data to pattern members
                    RDFPatternMember leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(leftArgumentValue);
                    RDFPatternMember rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(rightArgumentValue);

                    //Check compatibility of pattern members with the built-in (requires numeric typed literals)
                    if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                            && leftArgumentTypedLiteral.HasDecimalDatatype()
                                && rightArgumentPMember is RDFTypedLiteral rightArgumentTypedLiteral
                                    && rightArgumentTypedLiteral.HasDecimalDatatype())
                    {
                        if (double.TryParse(leftArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double leftArgumentNumericValue)
                                && double.TryParse(rightArgumentTypedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double rightArgumentNumericValue))
                        {
                            //Execute the built-in's comparison logics (requires leftargument = rightargument + addValue)
                            bool keepRow = (leftArgumentNumericValue == rightArgumentNumericValue + this.AddValue);

                            //If the row has passed the built-in, keep it in the filtered result table
                            if (keepRow)
                            {
                                DataRow newRow = filteredTable.NewRow();
                                newRow.ItemArray = ((DataRow)rowsEnum.Current).ItemArray;
                                filteredTable.Rows.Add(newRow);
                            }
                        }
                    }
                }
                catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }
            }

            return filteredTable;
        }

        internal override DataTable EvaluateOnAntecedent(RDFOntology ontology, RDFOntologyReasonerOptions options) => null;
        internal override RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options) => null;
        #endregion
    }
}