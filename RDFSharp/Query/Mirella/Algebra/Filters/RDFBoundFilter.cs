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
    /// RDFBoundFilter represents a filter checking if a variable is bound to a value.
    /// </summary>
    public class RDFBoundFilter : RDFFilter
    {

        #region Properties
        /// <summary>
        /// Variable to be filtered
        /// </summary>
        public RDFVariable Variable { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable
        /// </summary>
        public RDFBoundFilter(RDFVariable variable)
        {
            if (variable != null)
            {
                this.Variable = variable;
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFBoundFilter because given \"variable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
        {
            return this.ToString(new List<RDFNamespace>());
        }
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            return "FILTER ( BOUND(" + this.Variable + ") )";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(this.Variable.ToString()))
            {

                //Successfull match if the variable is bound to a value
                keepRow = !row.IsNull(this.Variable.ToString());

                //Apply the eventual negation
                if (applyNegation)
                {
                    keepRow = !keepRow;
                }
            }

            return keepRow;
        }
        #endregion

    }

}