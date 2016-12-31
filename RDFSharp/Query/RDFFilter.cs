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

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFFilter represents a filter to be applied on a query results table.
    /// </summary>
    public abstract class RDFFilter: IEquatable<RDFFilter> {

        #region Properties
        /// <summary>
        /// Unique representation of the filter
        /// </summary>
        protected Int64 FilterID { get; set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override String ToString() {
            return base.ToString();
        }

        /// <summary>
        /// Performs the equality comparison between two filters
        /// </summary>
        public Boolean Equals(RDFFilter other) {
            return (other != null && this.FilterID.Equals(other.FilterID));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow 
        /// </summary>
        internal abstract Boolean ApplyFilter(DataRow row, Boolean applyNegation);
        #endregion

    }

}