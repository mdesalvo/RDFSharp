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
        internal RDFGroupByModifier(List<RDFVariable> partitionVariables)
        {
            if (partitionVariables != null && partitionVariables.Any())
            {
                partitionVariables.ForEach(pv1 => {
                    if (!this.PartitionVariables.Any(pv2 => pv2.Equals(pv1)))
                    {
                        this.PartitionVariables.Add(pv1);
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
        public override String ToString()
        {
            return String.Format("GROUP BY {0}", String.Join(" ", this.PartitionVariables));
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
            this.PartitionVariables.ForEach(pv => 
                RDFQueryEngine.AddColumn(result, pv.VariableName));
            this.Aggregators.ForEach(ag => 
                RDFQueryEngine.AddColumn(result, ag.ProjectionVariable.VariableName));
            result.AcceptChanges();

            //Initialize partition registry
            //We need to store for each partition key
            //the results of each projection variable
            var partitionRegistry = new Dictionary<String, Dictionary<String, Object>>();
            
            //Execute partition algorythm
            foreach (DataRow tableRow in table.Rows)
            {

                //Calculate partition key
                String partitionKey = GetPartitionKey(tableRow);

                //Update partition registry with partition key
                UpdatePartitionRegistry(partitionRegistry, partitionKey);

                //Execute aggregator functions on the current row 
                ExecuteAggregatorFunctions(partitionRegistry, partitionKey, tableRow);
                
            }

            //Finalize partition algorythm
            FinalizeAggregatorFunctions(partitionRegistry, table);
            
            return result;
        }

        /// <summary>
        /// Performs preliminary consistency checks on the configuration of the modifier.
        /// </summary>
        private void PreliminaryChecks(DataTable table)
        {
            //1 - Every grouping variable must be found in the working table as a column
            if (!this.PartitionVariables.TrueForAll(pv => table.Columns.Contains(pv.ToString())))
            {
                var notfoundPartitionVars = String.Join(",", this.PartitionVariables.Where(pv => !table.Columns.Contains(pv.ToString()))
                                                                                    .Select(pv => pv.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: {0}", notfoundPartitionVars));
            }
            //2 - Every aggregation variable must be found in the working table as a column
            if (!this.Aggregators.TrueForAll(ag => table.Columns.Contains(ag.AggregatorVariable.ToString())))
            {
                var notfoundAggregationVars = String.Join(",", this.Aggregators.Where(ag => !table.Columns.Contains(ag.AggregatorVariable.ToString()))
                                                                               .Select(ag => ag.AggregatorVariable.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {0}", notfoundAggregationVars));
            }
            //3 - There should NOT be intersection between grouping variables and projection variables
            if (this.PartitionVariables.Any(pv => this.Aggregators.Any(ag => pv.Equals(ag.ProjectionVariable))))
            {
                var commonPartitionProjectionVars = String.Join(",", this.PartitionVariables.Where(pv => this.Aggregators.Any(ag => pv.Equals(ag.ProjectionVariable)))
                                                                                            .Select(pv => pv.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the following variables have been specified both for partitioning and projection operations: {0}", commonPartitionProjectionVars));
            }
        }

        /// <summary>
        /// Calculates the partition key on the given datarow
        /// </summary>
        private String GetPartitionKey(DataRow tableRow)
        {
            List<String> partitionKey = new List<String>();
            this.PartitionVariables.ForEach(pv => {
                if (tableRow.IsNull(pv.VariableName))
                {
                    partitionKey.Add(String.Empty);
                }
                else
                {
                    partitionKey.Add(tableRow[pv.VariableName].ToString());
                }
            });
            return String.Join("§§", partitionKey);
        }
        
        /// <summary>
        /// Updates partition registry with the given partition key
        /// </summary>
        private void UpdatePartitionRegistry(Dictionary<String, Dictionary<String, Object>> partitionRegistry, String partitionKey)
        {
            if (!partitionRegistry.ContainsKey(partitionKey))
            {
                var partitionRegistryOfKey = new Dictionary<String, Object>();
                this.Aggregators.ForEach(ag =>
                {
                    partitionRegistryOfKey.Add(ag.ProjectionVariable.VariableName, null);
                });
                partitionRegistry.Add(partitionKey, partitionRegistryOfKey);
            }
        }
        
        /// <summary>
        /// Executes aggregator functions on the given datarow
        /// </summary>
        private void ExecuteAggregatorFunctions(Dictionary<String, Dictionary<String, Object>> partitionRegistry, String partitionKey, DataRow tableRow)
        {
            this.Aggregators.ForEach(ag => ag.ExecuteAggregatorFunction(partitionRegistry, partitionKey, tableRow));
        }

        /// <summary>
        /// Finalizes aggregator functions on the results table
        /// </summary>
        private void FinalizeAggregatorFunctions(Dictionary<String, Dictionary<String, Object>> partitionRegistry, DataTable workingTable)
        {
            this.Aggregators.ForEach(ag => ag.FinalizeAggregatorFunction(partitionRegistry, workingTable));
        }
        #endregion

    }
}