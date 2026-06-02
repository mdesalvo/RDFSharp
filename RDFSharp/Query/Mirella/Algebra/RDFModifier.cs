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

using System.Data;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFModifier represents a modifier to be applied on a query results table.
    /// </summary>
    public abstract class RDFModifier : RDFQueryMember
    {
        #region Methods
        /// <summary>
        /// Applies the modifier on the given table
        /// </summary>
        internal abstract RDFTable ApplyModifier(RDFTable tableToFilter);

        /// <summary>
        /// Applies the modifier on the given datatable (thin DataTable-compatibility wrapper kept for the test
        /// suite: converts the table to an RDFTable and delegates to the real implementation). Modifiers whose
        /// legacy DataTable behavior carried DataView.Sort semantics (OrderBy/Limit/Offset) override this.
        /// </summary>
        internal virtual DataTable ApplyModifier(DataTable tableToFilter)
            => ApplyModifier(RDFTable.FromDataTable(tableToFilter)).ToDataTable();
        #endregion
    }
}