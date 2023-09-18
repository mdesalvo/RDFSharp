﻿/*
   Copyright 2012-2023 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFBooleanOrFilter represents a filter applying an "OR" on the logics of the given filters.
    /// </summary>
    public class RDFBooleanOrFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Left Filter
        /// </summary>
        public RDFFilter LeftFilter { get; internal set; }

        /// <summary>
        /// Right Filter
        /// </summary>
        public RDFFilter RightFilter { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an "OR" filter on the given filters
        /// </summary>
        public RDFBooleanOrFilter(RDFFilter leftFilter, RDFFilter rightFilter)
        {
            #region Guards
            if (leftFilter == null)
                throw new RDFQueryException("Cannot create RDFBooleanOrFilter because given \"leftFilter\" parameter is null.");
            if (leftFilter is RDFExistsFilter)
                throw new RDFQueryException("Cannot create RDFBooleanOrFilter because given \"leftFilter\" parameter is of type RDFExistsFilter: this is not allowed.");
            if (rightFilter == null)
                throw new RDFQueryException("Cannot create RDFBooleanOrFilter because given \"rightFilter\" parameter is null.");
            if (rightFilter is RDFExistsFilter)
                throw new RDFQueryException("Cannot create RDFBooleanOrFilter because given \"rightFilter\" parameter is of type RDFExistsFilter: this is not allowed.");
            #endregion

            LeftFilter = leftFilter;
            RightFilter = rightFilter;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat(
                "FILTER ( ",
                LeftFilter.ToString(prefixes).Replace("FILTER ", string.Empty).Trim(), " || ",
                RightFilter.ToString(prefixes).Replace("FILTER ", string.Empty).Trim(), " )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = LeftFilter.ApplyFilter(row, false) || RightFilter.ApplyFilter(row, false);

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion
    }
}