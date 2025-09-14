/*
   Copyright 2012-2025 Marco De Salvo

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

using System.Data;
using System.Linq;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFDistinctModifier is a modifier which drops duplicate rows for the level of detail of a SELECT query.
    /// </summary>
    public sealed class RDFDistinctModifier : RDFModifier
    {
        #region Ctors
        /// <summary>
        /// Builds a Distinct modifier on a query
        /// </summary>
        public RDFDistinctModifier() { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier
        /// </summary>
        public override string ToString()
            => "DISTINCT";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the modifier on the given datatable
        /// </summary>
        internal override DataTable ApplyModifier(DataTable table)
            => table.DefaultView.ToTable(true, table.Columns.OfType<DataColumn>()
                                                            .Select(c => c.ColumnName)
                                                            .ToArray());
        #endregion
    }
}