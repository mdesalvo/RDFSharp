﻿/*
   Copyright 2012-2025 Marco De Salvo

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
    /// RDFPartitionAggregator represents a PARTITION aggregation function applied by a GroupBy modifier
    /// </summary>
    internal sealed class RDFPartitionAggregator : RDFSampleAggregator
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
        public override string ToString()
            => string.Empty;
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, DataRow tableRow)
        {
            //Get aggregator value
            string aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, string.Empty) ?? string.Empty;
            //Update aggregator context (partition)
            if (string.IsNullOrEmpty(aggregatorValue))
                AggregatorContext.UpdatePartitionKeyExecutionResult(partitionKey, partitionKey);
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
            RDFQueryEngine.AddColumn(projFuncTable, ProjectionVariable.VariableName);

            //Finalization
            foreach (string partitionKey in AggregatorContext.ExecutionRegistry.Keys)
                //Update result's table
                UpdateProjectionTable(partitionKey, projFuncTable);

            return projFuncTable;
        }

        /// <summary>
        /// Helps in finalization step by updating the projection's result table
        /// </summary>
        internal override void UpdateProjectionTable(string partitionKey, DataTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<string, string> bindings = partitionKey.Split(ProjectionKeyPlaceholder, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(pkValue => pkValue.Split(ProjectionValuePlaceholder, StringSplitOptions.None)).ToDictionary(pValues => pValues[0], pValues => pValues[1]);

            //Add aggregator value to bindings
            string aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, string.Empty);
            if (!bindings.ContainsKey(ProjectionVariable.VariableName))
                bindings.Add(ProjectionVariable.VariableName, aggregatorValue);

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion
    }
}