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
    /// as a string, a column is fully described by its (already normalized) name and its position.
    /// It is the lightweight counterpart of System.Data.DataColumn, without typing or constraints.
    /// </summary>
    public sealed class RDFTableColumn
    {
        #region Properties
        /// <summary>
        /// Name of the column (normalized as Trim().ToUpperInvariant(), e.g. "?SUBJECT")
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Zero-based position of the column within the owning table
        /// </summary>
        public int Ordinal { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a column with the given (already normalized) name and ordinal
        /// </summary>
        internal RDFTableColumn(string name, int ordinal)
        {
            Name = name;
            Ordinal = ordinal;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the column (its name), so that callers iterating
        /// the columns can keep using ToString() exactly as they did with System.Data.DataColumn
        /// </summary>
        public override string ToString()
            => Name;
        #endregion
    }
}