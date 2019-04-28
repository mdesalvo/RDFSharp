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
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFSampleAggregator represents a SAMPLE aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFSampleAggregator : RDFAggregator
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build a SAMPLE aggregator on the given variable and with the given projection name
        /// </summary>
        public RDFSampleAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the SAMPLE aggregator
        /// </summary>
        public override String ToString()
        {
            return (this.IsDistinct ? String.Format("(SAMPLE(DISTINCT {0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable)
                                    : String.Format("(SAMPLE({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(String partitionKey, DataRow tableRow)
        {
            //Get row value
            String rowValue = GetRowValueAsString(tableRow);
            if (this.IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (this.AggregatorContext.CheckPartitionKeyRowValueCache<String>(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                else
                    this.AggregatorContext.UpdatePartitionKeyRowValueCache<String>(partitionKey, rowValue);
            }
            //Get aggregator value
            String aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<String>(partitionKey, String.Empty) ?? String.Empty;
            //Update aggregator context (sample)
            if (String.IsNullOrEmpty(aggregatorValue))
                this.AggregatorContext.UpdatePartitionKeyExecutionResult<String>(partitionKey, rowValue);
        }

        /// <summary>
        /// Executes the projection producing result's table
        /// </summary>
        internal override DataTable ExecuteProjection(List<RDFVariable> partitionVariables)
        {
            DataTable projFuncTable = new DataTable();

            //Initialization
            partitionVariables.ForEach(pv =>
                RDFQueryEngine.AddColumn(projFuncTable, pv.VariableName));
            RDFQueryEngine.AddColumn(projFuncTable, this.ProjectionVariable.VariableName);

            //Finalization
            foreach (String partitionKey in this.AggregatorContext.ExecutionRegistry.Keys)
            {
                //Update result's table
                this.UpdateProjectionTable(partitionKey, projFuncTable);
            }

            return projFuncTable;
        }

        /// <summary>
        /// Helps in finalization step by updating the projection's result table 
        /// </summary>
        internal override void UpdateProjectionTable(String partitionKey, DataTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<String, String> bindings = new Dictionary<String, String>();
            foreach (String pkValue in partitionKey.Split(new String[] { "§PK§" }, StringSplitOptions.RemoveEmptyEntries))
            {
                String[] pValues = pkValue.Split(new String[] { "§PV§" }, StringSplitOptions.None);
                bindings.Add(pValues[0], pValues[1]);
            }
        
            //Add aggregator value to bindings
            String aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<String>(partitionKey, String.Empty);
            bindings.Add(this.ProjectionVariable.VariableName, aggregatorValue);

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion

    }

}