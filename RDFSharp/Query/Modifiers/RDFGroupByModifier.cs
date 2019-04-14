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
using RDFSharp.Model;

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
        /// List of functions applied on the result groups
        /// </summary>
        internal List<RDFAggregatorFunction> AggregatorFunctions { get; set; }
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
            var result = new StringBuilder();
            result.Append(String.Format("GROUP BY {0}", String.Join(" ", this.GroupByVariables)));
            if (this.AggregatorFunctions.Any())
            {
                result.Append("??");
                result.Append(String.Join(" ", this.AggregatorFunctions.Select(v => v.ToString())));
            }            
            return result.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given aggregator function to the modifier
        /// </summary>
        public RDFGroupByModifier AddAggregatorFunction(RDFAggregatorFunction aggregatorFunction)
        {
            if (aggregatorFunction != null)
            {
                //There cannot exist two aggregator functions projecting the same variable
                if (!this.AggregatorFunctions.Any(af => af.ProjectionVariable.Equals(aggregatorFunction.ProjectionVariable)))
                {
                    this.AggregatorFunctions.Add(aggregatorFunction);
                }
                else
                {
                    throw new RDFQueryException(String.Format("Cannot add aggregator function to GroupBy modifier because the given projection variable '{0}' is already used by another aggregator function.", aggregatorFunction.ProjectionVariable));
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
            this.AggregatorFunctions.ForEach(af => 
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

                //Update grouping registry
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
            if (!this.AggregatorFunctions.TrueForAll(af => table.Columns.Contains(af.AggregatorVariable.ToString())))
            {
                var notfoundAggregationVars = String.Join(",", this.AggregatorFunctions.Where(af => !table.Columns.Contains(af.AggregatorVariable.ToString()))
                                                                                       .Select(af => af.AggregatorVariable.ToString()));
                throw new RDFQueryException(String.Format("Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {0}", notfoundAggregationVars));
            }
            //3 - There should NOT be intersection between grouping variables and projection variables
            if (this.GroupByVariables.Any(gv => this.AggregatorFunctions.Any(af => gv.Equals(af.ProjectionVariable))))
            {
                var commonGroupingProjectionVars = String.Join(",", this.GroupByVariables.Where(gv => this.AggregatorFunctions.Any(af => gv.Equals(af.ProjectionVariable)))
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
                this.AggregatorFunctions.ForEach(af => {
                    RDFQueryEngine.AddColumn(newGroupingTable, af.ProjectionVariable.VariableName);
                });
                newGroupingTable.AcceptChanges();
                groupingRegistry.Add(groupingKey, newGroupingTable);
            }
        }
        
        /// <summary>
        /// Executes aggregator functions on the given datarow
        /// </summary>
        private void ExecuteAggregatorFunctions(Dictionary<String, DataTable> groupingRegistry, String groupingKey, DataRow tableRow)
        {
            this.AggregatorFunctions.ForEach(af => {
                Decimal agValue = GetAggregatorValue(af.AggregatorVariable, tableRow);
                switch (af.FunctionType)
                {
                    case RDFQueryEnums.RDFAggregatorFunctionTypes.AVG:
                        break;
                    case RDFQueryEnums.RDFAggregatorFunctionTypes.COUNT:
                        break;
                    case RDFQueryEnums.RDFAggregatorFunctionTypes.MAX:
                        break;
                    case RDFQueryEnums.RDFAggregatorFunctionTypes.MIN:
                        break;
                    case RDFQueryEnums.RDFAggregatorFunctionTypes.SUM:
                        break;
                }
            });
        }

        /// <summary>
        /// Gets the value of the given aggregation function for the given row.
        /// Null values or type errors are automatically considered 0.
        /// </summary>
        private Decimal GetAggregatorValue(RDFVariable agVariable, DataRow tableRow)
        {
            Decimal defaultAggregatorValue = 0;
            if (!tableRow.IsNull(agVariable.VariableName))  {
                RDFPatternMember rowAggregatorValue = RDFQueryUtilities.ParseRDFPatternMember(tableRow[agVariable.VariableName].ToString());
                //PlainLiteral: will be accepted only if non-languaged and parsable to Decimal
                if (rowAggregatorValue is RDFPlainLiteral)
                {
                    if (String.IsNullOrEmpty(((RDFPlainLiteral)rowAggregatorValue).Language))
                    {
                        if(Decimal.TryParse(rowAggregatorValue.ToString(), out Decimal decimalValue))
                        {
                            return decimalValue;
                        }
                    }
                }
                //TypedLiteral: will be accepted only if parsable to Decimal
                else if (rowAggregatorValue is RDFTypedLiteral)
                {
                    if (((RDFTypedLiteral)rowAggregatorValue).HasDecimalDatatype())
                    {
                        return Decimal.Parse(((RDFTypedLiteral)rowAggregatorValue).Value);
                    }
                }
            }
            return defaultAggregatorValue;
        }
        #endregion

    }

    /// <summary>
    /// RDFAggregatorFunction represents an aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFAggregatorFunction
    {

        #region Properties
        /// <summary>
        /// Type of aggregator function applied on the in-scope variable
        /// </summary>
        public RDFQueryEnums.RDFAggregatorFunctionTypes FunctionType { get; internal set; }

        /// <summary>
        /// Variable on which the aggregator function is applied
        /// </summary>
        public RDFVariable AggregatorVariable { get; internal set; }

        /// <summary>
        /// Variable used for projection of aggregator function results
        /// </summary>
        public RDFVariable ProjectionVariable { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an aggregator function of the given type on the given variable and with the given projection name
        /// </summary>
        public RDFAggregatorFunction(RDFQueryEnums.RDFAggregatorFunctionTypes functionType, RDFVariable aggregatorVariable, RDFVariable projectionVariable)
        {
            if (aggregatorVariable != null)
            {
                if (projectionVariable != null)
                {
                    this.FunctionType = functionType;
                    this.AggregatorVariable = aggregatorVariable;
                    this.ProjectionVariable = projectionVariable;
                }
                else
                {
                    throw new RDFQueryException("Cannot create RDFAggregatorFunction because given \"projectionVariable\" parameter is null.");
                }
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFAggregatorFunction because given \"aggregatorVariable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the aggregator function 
        /// </summary>
        public override String ToString()
        {
            var result = new StringBuilder();
            switch (this.FunctionType)
            {
                case RDFQueryEnums.RDFAggregatorFunctionTypes.AVG:
                    result.Append(String.Format("(AVG({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable));
                    break;
                case RDFQueryEnums.RDFAggregatorFunctionTypes.COUNT:
                    result.Append(String.Format("(COUNT({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable));
                    break;
                case RDFQueryEnums.RDFAggregatorFunctionTypes.MAX:
                    result.Append(String.Format("(MAX({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable));
                    break;
                case RDFQueryEnums.RDFAggregatorFunctionTypes.MIN:
                    result.Append(String.Format("(MIN({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable));
                    break;
                case RDFQueryEnums.RDFAggregatorFunctionTypes.SUM:
                    result.Append(String.Format("(SUM({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable));
                    break;
            }
            return result.ToString();
        }
        #endregion

    }
}