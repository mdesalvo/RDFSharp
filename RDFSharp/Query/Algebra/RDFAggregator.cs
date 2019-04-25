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
using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFAggregator represents an aggregation function applied by a GroupBy modifier
    /// </summary>
    public abstract class RDFAggregator
    {

        #region Properties
        /// <summary>
        /// Variable on which the aggregator is applied
        /// </summary>
        public RDFVariable AggregatorVariable { get; internal set; }

        /// <summary>
        /// Variable used for projection of aggregator results
        /// </summary>
        public RDFVariable ProjectionVariable { get; internal set; }

        /// <summary>
        /// Flag indicating that the aggregator discards duplicates
        /// </summary>
        public Boolean IsDistinct { get; internal set; }

        /// <summary>
        /// Context for keeping track of aggregator's execution
        /// </summary>
        internal RDFAggregatorContext AggregatorContext { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an aggregator on the given variable and with the given projection name
        /// </summary>
        internal RDFAggregator(RDFVariable aggregatorVariable, RDFVariable projectionVariable, Boolean isDistinct)
        {
            if (aggregatorVariable != null)
            {
                if (projectionVariable != null)
                {
                    this.AggregatorVariable = aggregatorVariable;
                    this.ProjectionVariable = projectionVariable;
                    this.IsDistinct = isDistinct;
                    this.AggregatorContext = new RDFAggregatorContext();
                }
                else
                {
                    throw new RDFQueryException("Cannot create RDFAggregator because given \"projectionVariable\" parameter is null.");
                }
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFAggregator because given \"aggregatorVariable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the aggregator function 
        /// </summary>
        public override String ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the partition function on the given tablerow
        /// </summary>
        internal abstract void ExecutePartitionFunction(String partitionKey, DataRow tableRow);

        /// <summary>
        /// Executes the projection function producing result's table
        /// </summary>
        internal abstract DataTable ExecuteProjectionFunction(List<RDFVariable> partitionVariables);

        /// <summary>
        /// Helps in finalization step by updating the projection function's result table 
        /// </summary>
        internal abstract void UpdateProjectionFunctionTable(String partitionKey, DataTable projFuncTable);

        /// <summary>
        /// Gets the row value for the aggregator as number
        /// </summary>
        internal Double GetRowValueAsNumber(DataRow tableRow)
        {
            if (!tableRow.IsNull(this.AggregatorVariable.VariableName))
            {
                RDFPatternMember rowAggregatorValue = RDFQueryUtilities.ParseRDFPatternMember(tableRow[this.AggregatorVariable.VariableName].ToString());
                //PlainLiteral: accepted only if numeric and non-languaged
                if (rowAggregatorValue is RDFPlainLiteral)
                {
                    if (String.IsNullOrEmpty(((RDFPlainLiteral)rowAggregatorValue).Language))
                    {
                        if (Double.TryParse(rowAggregatorValue.ToString(), out Double doubleValue))
                        {
                            return doubleValue;
                        }
                    }
                }
                //TypedLiteral: accepted only if numeric
                else if (rowAggregatorValue is RDFTypedLiteral)
                {
                    if (((RDFTypedLiteral)rowAggregatorValue).HasDecimalDatatype())
                    {
                        return Double.Parse(((RDFTypedLiteral)rowAggregatorValue).Value);
                    }
                }
            }
            return Double.NaN;
        }

        /// <summary>
        /// Gets the row value for the aggregator as string
        /// </summary>
        internal String GetRowValueAsString(DataRow tableRow)
        {
            if (!tableRow.IsNull(this.AggregatorVariable.VariableName))
            {
                return tableRow[this.AggregatorVariable.VariableName].ToString();
            }
            return String.Empty;
        }
        #endregion

    }

    #region RDFAggregatorContext
    /// <summary>
    /// RDFAggregatorContext represents a registry for keeping track of aggregator's execution
    /// </summary>
    internal class RDFAggregatorContext
    {

        #region Properties
        /// <summary>
        /// Registry to keep rack of aggregator's execution
        /// </summary>
        internal Dictionary<String, Dictionary<String, Object>> AggregatorContextRegistry { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty aggregator context
        /// </summary>
        internal RDFAggregatorContext()
        {
            this.AggregatorContextRegistry = new Dictionary<String, Dictionary<String, Object>>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given partitionKey to the aggregator context
        /// </summary>
        internal void AddPartitionKey<T>(String partitionKey)
        {
            if (!this.AggregatorContextRegistry.ContainsKey(partitionKey))
            {
                this.AggregatorContextRegistry.Add(partitionKey, new Dictionary<String, Object>()
                {
                    { "ExecutionResult", default(T) },
                    { "ExecutionCounter", 0d }
                });
            }
        }

        /// <summary>
        /// Gets the execution result for the given partition key
        /// </summary>
        internal T GetPartitionKeyExecutionResult<T>(String partitionKey)
        {
            if (!this.AggregatorContextRegistry.ContainsKey(partitionKey))
                AddPartitionKey<T>(partitionKey);

            return (T)this.AggregatorContextRegistry[partitionKey]["ExecutionResult"];
        }

        /// <summary>
        /// Gets the execution counter for the given partition key
        /// </summary>
        internal Double GetPartitionKeyExecutionCounter(String partitionKey)
        {
            return (Double)this.AggregatorContextRegistry[partitionKey]["ExecutionCounter"];
        }

        /// <summary>
        /// Gets the execution result for the given partition key
        /// </summary>
        internal void UpdatePartitionKeyExecutionResult<T>(String partitionKey, T newValue)
        {
            this.AggregatorContextRegistry[partitionKey]["ExecutionResult"] = newValue;
        }

        /// <summary>
        /// Updates the execution counter for the given partition key
        /// </summary>
        internal void UpdatePartitionKeyExecutionCounter(String partitionKey)
        {
            this.AggregatorContextRegistry[partitionKey]["ExecutionCounter"] = GetPartitionKeyExecutionCounter(partitionKey) + 1d;
        }
        #endregion

    }
    #endregion

}