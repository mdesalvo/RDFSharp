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
    /// RDFCountAggregator represents a COUNT aggregation function applied by a GroupBy modifier
    /// </summary>
    public sealed class RDFCountAggregator : RDFAggregator
    {
        #region Properties
        /// <summary>
        /// Whether this is a COUNT(*): it counts the solutions of the group (each row) instead of the bound values
        /// of a single variable, so it reads no column from the working table.
        /// </summary>
        public bool IsCountAll { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a COUNT aggregator on the given variable and with the given projection name
        /// </summary>
        public RDFCountAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }

        /// <summary>
        /// Builds a COUNT aggregator on the given expression and with the given projection name. The expression is
        /// materialized into a synthetic column before partitioning, the aggregator then operating on it.
        /// </summary>
        public RDFCountAggregator(RDFExpression aggrExpression, RDFVariable projVariable) : base(MakeExpressionVariable(projVariable), projVariable)
            => Metadata.AggregatorExpression = aggrExpression ?? throw new RDFQueryException("Cannot create RDFCountAggregator because given \"aggrExpression\" parameter is null.");

        /// <summary>
        /// Builds a COUNT(*) aggregator with the given projection name: it counts the group's solutions (rows). The
        /// aggregator variable is set to the projection variable as a harmless placeholder (it is never read).
        /// </summary>
        public RDFCountAggregator(RDFVariable projVariable) : base(projVariable, projVariable)
            => IsCountAll = true;
        #endregion

        #region Interfaces
        /// <summary>
        /// The COUNT function (without the surrounding "(... AS ?proj)"), honoring COUNT(*) and DISTINCT.
        /// </summary>
        protected override string AggregatorFunction
        {
            get
            {
                if (IsCountAll)
                    return Metadata.IsDistinct ? "COUNT(DISTINCT *)" : "COUNT(*)";
                return Metadata.IsDistinct ? $"COUNT(DISTINCT {AggregatorArgument})" : $"COUNT({AggregatorArgument})";
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, RDFTableRow tableRow)
        {
            //COUNT(*): count the group's solutions (each row), honoring DISTINCT over the whole solution
            if (IsCountAll)
            {
                if (Metadata.IsDistinct)
                {
                    string rowSignature = tableRow.Signature;
                    //Cache-Hit: distinctness failed
                    if (Context.CheckPartitionKeyRowValueCache(partitionKey, rowSignature))
                        return;
                    //Cache-Miss: distinctness passed
                    Context.UpdatePartitionKeyRowValueCache(partitionKey, rowSignature);
                }
                Context.UpdatePartitionKeyExecutionResult(partitionKey, Context.GetPartitionKeyExecutionResult(partitionKey, 0d) + 1d);
                return;
            }

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
            //Update aggregator context (count)
            if (!string.IsNullOrEmpty(rowValue))
                Context.UpdatePartitionKeyExecutionResult(partitionKey, Context.GetPartitionKeyExecutionResult(partitionKey, 0d) + 1d);
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
            double aggregatorValue = Context.GetPartitionKeyExecutionResult(partitionKey, 0d);
            bindings.Add(Metadata.ProjectionVariable.VariableName, new RDFTypedLiteral(Convert.ToString(aggregatorValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString());

            //Add bindings to result's table
            projFuncTable.AddRow(bindings);
        }
        #endregion
    }
}