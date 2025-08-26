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

using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFFilter represents a filter to be applied on a query results table.
    /// </summary>
    public abstract class RDFFilter : RDFPatternGroupMember
    {
        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>(0));
        internal abstract string ToString(List<RDFNamespace> prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow
        /// </summary>
        internal abstract bool ApplyFilter(DataRow row, bool applyNegation);
        #endregion
    }
}