/*
   Copyright 2012-2026 Marco De Salvo

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

using System.Collections.Generic;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFSampleAggregator represents a SAMPLE aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFSampleAggregator : RDFAggregator
    {
        #region Ctors
        /// <summary>
        /// Builds a SAMPLE aggregator on the given variable and with the given projection name
        /// </summary>
        public RDFSampleAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }

        /// <summary>
        /// Builds a SAMPLE aggregator on the given expression and with the given projection name. The expression is
        /// materialized into a synthetic column before partitioning, the aggregator then operating on it.
        /// </summary>
        public RDFSampleAggregator(RDFExpression aggrExpression, RDFVariable projVariable) : base(MakeExpressionVariable(projVariable), projVariable)
            => Metadata.AggregatorExpression = aggrExpression ?? throw new RDFQueryException("Cannot create RDFSampleAggregator because given \"aggrExpression\" parameter is null.");
        #endregion

        #region Interfaces
        /// <summary>
        /// The SAMPLE function (without the surrounding "(... AS ?proj)"), honoring DISTINCT.
        /// </summary>
        protected override string AggregatorFunction
            => Metadata.IsDistinct ? $"SAMPLE(DISTINCT {AggregatorArgument})" : $"SAMPLE({AggregatorArgument})";
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, RDFTableRow tableRow)
        {
            //Get row value
            string rowValue = GetRowValueAsString(tableRow);
            if (Metadata.IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (Context.CheckPartitionKeyRowValueCache(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                Context.UpdatePartitionKeyRowValueCache(partitionKey, rowValue);
            }
            //Get aggregator value
            string aggregatorValue = Context.GetPartitionKeyExecutionResult(partitionKey, string.Empty) ?? string.Empty;
            //Update aggregator context (sample)
            if (string.IsNullOrEmpty(aggregatorValue))
                Context.UpdatePartitionKeyExecutionResult(partitionKey, rowValue);
        }

        /// <summary>
        /// Executes the projection producing result's table
        /// </summary>
        internal override RDFTable ExecuteProjectionTable(List<RDFVariable> partitionVariables)
        {
            RDFTable projFuncTable = new RDFTable();

            //Initialization
            partitionVariables.ForEach(pv =>
                projFuncTable.AddColumn(pv.VariableName));
            projFuncTable.AddColumn(Metadata.ProjectionVariable.VariableName);

            //Finalization
            foreach (string partitionKey in Context.ExecutionRegistry.Keys)
            {
                //Update result's table
                UpdateProjectionTable(partitionKey, projFuncTable);
            }

            return projFuncTable;
        }

        /// <summary>
        /// Helps in finalization step by updating the projection's result table
        /// </summary>
        internal override void UpdateProjectionTable(string partitionKey, RDFTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<string, string> bindings = GetProjectionBindings(partitionKey);

            //Add aggregator value to bindings
            string aggregatorValue = Context.GetPartitionKeyExecutionResult(partitionKey, string.Empty);
            bindings.Add(Metadata.ProjectionVariable.VariableName, aggregatorValue);

            //Add bindings to result's table
            projFuncTable.AddRow(bindings);
        }
        #endregion
    }
}