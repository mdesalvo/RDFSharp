﻿/*
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
using System.Globalization;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFAvgAggregator represents an AVG aggregation function applied by a GroupBy modifier
    /// </summary>
    public sealed class RDFAvgAggregator : RDFAggregator
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
            => IsDistinct ? $"(AVG(DISTINCT {AggregatorVariable}) AS {ProjectionVariable})"
                          : $"(AVG({AggregatorVariable}) AS {ProjectionVariable})";
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition on the given tablerow
        /// </summary>
        internal override void ExecutePartition(string partitionKey, DataRow tableRow)
        {
            //Get row value
            double rowValue = GetRowValueAsNumber(tableRow);
            if (IsDistinct)
            {
                //Cache-Hit: distinctness failed
                if (AggregatorContext.CheckPartitionKeyRowValueCache(partitionKey, rowValue))
                    return;
                //Cache-Miss: distinctness passed
                AggregatorContext.UpdatePartitionKeyRowValueCache(partitionKey, rowValue);
            }
            //Get aggregator value
            double aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, 0d);
            //In case of non-numeric values, consider partitioning failed
            double newAggregatorValue = double.NaN;
            if (!aggregatorValue.Equals(double.NaN) && !rowValue.Equals(double.NaN))
                newAggregatorValue = rowValue + aggregatorValue;
            //Update aggregator context (sum, count)
            AggregatorContext.UpdatePartitionKeyExecutionResult(partitionKey, newAggregatorValue);
            AggregatorContext.UpdatePartitionKeyExecutionCounter(partitionKey);
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
                //Get aggregator value
                double aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, 0d);
                //Get aggregator counter
                double aggregatorCounter = AggregatorContext.GetPartitionKeyExecutionCounter(partitionKey);
                //In case of non-numeric values, consider partition failed
                double finalAggregatorValue = double.NaN;
                if (!aggregatorValue.Equals(double.NaN))
                    finalAggregatorValue = aggregatorValue / aggregatorCounter;
                //Update aggregator context (sum, count)
                AggregatorContext.UpdatePartitionKeyExecutionResult(partitionKey, finalAggregatorValue);
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
            //Get bindings from context
            Dictionary<string, string> bindings = partitionKey.Split(new[] { "§PK§" }, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(pkValue => pkValue.Split(new[] { "§PV§" }, StringSplitOptions.None)).ToDictionary(pValues => pValues[0], pValues => pValues[1]);

            //Add aggregator value to bindings
            double aggregatorValue = AggregatorContext.GetPartitionKeyExecutionResult(partitionKey, 0d);
            bindings.Add(ProjectionVariable.VariableName,
                aggregatorValue.Equals(double.NaN)
                    ? string.Empty
                    : new RDFTypedLiteral(Convert.ToString(aggregatorValue, CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString());

            //Add bindings to result's table
            RDFQueryEngine.AddRow(projFuncTable, bindings);
        }
        #endregion
    }
}