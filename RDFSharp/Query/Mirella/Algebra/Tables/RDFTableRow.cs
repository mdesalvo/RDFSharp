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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFTableRow is a lightweight, read-only view over a single row of an RDFTable.
    /// It is the counterpart of System.Data.DataRow but carries no change-tracking, no
    /// versioning and no per-cell boxing: a cell is either a bound string or null (UNBOUND).
    /// The struct only references the backing storage of its owning table, so enumerating
    /// rows does not allocate one object per physical row.
    /// </summary>
    public readonly struct RDFTableRow
    {
        #region Fields
        /// <summary>
        /// Backing cells of the row (null entries represent UNBOUND variables)
        /// </summary>
        private readonly string[] _cells;

        /// <summary>
        /// Shared map (column name => ordinal) of the owning table
        /// </summary>
        private readonly Dictionary<string, int> _ordinals;
        #endregion

        #region Ctor
        /// <summary>
        /// Builds a view over the given backing cells, using the owning table's ordinal map
        /// </summary>
        internal RDFTableRow(string[] cells, Dictionary<string, int> ordinals)
        {
            _cells = cells;
            _ordinals = ordinals;
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets the value bound to the given column, or null if the variable is UNBOUND
        /// </summary>
        /// <exception cref="RDFQueryException">the column is null or does not belong to the table</exception>
        public string this[string column]
        {
            get
            {
                int ordinal = ResolveOrdinal(column);
                return _cells[ordinal];
            }
        }

        /// <summary>
        /// Gets the value at the given ordinal, or null if the variable is UNBOUND
        /// </summary>
        public string this[int ordinal]
            => _cells[ordinal];
        #endregion

        #region Methods
        /// <summary>
        /// Tells whether the row's table owns a column with the given (possibly non-normalized) name.
        /// Replaces the old "row.Table.Columns.Contains(...)" check used throughout the algebra.
        /// </summary>
        public bool HasColumn(string column)
            => column != null && _ordinals.ContainsKey(RDFTable.NormalizeColumnName(column));

        /// <summary>
        /// Tells whether the given column is UNBOUND in this row (counterpart of DataRow.IsNull)
        /// </summary>
        /// <exception cref="RDFQueryException">the column is null or does not belong to the table</exception>
        public bool IsUnbound(string column)
            => _cells[ResolveOrdinal(column)] == null;

        /// <summary>
        /// Tells whether the given column is bound to a value in this row
        /// </summary>
        /// <exception cref="RDFQueryException">the column is null or does not belong to the table</exception>
        public bool IsBound(string column)
            => _cells[ResolveOrdinal(column)] != null;

        /// <summary>
        /// Resolves the ordinal of the given (possibly non-normalized) column name,
        /// throwing the same way DataRow does when the column is unknown
        /// </summary>
        private int ResolveOrdinal(string column)
        {
            //Every misuse surfaces as RDFQueryException, keeping the same behavioral distinction
            //the old DataRow drew between a null name and a name not belonging to the table.
            //The null guard also avoids feeding a null key to the dictionary.
            if (column == null)
                throw new RDFQueryException("Cannot access table cell because given \"column\" parameter is null.");

            return _ordinals.TryGetValue(RDFTable.NormalizeColumnName(column), out int ordinal)
                ? ordinal
                : throw new RDFQueryException($"Cannot access table cell because column '{column}' does not belong to the table.");
        }
        #endregion
    }
}