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
    /// RDFIsBlankFilter represents a filter for Blank Uri values of a variable.
    /// </summary>
    public class RDFIsBlankFilter : RDFFilter
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
        public RDFIsBlankFilter(RDFVariable variable)
        {
            if (variable != null)
            {
                this.Variable = variable;
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFIsBlankFilter because given \"variable\" parameter is null.");
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
            return "FILTER ( ISBLANK(" + this.Variable + ") )";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override Boolean ApplyFilter(DataRow row, Boolean applyNegation)
        {
            Boolean keepRow = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(this.Variable.ToString()))
            {
                String variableValue = row[this.Variable.ToString()].ToString();

                //Apply a blank-checking logic on result of an "IsUri" filter
                RDFIsUriFilter isUriFilter = new RDFIsUriFilter(this.Variable);
                keepRow = (isUriFilter.ApplyFilter(row, false) && variableValue.StartsWith("bnode:"));

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