﻿/*
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
    /// RDFBooleanAndFilter represents a filter applying an "AND" on the logics of the given filters.
    /// </summary>
    public class RDFBooleanAndFilter : RDFFilter
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
        /// Default-ctor to build an "AND" filter on the given filters
        /// </summary>
        public RDFBooleanAndFilter(RDFFilter leftFilter, RDFFilter rightFilter)
        {
            if (leftFilter != null)
            {
                if (leftFilter is RDFExistsFilter)
                {
                    throw new RDFQueryException("Cannot create RDFBooleanAndFilter because given \"leftFilter\" parameter is of type RDFExistsFilter: this is not allowed.");
                }
                else
                {
                    if (rightFilter != null)
                    {
                        if (rightFilter is RDFExistsFilter)
                        {
                            throw new RDFQueryException("Cannot create RDFBooleanAndFilter because given \"rightFilter\" parameter is of type RDFExistsFilter: this is not allowed.");
                        }
                        else
                        {
                            this.LeftFilter = leftFilter;
                            this.RightFilter = rightFilter;
                        }
                    }
                    else
                    {
                        throw new RDFQueryException("Cannot create RDFBooleanAndFilter because given \"rightFilter\" parameter is null.");
                    }
                }
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFBooleanAndFilter because given \"leftFilter\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter 
        /// </summary>
        public override String ToString()
        {
            return this.ToString(new List<RDFNamespace>());
        }
        internal override String ToString(List<RDFNamespace> prefixes)
        {
            return "FILTER ( " +
                this.LeftFilter.ToString(prefixes).Replace("FILTER ", String.Empty).Trim() + " && " +
                this.RightFilter.ToString(prefixes).Replace("FILTER ", String.Empty).Trim() + " )";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow
        /// </summary>
        internal override Boolean ApplyFilter(DataRow row, Boolean applyNegation)
        {
            Boolean keepRow = this.LeftFilter.ApplyFilter(row, false) && this.RightFilter.ApplyFilter(row, false);

            //Apply the eventual negation
            if (applyNegation)
            {
                keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion

    }

}