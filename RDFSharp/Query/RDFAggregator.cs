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
        /// Executes the aggregator function on the given tablerow
        /// </summary>
        internal abstract void ExecuteAggregatorFunction(String partitionKey, DataRow tableRow);

        /// <summary>
        /// Finalizes the aggregator function on the result table
        /// </summary>
        internal abstract void FinalizeAggregatorFunction(List<RDFVariable> partitionVariables, DataTable workingTable);

        /// <summary>
        /// Gets the row value for the aggregator as Decimal
        /// </summary>
        internal Decimal GetRowValueAsDecimal(DataRow tableRow)
        {
            if (!tableRow.IsNull(this.AggregatorVariable.VariableName))
            {
                RDFPatternMember rowAggregatorValue = RDFQueryUtilities.ParseRDFPatternMember(tableRow[this.AggregatorVariable.VariableName].ToString());
                //PlainLiteral: accepted only if numeric and non-languaged
                if (rowAggregatorValue is RDFPlainLiteral)
                {
                    if (String.IsNullOrEmpty(((RDFPlainLiteral)rowAggregatorValue).Language))
                    {
                        if (Decimal.TryParse(rowAggregatorValue.ToString(), out Decimal decimalValue))
                        {
                            return decimalValue;
                        }
                    }
                }
                //TypedLiteral: accepted only if numeric
                else if (rowAggregatorValue is RDFTypedLiteral)
                {
                    if (((RDFTypedLiteral)rowAggregatorValue).HasDecimalDatatype())
                    {
                        return Decimal.Parse(((RDFTypedLiteral)rowAggregatorValue).Value);
                    }
                }
            }
            return Decimal.Zero;
        }

        /// <summary>
        /// Gets the row value for the aggregator as String
        /// </summary>
        internal String GetRowValueAsString(DataRow tableRow)
        {
            if (!tableRow.IsNull(this.AggregatorVariable.VariableName))
            {
                return tableRow[this.AggregatorVariable.VariableName].ToString();
            }
            return String.Empty;
        }
        
        internal void UpdateWorkingTable(String partitionKey, DataTable workingTable)
        {
            Dictionary<String, String> aggregatorResults = new Dictionary<String, String>();
            foreach (String pkValue in partitionKey.Split(new String[] { "__PK__" }, StringSplitOptions.RemoveEmptyEntries))
            {
                String[] pValues = pkValue.Split(new String[] { "__PV__" }, StringSplitOptions.RemoveEmptyEntries);
                aggregatorResults.Add(pValues[0], pValues[1]);
            }
            aggregatorResults.Add(this.ProjectionVariable.VariableName, this.AggregatorContext.GetPartitionKeyExecutionResult<Decimal>(partitionKey).ToString());
            RDFQueryEngine.AddRow(workingTable, aggregatorResults);
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
                    { "ExecutionCounter", Decimal.Zero }
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
        internal Decimal GetPartitionKeyExecutionCounter(String partitionKey)
        {
            if (this.AggregatorContextRegistry.ContainsKey(partitionKey))
            {
                return (Decimal)this.AggregatorContextRegistry[partitionKey]["ExecutionCounter"];
            }
            return Decimal.Zero;
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
            this.AggregatorContextRegistry[partitionKey]["ExecutionCounter"] = GetPartitionKeyExecutionCounter(partitionKey) + 1;
        }
        #endregion

    }
    #endregion

}