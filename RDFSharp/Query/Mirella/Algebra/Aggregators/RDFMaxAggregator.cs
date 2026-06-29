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
using System.Globalization;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFMinAggregator represents a MAX aggregation function applied by a GroupBy modifier
    /// </summary>
    public sealed class RDFMaxAggregator : RDFAggregator
    {
        #region Properties
        /// <summary>
        /// Flavor of the aggregator
        /// </summary>
        public RDFQueryEnums.RDFMinMaxAggregatorFlavors AggregatorFlavor { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a MAX aggregator on the given variable, with the given projection name and given flavor
        /// </summary>
        public RDFMaxAggregator(RDFVariable aggrVariable, RDFVariable projVariable, RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor) : base(aggrVariable, projVariable)
            => AggregatorFlavor = aggregatorFlavor;

        /// <summary>
        /// Builds a MAX aggregator on the given expression, with the given projection name and given flavor. The
        /// expression is materialized into a synthetic column before partitioning, the aggregator then operating on it.
        /// </summary>
        public RDFMaxAggregator(RDFExpression aggrExpression, RDFVariable projVariable, RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor) : base(MakeExpressionVariable(projVariable), projVariable)
        {
            AggregatorFlavor = aggregatorFlavor;
            Metadata.AggregatorExpression = aggrExpression ?? throw new RDFQueryException("Cannot create RDFMaxAggregator because given \"aggrExpression\" parameter is null.");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// The MAX function (without the surrounding "(... AS ?proj)"), honoring DISTINCT.
        /// </summary>
        protected override string AggregatorFunction
            => Metadata.IsDistinct ? $"MAX(DISTINCT {AggregatorArgument})" : $"MAX({AggregatorArgument})";
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
            //Get row value (numeric typed literal, or null when unbound/non-numeric)
            RDFTypedLiteral rowValue = GetRowValueAsTypedLiteral(tableRow);
            if (Metadata.IsDistinct)
            {
                string distinctKey = rowValue?.ToString() ?? string.Empty;
                //Cache-Hit: distinctness failed
                if (Context.CheckPartitionKeyRowValueCache(partitionKey, distinctKey))
                    return;
                //Cache-Miss: distinctness passed
                Context.UpdatePartitionKeyRowValueCache(partitionKey, distinctKey);
            }
            //An already-poisoned partition stays poisoned
            if (Context.IsPartitionKeyPoisoned(partitionKey))
                return;
            //A non-numeric row poisons the partition
            if (rowValue == null)
            {
                Context.MarkPartitionKeyAsPoisoned(partitionKey);
                return;
            }
            //Keep the larger value as the winner, preserving its original typed literal (and thus its datatype);
            //a null current value means no numeric row has been seen yet
            RDFTypedLiteral aggregatorValue = Context.GetPartitionKeyExecutionResult<RDFTypedLiteral>(partitionKey, null);
            if (aggregatorValue == null || ParseTypedLiteralAsDouble(rowValue) > ParseTypedLiteralAsDouble(aggregatorValue))
                Context.UpdatePartitionKeyExecutionResult(partitionKey, rowValue);
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
                if (Context.CheckPartitionKeyRowValueCache(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                Context.UpdatePartitionKeyRowValueCache(partitionKey, rowValue);
            }
            //Get aggregator value
            string aggregatorValue = Context.GetPartitionKeyExecutionResult<string>(partitionKey, null);
            //Update aggregator context (max)
            if (aggregatorValue == null)
                Context.UpdatePartitionKeyExecutionResult(partitionKey, rowValue);
            else
                Context.UpdatePartitionKeyExecutionResult(partitionKey, string.Compare(rowValue, aggregatorValue, false, CultureInfo.InvariantCulture) == 1 ? rowValue : aggregatorValue);
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

            //Add aggregator value to bindings (no numeric row / poisoned partition projects an unbound value)
            RDFTypedLiteral aggregatorValue = Context.GetPartitionKeyExecutionResult<RDFTypedLiteral>(partitionKey, null);
            bindings.Add(Metadata.ProjectionVariable.VariableName,
                aggregatorValue == null || Context.IsPartitionKeyPoisoned(partitionKey)
                    ? string.Empty
                    : aggregatorValue.ToString());

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
            string aggregatorValue = Context.GetPartitionKeyExecutionResult(partitionKey, string.Empty);
            bindings.Add(Metadata.ProjectionVariable.VariableName, aggregatorValue);

            //Add bindings to result's table
            projFuncTable.AddRow(bindings);
        }
        #endregion
    }
}