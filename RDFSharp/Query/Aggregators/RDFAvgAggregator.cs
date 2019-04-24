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
using System.Linq;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFAvgAggregator represents an AVG aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFAvgAggregator: RDFAggregator
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build an AVG aggregator on the given variable and with the given projection name
        /// </summary>
        public RDFAvgAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the AVG aggregator
        /// </summary>
        public override String ToString()
        {
            return String.Format("(AVG({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the AVG aggregator function on the given tablerow
        /// </summary>
        internal override void ExecuteAggregatorFunction(String partitionKey, DataRow tableRow)
        {
            //Get row value
            Decimal rowValue = GetRowValueAsDecimal(tableRow);
            //Get aggregator value
            Decimal aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<Decimal>(partitionKey);
            //Update aggregator context (sum, count)
            this.AggregatorContext.UpdatePartitionKeyExecutionResult<Decimal>(partitionKey, rowValue + aggregatorValue);
            this.AggregatorContext.UpdatePartitionKeyExecutionCounter(partitionKey);
        }

        /// <summary>
        /// Finalizes the AVG aggregator function on the result table
        /// </summary>
        internal override void FinalizeAggregatorFunction(List<RDFVariable> partitionVariables, DataTable workingTable)
        {
            foreach(String partitionKey in this.AggregatorContext.AggregatorContextRegistry.Keys)
            {
                //Get aggregator value
                Decimal aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<Decimal>(partitionKey);
                //Get aggregator counter
                Decimal aggregatorCounter = this.AggregatorContext.GetPartitionKeyExecutionCounter(partitionKey);
                //Update aggregator context (avg)
                this.AggregatorContext.UpdatePartitionKeyExecutionResult<Decimal>(partitionKey, aggregatorValue / aggregatorCounter);
                //Update working table
                this.UpdateWorkingTable(partitionKey, workingTable);
            }
        }
        #endregion

    }

}