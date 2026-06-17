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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFTableColumn represents a column of an RDFTable: since Mirella treats every binding
    /// as a string, a column is fully described by its (already normalized) name and its position,
    /// without typing or constraints.
    /// </summary>
    internal sealed class RDFTableColumn
    {
        #region Properties
        /// <summary>
        /// Name of the column (normalized as Trim().ToUpperInvariant(), e.g. "?SUBJECT")
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Zero-based position of the column within the owning table
        /// </summary>
        internal int Ordinal { get; set; }

        /// <summary>
        /// Whether the column was injected by the engine (e.g. a desugared/materialized expression used for grouping
        /// or aggregation) rather than coming from the query's solution bindings. Such columns are an internal scratch
        /// and must not surface in the query results.
        /// </summary>
        internal bool IsSynthetic { get; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a column with the given (already normalized) name and ordinal
        /// </summary>
        internal RDFTableColumn(string name, int ordinal, bool isSynthetic=false)
        {
            Name = name;
            Ordinal = ordinal;
            IsSynthetic = isSynthetic;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the column (its name), so that callers iterating
        /// the columns can obtain the variable name via ToString()
        /// </summary>
        public override string ToString()
            => Name;
        #endregion
    }
}