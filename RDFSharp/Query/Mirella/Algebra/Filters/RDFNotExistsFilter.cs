/*
   Copyright 2012-2020 Marco De Salvo

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

using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFNotExistsFilter represents a filter for checking absence of given RDF pattern.
    /// </summary>
    public class RDFNotExistsFilter : RDFExistsFilter
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given pattern
        /// </summary>
        public RDFNotExistsFilter(RDFPattern pattern) : base(pattern) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat("FILTER ( NOT EXISTS { ", this.Pattern.ToString(prefixes), " } )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the pattern in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = base.ApplyFilter(row, true);

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion

    }

}