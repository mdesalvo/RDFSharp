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
    /// RDFBooleanNotFilter represents a filter applying a negation on the logics of the given filter.
    /// </summary>
    public sealed class RDFBooleanNotFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Filter to be negated
        /// </summary>
        public RDFFilter Filter { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a negation filter on the given filter
        /// </summary>
        public RDFBooleanNotFilter(RDFFilter filter)
        {
            switch (filter)
            {
                case null:
                    throw new RDFQueryException("Cannot create RDFBooleanNotFilter because given \"filter\" parameter is null.");
                case RDFExistsFilter _:
                    throw new RDFQueryException("Cannot create RDFBooleanNotFilter because given \"filter\" parameter is of type RDFExistsFilter: this is not allowed.");
                default:
                    Filter = filter;
                    break;
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => $"FILTER ( !{Filter.ToString(prefixes).Replace("FILTER ", string.Empty).Trim()} )";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            //Negation logic is applied on the given filter result
            bool keepRow = Filter.ApplyFilter(row, true);

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion
    }
}