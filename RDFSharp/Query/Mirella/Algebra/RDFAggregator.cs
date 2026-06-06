/*
   Copyright 2012-2026 Marco De Salvo

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
using System.Globalization;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFAggregator represents an aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFAggregator
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
        public bool IsDistinct { get; internal set; }

        /// <summary>
        /// Tuple indicating that the aggregator is also an having-clause
        /// </summary>
        public (bool, RDFQueryEnums.RDFComparisonFlavors, RDFPatternMember) HavingClause { get; internal set; }

        /// <summary>
        /// Context for keeping track of aggregator's execution
        /// </summary>
        internal RDFAggregatorContext AggregatorContext { get; set; }

        /// <summary>
        /// Delimiter separating one partition-variable chunk from the next inside a partition key
        /// </summary>
        internal const string ProjectionKeyPlaceholder = "§PK§";
        internal static readonly string[] ProjectionKeyPlaceholders = { ProjectionKeyPlaceholder };

        /// <summary>
        /// Delimiter separating a partition variable's name from its value inside a partition key
        /// </summary>
        internal const string ProjectionValuePlaceholder = "§PV§";
        internal static readonly string[] ProjectionValuePlaceholders = { ProjectionValuePlaceholder };
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an aggregator on the given variable and with the given projection name
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        internal RDFAggregator(RDFVariable aggregatorVariable, RDFVariable projectionVariable)
        {
            AggregatorVariable = aggregatorVariable ?? throw new RDFQueryException("Cannot create RDFAggregator because given \"aggregatorVariable\" parameter is null.");
            ProjectionVariable = projectionVariable ?? throw new RDFQueryException("Cannot create RDFAggregator because given \"projectionVariable\" parameter is null.");
            HavingClause = (false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null);
            AggregatorContext = new RDFAggregatorContext();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the aggregator function
        /// </summary>
        public override string ToString()
            => string.Empty;
        #endregion

        #region Methods
        /// <summary>
        /// Resets the aggregator's execution context, so that the same aggregator (and the
        /// query owning it) can be safely re-executed without carrying over state from a
        /// previous run (which would otherwise corrupt sums, counters and caches)
        /// </summary>
        internal void ResetContext()
            => AggregatorContext = new RDFAggregatorContext();

        /// <summary>
        /// Executes the partition on the given table row
        /// </summary>
        internal virtual void ExecutePartition(string partitionKey, RDFTableRow tableRow) { }

        /// <summary>
        /// Executes the projection producing result's table
        /// </summary>
        internal virtual RDFTable ExecuteProjectionTable(List<RDFVariable> partitionVariables) => null;

        /// <summary>
        /// Helps in finalization step by updating the projection's result table
        /// </summary>
        internal virtual void UpdateProjectionTable(string partitionKey, RDFTable projFuncTable) { }

        /// <summary>
        /// Gets the row value for the aggregator as number
        /// </summary>
        internal double GetRowValueAsNumber(RDFTableRow tableRow)
        {
            try
            {
                if (tableRow.IsBound(AggregatorVariable.VariableName))
                {
                    RDFPatternMember rowAggregatorValue = RDFQueryUtilities.ParseRDFPatternMember(tableRow[AggregatorVariable.VariableName]);
                    //Only numeric typedliterals are suitable for processing
                    if (rowAggregatorValue is RDFTypedLiteral rowAggregatorValueTLit && rowAggregatorValueTLit.HasDecimalDatatype())
                    {
                        //owl:rational needs parsing and evaluation before being compared
                        if (rowAggregatorValueTLit.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                            return Convert.ToDouble(RDFModelUtilities.ComputeOWLRationalValue(rowAggregatorValueTLit), CultureInfo.InvariantCulture);
                        if (double.TryParse(rowAggregatorValueTLit.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                            return result;
                    }
                }
                return double.NaN;
            }
            catch { return double.NaN; }
        }

        /// <summary>
        /// Gets the row value for the aggregator as string
        /// </summary>
        internal string GetRowValueAsString(RDFTableRow tableRow)
        {
            try
            {
                return tableRow.IsBound(AggregatorVariable.VariableName)
                    ? tableRow[AggregatorVariable.VariableName]
                    : string.Empty;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Parses a partition key ("name§PV§value§PK§name§PV§value…") back into its variable => value bindings.
        /// This is the inverse of RDFGroupByModifier.GetPartitionKey, used by every aggregator's projection step.
        /// </summary>
        internal static Dictionary<string, string> GetProjectionBindings(string partitionKey)
        {
            Dictionary<string, string> bindings = new Dictionary<string, string>();
            foreach (string variableChunk in partitionKey.Split(ProjectionKeyPlaceholders, StringSplitOptions.RemoveEmptyEntries))
            {
                //"name§PV§value" => [name, value] (an UNBOUND value yields an empty second slot)
                string[] nameAndValue = variableChunk.Split(ProjectionValuePlaceholders, StringSplitOptions.None);
                bindings.Add(nameAndValue[0], nameAndValue[1]);
            }
            return bindings;
        }

        /// <summary>
        /// Prints the having-clause of the aggregator
        /// </summary>
        internal string PrintHavingClause(List<RDFNamespace> prefixes)
        {
            if (HavingClause.Item1)
            {
                StringBuilder result = new StringBuilder();
                result.Append('(');
                result.Append(ToString(), 1, ToString().LastIndexOf(" AS ?", StringComparison.Ordinal));
                switch (HavingClause.Item2)
                {
                    case RDFQueryEnums.RDFComparisonFlavors.LessThan:
                        result.Append("< ");
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan:
                        result.Append("<= ");
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.EqualTo:
                        result.Append("= ");
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.NotEqualTo:
                        result.Append("!= ");
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan:
                        result.Append(">= ");
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.GreaterThan:
                        result.Append("> ");
                        break;
                }
                result.Append(RDFQueryPrinter.PrintPatternMember(HavingClause.Item3, prefixes));
                result.Append(')');
                return result.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Sets the aggregator to discard duplicates
        /// </summary>
        public RDFAggregator Distinct()
        {
            IsDistinct = true;
            return this;
        }

        /// <summary>
        /// Sets the aggregator to also represent an having-clause
        /// </summary>
        public RDFAggregator SetHavingClause(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor, RDFPatternMember comparisonValue)
        {
            if (comparisonValue != null)
                HavingClause = (true, comparisonFlavor, comparisonValue);
            return this;
        }
        #endregion
    }

    #region RDFAggregatorContext
    /// <summary>
    /// RDFAggregatorPartitionState holds the running aggregation state of a single partition: the current
    /// result and how many rows have been folded into it so far. It replaces the former
    /// Dictionary&lt;string,object&gt; keyed by the literal strings "ExecutionResult"/"ExecutionCounter",
    /// removing - on every aggregated row - two string-keyed dictionary lookups and the boxing of the counter.
    /// </summary>
    internal sealed class RDFAggregatorPartitionState
    {
        /// <summary>
        /// Running result of the aggregation for this partition. It stays an object because its concrete type is
        /// aggregator-dependent: a double for COUNT/SUM/AVG/MIN/MAX over numbers, a string for GROUP_CONCAT/
        /// SAMPLE/PARTITION and for MIN/MAX over strings.
        /// </summary>
        internal object ExecutionResult { get; set; }

        /// <summary>
        /// Number of rows folded into this partition so far (used e.g. by AVG). A plain double, never boxed.
        /// </summary>
        internal double ExecutionCounter { get; set; }

        /// <summary>
        /// Builds the partition state seeded with the given initial result (counter starts at zero)
        /// </summary>
        internal RDFAggregatorPartitionState(object initResult)
        {
            ExecutionResult = initResult;
            ExecutionCounter = 0d;
        }
    }

    /// <summary>
    /// RDFAggregatorContext represents a registry for keeping track of aggregator's execution
    /// </summary>
    internal sealed class RDFAggregatorContext
    {
        #region Properties
        /// <summary>
        /// Registry to keep track of aggregator execution flow (one state object per partition key)
        /// </summary>
        internal Dictionary<string, RDFAggregatorPartitionState> ExecutionRegistry { get; set; }

        /// <summary>
        /// Cache to keep track of aggregator execution values
        /// </summary>
        internal Dictionary<string, HashSet<object>> ExecutionCache { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an empty aggregator context
        /// </summary>
        internal RDFAggregatorContext()
        {
            ExecutionRegistry = new Dictionary<string, RDFAggregatorPartitionState>();
            ExecutionCache = new Dictionary<string, HashSet<object>>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given partitionKey to the aggregator context
        /// </summary>
        internal void AddPartitionKey<T>(string partitionKey, T initValue)
        {
            if (!ExecutionRegistry.ContainsKey(partitionKey))
                ExecutionRegistry.Add(partitionKey, new RDFAggregatorPartitionState(initValue));
        }

        /// <summary>
        /// Gets the execution result for the given partition key (creating the partition state if absent)
        /// </summary>
        internal T GetPartitionKeyExecutionResult<T>(string partitionKey, T initValue)
        {
            if (!ExecutionRegistry.TryGetValue(partitionKey, out RDFAggregatorPartitionState partitionState))
            {
                partitionState = new RDFAggregatorPartitionState(initValue);
                ExecutionRegistry.Add(partitionKey, partitionState);
            }
            return (T)partitionState.ExecutionResult;
        }

        /// <summary>
        /// Gets the execution counter for the given partition key
        /// </summary>
        internal double GetPartitionKeyExecutionCounter(string partitionKey)
            => ExecutionRegistry[partitionKey].ExecutionCounter;

        /// <summary>
        /// Updates the execution result for the given partition key
        /// </summary>
        internal void UpdatePartitionKeyExecutionResult<T>(string partitionKey, T newValue)
            => ExecutionRegistry[partitionKey].ExecutionResult = newValue;

        /// <summary>
        /// Updates the execution counter for the given partition key
        /// </summary>
        internal void UpdatePartitionKeyExecutionCounter(string partitionKey)
            => ExecutionRegistry[partitionKey].ExecutionCounter++;

        /// <summary>
        /// Checks for presence of the given value in given partitionkey's cache
        /// </summary>
        internal bool CheckPartitionKeyRowValueCache<T>(string partitionKey, T value)
        {
            if (!ExecutionCache.ContainsKey(partitionKey))
                ExecutionCache.Add(partitionKey, new HashSet<object>());
            return ExecutionCache[partitionKey].Contains(value);
        }

        /// <summary>
        /// Updates the given partitionKey's cache with the given value
        /// </summary>
        internal void UpdatePartitionKeyRowValueCache<T>(string partitionKey, T newValue)
            => ExecutionCache[partitionKey].Add(newValue);
        #endregion
    }
    #endregion
}