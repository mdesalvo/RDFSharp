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
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFDatatypeFilter represents a filter on the datatype of a variable.
    /// </summary>
    public class RDFDatatypeFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Name of the variable to be filtered
        /// </summary>
        public string VariableName { get; internal set; }

        /// <summary>
        /// Datatype to be filtered
        /// </summary>
        public RDFModelEnums.RDFDatatypes Datatype { get; internal set; }

        /// <summary>
        /// Regex to be applied for datatype filtering
        /// </summary>
        internal Regex DatatypeRegex {get; set;}
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable for the given datatype
        /// </summary>
        public RDFDatatypeFilter(RDFVariable variable, RDFModelEnums.RDFDatatypes datatype)
        {
            if (variable == null)
                throw new RDFQueryException("Cannot create RDFDatatypeFilter because given \"variable\" parameter is null.");

            this.VariableName = variable.ToString();
            this.Datatype = datatype;
            this.DatatypeRegex = new Regex($"\\^\\^{RDFModelUtilities.GetDatatypeFromEnum(this.Datatype)}$");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat(
                "FILTER ( DATATYPE(", this.VariableName, ") = ",
                RDFQueryPrinter.PrintPatternMember(RDFQueryUtilities.ParseRDFPatternMember(RDFModelUtilities.GetDatatypeFromEnum(this.Datatype)), prefixes), " )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(this.VariableName))
            {
                string variableValue = row[this.VariableName].ToString();

                //Successfull match if given datatype is found in the variable
                keepRow = this.DatatypeRegex.IsMatch(variableValue);

                //Apply the eventual negation
                if (applyNegation)
                    keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion
    }
}