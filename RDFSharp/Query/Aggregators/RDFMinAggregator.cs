/*
   Copyright 2012-2019 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFMinAggregator represents a MIN aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFMinAggregator : RDFAggregator
    {

        #region Properties
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a MIN aggregator on the given variable and with the given projection name
        /// </summary>
        public RDFMinAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the MIN aggregator
        /// </summary>
        public override String ToString()
        {
            return String.Format("(MIN({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the MIN aggregator function on the given tablerow
        /// </summary>
        internal override void ExecuteAggregatorFunction(String partitionKey, DataRow tableRow)
        {
            //TODO

        }

        /// <summary>
        /// Finalizes the MIN aggregator function on the result table
        /// </summary>
        internal override void FinalizeAggregatorFunction(DataTable workingTable)
        {
            //TODO

        }
        #endregion

    }

}