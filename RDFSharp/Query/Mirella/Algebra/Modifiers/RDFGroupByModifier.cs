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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFGroupByModifier represents an aggregator modifier to be applied on a query results table.
    /// </summary>
    public sealed class RDFGroupByModifier : RDFModifier
    {
        #region Properties
        /// <summary>
        /// The GROUP BY conditions, in order: each is either a bare grouping variable ('GROUP BY ?v') or a grouping
        /// expression ('GROUP BY (expr AS ?v)' / anonymous '(expr)'). This is the SINGLE source of truth for grouping:
        /// the partition variables, the expressions to materialize and the synthetic (anonymous) columns are all read
        /// off these conditions (see <see cref="RDFGroupByCondition"/>).
        /// </summary>
        internal List<RDFGroupByCondition> PartitionConditions { get; set; }

        /// <summary>
        /// List of aggregators applied on the result groups. It also holds the automatic <see cref="RDFPartitionAggregator"/>
        /// of each partition variable; use <see cref="EvaluableAggregators"/> / <see cref="ProjectableAggregators"/> to filter.
        /// </summary>
        internal List<RDFAggregator> Aggregators { get; set; }

        /// <summary>
        /// The aggregators that actually evaluate an aggregate function (COUNT/SUM/AVG/...): every aggregator except
        /// the automatic partition ones.
        /// </summary>
        internal IEnumerable<RDFAggregator> EvaluableAggregators
            => Aggregators.Where(ag => !(ag is RDFPartitionAggregator));

        /// <summary>
        /// The evaluable aggregators that ALSO surface as query result columns: the <see cref="EvaluableAggregators"/>
        /// minus the hidden ones (which only feed a HAVING / projection expression).
        /// </summary>
        internal IEnumerable<RDFAggregator> ProjectableAggregators
            => EvaluableAggregators.Where(ag => !ag.Metadata.IsHidden);

        /// <summary>
        /// HAVING condition: a single, all-encompassing boolean expression evaluated on the RESULT table (after
        /// projection), keeping a grouped row only when it evaluates to true. It expresses the full SPARQL HAVING
        /// power (disjunctions, aggregate on the right-hand side, non-comparison constraints, an aggregate not present
        /// in the SELECT projection). Aggregates referenced here read their already-materialized columns through a
        /// plain <see cref="RDFVariableExpression"/>.
        /// </summary>
        internal RDFExpression HavingExpression { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an implicit-grouping modifier (no partition conditions): every solution falls into a single group,
        /// so the aggregates added to it are computed over the WHOLE result set (SPARQL implicit grouping, i.e. an
        /// aggregate projection without a GROUP BY clause).
        /// </summary>
        public RDFGroupByModifier()
        {
            PartitionConditions = new List<RDFGroupByCondition>();
            Aggregators = new List<RDFAggregator>();
            IsEvaluable = true;
        }

        /// <summary>
        /// Builds a GroupBy modifier on the given variables
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFGroupByModifier(List<RDFVariable> partitionVariables) : this()
        {
            #region Guards
            if (partitionVariables == null || partitionVariables.Count == 0)
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"partitionVariables\" parameter is null or empty.");
            if (partitionVariables.Contains(null))
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"partitionVariables\" parameter contains null elements.");
            #endregion

            partitionVariables.ForEach(pv => AddPartitionConditionInternal(pv, null, false));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier. A bare partition variable prints as '?v'; a GROUP BY
        /// expression prints as '(expr AS ?v)' when named or '(expr)' when anonymous.
        /// </summary>
        public override string ToString()
            => $"GROUP BY {string.Join(" ", PartitionConditions.Select(PrintPartitionCondition))}";

        /// <summary>
        /// Prints a single GROUP BY condition (bare variable, named expression or anonymous expression).
        /// </summary>
        private static string PrintPartitionCondition(RDFGroupByCondition condition)
        {
            if (condition.IsExpression)
                return condition.IsNamed ? $"({condition.Expression} AS {condition.Variable})" : $"{condition.Expression}";
            return condition.Variable.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given aggregator to the modifier
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFGroupByModifier AddAggregator(RDFAggregator aggregator)
        {
            if (aggregator != null)
            {
                //There cannot exist two aggregators projecting the same variable (exclude automatic partition aggregators from the check)
                if (EvaluableAggregators.Any(ag => ag.Metadata.ProjectionVariable.Equals(aggregator.Metadata.ProjectionVariable)))
                    throw new RDFQueryException($"Cannot add aggregator to GroupBy modifier because the given projection variable '{aggregator.Metadata.ProjectionVariable}' is already used by another aggregator.");

                Aggregators.Add(aggregator);
            }
            return this;
        }

        /// <summary>
        /// Sets the HAVING condition: a single boolean expression evaluated on the result table, keeping only the
        /// grouped rows for which it holds. This is the fluent-API counterpart of the SPARQL HAVING clause, with the
        /// full expressive power (disjunctions, reversed comparisons, non-comparison constraints).
        /// <para>
        /// To reference an aggregate inside the expression, point at the aggregator's projection variable with a plain
        /// <see cref="RDFVariableExpression"/> (e.g. for '(COUNT(?e) AS ?cnt)', reference '?cnt'): the value is read
        /// from that already-computed column. There is no need to restate the aggregate function call.
        /// </para>
        /// </summary>
        public RDFGroupByModifier SetHavingExpression(RDFExpression havingExpression)
        {
            HavingExpression = havingExpression;
            return this;
        }

        /// <summary>
        /// Re-prints, inside an already-rendered expression (a HAVING condition or a projection expression), any
        /// aggregate the parser had to materialize behind the scenes — a HIDDEN aggregator, used for an aggregate
        /// referenced in HAVING but not projected, or one nested in a projection expression — as its ORIGINAL call
        /// (e.g. "AVG(?g)") instead of the synthetic column name it is evaluated through.
        /// <para>
        /// This mirrors how the printer already renders other synthetic columns by their definition rather than their
        /// internal name (GROUP BY '(expr)', SUM(?x + ?y)). Only the reserved hidden-aggregator column names are
        /// substituted, longest first (so '?__HAVINGAGG_10' is not corrupted by '?__HAVINGAGG_1'), so it can never
        /// touch a user variable or alias.
        /// </para>
        /// </summary>
        internal string ReprintHiddenAggregateCalls(string renderedExpression)
        {
            foreach (RDFAggregator hiddenAggregator in Aggregators.Where(ag => ag.Metadata.IsHidden)
                                                                  .OrderByDescending(ag => ag.Metadata.ProjectionVariable.ToString().Length))
                renderedExpression = renderedExpression.Replace(hiddenAggregator.Metadata.ProjectionVariable.ToString(), hiddenAggregator.GetAggregateCallString());
            return renderedExpression;
        }

        /// <summary>
        /// Adds a bare GROUP BY variable condition ('GROUP BY ?v').
        /// </summary>
        internal RDFGroupByModifier AddPartitionVariable(RDFVariable partitionVariable)
        {
            AddPartitionConditionInternal(partitionVariable, null, false);
            return this;
        }

        /// <summary>
        /// Adds a GROUP BY condition over an expression: a fresh partition variable (named '(expr AS ?v)' or
        /// anonymous '(expr)') whose values are materialized from the expression before partitioning.
        /// </summary>
        internal RDFGroupByModifier AddPartitionExpression(RDFVariable targetVariable, RDFExpression expression, bool named)
        {
            AddPartitionConditionInternal(targetVariable, expression, named);
            return this;
        }

        /// <summary>
        /// Shared GROUP BY condition registration: adds the condition (once per variable) and its mandatory partition
        /// aggregator. Everything else about the condition — the expression to materialize, whether it is named, and
        /// whether it is an anonymous synthetic column — lives on the <see cref="RDFGroupByCondition"/> itself.
        /// </summary>
        private void AddPartitionConditionInternal(RDFVariable partitionVariable, RDFExpression expression, bool named)
        {
            if (!PartitionConditions.Any(condition => condition.Variable.Equals(partitionVariable)))
            {
                PartitionConditions.Add(new RDFGroupByCondition(partitionVariable, expression, named));
                IsEvaluable = true;

                //At every partition variable must correspond a partition aggregator
                Aggregators.Add(new RDFPartitionAggregator(partitionVariable, partitionVariable));
            }
        }

        /// <summary>
        /// The (synthetic) columns to materialize into the working table BEFORE partitioning, so that grouping and
        /// aggregation keep operating over single columns: every GROUP BY expression condition (e.g. GROUP BY (?x+?y))
        /// plus every aggregate-over-expression (e.g. SUM(?x+?y)). Each entry pairs the target column with the
        /// expression producing its values, and flags whether the column is anonymous grouping scratch to be dropped.
        /// </summary>
        private List<(RDFVariable Target, RDFExpression Expression, bool IsSynthetic)> CollectComputedColumns()
        {
            List<(RDFVariable, RDFExpression, bool)> computedColumns = new List<(RDFVariable, RDFExpression, bool)>();
            foreach (RDFGroupByCondition condition in PartitionConditions.Where(c => c.IsExpression))
                computedColumns.Add((condition.Variable, condition.Expression, condition.IsSynthetic));
            foreach (RDFAggregator aggregator in Aggregators.Where(ag => ag.Metadata.AggregatorExpression != null))
                computedColumns.Add((aggregator.Metadata.AggregatorVariable, aggregator.Metadata.AggregatorExpression, false));
            return computedColumns;
        }

        /// <summary>
        /// Applies the modifier on the given table
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        internal override RDFTable ApplyModifier(RDFTable table)
        {
            //Materialize the computed columns (GROUP BY / aggregator expressions) into the working table
            RDFTable workingTable = MaterializeComputedColumns(table);

            //Perform consistency checks
            ConsistencyChecks(workingTable);

            //Reset aggregators' execution context, so that re-executing the same query
            //(or the same modifier) does not carry over state from a previous run
            Aggregators.ForEach(ag => ag.ResetContext());

            //Execute partition algorythm
            ExecutePartitionAlgorythm(workingTable);

            //Execute projection algorythm
            RDFTable resultTable = ExecuteProjectionAlgorythm();

            //Execute filter algorythm
            resultTable = ExecuteFilterAlgorythm(resultTable);

            //Drop the anonymous GROUP BY expression columns (internal grouping scratch) from the results
            DropSyntheticColumns(resultTable);

            return resultTable;
        }

        /// <summary>
        /// Builds a working table augmented with the computed columns: each declared expression is evaluated per-row
        /// and stored in a (synthetic) column, so partitioning/aggregation can keep operating over single columns.
        /// Returns the original table unchanged when there is nothing to materialize.
        /// </summary>
        private RDFTable MaterializeComputedColumns(RDFTable table)
        {
            List<(RDFVariable Target, RDFExpression Expression, bool IsSynthetic)> computedColumns = CollectComputedColumns();
            if (computedColumns.Count == 0)
                return table;

            RDFTable augmentedTable = new RDFTable();

            //Preserve the original columns, then append one (synthetic) column per computed expression
            foreach (RDFTableColumn column in table.Columns)
                augmentedTable.AddColumn(column.Name);
            foreach ((RDFVariable target, RDFExpression _, bool isSynthetic) in computedColumns)
                augmentedTable.AddColumn(target.VariableName, isSynthetic: isSynthetic);

            int baseWidth = table.ColumnsCount;
            int augmentedWidth = augmentedTable.ColumnsCount;
            foreach (RDFTableRow row in table.Rows)
            {
                string[] cells = new string[augmentedWidth];
                for (int c = 0; c < baseWidth; c++)
                    cells[c] = row[c];
                for (int k = 0; k < computedColumns.Count; k++)
                    cells[baseWidth + k] = computedColumns[k].Expression.ApplyExpression(row)?.ToString();
                augmentedTable.AddRow(cells);
            }

            return augmentedTable;
        }

        /// <summary>
        /// Removes the anonymous GROUP BY expression columns from the result table (they are an internal scratch).
        /// </summary>
        private void DropSyntheticColumns(RDFTable resultTable)
        {
            foreach (RDFGroupByCondition syntheticCondition in PartitionConditions.Where(condition => condition.IsSynthetic))
                if (resultTable.HasColumn(syntheticCondition.Variable.VariableName))
                    resultTable.RemoveColumn(syntheticCondition.Variable.VariableName);
        }

        /// <summary>
        /// Performs consistency checks
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        private void ConsistencyChecks(RDFTable table)
        {
            //Every partition variable must be found in the working table as a column
            List<string> unavailablePartitionVariables = PartitionConditions.Where(condition => !table.HasColumn(condition.Variable.ToString()))
                                                                            .Select(condition => condition.Variable.ToString())
                                                                            .ToList();
            if (unavailablePartitionVariables.Count > 0)
                throw new RDFQueryException($"Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: {string.Join(",", unavailablePartitionVariables.Distinct())}");

            //Every aggregator variable must be found in the working table as a column (COUNT(*) reads no column)
            List<string> unavailableAggregatorVariables = Aggregators.Where(ag => ag.RequiresAggregatorColumn && !table.HasColumn(ag.Metadata.AggregatorVariable.ToString()))
                                                                     .Select(ag => ag.Metadata.AggregatorVariable.ToString())
                                                                     .ToList();
            if (unavailableAggregatorVariables.Count > 0)
                throw new RDFQueryException($"Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {string.Join(",", unavailableAggregatorVariables.Distinct())}");

            //There should NOT be intersection between partition variables (GroupBy) and projection variables (Aggregators)
            List<string> commonPartitionProjectionVariables = PartitionConditions.Where(condition => EvaluableAggregators.Any(ag => condition.Variable.Equals(ag.Metadata.ProjectionVariable)))
                                                                                 .Select(condition => condition.Variable.ToString())
                                                                                 .ToList();
            if (commonPartitionProjectionVariables.Count > 0)
                throw new RDFQueryException($"Cannot apply GroupBy modifier because the following variables have been specified both for partitioning (in GroupBy) and projection (in Aggregator): {string.Join(",", commonPartitionProjectionVariables.Distinct())}");
        }

        /// <summary>
        /// Executes partition algorithm
        /// </summary>
        private void ExecutePartitionAlgorythm(RDFTable table)
        {
            foreach (RDFTableRow tableRow in table.Rows)
            {
                string partitionKey = GetPartitionKey(tableRow);
                Aggregators.ForEach(ag => ag.ExecutePartition(partitionKey, tableRow));
            }
        }

        /// <summary>
        /// Executes projection algorythm
        /// </summary>
        private RDFTable ExecuteProjectionAlgorythm()
        {
            List<RDFVariable> partitionVariables = PartitionConditions.Select(condition => condition.Variable).ToList();

            List<RDFTable> projFuncTables = new List<RDFTable>(Aggregators.Count);
            Aggregators.ForEach(ag => projFuncTables.Add(ag.ExecuteProjectionTable(partitionVariables)));
            projFuncTables.RemoveAll(pft => pft == null);

            return RDFTableEngine.CombineTables(projFuncTables);
        }

        /// <summary>
        /// Execute filter algorythm: applies the HAVING condition (the single free boolean
        /// <see cref="HavingExpression"/>) on the projected result table, keeping only the grouped rows for which it
        /// evaluates to true. When no HAVING is set the table is returned unchanged.
        /// </summary>
        private RDFTable ExecuteFilterAlgorythm(RDFTable resultTable)
        {
            if (HavingExpression == null)
                return resultTable;

            RDFTable filteredTable = resultTable.Clone();
            int width = resultTable.ColumnsCount;
            foreach (RDFTableRow resultRow in resultTable.Rows)
            {
                //A grouped row survives only when the HAVING expression evaluates to true
                RDFPatternMember havingResult = HavingExpression.ApplyExpression(resultRow);
                if (havingResult?.Equals(RDFTypedLiteral.True) ?? false)
                {
                    string[] cells = new string[width];
                    for (int c = 0; c < width; c++)
                        cells[c] = resultRow[c];
                    filteredTable.AddRow(cells);
                }
            }

            return filteredTable;
        }

        /// <summary>
        /// Calculates the partition key on the given table row. The key is built by appending, for every
        /// partition variable, "name§PV§value" (an UNBOUND value contributes an empty string) and joining the
        /// pieces with "§PK§". The exact layout MUST be preserved: the projection step splits the key back on
        /// these very placeholders to recover the bindings (see RDFAggregator.UpdateProjectionTable).
        /// </summary>
        private string GetPartitionKey(RDFTableRow tableRow)
        {
            StringBuilder partitionKey = new StringBuilder();
            for (int i = 0; i < PartitionConditions.Count; i++)
            {
                RDFVariable partitionVariable = PartitionConditions[i].Variable;

                //ProjectionKeyPlaceholder separates one variable's chunk from the next (between chunks only)
                if (i > 0)
                    partitionKey.Append(RDFAggregator.ProjectionKeyPlaceholder);

                //"name<PV>" then the bound value (nothing when UNBOUND)
                partitionKey.Append(partitionVariable.VariableName)
                            .Append(RDFAggregator.ProjectionValuePlaceholder);
                if (!tableRow.IsUnbound(partitionVariable.VariableName))
                    partitionKey.Append(tableRow[partitionVariable.VariableName]);
            }
            return partitionKey.ToString();
        }
        #endregion
    }

    #region RDFGroupByCondition
    /// <summary>
    /// RDFGroupByCondition is a single SPARQL GROUP BY condition, unifying the three legal forms under one shape so
    /// the modifier no longer has to spread a grouping condition across several parallel structures:
    /// <list type="bullet">
    /// <item>a bare grouping variable 'GROUP BY ?v' — <see cref="Expression"/> is null;</item>
    /// <item>a named grouping expression 'GROUP BY (expr AS ?v)' — <see cref="IsNamed"/> is true, '?v' is a real,
    /// projectable variable;</item>
    /// <item>an anonymous grouping expression 'GROUP BY (expr)' — <see cref="IsNamed"/> is false, backed by a reserved
    /// (synthetic) variable that must NOT surface in the results.</item>
    /// </list>
    /// </summary>
    internal sealed class RDFGroupByCondition
    {
        #region Properties
        /// <summary>
        /// The grouping variable: the variable named in 'GROUP BY ?v', the '?v' of '(expr AS ?v)', or the reserved
        /// (synthetic) variable backing an anonymous '(expr)'.
        /// </summary>
        internal RDFVariable Variable { get; }

        /// <summary>
        /// The grouping expression to materialize into the <see cref="Variable"/> column before partitioning, or null
        /// for a bare grouping variable.
        /// </summary>
        internal RDFExpression Expression { get; }

        /// <summary>
        /// Whether an expression condition was explicitly named ('(expr AS ?v)', projectable) rather than anonymous
        /// ('(expr)', internal grouping scratch). Meaningless for a bare variable condition.
        /// </summary>
        internal bool IsNamed { get; }

        /// <summary>
        /// Whether this condition groups over an expression (true) rather than a bare variable (false).
        /// </summary>
        internal bool IsExpression => Expression != null;

        /// <summary>
        /// Whether this condition is an anonymous grouping expression ('GROUP BY (expr)'): its column is internal
        /// scratch, materialized for partitioning but dropped from the query results.
        /// </summary>
        internal bool IsSynthetic => Expression != null && !IsNamed;
        #endregion

        #region Ctor
        /// <summary>
        /// Builds a GROUP BY condition over the given variable, with an optional grouping expression (null for a bare
        /// variable) and a flag telling, for an expression, whether it was explicitly named.
        /// </summary>
        internal RDFGroupByCondition(RDFVariable variable, RDFExpression expression, bool isNamed)
        {
            Variable = variable;
            Expression = expression;
            IsNamed = isNamed;
        }
        #endregion
    }
    #endregion
}
