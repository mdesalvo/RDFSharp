﻿/*
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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOrderByModifier is a modifier which applies a sort on the results of a SELECT query for the given variable.
    /// </summary>
    public class RDFOrderByModifier : RDFModifier
    {
        #region Properties
        /// <summary>
        /// Variable to be ordered
        /// </summary>
        public RDFVariable Variable { get; internal set; }

        /// <summary>
        /// Flavor of variable ordering (ASC/DESC)
        /// </summary>
        public RDFQueryEnums.RDFOrderByFlavors OrderByFlavor { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an OrderBy modifier of the given flavor on the given variable
        /// </summary>
        public RDFOrderByModifier(RDFVariable variable, RDFQueryEnums.RDFOrderByFlavors orderbyFlavor)
        {
            OrderByFlavor = orderbyFlavor;
            Variable = variable ?? throw new RDFQueryException("Cannot create RDFOrderByModifier because given \"variable\" parameter is null.");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier
        /// </summary>
        public override string ToString()
            => string.Concat(OrderByFlavor, "(", Variable, ")");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the modifier on the column corresponding to the variable in the given datatable
        /// </summary>
        internal override DataTable ApplyModifier(DataTable table)
        {
            if (table.Columns.Contains(Variable.ToString()))
                table.DefaultView.Sort = !string.IsNullOrEmpty(table.DefaultView.Sort)
                                            ? string.Concat(table.DefaultView.Sort, ", ", Variable, " ", OrderByFlavor)
                                            : string.Concat(Variable, " ", OrderByFlavor);
            return table;
        }
        #endregion
    }
}