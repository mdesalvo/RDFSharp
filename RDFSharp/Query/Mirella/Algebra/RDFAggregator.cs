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
using System.Linq;
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
        /// The aggregator's metadata: the aggregated argument (variable or expression), the projection target, and the
        /// distinct/hidden flags. Grouped into one descriptor so the aggregator class itself stays focused on behavior
        /// (partition/projection execution) rather than carrying a flat bag of definition fields. See
        /// <see cref="RDFAggregatorMetadata"/>.
        /// </summary>
        internal RDFAggregatorMetadata Metadata { get; }

        /// <summary>
        /// Whether the aggregator needs its aggregated variable to exist as a column in the working table. True for
        /// every value-aggregator; overridden to false by COUNT(*), which counts rows without reading any column. This
        /// stays on the aggregator (not in the metadata) because it is polymorphic BEHAVIOR, not a data field.
        /// </summary>
        internal virtual bool RequiresAggregatorColumn => true;

        /// <summary>
        /// Context for keeping track of aggregator's execution
        /// </summary>
        internal RDFAggregatorContext AggregatorContext { get; set; }

        /// <summary>
        /// Delimiter separating a partition-variable chunk from the next inside a partition key
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
            Metadata = new RDFAggregatorMetadata(aggregatorVariable, projectionVariable);
            AggregatorContext = new RDFAggregatorContext();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// The aggregate function as it appears in SPARQL — e.g. "COUNT(?e)", "AVG(DISTINCT ?g)",
        /// "GROUP_CONCAT(?x; SEPARATOR=\",\")", "COUNT(*)" — WITHOUT the surrounding "(... AS ?proj)". Each concrete
        /// value-aggregator provides its own; the base (and the partition aggregator) has none.
        /// </summary>
        protected virtual string AggregatorFunction => string.Empty;

        /// <summary>
        /// Gives the string representation of the aggregator function: "(call AS ?proj)", or the empty string when the
        /// aggregator has no call form (the partition aggregator).
        /// </summary>
        public override string ToString()
            => AggregatorFunction.Length == 0 ? string.Empty : $"({AggregatorFunction} AS {Metadata.ProjectionVariable})";
        #endregion

        #region Methods
        /// <summary>
        /// The printed form of the aggregated argument: the original expression when the aggregator was built over an
        /// expression (SUM(?x + ?y)), otherwise the bare aggregated variable. Used by every aggregator's ToString.
        /// </summary>
        protected string AggregatorArgument
            => Metadata.AggregatorExpression != null ? Metadata.AggregatorExpression.ToString() : Metadata.AggregatorVariable.ToString();

        /// <summary>
        /// Builds the (synthetic) aggregator variable backing an aggregate-over-expression (e.g. SUM(?x + ?y)): a
        /// reserved name derived from the projection variable, into which the GroupBy modifier materializes the
        /// expression's per-row values so the aggregation machinery keeps operating over a single column.
        /// </summary>
        internal static RDFVariable MakeExpressionVariable(RDFVariable projectionVariable)
            => new RDFVariable("?__AGGEXPR_" + (projectionVariable ?? throw new RDFQueryException("Cannot create aggregator because given \"projectionVariable\" parameter is null.")).VariableName.TrimStart('?', '$'));

        /// <summary>
        /// The aggregate function call (e.g. "COUNT(?e)") used to re-print an aggregate referenced inside a composite
        /// expression (HAVING / projection) without leaking the synthetic projection-column name.
        /// </summary>
        internal string GetAggregateCallString()
            => AggregatorFunction;

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
                if (tableRow.IsBound(Metadata.AggregatorVariable.VariableName))
                {
                    RDFPatternMember rowAggregatorValue = RDFQueryUtilities.ParseRDFPatternMember(tableRow[Metadata.AggregatorVariable.VariableName]);
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
                return tableRow.IsBound(Metadata.AggregatorVariable.VariableName)
                    ? tableRow[Metadata.AggregatorVariable.VariableName]
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
            return partitionKey.Split(ProjectionKeyPlaceholders, StringSplitOptions.RemoveEmptyEntries)
                               .Select(variableChunk => variableChunk.Split(ProjectionValuePlaceholders, StringSplitOptions.None))
                               .ToDictionary(nameAndValue => nameAndValue[0], nameAndValue => nameAndValue[1]);
        }

        /// <summary>
        /// Sets the aggregator to discard duplicates
        /// </summary>
        public RDFAggregator Distinct()
        {
            Metadata.IsDistinct = true;
            return this;
        }
        #endregion
    }

    #region RDFAggregatorMetadata
    /// <summary>
    /// RDFAggregatorMetadata bundles the DEFINITION of an aggregator (what it aggregates, where it projects, and its
    /// distinct/hidden flags), keeping it together as one descriptor instead of a flat bag of fields scattered on
    /// <see cref="RDFAggregator"/>. It carries data only — polymorphic behavior (e.g. RequiresAggregatorColumn) and
    /// execution state (the AggregatorContext) stay on the aggregator.
    /// </summary>
    internal sealed class RDFAggregatorMetadata
    {
        #region Properties
        /// <summary>
        /// Variable aggregated by the aggregator, when the argument is a bare variable (e.g. SUM(?x)). For an
        /// expression argument it is the (synthetic) column the expression is materialized into.
        /// </summary>
        internal RDFVariable AggregatorVariable { get; set; }

        /// <summary>
        /// Expression aggregated by the aggregator, when the argument is not a bare variable (e.g. SUM(?x + ?y)). When
        /// set, the GroupBy modifier materializes it into the (synthetic) <see cref="AggregatorVariable"/> column
        /// before partitioning, and the printer re-emits the original expression instead of the synthetic variable.
        /// </summary>
        internal RDFExpression AggregatorExpression { get; set; }

        /// <summary>
        /// Variable used for projection of the aggregator's results.
        /// </summary>
        internal RDFVariable ProjectionVariable { get; set; }

        /// <summary>
        /// Whether the aggregator discards duplicates (DISTINCT).
        /// </summary>
        internal bool IsDistinct { get; set; }

        /// <summary>
        /// Whether the aggregator exists ONLY to feed a composite expression (a free HAVING condition, or a projection
        /// like '?x + COUNT(?y)') rather than to be projected as a query result column. The GroupBy modifier still
        /// computes its value into a (synthetic) column, but the engine keeps it out of the output projection.
        /// </summary>
        internal bool IsHidden { get; set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Builds the metadata for an aggregator on the given aggregated variable and projection name.
        /// </summary>
        /// <exception cref="RDFQueryException">When either variable is null.</exception>
        internal RDFAggregatorMetadata(RDFVariable aggregatorVariable, RDFVariable projectionVariable)
        {
            AggregatorVariable = aggregatorVariable ?? throw new RDFQueryException("Cannot create RDFAggregator because given \"aggregatorVariable\" parameter is null.");
            ProjectionVariable = projectionVariable ?? throw new RDFQueryException("Cannot create RDFAggregator because given \"projectionVariable\" parameter is null.");
        }
        #endregion
    }
    #endregion

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