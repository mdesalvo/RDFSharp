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
        public RDFGroupByModifier(List<RDFVariable> partitionVariables)
        {
            if (partitionVariables != null && partitionVariables.Any())
            {
                this.PartitionVariables = new List<RDFVariable>();
                this.Aggregators = new List<RDFAggregator>();
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
                    this.IsEvaluable = true;
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
            //Perform consistency checks
            ConsistencyChecks(table);

            //Execute partition algorythm
            foreach (DataRow tableRow in table.Rows)
            {
                String partitionKey = GetPartitionKey(tableRow);
                this.Aggregators.ForEach(ag =>
                    ag.ExecutePartition(partitionKey, tableRow));
            }                

            //Execute projection algorythm
            List<DataTable> projFuncTables = new List<DataTable>(); 
            this.Aggregators.ForEach(ag =>
                projFuncTables.Add(ag.ExecuteProjection(this.PartitionVariables)));

            //Produce result's table
            return RDFQueryEngine.CreateNew().CombineTables(projFuncTables, false);
        }

        /// <summary>
        /// Performs consistency checks on the modifier
        /// </summary>
        private void ConsistencyChecks(DataTable table)
        {
            //1 - Every partition variable must be found in the working table as a column
            if (!this.PartitionVariables.TrueForAll(pv => table.Columns.Contains(pv.ToString())))
            {
                String notfoundPartitionVars = String.Join(",", this.PartitionVariables.Where(pv => !table.Columns.Contains(pv.ToString()))
                                                                                       .Select(pv => pv.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: {0}", notfoundPartitionVars));
            }
            //2 - Every aggregation variable must be found in the working table as a column
            if (!this.Aggregators.TrueForAll(ag => table.Columns.Contains(ag.AggregatorVariable.ToString())))
            {
                //Use lookup hashset to ensure distinctness of result variables
                HashSet<String> notfoundAggregatorVarsLookup = new HashSet<String>();
                foreach (RDFAggregator notfoundAggregatorVar in this.Aggregators.Where(ag => !table.Columns.Contains(ag.AggregatorVariable.ToString())))
                {
                    if (!notfoundAggregatorVarsLookup.Contains(notfoundAggregatorVar.AggregatorVariable.ToString()))
                        notfoundAggregatorVarsLookup.Add(notfoundAggregatorVar.AggregatorVariable.ToString());
                }
                var notfoundAggregatorVars = String.Join(",", notfoundAggregatorVarsLookup);
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {0}", notfoundAggregatorVars));
            }
            //3 - There should NOT be intersection between partition variables and projection variables
            if (this.PartitionVariables.Any(pv => this.Aggregators.Any(ag => pv.Equals(ag.ProjectionVariable))))
            {
                String commonPartitionProjectionVars = String.Join(",", this.PartitionVariables.Where(pv => this.Aggregators.Any(ag => pv.Equals(ag.ProjectionVariable)))
                                                                                               .Select(pv => pv.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the following variables have been specified both for partitioning and projection: {0}", commonPartitionProjectionVars));
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
                    partitionKey.Add(pv.VariableName + "§PV§" + String.Empty);
                else
                    partitionKey.Add(pv.VariableName + "§PV§" + tableRow[pv.VariableName].ToString());
            });
            return String.Join("§PK§", partitionKey);
        }
        #endregion

    }
}