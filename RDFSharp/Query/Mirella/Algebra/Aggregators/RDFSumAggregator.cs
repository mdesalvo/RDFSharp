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
    /// RDFSumAggregator represents a SUM aggregation function applied by a GroupBy modifier
    /// </summary>
    public sealed class RDFSumAggregator : RDFAggregator
    {
        #region Ctors
        /// <summary>
        /// Builds a SUM aggregator on the given variable and with the given projection name
        /// </summary>
        public RDFSumAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }

        /// <summary>
        /// Builds a SUM aggregator on the given expression and with the given projection name. The expression is
        /// materialized into a synthetic column before partitioning, the aggregator then operating on it.
        /// </summary>
        public RDFSumAggregator(RDFExpression aggrExpression, RDFVariable projVariable) : base(MakeExpressionVariable(projVariable), projVariable)
            => Metadata.AggregatorExpression = aggrExpression ?? throw new RDFQueryException("Cannot create RDFSumAggregator because given \"aggrExpression\" parameter is null.");
        #endregion

        #region Interfaces
        /// <summary>
        /// The SUM function (without the surrounding "(... AS ?proj)"), honoring DISTINCT.
        /// </summary>
        protected override string AggregatorFunction
            => Metadata.IsDistinct ? $"SUM(DISTINCT {AggregatorArgument})" : $"SUM({AggregatorArgument})";
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, RDFTableRow tableRow)
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
            //Fold the addition via the shared promotion-aware primitive (running sum starts at integer 0); an overflow
            //(null) poisons the partition
            RDFTypedLiteral aggregatorValue = Context.GetPartitionKeyExecutionResult(partitionKey, RDFTypedLiteral.Zero);
            RDFTypedLiteral newAggregatorValue = RDFModelUtilities.ComputeNumericArithmetic(aggregatorValue, rowValue, '+');
            if (newAggregatorValue == null)
                Context.MarkPartitionKeyAsPoisoned(partitionKey);
            else
                Context.UpdatePartitionKeyExecutionResult(partitionKey, newAggregatorValue);
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

            //Add aggregator value to bindings (a poisoned partition projects an unbound value)
            bindings.Add(Metadata.ProjectionVariable.VariableName,
                Context.IsPartitionKeyPoisoned(partitionKey)
                    ? string.Empty
                    : Context.GetPartitionKeyExecutionResult(partitionKey, RDFTypedLiteral.Zero).ToString());

            //Add bindings to result's table
            projFuncTable.AddRow(bindings);
        }
        #endregion
    }
}