/*
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
        /// Default-ctor to build a GROUP_CONCAT aggregator on the given variable, with the given projection name and given separator
        /// </summary>
        public RDFGroupConcatAggregator(RDFVariable aggrVariable, RDFVariable projVariable, string separator) : base(aggrVariable, projVariable)
            => Separator = string.IsNullOrEmpty(separator) ? " " : separator;
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the GROUP_CONCAT aggregator
        /// </summary>
        public override string ToString()
            => IsDistinct ? $"(GROUP_CONCAT(DISTINCT {AggregatorVariable}; SEPARATOR=\"{Separator}\") AS {ProjectionVariable})"
                          : $"(GROUP_CONCAT({AggregatorVariable}; SEPARATOR=\"{Separator}\") AS {ProjectionVariable})";
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, DataRow tableRow)
        {
            //Get row value
            string rowValue = GetRowValueAsString(tableRow);
            if (IsDistinct)
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
            bindings.Add(ProjectionVariable.VariableName, aggregatorValue.TrimEnd(Separator));

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion
    }
}