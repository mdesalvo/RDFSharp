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

using System;
using System.Collections.Generic;
using System.Globalization;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFMinAggregator represents a MIN aggregation function applied by a GroupBy modifier
    /// </summary>
    public sealed class RDFMinAggregator : RDFAggregator
    {
        #region Properties
        /// <summary>
        /// Flavor of the aggregator
        /// </summary>
        public RDFQueryEnums.RDFMinMaxAggregatorFlavors AggregatorFlavor { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a MIN aggregator on the given variable, with the given projection name and given flavor
        /// </summary>
        public RDFMinAggregator(RDFVariable aggrVariable, RDFVariable projVariable, RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor) : base(aggrVariable, projVariable)
            => AggregatorFlavor = aggregatorFlavor;

        /// <summary>
        /// Builds a MIN aggregator on the given expression, with the given projection name and given flavor. The
        /// expression is materialized into a synthetic column before partitioning, the aggregator then operating on it.
        /// </summary>
        public RDFMinAggregator(RDFExpression aggrExpression, RDFVariable projVariable, RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor) : base(MakeExpressionVariable(projVariable), projVariable)
        {
            AggregatorFlavor = aggregatorFlavor;
            Metadata.AggregatorExpression = aggrExpression ?? throw new RDFQueryException("Cannot create RDFMinAggregator because given \"aggrExpression\" parameter is null.");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// The MIN function (without the surrounding "(... AS ?proj)"), honoring DISTINCT.
        /// </summary>
        protected override string AggregatorFunction
            => Metadata.IsDistinct ? $"MIN(DISTINCT {AggregatorArgument})" : $"MIN({AggregatorArgument})";
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, RDFTableRow tableRow)
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
        private void ExecutePartitionNumeric(string partitionKey, RDFTableRow tableRow)
        {
            //Get row value
            double rowValue = GetRowValueAsNumber(tableRow);
            if (Metadata.IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (AggregatorContext.CheckPartitionKeyRowValueCache(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                AggregatorContext.UpdatePartitionKeyRowValueCache(partitionKey, rowValue);
            }
            //Get aggregator value
            double aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, double.PositiveInfinity);
            //In case of non-numeric values, consider partitioning failed
            double newAggregatorValue = double.NaN;
            if (!aggregatorValue.Equals(double.NaN) && !rowValue.Equals(double.NaN))
                newAggregatorValue = Math.Min(rowValue, aggregatorValue);
            //Update aggregator context (min)
            AggregatorContext.UpdatePartitionKeyExecutionResult(partitionKey, newAggregatorValue);
        }
        /// <summary>
        /// Executes the partition on the given tablerow (STRING)
        /// </summary>
        private void ExecutePartitionString(string partitionKey, RDFTableRow tableRow)
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
            string aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult<string>(partitionKey, null);
            //Update aggregator context (min)
            if (aggregatorValue == null)
                AggregatorContext.UpdatePartitionKeyExecutionResult(partitionKey, rowValue);
            else
                AggregatorContext.UpdatePartitionKeyExecutionResult(partitionKey, string.Compare(rowValue, aggregatorValue, false, CultureInfo.InvariantCulture) == -1 ? rowValue : aggregatorValue);
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
        private void UpdateProjectionTableNumeric(string partitionKey, RDFTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<string, string> bindings = GetProjectionBindings(partitionKey);

            //Add aggregator value to bindings
            double aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, double.PositiveInfinity);
            bindings.Add(Metadata.ProjectionVariable.VariableName,
                aggregatorValue.Equals(double.NaN)
                    ? string.Empty
                    : new RDFTypedLiteral(Convert.ToString(aggregatorValue, CultureInfo.InvariantCulture),RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString());

            //Add bindings to result's table
            projFuncTable.AddRow(bindings);
        }
        /// <summary>
        /// Helps in finalization step by updating the projection's result table (STRING)
        /// </summary>
        private void UpdateProjectionTableString(string partitionKey, RDFTable projFuncTable)
        {
            //Get bindings from context
            Dictionary<string, string> bindings = GetProjectionBindings(partitionKey);

            //Add aggregator value to bindings
            string aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, string.Empty);
            bindings.Add(Metadata.ProjectionVariable.VariableName, aggregatorValue);

            //Add bindings to result's table
            projFuncTable.AddRow(bindings);
        }
        #endregion
    }
}