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

using RDFSharp.Model;
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
        {
            if (string.IsNullOrEmpty(separator))
                this.Separator = " ";
            else
                this.Separator = separator;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the GROUP_CONCAT aggregator
        /// </summary>
        public override string ToString()
            => this.IsDistinct ? string.Format("(GROUP_CONCAT(DISTINCT {0}; SEPARATOR=\"{1}\") AS {2})", this.AggregatorVariable, this.Separator, this.ProjectionVariable)
                               : string.Format("(GROUP_CONCAT({0}; SEPARATOR=\"{1}\") AS {2})", this.AggregatorVariable, this.Separator, this.ProjectionVariable);
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
            //Update aggregator context (group_concat)
            this.AggregatorContext.UpdatePartitionKeyExecutionResult<string>(partitionKey, string.Concat(aggregatorValue, rowValue, this.Separator));
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
            bindings.Add(this.ProjectionVariable.VariableName, aggregatorValue.TrimEnd(this.Separator));

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion

    }

}