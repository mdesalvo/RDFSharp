/*
   Copyright 2012-2019 Marco De Salvo

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
using System.Text;
using System.Linq;
using System.Data;

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
        internal List<RDFVariable> GroupByVariables { get; set; }

        /// <summary>
        /// List of aggregators applied on the result groups
        /// </summary>
        internal List<RDFAggregator> Aggregators { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a GroupBy modifier on the given variables
        /// </summary>
        internal RDFGroupByModifier(List<RDFVariable> groupByVariables)
        {
            if (groupByVariables != null && groupByVariables.Any())
            {
                groupByVariables.ForEach(gv1 => {
                    if (!this.GroupByVariables.Any(gv2 => gv2.Equals(gv1)))
                    {
                        this.GroupByVariables.Add(gv1);
                    }
                });
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"groupByVariables\" parameter is null or empty.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier
        /// </summary>
        public override String ToString()
        {
            return String.Format("GROUP BY {0}", String.Join(" ", this.GroupByVariables));
        }
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
                    throw new RDFQueryException(String.Format("Cannot add aggregator to GroupBy modifier because the given projection variable '{0}' is already used by another aggregator.", aggregator.ProjectionVariable));
                }
            }
            return this;
        }

        /// <summary>
        /// Applies the modifier on the given datatable 
        /// </summary>
        internal override DataTable ApplyModifier(DataTable table)
        {
            DataTable result = new DataTable();

            //Perform preliminary consistency checks
            PreliminaryChecks(table);

            //Initialize result table
            this.GroupByVariables.ForEach(gv => 
                RDFQueryEngine.AddColumn(result, gv.VariableName));
            this.Aggregators.ForEach(af => 
                RDFQueryEngine.AddColumn(result, af.ProjectionVariable.VariableName));
            result.AcceptChanges();

            //Initialize grouping algorythm registry
            var groupingRegistry = new Dictionary<String, DataTable>();
            
            //Start executing grouping algorythm
            result.BeginLoadData();
            foreach (DataRow tableRow in table.Rows)
            {

                //Calculate grouping key
                String groupingKey = GetGroupingKey(tableRow);

                //Update grouping registry with grouping key
                UpdateGroupingRegistry(groupingRegistry, groupingKey);

                //Execute aggregator functions on the current row 
                ExecuteAggregatorFunctions(groupingRegistry, groupingKey, tableRow);
                
            }
            result.EndLoadData();

            return result;
        }

        /// <summary>
        /// Performs preliminary consistency checks on the configuration of the modifier.
        /// Throws error when a validation condition is violated.
        /// </summary>
        private void PreliminaryChecks(DataTable table)
        {
            //1 - Every grouping variable must be found in the working table as a column
            if (!this.GroupByVariables.TrueForAll(gv => table.Columns.Contains(gv.ToString())))
            {
                var notfoundGroupingVars = String.Join(",", this.GroupByVariables.Where(gv => !table.Columns.Contains(gv.ToString()))
                                                                                 .Select(gv => gv.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for grouping: {0}", notfoundGroupingVars));
            }
            //2 - Every aggregation variable must be found in the working table as a column
            if (!this.Aggregators.TrueForAll(ag => table.Columns.Contains(ag.AggregatorVariable.ToString())))
            {
                var notfoundAggregationVars = String.Join(",", this.Aggregators.Where(ag => !table.Columns.Contains(ag.AggregatorVariable.ToString()))
                                                                                       .Select(ag => ag.AggregatorVariable.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {0}", notfoundAggregationVars));
            }
            //3 - There should NOT be intersection between grouping variables and projection variables
            if (this.GroupByVariables.Any(gv => this.Aggregators.Any(ag => gv.Equals(ag.ProjectionVariable))))
            {
                var commonGroupingProjectionVars = String.Join(",", this.GroupByVariables.Where(gv => this.Aggregators.Any(ag => gv.Equals(ag.ProjectionVariable)))
                                                                                         .Select(gv => gv.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the following variables have been specified both for grouping and projection operations: {0}", commonGroupingProjectionVars));
            }
        }

        /// <summary>
        /// Calculates the grouping key on the given datarow
        /// </summary>
        private String GetGroupingKey(DataRow tableRow)
        {
            List<String> groupingKeyList = new List<String>();
            this.GroupByVariables.ForEach(gv => {
                if (tableRow.IsNull(gv.VariableName))
                {
                    groupingKeyList.Add(String.Empty);
                }
                else
                {
                    groupingKeyList.Add(tableRow[gv.VariableName].ToString());
                }
            });
            String groupingKey = String.Join("§§", groupingKeyList);
            return groupingKey;
        }
        
        /// <summary>
        /// Updates grouping registry with the given grouping key
        /// </summary>
        private void UpdateGroupingRegistry(Dictionary<String, DataTable> groupingRegistry, String groupingKey)
        {
            if (!groupingRegistry.ContainsKey(groupingKey))
            {
                DataTable newGroupingTable = new DataTable();
                this.Aggregators.ForEach(ag => 
                    RDFQueryEngine.AddColumn(newGroupingTable, ag.ProjectionVariable.VariableName));
                newGroupingTable.AcceptChanges();
                groupingRegistry.Add(groupingKey, newGroupingTable);
            }
        }
        
        /// <summary>
        /// Executes aggregator functions on the given datarow
        /// </summary>
        private void ExecuteAggregatorFunctions(Dictionary<String, DataTable> groupingRegistry, String groupingKey, DataRow tableRow)
        {
            this.Aggregators.ForEach(ag => ag.ExecuteAggregatorFunction(groupingRegistry, groupingKey, tableRow));
        }
        #endregion

    }
}