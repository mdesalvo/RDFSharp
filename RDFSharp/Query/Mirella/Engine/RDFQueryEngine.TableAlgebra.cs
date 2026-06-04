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
using System.Data;
using System.Linq;
using System.Text;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    // RDFQueryEngine (MIRELLA): relational algebra over RDFTable (joins, combine, sort, distinct,
    // projection, expression evaluation and table population helpers).
    internal partial class RDFQueryEngine
    {
        /// <summary>
        /// Adds a new column to the given table, avoiding duplicates
        /// </summary>
        internal static void AddColumn(DataTable table, string columnName)
        {
            string colName = columnName.Trim().ToUpperInvariant();
            if (!table.Columns.Contains(colName))
                table.Columns.Add(colName, typeof(string));
        }

        /// <summary>
        /// Adds a new row to the given table
        /// </summary>
        internal static void AddRow(DataTable table, Dictionary<string, string> bindings)
        {
            bool rowAdded = false;

            DataRow resultRow = table.NewRow();
            //Plain iteration over the bindings avoids the per-row LINQ Where() iterator/closure
            //allocation, and the KeyValuePair access avoids re-looking-up the value by key
            foreach (KeyValuePair<string, string> binding in bindings)
            {
                if (table.Columns.Contains(binding.Key))
                {
                    resultRow[binding.Key] = binding.Value;
                    rowAdded = true;
                }
            }

            if (rowAdded)
                table.Rows.Add(resultRow);
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given graph
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, List<RDFTriple> triples, RDFTable resultTable)
        {
            //Resolve the target ordinal of each variable position once (-1 for non-variable positions, whose
            //ToString() is not a column name). Positions binding the same variable resolve to the same ordinal,
            //so the inequality checks apply "first key wins" dedup (in S,P,O order).
            int colS = resultTable.OrdinalOf(pattern.Subject.ToString());
            int colP = resultTable.OrdinalOf(pattern.Predicate.ToString());
            int colO = resultTable.OrdinalOf(pattern.Object.ToString());
            bool writeS = colS >= 0;
            bool writeP = colP >= 0 && colP != colS;
            bool writeO = colO >= 0 && colO != colS && colO != colP;
            int width = resultTable.ColumnsCount;

            //Iterate result graph's triples
            foreach (RDFTriple triple in triples)
            {
                string[] cells = new string[width];
                if (writeS)
                    cells[colS] = triple.Subject.ToString();
                if (writeP)
                    cells[colP] = triple.Predicate.ToString();
                if (writeO)
                    cells[colO] = triple.Object.ToString();
                resultTable.AddRow(cells);
            }
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given store
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, List<RDFQuadruple> quadruples, RDFTable resultTable)
        {
            //Resolve the target ordinal of each variable position once (-1 for non-variable positions).
            //Positions binding the same variable resolve to the same ordinal, so the inequality checks
            //apply "first key wins" dedup (in C,S,P,O order).
            string patternContext = pattern.Context?.ToString();
            int colC = patternContext != null ? resultTable.OrdinalOf(patternContext) : -1;
            int colS = resultTable.OrdinalOf(pattern.Subject.ToString());
            int colP = resultTable.OrdinalOf(pattern.Predicate.ToString());
            int colO = resultTable.OrdinalOf(pattern.Object.ToString());
            bool writeC = colC >= 0;
            bool writeS = colS >= 0 && colS != colC;
            bool writeP = colP >= 0 && colP != colC && colP != colS;
            bool writeO = colO >= 0 && colO != colC && colO != colS && colO != colP;
            int width = resultTable.ColumnsCount;

            //Iterate result store's quadruples
            foreach (RDFQuadruple quadruple in quadruples)
            {
                string[] cells = new string[width];
                if (writeC)
                    cells[colC] = quadruple.Context.ToString();
                if (writeS)
                    cells[colS] = quadruple.Subject.ToString();
                if (writeP)
                    cells[colP] = quadruple.Predicate.ToString();
                if (writeO)
                    cells[colO] = quadruple.Object.ToString();
                resultTable.AddRow(cells);
            }
        }

        /// <summary>
        /// Builds a collision-free Ordinal key over the row's common columns, or null if any of them is UNBOUND
        /// (an UNBOUND column can never take part in an equality match)
        /// </summary>
        private static string BuildJoinKey(RDFTableRow row, int[] commonOrdinals)
        {
            StringBuilder keyBuilder = new StringBuilder();
            for (int i = 0; i < commonOrdinals.Length; i++)
            {
                string cell = row[commonOrdinals[i]];
                if (cell == null)
                    return null;
                keyBuilder.Append(cell.Length).Append(':').Append(cell);
            }
            return keyBuilder.ToString();
        }

        /// <summary>
        /// Tells whether two rows are join-compatible on their common columns: for each of them either side
        /// is UNBOUND, or both are bound to the same value (Ordinal).
        /// </summary>
        private static bool AreJoinCompatible(RDFTableRow leftRow, int[] leftCommonOrdinals, RDFTableRow rightRow, int[] rightCommonOrdinals)
        {
            for (int i = 0; i < leftCommonOrdinals.Length; i++)
            {
                string leftCell = leftRow[leftCommonOrdinals[i]];
                string rightCell = rightRow[rightCommonOrdinals[i]];
                if (leftCell != null && rightCell != null && !string.Equals(leftCell, rightCell, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Resolves the names of the columns shared by both tables (already normalized, Ordinal comparison)
        /// </summary>
        private static List<string> CommonColumnNames(RDFTable leftTable, RDFTable rightTable)
        {
            List<string> commonNames = new List<string>();
            foreach (RDFTableColumn leftColumn in leftTable.Columns)
                if (rightTable.HasColumn(leftColumn.Name))
                    commonNames.Add(leftColumn.Name);
            return commonNames;
        }

        /// <summary>
        /// Joins two tables WITHOUT support for OPTIONAL/UNION (strict inner-join, or product when no common column)
        /// </summary>
        internal static RDFTable InnerJoinTables(RDFTable leftTable, RDFTable rightTable)
        {
            RDFTable joinTable = new RDFTable();
            List<string> commonNames = CommonColumnNames(leftTable, rightTable);

            //PRODUCT-JOIN (no common columns): full cartesian product, left-major
            if (commonNames.Count == 0)
            {
                foreach (RDFTableColumn leftColumn in leftTable.Columns)
                    joinTable.AddColumn(leftColumn.Name);
                foreach (RDFTableColumn rightColumn in rightTable.Columns)
                    joinTable.AddColumn(rightColumn.Name);

                int leftWidth = leftTable.ColumnsCount;
                int rightWidth = rightTable.ColumnsCount;
                foreach (RDFTableRow leftRow in leftTable.Rows)
                    foreach (RDFTableRow rightRow in rightTable.Rows)
                    {
                        string[] cells = new string[leftWidth + rightWidth];
                        for (int i = 0; i < leftWidth; i++)
                            cells[i] = leftRow[i];
                        for (int i = 0; i < rightWidth; i++)
                            cells[leftWidth + i] = rightRow[i];
                        joinTable.AddRow(cells);
                    }
                return joinTable;
            }

            //INNER-JOIN: result columns = left columns + right non-common columns (common columns kept from left)
            foreach (RDFTableColumn leftColumn in leftTable.Columns)
                joinTable.AddColumn(leftColumn.Name);
            List<int> rightNonCommonOrdinals = new List<int>();
            foreach (RDFTableColumn rightColumn in rightTable.Columns)
                if (!leftTable.HasColumn(rightColumn.Name))
                {
                    joinTable.AddColumn(rightColumn.Name);
                    rightNonCommonOrdinals.Add(rightColumn.Ordinal);
                }

            int leftColumnsWidth = leftTable.ColumnsCount;
            int joinColumnsWidth = joinTable.ColumnsCount;
            int[] leftCommonOrdinals = commonNames.Select(leftTable.OrdinalOf).ToArray();
            int[] rightCommonOrdinals = commonNames.Select(rightTable.OrdinalOf).ToArray();

            //Hash the right table by its common-column key (rows with any UNBOUND common cell can never match)
            RDFTableRowCollection rightRows = rightTable.Rows;
            Dictionary<string, List<int>> rightIndex = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (int ri = 0; ri < rightRows.Count; ri++)
            {
                string key = BuildJoinKey(rightRows[ri], rightCommonOrdinals);
                if (key != null)
                {
                    if (!rightIndex.TryGetValue(key, out List<int> bucket))
                        rightIndex[key] = bucket = new List<int>();
                    bucket.Add(ri);
                }
            }

            //Probe with the left table, preserving left-major then right order
            foreach (RDFTableRow leftRow in leftTable.Rows)
            {
                string key = BuildJoinKey(leftRow, leftCommonOrdinals);
                if (key == null || !rightIndex.TryGetValue(key, out List<int> matches))
                    continue;
                foreach (int ri in matches)
                {
                    RDFTableRow rightRow = rightRows[ri];
                    string[] cells = new string[joinColumnsWidth];
                    for (int i = 0; i < leftColumnsWidth; i++)
                        cells[i] = leftRow[i];
                    for (int k = 0; k < rightNonCommonOrdinals.Count; k++)
                        cells[leftColumnsWidth + k] = rightRow[rightNonCommonOrdinals[k]];
                    joinTable.AddRow(cells);
                }
            }
            return joinTable;
        }

        /// <summary>
        /// Joins two tables WITH support for OPTIONAL/UNION (compatible-mappings outer-join with coalescing)
        /// </summary>
        internal static RDFTable OuterJoinTables(RDFTable leftTable, RDFTable rightTable)
        {
            RDFTable joinTable = new RDFTable();
            bool rightIsOptional = rightTable.IsOptional;
            List<string> commonNames = CommonColumnNames(leftTable, rightTable);

            //Result columns = left columns + right non-common columns
            foreach (RDFTableColumn leftColumn in leftTable.Columns)
                joinTable.AddColumn(leftColumn.Name);
            List<int> rightNonCommonOrdinals = new List<int>();
            foreach (RDFTableColumn rightColumn in rightTable.Columns)
                if (!leftTable.HasColumn(rightColumn.Name))
                {
                    joinTable.AddColumn(rightColumn.Name);
                    rightNonCommonOrdinals.Add(rightColumn.Ordinal);
                }

            int leftColumnsWidth = leftTable.ColumnsCount;
            int joinColumnsWidth = joinTable.ColumnsCount;
            int[] leftCommonOrdinals = commonNames.Select(leftTable.OrdinalOf).ToArray();
            int[] rightCommonOrdinals = commonNames.Select(rightTable.OrdinalOf).ToArray();

            RDFTableRowCollection rightRows = rightTable.Rows;
            foreach (RDFTableRow leftRow in leftTable.Rows)
            {
                bool foundRelated = false;
                for (int ri = 0; ri < rightRows.Count; ri++)
                {
                    RDFTableRow rightRow = rightRows[ri];
                    if (!AreJoinCompatible(leftRow, leftCommonOrdinals, rightRow, rightCommonOrdinals))
                        continue;

                    foundRelated = true;
                    string[] cells = new string[joinColumnsWidth];
                    //Left part (includes common columns, taken from left)
                    for (int i = 0; i < leftColumnsWidth; i++)
                        cells[i] = leftRow[i];
                    //Coalesce common columns: when left is UNBOUND but right is bound, take right
                    for (int c = 0; c < leftCommonOrdinals.Length; c++)
                    {
                        int leftOrdinal = leftCommonOrdinals[c];
                        if (cells[leftOrdinal] == null)
                            cells[leftOrdinal] = rightRow[rightCommonOrdinals[c]];
                    }
                    //Right non-common columns
                    for (int k = 0; k < rightNonCommonOrdinals.Count; k++)
                        cells[leftColumnsWidth + k] = rightRow[rightNonCommonOrdinals[k]];
                    joinTable.AddRow(cells);
                }

                //No related rows but right table is OPTIONAL => keep left row, right non-common stay UNBOUND
                if (!foundRelated && rightIsOptional)
                {
                    string[] cells = new string[joinColumnsWidth];
                    for (int i = 0; i < leftColumnsWidth; i++)
                        cells[i] = leftRow[i];
                    joinTable.AddRow(cells);
                }
            }
            return joinTable;
        }

        /// <summary>
        /// Computes the difference between left table and right table (MINUS): keeps each left row that has
        /// no join-compatible right row; when there are no common columns, every left row is kept.
        /// </summary>
        internal static RDFTable DiffJoinTables(RDFTable leftTable, RDFTable rightTable)
        {
            RDFTable diffTable = new RDFTable();
            foreach (RDFTableColumn leftColumn in leftTable.Columns)
                diffTable.AddColumn(leftColumn.Name);

            List<string> commonNames = CommonColumnNames(leftTable, rightTable);
            int width = leftTable.ColumnsCount;

            //No common columns => keep all left rows
            if (commonNames.Count == 0)
            {
                foreach (RDFTableRow leftRow in leftTable.Rows)
                {
                    string[] cells = new string[width];
                    for (int i = 0; i < width; i++)
                        cells[i] = leftRow[i];
                    diffTable.AddRow(cells);
                }
                return diffTable;
            }

            int[] leftCommonOrdinals = commonNames.Select(leftTable.OrdinalOf).ToArray();
            int[] rightCommonOrdinals = commonNames.Select(rightTable.OrdinalOf).ToArray();
            RDFTableRowCollection rightRows = rightTable.Rows;
            foreach (RDFTableRow leftRow in leftTable.Rows)
            {
                bool hasMatch = false;
                for (int ri = 0; ri < rightRows.Count; ri++)
                    if (AreJoinCompatible(leftRow, leftCommonOrdinals, rightRows[ri], rightCommonOrdinals))
                    {
                        hasMatch = true;
                        break;
                    }

                if (!hasMatch)
                {
                    string[] cells = new string[width];
                    for (int i = 0; i < width; i++)
                        cells[i] = leftRow[i];
                    diffTable.AddRow(cells);
                }
            }
            return diffTable;
        }

        /// <summary>
        /// Merges the source table into the target table (UNION, MissingSchemaAction.Add): columns of source
        /// missing in target are added (existing target rows get UNBOUND there), then source rows are appended.
        /// </summary>
        private static void MergeTable(RDFTable targetTable, RDFTable sourceTable)
        {
            foreach (RDFTableColumn sourceColumn in sourceTable.Columns)
                targetTable.AddColumn(sourceColumn.Name);

            int targetWidth = targetTable.ColumnsCount;
            int[] sourceToTarget = new int[sourceTable.ColumnsCount];
            for (int i = 0; i < sourceTable.ColumnsCount; i++)
                sourceToTarget[i] = targetTable.OrdinalOf(sourceTable.Columns[i].Name);

            foreach (RDFTableRow sourceRow in sourceTable.Rows)
            {
                string[] cells = new string[targetWidth];
                for (int i = 0; i < sourceToTarget.Length; i++)
                    cells[sourceToTarget[i]] = sourceRow[i];
                targetTable.AddRow(cells);
            }
        }

        /// <summary>
        /// Combines the given tables, applying dynamically detected Union / Minus / Optional operators
        /// </summary>
        internal static RDFTable CombineTables(List<RDFTable> dataTables)
        {
            switch (dataTables.Count)
            {
                case 0: return new RDFTable();
                case 1: return dataTables[0];
            }

            //Step 1: process Union operators (merge previous into current, then logically delete previous)
            bool hasDoneUnions = false;
            for (int i = 1; i < dataTables.Count; i++)
                if (dataTables[i - 1].JoinAsUnion)
                {
                    MergeTable(dataTables[i], dataTables[i - 1]);
                    dataTables[i - 1] = null;
                    hasDoneUnions = true;
                }
            if (hasDoneUnions)
                dataTables.RemoveAll(dt => dt == null);

            //Step 2: process Minus operators (diff previous against current, preserving current's flags)
            bool hasDoneMinus = false;
            for (int i = 1; i < dataTables.Count; i++)
                if (dataTables[i - 1].JoinAsMinus)
                {
                    RDFTable diffTable = DiffJoinTables(dataTables[i - 1], dataTables[i]);
                    diffTable.IsOptional = dataTables[i].IsOptional;
                    diffTable.JoinAsUnion = dataTables[i].JoinAsUnion;
                    diffTable.JoinAsMinus = dataTables[i].JoinAsMinus;
                    dataTables[i] = diffTable;
                    dataTables[i - 1] = null;
                    hasDoneMinus = true;
                }
            if (hasDoneMinus)
                dataTables.RemoveAll(dt => dt == null);

            //Step 3: compute joins (switch to outer-join on Optional, or always when coming from Union)
            RDFTable finalTable = dataTables[0];
            bool needsOuterJoin = hasDoneUnions;
            for (int i = 1; i < dataTables.Count; i++)
            {
                needsOuterJoin |= dataTables[i].IsOptional;
                finalTable = needsOuterJoin ? OuterJoinTables(finalTable, dataTables[i])
                                            : InnerJoinTables(finalTable, dataTables[i]);
            }
            return finalTable;
        }

        /// <summary>
        /// Compares two cells for sorting: an UNBOUND (null) cell is the smallest value; bound values are
        /// compared by Unicode code point (Ordinal), as agreed for the migration (SPARQL-style ordering).
        /// </summary>
        private static int CompareCells(string leftCell, string rightCell)
        {
            if (leftCell == null)
                return rightCell == null ? 0 : -1;
            if (rightCell == null)
                return 1;
            return string.CompareOrdinal(leftCell, rightCell);
        }

        /// <summary>
        /// Returns a new table with the rows ordered by the given keys (column name + descending flag).
        /// The sort is STABLE (rows equal on all keys keep their original order), UNBOUND sorts first when
        /// ascending and last when descending, and keys whose column is absent are ignored.
        /// </summary>
        internal static RDFTable SortTable(RDFTable table, IList<(string column, bool descending)> sortKeys)
        {
            //Resolve sort-key ordinals once, dropping keys whose column is not in the table
            List<(int ordinal, bool descending)> keys = new List<(int, bool)>();
            foreach ((string column, bool descending) sortKey in sortKeys)
            {
                int ordinal = table.OrdinalOf(sortKey.column);
                if (ordinal >= 0)
                    keys.Add((ordinal, sortKey.descending));
            }

            RDFTable sortedTable = new RDFTable();
            foreach (RDFTableColumn column in table.Columns)
                sortedTable.AddColumn(column.Name);

            //Snapshot rows as cell arrays so they can be reordered
            int width = table.ColumnsCount;
            int rowCount = table.RowsCount;
            RDFTableRowCollection sourceRows = table.Rows;
            string[][] rows = new string[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                string[] cells = new string[width];
                RDFTableRow sourceRow = sourceRows[i];
                for (int c = 0; c < width; c++)
                    cells[c] = sourceRow[c];
                rows[i] = cells;
            }

            if (keys.Count > 0)
            {
                //Sort an index array; the original index is the final tie-break, making the sort stable
                int[] order = Enumerable.Range(0, rowCount).ToArray();
                Array.Sort(order, (x, y) =>
                {
                    foreach ((int ordinal, bool descending) key in keys)
                    {
                        int comparison = CompareCells(rows[x][key.ordinal], rows[y][key.ordinal]);
                        if (comparison != 0)
                            return key.descending ? -comparison : comparison;
                    }
                    return x.CompareTo(y);
                });

                string[][] reordered = new string[rowCount][];
                for (int i = 0; i < rowCount; i++)
                    reordered[i] = rows[order[i]];
                rows = reordered;
            }

            foreach (string[] cells in rows)
                sortedTable.AddRow(cells);
            return sortedTable;
        }

        /// <summary>
        /// Returns a new table with duplicate rows removed (preserving first-occurrence order). Two rows are
        /// equal when every cell matches with Ordinal comparison and UNBOUND equals UNBOUND.
        /// </summary>
        internal static RDFTable DistinctTable(RDFTable table)
        {
            RDFTable distinctTable = new RDFTable();
            foreach (RDFTableColumn column in table.Columns)
                distinctTable.AddColumn(column.Name);

            int width = table.ColumnsCount;
            HashSet<string> seenKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (RDFTableRow row in table.Rows)
            {
                string[] cells = new string[width];
                StringBuilder keyBuilder = new StringBuilder();
                for (int c = 0; c < width; c++)
                {
                    string cell = row[c];
                    cells[c] = cell;
                    //UNBOUND gets a marker distinct from any bound value; bound values are length-prefixed
                    if (cell == null)
                        keyBuilder.Append(" ;");
                    else
                        keyBuilder.Append(cell.Length).Append(':').Append(cell).Append(';');
                }

                if (seenKeys.Add(keyBuilder.ToString()))
                    distinctTable.AddRow(cells);
            }
            return distinctTable;
        }

        /// <summary>
        /// Applies the projection operator on the given table, based on the given query's projection variables
        /// </summary>
        internal static RDFTable ProjectTable(RDFSelectQuery query, RDFTable table)
        {
            //Projection expression variables
            ProjectExpressions(query, table);

            //Execute configured sort modifiers (stable Ordinal sort via RDFTable, UNBOUND sorts smallest)
            RDFOrderByModifier[] orderByModifiers = query.GetModifiers().OfType<RDFOrderByModifier>().ToArray();
            if (orderByModifiers.Length > 0)
            {
                List<(string, bool)> sortKeys = orderByModifiers
                    .Select(m => (m.Variable.ToString(), m.OrderByFlavor == RDFQueryEnums.RDFOrderByFlavors.DESC))
                    .ToList();
                table = SortTable(table, sortKeys);
            }

            //Execute projection algorithm
            if (query.ProjectionVars.Count > 0)
            {
                //Build the projected table with the projection variables, ordered by their target ordinal:
                //values are taken from the matching source column when present, otherwise the column stays UNBOUND
                List<KeyValuePair<RDFVariable, (int, RDFExpression)>> orderedProjections = query.ProjectionVars
                    .OrderBy(pv => pv.Value.Item1)
                    .ToList();

                RDFTable projectedTable = new RDFTable();
                int projCount = orderedProjections.Count;
                int[] sourceOrdinals = new int[projCount];
                for (int i = 0; i < projCount; i++)
                {
                    string projVarString = orderedProjections[i].Key.ToString();
                    projectedTable.AddColumn(projVarString);
                    sourceOrdinals[i] = table.OrdinalOf(projVarString);
                }

                foreach (RDFTableRow sourceRow in table.Rows)
                {
                    string[] cells = new string[projCount];
                    for (int i = 0; i < projCount; i++)
                        cells[i] = sourceOrdinals[i] >= 0 ? sourceRow[sourceOrdinals[i]] : null;
                    projectedTable.AddRow(cells);
                }
                table = projectedTable;
            }
            return table;
        }

        /// <summary>
        /// Fills the given table with new columns from the given query's projection expressions
        /// </summary>
        internal static void ProjectExpressions(RDFSelectQuery query, RDFTable table)
        {
            foreach (KeyValuePair<RDFVariable, (int, RDFExpression)> projectionExpression in query.ProjectionVars.Where(pv => pv.Value.Item2 != null)
                                                                                                                 .OrderBy(pv => pv.Value.Item1))
                EvaluateExpression(projectionExpression.Value.Item2, projectionExpression.Key, table);
        }

        /// <summary>
        /// Fills the given table with new column from the given bind's variable
        /// </summary>
        internal static void ProjectBind(RDFBind bind, RDFTable table)
            => EvaluateExpression(bind.Expression, bind.Variable, table);

        /// <summary>
        /// Evaluates the given expression on the given table and projects the given variable
        /// </summary>
        internal static void EvaluateExpression(RDFExpression expression, RDFVariable variable, RDFTable table)
        {
            string bindVariable = variable.ToString();
            if (!table.HasColumn(bindVariable))
            {
                //Project bind column
                table.AddColumn(bindVariable);
                int bindOrdinal = table.OrdinalOf(bindVariable);

                //Valorize bind column
                if (table.RowsCount == 0)
                {
                    //Ensure to add the row only in case the expression has evaluated without binding errors,
                    //(otherwise in this scenario we would always answer true for ASK queries due to this row)
                    RDFPatternMember bindResult = expression.ApplyExpression(table.NewRow());
                    if (bindResult != null)
                        table.AddRow(new Dictionary<string, string> { { bindVariable, bindResult.ToString() } });
                }
                else
                {
                    int rowCount = table.RowsCount;
                    RDFTableRowCollection rows = table.Rows;
                    for (int i = 0; i < rowCount; i++)
                    {
                        string[] cells = table.GetRowArray(i);
                        cells[bindOrdinal] = expression.ApplyExpression(rows[i])?.ToString();
                    }
                }
            }
        }
    }
}
