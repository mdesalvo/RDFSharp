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
using System.Globalization;
using System.Linq;
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
        internal RDFAggregator(RDFVariable aggregatorVariable, RDFVariable projectionVariable)
        {
            if (aggregatorVariable != null)
            {
                if (projectionVariable != null)
                {
                    this.AggregatorVariable = aggregatorVariable;
                    this.ProjectionVariable = projectionVariable;
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
        /// Executes the partition on the given tablerow
        /// </summary>
        internal abstract void ExecutePartition(String partitionKey, DataRow tableRow);

        /// <summary>
        /// Executes the projection producing result's table
        /// </summary>
        internal abstract DataTable ExecuteProjection(List<RDFVariable> partitionVariables);

        /// <summary>
        /// Helps in finalization step by updating the projection's result table 
        /// </summary>
        internal abstract void UpdateProjectionTable(String partitionKey, DataTable projFuncTable);

        /// <summary>
        /// Gets the row value for the aggregator as number
        /// </summary>
        internal Double GetRowValueAsNumber(DataRow tableRow)
        {
            try
            {
                if (!tableRow.IsNull(this.AggregatorVariable.VariableName))
                {
                    RDFPatternMember rowAggregatorValue = RDFQueryUtilities.ParseRDFPatternMember(tableRow[this.AggregatorVariable.VariableName].ToString());
                    //Only numeric typedliterals are suitable for processing
                    if (rowAggregatorValue is RDFTypedLiteral && ((RDFTypedLiteral)rowAggregatorValue).HasDecimalDatatype())
                    {
                        if (Double.TryParse(((RDFTypedLiteral)rowAggregatorValue).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out Double result))
                        {
                            return result;
                        }
                    }
                }
                return Double.NaN;
            }
            catch (Exception ex)
            {
                RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Exception intercepted during evaluation of RDFAggregator '{0}' in method GetRowValueAsNumber: '{1}'", this, ex.Message));
                return Double.NaN;
            }
        }

        /// <summary>
        /// Gets the row value for the aggregator as string
        /// </summary>
        internal String GetRowValueAsString(DataRow tableRow)
        {
            try
            {
                if (!tableRow.IsNull(this.AggregatorVariable.VariableName))
                {
                    return tableRow[this.AggregatorVariable.VariableName].ToString();
                }
                return String.Empty;
            }
            catch (Exception ex)
            {
                RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Exception intercepted during evaluation of RDFAggregator '{0}' in method GetRowValueAsString: '{1}'", this, ex.Message));
                return String.Empty;
            }
        }

        /// <summary>
        /// Sets the aggregator in order to discard duplicates
        /// </summary>
        public RDFAggregator Distinct()
        {
            this.IsDistinct = true;
            return this;
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
        /// Registry to keep track of aggregator execution flow
        /// </summary>
        internal Dictionary<String, Dictionary<String, Object>> ExecutionRegistry { get; set; }

        /// <summary>
        /// Cache to keep track of aggregator execution values
        /// </summary>
        internal Dictionary<String, HashSet<Object>> ExecutionCache { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty aggregator context
        /// </summary>
        internal RDFAggregatorContext()
        {
            this.ExecutionRegistry = new Dictionary<String, Dictionary<String, Object>>();
            this.ExecutionCache = new Dictionary<String, HashSet<Object>>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given partitionKey to the aggregator context
        /// </summary>
        internal void AddPartitionKey<T>(String partitionKey, T initValue)
        {
            if (!this.ExecutionRegistry.ContainsKey(partitionKey))
            {
                this.ExecutionRegistry.Add(partitionKey, new Dictionary<String, Object>()
                {
                    { "ExecutionResult", initValue },
                    { "ExecutionCounter", 0d }
                });
            }
        }

        /// <summary>
        /// Gets the execution result for the given partition key
        /// </summary>
        internal T GetPartitionKeyExecutionResult<T>(String partitionKey, T initValue)
        {
            if (!this.ExecutionRegistry.ContainsKey(partitionKey))
                AddPartitionKey<T>(partitionKey, initValue);

            return (T)this.ExecutionRegistry[partitionKey]["ExecutionResult"];
        }

        /// <summary>
        /// Gets the execution counter for the given partition key
        /// </summary>
        internal Double GetPartitionKeyExecutionCounter(String partitionKey)
        {
            return (Double)this.ExecutionRegistry[partitionKey]["ExecutionCounter"];
        }

        /// <summary>
        /// Updates the execution result for the given partition key
        /// </summary>
        internal void UpdatePartitionKeyExecutionResult<T>(String partitionKey, T newValue)
        {
            this.ExecutionRegistry[partitionKey]["ExecutionResult"] = newValue;
        }

        /// <summary>
        /// Updates the execution counter for the given partition key
        /// </summary>
        internal void UpdatePartitionKeyExecutionCounter(String partitionKey)
        {
            this.ExecutionRegistry[partitionKey]["ExecutionCounter"] = GetPartitionKeyExecutionCounter(partitionKey) + 1d;
        }

        /// <summary>
        /// Checks for presence of the given value in given partitionkey's cache
        /// </summary>
        internal Boolean CheckPartitionKeyRowValueCache<T>(String partitionKey, T value)
        {
            if (!this.ExecutionCache.ContainsKey(partitionKey))
                this.ExecutionCache.Add(partitionKey, new HashSet<Object>());
            return this.ExecutionCache[partitionKey].Any(x => ((T)x).Equals(value));
        }

        /// <summary>
        /// Updates the given partitionKey's cache with the given value
        /// </summary>
        internal void UpdatePartitionKeyRowValueCache<T>(String partitionKey, T newValue)
        {
            this.ExecutionCache[partitionKey].Add(newValue);
        }
        #endregion

    }
    #endregion

}