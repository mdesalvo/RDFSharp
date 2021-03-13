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

using System;
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
            if (partitionVariables != null && partitionVariables.Any())
            {
                this.PartitionVariables = new List<RDFVariable>();
                this.Aggregators = new List<RDFAggregator>();
                partitionVariables.ForEach(pv1 =>
                {
                    if (!this.PartitionVariables.Any(pv2 => pv2.Equals(pv1)))
                    {
                        this.PartitionVariables.Add(pv1);
                        this.IsEvaluable = true;

                        //At every partition variable must correspond a partition aggregator
                        this.Aggregators.Add(new RDFPartitionAggregator(pv1, pv1));
                    }
                });
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"partitionVariables\" parameter is null or empty.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier
        /// </summary>
        public override string ToString()
            => string.Format("GROUP BY {0}", string.Join(" ", this.PartitionVariables));
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given aggregator to the modifier
        /// </summary>
        public RDFGroupByModifier AddAggregator(RDFAggregator aggregator)
        {
            if (aggregator != null)
            {
                //There cannot exist two aggregators projecting the same variable
                if (!this.Aggregators.Any(af => af.ProjectionVariable.Equals(aggregator.ProjectionVariable)))
                {
                    this.Aggregators.Add(aggregator);
                }
                else
                {
                    throw new RDFQueryException(string.Format("Cannot add aggregator to GroupBy modifier because the given projection variable '{0}' is already used by another aggregator.", aggregator.ProjectionVariable));
                }
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
            //1 - Every partition variable must be found in the working table as a column
            if (!this.PartitionVariables.TrueForAll(pv => table.Columns.Contains(pv.ToString())))
            {
                string notfoundPartitionVars = string.Join(",", this.PartitionVariables.Where(pv => !table.Columns.Contains(pv.ToString()))
                                                                                       .Select(pv => pv.ToString()));
                throw new RDFQueryException(string.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: {0}", notfoundPartitionVars));
            }
            //2 - Every aggregation variable must be found in the working table as a column
            if (!this.Aggregators.TrueForAll(ag => table.Columns.Contains(ag.AggregatorVariable.ToString())))
            {
                //Use lookup hashset to ensure distinctness of result variables
                HashSet<string> notfoundAggregatorVarsLookup = new HashSet<string>();
                foreach (RDFAggregator notfoundAggregatorVar in this.Aggregators.Where(ag => !table.Columns.Contains(ag.AggregatorVariable.ToString())))
                {
                    if (!notfoundAggregatorVarsLookup.Contains(notfoundAggregatorVar.AggregatorVariable.ToString()))
                        notfoundAggregatorVarsLookup.Add(notfoundAggregatorVar.AggregatorVariable.ToString());
                }
                var notfoundAggregatorVars = string.Join(",", notfoundAggregatorVarsLookup);
                throw new RDFQueryException(string.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {0}", notfoundAggregatorVars));
            }
            //3 - There should NOT be intersection between partition variables and projection variables
            if (this.PartitionVariables.Any(pv => this.Aggregators.Any(ag => (!(ag is RDFPartitionAggregator)) && pv.Equals(ag.ProjectionVariable))))
            {
                string commonPartitionProjectionVars = string.Join(",", this.PartitionVariables.Where(pv => this.Aggregators.Any(ag => pv.Equals(ag.ProjectionVariable)))
                                                                                               .Select(pv => pv.ToString()));
                throw new RDFQueryException(string.Format("Cannot apply GroupBy modifier because the following variables have been specified both for partitioning and projection: {0}", commonPartitionProjectionVars));
            }
        }

        /// <summary>
        /// Executes partition algorythm
        /// </summary>
        private void ExecutePartitionAlgorythm(DataTable table)
        {
            foreach (DataRow tableRow in table.Rows)
            {
                string partitionKey = GetPartitionKey(tableRow);
                this.Aggregators.ForEach(ag =>
                    ag.ExecutePartition(partitionKey, tableRow));
            }
        }

        /// <summary>
        /// Executes projection algorythm
        /// </summary>
        private DataTable ExecuteProjectionAlgorythm()
        {
            List<DataTable> projFuncTables = new List<DataTable>();
            this.Aggregators.ForEach(ag =>
                projFuncTables.Add(ag.ExecuteProjection(this.PartitionVariables)));

            DataTable resultTable = new RDFQueryEngine().CombineTables(projFuncTables, false);
            return resultTable;
        }

        /// <summary>
        /// Execute filter algorythm
        /// </summary>
        private DataTable ExecuteFilterAlgorythm(DataTable resultTable)
        {
            if (this.Aggregators.Any(ag => ag.HavingClause.Item1))
            {
                DataTable filteredTable = resultTable.Clone();
                IEnumerator rowsEnum = resultTable.Rows.GetEnumerator();
                IEnumerable<RDFComparisonFilter> havingFilters = this.Aggregators.Where(ag => ag.HavingClause.Item1)
                                                                                 .Select(ag => new RDFComparisonFilter(ag.HavingClause.Item2,
                                                                                                                       ag.ProjectionVariable,
                                                                                                                       ag.HavingClause.Item3));
                #region ExecuteFilters
                bool keepRow = false;
                while (rowsEnum.MoveNext())
                {

                    keepRow = true;
                    var filtersEnum = havingFilters.GetEnumerator();
                    while (keepRow && filtersEnum.MoveNext())
                    {
                        keepRow = filtersEnum.Current.ApplyFilter((DataRow)rowsEnum.Current, false);
                    }

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
            this.PartitionVariables.ForEach(pv =>
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