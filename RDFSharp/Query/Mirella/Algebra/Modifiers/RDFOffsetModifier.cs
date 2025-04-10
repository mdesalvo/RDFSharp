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
    /// RDFOffsetModifier is a modifier which makes the first N query results to be not considered.
    /// </summary>
    public sealed class RDFOffsetModifier : RDFModifier
    {
        #region Properties
        /// <summary>
        /// Number of results not considered from the query
        /// </summary>
        public int Offset { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an Offset modifier on a query
        /// </summary>
        public RDFOffsetModifier(int offset)
        {
            #region Guards
            if (offset < 0)
                throw new RDFQueryException("Cannot create RDFOffsetModifier because given \"offset\" parameter (" + offset + ") is negative.");
            #endregion

            Offset = offset;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier
        /// </summary>
        public override string ToString()
            => string.Concat("OFFSET ", Offset.ToString());
        #endregion

        #region Methods
        /// <summary>
        /// Applies the modifier on the given datatable
        /// </summary>
        internal override DataTable ApplyModifier(DataTable table)
        {
            string tableSort = table.DefaultView.Sort;
            if (table.Rows.Count == 0 || Offset >= table.Rows.Count)
                table = table.Clone();
            else
                table = table.AsEnumerable().Skip(Offset).CopyToDataTable();
            table.DefaultView.Sort = tableSort;
            return table;
        }
        #endregion
    }
}