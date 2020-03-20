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
        {
            this.AggregatorFlavor = aggregatorFlavor;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the MAX aggregator
        /// </summary>
        public override String ToString()
        {
            return (this.IsDistinct ? String.Format("(MAX(DISTINCT {0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable)
                                    : String.Format("(MAX({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(String partitionKey, DataRow tableRow)
        {
            switch (this.AggregatorFlavor)
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
        private void ExecutePartitionNumeric(String partitionKey, DataRow tableRow)
        {
            //Get row value
            Double rowValue = GetRowValueAsNumber(tableRow);
            if (this.IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (this.AggregatorContext.CheckPartitionKeyRowValueCache<Double>(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                else
                    this.AggregatorContext.UpdatePartitionKeyRowValueCache<Double>(partitionKey, rowValue);
            }
            //Get aggregator value
            Double aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<Double>(partitionKey, Double.NegativeInfinity);
            //In case of non-numeric values, consider partitioning failed
            Double newAggregatorValue = Double.NaN;
            if (!aggregatorValue.Equals(Double.NaN) && !rowValue.Equals(Double.NaN))
                newAggregatorValue = Math.Max(rowValue, aggregatorValue);
            //Update aggregator context (max)
            this.AggregatorContext.UpdatePartitionKeyExecutionResult<Double>(partitionKey, newAggregatorValue);
        }
        /// <summary>
        /// Executes the partition on the given tablerow (STRING)
        /// </summary>
        private void ExecutePartitionString(String partitionKey, DataRow tableRow)
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
            String aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<String>(partitionKey, null);
            //Update aggregator context (max)
            if (aggregatorValue == null)
                this.AggregatorContext.UpdatePartitionKeyExecutionResult<String>(partitionKey, rowValue);
            else
                this.AggregatorContext.UpdatePartitionKeyExecutionResult<String>(partitionKey, String.Compare(rowValue, aggregatorValue, false, CultureInfo.InvariantCulture) == 1 ? rowValue : aggregatorValue);
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
            switch (this.AggregatorFlavor)
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
        private void UpdateProjectionTableNumeric(String partitionKey, DataTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<String, String> bindings = new Dictionary<String, String>();
            foreach (String pkValue in partitionKey.Split(new String[] { "§PK§" }, StringSplitOptions.RemoveEmptyEntries))
            {
                String[] pValues = pkValue.Split(new String[] { "§PV§" }, StringSplitOptions.None);
                bindings.Add(pValues[0], pValues[1]);
            }

            //Add aggregator value to bindings
            Double aggregatorValue = this.AggregatorContext.GetPartitionKeyExecutionResult<Double>(partitionKey, Double.NegativeInfinity);
            if (aggregatorValue.Equals(Double.NaN))
                bindings.Add(this.ProjectionVariable.VariableName, String.Empty);
            else
                bindings.Add(this.ProjectionVariable.VariableName, new RDFTypedLiteral(Convert.ToString(aggregatorValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString());

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        /// <summary>
        /// Helps in finalization step by updating the projection's result table (STRING)
        /// </summary>
        private void UpdateProjectionTableString(String partitionKey, DataTable projFuncTable)
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