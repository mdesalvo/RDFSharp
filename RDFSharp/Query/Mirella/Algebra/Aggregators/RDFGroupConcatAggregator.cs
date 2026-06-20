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
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFGroupConcatAggregator represents a GROUP_CONCAT aggregation function applied by a GroupBy modifier
    /// </summary>
    public sealed class RDFGroupConcatAggregator : RDFAggregator
    {
        #region Properties
        /// <summary>
        /// Separator of the group values
        /// </summary>
        public string Separator { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a GROUP_CONCAT aggregator on the given variable, with the given projection name and given separator
        /// </summary>
        public RDFGroupConcatAggregator(RDFVariable aggrVariable, RDFVariable projVariable, string separator) : base(aggrVariable, projVariable)
            => Separator = string.IsNullOrEmpty(separator) ? " " : separator;

        /// <summary>
        /// Builds a GROUP_CONCAT aggregator on the given expression, with the given projection name and separator. The
        /// expression is materialized into a synthetic column before partitioning, the aggregator then operating on it.
        /// </summary>
        public RDFGroupConcatAggregator(RDFExpression aggrExpression, RDFVariable projVariable, string separator) : base(MakeExpressionVariable(projVariable), projVariable)
        {
            Separator = string.IsNullOrEmpty(separator) ? " " : separator;
            Metadata.AggregatorExpression = aggrExpression ?? throw new RDFQueryException("Cannot create RDFGroupConcatAggregator because given \"aggrExpression\" parameter is null.");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// The GROUP_CONCAT function (without the surrounding "(... AS ?proj)"), honoring DISTINCT and the separator.
        /// </summary>
        protected override string AggregatorFunction
            => Metadata.IsDistinct ? $"GROUP_CONCAT(DISTINCT {AggregatorArgument}; SEPARATOR=\"{Separator}\")"
                          : $"GROUP_CONCAT({AggregatorArgument}; SEPARATOR=\"{Separator}\")";
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
                if (AggregatorContext.CheckPartitionKeyRowValueCache(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                AggregatorContext.UpdatePartitionKeyRowValueCache(partitionKey, rowValue);
            }
            //Get aggregator value
            string aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, string.Empty) ?? string.Empty;
            //Update aggregator context (group_concat)
            AggregatorContext.UpdatePartitionKeyExecutionResult(partitionKey, string.Concat(aggregatorValue, rowValue, Separator));
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
            foreach (string partitionKey in AggregatorContext.ExecutionRegistry.Keys)
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
            string aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, string.Empty);
            bindings.Add(Metadata.ProjectionVariable.VariableName, aggregatorValue.TrimEnd(Separator));

            //Add bindings to result's table
            projFuncTable.AddRow(bindings);
        }
        #endregion
    }
}