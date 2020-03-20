﻿/*
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
    /// RDFPartitionAggregator represents a PARTITION aggregation function applied by a GroupBy modifier
    /// </summary>
    internal class RDFPartitionAggregator : RDFSampleAggregator
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build a PARTITION aggregator on the given variable and with the given projection name
        /// </summary>
        internal RDFPartitionAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the PARTITION aggregator
        /// </summary>
        public override String ToString()
        {
            return String.Empty;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(String partitionKey, DataRow tableRow)
        {
            //Get aggregator value
            String aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<String>(partitionKey, String.Empty) ?? String.Empty;
            //Update aggregator context (partition)
            if (String.IsNullOrEmpty(aggregatorValue))
                this.AggregatorContext.UpdatePartitionKeyExecutionResult<String>(partitionKey, partitionKey);
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
            if (!bindings.ContainsKey(this.ProjectionVariable.VariableName))
                bindings.Add(this.ProjectionVariable.VariableName, aggregatorValue);

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion

    }

}