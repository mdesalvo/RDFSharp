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
        /// Variable to be filtered
        /// </summary>
        public RDFVariable Variable { get; internal set; }

        /// <summary>
        /// Datatype to be filtered
        /// </summary>
        public RDFModelEnums.RDFDatatypes Datatype { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable for the given datatype
        /// </summary>
        public RDFDatatypeFilter(RDFVariable variable, RDFModelEnums.RDFDatatypes datatype)
        {
            if (variable != null)
            {
                this.Variable = variable;
                this.Datatype = datatype;
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFDatatypeFilter because given \"variable\" parameter is null.");
            }
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
                "FILTER ( DATATYPE(", this.Variable, ") = ",
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
            if (row.Table.Columns.Contains(this.Variable.ToString()))
            {
                string variableValue = row[this.Variable.ToString()].ToString();

                //Successfull match if given datatype is found in the variable
                keepRow = Regex.IsMatch(variableValue, string.Concat("\\^\\^", RDFModelUtilities.GetDatatypeFromEnum(this.Datatype), "$"));

                //Apply the eventual negation
                if (applyNegation)
                    keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion

    }

}