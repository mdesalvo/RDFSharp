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
    /// RDFBooleanNotFilter represents a filter applying a negation on the logics of the given filter.
    /// </summary>
    public class RDFBooleanNotFilter: RDFFilter {

        #region Properties
        /// <summary>
        /// Filter to be negated
        /// </summary>
        public RDFFilter Filter { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a negation filter on the given filter
        /// </summary>
        public RDFBooleanNotFilter(RDFFilter filter) {
            if (filter       != null) {
                this.Filter   = filter;
                this.FilterID = RDFModelUtilities.CreateHash(this.ToString());
            }
            else {
                throw new RDFQueryException("Cannot create RDFBooleanNotFilter because given \"filter\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter 
        /// </summary>
        public override String ToString() {
            return "FILTER ( !" + this.Filter.ToString().Replace("FILTER ", String.Empty).Trim() + " )";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow
        /// </summary>
        internal override Boolean ApplyFilter(DataRow row, Boolean applyNegation) {
            //Negation logic is applied on the given filter result
            Boolean keepRow = this.Filter.ApplyFilter(row, true);

            //Apply the eventual negation
            if (applyNegation) {
                keepRow     = !keepRow;
            }

            return keepRow;
        }
        #endregion

    }

}