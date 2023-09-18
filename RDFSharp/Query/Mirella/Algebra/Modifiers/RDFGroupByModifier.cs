﻿/*
   Copyright 2012-2023 Marco De Salvo

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

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFGroupByModifier represents an aggregator modifier to be applied on a query results table.
    /// </summary>
    public class RDFGroupByModifier : RDFModifier
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
        /// Default-ctor to build a GroupBy modifier on the given variables
        /// </summary>
        public RDFGroupByModifier(List<RDFVariable> partitionVariables)
        {
            #region Guards
            if (partitionVariables == null || partitionVariables.Count == 0)
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"partitionVariables\" parameter is null or empty.");
            if (partitionVariables.Any(pv => pv == null))
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"partitionVariables\" parameter contains null elements.");
            #endregion

            PartitionVariables = new List<RDFVariable>();
            Aggregators = new List<RDFAggregator>();
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
            => string.Format("GROUP BY {0}", string.Join(" ", PartitionVariables));
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given aggregator to the modifier
        /// </summary>
        public RDFGroupByModifier AddAggregator(RDFAggregator aggregator)
        {
            if (aggregator != null)
            {
                //There cannot exist two aggregators projecting the same variable (exclude automatic partition aggregators from the check)
                if (Aggregators.Any(ag => (!(ag is RDFPartitionAggregator)) && ag.ProjectionVariable.Equals(aggregator.ProjectionVariable)))
                    throw new RDFQueryException(string.Format("Cannot add aggregator to GroupBy modifier because the given projection variable '{0}' is already used by another aggregator.", aggregator.ProjectionVariable));

                Aggregators.Add(aggregator);
            }
            return this;
        }

        /// <summary>
        /// Applies the modifier on the given datatable
        /// </summary>
        internal override DataTable ApplyModifier(DataTable table)
        {
            //Perform consistency checks
            ConsistencyChecks(table);

            //Execute partition algorythm
            ExecutePartitionAlgorythm(table);

            //Execute projection algorythm
            DataTable resultTable = ExecuteProjectionAlgorythm();

            //Execute filter algorythm
            DataTable filteredTable = ExecuteFilterAlgorythm(resultTable);
            return filteredTable;
        }

        /// <summary>
        /// Performs consistency checks
        /// </summary>
        private void ConsistencyChecks(DataTable table)
        {
            //Every partition variable must be found in the working table as a column
            IEnumerable<string> unavailablePartitionVariables = PartitionVariables.Where(pv => !table.Columns.Contains(pv.ToString()))
                                                                                  .Select(pv => pv.ToString());
            if (unavailablePartitionVariables.Any())
                throw new RDFQueryException(string.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: {0}", string.Join(",", unavailablePartitionVariables.Distinct())));
            
            //Every aggregator variable must be found in the working table as a column
            IEnumerable<string> unavailableAggregatorVariables = Aggregators.Where(ag => !table.Columns.Contains(ag.AggregatorVariable.ToString()))
                                                                            .Select(ag => ag.AggregatorVariable.ToString());
            if (unavailableAggregatorVariables.Any())
                throw new RDFQueryException(string.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {0}", string.Join(",", unavailableAggregatorVariables.Distinct())));
            
            //There should NOT be intersection between partition variables (GroupBy) and projection variables (Aggregators)
            IEnumerable<string> commonPartitionProjectionVariables = PartitionVariables.Where(pv => Aggregators.Any(ag => (!(ag is RDFPartitionAggregator)) && pv.Equals(ag.ProjectionVariable)))
                                                                                       .Select(pav => pav.ToString());
            if (commonPartitionProjectionVariables.Any())
                throw new RDFQueryException(string.Format("Cannot apply GroupBy modifier because the following variables have been specified both for partitioning (in GroupBy) and projection (in Aggregator): {0}", string.Join(",", commonPartitionProjectionVariables.Distinct())));
        }

        /// <summary>
        /// Executes partition algorythm
        /// </summary>
        private void ExecutePartitionAlgorythm(DataTable table)
        {
            foreach (DataRow tableRow in table.Rows)
            {
                string partitionKey = GetPartitionKey(tableRow);
                Aggregators.ForEach(ag =>
                    ag.ExecutePartition(partitionKey, tableRow));
            }
        }

        /// <summary>
        /// Executes projection algorythm
        /// </summary>
        private DataTable ExecuteProjectionAlgorythm()
        {
            List<DataTable> projFuncTables = new List<DataTable>();
            Aggregators.ForEach(ag =>
                projFuncTables.Add(ag.ExecuteProjection(PartitionVariables)));
            projFuncTables.RemoveAll(pft => pft == null);

            DataTable resultTable = RDFQueryEngine.CombineTables(projFuncTables, false);
            return resultTable;
        }

        /// <summary>
        /// Execute filter algorythm
        /// </summary>
        private DataTable ExecuteFilterAlgorythm(DataTable resultTable)
        {
            if (Aggregators.Any(ag => ag.HavingClause.Item1))
            {
                DataTable filteredTable = resultTable.Clone();
                IEnumerator rowsEnum = resultTable.Rows.GetEnumerator();
                IEnumerable<RDFComparisonFilter> havingFilters = Aggregators.Where(ag => ag.HavingClause.Item1)
                                                                            .Select(ag => new RDFComparisonFilter(ag.HavingClause.Item2, ag.ProjectionVariable, ag.HavingClause.Item3));
                #region ExecuteFilters
                bool keepRow = false;
                while (rowsEnum.MoveNext())
                {
                    keepRow = true;
                    IEnumerator<RDFComparisonFilter> filtersEnum = havingFilters.GetEnumerator();
                    while (keepRow && filtersEnum.MoveNext())
                        keepRow = filtersEnum.Current.ApplyFilter((DataRow)rowsEnum.Current, false);

                    if (keepRow)
                    {
                        DataRow newRow = filteredTable.NewRow();
                        newRow.ItemArray = ((DataRow)rowsEnum.Current).ItemArray;
                        filteredTable.Rows.Add(newRow);
                    }
                }
                #endregion

                return filteredTable;
            }
            else
            {
                return resultTable;
            }
        }

        /// <summary>
        /// Calculates the partition key on the given datarow
        /// </summary>
        private string GetPartitionKey(DataRow tableRow)
        {
            List<string> partitionKey = new List<string>();
            PartitionVariables.ForEach(pv =>
            {
                if (tableRow.IsNull(pv.VariableName))
                    partitionKey.Add(string.Concat(pv.VariableName, "§PV§", string.Empty));
                else
                    partitionKey.Add(string.Concat(pv.VariableName, "§PV§", tableRow[pv.VariableName].ToString()));
            });
            return string.Join("§PK§", partitionKey);
        }
        #endregion
    }
}