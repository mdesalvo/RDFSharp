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
using System.Globalization;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFAvgAggregator represents an AVG aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFAvgAggregator : RDFAggregator
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
        public override string ToString()
            => this.IsDistinct ? string.Format("(AVG(DISTINCT {0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable)
                               : string.Format("(AVG({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable);
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, DataRow tableRow)
        {
            //Get row value
            double rowValue = GetRowValueAsNumber(tableRow);
            if (this.IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (this.AggregatorContext.CheckPartitionKeyRowValueCache<double>(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                else
                    this.AggregatorContext.UpdatePartitionKeyRowValueCache<double>(partitionKey, rowValue);
            }
            //Get aggregator value
            double aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<double>(partitionKey, 0d);
            //In case of non-numeric values, consider partitioning failed
            double newAggregatorValue = double.NaN;
            if (!aggregatorValue.Equals(double.NaN) && !rowValue.Equals(double.NaN))
                newAggregatorValue = rowValue + aggregatorValue;
            //Update aggregator context (sum, count)
            this.AggregatorContext.UpdatePartitionKeyExecutionResult<double>(partitionKey, newAggregatorValue);
            this.AggregatorContext.UpdatePartitionKeyExecutionCounter(partitionKey);
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
                //Get aggregator value
                double aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<double>(partitionKey, 0d);
                //Get aggregator counter
                double aggregatorCounter = this.AggregatorContext.GetPartitionKeyExecutionCounter(partitionKey);
                //In case of non-numeric values, consider partition failed
                double finalAggregatorValue = double.NaN;
                if (!aggregatorValue.Equals(double.NaN))
                    finalAggregatorValue = aggregatorValue / aggregatorCounter;
                //Update aggregator context (sum, count)
                this.AggregatorContext.UpdatePartitionKeyExecutionResult<double>(partitionKey, finalAggregatorValue);
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
            double aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<double>(partitionKey, 0d);
            if (aggregatorValue.Equals(double.NaN))
                bindings.Add(this.ProjectionVariable.VariableName, string.Empty);
            else
                bindings.Add(this.ProjectionVariable.VariableName, new RDFTypedLiteral(Convert.ToString(aggregatorValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString());

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion

    }

}