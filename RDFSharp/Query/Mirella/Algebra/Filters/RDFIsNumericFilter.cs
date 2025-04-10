/*
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
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFIsNumericFilter represents a filter for numeric typed literal values of a variable.
    /// </summary>
    public sealed class RDFIsNumericFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Name of the variable to be filtered
        /// </summary>
        public string VariableName { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable
        /// </summary>
        public RDFIsNumericFilter(RDFVariable variable)
        {
            #region Guards
            if (variable == null)
                throw new RDFQueryException("Cannot create RDFIsNumericFilter because given \"variable\" parameter is null.");
            #endregion

            VariableName = variable.ToString();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => $"FILTER ( ISNUMERIC({VariableName}) )";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(VariableName))
            {
                string variableValue = row[VariableName].ToString();

                //Successful match if a numeric typed literal can be created with the variable value
                RDFPatternMember variableValuePMember = RDFQueryUtilities.ParseRDFPatternMember(variableValue);
                if (variableValuePMember is RDFResource
                     || variableValuePMember is RDFPlainLiteral
                     || (variableValuePMember is RDFTypedLiteral variableValuePMemberTLit && !variableValuePMemberTLit.HasDecimalDatatype()))
                    keepRow = false;

                //Apply the eventual negation
                if (applyNegation)
                    keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion
    }
}