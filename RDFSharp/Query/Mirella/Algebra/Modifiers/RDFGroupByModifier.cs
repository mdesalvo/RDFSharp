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

using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFGroupByModifier represents an aggregator modifier to be applied on a query results table.
    /// </summary>
    public sealed class RDFGroupByModifier : RDFModifier
    {
        #region Properties
        /// <summary>
        /// List of variables on which query results are grouped
        /// </summary>
        internal List<RDFVariable> PartitionVariables { get; set; }

        /// <summary>
        /// List of aggregators applied on the result groups
        /// </summary>
        internal List<RDFAggregator> Aggregators { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a GroupBy modifier on the given variables
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFGroupByModifier(List<RDFVariable> partitionVariables)
        {
            #region Guards
            if (partitionVariables == null || partitionVariables.Count == 0)
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"partitionVariables\" parameter is null or empty.");
            if (partitionVariables.Contains(null))
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"partitionVariables\" parameter contains null elements.");
            #endregion

            PartitionVariables = [];
            Aggregators = [];
            partitionVariables.ForEach(pv1 =>
            {
                if (!PartitionVariables.Any(pv2 => pv2.Equals(pv1)))
                {
                    PartitionVariables.Add(pv1);
                    IsEvaluable = true;

                    //At every partition variable must correspond a partition aggregator
                    Aggregators.Add(new RDFPartitionAggregator(pv1, pv1));
                }
            });
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier
        /// </summary>
        public override string ToString()
            => $"GROUP BY {string.Join(" ", PartitionVariables)}";
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given aggregator to the modifier
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFGroupByModifier AddAggregator(RDFAggregator aggregator)
        {
            if (aggregator != null)
            {
                //There cannot exist two aggregators projecting the same variable (exclude automatic partition aggregators from the check)
                if (Aggregators.Any(ag => !(ag is RDFPartitionAggregator) && ag.ProjectionVariable.Equals(aggregator.ProjectionVariable)))
                    throw new RDFQueryException($"Cannot add aggregator to GroupBy modifier because the given projection variable '{aggregator.ProjectionVariable}' is already used by another aggregator.");

                Aggregators.Add(aggregator);
            }
            return this;
        }

        /// <summary>
        /// Applies the modifier on the given datatable
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        internal override DataTable ApplyModifier(DataTable table)
        {
            //Perform consistency checks
            ConsistencyChecks(table);

            //Execute partition algorythm
            ExecutePartitionAlgorythm(table);

            //Execute projection algorythm
            DataTable resultTable = ExecuteProjectionAlgorythm();

            //Execute filter algorythm
            return ExecuteFilterAlgorythm(resultTable);
        }

        /// <summary>
        /// Performs consistency checks
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        private void ConsistencyChecks(DataTable table)
        {
            //Every partition variable must be found in the working table as a column
            List<string> unavailablePartitionVariables = [.. PartitionVariables.Where(pv => !table.Columns.Contains(pv.ToString())).Select(pv => pv.ToString())];
            if (unavailablePartitionVariables.Count > 0)
                throw new RDFQueryException($"Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: {string.Join(",", unavailablePartitionVariables.Distinct())}");

            //Every aggregator variable must be found in the working table as a column
            List<string> unavailableAggregatorVariables = [.. Aggregators.Where(ag => !table.Columns.Contains(ag.AggregatorVariable.ToString())).Select(ag => ag.AggregatorVariable.ToString())];
            if (unavailableAggregatorVariables.Count > 0)
                throw new RDFQueryException($"Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {string.Join(",", unavailableAggregatorVariables.Distinct())}");

            //There should NOT be intersection between partition variables (GroupBy) and projection variables (Aggregators)
            List<string> commonPartitionProjectionVariables = [.. PartitionVariables.Where(pv => Aggregators.Any(ag => !(ag is RDFPartitionAggregator) && pv.Equals(ag.ProjectionVariable))).Select(pav => pav.ToString())];
            if (commonPartitionProjectionVariables.Count > 0)
                throw new RDFQueryException($"Cannot apply GroupBy modifier because the following variables have been specified both for partitioning (in GroupBy) and projection (in Aggregator): {string.Join(",", commonPartitionProjectionVariables.Distinct())}");
        }

        /// <summary>
        /// Executes partition algorithm
        /// </summary>
        private void ExecutePartitionAlgorythm(DataTable table)
        {
            foreach (DataRow tableRow in table.Rows)
            {
                string partitionKey = GetPartitionKey(tableRow);
                Aggregators.ForEach(ag => ag.ExecutePartition(partitionKey, tableRow));
            }
        }

        /// <summary>
        /// Executes projection algorythm
        /// </summary>
        private DataTable ExecuteProjectionAlgorythm()
        {
            List<DataTable> projFuncTables = [];
            Aggregators.ForEach(ag => projFuncTables.Add(ag.ExecuteProjection(PartitionVariables)));
            projFuncTables.RemoveAll(pft => pft == null);

            return RDFQueryEngine.CombineTables(projFuncTables);
        }

        /// <summary>
        /// Execute filter algorythm
        /// </summary>
        private DataTable ExecuteFilterAlgorythm(DataTable resultTable)
        {
            if (Aggregators.Any(ag => ag.HavingClause.Item1))
            {
                DataTable filteredTable = resultTable.Clone();
                List<RDFComparisonExpression> havingExpressions = [.. Aggregators
                    .Where(ag => ag.HavingClause.Item1)
                    .Select(ag => new RDFComparisonExpression(
                        ag.HavingClause.Item2,
                        ag.ProjectionVariable,
                        ag.HavingClause.Item3 is RDFResource havingRes ? new RDFConstantExpression(havingRes)
                         : ag.HavingClause.Item3 is RDFLiteral havingLit ?  new RDFConstantExpression(havingLit)
                         : ag.HavingClause.Item3 is RDFVariable havingVar ? new RDFVariableExpression(havingVar)
                         : null as RDFExpression))];

                #region ExecuteFilters
                foreach (DataRow resultRow in resultTable.Rows)
                {
                    bool keepRow = true;
                    List<RDFComparisonExpression>.Enumerator comparisonsEnum = havingExpressions.GetEnumerator();
                    while (keepRow && comparisonsEnum.MoveNext())
                    {
                        RDFPatternMember comparisonResult = comparisonsEnum.Current?.ApplyExpression(resultRow);
                        if (!(comparisonResult?.Equals(RDFTypedLiteral.True) ?? false))
                            keepRow = false;
                    }

                    if (keepRow)
                    {
                        DataRow newRow = filteredTable.NewRow();
                        newRow.ItemArray = resultRow.ItemArray;
                        filteredTable.Rows.Add(newRow);
                    }
                }
                #endregion

                return filteredTable;
            }

            return resultTable;
        }

        /// <summary>
        /// Calculates the partition key on the given datarow
        /// </summary>
        private string GetPartitionKey(DataRow tableRow)
        {
            List<string> partitionKey = [];
            PartitionVariables.ForEach(pv =>
            {
                partitionKey.Add(tableRow.IsNull(pv.VariableName)
                    ? $"{pv.VariableName}§PV§{string.Empty}"
                    : $"{pv.VariableName}§PV§{tableRow[pv.VariableName]}");
            });
            return string.Join("§PK§", partitionKey);
        }
        #endregion
    }
}