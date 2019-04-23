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
    /// RDFGroupConcatAggregator represents a GROUP_CONCAT aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFGroupConcatAggregator : RDFAggregator
    {

        #region Properties
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a GROUP_CONCAT aggregator on the given variable and with the given projection name
        /// </summary>
        public RDFGroupConcatAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the GROUP_CONCAT aggregator
        /// </summary>
        public override String ToString()
        {
            return String.Format("(GROUP_CONCAT({0} (;SEPARATOR=)))", this.AggregatorVariable, this.ProjectionVariable);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the GROUP_CONCAT aggregator function on the given tablerow
        /// </summary>
        internal override void ExecuteAggregatorFunction(Dictionary<String, DataTable> partitionRegistry, String partitionKey, DataRow tableRow)
        {
            //TODO

        }

        /// <summary>
        /// Finalizes the GROUP_CONCAT aggregator function on the result table
        /// </summary>
        internal override void FinalizeAggregatorFunction(Dictionary<String, DataTable> partitionRegistry, DataTable resultTable)
        {
            //TODO

        }
        #endregion

    }

}