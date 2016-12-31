/*
   Copyright 2012-2017 Marco De Salvo

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
using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFIsLiteralFilter represents a filter for literal values of a variable.
    /// </summary>
    public class RDFIsLiteralFilter: RDFFilter {

        #region Properties
        /// <summary>
        /// Variable to be filtered
        /// </summary>
        public RDFVariable Variable { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable
        /// </summary>
        public RDFIsLiteralFilter(RDFVariable variable) {
            if (variable     != null) {
                this.Variable = variable;
                this.FilterID = RDFModelUtilities.CreateHash(this.ToString());
            }
            else {
                throw new RDFQueryException("Cannot create RDFIsLiteralFilter because given \"variable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter 
        /// </summary>
        public override String ToString() {
            return "FILTER ( ISLITERAL(" + this.Variable + ") )";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override Boolean ApplyFilter(DataRow row, Boolean applyNegation) {
            Boolean keepRow  = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(this.Variable.ToString())) {
 
                //Apply a negation logic on result of an "IsUri" filter
                RDFIsUriFilter isUriFilter = new RDFIsUriFilter(this.Variable);
                keepRow      = isUriFilter.ApplyFilter(row, true);

                //Apply the eventual negation
                if (applyNegation) {
                    keepRow  = !keepRow;
                }
            }

            return keepRow;
        }
        #endregion

    }

}