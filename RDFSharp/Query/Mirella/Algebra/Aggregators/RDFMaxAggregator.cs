/*
   Copyright 2012-2022 Marco De Salvo

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
    /// RDFMinAggregator represents a MAX aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFMaxAggregator : RDFAggregator
    {
        #region Properties
        /// <summary>
        /// Flavor of the aggregator
        /// </summary>
        public RDFQueryEnums.RDFMinMaxAggregatorFlavors AggregatorFlavor { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a MAX aggregator on the given variable, with the given projection name and given flavor
        /// </summary>
        public RDFMaxAggregator(RDFVariable aggrVariable, RDFVariable projVariable, RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor) : base(aggrVariable, projVariable)
            => AggregatorFlavor = aggregatorFlavor;
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the MAX aggregator
        /// </summary>
        public override string ToString()
            => IsDistinct ? string.Format("(MAX(DISTINCT {0}) AS {1})", AggregatorVariable, ProjectionVariable)
                               : string.Format("(MAX({0}) AS {1})", AggregatorVariable, ProjectionVariable);
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, DataRow tableRow)
        {
            switch (AggregatorFlavor)
            {
                case RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric:
                    ExecutePartitionNumeric(partitionKey, tableRow);
                    break;
                case RDFQueryEnums.RDFMinMaxAggregatorFlavors.String:
                    ExecutePartitionString(partitionKey, tableRow);
                    break;
            }
        }
        /// <summary>
        /// Executes the partition on the given tablerow (NUMERIC)
        /// </summary>
        private void ExecutePartitionNumeric(string partitionKey, DataRow tableRow)
        {
            //Get row value
            double rowValue = GetRowValueAsNumber(tableRow);
            if (IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (AggregatorContext.CheckPartitionKeyRowValueCache<double>(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                else
                    AggregatorContext.UpdatePartitionKeyRowValueCache<double>(partitionKey, rowValue);
            }
            //Get aggregator value
            double aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult<double>(partitionKey, double.NegativeInfinity);
            //In case of non-numeric values, consider partitioning failed
            double newAggregatorValue = double.NaN;
            if (!aggregatorValue.Equals(double.NaN) && !rowValue.Equals(double.NaN))
                newAggregatorValue = Math.Max(rowValue, aggregatorValue);
            //Update aggregator context (max)
            AggregatorContext.UpdatePartitionKeyExecutionResult<double>(partitionKey, newAggregatorValue);
        }
        /// <summary>
        /// Executes the partition on the given tablerow (STRING)
        /// </summary>
        private void ExecutePartitionString(string partitionKey, DataRow tableRow)
        {
            //Get row value
            string rowValue = GetRowValueAsString(tableRow);
            if (IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (AggregatorContext.CheckPartitionKeyRowValueCache<string>(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                else
                    AggregatorContext.UpdatePartitionKeyRowValueCache<string>(partitionKey, rowValue);
            }
            //Get aggregator value
            string aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult<string>(partitionKey, null);
            //Update aggregator context (max)
            if (aggregatorValue == null)
                AggregatorContext.UpdatePartitionKeyExecutionResult<string>(partitionKey, rowValue);
            else
                AggregatorContext.UpdatePartitionKeyExecutionResult<string>(partitionKey, string.Compare(rowValue, aggregatorValue, false, CultureInfo.InvariantCulture) == 1 ? rowValue : aggregatorValue);
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
            {
                //Update result's table
                UpdateProjectionTable(partitionKey, projFuncTable);
            }

            return projFuncTable;
        }

        /// <summary>
        /// Helps in finalization step by updating the projection's result table
        /// </summary>
        internal override void UpdateProjectionTable(string partitionKey, DataTable projFuncTable)
        {
            switch (AggregatorFlavor)
            {
                case RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric:
                    UpdateProjectionTableNumeric(partitionKey, projFuncTable);
                    break;
                case RDFQueryEnums.RDFMinMaxAggregatorFlavors.String:
                    UpdateProjectionTableString(partitionKey, projFuncTable);
                    break;
            }
        }
        /// <summary>
        /// Helps in finalization step by updating the projection's result table (NUMERIC)
        /// </summary>
        private void UpdateProjectionTableNumeric(string partitionKey, DataTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<string, string> bindings = new Dictionary<string, string>();
            foreach (string pkValue in partitionKey.Split(new string[] { "§PK§" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pValues = pkValue.Split(new string[] { "§PV§" }, StringSplitOptions.None);
                bindings.Add(pValues[0], pValues[1]);
            }

            //Add aggregator value to bindings
            double aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult<double>(partitionKey, double.NegativeInfinity);
            if (aggregatorValue.Equals(double.NaN))
                bindings.Add(ProjectionVariable.VariableName, string.Empty);
            else
                bindings.Add(ProjectionVariable.VariableName, new RDFTypedLiteral(Convert.ToString(aggregatorValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString());

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        /// <summary>
        /// Helps in finalization step by updating the projection's result table (STRING)
        /// </summary>
        private void UpdateProjectionTableString(string partitionKey, DataTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<string, string> bindings = new Dictionary<string, string>();
            foreach (string pkValue in partitionKey.Split(new string[] { "§PK§" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pValues = pkValue.Split(new string[] { "§PV§" }, StringSplitOptions.None);
                bindings.Add(pValues[0], pValues[1]);
            }

            //Add aggregator value to bindings
            string aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult<string>(partitionKey, string.Empty);
            bindings.Add(ProjectionVariable.VariableName, aggregatorValue);

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion
    }
}