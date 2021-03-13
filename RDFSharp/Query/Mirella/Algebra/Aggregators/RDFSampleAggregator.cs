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

using System;
using System.Collections.Generic;
using System.Data;

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
        public override string ToString()
            => this.IsDistinct ? string.Format("(SAMPLE(DISTINCT {0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable)
                               : string.Format("(SAMPLE({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable);
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, DataRow tableRow)
        {
            //Get row value
            string rowValue = GetRowValueAsString(tableRow);
            if (this.IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (this.AggregatorContext.CheckPartitionKeyRowValueCache<string>(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                else
                    this.AggregatorContext.UpdatePartitionKeyRowValueCache<string>(partitionKey, rowValue);
            }
            //Get aggregator value
            string aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<string>(partitionKey, string.Empty) ?? string.Empty;
            //Update aggregator context (sample)
            if (string.IsNullOrEmpty(aggregatorValue))
                this.AggregatorContext.UpdatePartitionKeyExecutionResult<string>(partitionKey, rowValue);
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
            foreach (string partitionKey in this.AggregatorContext.ExecutionRegistry.Keys)
            {
                //Update result's table
                this.UpdateProjectionTable(partitionKey, projFuncTable);
            }

            return projFuncTable;
        }

        /// <summary>
        /// Helps in finalization step by updating the projection's result table
        /// </summary>
        internal override void UpdateProjectionTable(string partitionKey, DataTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<string, string> bindings = new Dictionary<string, string>();
            foreach (string pkValue in partitionKey.Split(new string[] { "§PK§" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pValues = pkValue.Split(new string[] { "§PV§" }, StringSplitOptions.None);
                bindings.Add(pValues[0], pValues[1]);
            }

            //Add aggregator value to bindings
            string aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<string>(partitionKey, string.Empty);
            bindings.Add(this.ProjectionVariable.VariableName, aggregatorValue);

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion

    }

}