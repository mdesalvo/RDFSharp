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
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFTable is the lightweight, string-only tabular structure backing Mirella's query
    /// evaluation. It stores bindings as plain strings (a null cell represents an UNBOUND
    /// variable), keeps no typing, no constraints, no relations and no change-tracking, and
    /// can be exported to / imported from a System.Data.DataTable at the public boundary,
    /// since for Mirella every value is a string anyway.
    /// </summary>
    internal sealed class RDFTable
    {
        #region Fields
        /// <summary>
        /// Columns of the table, in ordinal order
        /// </summary>
        private readonly List<RDFTableColumn> _columns = new List<RDFTableColumn>();

        /// <summary>
        /// Map (normalized column name => ordinal), using Ordinal comparison over already
        /// normalized (Trim + UpperInvariant) names
        /// </summary>
        private readonly Dictionary<string, int> _ordinals = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>
        /// Rows of the table: each row is an array sized to the columns count, where a null
        /// entry represents an UNBOUND variable
        /// </summary>
        private readonly List<string[]> _rows = new List<string[]>();
        #endregion

        #region Properties
        /// <summary>
        /// Columns of the table, in ordinal order
        /// </summary>
        internal IReadOnlyList<RDFTableColumn> Columns
            => _columns;

        /// <summary>
        /// Number of columns of the table
        /// </summary>
        internal int ColumnsCount
            => _columns.Count;

        /// <summary>
        /// Rows of the table (zero-allocation enumeration via struct enumerator)
        /// </summary>
        internal RDFTableRowCollection Rows
            => new RDFTableRowCollection(_rows, _ordinals);

        /// <summary>
        /// Number of rows of the table
        /// </summary>
        internal int RowsCount
            => _rows.Count;
        #endregion

        #region Metadata
        /// <summary>
        /// Marks this table as the result of an OPTIONAL pattern
        /// </summary>
        internal bool IsOptional;
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an empty table
        /// </summary>
        internal RDFTable() { }
        #endregion

        #region Methods

        #region Schema
        /// <summary>
        /// Normalizes a column name (Trim + UpperInvariant), so that lookups and serialized
        /// variable names are canonical (e.g. "?SUBJECT")
        /// </summary>
        internal static string NormalizeColumnName(string columnName)
            => columnName?.Trim().ToUpperInvariant();

        /// <summary>
        /// Adds a new column to the table (avoiding duplicates) and returns it. If rows already
        /// exist, they are widened with an UNBOUND cell for the new column.
        /// </summary>
        internal RDFTableColumn AddColumn(string columnName)
        {
            string normalizedName = NormalizeColumnName(columnName);
            if (_ordinals.TryGetValue(normalizedName, out int existingOrdinal))
                return _columns[existingOrdinal];

            RDFTableColumn column = new RDFTableColumn(normalizedName, _columns.Count);
            _ordinals.Add(normalizedName, column.Ordinal);
            _columns.Add(column);

            //Widen already existing rows so that every row stays sized to the columns count,
            //leaving the newly added column UNBOUND on them
            if (_rows.Count > 0)
            {
                int newWidth = _columns.Count;
                for (int i = 0; i < _rows.Count; i++)
                {
                    string[] widened = new string[newWidth];
                    Array.Copy(_rows[i], widened, _rows[i].Length);
                    _rows[i] = widened;
                }
            }

            return column;
        }

        /// <summary>
        /// Tells whether the table owns a column with the given (possibly non-normalized) name
        /// </summary>
        internal bool HasColumn(string columnName)
            => columnName != null && _ordinals.ContainsKey(NormalizeColumnName(columnName));

        /// <summary>
        /// Gets the ordinal of the column with the given (possibly non-normalized) name, or -1 if absent
        /// </summary>
        internal int OrdinalOf(string columnName)
            => columnName != null && _ordinals.TryGetValue(NormalizeColumnName(columnName), out int ordinal) ? ordinal : -1;

        /// <summary>
        /// Removes the column with the given (possibly non-normalized) name from the table, re-indexing
        /// the remaining columns and shrinking every existing row accordingly. No-op if the column is absent.
        /// Used by the engine to drop property-path / non-projection columns.
        /// </summary>
        internal void RemoveColumn(string columnName)
        {
            if (columnName == null)
                return;
            if (!_ordinals.TryGetValue(NormalizeColumnName(columnName), out int removedOrdinal))
                return;

            //Rebuild the columns list and the ordinal map without the removed column
            _columns.RemoveAt(removedOrdinal);
            _ordinals.Clear();
            for (int i = 0; i < _columns.Count; i++)
            {
                _columns[i].Ordinal = i;
                _ordinals.Add(_columns[i].Name, i);
            }

            //Shrink every existing row by dropping the removed ordinal
            int newWidth = _columns.Count;
            for (int r = 0; r < _rows.Count; r++)
            {
                string[] oldCells = _rows[r];
                string[] newCells = new string[newWidth];
                if (removedOrdinal > 0)
                    Array.Copy(oldCells, 0, newCells, 0, removedOrdinal);
                if (removedOrdinal < newWidth)
                    Array.Copy(oldCells, removedOrdinal + 1, newCells, removedOrdinal, newWidth - removedOrdinal);
                _rows[r] = newCells;
            }
        }
        #endregion

        #region Data
        /// <summary>
        /// Adds a row from the given variable=>value bindings: only bindings matching an existing
        /// column are written, and the row is added only if at least one binding matched a column.
        /// Missing columns stay UNBOUND (null).
        /// </summary>
        internal void AddRow(IDictionary<string, string> bindings)
        {
            string[] cells = new string[_columns.Count];
            bool rowAdded = false;
            foreach (KeyValuePair<string, string> binding in bindings)
            {
                if (_ordinals.TryGetValue(NormalizeColumnName(binding.Key), out int ordinal))
                {
                    cells[ordinal] = binding.Value;
                    rowAdded = true;
                }
            }

            if (rowAdded)
                _rows.Add(cells);
        }

        /// <summary>
        /// Adds a row from a pre-built cells array (used by the evaluation engine when building
        /// join/product results). The array length must equal the columns count; null entries
        /// represent UNBOUND variables. The array is taken by reference (no defensive copy).
        /// </summary>
        internal void AddRow(string[] cells)
        {
            if (cells == null)
                throw new RDFQueryException("Cannot add row to table because given \"cells\" parameter is null.");
            if (cells.Length != _columns.Count)
                throw new RDFQueryException($"Cannot add row to table because given \"cells\" parameter has length {cells.Length} but the table has {_columns.Count} columns.");

            _rows.Add(cells);
        }

        /// <summary>
        /// Builds a transient all-UNBOUND row view sized to the table's current columns, sharing the table's
        /// ordinal map but NOT added to the table. Used by the engine to evaluate an expression on an empty
        /// table (e.g. BIND on a zero-row table).
        /// </summary>
        internal RDFTableRow NewRow()
            => new RDFTableRow(new string[_columns.Count], _ordinals);

        /// <summary>
        /// Gets the mutable backing cells array of the row at the given index (by reference, no copy), so the
        /// engine can valorize a freshly projected column in place. The array length equals the columns count.
        /// </summary>
        internal string[] GetRowArray(int index)
            => _rows[index];

        /// <summary>
        /// Creates an empty table with the same columns (and join flags) as this one, but no rows
        /// (schema-only copy).
        /// </summary>
        internal RDFTable Clone()
        {
            RDFTable clone = new RDFTable
            {
                IsOptional = IsOptional
            };
            foreach (RDFTableColumn column in _columns)
                clone.AddColumn(column.Name);
            return clone;
        }
        #endregion

        #region Interop
        /// <summary>
        /// Exports the table to an equivalent System.Data.DataTable made of string columns,
        /// mapping UNBOUND cells to DBNull.Value. Column names and order are preserved.
        /// </summary>
        internal DataTable ToDataTable()
        {
            DataTable dataTable = new DataTable();
            foreach (RDFTableColumn column in _columns)
                dataTable.Columns.Add(column.Name, typeof(string));

            dataTable.BeginLoadData();
            int width = _columns.Count;
            foreach (string[] cells in _rows)
            {
                object[] items = new object[width];
                for (int i = 0; i < width; i++)
                    items[i] = cells[i] ?? (object)DBNull.Value;
                dataTable.LoadDataRow(items, true);
            }
            dataTable.EndLoadData();

            return dataTable;
        }

        /// <summary>
        /// Imports an equivalent RDFTable from the given System.Data.DataTable: every cell is
        /// projected to its string representation (since, for Mirella, everything is a string),
        /// mapping null / DBNull to UNBOUND. Column names are normalized (Trim + UpperInvariant).
        /// </summary>
        internal static RDFTable FromDataTable(DataTable dataTable)
        {
            if (dataTable == null)
                throw new RDFQueryException("Cannot import RDFTable because given \"dataTable\" parameter is null.");

            RDFTable table = new RDFTable();
            foreach (DataColumn column in dataTable.Columns)
                table.AddColumn(column.ColumnName);

            int width = table._columns.Count;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                string[] cells = new string[width];
                for (int i = 0; i < width; i++)
                {
                    object value = dataRow[i];
                    cells[i] = value == null || value is DBNull ? null : value.ToString();
                }
                table._rows.Add(cells);
            }

            return table;
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// RDFTableRowCollection is a lightweight, enumerable view over the rows of an RDFTable.
    /// It exposes a struct enumerator so that a foreach over the rows allocates nothing, while
    /// still implementing IEnumerable&lt;RDFTableRow&gt; for LINQ-style consumers.
    /// </summary>
    internal readonly struct RDFTableRowCollection : IEnumerable<RDFTableRow>
    {
        #region Fields
        private readonly List<string[]> _rows;
        private readonly Dictionary<string, int> _ordinals;
        #endregion

        #region Ctor
        internal RDFTableRowCollection(List<string[]> rows, Dictionary<string, int> ordinals)
        {
            _rows = rows;
            _ordinals = ordinals;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Number of rows in the collection
        /// </summary>
        internal int Count
            => _rows.Count;

        /// <summary>
        /// Gets a view over the row at the given index
        /// </summary>
        internal RDFTableRow this[int index]
            => new RDFTableRow(_rows[index], _ordinals);
        #endregion

        #region Enumeration
        /// <summary>
        /// Returns a zero-allocation struct enumerator over the rows
        /// </summary>
        public Enumerator GetEnumerator()
            => new Enumerator(_rows, _ordinals);

        IEnumerator<RDFTableRow> IEnumerable<RDFTableRow>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Struct enumerator over the rows of an RDFTable
        /// </summary>
        public struct Enumerator : IEnumerator<RDFTableRow>
        {
            private readonly List<string[]> _rows;
            private readonly Dictionary<string, int> _ordinals;
            private int _index;

            internal Enumerator(List<string[]> rows, Dictionary<string, int> ordinals)
            {
                _rows = rows;
                _ordinals = ordinals;
                _index = -1;
            }

            /// <summary>
            /// Gets a view over the row at the current position of the enumerator
            /// </summary>
            public RDFTableRow Current
                => new RDFTableRow(_rows[_index], _ordinals);

            object IEnumerator.Current
                => Current;

            /// <summary>
            /// Advances the enumerator to the next row, returning false once past the last one
            /// </summary>
            public bool MoveNext()
                => ++_index < _rows.Count;

            /// <summary>
            /// Resets the enumerator to its initial position, before the first row
            /// </summary>
            public void Reset()
                => _index = -1;

            /// <summary>
            /// Releases the resources held by the enumerator (none: provided for IEnumerator compliance)
            /// </summary>
            public void Dispose() { }
        }
        #endregion
    }
}