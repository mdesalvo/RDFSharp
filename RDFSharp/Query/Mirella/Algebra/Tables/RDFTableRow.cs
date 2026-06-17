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
    /// It carries no change-tracking, no versioning and no per-cell boxing: a cell is
    /// either a bound string or null (UNBOUND).
    /// The struct only references the backing storage of its owning table, so enumerating
    /// rows does not allocate one object per physical row.
    /// </summary>
    internal readonly struct RDFTableRow
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
        internal string this[string column]
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
        internal string this[int ordinal]
            => _cells[ordinal];
        #endregion

        #region Methods
        /// <summary>
        /// A stable signature of the whole row (all cells joined with a control separator), used to tell distinct
        /// solutions apart (e.g. for COUNT(DISTINCT *)). UNBOUND cells contribute an empty chunk.
        /// </summary>
        internal string Signature
            => string.Join("§", _cells);

        /// <summary>
        /// Tells whether the row's table owns a column with the given (possibly non-normalized) name.
        /// </summary>
        internal bool HasColumn(string column)
            => column != null && _ordinals.ContainsKey(RDFTable.NormalizeColumnName(column));

        /// <summary>
        /// Tells whether the given column is UNBOUND in this row
        /// </summary>
        /// <exception cref="RDFQueryException">the column is null or does not belong to the table</exception>
        internal bool IsUnbound(string column)
            => _cells[ResolveOrdinal(column)] == null;

        /// <summary>
        /// Tells whether the given column is bound to a value in this row
        /// </summary>
        /// <exception cref="RDFQueryException">the column is null or does not belong to the table</exception>
        internal bool IsBound(string column)
            => _cells[ResolveOrdinal(column)] != null;

        /// <summary>
        /// Resolves the ordinal of the given (possibly non-normalized) column name,
        /// throwing an RDFQueryException when the column is unknown
        /// </summary>
        private int ResolveOrdinal(string column)
        {
            //Every misuse surfaces as RDFQueryException, distinguishing a null name from a name
            //not belonging to the table. The null guard also avoids feeding a null key to the dictionary.
            if (column == null)
                throw new RDFQueryException("Cannot access table cell because given \"column\" parameter is null.");

            return _ordinals.TryGetValue(RDFTable.NormalizeColumnName(column), out int ordinal)
                ? ordinal
                : throw new RDFQueryException($"Cannot access table cell because column '{column}' does not belong to the table.");
        }
        #endregion
    }
}