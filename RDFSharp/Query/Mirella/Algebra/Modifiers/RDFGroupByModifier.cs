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
        /// List of variables on which query results are grouped
        /// </summary>
        internal List<RDFVariable> PartitionVariables { get; set; }

        /// <summary>
        /// List of aggregators applied on the result groups
        /// </summary>
        internal List<RDFAggregator> Aggregators { get; set; }

        /// <summary>
        /// Expressions to be materialized into (synthetic) columns of the working table BEFORE partitioning, so that
        /// grouping/aggregation over an expression (e.g. GROUP BY (?x+?y) or SUM(?x+?y)) keeps working over a single
        /// column. Each entry maps the target (group/aggregator) variable to the expression producing its values.
        /// </summary>
        internal List<(RDFVariable Target, RDFExpression Expression)> ComputedColumns { get; set; }

        /// <summary>
        /// Print metadata for the partition variables that come from a GROUP BY expression: keyed by variable name,
        /// it tells the original expression and whether it was explicitly named ('(expr AS ?v)') or anonymous
        /// ('(expr)'). Used to re-emit the GROUP BY clause and the SELECT projection faithfully.
        /// </summary>
        internal Dictionary<string, (RDFExpression Expression, bool Named)> PartitionConditions { get; set; }

        /// <summary>
        /// Names of the (anonymous) GROUP BY expression columns that must NOT surface in the query results: they are
        /// an internal grouping scratch (an unnamed 'GROUP BY (expr)' has no projectable variable).
        /// </summary>
        internal HashSet<string> SyntheticPartitionVariables { get; set; }

        /// <summary>
        /// Free HAVING condition: a single, all-encompassing boolean expression evaluated on the RESULT table (after
        /// projection), keeping a grouped row only when it evaluates to true. It represents the full SPARQL HAVING
        /// power the per-aggregator <see cref="RDFAggregator.HavingClause"/> cannot express (disjunctions, aggregate
        /// on the right-hand side, non-comparison constraints, an aggregate not present in the SELECT projection).
        /// Aggregates referenced here read their already-materialized columns via <see cref="RDFAggregateReferenceExpression"/>.
        /// When both this and per-aggregator having-clauses are present, they are conjoined (ANDed).
        /// </summary>
        internal RDFExpression HavingExpression { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an implicit-grouping modifier (no partition variables): every solution falls into a single group,
        /// so the aggregates added to it are computed over the WHOLE result set (SPARQL implicit grouping, i.e. an
        /// aggregate projection without a GROUP BY clause).
        /// </summary>
        public RDFGroupByModifier()
        {
            PartitionVariables = new List<RDFVariable>();
            Aggregators = new List<RDFAggregator>();
            ComputedColumns = new List<(RDFVariable, RDFExpression)>();
            PartitionConditions = new Dictionary<string, (RDFExpression, bool)>();
            SyntheticPartitionVariables = new HashSet<string>();
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

            partitionVariables.ForEach(pv => AddPartitionVariableInternal(pv, null, false));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier. A bare partition variable prints as '?v'; a GROUP BY
        /// expression prints as '(expr AS ?v)' when named or '(expr)' when anonymous.
        /// </summary>
        public override string ToString()
            => $"GROUP BY {string.Join(" ", PartitionVariables.Select(PrintPartitionCondition))}";

        /// <summary>
        /// Prints a single GROUP BY condition (bare variable, named expression or anonymous expression).
        /// </summary>
        private string PrintPartitionCondition(RDFVariable partitionVariable)
        {
            if (PartitionConditions.TryGetValue(partitionVariable.VariableName, out (RDFExpression Expression, bool Named) condition))
                return condition.Named ? $"({condition.Expression} AS {partitionVariable})" : $"{condition.Expression}";
            return partitionVariable.ToString();
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
                if (Aggregators.Any(ag => !(ag is RDFPartitionAggregator) && ag.ProjectionVariable.Equals(aggregator.ProjectionVariable)))
                    throw new RDFQueryException($"Cannot add aggregator to GroupBy modifier because the given projection variable '{aggregator.ProjectionVariable}' is already used by another aggregator.");

                Aggregators.Add(aggregator);

                //An aggregate-over-expression needs its expression materialized into the (synthetic) aggregator
                //column before partitioning: register it as a computed column of the modifier
                if (aggregator.AggregatorExpression != null)
                    AddComputedColumn(aggregator.AggregatorVariable, aggregator.AggregatorExpression);
            }
            return this;
        }

        /// <summary>
        /// Sets the free HAVING condition: a single boolean expression evaluated on the result table, keeping only
        /// the grouped rows for which it holds. This is the fluent-API counterpart of the SPARQL HAVING clause and
        /// supersedes the restricted per-aggregator <see cref="RDFAggregator.SetHavingClause"/> for any condition
        /// the latter cannot express (disjunctions, reversed comparisons, non-comparison constraints).
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
        /// Adds a bare GROUP BY variable condition ('GROUP BY ?v').
        /// </summary>
        internal RDFGroupByModifier AddPartitionVariable(RDFVariable partitionVariable)
        {
            AddPartitionVariableInternal(partitionVariable, null, false);
            return this;
        }

        /// <summary>
        /// Adds a GROUP BY condition over an expression: a fresh partition variable (named '(expr AS ?v)' or
        /// anonymous '(expr)') whose values are materialized from the expression before partitioning.
        /// </summary>
        internal RDFGroupByModifier AddPartitionExpression(RDFVariable targetVariable, RDFExpression expression, bool named)
        {
            AddPartitionVariableInternal(targetVariable, expression, named);
            return this;
        }

        /// <summary>
        /// Declares a synthetic column to be materialized from the given expression before partitioning (used for the
        /// aggregated-expression case, e.g. SUM(?x + ?y), where the aggregator operates on the materialized column).
        /// </summary>
        internal RDFGroupByModifier AddComputedColumn(RDFVariable targetVariable, RDFExpression expression)
        {
            if (targetVariable != null && expression != null)
                ComputedColumns.Add((targetVariable, expression));
            return this;
        }

        /// <summary>
        /// Shared partition-variable registration: adds the variable (once), its mandatory partition aggregator and,
        /// for an expression condition, the print metadata + the computed column to materialize (anonymous
        /// expressions are also tracked as synthetic, so they do not surface in the results).
        /// </summary>
        private void AddPartitionVariableInternal(RDFVariable partitionVariable, RDFExpression expression, bool named)
        {
            if (!PartitionVariables.Any(pv => pv.Equals(partitionVariable)))
            {
                PartitionVariables.Add(partitionVariable);
                IsEvaluable = true;

                //At every partition variable must correspond a partition aggregator
                Aggregators.Add(new RDFPartitionAggregator(partitionVariable, partitionVariable));

                //An expression condition is materialized into the partition column and remembered for printing
                if (expression != null)
                {
                    PartitionConditions[partitionVariable.VariableName] = (expression, named);
                    ComputedColumns.Add((partitionVariable, expression));
                    if (!named)
                        SyntheticPartitionVariables.Add(partitionVariable.VariableName);
                }
            }
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
            if (ComputedColumns.Count == 0)
                return table;

            RDFTable augmentedTable = new RDFTable();

            //Preserve the original columns, then append one (synthetic) column per computed expression
            foreach (RDFTableColumn column in table.Columns)
                augmentedTable.AddColumn(column.Name);
            foreach ((RDFVariable target, RDFExpression _) in ComputedColumns)
                augmentedTable.AddColumn(target.VariableName, isSynthetic: SyntheticPartitionVariables.Contains(target.VariableName));

            int baseWidth = table.ColumnsCount;
            int augmentedWidth = augmentedTable.ColumnsCount;
            foreach (RDFTableRow row in table.Rows)
            {
                string[] cells = new string[augmentedWidth];
                for (int c = 0; c < baseWidth; c++)
                    cells[c] = row[c];
                for (int k = 0; k < ComputedColumns.Count; k++)
                    cells[baseWidth + k] = ComputedColumns[k].Expression.ApplyExpression(row)?.ToString();
                augmentedTable.AddRow(cells);
            }

            return augmentedTable;
        }

        /// <summary>
        /// Removes the anonymous GROUP BY expression columns from the result table (they are an internal scratch).
        /// </summary>
        private void DropSyntheticColumns(RDFTable resultTable)
        {
            foreach (string syntheticVariable in SyntheticPartitionVariables)
                if (resultTable.HasColumn(syntheticVariable))
                    resultTable.RemoveColumn(syntheticVariable);
        }

        /// <summary>
        /// Performs consistency checks
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        private void ConsistencyChecks(RDFTable table)
        {
            //Every partition variable must be found in the working table as a column
            List<string> unavailablePartitionVariables = PartitionVariables.Where(pv => !table.HasColumn(pv.ToString()))
                                                                           .Select(pv => pv.ToString())
                                                                           .ToList();
            if (unavailablePartitionVariables.Count > 0)
                throw new RDFQueryException($"Cannot apply GroupBy modifier because the working table does not contain the following columns needed for partitioning: {string.Join(",", unavailablePartitionVariables.Distinct())}");

            //Every aggregator variable must be found in the working table as a column (COUNT(*) reads no column)
            List<string> unavailableAggregatorVariables = Aggregators.Where(ag => ag.RequiresAggregatorColumn && !table.HasColumn(ag.AggregatorVariable.ToString()))
                                                                     .Select(ag => ag.AggregatorVariable.ToString())
                                                                     .ToList();
            if (unavailableAggregatorVariables.Count > 0)
                throw new RDFQueryException($"Cannot apply GroupBy modifier because the working table does not contain the following columns needed for aggregation: {string.Join(",", unavailableAggregatorVariables.Distinct())}");

            //There should NOT be intersection between partition variables (GroupBy) and projection variables (Aggregators)
            List<string> commonPartitionProjectionVariables = PartitionVariables.Where(pv => Aggregators.Any(ag => !(ag is RDFPartitionAggregator) && pv.Equals(ag.ProjectionVariable)))
                                                                                .Select(pav => pav.ToString())
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
            List<RDFTable> projFuncTables = new List<RDFTable>(Aggregators.Count);
            Aggregators.ForEach(ag => projFuncTables.Add(ag.ExecuteProjectionTable(PartitionVariables)));
            projFuncTables.RemoveAll(pft => pft == null);

            return RDFTableEngine.CombineTables(projFuncTables);
        }

        /// <summary>
        /// Execute filter algorythm: applies the HAVING conditions on the projected result table, keeping only the
        /// grouped rows that satisfy ALL of them. Two (conjoined) sources of conditions are honored:
        /// <list type="bullet">
        /// <item>the restricted per-aggregator having-clauses (legacy fluent API
        /// <see cref="RDFAggregator.SetHavingClause"/>), each a single '(AGGREGATE OP value)' comparison;</item>
        /// <item>the free <see cref="HavingExpression"/> (a full boolean expression, e.g. produced by the SPARQL
        /// parser), evaluated once per row.</item>
        /// </list>
        /// When neither source is present the table is returned unchanged.
        /// </summary>
        private RDFTable ExecuteFilterAlgorythm(RDFTable resultTable)
        {
            //Build a per-row predicate for every legacy per-aggregator having-clause (each '(AGGREGATE OP value)')
            List<RDFComparisonExpression> aggregatorHavingComparisons = Aggregators
                .Where(ag => ag.HavingClause.Item1)
                .Select(ag => new RDFComparisonExpression(
                    ag.HavingClause.Item2,
                    ag.ProjectionVariable,
                    ag.HavingClause.Item3 is RDFResource havingRes ? new RDFConstantExpression(havingRes)
                     : ag.HavingClause.Item3 is RDFLiteral havingLit ?  new RDFConstantExpression(havingLit)
                     : ag.HavingClause.Item3 is RDFVariable havingVar ? new RDFVariableExpression(havingVar)
                     : null as RDFExpression))
                .ToList();

            //Nothing to filter: neither the legacy clauses nor the free expression are set
            if (aggregatorHavingComparisons.Count == 0 && HavingExpression == null)
                return resultTable;

            #region ExecuteFilters
            RDFTable filteredTable = resultTable.Clone();
            int width = resultTable.ColumnsCount;
            foreach (RDFTableRow resultRow in resultTable.Rows)
            {
                //A row survives only when every legacy comparison AND the free HAVING expression evaluate to true
                bool keepRow = true;

                List<RDFComparisonExpression>.Enumerator comparisonsEnum = aggregatorHavingComparisons.GetEnumerator();
                while (keepRow && comparisonsEnum.MoveNext())
                {
                    RDFPatternMember comparisonResult = comparisonsEnum.Current?.ApplyExpression(resultRow);
                    if (!(comparisonResult?.Equals(RDFTypedLiteral.True) ?? false))
                        keepRow = false;
                }

                if (keepRow && HavingExpression != null)
                {
                    RDFPatternMember havingResult = HavingExpression.ApplyExpression(resultRow);
                    if (!(havingResult?.Equals(RDFTypedLiteral.True) ?? false))
                        keepRow = false;
                }

                if (keepRow)
                {
                    string[] cells = new string[width];
                    for (int c = 0; c < width; c++)
                        cells[c] = resultRow[c];
                    filteredTable.AddRow(cells);
                }
            }
            #endregion

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
            for (int i = 0; i < PartitionVariables.Count; i++)
            {
                RDFVariable partitionVariable = PartitionVariables[i];

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
}