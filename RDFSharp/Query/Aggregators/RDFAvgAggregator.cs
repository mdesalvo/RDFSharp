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
        /// Executes the partition function on the given tablerow
        /// </summary>
        internal override void ExecutePartitionFunction(String partitionKey, DataRow tableRow)
        {
            //Get row value
            Double rowValue = GetRowValueAsNumber(tableRow);
            //Get aggregator value
            Double aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<Double>(partitionKey);
            //In case of non-numeric values, consider partitioning failed
            Double newAggregatorValue = Double.NaN;
            if (!aggregatorValue.Equals(Double.NaN) && !rowValue.Equals(Double.NaN))
                newAggregatorValue = rowValue + aggregatorValue;
            //Update aggregator context (sum, count)
            this.AggregatorContext.UpdatePartitionKeyExecutionResult<Double>(partitionKey, newAggregatorValue);
            this.AggregatorContext.UpdatePartitionKeyExecutionCounter(partitionKey);
        }

        /// <summary>
        /// Executes the projection function producing result's table
        /// </summary>
        internal override DataTable ExecuteProjectionFunction(List<RDFVariable> partitionVariables)
        {
            DataTable projFuncTable = new DataTable();

            //Initialization
            partitionVariables.ForEach(pv =>
                RDFQueryEngine.AddColumn(projFuncTable, pv.VariableName));
            RDFQueryEngine.AddColumn(projFuncTable, this.ProjectionVariable.VariableName);

            //Finalization
            foreach (String partitionKey in this.AggregatorContext.AggregatorContextRegistry.Keys)
            {
                //Get aggregator value
                Double aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<Double>(partitionKey);
                //Get aggregator counter
                Double aggregatorCounter = this.AggregatorContext.GetPartitionKeyExecutionCounter(partitionKey);
                //In case of non-numeric values, consider partition failed
                Double finalAggregatorValue = Double.NaN;
                if (!aggregatorValue.Equals(Double.NaN))
                    finalAggregatorValue = aggregatorValue / aggregatorCounter;
                //Update aggregator context (sum, count)
                this.AggregatorContext.UpdatePartitionKeyExecutionResult<Double>(partitionKey, finalAggregatorValue);
                //Update result's table
                this.UpdateProjectionFunctionTable(partitionKey, projFuncTable);
            }

            return projFuncTable;
        }
        #endregion

    }

}